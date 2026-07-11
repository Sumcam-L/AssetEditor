using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler;

public sealed class Tokenizer : TokenizerService
{
	private class TokenEqualityComparer : IEqualityComparer<object>
	{
		private readonly Tokenizer _tokenizer;

		public TokenEqualityComparer(Tokenizer tokenizer)
		{
			_tokenizer = tokenizer;
		}

		bool IEqualityComparer<object>.Equals(object x, object y)
		{
			if (x == _currentName)
			{
				if (y == _currentName)
				{
					return true;
				}
				return Equals((string)y);
			}
			if (y == _currentName)
			{
				return Equals((string)x);
			}
			return (string)x == (string)y;
		}

		public int GetHashCode(object obj)
		{
			int num = 5381;
			if (obj == _currentName)
			{
				char[] buffer = _tokenizer._buffer;
				int start = _tokenizer._start;
				int tokenEnd = _tokenizer._tokenEnd;
				for (int i = start; i < tokenEnd; i++)
				{
					int num2 = buffer[i];
					num = ((num << 5) + num) ^ num2;
				}
			}
			else
			{
				string text = (string)obj;
				foreach (int num3 in text)
				{
					num = ((num << 5) + num) ^ num3;
				}
			}
			return num;
		}

		private bool Equals(string value)
		{
			int num = _tokenizer._tokenEnd - _tokenizer._start;
			if (num != value.Length)
			{
				return false;
			}
			char[] buffer = _tokenizer._buffer;
			int num2 = 0;
			int num3 = _tokenizer._start;
			while (num2 < value.Length)
			{
				if (value[num2] != buffer[num3])
				{
					return false;
				}
				num2++;
				num3++;
			}
			return true;
		}
	}

	[Serializable]
	private class IncompleteString : IEquatable<IncompleteString>
	{
		public readonly bool IsRaw;

		public readonly bool IsUnicode;

		public readonly bool IsTripleQuoted;

		public readonly bool IsSingleTickQuote;

		public IncompleteString(bool isSingleTickQuote, bool isRaw, bool isUnicode, bool isTriple)
		{
			IsRaw = isRaw;
			IsUnicode = isUnicode;
			IsTripleQuoted = isTriple;
			IsSingleTickQuote = isSingleTickQuote;
		}

		public override bool Equals(object obj)
		{
			IncompleteString incompleteString = obj as IncompleteString;
			if (incompleteString != null)
			{
				return Equals(incompleteString);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (IsRaw ? 1 : 0) | (IsUnicode ? 2 : 0) | (IsTripleQuoted ? 4 : 0) | (IsSingleTickQuote ? 8 : 0);
		}

		public static bool operator ==(IncompleteString left, IncompleteString right)
		{
			return left?.Equals(right) ?? ((object)right == null);
		}

		public static bool operator !=(IncompleteString left, IncompleteString right)
		{
			return !(left == right);
		}

		public bool Equals(IncompleteString other)
		{
			if (other == null)
			{
				return false;
			}
			if (IsRaw == other.IsRaw && IsUnicode == other.IsUnicode && IsTripleQuoted == other.IsTripleQuoted)
			{
				return IsSingleTickQuote == other.IsSingleTickQuote;
			}
			return false;
		}
	}

	[Serializable]
	private struct State : IEquatable<State>
	{
		public int[] Indent;

		public int IndentLevel;

		public int PendingDedents;

		public bool LastNewLine;

		public IncompleteString IncompleteString;

		public StringBuilder[] IndentFormat;

		public int ParenLevel;

		public int BraceLevel;

		public int BracketLevel;

		public State(State state)
		{
			Indent = (int[])state.Indent.Clone();
			LastNewLine = state.LastNewLine;
			BracketLevel = state.BraceLevel;
			ParenLevel = state.ParenLevel;
			BraceLevel = state.BraceLevel;
			PendingDedents = state.PendingDedents;
			IndentLevel = state.IndentLevel;
			IndentFormat = ((state.IndentFormat != null) ? ((StringBuilder[])state.IndentFormat.Clone()) : null);
			IncompleteString = state.IncompleteString;
		}

		public State(object dummy)
		{
			Indent = new int[80];
			LastNewLine = false;
			BracketLevel = (ParenLevel = (BraceLevel = (PendingDedents = (IndentLevel = 0))));
			IndentFormat = null;
			IncompleteString = null;
		}

