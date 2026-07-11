using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler;

public class Parser : IDisposable
{
	private struct WithItem
	{
		public readonly int Start;

		public readonly Expression ContextManager;

		public readonly Expression Variable;

		public WithItem(int start, Expression contextManager, Expression variable)
		{
			Start = start;
			ContextManager = contextManager;
			Variable = variable;
		}
	}

	private class TokenizerErrorSink : ErrorSink
	{
		private readonly Parser _parser;

		public TokenizerErrorSink(Parser parser)
		{
			_parser = parser;
		}

		public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity)
		{
			if (_parser._errorCode == 0 && (severity == Severity.Error || severity == Severity.FatalError))
			{
				_parser._errorCode = errorCode;
			}
			_parser.ErrorSink.Add(sourceUnit, message, span, errorCode, severity);
		}
	}

	private readonly Tokenizer _tokenizer;

	private ErrorSink _errors;

	private ParserSink _sink;

	private SourceUnit _sourceUnit;

	private ModuleOptions _languageFeatures;

	private TokenWithSpan _token;

	private TokenWithSpan _lookahead;

	private Stack<FunctionDefinition> _functions;

	private bool _fromFutureAllowed;

	private string _privatePrefix;

	private bool _parsingStarted;

	private bool _allowIncomplete;

	private bool _inLoop;

	private bool _inFinally;

	private bool _isGenerator;

	private bool _returnWithValue;

	private SourceCodeReader _sourceReader;

	private int _errorCode;

	private readonly CompilerContext _context;

	private PythonAst _globalParent;

	private static readonly char[] newLineChar = new char[1] { '\n' };

	private static readonly char[] whiteSpace = new char[2] { ' ', '\t' };

	public ErrorSink ErrorSink
	{
		get
		{
			return _errors;
		}
		set
		{
			ContractUtils.RequiresNotNull(value, "value");
			_errors = value;
		}
	}

	public ParserSink ParserSink
	{
		get
		{
			return _sink;
		}
		set
		{
			if (_sink == ParserSink.Null)
			{
				_sink = null;
			}
			else
			{
				_sink = value;
			}
		}
	}

	public int ErrorCode => _errorCode;

	private FunctionDefinition CurrentFunction
	{
		get
		{
			if (_functions != null && _functions.Count > 0)
			{
				return _functions.Peek();
			}
			return null;
		}
	}

	private bool TrueDivision => (_languageFeatures & ModuleOptions.TrueDivision) == ModuleOptions.TrueDivision;

	private bool AbsoluteImports => (_languageFeatures & ModuleOptions.AbsoluteImports) == ModuleOptions.AbsoluteImports;

	private Parser(CompilerContext context, Tokenizer tokenizer, ErrorSink errorSink, ParserSink parserSink, ModuleOptions languageFeatures)
	{
		ContractUtils.RequiresNotNull(tokenizer, "tokenizer");
		ContractUtils.RequiresNotNull(errorSink, "errorSink");
		ContractUtils.RequiresNotNull(parserSink, "parserSink");
		tokenizer.ErrorSink = new TokenizerErrorSink(this);
		_tokenizer = tokenizer;
		_errors = errorSink;
		if (parserSink != ParserSink.Null)
		{
			_sink = parserSink;
		}
		_context = context;
		Reset(tokenizer.SourceUnit, languageFeatures);
	}

	public static Parser CreateParser(CompilerContext context, PythonOptions options)
	{
		return CreateParserWorker(context, options, verbatim: false);
	}

	[Obsolete("pass verbatim via PythonCompilerOptions in PythonOptions")]
	public static Parser CreateParser(CompilerContext context, PythonOptions options, bool verbatim)
	{
		return CreateParserWorker(context, options, verbatim);
	}

	private static Parser CreateParserWorker(CompilerContext context, PythonOptions options, bool verbatim)
	{
		ContractUtils.RequiresNotNull(context, "context");
		ContractUtils.RequiresNotNull(options, "options");
		PythonCompilerOptions pythonCompilerOptions = context.Options as PythonCompilerOptions;
		if (options == null)
		{
			throw new ValueErrorException(Resources.PythonContextRequired);
		}
		SourceCodeReader reader;
		try
		{
			reader = context.SourceUnit.GetReader();
			if (pythonCompilerOptions.SkipFirstLine)
			{
				reader.ReadLine();
			}
		}
		catch (IOException ex)
		{
			context.Errors.Add(context.SourceUnit, ex.Message, SourceSpan.Invalid, 0, Severity.Error);
			throw;
		}
		Tokenizer tokenizer = new Tokenizer(context.Errors, pythonCompilerOptions, verbatim);
		tokenizer.Initialize(null, reader, context.SourceUnit, SourceLocation.MinValue);
		tokenizer.IndentationInconsistencySeverity = options.IndentationInconsistencySeverity;
		Parser parser = new Parser(context, tokenizer, context.Errors, context.ParserSink, pythonCompilerOptions.Module);
		parser._sourceReader = reader;
		return parser;
	}

	public PythonAst ParseFile(bool makeModule)
	{
		return ParseFile(makeModule, returnValue: false);
	}

	public PythonAst ParseFile(bool makeModule, bool returnValue)
	{
		try
		{
			return ParseFileWorker(makeModule, returnValue);
		}
		catch (BadSourceException bse)
		{
			throw BadSourceError(bse);
		}
	}

	public PythonAst ParseInteractiveCode(out ScriptCodeParseResult properties)
	{
		bool isEmptyStmt = false;
		properties = ScriptCodeParseResult.Complete;
		_globalParent = new PythonAst(isModule: false, _languageFeatures, printExpressions: true, _context);
		StartParsing();
		bool parsingMultiLineCmpdStmt;
		Statement ret = InternalParseInteractiveInput(out parsingMultiLineCmpdStmt, out isEmptyStmt);
		if (_errorCode == 0)
		{
			if (isEmptyStmt)
			{
				properties = ScriptCodeParseResult.Empty;
			}
			else if (parsingMultiLineCmpdStmt)
			{
				properties = ScriptCodeParseResult.IncompleteStatement;
			}
			if (isEmptyStmt)
			{
				return null;
			}
			return FinishParsing(ret);
		}
		if ((_errorCode & 0xF) != 0)
		{
			if ((_errorCode & 2) != 0)
			{
				properties = ScriptCodeParseResult.IncompleteToken;
				return null;
			}
			if ((_errorCode & 1) != 0)
			{
				if (parsingMultiLineCmpdStmt)
				{
					properties = ScriptCodeParseResult.IncompleteStatement;
				}
				else
				{
					properties = ScriptCodeParseResult.IncompleteToken;
				}
				return null;
			}
		}
		properties = ScriptCodeParseResult.Invalid;
		return null;
	}

	private PythonAst FinishParsing(Statement ret)
	{
		PythonAst globalParent = _globalParent;
		_globalParent = null;
		int[] array = _tokenizer.GetLineLocations();
		if (_sourceUnit.HasLineMapping)
		{
			List<int> list = new List<int>();
			int item = 0;
			for (int i = 0; i < array.Length; i++)
			{
				while (list.Count < i)
				{
					list.Add(item);
				}
				item = array[i] + 1;
				list.Add(array[i]);
			}
			array = list.ToArray();
		}
		globalParent.ParsingFinished(array, ret, _languageFeatures);
		return globalParent;
	}

	public PythonAst ParseSingleStatement()
	{
		try
		{
			_globalParent = new PythonAst(isModule: false, _languageFeatures, printExpressions: true, _context);
			StartParsing();
			MaybeEatNewLine();
			Statement ret = ParseStmt();
			EatEndOfInput();
			return FinishParsing(ret);
		}
		catch (BadSourceException bse)
		{
			throw BadSourceError(bse);
		}
	}

	public PythonAst ParseTopExpression()
	{
		try
		{
			_globalParent = new PythonAst(isModule: false, _languageFeatures, printExpressions: false, _context);
			ReturnStatement returnStatement = new ReturnStatement(ParseTestListAsExpression());
			returnStatement.SetLoc(_globalParent, 0, 0);
			return FinishParsing(returnStatement);
		}
		catch (BadSourceException bse)
		{
			throw BadSourceError(bse);
		}
	}

	public static int GetNextAutoIndentSize(string text, int autoIndentTabWidth)
	{
		ContractUtils.RequiresNotNull(text, "text");
		string[] array = text.Split(newLineChar);
		if (array.Length <= 1)
		{
			return 0;
		}
		string text2 = array[array.Length - 2];
		int i;
		for (i = 0; i < text2.Length && text2[i] == ' '; i++)
		{
		}
		int num = i;
		if (text2.TrimEnd(whiteSpace).EndsWith(":"))
		{
			num += autoIndentTabWidth;
		}
		return num;
	}

	public void Reset(SourceUnit sourceUnit, ModuleOptions languageFeatures)
	{
		ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");
		_sourceUnit = sourceUnit;
		_languageFeatures = languageFeatures;
		_token = default(TokenWithSpan);
		_lookahead = default(TokenWithSpan);
		_fromFutureAllowed = true;
		_functions = null;
		_privatePrefix = null;
		_parsingStarted = false;
		_errorCode = 0;
	}

	public void Reset()
	{
		Reset(_sourceUnit, _languageFeatures);
	}

	private void ReportSyntaxError(TokenWithSpan t)
	{
		ReportSyntaxError(t, 16);
	}

	private void ReportSyntaxError(TokenWithSpan t, int errorCode)
	{
		ReportSyntaxError(t.Token, t.Span, errorCode, allowIncomplete: true);
	}

	private void ReportSyntaxError(Token t, IndexSpan span, int errorCode, bool allowIncomplete)
	{
		int start = span.Start;
		int end = span.End;
		if (allowIncomplete && (t.Kind == TokenKind.EndOfFile || (_tokenizer.IsEndOfFile && (t.Kind == TokenKind.Dedent || t.Kind == TokenKind.NLToken))))
		{
			errorCode |= 1;
		}
		string message = string.Format(CultureInfo.InvariantCulture, GetErrorMessage(t, errorCode), new object[1] { t.Image });
		ReportSyntaxError(start, end, message, errorCode);
	}

	private static string GetErrorMessage(Token t, int errorCode)
	{
		if ((errorCode & -16) == 32)
		{
			return Resources.ExpectedIndentation;
		}
		if (t.Kind != TokenKind.EndOfFile)
		{
			return Resources.UnexpectedToken;
		}
		return "unexpected EOF while parsing";
	}

	private void ReportSyntaxError(string message)
	{
		ReportSyntaxError(_lookahead.Span.Start, _lookahead.Span.End, message);
	}

	internal void ReportSyntaxError(int start, int end, string message)
	{
		ReportSyntaxError(start, end, message, 16);
	}

	internal void ReportSyntaxError(int start, int end, string message, int errorCode)
	{
		if (_errorCode == 0)
		{
			_errorCode = errorCode;
		}
		_errors.Add(_sourceUnit, message, new SourceSpan(_tokenizer.IndexToLocation(start), _tokenizer.IndexToLocation(end)), errorCode, Severity.FatalError);
	}

	private static bool IsPrivateName(string name)
	{
		if (name.StartsWith("__"))
		{
			return !name.EndsWith("__");
		}
		return false;
	}

	private string FixName(string name)
	{
		if (_privatePrefix != null && IsPrivateName(name))
		{
			name = "_" + _privatePrefix + name;
		}
		return name;
	}

	private string ReadNameMaybeNone()
	{
		Token token = PeekToken();
		if (token == Tokens.NoneToken)
		{
			NextToken();
			return "None";
		}
		if (!(token is NameToken nameToken))
		{
			ReportSyntaxError("syntax error");
			return null;
		}
		NextToken();
		return FixName(nameToken.Name);
	}

	private string ReadName()
	{
		if (!(PeekToken() is NameToken nameToken))
		{
			ReportSyntaxError(_lookahead);
			return null;
		}
		NextToken();
		return FixName(nameToken.Name);
	}

	private Statement ParseStmt()
	{
		return PeekToken().Kind switch
		{
			TokenKind.KeywordIf => ParseIfStmt(), 
			TokenKind.KeywordWhile => ParseWhileStmt(), 
			TokenKind.KeywordFor => ParseForStmt(), 
			TokenKind.KeywordTry => ParseTryStatement(), 
			TokenKind.At => ParseDecorated(), 
			TokenKind.KeywordDef => ParseFuncDef(), 
			TokenKind.KeywordClass => ParseClassDef(), 
			TokenKind.LastKeyword => ParseWithStmt(), 
			_ => ParseSimpleStmt(), 
		};
	}

	private Statement ParseSimpleStmt()
	{
		Statement statement = ParseSmallStmt();
		if (MaybeEat(TokenKind.Semicolon))
		{
			int startIndex = statement.StartIndex;
			List<Statement> list = new List<Statement>();
			list.Add(statement);
			while (!MaybeEatNewLine() && !MaybeEat(TokenKind.EndOfFile))
			{
				list.Add(ParseSmallStmt());
				if (MaybeEat(TokenKind.EndOfFile))
				{
					break;
				}
				if (!MaybeEat(TokenKind.Semicolon))
				{
					EatNewLine();
					break;
				}
			}
			Statement[] array = list.ToArray();
			SuiteStatement suiteStatement = new SuiteStatement(array);
			suiteStatement.SetLoc(_globalParent, startIndex, array[array.Length - 1].EndIndex);
			return suiteStatement;
		}
		if (!MaybeEat(TokenKind.EndOfFile) && !EatNewLine())
		{
			NextToken();
		}
		return statement;
	}

	private Statement ParseSmallStmt()
	{
		switch (PeekToken().Kind)
		{
		case TokenKind.KeywordPrint:
			return ParsePrintStmt();
		case TokenKind.KeywordPass:
			return FinishSmallStmt(new EmptyStatement());
		case TokenKind.KeywordBreak:
			if (!_inLoop)
			{
				ReportSyntaxError("'break' outside loop");
			}
			return FinishSmallStmt(new BreakStatement());
		case TokenKind.KeywordContinue:
			if (!_inLoop)
			{
				ReportSyntaxError("'continue' not properly in loop");
			}
			else if (_inFinally)
			{
				ReportSyntaxError("'continue' not supported inside 'finally' clause");
			}
			return FinishSmallStmt(new ContinueStatement());
		case TokenKind.KeywordReturn:
			return ParseReturnStmt();
		case TokenKind.KeywordFrom:
			return ParseFromImportStmt();
		case TokenKind.KeywordImport:
			return ParseImportStmt();
		case TokenKind.KeywordGlobal:
			return ParseGlobalStmt();
		case TokenKind.KeywordRaise:
			return ParseRaiseStmt();
		case TokenKind.KeywordAssert:
			return ParseAssertStmt();
		case TokenKind.KeywordExec:
			return ParseExecStmt();
		case TokenKind.KeywordDel:
			return ParseDelStmt();
		case TokenKind.KeywordYield:
			return ParseYieldStmt();
		default:
			return ParseExprStmt();
		}
	}

	private Statement ParseDelStmt()
	{
		NextToken();
		int start = GetStart();
		List<Expression> list = ParseExprList();
		foreach (Expression item in list)
		{
			string text = item.CheckDelete();
			if (text != null)
			{
				ReportSyntaxError(item.StartIndex, item.EndIndex, text, 16);
			}
		}
		DelStatement delStatement = new DelStatement(list.ToArray());
		delStatement.SetLoc(_globalParent, start, GetEnd());
		return delStatement;
	}

	private Statement ParseReturnStmt()
	{
		if (CurrentFunction == null)
		{
			ReportSyntaxError(Resources.MisplacedReturn);
		}
		NextToken();
		Expression expression = null;
		int start = GetStart();
		if (!NeverTestToken(PeekToken()))
		{
			expression = ParseTestListAsExpr();
		}
		if (expression != null)
		{
			_returnWithValue = true;
			if (_isGenerator)
			{
				ReportSyntaxError("'return' with argument inside generator");
			}
		}
		ReturnStatement returnStatement = new ReturnStatement(expression);
		returnStatement.SetLoc(_globalParent, start, GetEnd());
		return returnStatement;
	}

	private Statement FinishSmallStmt(Statement stmt)
	{
		NextToken();
		stmt.SetLoc(_globalParent, GetStart(), GetEnd());
		return stmt;
	}

	private Statement ParseYieldStmt()
	{
		FunctionDefinition currentFunction = CurrentFunction;
		if (currentFunction == null)
		{
			ReportSyntaxError(Resources.MisplacedYield);
		}
		_isGenerator = true;
		if (_returnWithValue)
		{
			ReportSyntaxError("'return' with argument inside generator");
		}
		Eat(TokenKind.KeywordYield);
		Expression expression = ParseYieldExpression();
		Statement statement = new ExpressionStatement(expression);
		statement.SetLoc(_globalParent, expression.IndexSpan);
		return statement;
	}

	private Expression ParseYieldExpression()
	{
		FunctionDefinition currentFunction = CurrentFunction;
		if (currentFunction != null)
		{
			currentFunction.IsGenerator = true;
		}
		int start = GetStart();
		bool trailingComma;
		List<Expression> list = ParseExpressionList(out trailingComma);
		Expression expression = ((list.Count == 0) ? new ConstantExpression(null) : ((list.Count == 1) ? list[0] : MakeTupleOrExpr(list, trailingComma)));
		Expression expression2 = new YieldExpression(expression);
		expression2.SetLoc(_globalParent, start, GetEnd());
		return expression2;
	}

	private Statement FinishAssignments(Expression right)
	{
		List<Expression> list = null;
		Expression expression = null;
		while (MaybeEat(TokenKind.Assign))
		{
			string text = right.CheckAssign();
			if (text != null)
			{
				ReportSyntaxError(right.StartIndex, right.EndIndex, text, 80);
			}
			if (expression == null)
			{
				expression = right;
			}
			else
			{
				if (list == null)
				{
					list = new List<Expression>();
					list.Add(expression);
				}
				list.Add(right);
			}
			right = ((!MaybeEat(TokenKind.KeywordYield)) ? ParseTestListAsExpr() : ParseYieldExpression());
		}
		if (list != null)
		{
			AssignmentStatement assignmentStatement = new AssignmentStatement(list.ToArray(), right);
			assignmentStatement.SetLoc(_globalParent, list[0].StartIndex, right.EndIndex);
			return assignmentStatement;
		}
		AssignmentStatement assignmentStatement2 = new AssignmentStatement(new Expression[1] { expression }, right);
		assignmentStatement2.SetLoc(_globalParent, expression.StartIndex, right.EndIndex);
		return assignmentStatement2;
	}

	private Statement ParseExprStmt()
	{
		Expression expression = ParseTestListAsExpr();
		if (expression is ErrorExpression)
		{
			NextToken();
		}
		if (PeekToken(TokenKind.Assign))
		{
			return FinishAssignments(expression);
		}
		PythonOperator assignOperator = GetAssignOperator(PeekToken());
		if (assignOperator != PythonOperator.None)
		{
			NextToken();
			Expression right = ((!MaybeEat(TokenKind.KeywordYield)) ? ParseTestListAsExpr() : ParseYieldExpression());
			string text = expression.CheckAugmentedAssign();
			if (text != null)
			{
				ReportSyntaxError(text);
			}
			AugmentedAssignStatement augmentedAssignStatement = new AugmentedAssignStatement(assignOperator, expression, right);
			augmentedAssignStatement.SetLoc(_globalParent, expression.StartIndex, GetEnd());
			return augmentedAssignStatement;
		}
		Statement statement = new ExpressionStatement(expression);
		statement.SetLoc(_globalParent, expression.IndexSpan);
		return statement;
	}

	private PythonOperator GetAssignOperator(Token t)
	{
		switch (t.Kind)
		{
		case TokenKind.AddEqual:
			return PythonOperator.Add;
		case TokenKind.SubtractEqual:
			return PythonOperator.Subtract;
		case TokenKind.MultiplyEqual:
			return PythonOperator.Multiply;
		case TokenKind.DivideEqual:
			if (!TrueDivision)
			{
				return PythonOperator.Divide;
			}
			return PythonOperator.TrueDivide;
		case TokenKind.ModEqual:
			return PythonOperator.Mod;
		case TokenKind.BitwiseAndEqual:
			return PythonOperator.BitwiseAnd;
		case TokenKind.BitwiseOrEqual:
			return PythonOperator.BitwiseOr;
		case TokenKind.ExclusiveOrEqual:
			return PythonOperator.Xor;
		case TokenKind.LeftShiftEqual:
			return PythonOperator.LeftShift;
		case TokenKind.RightShiftEqual:
			return PythonOperator.RightShift;
		case TokenKind.PowerEqual:
			return PythonOperator.Power;
		case TokenKind.FloorDivideEqual:
			return PythonOperator.FloorDivide;
		default:
			return PythonOperator.None;
		}
	}

	private PythonOperator GetBinaryOperator(OperatorToken token)
	{
		switch (token.Kind)
		{
		case TokenKind.Add:
			return PythonOperator.Add;
		case TokenKind.Subtract:
			return PythonOperator.Subtract;
		case TokenKind.Multiply:
			return PythonOperator.Multiply;
		case TokenKind.Divide:
			if (!TrueDivision)
			{
				return PythonOperator.Divide;
			}
			return PythonOperator.TrueDivide;
		case TokenKind.Mod:
			return PythonOperator.Mod;
		case TokenKind.BitwiseAnd:
			return PythonOperator.BitwiseAnd;
		case TokenKind.BitwiseOr:
			return PythonOperator.BitwiseOr;
		case TokenKind.ExclusiveOr:
			return PythonOperator.Xor;
		case TokenKind.LeftShift:
			return PythonOperator.LeftShift;
		case TokenKind.RightShift:
			return PythonOperator.RightShift;
		case TokenKind.Power:
			return PythonOperator.Power;
		case TokenKind.FloorDivide:
			return PythonOperator.FloorDivide;
		default:
		{
			string msg = string.Format(CultureInfo.InvariantCulture, Resources.UnexpectedToken, new object[1] { token.Kind });
			throw new ValueErrorException(msg);
		}
		}
	}

	private ImportStatement ParseImportStmt()
	{
		Eat(TokenKind.KeywordImport);
		int start = GetStart();
		List<ModuleName> list = new List<ModuleName>();
		List<string> list2 = new List<string>();
		list.Add(ParseModuleName());
		list2.Add(MaybeParseAsName());
		while (MaybeEat(TokenKind.Comma))
		{
			list.Add(ParseModuleName());
			list2.Add(MaybeParseAsName());
		}
		ModuleName[] names = list.ToArray();
		string[] asNames = list2.ToArray();
		ImportStatement importStatement = new ImportStatement(names, asNames, AbsoluteImports);
		importStatement.SetLoc(_globalParent, start, GetEnd());
		return importStatement;
	}

	private ModuleName ParseModuleName()
	{
		int start = GetStart();
		ModuleName moduleName = new ModuleName(ReadNames());
		moduleName.SetLoc(_globalParent, start, GetEnd());
		return moduleName;
	}

	private ModuleName ParseRelativeModuleName()
	{
		int start = GetStart();
		int num = 0;
		while (MaybeEat(TokenKind.Dot))
		{
			num++;
		}
		string[] array = ArrayUtils.EmptyStrings;
		if (PeekToken() is NameToken)
		{
			array = ReadNames();
		}
		ModuleName moduleName;
		if (num > 0)
		{
			moduleName = new RelativeModuleName(array, num);
		}
		else
		{
			if (array.Length == 0)
			{
				ReportSyntaxError(_lookahead.Span.Start, _lookahead.Span.End, "invalid syntax");
			}
			moduleName = new ModuleName(array);
		}
		moduleName.SetLoc(_globalParent, start, GetEnd());
		return moduleName;
	}

	private string[] ReadNames()
	{
		List<string> list = new List<string>();
		list.Add(ReadName());
		while (MaybeEat(TokenKind.Dot))
		{
			list.Add(ReadName());
		}
		return list.ToArray();
	}

	private FromImportStatement ParseFromImportStmt()
	{
		Eat(TokenKind.KeywordFrom);
		int start = GetStart();
		ModuleName moduleName = ParseRelativeModuleName();
		Eat(TokenKind.KeywordImport);
		bool flag = MaybeEat(TokenKind.LeftParenthesis);
		bool fromFuture = false;
		string[] array;
		string[] asNames;
		if (MaybeEat(TokenKind.Multiply))
		{
			array = (string[])FromImportStatement.Star;
			asNames = null;
		}
		else
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			if (MaybeEat(TokenKind.LeftParenthesis))
			{
				ParseAsNameList(list, list2);
				Eat(TokenKind.RightParenthesis);
			}
			else
			{
				ParseAsNameList(list, list2);
			}
			array = list.ToArray();
			asNames = list2.ToArray();
		}
		if (moduleName.Names.Count == 1 && moduleName.Names[0] == "__future__")
		{
			if (!_fromFutureAllowed)
			{
				ReportSyntaxError(Resources.MisplacedFuture);
			}
			if (array == FromImportStatement.Star)
			{
				ReportSyntaxError(Resources.NoFutureStar);
			}
			fromFuture = true;
			string[] array2 = array;
			foreach (string text in array2)
			{
				switch (text)
				{
				case "division":
					_languageFeatures |= ModuleOptions.TrueDivision;
					continue;
				case "with_statement":
					_languageFeatures |= ModuleOptions.WithStatement;
					continue;
				case "absolute_import":
					_languageFeatures |= ModuleOptions.AbsoluteImports;
					continue;
				case "print_function":
					_languageFeatures |= ModuleOptions.PrintFunction;
					_tokenizer.PrintFunction = true;
					continue;
				case "unicode_literals":
					_tokenizer.UnicodeLiterals = true;
					_languageFeatures |= ModuleOptions.UnicodeLiterals;
					continue;
				case "nested_scopes":
				case "generators":
					continue;
				}
				string text2 = text;
				fromFuture = false;
				if (text2 != "braces")
				{
					ReportSyntaxError(Resources.UnknownFutureFeature + text2);
				}
				else
				{
					ReportSyntaxError(Resources.NotAChance);
				}
			}
		}
		if (flag)
		{
			Eat(TokenKind.RightParenthesis);
		}
		FromImportStatement fromImportStatement = new FromImportStatement(moduleName, array, asNames, fromFuture, AbsoluteImports);
		fromImportStatement.SetLoc(_globalParent, start, GetEnd());
		return fromImportStatement;
	}

	private void ParseAsNameList(List<string> l, List<string> las)
	{
		l.Add(ReadName());
		las.Add(MaybeParseAsName());
		while (MaybeEat(TokenKind.Comma) && !PeekToken(TokenKind.RightParenthesis))
		{
			l.Add(ReadName());
			las.Add(MaybeParseAsName());
		}
	}

	private string MaybeParseAsName()
	{
		if (MaybeEat(TokenKind.KeywordAs))
		{
			return ReadName();
		}
		return null;
	}

	private ExecStatement ParseExecStmt()
	{
		Eat(TokenKind.KeywordExec);
		int start = GetStart();
		Expression locals = null;
		Expression globals = null;
		Expression code = ParseExpr();
		if (MaybeEat(TokenKind.KeywordIn))
		{
			globals = ParseExpression();
			if (MaybeEat(TokenKind.Comma))
			{
				locals = ParseExpression();
			}
		}
		ExecStatement execStatement = new ExecStatement(code, locals, globals);
		execStatement.SetLoc(_globalParent, start, GetEnd());
		return execStatement;
	}

	private GlobalStatement ParseGlobalStmt()
	{
		Eat(TokenKind.KeywordGlobal);
		int start = GetStart();
		List<string> list = new List<string>();
		list.Add(ReadName());
		while (MaybeEat(TokenKind.Comma))
		{
			list.Add(ReadName());
		}
		string[] names = list.ToArray();
		GlobalStatement globalStatement = new GlobalStatement(names);
		globalStatement.SetLoc(_globalParent, start, GetEnd());
		return globalStatement;
	}

	private RaiseStatement ParseRaiseStmt()
	{
		Eat(TokenKind.KeywordRaise);
		int start = GetStart();
		Expression exceptionType = null;
		Expression exceptionValue = null;
		Expression traceBack = null;
		if (!NeverTestToken(PeekToken()))
		{
			exceptionType = ParseExpression();
			if (MaybeEat(TokenKind.Comma))
			{
				exceptionValue = ParseExpression();
				if (MaybeEat(TokenKind.Comma))
				{
					traceBack = ParseExpression();
				}
			}
		}
		RaiseStatement raiseStatement = new RaiseStatement(exceptionType, exceptionValue, traceBack);
		raiseStatement.SetLoc(_globalParent, start, GetEnd());
		return raiseStatement;
	}

	private AssertStatement ParseAssertStmt()
	{
		Eat(TokenKind.KeywordAssert);
		int start = GetStart();
		Expression test = ParseExpression();
		Expression message = null;
		if (MaybeEat(TokenKind.Comma))
		{
			message = ParseExpression();
		}
		AssertStatement assertStatement = new AssertStatement(test, message);
		assertStatement.SetLoc(_globalParent, start, GetEnd());
		return assertStatement;
	}

	private PrintStatement ParsePrintStmt()
	{
		Eat(TokenKind.KeywordPrint);
		int start = GetStart();
		Expression destination = null;
		bool flag = false;
		PrintStatement printStatement;
		if (MaybeEat(TokenKind.RightShift))
		{
			destination = ParseExpression();
			if (!MaybeEat(TokenKind.Comma))
			{
				printStatement = new PrintStatement(destination, new Expression[0], trailingComma: false);
				printStatement.SetLoc(_globalParent, start, GetEnd());
				return printStatement;
			}
			flag = true;
		}
		bool trailingComma;
		List<Expression> list = ParseExpressionList(out trailingComma);
		if (flag && list.Count == 0)
		{
			ReportSyntaxError(_lookahead);
		}
		Expression[] expressions = list.ToArray();
		printStatement = new PrintStatement(destination, expressions, trailingComma);
		printStatement.SetLoc(_globalParent, start, GetEnd());
		return printStatement;
	}

	private string SetPrivatePrefix(string name)
	{
		string privatePrefix = _privatePrefix;
		_privatePrefix = GetPrivatePrefix(name);
		return privatePrefix;
	}

	internal static string GetPrivatePrefix(string name)
	{
		if (name != null)
		{
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] != '_')
				{
					return name.Substring(i);
				}
			}
		}
		return null;
	}

	private ErrorExpression Error()
	{
		ErrorExpression errorExpression = new ErrorExpression();
		errorExpression.SetLoc(_globalParent, GetStart(), GetEnd());
		return errorExpression;
	}

	private ExpressionStatement ErrorStmt()
	{
		return new ExpressionStatement(Error());
	}

	private ClassDefinition ParseClassDef()
	{
		Eat(TokenKind.KeywordClass);
		int start = GetStart();
		string text = ReadName();
		if (text == null)
		{
			return new ClassDefinition(null, new Expression[0], ErrorStmt());
		}
		Expression[] bases = new Expression[0];
		if (MaybeEat(TokenKind.LeftParenthesis))
		{
			List<Expression> list = ParseTestList();
			if (list.Count == 1 && list[0] is ErrorExpression)
			{
				return new ClassDefinition(text, new Expression[0], ErrorStmt());
			}
			bases = list.ToArray();
			Eat(TokenKind.RightParenthesis);
		}
		int end = GetEnd();
		string privatePrefix = SetPrivatePrefix(text);
		Statement body = ParseClassOrFuncBody();
		_privatePrefix = privatePrefix;
		ClassDefinition classDefinition = new ClassDefinition(text, bases, body);
		classDefinition.HeaderIndex = end;
		classDefinition.SetLoc(_globalParent, start, GetEnd());
		return classDefinition;
	}

	private List<Expression> ParseDecorators()
	{
		List<Expression> list = new List<Expression>();
		while (MaybeEat(TokenKind.At))
		{
			int start = GetStart();
			Expression expression = new NameExpression(ReadName());
			expression.SetLoc(_globalParent, start, GetEnd());
			while (MaybeEat(TokenKind.Dot))
			{
				string name = ReadNameMaybeNone();
				expression = new MemberExpression(expression, name);
				expression.SetLoc(_globalParent, GetStart(), GetEnd());
			}
			expression.SetLoc(_globalParent, start, GetEnd());
			if (MaybeEat(TokenKind.LeftParenthesis))
			{
				if (_sink != null)
				{
					_sink.StartParameters(GetSourceSpan());
				}
				Arg[] args = FinishArgumentList(null);
				expression = FinishCallExpr(expression, args);
			}
			expression.SetLoc(_globalParent, start, GetEnd());
			EatNewLine();
			list.Add(expression);
		}
		return list;
	}

	private Statement ParseDecorated()
	{
		List<Expression> list = ParseDecorators();
		Statement result;
		if (PeekToken() == Tokens.KeywordDefToken)
		{
			FunctionDefinition functionDefinition = ParseFuncDef();
			functionDefinition.Decorators = list.ToArray();
			result = functionDefinition;
		}
		else if (PeekToken() == Tokens.KeywordClassToken)
		{
			ClassDefinition classDefinition = ParseClassDef();
			classDefinition.Decorators = list.ToArray();
			result = classDefinition;
		}
		else
		{
			result = new EmptyStatement();
			ReportSyntaxError(_lookahead);
		}
		return result;
	}

	private FunctionDefinition ParseFuncDef()
	{
		Eat(TokenKind.KeywordDef);
		int start = GetStart();
		string name = ReadName();
		Eat(TokenKind.LeftParenthesis);
		int start2 = GetStart();
		int end = GetEnd();
		int groupingLevel = _tokenizer.GroupingLevel;
		Parameter[] array = ParseVarArgsList(TokenKind.RightParenthesis);
		FunctionDefinition functionDefinition;
		if (array == null)
		{
			functionDefinition = new FunctionDefinition(name, new Parameter[0]);
			functionDefinition.SetLoc(_globalParent, start, end);
			return functionDefinition;
		}
		int start3 = GetStart();
		int end2 = GetEnd();
		functionDefinition = new FunctionDefinition(name, array);
		PushFunction(functionDefinition);
		Statement statement = ParseClassOrFuncBody();
		PopFunction();
		functionDefinition.Body = statement;
		functionDefinition.HeaderIndex = end2;
		if (_sink != null)
		{
			_sink.MatchPair(new SourceSpan(_tokenizer.IndexToLocation(start2), _tokenizer.IndexToLocation(end)), new SourceSpan(_tokenizer.IndexToLocation(start3), _tokenizer.IndexToLocation(end2)), groupingLevel);
		}
		functionDefinition.SetLoc(_globalParent, start, statement.EndIndex);
		return functionDefinition;
	}

	private Parameter ParseParameterName(HashSet<string> names, ParameterKind kind)
	{
		string text = ReadName();
		if (text != null)
		{
			CheckUniqueParameter(names, text);
			Parameter parameter = new Parameter(text, kind);
			parameter.SetLoc(_globalParent, GetStart(), GetEnd());
			return parameter;
		}
		return null;
	}

	private void CheckUniqueParameter(HashSet<string> names, string name)
	{
		if (names.Contains(name))
		{
			ReportSyntaxError(string.Format(CultureInfo.InvariantCulture, Resources.DuplicateArgumentInFuncDef, new object[1] { name }));
		}
		names.Add(name);
	}

	private Parameter[] ParseVarArgsList(TokenKind terminator)
	{
		List<Parameter> list = new List<Parameter>();
		HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
		bool flag = false;
		int num = 0;
		while (!MaybeEat(terminator))
		{
			Parameter parameter;
			if (MaybeEat(TokenKind.Multiply))
			{
				parameter = ParseParameterName(names, ParameterKind.List);
				if (parameter == null)
				{
					return null;
				}
				list.Add(parameter);
				if (MaybeEat(TokenKind.Comma))
				{
					Eat(TokenKind.Power);
					parameter = ParseParameterName(names, ParameterKind.Dictionary);
					if (parameter == null)
					{
						return null;
					}
					list.Add(parameter);
				}
				Eat(terminator);
				break;
			}
			if (MaybeEat(TokenKind.Power))
			{
				parameter = ParseParameterName(names, ParameterKind.Dictionary);
				if (parameter == null)
				{
					return null;
				}
				list.Add(parameter);
				Eat(terminator);
				break;
			}
			if ((parameter = ParseParameter(num, names)) != null)
			{
				list.Add(parameter);
				if (MaybeEat(TokenKind.Assign))
				{
					flag = true;
					parameter.DefaultValue = ParseExpression();
				}
				else if (flag)
				{
					ReportSyntaxError(Resources.DefaultRequired);
				}
				if (!MaybeEat(TokenKind.Comma))
				{
					Eat(terminator);
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return list.ToArray();
	}

	private Parameter ParseParameter(int position, HashSet<string> names)
	{
		Token token = PeekToken();
		Parameter parameter = null;
		switch (token.Kind)
		{
		case TokenKind.LeftParenthesis:
		{
			NextToken();
			Expression expression = ParseSublist(names);
			Eat(TokenKind.RightParenthesis);
			if (expression is TupleExpression tuple)
			{
				parameter = new SublistParameter(position, tuple);
			}
			else if (expression is NameExpression nameExpression)
			{
				parameter = new Parameter(nameExpression.Name);
			}
			else
			{
				ReportSyntaxError(_lookahead);
			}
			parameter?.SetLoc(_globalParent, expression.IndexSpan);
			break;
		}
		case TokenKind.Name:
		{
			NextToken();
			string name = FixName((string)token.Value);
			parameter = new Parameter(name);
			CompleteParameterName(parameter, name, names);
			break;
		}
		default:
			ReportSyntaxError(_lookahead);
			break;
		}
		return parameter;
	}

	private void CompleteParameterName(Node node, string name, HashSet<string> names)
	{
		if (_sink != null)
		{
			_sink.StartName(GetSourceSpan(), name);
		}
		CheckUniqueParameter(names, name);
		node.SetLoc(_globalParent, GetStart(), GetEnd());
	}

	private Expression ParseSublistParameter(HashSet<string> names)
	{
		Token token = NextToken();
		Expression expression = null;
		switch (token.Kind)
		{
		case TokenKind.LeftParenthesis:
			expression = ParseSublist(names);
			Eat(TokenKind.RightParenthesis);
			break;
		case TokenKind.Name:
		{
			string name = FixName((string)token.Value);
			NameExpression nameExpression = new NameExpression(name);
			CompleteParameterName(nameExpression, name, names);
			return nameExpression;
		}
		default:
			ReportSyntaxError(_token);
			expression = Error();
			break;
		}
		return expression;
	}

	private Expression ParseSublist(HashSet<string> names)
	{
		List<Expression> list = new List<Expression>();
		bool flag;
		TokenKind kind;
		do
		{
			flag = false;
			list.Add(ParseSublistParameter(names));
			if (MaybeEat(TokenKind.Comma))
			{
				flag = true;
				kind = PeekToken().Kind;
				continue;
			}
			flag = false;
			break;
		}
		while (kind == TokenKind.Name || kind == TokenKind.LeftParenthesis);
		return MakeTupleOrExpr(list, flag);
	}

	private Expression FinishOldLambdef()
	{
		FunctionDefinition func = ParseLambdaHelperStart(null);
		Expression expr = ParseOldExpression();
		return ParseLambdaHelperEnd(func, expr);
	}

	private Expression FinishLambdef()
	{
		FunctionDefinition func = ParseLambdaHelperStart(null);
		Expression expr = ParseExpression();
		return ParseLambdaHelperEnd(func, expr);
	}

	private FunctionDefinition ParseLambdaHelperStart(string name)
	{
		int start = GetStart();
		Parameter[] array = ParseVarArgsList(TokenKind.Colon);
		int end = GetEnd();
		FunctionDefinition functionDefinition = new FunctionDefinition(name, array ?? new Parameter[0]);
		functionDefinition.HeaderIndex = end;
		functionDefinition.StartIndex = start;
		PushFunction(functionDefinition);
		return functionDefinition;
	}

	private Expression ParseLambdaHelperEnd(FunctionDefinition func, Expression expr)
	{
		Statement statement;
		if (func.IsGenerator)
		{
			YieldExpression yieldExpression = new YieldExpression(expr);
			yieldExpression.SetLoc(_globalParent, expr.IndexSpan);
			statement = new ExpressionStatement(yieldExpression);
		}
		else
		{
			statement = new ReturnStatement(expr);
		}
		statement.SetLoc(_globalParent, expr.StartIndex, expr.EndIndex);
		PopFunction();
		func.Body = statement;
		func.EndIndex = GetEnd();
		LambdaExpression lambdaExpression = new LambdaExpression(func);
		func.SetLoc(_globalParent, func.IndexSpan);
		lambdaExpression.SetLoc(_globalParent, func.IndexSpan);
		return lambdaExpression;
	}

	private WhileStatement ParseWhileStmt()
	{
		Eat(TokenKind.KeywordWhile);
		int start = GetStart();
		Expression test = ParseExpression();
		int end = GetEnd();
		Statement body = ParseLoopSuite();
		Statement else_ = null;
		if (MaybeEat(TokenKind.KeywordElse))
		{
			else_ = ParseSuite();
		}
		WhileStatement whileStatement = new WhileStatement(test, body, else_);
		whileStatement.SetLoc(_globalParent, start, end, GetEnd());
		return whileStatement;
	}

	private WithStatement ParseWithStmt()
	{
		Eat(TokenKind.LastKeyword);
		WithItem withItem = ParseWithItem();
		List<WithItem> list = null;
		while (MaybeEat(TokenKind.Comma))
		{
			if (list == null)
			{
				list = new List<WithItem>();
			}
			list.Add(ParseWithItem());
		}
		int end = GetEnd();
		Statement body = ParseSuite();
		if (list != null)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				WithItem withItem2 = list[num];
				WithStatement withStatement = new WithStatement(withItem2.ContextManager, withItem2.Variable, body);
				withStatement.HeaderIndex = end;
				withStatement.SetLoc(_globalParent, withItem.Start, GetEnd());
				body = withStatement;
				end = GetEnd();
			}
		}
		WithStatement withStatement2 = new WithStatement(withItem.ContextManager, withItem.Variable, body);
		withStatement2.HeaderIndex = end;
		withStatement2.SetLoc(_globalParent, withItem.Start, GetEnd());
		return withStatement2;
	}

	private WithItem ParseWithItem()
	{
		int start = GetStart();
		Expression contextManager = ParseExpression();
		Expression variable = null;
		if (MaybeEat(TokenKind.KeywordAs))
		{
			variable = ParseExpression();
		}
		return new WithItem(start, contextManager, variable);
	}

	private ForStatement ParseForStmt()
	{
		Eat(TokenKind.KeywordFor);
		int start = GetStart();
		bool trailingComma;
		List<Expression> l = ParseTargetList(out trailingComma);
		Expression left = MakeTupleOrExpr(l, trailingComma);
		Eat(TokenKind.KeywordIn);
		Expression list = ParseTestListAsExpr();
		int end = GetEnd();
		Statement body = ParseLoopSuite();
		Statement else_ = null;
		if (MaybeEat(TokenKind.KeywordElse))
		{
			else_ = ParseSuite();
		}
		ForStatement forStatement = new ForStatement(left, list, body, else_);
		forStatement.HeaderIndex = end;
		forStatement.SetLoc(_globalParent, start, GetEnd());
		return forStatement;
	}

	private Statement ParseLoopSuite()
	{
		bool inLoop = _inLoop;
		try
		{
			_inLoop = true;
			return ParseSuite();
		}
		finally
		{
			_inLoop = inLoop;
		}
	}

	private Statement ParseClassOrFuncBody()
	{
		bool inLoop = _inLoop;
		bool inFinally = _inFinally;
		bool isGenerator = _isGenerator;
		bool returnWithValue = _returnWithValue;
		try
		{
			_inLoop = false;
			_inFinally = false;
			_isGenerator = false;
			_returnWithValue = false;
			return ParseSuite();
		}
		finally
		{
			_inLoop = inLoop;
			_inFinally = inFinally;
			_isGenerator = isGenerator;
			_returnWithValue = returnWithValue;
		}
	}

	private IfStatement ParseIfStmt()
	{
		Eat(TokenKind.KeywordIf);
		int start = GetStart();
		List<IfStatementTest> list = new List<IfStatementTest>();
		list.Add(ParseIfStmtTest());
		while (MaybeEat(TokenKind.KeywordElseIf))
		{
			list.Add(ParseIfStmtTest());
		}
		Statement else_ = null;
		if (MaybeEat(TokenKind.KeywordElse))
		{
			else_ = ParseSuite();
		}
		IfStatementTest[] tests = list.ToArray();
		IfStatement ifStatement = new IfStatement(tests, else_);
		ifStatement.SetLoc(_globalParent, start, GetEnd());
		return ifStatement;
	}

	private IfStatementTest ParseIfStmtTest()
	{
		int start = GetStart();
		Expression test = ParseExpression();
		int end = GetEnd();
		Statement statement = ParseSuite();
		IfStatementTest ifStatementTest = new IfStatementTest(test, statement);
		ifStatementTest.SetLoc(_globalParent, start, statement.EndIndex);
		ifStatementTest.HeaderIndex = end;
		return ifStatementTest;
	}

	private Statement ParseTryStatement()
	{
		Eat(TokenKind.KeywordTry);
		int start = GetStart();
		int end = GetEnd();
		Statement body = ParseSuite();
		Statement statement = null;
		Statement statement2 = null;
		int endIndex;
		Statement statement3;
		if (MaybeEat(TokenKind.KeywordFinally))
		{
			statement = ParseFinallySuite(statement);
			endIndex = statement.EndIndex;
			TryStatement tryStatement = new TryStatement(body, null, statement2, statement);
			tryStatement.HeaderIndex = end;
			statement3 = tryStatement;
		}
		else
		{
			List<TryStatementHandler> list = new List<TryStatementHandler>();
			TryStatementHandler tryStatementHandler = null;
			do
			{
				TryStatementHandler tryStatementHandler2 = ParseTryStmtHandler();
				endIndex = tryStatementHandler2.EndIndex;
				list.Add(tryStatementHandler2);
				if (tryStatementHandler != null)
				{
					ReportSyntaxError(tryStatementHandler.StartIndex, tryStatementHandler.EndIndex, "default 'except' must be last");
				}
				if (tryStatementHandler2.Test == null)
				{
					tryStatementHandler = tryStatementHandler2;
				}
			}
			while (PeekToken().Kind == TokenKind.KeywordExcept);
			if (MaybeEat(TokenKind.KeywordElse))
			{
				statement2 = ParseSuite();
				endIndex = statement2.EndIndex;
			}
			if (MaybeEat(TokenKind.KeywordFinally))
			{
				statement = ParseFinallySuite(statement);
				endIndex = statement.EndIndex;
			}
			TryStatement tryStatement2 = new TryStatement(body, list.ToArray(), statement2, statement);
			tryStatement2.HeaderIndex = end;
			statement3 = tryStatement2;
		}
		statement3.SetLoc(_globalParent, start, endIndex);
		return statement3;
	}

	private Statement ParseFinallySuite(Statement finallySuite)
	{
		MarkFunctionContainsFinally();
		bool inFinally = _inFinally;
		try
		{
			_inFinally = true;
			finallySuite = ParseSuite();
			return finallySuite;
		}
		finally
		{
			_inFinally = inFinally;
		}
	}

	private void MarkFunctionContainsFinally()
	{
		FunctionDefinition currentFunction = CurrentFunction;
		if (currentFunction != null)
		{
			currentFunction.ContainsTryFinally = true;
		}
	}

	private TryStatementHandler ParseTryStmtHandler()
	{
		Eat(TokenKind.KeywordExcept);
		FunctionDefinition currentFunction = CurrentFunction;
		if (currentFunction != null)
		{
			currentFunction.CanSetSysExcInfo = true;
		}
		int start = GetStart();
		Expression test = null;
		Expression target = null;
		if (PeekToken().Kind != TokenKind.Colon)
		{
			test = ParseExpression();
			if (MaybeEat(TokenKind.Comma) || MaybeEat(TokenKind.KeywordAs))
			{
				target = ParseExpression();
			}
		}
		int end = GetEnd();
		Statement statement = ParseSuite();
		TryStatementHandler tryStatementHandler = new TryStatementHandler(test, target, statement);
		tryStatementHandler.HeaderIndex = end;
		tryStatementHandler.SetLoc(_globalParent, start, statement.EndIndex);
		return tryStatementHandler;
	}

	private Statement ParseSuite()
	{
		if (!EatNoEof(TokenKind.Colon))
		{
			return ErrorStmt();
		}
		TokenWithSpan lookahead = _lookahead;
		List<Statement> list = new List<Statement>();
		if (MaybeEat(TokenKind.NewLine))
		{
			CheckSuiteEofError(lookahead);
			lookahead = _lookahead;
			while (PeekToken(TokenKind.NLToken))
			{
				lookahead = _lookahead;
				NextToken();
			}
			if (!MaybeEat(TokenKind.Indent))
			{
				if (lookahead.Token.Kind == TokenKind.Dedent)
				{
					ReportSyntaxError(_lookahead.Span.Start, _lookahead.Span.End, "expected an indented block", 17);
				}
				else
				{
					ReportSyntaxError(lookahead, 32);
				}
				return ErrorStmt();
			}
			while (true)
			{
				Statement item = ParseStmt();
				list.Add(item);
				if (MaybeEat(TokenKind.Dedent))
				{
					break;
				}
				if (PeekToken().Kind == TokenKind.EndOfFile)
				{
					ReportSyntaxError("unexpected end of file");
					break;
				}
			}
			Statement[] array = list.ToArray();
			SuiteStatement suiteStatement = new SuiteStatement(array);
			suiteStatement.SetLoc(_globalParent, array[0].StartIndex, array[array.Length - 1].EndIndex);
			return suiteStatement;
		}
		return ParseSimpleStmt();
	}

	private void CheckSuiteEofError(TokenWithSpan cur)
	{
		if (MaybeEat(TokenKind.EndOfFile))
		{
			ReportSyntaxError(_lookahead.Token, cur.Span, 16, allowIncomplete: true);
		}
	}

	private Expression ParseOldExpression()
	{
		if (MaybeEat(TokenKind.KeywordLambda))
		{
			return FinishOldLambdef();
		}
		return ParseOrTest();
	}

	private Expression ParseExpression()
	{
		if (MaybeEat(TokenKind.KeywordLambda))
		{
			return FinishLambdef();
		}
		Expression expression = ParseOrTest();
		if (MaybeEat(TokenKind.KeywordIf))
		{
			int startIndex = expression.StartIndex;
			expression = ParseConditionalTest(expression);
			expression.SetLoc(_globalParent, startIndex, GetEnd());
		}
		return expression;
	}

	private Expression ParseOrTest()
	{
		Expression expression = ParseAndTest();
		while (MaybeEat(TokenKind.KeywordOr))
		{
			int startIndex = expression.StartIndex;
			expression = new OrExpression(expression, ParseAndTest());
			expression.SetLoc(_globalParent, startIndex, GetEnd());
		}
		return expression;
	}

	private Expression ParseConditionalTest(Expression trueExpr)
	{
		Expression testExpression = ParseOrTest();
		Eat(TokenKind.KeywordElse);
		Expression falseExpression = ParseExpression();
		return new ConditionalExpression(testExpression, trueExpr, falseExpression);
	}

	private Expression ParseAndTest()
	{
		Expression expression = ParseNotTest();
		while (MaybeEat(TokenKind.FirstKeyword))
		{
			int startIndex = expression.StartIndex;
			expression = new AndExpression(expression, ParseAndTest());
			expression.SetLoc(_globalParent, startIndex, GetEnd());
		}
		return expression;
	}

	private Expression ParseNotTest()
	{
		if (MaybeEat(TokenKind.KeywordNot))
		{
			int start = GetStart();
			Expression expression = new UnaryExpression(PythonOperator.Not, ParseNotTest());
			expression.SetLoc(_globalParent, start, GetEnd());
			return expression;
		}
		return ParseComparison();
	}

	private Expression ParseComparison()
	{
		Expression expression = ParseExpr();
		while (true)
		{
			PythonOperator op;
			switch (PeekToken().Kind)
			{
			case TokenKind.LessThan:
				NextToken();
				op = PythonOperator.LessThan;
				break;
			case TokenKind.LessThanOrEqual:
				NextToken();
				op = PythonOperator.LessThanOrEqual;
				break;
			case TokenKind.GreaterThan:
				NextToken();
				op = PythonOperator.GreaterThan;
				break;
			case TokenKind.GreaterThanOrEqual:
				NextToken();
				op = PythonOperator.GreaterThanOrEqual;
				break;
			case TokenKind.Equals:
				NextToken();
				op = PythonOperator.Equal;
				break;
			case TokenKind.NotEquals:
				NextToken();
				op = PythonOperator.NotEqual;
				break;
			case TokenKind.LessThanGreaterThan:
				NextToken();
				op = PythonOperator.NotEqual;
				break;
			case TokenKind.KeywordIn:
				NextToken();
				op = PythonOperator.In;
				break;
			case TokenKind.KeywordNot:
				NextToken();
				Eat(TokenKind.KeywordIn);
				op = PythonOperator.NotIn;
				break;
			case TokenKind.KeywordIs:
				NextToken();
				op = ((!MaybeEat(TokenKind.KeywordNot)) ? PythonOperator.Is : PythonOperator.IsNot);
				break;
			default:
				return expression;
			}
			Expression right = ParseComparison();
			BinaryExpression binaryExpression = new BinaryExpression(op, expression, right);
			binaryExpression.SetLoc(_globalParent, expression.StartIndex, GetEnd());
			expression = binaryExpression;
		}
	}

	private Expression ParseExpr()
	{
		return ParseExpr(0);
	}

	private Expression ParseExpr(int precedence)
	{
		Expression expression = ParseFactor();
		while (true)
		{
			Token token = PeekToken();
			if (!(token is OperatorToken { Precedence: var precedence2 } operatorToken))
			{
				return expression;
			}
			if (precedence2 < precedence)
			{
				break;
			}
			NextToken();
			Expression right = ParseExpr(precedence2 + 1);
			int startIndex = expression.StartIndex;
			expression = new BinaryExpression(GetBinaryOperator(operatorToken), expression, right);
			expression.SetLoc(_globalParent, startIndex, GetEnd());
		}
		return expression;
	}

	private Expression ParseFactor()
	{
		int start = _lookahead.Span.Start;
		Expression expression;
		switch (PeekToken().Kind)
		{
		case TokenKind.Add:
			NextToken();
			expression = new UnaryExpression(PythonOperator.Pos, ParseFactor());
			break;
		case TokenKind.Subtract:
			NextToken();
			expression = FinishUnaryNegate();
			break;
		case TokenKind.Twiddle:
			NextToken();
			expression = new UnaryExpression(PythonOperator.Invert, ParseFactor());
			break;
		default:
			return ParsePower();
		}
		expression.SetLoc(_globalParent, start, GetEnd());
		return expression;
	}

	private Expression FinishUnaryNegate()
	{
		if (PeekToken().Kind == TokenKind.Constant)
		{
			Token token = PeekToken();
			if (token.Value is BigInteger)
			{
				BigInteger self = (BigInteger)token.Value;
				if (self.AsUInt32(out var ret) && ret == 2147483648u)
				{
					string tokenString = _tokenizer.GetTokenString();
					if (tokenString[tokenString.Length - 1] != 'L' && tokenString[tokenString.Length - 1] != 'l')
					{
						NextToken();
						return new ConstantExpression(int.MinValue);
					}
				}
			}
		}
		return new UnaryExpression(PythonOperator.Negate, ParseFactor());
	}

	private Expression ParsePower()
	{
		Expression ret = ParsePrimary();
		ret = AddTrailers(ret);
		if (MaybeEat(TokenKind.Power))
		{
			int startIndex = ret.StartIndex;
			ret = new BinaryExpression(PythonOperator.Power, ret, ParseFactor());
			ret.SetLoc(_globalParent, startIndex, GetEnd());
		}
		return ret;
	}

	private Expression ParsePrimary()
	{
		Token token = PeekToken();
		switch (token.Kind)
		{
		case TokenKind.LeftParenthesis:
			NextToken();
			return FinishTupleOrGenExp();
		case TokenKind.LeftBracket:
			NextToken();
			return FinishListValue();
		case TokenKind.LeftBrace:
			NextToken();
			return FinishDictOrSetValue();
		case TokenKind.BackQuote:
			NextToken();
			return FinishStringConversion();
		case TokenKind.Name:
		{
			NextToken();
			string name = (string)token.Value;
			if (_sink != null)
			{
				_sink.StartName(GetSourceSpan(), name);
			}
			Expression expression = new NameExpression(FixName(name));
			expression.SetLoc(_globalParent, GetStart(), GetEnd());
			return expression;
		}
		case TokenKind.Constant:
		{
			NextToken();
			int start = GetStart();
			object obj = token.Value;
			if (obj is string s)
			{
				obj = FinishStringPlus(s);
			}
			else if (obj is Bytes s2)
			{
				obj = FinishBytesPlus(s2);
			}
			Expression expression = ((!(token is UnicodeStringToken)) ? new ConstantExpression(obj) : ConstantExpression.MakeUnicode((string)obj));
			expression.SetLoc(_globalParent, start, GetEnd());
			return expression;
		}
		default:
		{
			ReportSyntaxError(_lookahead.Token, _lookahead.Span, 16, _allowIncomplete || _tokenizer.EndContinues);
			Expression expression = new ErrorExpression();
			expression.SetLoc(_globalParent, _lookahead.Span.Start, _lookahead.Span.End);
			return expression;
		}
		}
	}

	private string FinishStringPlus(string s)
	{
		Token token = PeekToken();
		while (token is ConstantValueToken)
		{
			if (token.Value is string text)
			{
				s += text;
				NextToken();
				token = PeekToken();
				continue;
			}
			ReportSyntaxError("invalid syntax");
			break;
		}
		return s;
	}

	private Bytes FinishBytesPlus(Bytes s)
	{
		Token token = PeekToken();
		while (token is ConstantValueToken)
		{
			if (token.Value is Bytes bytes)
			{
				s += bytes;
				NextToken();
				token = PeekToken();
				continue;
			}
			ReportSyntaxError("invalid syntax");
			break;
		}
		return s;
	}

	private Expression AddTrailers(Expression ret)
	{
		return AddTrailers(ret, allowGeneratorExpression: true);
	}

	private Expression AddTrailers(Expression ret, bool allowGeneratorExpression)
	{
		bool allowIncomplete = _allowIncomplete;
		try
		{
			_allowIncomplete = true;
			while (true)
			{
				switch (PeekToken().Kind)
				{
				case TokenKind.LeftParenthesis:
				{
					if (!allowGeneratorExpression)
					{
						return ret;
					}
					NextToken();
					Arg[] array = FinishArgListOrGenExpr();
					CallExpression callExpression = ((array == null) ? new CallExpression(ret, new Arg[0]) : FinishCallExpr(ret, array));
					callExpression.SetLoc(_globalParent, ret.StartIndex, GetEnd());
					ret = callExpression;
					break;
				}
				case TokenKind.LeftBracket:
				{
					NextToken();
					Expression index = ParseSubscriptList();
					IndexExpression indexExpression = new IndexExpression(ret, index);
					indexExpression.SetLoc(_globalParent, ret.StartIndex, GetEnd());
					ret = indexExpression;
					break;
				}
				case TokenKind.Dot:
				{
					NextToken();
					string name = ReadNameMaybeNone();
					MemberExpression memberExpression = new MemberExpression(ret, name);
					memberExpression.SetLoc(_globalParent, ret.StartIndex, GetEnd());
					ret = memberExpression;
					break;
				}
				case TokenKind.Constant:
					ReportSyntaxError("invalid syntax");
					return Error();
				default:
					return ret;
				}
			}
		}
		finally
		{
			_allowIncomplete = allowIncomplete;
		}
	}

	private Expression ParseSubscriptList()
	{
		int start = GetStart();
		bool flag = false;
		List<Expression> list = new List<Expression>();
		do
		{
			Expression expression;
			if (MaybeEat(TokenKind.Dot))
			{
				int start2 = GetStart();
				Eat(TokenKind.Dot);
				Eat(TokenKind.Dot);
				expression = new ConstantExpression(Ellipsis.Value);
				expression.SetLoc(_globalParent, start2, GetEnd());
			}
			else if (MaybeEat(TokenKind.Colon))
			{
				expression = FinishSlice(null, GetStart());
			}
			else
			{
				expression = ParseExpression();
				if (MaybeEat(TokenKind.Colon))
				{
					expression = FinishSlice(expression, expression.StartIndex);
				}
			}
			list.Add(expression);
			if (!MaybeEat(TokenKind.Comma))
			{
				Eat(TokenKind.RightBracket);
				flag = false;
				break;
			}
			flag = true;
		}
		while (!MaybeEat(TokenKind.RightBracket));
		Expression expression2 = MakeTupleOrExpr(list, flag, expandable: true);
		expression2.SetLoc(_globalParent, start, GetEnd());
		return expression2;
	}

	private Expression ParseSliceEnd()
	{
		Expression result = null;
		TokenKind kind = PeekToken().Kind;
		if (kind != TokenKind.RightBracket && kind != TokenKind.Comma)
		{
			result = ParseExpression();
		}
		return result;
	}

	private Expression FinishSlice(Expression e0, int start)
	{
		Expression stop = null;
		Expression step = null;
		bool stepProvided = false;
		switch (PeekToken().Kind)
		{
		case TokenKind.Colon:
			stepProvided = true;
			NextToken();
			step = ParseSliceEnd();
			break;
		default:
			stop = ParseExpression();
			if (MaybeEat(TokenKind.Colon))
			{
				stepProvided = true;
				step = ParseSliceEnd();
			}
			break;
		case TokenKind.RightBracket:
		case TokenKind.Comma:
			break;
		}
		SliceExpression sliceExpression = new SliceExpression(e0, stop, step, stepProvided);
		sliceExpression.SetLoc(_globalParent, start, GetEnd());
		return sliceExpression;
	}

	private List<Expression> ParseExprList()
	{
		List<Expression> list = new List<Expression>();
		do
		{
			Expression item = ParseExpr();
			list.Add(item);
		}
		while (MaybeEat(TokenKind.Comma) && !NeverTestToken(PeekToken()));
		return list;
	}

	private Arg[] FinishArgListOrGenExpr()
	{
		Arg arg = null;
		if (_sink != null)
		{
			_sink.StartParameters(GetSourceSpan());
		}
		Token token = PeekToken();
		if (token.Kind != TokenKind.RightParenthesis && token.Kind != TokenKind.Multiply && token.Kind != TokenKind.Power)
		{
			int start = GetStart();
			Expression expression = ParseExpression();
			if (expression is ErrorExpression)
			{
				return null;
			}
			if (MaybeEat(TokenKind.Assign))
			{
				arg = FinishKeywordArgument(expression);
				if (arg == null)
				{
					arg = new Arg(expression);
					arg.SetLoc(_globalParent, expression.StartIndex, GetEnd());
				}
			}
			else
			{
				if (PeekToken(Tokens.KeywordForToken))
				{
					arg = new Arg(ParseGeneratorExpression(expression));
					Eat(TokenKind.RightParenthesis);
					arg.SetLoc(_globalParent, start, GetEnd());
					if (_sink != null)
					{
						_sink.EndParameters(GetSourceSpan());
					}
					return new Arg[1] { arg };
				}
				arg = new Arg(expression);
				arg.SetLoc(_globalParent, expression.StartIndex, expression.EndIndex);
			}
			if (!MaybeEat(TokenKind.Comma))
			{
				Eat(TokenKind.RightParenthesis);
				arg.SetLoc(_globalParent, start, GetEnd());
				if (_sink != null)
				{
					_sink.EndParameters(GetSourceSpan());
				}
				return new Arg[1] { arg };
			}
			if (_sink != null)
			{
				_sink.NextParameter(GetSourceSpan());
			}
		}
		return FinishArgumentList(arg);
	}

	private Arg FinishKeywordArgument(Expression t)
	{
		if (!(t is NameExpression nameExpression))
		{
			ReportSyntaxError(Resources.ExpectedName);
			Arg arg = new Arg(null, t);
			arg.SetLoc(_globalParent, t.StartIndex, t.EndIndex);
			return arg;
		}
		Expression expression = ParseExpression();
		Arg arg2 = new Arg(nameExpression.Name, expression);
		arg2.SetLoc(_globalParent, nameExpression.StartIndex, expression.EndIndex);
		return arg2;
	}

	private void CheckUniqueArgument(List<Arg> names, Arg arg)
	{
		if (arg == null || arg.Name == null)
		{
			return;
		}
		for (int i = 0; i < names.Count; i++)
		{
			if (names[i].Name == arg.Name)
			{
				ReportSyntaxError(Resources.DuplicateKeywordArg);
			}
		}
	}

	private Arg[] FinishArgumentList(Arg first)
	{
		List<Arg> list = new List<Arg>();
		if (first != null)
		{
			list.Add(first);
		}
		while (!MaybeEat(TokenKind.RightParenthesis))
		{
			int start = GetStart();
			Arg arg;
			if (MaybeEat(TokenKind.Multiply))
			{
				Expression expression = ParseExpression();
				arg = new Arg("*", expression);
			}
			else if (MaybeEat(TokenKind.Power))
			{
				Expression expression2 = ParseExpression();
				arg = new Arg("**", expression2);
			}
			else
			{
				Expression expression3 = ParseExpression();
				if (MaybeEat(TokenKind.Assign))
				{
					arg = FinishKeywordArgument(expression3);
					CheckUniqueArgument(list, arg);
				}
				else
				{
					arg = new Arg(expression3);
				}
			}
			arg.SetLoc(_globalParent, start, GetEnd());
			list.Add(arg);
			if (MaybeEat(TokenKind.Comma))
			{
				if (_sink != null)
				{
					_sink.NextParameter(GetSourceSpan());
				}
				continue;
			}
			Eat(TokenKind.RightParenthesis);
			break;
		}
		if (_sink != null)
		{
			_sink.EndParameters(GetSourceSpan());
		}
		return list.ToArray();
	}

	private List<Expression> ParseTestList()
	{
		bool trailingComma;
		return ParseExpressionList(out trailingComma);
	}

	private Expression ParseOldExpressionListAsExpr()
	{
		bool trailingComma;
		List<Expression> list = ParseOldExpressionList(out trailingComma);
		if (list.Count == 0 && !trailingComma)
		{
			ReportSyntaxError("invalid syntax");
		}
		return MakeTupleOrExpr(list, trailingComma);
	}

	private List<Expression> ParseOldExpressionList(out bool trailingComma)
	{
		List<Expression> list = new List<Expression>();
		trailingComma = false;
		while (!NeverTestToken(PeekToken()))
		{
			list.Add(ParseOldExpression());
			if (!MaybeEat(TokenKind.Comma))
			{
				trailingComma = false;
				break;
			}
			trailingComma = true;
		}
		return list;
	}

	private List<Expression> ParseTargetList(out bool trailingComma)
	{
		List<Expression> list = new List<Expression>();
		do
		{
			list.Add(ParseTarget());
			if (!MaybeEat(TokenKind.Comma))
			{
				trailingComma = false;
				break;
			}
			trailingComma = true;
		}
		while (!NeverTestToken(PeekToken()));
		return list;
	}

	private Expression ParseTarget()
	{
		Token token = PeekToken();
		switch (token.Kind)
		{
		case TokenKind.LeftParenthesis:
		case TokenKind.LeftBracket:
		{
			Eat(token.Kind);
			bool trailingComma;
			Expression result = MakeTupleOrExpr(ParseTargetList(out trailingComma), trailingComma);
			if (token.Kind == TokenKind.LeftParenthesis)
			{
				Eat(TokenKind.RightParenthesis);
			}
			else
			{
				Eat(TokenKind.RightBracket);
			}
			return result;
		}
		default:
			return AddTrailers(ParsePrimary(), allowGeneratorExpression: false);
		}
	}

	private List<Expression> ParseExpressionList(out bool trailingComma)
	{
		List<Expression> list = new List<Expression>();
		trailingComma = false;
		while (!NeverTestToken(PeekToken()))
		{
			list.Add(ParseExpression());
			if (!MaybeEat(TokenKind.Comma))
			{
				trailingComma = false;
				break;
			}
			trailingComma = true;
		}
		return list;
	}

	private Expression ParseTestListAsExpr()
	{
		if (!NeverTestToken(PeekToken()))
		{
			Expression expression = ParseExpression();
			if (!MaybeEat(TokenKind.Comma))
			{
				return expression;
			}
			return ParseTestListAsExpr(expression);
		}
		return ParseTestListAsExprError();
	}

	private Expression ParseTestListAsExpr(Expression expr)
	{
		List<Expression> list = new List<Expression>();
		list.Add(expr);
		bool trailingComma = true;
		while (!NeverTestToken(PeekToken()))
		{
			list.Add(ParseExpression());
			if (!MaybeEat(TokenKind.Comma))
			{
				trailingComma = false;
				break;
			}
		}
		return MakeTupleOrExpr(list, trailingComma);
	}

	private Expression ParseTestListAsExprError()
	{
		if (MaybeEat(TokenKind.Indent))
		{
			NextToken();
			ReportSyntaxError(GetStart(), GetEnd(), "unexpected indent", 32);
		}
		else
		{
			ReportSyntaxError(_lookahead);
		}
		return new ErrorExpression();
	}

	private Expression FinishExpressionListAsExpr(Expression expr)
	{
		int start = GetStart();
		bool trailingComma = true;
		List<Expression> list = new List<Expression>();
		list.Add(expr);
		while (!NeverTestToken(PeekToken()))
		{
			expr = ParseExpression();
			list.Add(expr);
			if (!MaybeEat(TokenKind.Comma))
			{
				trailingComma = false;
				break;
			}
			trailingComma = true;
		}
		Expression expression = MakeTupleOrExpr(list, trailingComma);
		expression.SetLoc(_globalParent, start, GetEnd());
		return expression;
	}

	private Expression FinishTupleOrGenExp()
	{
		int start = GetStart();
		int end = GetEnd();
		int groupingLevel = _tokenizer.GroupingLevel;
		Expression expression;
		bool flag;
		if (MaybeEat(TokenKind.RightParenthesis))
		{
			expression = MakeTupleOrExpr(new List<Expression>(), trailingComma: false);
			flag = true;
		}
		else if (MaybeEat(TokenKind.KeywordYield))
		{
			expression = ParseYieldExpression();
			Eat(TokenKind.RightParenthesis);
			flag = true;
		}
		else
		{
			bool allowIncomplete = _allowIncomplete;
			try
			{
				_allowIncomplete = true;
				Expression expression2 = ParseExpression();
				expression = (MaybeEat(TokenKind.Comma) ? FinishExpressionListAsExpr(expression2) : ((!PeekToken(Tokens.KeywordForToken)) ? ((expression2 is ParenthesisExpression) ? expression2 : new ParenthesisExpression(expression2)) : ParseGeneratorExpression(expression2)));
				flag = Eat(TokenKind.RightParenthesis);
			}
			finally
			{
				_allowIncomplete = allowIncomplete;
			}
		}
		int start2 = GetStart();
		int end2 = GetEnd();
		if (flag && _sink != null)
		{
			_sink.MatchPair(new SourceSpan(_tokenizer.IndexToLocation(start), _tokenizer.IndexToLocation(end)), new SourceSpan(_tokenizer.IndexToLocation(start2), _tokenizer.IndexToLocation(end2)), groupingLevel);
		}
		expression.SetLoc(_globalParent, start, end2);
		return expression;
	}

	private Expression ParseGeneratorExpression(Expression expr)
	{
		ForStatement forStatement = ParseGenExprFor();
		Statement current = forStatement;
		while (true)
		{
			if (PeekToken(Tokens.KeywordForToken))
			{
				current = NestGenExpr(current, ParseGenExprFor());
				continue;
			}
			if (!PeekToken(Tokens.KeywordIfToken))
			{
				break;
			}
			current = NestGenExpr(current, ParseGenExprIf());
		}
		ExpressionStatement expressionStatement = new ExpressionStatement(new YieldExpression(expr));
		expressionStatement.Expression.SetLoc(_globalParent, expr.IndexSpan);
		expressionStatement.SetLoc(_globalParent, expr.IndexSpan);
		NestGenExpr(current, expressionStatement);
		Parameter parameter = new Parameter("__gen_$_parm__", ParameterKind.Normal);
		FunctionDefinition functionDefinition = new FunctionDefinition("<genexpr>", new Parameter[1] { parameter }, forStatement);
		functionDefinition.IsGenerator = true;
		functionDefinition.SetLoc(_globalParent, forStatement.StartIndex, GetEnd());
		functionDefinition.HeaderIndex = forStatement.EndIndex;
		Expression list = forStatement.List;
		NameExpression nameExpression = new NameExpression("__gen_$_parm__");
		nameExpression.SetLoc(_globalParent, list.IndexSpan);
		forStatement.List = nameExpression;
		GeneratorExpression generatorExpression = new GeneratorExpression(functionDefinition, list);
		generatorExpression.SetLoc(_globalParent, expr.StartIndex, GetEnd());
		return generatorExpression;
	}

	private static Statement NestGenExpr(Statement current, Statement nested)
	{
		if (current is ForStatement forStatement)
		{
			forStatement.Body = nested;
		}
		else if (current is IfStatement ifStatement)
		{
			ifStatement.Tests[0].Body = nested;
		}
		return nested;
	}

	private ForStatement ParseGenExprFor()
	{
		int start = GetStart();
		Eat(TokenKind.KeywordFor);
		bool trailingComma;
		List<Expression> l = ParseTargetList(out trailingComma);
		Expression left = MakeTupleOrExpr(l, trailingComma);
		Eat(TokenKind.KeywordIn);
		Expression expression = null;
		expression = ParseOrTest();
		ForStatement forStatement = new ForStatement(left, expression, null, null);
		int end = GetEnd();
		forStatement.SetLoc(_globalParent, start, end);
		forStatement.HeaderIndex = end;
		return forStatement;
	}

	private IfStatement ParseGenExprIf()
	{
		int start = GetStart();
		Eat(TokenKind.KeywordIf);
		Expression test = ParseOldExpression();
		IfStatementTest ifStatementTest = new IfStatementTest(test, null);
		int end = (ifStatementTest.HeaderIndex = GetEnd());
		ifStatementTest.SetLoc(_globalParent, start, end);
		IfStatement ifStatement = new IfStatement(new IfStatementTest[1] { ifStatementTest }, null);
		ifStatement.SetLoc(_globalParent, start, end);
		return ifStatement;
	}

	private Expression FinishDictOrSetValue()
	{
		int start = GetStart();
		int end = GetEnd();
		List<SliceExpression> list = null;
		List<Expression> list2 = null;
		bool allowIncomplete = _allowIncomplete;
		try
		{
			_allowIncomplete = true;
			while (!MaybeEat(TokenKind.RightBrace))
			{
				bool flag = false;
				Expression expression = ParseExpression();
				if (MaybeEat(TokenKind.Colon))
				{
					if (list2 != null)
					{
						ReportSyntaxError("invalid syntax");
					}
					else if (list == null)
					{
						list = new List<SliceExpression>();
						flag = true;
					}
					Expression expression2 = ParseExpression();
					if (PeekToken(Tokens.KeywordForToken))
					{
						if (!flag)
						{
							ReportSyntaxError("invalid syntax");
						}
						return FinishDictComp(expression, expression2, start, end);
					}
					SliceExpression sliceExpression = new SliceExpression(expression, expression2, null, stepProvided: false);
					sliceExpression.SetLoc(_globalParent, expression.StartIndex, expression2.EndIndex);
					list.Add(sliceExpression);
				}
				else
				{
					if (list != null)
					{
						ReportSyntaxError("invalid syntax");
					}
					else if (list2 == null)
					{
						list2 = new List<Expression>();
						flag = true;
					}
					if (PeekToken(Tokens.KeywordForToken))
					{
						if (!flag)
						{
							ReportSyntaxError("invalid syntax");
						}
						return FinishSetComp(expression, start, end);
					}
					list2?.Add(expression);
				}
				if (!MaybeEat(TokenKind.Comma))
				{
					Eat(TokenKind.RightBrace);
					break;
				}
			}
		}
		finally
		{
			_allowIncomplete = allowIncomplete;
		}
		int start2 = GetStart();
		int end2 = GetEnd();
		if (_sink != null)
		{
			_sink.MatchPair(new SourceSpan(_tokenizer.IndexToLocation(start), _tokenizer.IndexToLocation(end)), new SourceSpan(_tokenizer.IndexToLocation(start2), _tokenizer.IndexToLocation(end2)), 1);
		}
		if (list != null || list2 == null)
		{
			SliceExpression[] items = ((list == null) ? new SliceExpression[0] : list.ToArray());
			DictionaryExpression dictionaryExpression = new DictionaryExpression(items);
			dictionaryExpression.SetLoc(_globalParent, start, end2);
			return dictionaryExpression;
		}
		SetExpression setExpression = new SetExpression(list2.ToArray());
		setExpression.SetLoc(_globalParent, start, end2);
		return setExpression;
	}

	private SetComprehension FinishSetComp(Expression item, int oStart, int oEnd)
	{
		ComprehensionIterator[] iterators = ParseCompIter();
		Eat(TokenKind.RightBrace);
		int start = GetStart();
		int end = GetEnd();
		if (_sink != null)
		{
			_sink.MatchPair(new SourceSpan(_tokenizer.IndexToLocation(oStart), _tokenizer.IndexToLocation(oEnd)), new SourceSpan(_tokenizer.IndexToLocation(start), _tokenizer.IndexToLocation(end)), 1);
		}
		SetComprehension setComprehension = new SetComprehension(item, iterators);
		setComprehension.SetLoc(_globalParent, oStart, end);
		return setComprehension;
	}

	private DictionaryComprehension FinishDictComp(Expression key, Expression value, int oStart, int oEnd)
	{
		ComprehensionIterator[] iterators = ParseCompIter();
		Eat(TokenKind.RightBrace);
		int start = GetStart();
		int end = GetEnd();
		if (_sink != null)
		{
			_sink.MatchPair(new SourceSpan(_tokenizer.IndexToLocation(oStart), _tokenizer.IndexToLocation(oEnd)), new SourceSpan(_tokenizer.IndexToLocation(start), _tokenizer.IndexToLocation(end)), 1);
		}
		DictionaryComprehension dictionaryComprehension = new DictionaryComprehension(key, value, iterators);
		dictionaryComprehension.SetLoc(_globalParent, oStart, end);
		return dictionaryComprehension;
	}

	private ComprehensionIterator[] ParseCompIter()
	{
		List<ComprehensionIterator> list = new List<ComprehensionIterator>();
		ComprehensionFor item = ParseCompFor();
		list.Add(item);
		while (true)
		{
			if (PeekToken(Tokens.KeywordForToken))
			{
				list.Add(ParseCompFor());
				continue;
			}
			if (!PeekToken(Tokens.KeywordIfToken))
			{
				break;
			}
			list.Add(ParseCompIf());
		}
		return list.ToArray();
	}

	private ComprehensionFor ParseCompFor()
	{
		Eat(TokenKind.KeywordFor);
		int start = GetStart();
		bool trailingComma;
		List<Expression> l = ParseTargetList(out trailingComma);
		Expression lhs = MakeTupleOrExpr(l, trailingComma);
		Eat(TokenKind.KeywordIn);
		Expression list = ParseOrTest();
		ComprehensionFor comprehensionFor = new ComprehensionFor(lhs, list);
		comprehensionFor.SetLoc(_globalParent, start, GetEnd());
		return comprehensionFor;
	}

	private Expression FinishListValue()
	{
		int start = GetStart();
		int end = GetEnd();
		int groupingLevel = _tokenizer.GroupingLevel;
		Expression expression;
		if (MaybeEat(TokenKind.RightBracket))
		{
			expression = new ListExpression();
		}
		else
		{
			bool allowIncomplete = _allowIncomplete;
			try
			{
				_allowIncomplete = true;
				Expression expression2 = ParseExpression();
				if (MaybeEat(TokenKind.Comma))
				{
					List<Expression> list = ParseTestList();
					Eat(TokenKind.RightBracket);
					list.Insert(0, expression2);
					expression = new ListExpression(list.ToArray());
				}
				else if (PeekToken(Tokens.KeywordForToken))
				{
					expression = FinishListComp(expression2);
				}
				else
				{
					Eat(TokenKind.RightBracket);
					expression = new ListExpression(expression2);
				}
			}
			finally
			{
				_allowIncomplete = allowIncomplete;
			}
		}
		int start2 = GetStart();
		int end2 = GetEnd();
		if (_sink != null)
		{
			_sink.MatchPair(new SourceSpan(_tokenizer.IndexToLocation(start), _tokenizer.IndexToLocation(end)), new SourceSpan(_tokenizer.IndexToLocation(start2), _tokenizer.IndexToLocation(end2)), groupingLevel);
		}
		expression.SetLoc(_globalParent, start, end2);
		return expression;
	}

	private ListComprehension FinishListComp(Expression item)
	{
		ComprehensionIterator[] iterators = ParseListCompIter();
		Eat(TokenKind.RightBracket);
		return new ListComprehension(item, iterators);
	}

	private ComprehensionIterator[] ParseListCompIter()
	{
		List<ComprehensionIterator> list = new List<ComprehensionIterator>();
		ComprehensionFor item = ParseListCompFor();
		list.Add(item);
		while (true)
		{
			if (PeekToken(Tokens.KeywordForToken))
			{
				list.Add(ParseListCompFor());
				continue;
			}
			if (!PeekToken(Tokens.KeywordIfToken))
			{
				break;
			}
			list.Add(ParseCompIf());
		}
		return list.ToArray();
	}

	private ComprehensionFor ParseListCompFor()
	{
		Eat(TokenKind.KeywordFor);
		int start = GetStart();
		bool trailingComma;
		List<Expression> l = ParseTargetList(out trailingComma);
		Expression lhs = MakeTupleOrExpr(l, trailingComma);
		Eat(TokenKind.KeywordIn);
		Expression list = ParseOldExpressionListAsExpr();
		ComprehensionFor comprehensionFor = new ComprehensionFor(lhs, list);
		comprehensionFor.SetLoc(_globalParent, start, GetEnd());
		return comprehensionFor;
	}

	private ComprehensionIf ParseCompIf()
	{
		Eat(TokenKind.KeywordIf);
		int start = GetStart();
		Expression test = ParseOldExpression();
		ComprehensionIf comprehensionIf = new ComprehensionIf(test);
		comprehensionIf.SetLoc(_globalParent, start, GetEnd());
		return comprehensionIf;
	}

	private Expression FinishStringConversion()
	{
		int start = GetStart();
		Expression expression = ParseTestListAsExpr();
		Eat(TokenKind.BackQuote);
		Expression expression2 = new BackQuoteExpression(expression);
		expression2.SetLoc(_globalParent, start, GetEnd());
		return expression2;
	}

	private Expression MakeTupleOrExpr(List<Expression> l, bool trailingComma)
	{
		return MakeTupleOrExpr(l, trailingComma, expandable: false);
	}

	private Expression MakeTupleOrExpr(List<Expression> l, bool trailingComma, bool expandable)
	{
		if (l.Count == 1 && !trailingComma)
		{
			return l[0];
		}
		Expression[] array = l.ToArray();
		TupleExpression tupleExpression = new TupleExpression(expandable && !trailingComma, array);
		if (array.Length > 0)
		{
			tupleExpression.SetLoc(_globalParent, array[0].StartIndex, array[array.Length - 1].EndIndex);
		}
		return tupleExpression;
	}

	private static bool NeverTestToken(Token t)
	{
		switch (t.Kind)
		{
		case TokenKind.EndOfFile:
		case TokenKind.NewLine:
		case TokenKind.Indent:
		case TokenKind.Dedent:
		case TokenKind.AddEqual:
		case TokenKind.SubtractEqual:
		case TokenKind.PowerEqual:
		case TokenKind.MultiplyEqual:
		case TokenKind.FloorDivideEqual:
		case TokenKind.DivideEqual:
		case TokenKind.ModEqual:
		case TokenKind.LeftShiftEqual:
		case TokenKind.RightShiftEqual:
		case TokenKind.BitwiseAndEqual:
		case TokenKind.BitwiseOrEqual:
		case TokenKind.ExclusiveOrEqual:
		case TokenKind.RightParenthesis:
		case TokenKind.RightBracket:
		case TokenKind.RightBrace:
		case TokenKind.Comma:
		case TokenKind.Semicolon:
		case TokenKind.Assign:
		case TokenKind.KeywordFor:
		case TokenKind.KeywordIf:
		case TokenKind.KeywordIn:
			return true;
		default:
			return false;
		}
	}

	private FunctionDefinition PopFunction()
	{
		if (_functions != null && _functions.Count > 0)
		{
			return _functions.Pop();
		}
		return null;
	}

	private void PushFunction(FunctionDefinition function)
	{
		if (_functions == null)
		{
			_functions = new Stack<FunctionDefinition>();
		}
		_functions.Push(function);
	}

	private CallExpression FinishCallExpr(Expression target, params Arg[] args)
	{
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		foreach (Arg arg in args)
		{
			if (arg.Name == null)
			{
				if (flag || flag2 || num > 0)
				{
					ReportSyntaxError(Resources.NonKeywordAfterKeywordArg);
				}
			}
			else if (arg.Name == "*")
			{
				if (flag || flag2)
				{
					ReportSyntaxError(Resources.OneListArgOnly);
				}
				flag = true;
				num2++;
			}
			else if (arg.Name == "**")
			{
				if (flag2)
				{
					ReportSyntaxError(Resources.OneKeywordArgOnly);
				}
				flag2 = true;
				num2++;
			}
			else
			{
				if (flag2)
				{
					ReportSyntaxError(Resources.KeywordOutOfSequence);
				}
				num++;
			}
		}
		return new CallExpression(target, args);
	}

	public void Dispose()
	{
		if (_sourceReader != null)
		{
			_sourceReader.Dispose();
		}
	}

	private PythonAst ParseFileWorker(bool makeModule, bool returnValue)
	{
		_globalParent = new PythonAst(makeModule, _languageFeatures, printExpressions: false, _context);
		StartParsing();
		List<Statement> list = new List<Statement>();
		MaybeEatNewLine();
		if (PeekToken(TokenKind.Constant))
		{
			Statement statement = ParseStmt();
			list.Add(statement);
			_fromFutureAllowed = false;
			if (statement is ExpressionStatement { Expression: ConstantExpression expression } && expression.Value is string)
			{
				_fromFutureAllowed = true;
			}
		}
		MaybeEatNewLine();
		if (_fromFutureAllowed)
		{
			while (PeekToken(Tokens.KeywordFromToken))
			{
				Statement statement2 = ParseStmt();
				list.Add(statement2);
				if (statement2 is FromImportStatement { IsFromFuture: false })
				{
					break;
				}
			}
		}
		_fromFutureAllowed = false;
		while (!MaybeEat(TokenKind.EndOfFile))
		{
			if (!MaybeEatNewLine())
			{
				Statement item = ParseStmt();
				list.Add(item);
			}
		}
		Statement[] array = list.ToArray();
		if (returnValue && array.Length > 0 && array[array.Length - 1] is ExpressionStatement expressionStatement2)
		{
			ReturnStatement returnStatement = new ReturnStatement(expressionStatement2.Expression);
			array[array.Length - 1] = returnStatement;
			returnStatement.SetLoc(_globalParent, expressionStatement2.Expression.IndexSpan);
		}
		SuiteStatement suiteStatement = new SuiteStatement(array);
		suiteStatement.SetLoc(_globalParent, 0, GetEnd());
		return FinishParsing(suiteStatement);
	}

	private Statement InternalParseInteractiveInput(out bool parsingMultiLineCmpdStmt, out bool isEmptyStmt)
	{
		try
		{
			isEmptyStmt = false;
			parsingMultiLineCmpdStmt = false;
			Statement result;
			switch (PeekToken().Kind)
			{
			case TokenKind.NewLine:
				MaybeEatNewLine();
				Eat(TokenKind.EndOfFile);
				if (_tokenizer.EndContinues)
				{
					parsingMultiLineCmpdStmt = true;
					_errorCode = 1;
				}
				else
				{
					isEmptyStmt = true;
				}
				return null;
			case TokenKind.At:
			case TokenKind.KeywordClass:
			case TokenKind.KeywordDef:
			case TokenKind.KeywordFor:
			case TokenKind.KeywordIf:
			case TokenKind.KeywordTry:
			case TokenKind.KeywordWhile:
			case TokenKind.LastKeyword:
				parsingMultiLineCmpdStmt = true;
				result = ParseStmt();
				EatEndOfInput();
				break;
			default:
				result = ParseSimpleStmt();
				MaybeEatNewLine();
				Eat(TokenKind.EndOfFile);
				break;
			}
			return result;
		}
		catch (BadSourceException bse)
		{
			throw BadSourceError(bse);
		}
	}

	private Expression ParseTestListAsExpression()
	{
		StartParsing();
		Expression result = ParseTestListAsExpr();
		EatEndOfInput();
		return result;
	}

	private bool MaybeEatNewLine()
	{
		if (MaybeEat(TokenKind.NewLine))
		{
			while (MaybeEat(TokenKind.NLToken))
			{
			}
			return true;
		}
		return false;
	}

	private bool EatNewLine()
	{
		bool result = Eat(TokenKind.NewLine);
		while (MaybeEat(TokenKind.NLToken))
		{
		}
		return result;
	}

	private Token EatEndOfInput()
	{
		while (MaybeEatNewLine() || MaybeEat(TokenKind.Dedent))
		{
		}
		Token token = NextToken();
		if (token.Kind != TokenKind.EndOfFile)
		{
			ReportSyntaxError(_token);
		}
		return token;
	}

	private Exception BadSourceError(BadSourceException bse)
	{
		if (_sourceReader.BaseReader is StreamReader streamReader && streamReader.BaseStream.CanSeek)
		{
			return PythonContext.ReportEncodingError(streamReader.BaseStream, _sourceUnit.Path);
		}
		return PythonOps.BadSourceError(bse._badByte, new SourceSpan(_tokenizer.CurrentPosition, _tokenizer.CurrentPosition), _sourceUnit.Path);
	}

	private void StartParsing()
	{
		if (_parsingStarted)
		{
			throw new InvalidOperationException("Parsing already started. Use Restart to start again.");
		}
		_parsingStarted = true;
		FetchLookahead();
	}

	private int GetEnd()
	{
		return _token.Span.End;
	}

	private int GetStart()
	{
		return _token.Span.Start;
	}

	private SourceSpan GetSourceSpan()
	{
		return new SourceSpan(_tokenizer.IndexToLocation(GetStart()), _tokenizer.IndexToLocation(GetEnd()));
	}

	private Token NextToken()
	{
		_token = _lookahead;
		FetchLookahead();
		return _token.Token;
	}

	private Token PeekToken()
	{
		return _lookahead.Token;
	}

	private void FetchLookahead()
	{
		_lookahead = new TokenWithSpan(_tokenizer.GetNextToken(), _tokenizer.TokenSpan);
	}

	private bool PeekToken(TokenKind kind)
	{
		return PeekToken().Kind == kind;
	}

	private bool PeekToken(Token check)
	{
		return PeekToken() == check;
	}

	private bool Eat(TokenKind kind)
	{
		Token token = PeekToken();
		if (token.Kind != kind)
		{
			ReportSyntaxError(_lookahead);
			return false;
		}
		NextToken();
		return true;
	}

	private bool EatNoEof(TokenKind kind)
	{
		Token token = PeekToken();
		if (token.Kind != kind)
		{
			ReportSyntaxError(_lookahead.Token, _lookahead.Span, 16, allowIncomplete: false);
			return false;
		}
		NextToken();
		return true;
	}

	private bool MaybeEat(TokenKind kind)
	{
		if (PeekToken().Kind == kind)
		{
			NextToken();
			return true;
		}
		return false;
	}
}
