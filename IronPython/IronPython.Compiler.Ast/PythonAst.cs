using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public sealed class PythonAst : ScopeStatement
{
	internal class RewrittenBodyStatement : Statement
	{
		private readonly System.Linq.Expressions.Expression _body;

		private readonly string _doc;

		private readonly Statement _originalBody;

		public override string Documentation => _doc;

		public RewrittenBodyStatement(Statement originalBody, System.Linq.Expressions.Expression body)
		{
			_body = body;
			_doc = originalBody.Documentation;
			_originalBody = originalBody;
			SetLoc(originalBody.GlobalParent, originalBody.IndexSpan);
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			return _body;
		}

		public override void Walk(PythonWalker walker)
		{
			_originalBody.Walk(walker);
		}
	}

	internal class LookupVisitor : ExpressionVisitor
	{
		private readonly System.Linq.Expressions.Expression _globalContext;

		private ScopeStatement _curScope;

		public LookupVisitor(PythonAst ast, System.Linq.Expressions.Expression globalContext)
		{
			_globalContext = globalContext;
			_curScope = ast;
		}

		protected override System.Linq.Expressions.Expression VisitMember(System.Linq.Expressions.MemberExpression node)
		{
			if (node == _globalContext)
			{
				return PythonAst._globalContext;
			}
			return base.VisitMember(node);
		}

		protected override System.Linq.Expressions.Expression VisitExtension(System.Linq.Expressions.Expression node)
		{
			if (node == _globalContext)
			{
				return PythonAst._globalContext;
			}
			if (node is ScopeStatement scope)
			{
				return base.VisitExtension(VisitScope(scope));
			}
			if (node is LambdaExpression lambdaExpression)
			{
				return base.VisitExtension(new LambdaExpression((FunctionDefinition)VisitScope(lambdaExpression.Function)));
			}
			if (node is GeneratorExpression generatorExpression)
			{
				return base.VisitExtension(new GeneratorExpression((FunctionDefinition)VisitScope(generatorExpression.Function), generatorExpression.Iterable));
			}
			if (node is PythonGlobalVariableExpression pythonGlobalVariableExpression)
			{
				return new LookupGlobalVariable((_curScope == null) ? PythonAst._globalContext : _curScope.LocalContext, pythonGlobalVariableExpression.Variable.Name, pythonGlobalVariableExpression.Variable.Kind == VariableKind.Local);
			}
			if (node is PythonSetGlobalVariableExpression pythonSetGlobalVariableExpression)
			{
				if (pythonSetGlobalVariableExpression.Value == PythonGlobalVariableExpression.Uninitialized)
				{
					return new LookupGlobalVariable((_curScope == null) ? PythonAst._globalContext : _curScope.LocalContext, pythonSetGlobalVariableExpression.Global.Variable.Name, pythonSetGlobalVariableExpression.Global.Variable.Kind == VariableKind.Local).Delete();
				}
				return new LookupGlobalVariable((_curScope == null) ? PythonAst._globalContext : _curScope.LocalContext, pythonSetGlobalVariableExpression.Global.Variable.Name, pythonSetGlobalVariableExpression.Global.Variable.Kind == VariableKind.Local).Assign(Visit(pythonSetGlobalVariableExpression.Value));
			}
			if (node is PythonRawGlobalValueExpression pythonRawGlobalValueExpression)
			{
				return new LookupGlobalVariable((_curScope == null) ? PythonAst._globalContext : _curScope.LocalContext, pythonRawGlobalValueExpression.Global.Variable.Name, pythonRawGlobalValueExpression.Global.Variable.Kind == VariableKind.Local);
			}
			return base.VisitExtension(node);
		}

		private ScopeStatement VisitScope(ScopeStatement scope)
		{
			ScopeStatement scopeStatement = scope.CopyForRewrite();
			ScopeStatement curScope = _curScope;
			try
			{
				_curScope = scopeStatement;
				scopeStatement.Parent = curScope;
				scopeStatement.RewriteBody(this);
				return scopeStatement;
			}
			finally
			{
				_curScope = curScope;
			}
		}
	}

	internal const string GlobalContextName = "$globalContext";

	private Statement _body;

	private CompilationMode _mode;

	private readonly bool _isModule;

	private readonly bool _printExpressions;

	private ModuleOptions _languageFeatures;

	private readonly CompilerContext _compilerContext;

	private readonly SymbolDocumentInfo _document;

	private readonly string _name;

	internal int[] _lineLocations;

	private PythonVariable _docVariable;

	private PythonVariable _nameVariable;

	private PythonVariable _fileVariable;

	private ModuleContext _modContext;

	private readonly bool _onDiskProxy;

	internal System.Linq.Expressions.Expression _arrayExpression;

	private CompilationMode.ConstantInfo _contextInfo;

	private Dictionary<PythonVariable, System.Linq.Expressions.Expression> _globalVariables = new Dictionary<PythonVariable, System.Linq.Expressions.Expression>();

	internal readonly Profiler _profiler;

	internal static ParameterExpression _functionCode = System.Linq.Expressions.Expression.Variable(typeof(FunctionCode), "$functionCode");

	internal static readonly ParameterExpression _globalArray = System.Linq.Expressions.Expression.Parameter(typeof(PythonGlobal[]), "$globalArray");

	internal static readonly ParameterExpression _globalContext = System.Linq.Expressions.Expression.Parameter(typeof(CodeContext), "$globalContext");

	internal static readonly ReadOnlyCollection<ParameterExpression> _arrayFuncParams = new ReadOnlyCollectionBuilder<ParameterExpression>(new ParameterExpression[2] { _globalContext, _functionCode }).ToReadOnlyCollection();

	public override string Name => "<module>";

	internal override System.Linq.Expressions.Expression LocalContext => GetGlobalContext();

	internal override bool IsGlobal => true;

	internal PythonVariable DocVariable
	{
		get
		{
			return _docVariable;
		}
		set
		{
			_docVariable = value;
		}
	}

	internal PythonVariable NameVariable
	{
		get
		{
			return _nameVariable;
		}
		set
		{
			_nameVariable = value;
		}
	}

	internal PythonVariable FileVariable
	{
		get
		{
			return _fileVariable;
		}
		set
		{
			_fileVariable = value;
		}
	}

	internal CompilerContext CompilerContext => _compilerContext;

	internal System.Linq.Expressions.Expression GlobalArrayInstance => _arrayExpression;

	internal SymbolDocumentInfo Document => _document;

	internal Dictionary<PythonVariable, System.Linq.Expressions.Expression> ModuleVariables => _globalVariables;

	internal ModuleContext ModuleContext => _modContext;

	public override Type Type => CompilationMode.DelegateType;

	public bool TrueDivision => (_languageFeatures & ModuleOptions.TrueDivision) != 0;

	public bool AllowWithStatement => (_languageFeatures & ModuleOptions.WithStatement) != 0;

	public bool AbsoluteImports => (_languageFeatures & ModuleOptions.AbsoluteImports) != 0;

	internal PythonDivisionOptions DivisionOptions => base.PyContext.PythonOptions.DivisionOptions;

	public Statement Body => _body;

	public bool Module => _isModule;

	internal override bool PrintExpressions => _printExpressions;

	private string ModuleFileName => _name;

	private string ModuleName
	{
		get
		{
			PythonCompilerOptions pythonCompilerOptions = _compilerContext.Options as PythonCompilerOptions;
			string text = pythonCompilerOptions.ModuleName;
			if (text == null)
			{
				text = ((!_compilerContext.SourceUnit.HasPath || _compilerContext.SourceUnit.Path.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) ? "<module>" : Path.GetFileNameWithoutExtension(_compilerContext.SourceUnit.Path));
			}
			return text;
		}
	}

	internal override FunctionAttributes Flags
	{
		get
		{
			ModuleOptions module = ((PythonCompilerOptions)_compilerContext.Options).Module;
			FunctionAttributes functionAttributes = FunctionAttributes.None;
			if ((module & ModuleOptions.TrueDivision) != ModuleOptions.None)
			{
				functionAttributes |= FunctionAttributes.FutureDivision;
			}
			return functionAttributes;
		}
	}

	internal SourceUnit SourceUnit
	{
		get
		{
			if (_compilerContext == null)
			{
				return null;
			}
			return _compilerContext.SourceUnit;
		}
	}

	internal CompilationMode CompilationMode => _mode;

	internal override string ProfilerName
	{
		get
		{
			if (_mode == CompilationMode.Lookup)
			{
				return "module: <exec>";
			}
			if (_name.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
			{
				return "module " + _name;
			}
			return "module " + Path.GetFileNameWithoutExtension(_name);
		}
	}

	internal new bool EmitDebugSymbols => base.PyContext.EmitDebugSymbols(SourceUnit);

	internal bool OnDiskProxy => _onDiskProxy;

	public PythonAst(Statement body, bool isModule, ModuleOptions languageFeatures, bool printExpressions)
	{
		ContractUtils.RequiresNotNull(body, "body");
		_body = body;
		_isModule = isModule;
		_printExpressions = printExpressions;
		_languageFeatures = languageFeatures;
	}

	public PythonAst(Statement body, bool isModule, ModuleOptions languageFeatures, bool printExpressions, CompilerContext context, int[] lineLocations)
		: this(isModule, languageFeatures, printExpressions, context)
	{
		ContractUtils.RequiresNotNull(body, "body");
		_body = body;
		_lineLocations = lineLocations;
	}

	public PythonAst(bool isModule, ModuleOptions languageFeatures, bool printExpressions, CompilerContext context)
	{
		_isModule = isModule;
		_printExpressions = printExpressions;
		_languageFeatures = languageFeatures;
		_mode = ((PythonCompilerOptions)context.Options).CompilationMode ?? GetCompilationMode(context);
		_compilerContext = context;
		base.FuncCodeExpr = _functionCode;
		PythonCompilerOptions pythonCompilerOptions = context.Options as PythonCompilerOptions;
		string fileName = (_name = ((context.SourceUnit.HasPath && (pythonCompilerOptions.Module & ModuleOptions.ExecOrEvalCode) == 0) ? context.SourceUnit.Path : "<module>"));
		PythonOptions pythonOptions = ((PythonContext)context.SourceUnit.LanguageContext).PythonOptions;
		if (pythonOptions.EnableProfiler && _mode != CompilationMode.ToDisk)
		{
			_profiler = Profiler.GetProfiler(base.PyContext);
		}
		_document = context.SourceUnit.Document ?? System.Linq.Expressions.Expression.SymbolDocument(fileName, base.PyContext.LanguageGuid, base.PyContext.VendorGuid);
	}

	internal PythonAst(CompilerContext context)
		: this(new EmptyStatement(), isModule: true, ModuleOptions.None, printExpressions: false, context, null)
	{
		_onDiskProxy = true;
	}

	public void ParsingFinished(int[] lineLocations, Statement body, ModuleOptions languageFeatures)
	{
		ContractUtils.RequiresNotNull(body, "body");
		if (_body != null)
		{
			throw new InvalidOperationException("cannot set body twice");
		}
		_body = body;
		_lineLocations = lineLocations;
		_languageFeatures = languageFeatures;
	}

	public void Bind()
	{
		PythonNameBinder.BindAst(this, _compilerContext);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _body != null)
		{
			_body.Walk(walker);
		}
		walker.PostWalk(this);
	}

	internal override bool ExposesLocalVariable(PythonVariable variable)
	{
		return true;
	}

	internal override void FinishBind(PythonNameBinder binder)
	{
		_contextInfo = CompilationMode.GetContext();
		PythonGlobal[] array = new PythonGlobal[(base.Variables != null) ? base.Variables.Count : 0];
		Dictionary<string, PythonGlobal> dictionary = new Dictionary<string, PythonGlobal>();
		GlobalDictionaryStorage storage = new GlobalDictionaryStorage(dictionary, array);
		ModuleContext moduleContext = (_modContext = new ModuleContext(new PythonDictionary(storage), base.PyContext));
		if (_mode == CompilationMode.ToDisk)
		{
			_arrayExpression = _globalArray;
		}
		else
		{
			ConstantExpression constantExpression = new ConstantExpression(array);
			constantExpression.Parent = this;
			_arrayExpression = constantExpression;
		}
		if (base.Variables != null)
		{
			int num = 0;
			foreach (PythonVariable value in base.Variables.Values)
			{
				PythonGlobal pythonGlobal = new PythonGlobal(moduleContext.GlobalContext, value.Name);
				_globalVariables[value] = CompilationMode.GetGlobal(GetGlobalContext(), dictionary.Count, value, pythonGlobal);
				int num2 = num++;
				PythonGlobal pythonGlobal2 = (dictionary[value.Name] = pythonGlobal);
				array[num2] = pythonGlobal2;
			}
		}
		CompilationMode.PublishContext(moduleContext.GlobalContext, _contextInfo);
	}

	internal override PythonVariable BindReference(PythonNameBinder binder, PythonReference reference)
	{
		return EnsureVariable(reference.Name);
	}

	internal override bool TryBindOuter(ScopeStatement from, PythonReference reference, out PythonVariable variable)
	{
		from.AddReferencedGlobal(reference.Name);
		if (from.HasLateBoundVariableSets)
		{
			variable = null;
			return false;
		}
		variable = EnsureGlobalVariable(reference.Name);
		return true;
	}

	internal PythonVariable EnsureGlobalVariable(string name)
	{
		if (!TryGetVariable(name, out var variable))
		{
			return CreateVariable(name, VariableKind.Global);
		}
		return variable;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return GetLambda();
	}

	internal override LightLambdaExpression GetLambda()
	{
		string name = ((PythonCompilerOptions)_compilerContext.Options).ModuleName ?? "<unnamed>";
		return CompilationMode.ReduceAst(this, name);
	}

	internal ScriptCode ToScriptCode()
	{
		return CompilationMode.MakeScriptCode(this);
	}

	internal System.Linq.Expressions.Expression ReduceWorker()
	{
		if (_body is ReturnStatement returnStatement && (_languageFeatures == ModuleOptions.None || _languageFeatures == (ModuleOptions.ExecOrEvalCode | ModuleOptions.Interpret) || _languageFeatures == (ModuleOptions.ExecOrEvalCode | ModuleOptions.Interpret | ModuleOptions.LightThrow)))
		{
			ReturnStatement returnStatement2 = (ReturnStatement)_body;
			System.Linq.Expressions.Expression expression = (((_languageFeatures & ModuleOptions.LightThrow) == 0) ? returnStatement.Expression.Reduce() : LightExceptions.Rewrite(returnStatement.Expression.Reduce()));
			SourceLocation sourceLocation = IndexToLocation(returnStatement2.Expression.StartIndex);
			SourceLocation sourceLocation2 = IndexToLocation(returnStatement2.Expression.EndIndex);
			return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.DebugInfo(_document, sourceLocation.Line, sourceLocation.Column, sourceLocation2.Line, sourceLocation2.Column), Utils.Convert(expression, typeof(object)));
		}
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
		AddInitialiation(readOnlyCollectionBuilder);
		if (_isModule)
		{
			readOnlyCollectionBuilder.Add(Node.AssignValue(GetVariableExpression(_docVariable), System.Linq.Expressions.Expression.Constant(GetDocumentation(_body))));
		}
		if (!(_body is SuiteStatement) && _body.CanThrow)
		{
			readOnlyCollectionBuilder.Add(Node.UpdateLineNumber(_body.Start.Line));
		}
		readOnlyCollectionBuilder.Add(_body);
		System.Linq.Expressions.Expression body = System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder.ToReadOnlyCollection());
		body = WrapScopeStatements(body, Body.CanThrow);
		body = AddModulePublishing(body);
		body = AddProfiling(body);
		if ((((PythonCompilerOptions)_compilerContext.Options).Module & ModuleOptions.LightThrow) != ModuleOptions.None)
		{
			body = LightExceptions.Rewrite(body);
		}
		body = System.Linq.Expressions.Expression.Label(FunctionDefinition._returnLabel, Utils.Convert(body, typeof(object)));
		if (body.Type == typeof(void))
		{
			body = System.Linq.Expressions.Expression.Block(body, System.Linq.Expressions.Expression.Constant(null));
		}
		return body;
	}

	private void AddInitialiation(ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> block)
	{
		if (_isModule)
		{
			block.Add(Node.AssignValue(GetVariableExpression(_fileVariable), System.Linq.Expressions.Expression.Constant(ModuleFileName)));
			block.Add(Node.AssignValue(GetVariableExpression(_nameVariable), System.Linq.Expressions.Expression.Constant(ModuleName)));
		}
		if (_languageFeatures != ModuleOptions.None || _isModule)
		{
			block.Add(System.Linq.Expressions.Expression.Call(AstMethods.ModuleStarted, LocalContext, Utils.Constant(_languageFeatures)));
		}
	}

	private System.Linq.Expressions.Expression AddModulePublishing(System.Linq.Expressions.Expression body)
	{
		if (_isModule)
		{
			PythonCompilerOptions pythonCompilerOptions = _compilerContext.Options as PythonCompilerOptions;
			string moduleName = ModuleName;
			if ((pythonCompilerOptions.Module & ModuleOptions.Initialize) != ModuleOptions.None)
			{
				ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(object), "$originalModule");
				body = System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, Utils.Try(System.Linq.Expressions.Expression.Assign(parameterExpression, System.Linq.Expressions.Expression.Call(AstMethods.PublishModule, LocalContext, System.Linq.Expressions.Expression.Constant(moduleName))), body).Catch(typeof(Exception), System.Linq.Expressions.Expression.Call(AstMethods.RemoveModule, LocalContext, System.Linq.Expressions.Expression.Constant(moduleName), parameterExpression), System.Linq.Expressions.Expression.Rethrow(body.Type)));
			}
		}
		return body;
	}

	internal string[] GetNames()
	{
		string[] array = new string[base.Variables.Count];
		int num = 0;
		foreach (PythonVariable value in base.Variables.Values)
		{
			array[num++] = value.Name;
		}
		return array;
	}

	private static CompilationMode GetCompilationMode(CompilerContext context)
	{
		PythonCompilerOptions pythonCompilerOptions = (PythonCompilerOptions)context.Options;
		if ((pythonCompilerOptions.Module & ModuleOptions.ExecOrEvalCode) != ModuleOptions.None)
		{
			return CompilationMode.Lookup;
		}
		PythonContext pythonContext = (PythonContext)context.SourceUnit.LanguageContext;
		if ((!pythonContext.PythonOptions.Optimize && !pythonCompilerOptions.Optimized) || pythonContext.PythonOptions.LightweightScopes)
		{
			return CompilationMode.Collectable;
		}
		return CompilationMode.Uncollectable;
	}

	private System.Linq.Expressions.Expression GetGlobalContext()
	{
		if (_contextInfo != null)
		{
			return _contextInfo.Expression;
		}
		return _globalContext;
	}

	internal void PrepareScope(ReadOnlyCollectionBuilder<ParameterExpression> locals, List<System.Linq.Expressions.Expression> init)
	{
		CompilationMode.PrepareScope(this, locals, init);
	}

	internal new System.Linq.Expressions.Expression Constant(object value)
	{
		return new PythonConstantExpression(CompilationMode, value);
	}

	internal System.Linq.Expressions.Expression Convert(Type type, ConversionResultKind resultKind, System.Linq.Expressions.Expression target)
	{
		if (resultKind == ConversionResultKind.ExplicitCast)
		{
			return new DynamicConvertExpression(base.PyContext.Convert(type, resultKind), CompilationMode, target);
		}
		return CompilationMode.Dynamic(base.PyContext.Convert(type, resultKind), type, target);
	}

	internal System.Linq.Expressions.Expression Operation(Type resultType, PythonOperationKind operation, System.Linq.Expressions.Expression arg0)
	{
		if (resultType == typeof(object))
		{
			return new PythonDynamicExpression1(Binders.UnaryOperationBinder(base.PyContext, operation), CompilationMode, arg0);
		}
		return CompilationMode.Dynamic(Binders.UnaryOperationBinder(base.PyContext, operation), resultType, arg0);
	}

	internal System.Linq.Expressions.Expression Operation(Type resultType, PythonOperationKind operation, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1)
	{
		if (resultType == typeof(object))
		{
			return new PythonDynamicExpression2(Binders.BinaryOperationBinder(base.PyContext, operation), _mode, arg0, arg1);
		}
		return CompilationMode.Dynamic(Binders.BinaryOperationBinder(base.PyContext, operation), resultType, arg0, arg1);
	}

	internal System.Linq.Expressions.Expression Set(string name, System.Linq.Expressions.Expression target, System.Linq.Expressions.Expression value)
	{
		return new PythonDynamicExpression2(base.PyContext.SetMember(name), CompilationMode, target, value);
	}

	internal System.Linq.Expressions.Expression Get(string name, System.Linq.Expressions.Expression target)
	{
		return new DynamicGetMemberExpression(base.PyContext.GetMember(name), _mode, target, LocalContext);
	}

	internal System.Linq.Expressions.Expression Delete(Type resultType, string name, System.Linq.Expressions.Expression target)
	{
		return CompilationMode.Dynamic(base.PyContext.DeleteMember(name), resultType, target);
	}

	internal System.Linq.Expressions.Expression GetIndex(System.Linq.Expressions.Expression[] expressions)
	{
		return new PythonDynamicExpressionN(base.PyContext.GetIndex(expressions.Length), CompilationMode, expressions);
	}

	internal System.Linq.Expressions.Expression GetSlice(System.Linq.Expressions.Expression[] expressions)
	{
		return new PythonDynamicExpressionN(base.PyContext.GetSlice, CompilationMode, expressions);
	}

	internal System.Linq.Expressions.Expression SetIndex(System.Linq.Expressions.Expression[] expressions)
	{
		return new PythonDynamicExpressionN(base.PyContext.SetIndex(expressions.Length - 1), CompilationMode, expressions);
	}

	internal System.Linq.Expressions.Expression SetSlice(System.Linq.Expressions.Expression[] expressions)
	{
		return new PythonDynamicExpressionN(base.PyContext.SetSliceBinder, CompilationMode, expressions);
	}

	internal System.Linq.Expressions.Expression DeleteIndex(System.Linq.Expressions.Expression[] expressions)
	{
		return CompilationMode.Dynamic(base.PyContext.DeleteIndex(expressions.Length), typeof(void), expressions);
	}

	internal System.Linq.Expressions.Expression DeleteSlice(System.Linq.Expressions.Expression[] expressions)
	{
		return new PythonDynamicExpressionN(base.PyContext.DeleteSlice, CompilationMode, expressions);
	}

	internal PythonAst MakeLookupCode()
	{
		PythonAst pythonAst = (PythonAst)MemberwiseClone();
		pythonAst._mode = CompilationMode.Lookup;
		pythonAst._contextInfo = null;
		Dictionary<PythonVariable, System.Linq.Expressions.Expression> dictionary = new Dictionary<PythonVariable, System.Linq.Expressions.Expression>();
		foreach (KeyValuePair<PythonVariable, System.Linq.Expressions.Expression> globalVariable in _globalVariables)
		{
			dictionary[globalVariable.Key] = CompilationMode.Lookup.GetGlobal(_globalContext, -1, globalVariable.Key, null);
		}
		pythonAst._globalVariables = dictionary;
		pythonAst._body = new RewrittenBodyStatement(_body, new LookupVisitor(pythonAst, GetGlobalContext()).Visit(_body));
		return pythonAst;
	}
}