		public override bool Equals(object obj)
		{
			if (obj is State state)
			{
				return state == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(State left, State right)
		{
			if (left.BraceLevel == right.BraceLevel && left.BracketLevel == right.BracketLevel && left.IndentLevel == right.IndentLevel && left.ParenLevel == right.ParenLevel && left.PendingDedents == right.PendingDedents && left.LastNewLine == right.LastNewLine)
			{
				return left.IncompleteString == right.IncompleteString;
			}
			return false;
		}

		public static bool operator !=(State left, State right)
		{
			return !(left == right);
		}

		public bool Equals(State other)
		{
			return Equals(other);
		}
	}

	private const int EOF = -1;

	private const int MaxIndent = 80;

	private const int DefaultBufferCapacity = 1024;

	private State _state;

	private readonly bool _verbatim;

	internal bool _dontImplyDedent;

	private bool _disableLineFeedLineSeparator;

	private SourceUnit _sourceUnit;

	private ErrorSink _errors;

	private Severity _indentationInconsistencySeverity;

	private bool _endContinues;

	private bool _printFunction;

	private bool _unicodeLiterals;

	private List<int> _newLineLocations;

	private SourceLocation _initialLocation;

	private TextReader _reader;

	private char[] _buffer;

	private bool _multiEolns;

	private int _position;

	private int _end;

	private int _tokenEnd;

	private int _start;

	private int _tokenStartIndex;

	private int _tokenEndIndex;

	private bool _bufferResized;

	private Dictionary<object, NameToken> _names;

	private static object _currentName = new object();

	public override bool IsRestartable => true;

	public override object CurrentState => _state;

	public override SourceLocation CurrentPosition => IndexToLocation(CurrentIndex);

	public SourceUnit SourceUnit => _sourceUnit;

	public override ErrorSink ErrorSink
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

	public Severity IndentationInconsistencySeverity
	{
		get
		{
			return _indentationInconsistencySeverity;
		}
		set
		{
			_indentationInconsistencySeverity = value;
			if (value != Severity.Ignore && _state.IndentFormat == null)
			{
				_state.IndentFormat = new StringBuilder[80];
			}
		}
	}

	public bool IsEndOfFile => Peek() == -1;

	public IndexSpan TokenSpan => new IndexSpan(_tokenStartIndex, _tokenEndIndex - _tokenStartIndex);

	internal bool PrintFunction
	{
		get
		{
			return _printFunction;
		}
		set
		{
			_printFunction = value;
		}
	}

	internal bool UnicodeLiterals
	{
		get
		{
			return _unicodeLiterals;
		}
		set
		{
			_unicodeLiterals = value;
		}
	}

	public int GroupingLevel => _state.ParenLevel + _state.BraceLevel + _state.BracketLevel;

	public bool EndContinues => _endContinues;

	private int TokenLength => _tokenEnd - _start;

	private SourceLocation BufferTokenEnd => IndexToLocation(_tokenEndIndex);

	private IndexSpan BufferTokenSpan => new IndexSpan(_tokenStartIndex, _tokenEndIndex - _tokenStartIndex);

	private bool AtBeginning
	{
		get
		{
			if (_position == 0)
			{
				return !_bufferResized;
			}
			return false;
		}
	}

	private int CurrentIndex => _tokenStartIndex + Math.Min(_position, _end) - _start;

	public Tokenizer()
	{
		_errors = ErrorSink.Null;
		_verbatim = true;
		_state = new State(null);
		_names = new Dictionary<object, NameToken>(128, new TokenEqualityComparer(this));
	}

	public Tokenizer(ErrorSink errorSink)
	{
		_errors = errorSink;
		_state = new State(null);
		_names = new Dictionary<object, NameToken>(128, new TokenEqualityComparer(this));
	}

	public Tokenizer(ErrorSink errorSink, PythonCompilerOptions options)
	{
		ContractUtils.RequiresNotNull(errorSink, "errorSink");
		ContractUtils.RequiresNotNull(options, "options");
		_errors = errorSink;
		_verbatim = options.Verbatim;
		_state = new State(null);
		_dontImplyDedent = options.DontImplyDedent;
		_printFunction = options.PrintFunction;
		_unicodeLiterals = options.UnicodeLiterals;
		_names = new Dictionary<object, NameToken>(128, new TokenEqualityComparer(this));
	}

	internal Tokenizer(ErrorSink errorSink, PythonCompilerOptions options, bool verbatim)
		: this(errorSink, options)
	{
		_verbatim = verbatim || options.Verbatim;
	}

	public SourceLocation IndexToLocation(int index)
	{
		int num = _newLineLocations.BinarySearch(index);
		if (num < 0)
		{
			if (num == -1)
			{
				return new SourceLocation(index + _initialLocation.Index, _initialLocation.Line, checked(index + _initialLocation.Column));
			}
			num = ~num - 1;
		}
		return new SourceLocation(index + _initialLocation.Index, _sourceUnit.MapLine(num + 2) + _initialLocation.Line - 1, index - _newLineLocations[num] + _initialLocation.Column);
	}

	public void Initialize(SourceUnit sourceUnit)
	{
		ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");
		Initialize(null, sourceUnit.GetReader(), sourceUnit, SourceLocation.MinValue, 1024);
	}

	public override void Initialize(object state, TextReader reader, SourceUnit sourceUnit, SourceLocation initialLocation)
	{
		Initialize(state, reader, sourceUnit, initialLocation, 1024);
	}

	public void Initialize(object state, TextReader reader, SourceUnit sourceUnit, SourceLocation initialLocation, int bufferCapacity)
	{
		Initialize(state, reader, sourceUnit, initialLocation, bufferCapacity, null);
	}

	public void Initialize(object state, TextReader reader, SourceUnit sourceUnit, SourceLocation initialLocation, int bufferCapacity, PythonCompilerOptions compilerOptions)
	{
		ContractUtils.RequiresNotNull(reader, "reader");
		if (state != null)
		{
			if (!(state is State))
			{
				throw new ValueErrorException("bad state provided");
			}
			_state = new State((State)state);
		}
		else
		{
			_state = new State(null);
		}
		if (compilerOptions != null && compilerOptions.InitialIndent != null)
		{
			_state.Indent = (int[])compilerOptions.InitialIndent.Clone();
		}
		_sourceUnit = sourceUnit;
		_disableLineFeedLineSeparator = reader is NoLineFeedSourceContentProvider.Reader;
		_reader = reader;
		if (_buffer == null || _buffer.Length < bufferCapacity)
		{
			_buffer = new char[bufferCapacity];
		}
		_newLineLocations = new List<int>();
		_tokenEnd = -1;
		_multiEolns = !_disableLineFeedLineSeparator;
		_initialLocation = initialLocation;
		_tokenEndIndex = -1;
		_tokenStartIndex = 0;
		_start = (_end = 0);
		_position = 0;
	}

	public override TokenInfo ReadToken()
	{
		if (_buffer == null)
		{
			throw new InvalidOperationException("Uninitialized");
		}
		TokenInfo result = default(TokenInfo);
		Token nextToken = GetNextToken();
		result.SourceSpan = new SourceSpan(IndexToLocation(TokenSpan.Start), IndexToLocation(TokenSpan.End));
		switch (nextToken.Kind)
		{
		case TokenKind.EndOfFile:
			result.Category = TokenCategory.EndOfStream;
			break;
		case TokenKind.Comment:
			result.Category = TokenCategory.Comment;
			break;
		case TokenKind.Name:
			result.Category = TokenCategory.Identifier;
			break;
		case TokenKind.Error:
			if (nextToken is IncompleteStringErrorToken)
			{
				result.Category = TokenCategory.StringLiteral;
			}
			else
			{
				result.Category = TokenCategory.Error;
			}
			break;
		case TokenKind.Constant:
			result.Category = ((nextToken.Value is string) ? TokenCategory.StringLiteral : TokenCategory.NumericLiteral);
			break;
		case TokenKind.LeftParenthesis:
			result.Category = TokenCategory.Grouping;
			result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterStart;
			break;
		case TokenKind.RightParenthesis:
			result.Category = TokenCategory.Grouping;
			result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd;
			break;
		case TokenKind.LeftBracket:
		case TokenKind.RightBracket:
		case TokenKind.LeftBrace:
		case TokenKind.RightBrace:
			result.Category = TokenCategory.Grouping;
			result.Trigger = TokenTriggers.MatchBraces;
			break;
		case TokenKind.Colon:
			result.Category = TokenCategory.Delimiter;
			break;
		case TokenKind.Semicolon:
			result.Category = TokenCategory.Delimiter;
			break;
		case TokenKind.Comma:
			result.Category = TokenCategory.Delimiter;
			result.Trigger = TokenTriggers.ParameterNext;
			break;
		case TokenKind.Dot:
			result.Category = TokenCategory.Operator;
			result.Trigger = TokenTriggers.MemberSelect;
			break;
		case TokenKind.NewLine:
			result.Category = TokenCategory.WhiteSpace;
			break;
		default:
			if (nextToken.Kind >= TokenKind.FirstKeyword && nextToken.Kind <= TokenKind.LastKeyword)
			{
				result.Category = TokenCategory.Keyword;
			}
			else
			{
				result.Category = TokenCategory.Operator;
			}
			break;
		}
		return result;
	}

	internal bool TryGetTokenString(int len, out string tokenString)
	{
		if (len != TokenLength)
		{
			tokenString = null;
			return false;
		}
		tokenString = GetTokenString();
		return true;
	}

	public Token GetNextToken()
	{
		if (_state.PendingDedents != 0)
		{
			if (_state.PendingDedents == -1)
			{
				_state.PendingDedents = 0;
				return Tokens.IndentToken;
			}
			_state.PendingDedents--;
			return Tokens.DedentToken;
		}
		return Next();
	}

	private Token Next()
	{
		bool atBeginning = AtBeginning;
		if (_state.IncompleteString != null && Peek() != -1)
		{
			IncompleteString incompleteString = _state.IncompleteString;
			_state.IncompleteString = null;
			return ContinueString(incompleteString.IsSingleTickQuote ? '\'' : '"', incompleteString.IsRaw, incompleteString.IsUnicode, isBytes: false, incompleteString.IsTripleQuoted, 0);
		}
		DiscardToken();
		int num = NextChar();
		while (true)
		{
			switch (num)
			{
			case -1:
				return ReadEof();
			case 12:
				DiscardToken();
				num = NextChar();
				continue;
			case 9:
			case 32:
				num = SkipWhiteSpace(atBeginning);
				continue;
			case 35:
				if (_verbatim)
				{
					return ReadSingleLineComment();
				}
				num = SkipSingleLineComment();
				continue;
			case 92:
				if (ReadEolnOpt(NextChar()) > 0)
				{
					_newLineLocations.Add(CurrentIndex);
					DiscardToken();
					num = NextChar();
					if (num == -1)
					{
						_endContinues = true;
					}
					continue;
				}
				BufferBack();
				break;
			case 34:
			case 39:
				_state.LastNewLine = false;
				return ReadString((char)num, isRaw: false, isUni: false, isBytes: false);
			case 85:
			case 117:
				_state.LastNewLine = false;
				return ReadNameOrUnicodeString();
			case 82:
			case 114:
				_state.LastNewLine = false;
				return ReadNameOrRawString();
			case 66:
			case 98:
				_state.LastNewLine = false;
				return ReadNameOrBytes();
			case 95:
				_state.LastNewLine = false;
				return ReadName();
			case 46:
				_state.LastNewLine = false;
				num = Peek();
				if (num >= 48 && num <= 57)
				{
					return ReadFraction();
				}
				MarkTokenEnd();
				return Tokens.DotToken;
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
				_state.LastNewLine = false;
				return ReadNumber(num);
			}
			if (ReadEolnOpt(num) <= 0)
			{
				break;
			}
			_newLineLocations.Add(CurrentIndex);
			if (ReadNewline())
			{
				if (_state.LastNewLine)
				{
					return Tokens.NLToken;
				}
				_state.LastNewLine = true;
				return Tokens.NewLineToken;
			}
			DiscardToken();
			num = NextChar();
		}
		_state.LastNewLine = false;
		Token token = NextOperator(num);
		if (token != null)
		{
			MarkTokenEnd();
			return token;
		}
		if (IsNameStart(num))
		{
			return ReadName();
		}
		MarkTokenEnd();
		return BadChar(num);
	}

	private int SkipWhiteSpace(bool atBeginning)
	{
		int num;
		do
		{
			num = NextChar();
		}
		while (num == 32 || num == 9);
		BufferBack();
		if (atBeginning && num != 35 && num != 12 && num != -1 && !IsEoln(num))
		{
			MarkTokenEnd();
			ReportSyntaxError(BufferTokenSpan, Resources.InvalidSyntax, 16);
		}
		DiscardToken();
		SeekRelative(1);
		return num;
	}

	private int SkipSingleLineComment()
	{
		int result = ReadLine();
		MarkTokenEnd();
		DiscardToken();
		SeekRelative(1);
		return result;
	}

	private Token ReadSingleLineComment()
	{
		ReadLine();
		MarkTokenEnd();
		return new CommentToken(GetTokenString());
	}

	private Token ReadNameOrUnicodeString()
	{
		if (NextChar(34))
		{
			return ReadString('"', isRaw: false, isUni: true, isBytes: false);
		}
		if (NextChar(39))
		{
			return ReadString('\'', isRaw: false, isUni: true, isBytes: false);
		}
		if (NextChar(114) || NextChar(82))
		{
			if (NextChar(34))
			{
				return ReadString('"', isRaw: true, isUni: true, isBytes: false);
			}
			if (NextChar(39))
			{
				return ReadString('\'', isRaw: true, isUni: true, isBytes: false);
			}
			BufferBack();
		}
		return ReadName();
	}

	private Token ReadNameOrBytes()
	{
		if (NextChar(34))
		{
			return ReadString('"', isRaw: false, isUni: false, isBytes: true);
		}
		if (NextChar(39))
		{
			return ReadString('\'', isRaw: false, isUni: false, isBytes: true);
		}
		if (NextChar(114) || NextChar(82))
		{
			if (NextChar(34))
			{
				return ReadString('"', isRaw: true, isUni: false, isBytes: true);
			}
			if (NextChar(39))
			{
				return ReadString('\'', isRaw: true, isUni: false, isBytes: true);
			}
			BufferBack();
		}
		return ReadName();
	}

	private Token ReadNameOrRawString()
	{
		if (NextChar(34))
		{
			return ReadString('"', isRaw: true, isUni: false, isBytes: false);
		}
		if (NextChar(39))
		{
			return ReadString('\'', isRaw: true, isUni: false, isBytes: false);
		}
		return ReadName();
	}

	private Token ReadEof()
	{
		MarkTokenEnd();
		if (!_dontImplyDedent && _state.IndentLevel > 0)
		{
			if (!_state.LastNewLine)
			{
				_state.LastNewLine = true;
				return Tokens.NewLineToken;
			}
			SetIndent(0, null);
			_state.PendingDedents--;
			return Tokens.DedentToken;
		}
		return Tokens.EndOfFileToken;
	}

	private static ErrorToken BadChar(int ch)
	{
		return new ErrorToken(StringUtils.AddSlashes(((char)ch).ToString()));
	}

	private static bool IsNameStart(int ch)
	{
		if (!char.IsLetter((char)ch))
		{
			return ch == 95;
		}
		return true;
	}

	private static bool IsNamePart(int ch)
	{
		if (!char.IsLetterOrDigit((char)ch))
		{
			return ch == 95;
		}
		return true;
	}

	private Token ReadString(char quote, bool isRaw, bool isUni, bool isBytes)
	{
		int num = 0;
		bool isTriple = false;
		if (NextChar(quote))
		{
			if (NextChar(quote))
			{
				isTriple = true;
				num += 3;
			}
			else
			{
				BufferBack();
				num++;
			}
		}
		else
		{
			num++;
		}
		if (isRaw)
		{
			num++;
		}
		if (isUni)
		{
			num++;
		}
		if (isBytes)
		{
			num++;
		}
		return ContinueString(quote, isRaw, isUni, isBytes, isTriple, num);
	}

	private Token ContinueString(char quote, bool isRaw, bool isUnicode, bool isBytes, bool isTriple, int startAdd)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			int num3 = NextChar();
			if (num3 == -1)
			{
				BufferBack();
				if (isTriple)
				{
					MarkTokenEnd();
					SourceLocation sourceLocation = new SourceLocation(BufferTokenEnd.Index - 1, BufferTokenEnd.Line, IndexToLocation(_tokenStartIndex).Column + _tokenEndIndex - _tokenStartIndex - 1);
					ReportSyntaxError(new SourceSpan(sourceLocation, sourceLocation), Resources.EofInTripleQuotedString, 18);
				}
				else
				{
					MarkTokenEnd();
				}
				UnexpectedEndOfString(isTriple, isTriple);
				string tokenSubstring = GetTokenSubstring(startAdd, TokenLength - startAdd - num);
				_state.IncompleteString = new IncompleteString(quote == '\'', isRaw, isUnicode, isTriple);
				return new IncompleteStringErrorToken(Resources.EofInString, tokenSubstring);
			}
			if (num3 == quote)
			{
				if (!isTriple)
				{
					num++;
					break;
				}
				if (NextChar(quote) && NextChar(quote))
				{
					num += 3;
					break;
				}
			}
			else if (num3 == 92)
			{
				num3 = NextChar();
				if (num3 == -1)
				{
					BufferBack();
					MarkTokenEnd();
					UnexpectedEndOfString(isTriple, isTriple);
					string tokenSubstring2 = GetTokenSubstring(startAdd, TokenLength - startAdd - num - 1);
					_state.IncompleteString = new IncompleteString(quote == '\'', isRaw, isUnicode, isTriple);
					return new IncompleteStringErrorToken(Resources.EofInString, tokenSubstring2);
				}
				if ((num2 = ReadEolnOpt(num3)) > 0)
				{
					if (Peek() == -1)
					{
						SeekRelative(-num2);
						MarkTokenEnd();
						string tokenSubstring3 = GetTokenSubstring(startAdd, TokenLength - startAdd - num - 1);
						UnexpectedEndOfString(isTriple, isIncomplete: true);
						return new IncompleteStringErrorToken(Resources.EofInString, tokenSubstring3);
					}
				}
				else if (num3 != quote && num3 != 92)
				{
					BufferBack();
				}
			}
			else if ((num2 = ReadEolnOpt(num3)) > 0)
			{
				_newLineLocations.Add(CurrentIndex);
				if (!isTriple)
				{
					SeekRelative(-num2);
					MarkTokenEnd();
					UnexpectedEndOfString(isTriple, isIncomplete: false);
					string tokenSubstring4 = GetTokenSubstring(startAdd, TokenLength - startAdd - num);
					return new IncompleteStringErrorToken((quote == '"') ? Resources.NewLineInDoubleQuotedString : Resources.NewLineInSingleQuotedString, tokenSubstring4);
				}
			}
		}
		MarkTokenEnd();
		return MakeStringToken(quote, isRaw, isUnicode, isBytes, isTriple, _start + startAdd, TokenLength - startAdd - num);
	}

	private Token MakeStringToken(char quote, bool isRaw, bool isUnicode, bool isBytes, bool isTriple, int start, int length)
	{
		if (!isBytes)
		{
			string value = LiteralParser.ParseString(_buffer, start, length, isRaw, isUnicode || UnicodeLiterals, !_disableLineFeedLineSeparator);
			if (isUnicode)
			{
				return new UnicodeStringToken(value);
			}
			return new ConstantValueToken(value);
		}
		List<byte> list = LiteralParser.ParseBytes(_buffer, start, length, isRaw, !_disableLineFeedLineSeparator);
		if (list.Count == 0)
		{
			return new ConstantValueToken(Bytes.Empty);
		}
		return new ConstantValueToken(new Bytes(list));
	}

	private void UnexpectedEndOfString(bool isTriple, bool isIncomplete)
	{
		string message = (isTriple ? Resources.EofInTripleQuotedString : Resources.EolInSingleQuotedString);
		int errorCode = (isIncomplete ? 18 : 16);
		ReportSyntaxError(BufferTokenSpan, message, errorCode);
	}

	private Token ReadNumber(int start)
	{
		int num = 10;
		if (start == 48)
		{
			if (NextChar(120) || NextChar(88))
			{
				return ReadHexNumber();
			}
			if (NextChar(98) || NextChar(66))
			{
				return ReadBinaryNumber();
			}
			if (NextChar(111) || NextChar(79))
			{
				return ReadOctalNumber();
			}
			num = 8;
		}
		while (true)
		{
			switch (NextChar())
			{
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
				break;
			case 46:
				return ReadFraction();
			case 69:
			case 101:
				return ReadExponent();
			case 74:
			case 106:
				MarkTokenEnd();
				return new ConstantValueToken(LiteralParser.ParseImaginary(GetTokenString()));
			case 76:
			case 108:
				MarkTokenEnd();
				return new ConstantValueToken(LiteralParser.ParseBigInteger(GetTokenString(), num));
			default:
				BufferBack();
				MarkTokenEnd();
				return new ConstantValueToken(ParseInteger(GetTokenString(), num));
			}
		}
	}

	private Token ReadBinaryNumber()
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		BigInteger bigInteger = BigInteger.Zero;
		while (true)
		{
			int num3 = NextChar();
			switch (num3)
			{
			case 48:
				if (num2 == 0)
				{
					break;
				}
				goto case 49;
			case 49:
				num++;
				if (num == 32)
				{
					flag = true;
					bigInteger = num2;
				}
				if (num >= 32)
				{
					bigInteger = (bigInteger << 1) | (num3 - 48);
				}
				else
				{
					num2 = (num2 << 1) | (num3 - 48);
				}
				break;
			case 76:
			case 108:
				MarkTokenEnd();
				return new ConstantValueToken(flag ? bigInteger : ((BigInteger)num2));
			default:
				BufferBack();
				MarkTokenEnd();
				return new ConstantValueToken(flag ? ((object)bigInteger) : ((object)num2));
			}
		}
	}

	private Token ReadOctalNumber()
	{
		while (true)
		{
			switch (NextChar())
			{
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
				break;
			case 76:
			case 108:
				MarkTokenEnd();
				return new ConstantValueToken(LiteralParser.ParseBigInteger(GetTokenSubstring(2, TokenLength - 2), 8));
			default:
				BufferBack();
				MarkTokenEnd();
				return new ConstantValueToken(ParseInteger(GetTokenSubstring(2), 8));
			}
		}
	}

	private Token ReadHexNumber()
	{
		while (true)
		{
			switch (NextChar())
			{
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
			case 65:
			case 66:
			case 67:
			case 68:
			case 69:
			case 70:
			case 97:
			case 98:
			case 99:
			case 100:
			case 101:
			case 102:
				break;
			case 76:
			case 108:
				MarkTokenEnd();
				return new ConstantValueToken(LiteralParser.ParseBigInteger(GetTokenSubstring(2, TokenLength - 3), 16));
			default:
				BufferBack();
				MarkTokenEnd();
				return new ConstantValueToken(ParseInteger(GetTokenSubstring(2), 16));
			}
		}
	}

	private Token ReadFraction()
	{
		while (true)
		{
			switch (NextChar())
			{
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
				break;
			case 69:
			case 101:
				return ReadExponent();
			case 74:
			case 106:
				MarkTokenEnd();
				return new ConstantValueToken(LiteralParser.ParseImaginary(GetTokenString()));
			default:
				BufferBack();
				MarkTokenEnd();
				return new ConstantValueToken(ParseFloat(GetTokenString()));
			}
		}
	}

	private Token ReadExponent()
	{
		int num = NextChar();
		if (num == 45 || num == 43)
		{
			num = NextChar();
		}
		while (true)
		{
			switch (num)
			{
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
				break;
			case 74:
			case 106:
				MarkTokenEnd();
				return new ConstantValueToken(LiteralParser.ParseImaginary(GetTokenString()));
			default:
				BufferBack();
				MarkTokenEnd();
				return new ConstantValueToken(ParseFloat(GetTokenString()));
			}
			num = NextChar();
		}
	}

	private Token ReadName()
	{
		BufferBack();
		int num;
		switch (NextChar())
		{
		case 105:
			switch (NextChar())
			{
			case 110:
				if (!IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordInToken;
				}
				break;
			case 109:
				if (NextChar() == 112 && NextChar() == 111 && NextChar() == 114 && NextChar() == 116 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordImportToken;
				}
				break;
			case 115:
				if (!IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordIsToken;
				}
				break;
			case 102:
				if (!IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordIfToken;
				}
				break;
			}
			break;
		case 119:
			switch (NextChar())
			{
			case 105:
				if (NextChar() == 116 && NextChar() == 104 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordWithToken;
				}
				break;
			case 104:
				if (NextChar() == 105 && NextChar() == 108 && NextChar() == 101 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordWhileToken;
				}
				break;
			}
			break;
		case 116:
			if (NextChar() == 114 && NextChar() == 121 && !IsNamePart(Peek()))
			{
				MarkTokenEnd();
				return Tokens.KeywordTryToken;
			}
			break;
		case 114:
			switch (NextChar())
			{
			case 101:
				if (NextChar() == 116 && NextChar() == 117 && NextChar() == 114 && NextChar() == 110 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordReturnToken;
				}
				break;
			case 97:
				if (NextChar() == 105 && NextChar() == 115 && NextChar() == 101 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordRaiseToken;
				}
				break;
			}
			break;
		case 112:
			switch (NextChar())
			{
			case 97:
				if (NextChar() == 115 && NextChar() == 115 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordPassToken;
				}
				break;
			case 114:
				if (NextChar() == 105 && NextChar() == 110 && NextChar() == 116 && !IsNamePart(Peek()) && !_printFunction)
				{
					MarkTokenEnd();
					return Tokens.KeywordPrintToken;
				}
				break;
			}
			break;
		case 103:
			if (NextChar() == 108 && NextChar() == 111 && NextChar() == 98 && NextChar() == 97 && NextChar() == 108 && !IsNamePart(Peek()))
			{
				MarkTokenEnd();
				return Tokens.KeywordGlobalToken;
			}
			break;
		case 102:
			switch (NextChar())
			{
			case 114:
				if (NextChar() == 111 && NextChar() == 109 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordFromToken;
				}
				break;
			case 105:
				if (NextChar() == 110 && NextChar() == 97 && NextChar() == 108 && NextChar() == 108 && NextChar() == 121 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordFinallyToken;
				}
				break;
			case 111:
				if (NextChar() == 114 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordForToken;
				}
				break;
			}
			break;
		case 101:
			switch (NextChar())
			{
			case 120:
				switch (NextChar())
				{
				case 101:
					if (NextChar() == 99 && !IsNamePart(Peek()))
					{
						MarkTokenEnd();
						return Tokens.KeywordExecToken;
					}
					break;
				case 99:
					if (NextChar() == 101 && NextChar() == 112 && NextChar() == 116 && !IsNamePart(Peek()))
					{
						MarkTokenEnd();
						return Tokens.KeywordExceptToken;
					}
					break;
				}
				break;
			case 108:
				switch (NextChar())
				{
				case 115:
					if (NextChar() == 101 && !IsNamePart(Peek()))
					{
						MarkTokenEnd();
						return Tokens.KeywordElseToken;
					}
					break;
				case 105:
					if (NextChar() == 102 && !IsNamePart(Peek()))
					{
						MarkTokenEnd();
						return Tokens.KeywordElseIfToken;
					}
					break;
				}
				break;
			}
			break;
		case 100:
			num = NextChar();
			if (num != 101)
			{
				break;
			}
			switch (NextChar())
			{
			case 108:
				if (!IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordDelToken;
				}
				break;
			case 102:
				if (!IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordDefToken;
				}
				break;
			}
			break;
		case 99:
			switch (NextChar())
			{
			case 108:
				if (NextChar() == 97 && NextChar() == 115 && NextChar() == 115 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordClassToken;
				}
				break;
			case 111:
				if (NextChar() == 110 && NextChar() == 116 && NextChar() == 105 && NextChar() == 110 && NextChar() == 117 && NextChar() == 101 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordContinueToken;
				}
				break;
			}
			break;
		case 98:
			if (NextChar() == 114 && NextChar() == 101 && NextChar() == 97 && NextChar() == 107 && !IsNamePart(Peek()))
			{
				MarkTokenEnd();
				return Tokens.KeywordBreakToken;
			}
			break;
		case 97:
			switch (NextChar())
			{
			case 110:
				if (NextChar() == 100 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordAndToken;
				}
				break;
			case 115:
				if (!IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordAsToken;
				}
				if (NextChar() == 115 && NextChar() == 101 && NextChar() == 114 && NextChar() == 116 && !IsNamePart(Peek()))
				{
					MarkTokenEnd();
					return Tokens.KeywordAssertToken;
				}
				break;
			}
			break;
		case 121:
			if (NextChar() == 105 && NextChar() == 101 && NextChar() == 108 && NextChar() == 100 && !IsNamePart(Peek()))
			{
				MarkTokenEnd();
				return Tokens.KeywordYieldToken;
			}
			break;
		case 111:
			if (NextChar() == 114 && !IsNamePart(Peek()))
			{
				MarkTokenEnd();
				return Tokens.KeywordOrToken;
			}
			break;
		case 110:
			if (NextChar() == 111 && NextChar() == 116 && !IsNamePart(Peek()))
			{
				MarkTokenEnd();
				return Tokens.KeywordNotToken;
			}
			break;
		case 78:
			if (NextChar() == 111 && NextChar() == 110 && NextChar() == 101 && !IsNamePart(Peek()))
			{
				MarkTokenEnd();
				return Tokens.NoneToken;
			}
			break;
		case 108:
			if (NextChar() == 97 && NextChar() == 109 && NextChar() == 98 && NextChar() == 100 && NextChar() == 97 && !IsNamePart(Peek()))
			{
				MarkTokenEnd();
				return Tokens.KeywordLambdaToken;
			}
			break;
		}
		BufferBack();
		num = NextChar();
		while (IsNamePart(num))
		{
			num = NextChar();
		}
		BufferBack();
		MarkTokenEnd();
		if (!_names.TryGetValue(_currentName, out var value))
		{
			string tokenString = GetTokenString();
			return _names[tokenString] = new NameToken(tokenString);
		}
		return value;
	}

	private bool ReadNewline()
	{
		if (IndentationInconsistencySeverity != Severity.Ignore)
		{
			return ReadNewlineWithChecks();
		}
		int num = 0;
		while (true)
		{
			int num2 = NextChar();
			switch (num2)
			{
			case 32:
				num++;
				continue;
			case 9:
				num += 8 - num % 8;
				continue;
			case 12:
				num = 0;
				continue;
			case 35:
				if (_verbatim)
				{
					BufferBack();
					MarkTokenEnd();
					return true;
				}
				num2 = ReadLine();
				continue;
			}
			BufferBack();
			if (GroupingLevel > 0)
			{
				return false;
			}
			MarkTokenEnd();
			switch (num2)
			{
			case -1:
				if (num < _state.Indent[_state.IndentLevel])
				{
					if (_sourceUnit.Kind == SourceCodeKind.InteractiveCode || _sourceUnit.Kind == SourceCodeKind.Statements)
					{
						SetIndent(num, null);
					}
					else
					{
						DoDedent(num, _state.Indent[_state.IndentLevel]);
					}
				}
				break;
			default:
				SetIndent(num, null);
				break;
			case 10:
			case 13:
				break;
			}
			return true;
		}
	}

	private bool ReadNewlineWithChecks()
	{
		StringBuilder stringBuilder = new StringBuilder(80);
		int num = 0;
		while (true)
		{
			int num2 = NextChar();
			switch (num2)
			{
			case 32:
				num++;
				stringBuilder.Append(' ');
				continue;
			case 9:
				num += 8 - num % 8;
				stringBuilder.Append('\t');
				continue;
			case 12:
				num = 0;
				stringBuilder.Append('\f');
				continue;
			case 35:
				if (_verbatim)
				{
					BufferBack();
					MarkTokenEnd();
					return true;
				}
				num2 = ReadLine();
				continue;
			}
			if (ReadEolnOpt(num2) > 0)
			{
				_newLineLocations.Add(CurrentIndex);
				num = 0;
				stringBuilder.Length = 0;
				continue;
			}
			BufferBack();
			if (GroupingLevel > 0)
			{
				return false;
			}
			MarkTokenEnd();
			CheckIndent(stringBuilder);
			switch (num2)
			{
			case -1:
				if (num < _state.Indent[_state.IndentLevel])
				{
					if (_sourceUnit.Kind == SourceCodeKind.InteractiveCode || _sourceUnit.Kind == SourceCodeKind.Statements)
					{
						SetIndent(num, stringBuilder);
					}
					else
					{
						DoDedent(num, _state.Indent[_state.IndentLevel]);
					}
				}
				break;
			default:
				SetIndent(num, stringBuilder);
				break;
			case 10:
			case 13:
				break;
			}
			return true;
		}
	}

	private void CheckIndent(StringBuilder sb)
	{
		if (_state.Indent[_state.IndentLevel] <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = _state.IndentFormat[_state.IndentLevel];
		int num = ((stringBuilder.Length < sb.Length) ? stringBuilder.Length : sb.Length);
		for (int i = 0; i < num; i++)
		{
			if (sb[i] != stringBuilder[i])
			{
				SourceLocation bufferTokenEnd = BufferTokenEnd;
				_errors.Add(_sourceUnit, Resources.InconsistentWhitespace, new SourceSpan(bufferTokenEnd, bufferTokenEnd), 48, _indentationInconsistencySeverity);
				_indentationInconsistencySeverity = Severity.Ignore;
			}
		}
	}

	private void SetIndent(int spaces, StringBuilder chars)
	{
		int num = _state.Indent[_state.IndentLevel];
		if (spaces == num)
		{
			return;
		}
		if (spaces > num)
		{
			_state.Indent[++_state.IndentLevel] = spaces;
			if (_state.IndentFormat != null)
			{
				_state.IndentFormat[_state.IndentLevel] = chars;
			}
			_state.PendingDedents = -1;
		}
		else
		{
			num = DoDedent(spaces, num);
			if (spaces != num)
			{
				ReportSyntaxError(new SourceSpan(new SourceLocation(_tokenEndIndex, IndexToLocation(_tokenEndIndex).Line, IndexToLocation(_tokenEndIndex).Column - 1), BufferTokenEnd), Resources.IndentationMismatch, 32);
			}
		}
	}

	private int DoDedent(int spaces, int current)
	{
		while (spaces < current)
		{
			_state.IndentLevel--;
			_state.PendingDedents++;
			current = _state.Indent[_state.IndentLevel];
		}
		return current;
	}

	private object ParseInteger(string s, int radix)
	{
		try
		{
			return LiteralParser.ParseInteger(s, radix);
		}
		catch (ArgumentException ex)
		{
			ReportSyntaxError(BufferTokenSpan, ex.Message, 16);
		}
		return ScriptingRuntimeHelpers.Int32ToObject(0);
	}

	private object ParseFloat(string s)
	{
		try
		{
			return LiteralParser.ParseFloat(s);
		}
		catch (Exception ex)
		{
			ReportSyntaxError(BufferTokenSpan, ex.Message, 16);
			return 0.0;
		}
	}

	internal static bool TryGetEncoding(Encoding defaultEncoding, string line, ref Encoding enc, out string encName)
	{
		encName = null;
		int i = 0;
		if (line.Length < 10)
		{
			return false;
		}
		for (; i < line.Length && char.IsWhiteSpace(line[i]); i++)
		{
		}
		if (i == line.Length || line[i] != '#')
		{
			return false;
		}
		int num;
		if ((num = line.IndexOf("coding")) == -1)
		{
			return false;
		}
		if (line.Length <= num + 6)
		{
			return false;
		}
		if (line[num + 6] != ':' && line[num + 6] != '=')
		{
			return false;
		}
		int j;
		for (j = num + 7; j < line.Length && char.IsWhiteSpace(line[j]); j++)
		{
		}
		if (j == line.Length)
		{
			return false;
		}
		int k;
		for (k = j; k < line.Length && (line[k] == '-' || line[k] == '.' || char.IsLetterOrDigit(line[k])); k++)
		{
		}
		encName = line.Substring(j, k - j);
		if (StringOps.TryGetEncoding(encName, out enc))
		{
			enc.DecoderFallback = new NonStrictDecoderFallback();
			return true;
		}
		return false;
	}

	private void ReportSyntaxError(SourceSpan span, string message, int errorCode)
	{
		_errors.Add(_sourceUnit, message, span, errorCode, Severity.FatalError);
	}

	private void ReportSyntaxError(IndexSpan span, string message, int errorCode)
	{
		_errors.Add(_sourceUnit, message, new SourceSpan(IndexToLocation(span.Start), IndexToLocation(span.End)), errorCode, Severity.FatalError);
	}

	[Conditional("DUMP_TOKENS")]
	private void DumpBeginningOfUnit()
	{
		Console.WriteLine("--- Source unit: '{0}' ---", _sourceUnit.Path);
	}

	[Conditional("DUMP_TOKENS")]
	private static void DumpToken(Token token)
	{
		Console.WriteLine("{0} `{1}`", token.Kind, token.Image.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"));
	}

	public int[] GetLineLocations()
	{
		return _newLineLocations.ToArray();
	}

	private string GetTokenSubstring(int offset)
	{
		return GetTokenSubstring(offset, _tokenEnd - _start - offset);
	}

	private string GetTokenSubstring(int offset, int length)
	{
		return new string(_buffer, _start + offset, length);
	}

	[Conditional("DEBUG")]
	private void CheckInvariants()
	{
	}

	private int Peek()
	{
		if (_position >= _end)
		{
			RefillBuffer();
			if (_position >= _end)
			{
				return -1;
			}
		}
		return _buffer[_position];
	}

	private int ReadLine()
	{
		int num;
		do
		{
			num = NextChar();
		}
		while (num != -1 && !IsEoln(num));
		BufferBack();
		return num;
	}

	private void MarkTokenEnd()
	{
		_tokenEnd = Math.Min(_position, _end);
		int num = _tokenEnd - _start;
		_tokenEndIndex = _tokenStartIndex + num;
	}

	[Conditional("DUMP_TOKENS")]
	private void DumpToken()
	{
		Console.WriteLine("--> `{0}` {1}", GetTokenString().Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"), TokenSpan);
	}

	private void BufferBack()
	{
		SeekRelative(-1);
	}

	internal string GetTokenString()
	{
		return new string(_buffer, _start, _tokenEnd - _start);
	}

	private void SeekRelative(int disp)
	{
		_position += disp;
	}

	private bool NextChar(int ch)
	{
		if (Peek() == ch)
		{
			_position++;
			return true;
		}
		return false;
	}

	private int NextChar()
	{
		int result = Peek();
		_position++;
		return result;
	}

	private void DiscardToken()
	{
		if (_tokenEnd == -1)
		{
			MarkTokenEnd();
		}
		_start = _tokenEnd;
		_tokenStartIndex = _tokenEndIndex;
		_tokenEnd = -1;
	}

	private int ReadEolnOpt(int current)
	{
		switch (current)
		{
		case 10:
			return 1;
		case 13:
			if (_multiEolns)
			{
				if (Peek() == 10)
				{
					SeekRelative(1);
					return 2;
				}
				return 1;
			}
			break;
		}
		return 0;
	}

	private bool IsEoln(int current)
	{
		switch (current)
		{
		case 10:
			return true;
		case 13:
			if (_multiEolns)
			{
				if (Peek() == 10)
				{
					return true;
				}
				return true;
			}
			break;
		}
		return false;
	}

	private void RefillBuffer()
	{
		if (_end == _buffer.Length)
		{
			int newSize = Math.Max(Math.Max((_end - _start) * 2, _buffer.Length), _position);
			ResizeInternal(ref _buffer, newSize, _start, _end - _start);
			_end -= _start;
			_position -= _start;
			_start = 0;
			_bufferResized = true;
		}
		int num = _reader.Read(_buffer, _end, _buffer.Length - _end);
		_end += num;
	}

	private static void ResizeInternal(ref char[] array, int newSize, int start, int count)
	{
		char[] array2 = ((newSize != array.Length) ? new char[newSize] : array);
		Buffer.BlockCopy(array, start * 2, array2, 0, count * 2);
		array = array2;
	}

	[Conditional("DEBUG")]
	private void ClearInvalidChars()
	{
		for (int i = 0; i < _start; i++)
		{
			_buffer[i] = '\0';
		}
		for (int j = _end; j < _buffer.Length; j++)
		{
			_buffer[j] = '\0';
		}
	}

	private Token NextOperator(int ch)
	{
		switch (ch)
		{
		case 43:
			if (NextChar(61))
			{
				return Tokens.AddEqualToken;
			}
			return Tokens.AddToken;
		case 45:
			if (NextChar(61))
			{
				return Tokens.SubtractEqualToken;
			}
			return Tokens.SubtractToken;
		case 42:
			if (NextChar(61))
			{
				return Tokens.MultiplyEqualToken;
			}
			if (NextChar(42))
			{
				if (NextChar(61))
				{
					return Tokens.PowerEqualToken;
				}
				return Tokens.PowerToken;
			}
			return Tokens.MultiplyToken;
		case 47:
			if (NextChar(61))
			{
				return Tokens.DivideEqualToken;
			}
			if (NextChar(47))
			{
				if (NextChar(61))
				{
					return Tokens.FloorDivideEqualToken;
				}
				return Tokens.FloorDivideToken;
			}
			return Tokens.DivideToken;
		case 37:
			if (NextChar(61))
			{
				return Tokens.ModEqualToken;
			}
			return Tokens.ModToken;
		case 60:
			if (NextChar(62))
			{
				return Tokens.LessThanGreaterThanToken;
			}
			if (NextChar(61))
			{
				return Tokens.LessThanOrEqualToken;
			}
			if (NextChar(60))
			{
				if (NextChar(61))
				{
					return Tokens.LeftShiftEqualToken;
				}
				return Tokens.LeftShiftToken;
			}
			return Tokens.LessThanToken;
		case 62:
			if (NextChar(62))
			{
				if (NextChar(61))
				{
					return Tokens.RightShiftEqualToken;
				}
				return Tokens.RightShiftToken;
			}
			if (NextChar(61))
			{
				return Tokens.GreaterThanOrEqualToken;
			}
			return Tokens.GreaterThanToken;
		case 38:
			if (NextChar(61))
			{
				return Tokens.BitwiseAndEqualToken;
			}
			return Tokens.BitwiseAndToken;
		case 124:
			if (NextChar(61))
			{
				return Tokens.BitwiseOrEqualToken;
			}
			return Tokens.BitwiseOrToken;
		case 94:
			if (NextChar(61))
			{
				return Tokens.ExclusiveOrEqualToken;
			}
			return Tokens.ExclusiveOrToken;
		case 61:
			if (NextChar(61))
			{
				return Tokens.EqualsToken;
			}
			return Tokens.AssignToken;
		case 33:
			if (NextChar(61))
			{
				return Tokens.NotEqualsToken;
			}
			return BadChar(ch);
		case 40:
			_state.ParenLevel++;
			return Tokens.LeftParenthesisToken;
		case 41:
			_state.ParenLevel--;
			return Tokens.RightParenthesisToken;
		case 91:
			_state.BracketLevel++;
			return Tokens.LeftBracketToken;
		case 93:
			_state.BracketLevel--;
			return Tokens.RightBracketToken;
		case 123:
			_state.BraceLevel++;
			return Tokens.LeftBraceToken;
		case 125:
			_state.BraceLevel--;
			return Tokens.RightBraceToken;
		case 44:
			return Tokens.CommaToken;
		case 58:
			return Tokens.ColonToken;
		case 96:
			return Tokens.BackQuoteToken;
		case 59:
			return Tokens.SemicolonToken;
		case 126:
			return Tokens.TwiddleToken;
		case 64:
			return Tokens.AtToken;
		default:
			return null;
		}
	}
}
