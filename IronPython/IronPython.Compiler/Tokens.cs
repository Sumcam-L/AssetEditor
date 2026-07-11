namespace IronPython.Compiler;

public static class Tokens
{
	public static readonly Token EndOfFileToken = new SymbolToken(TokenKind.EndOfFile, "<eof>");

	public static readonly Token NewLineToken = new SymbolToken(TokenKind.NewLine, "<newline>");

	public static readonly Token NLToken = new SymbolToken(TokenKind.NLToken, "<NL>");

	public static readonly Token IndentToken = new SymbolToken(TokenKind.Indent, "<indent>");

	public static readonly Token DedentToken = new SymbolToken(TokenKind.Dedent, "<dedent>");

	public static readonly Token CommentToken = new SymbolToken(TokenKind.Comment, "<comment>");

	public static readonly Token NoneToken = new ConstantValueToken(null);

	public static readonly Token DotToken = new SymbolToken(TokenKind.Dot, ".");

	private static readonly Token symAddToken = new OperatorToken(TokenKind.Add, "+", 4);

	private static readonly Token symAddEqualToken = new SymbolToken(TokenKind.AddEqual, "+=");

	private static readonly Token symSubtractToken = new OperatorToken(TokenKind.Subtract, "-", 4);

	private static readonly Token symSubtractEqualToken = new SymbolToken(TokenKind.SubtractEqual, "-=");

	private static readonly Token symPowerToken = new OperatorToken(TokenKind.Power, "**", 6);

	private static readonly Token symPowerEqualToken = new SymbolToken(TokenKind.PowerEqual, "**=");

	private static readonly Token symMultiplyToken = new OperatorToken(TokenKind.Multiply, "*", 5);

	private static readonly Token symMultiplyEqualToken = new SymbolToken(TokenKind.MultiplyEqual, "*=");

	private static readonly Token symFloorDivideToken = new OperatorToken(TokenKind.FloorDivide, "//", 5);

	private static readonly Token symFloorDivideEqualToken = new SymbolToken(TokenKind.FloorDivideEqual, "//=");

	private static readonly Token symDivideToken = new OperatorToken(TokenKind.Divide, "/", 5);

	private static readonly Token symDivideEqualToken = new SymbolToken(TokenKind.DivideEqual, "/=");

	private static readonly Token symModToken = new OperatorToken(TokenKind.Mod, "%", 5);

	private static readonly Token symModEqualToken = new SymbolToken(TokenKind.ModEqual, "%=");

	private static readonly Token symLeftShiftToken = new OperatorToken(TokenKind.LeftShift, "<<", 3);

	private static readonly Token symLeftShiftEqualToken = new SymbolToken(TokenKind.LeftShiftEqual, "<<=");

	private static readonly Token symRightShiftToken = new OperatorToken(TokenKind.RightShift, ">>", 3);

	private static readonly Token symRightShiftEqualToken = new SymbolToken(TokenKind.RightShiftEqual, ">>=");

	private static readonly Token symBitwiseAndToken = new OperatorToken(TokenKind.BitwiseAnd, "&", 2);

	private static readonly Token symBitwiseAndEqualToken = new SymbolToken(TokenKind.BitwiseAndEqual, "&=");

	private static readonly Token symBitwiseOrToken = new OperatorToken(TokenKind.BitwiseOr, "|", 0);

	private static readonly Token symBitwiseOrEqualToken = new SymbolToken(TokenKind.BitwiseOrEqual, "|=");

	private static readonly Token symExclusiveOrToken = new OperatorToken(TokenKind.ExclusiveOr, "^", 1);

	private static readonly Token symExclusiveOrEqualToken = new SymbolToken(TokenKind.ExclusiveOrEqual, "^=");

	private static readonly Token symLessThanToken = new OperatorToken(TokenKind.LessThan, "<", -1);

	private static readonly Token symGreaterThanToken = new OperatorToken(TokenKind.GreaterThan, ">", -1);

	private static readonly Token symLessThanOrEqualToken = new OperatorToken(TokenKind.LessThanOrEqual, "<=", -1);

	private static readonly Token symGreaterThanOrEqualToken = new OperatorToken(TokenKind.GreaterThanOrEqual, ">=", -1);

	private static readonly Token symEqualsToken = new OperatorToken(TokenKind.Equals, "==", -1);

	private static readonly Token symNotEqualsToken = new OperatorToken(TokenKind.NotEquals, "!=", -1);

	private static readonly Token symLessThanGreaterThanToken = new SymbolToken(TokenKind.LessThanGreaterThan, "<>");

	private static readonly Token symLeftParenthesisToken = new SymbolToken(TokenKind.LeftParenthesis, "(");

	private static readonly Token symRightParenthesisToken = new SymbolToken(TokenKind.RightParenthesis, ")");

	private static readonly Token symLeftBracketToken = new SymbolToken(TokenKind.LeftBracket, "[");

	private static readonly Token symRightBracketToken = new SymbolToken(TokenKind.RightBracket, "]");

	private static readonly Token symLeftBraceToken = new SymbolToken(TokenKind.LeftBrace, "{");

	private static readonly Token symRightBraceToken = new SymbolToken(TokenKind.RightBrace, "}");

	private static readonly Token symCommaToken = new SymbolToken(TokenKind.Comma, ",");

	private static readonly Token symColonToken = new SymbolToken(TokenKind.Colon, ":");

	private static readonly Token symBackQuoteToken = new SymbolToken(TokenKind.BackQuote, "`");

	private static readonly Token symSemicolonToken = new SymbolToken(TokenKind.Semicolon, ";");

	private static readonly Token symAssignToken = new SymbolToken(TokenKind.Assign, "=");

	private static readonly Token symTwiddleToken = new SymbolToken(TokenKind.Twiddle, "~");

	private static readonly Token symAtToken = new SymbolToken(TokenKind.At, "@");

	private static readonly Token kwAndToken = new SymbolToken(TokenKind.FirstKeyword, "and");

	private static readonly Token kwAsToken = new SymbolToken(TokenKind.KeywordAs, "as");

	private static readonly Token kwAssertToken = new SymbolToken(TokenKind.KeywordAssert, "assert");

	private static readonly Token kwBreakToken = new SymbolToken(TokenKind.KeywordBreak, "break");

	private static readonly Token kwClassToken = new SymbolToken(TokenKind.KeywordClass, "class");

	private static readonly Token kwContinueToken = new SymbolToken(TokenKind.KeywordContinue, "continue");

	private static readonly Token kwDefToken = new SymbolToken(TokenKind.KeywordDef, "def");

	private static readonly Token kwDelToken = new SymbolToken(TokenKind.KeywordDel, "del");

	private static readonly Token kwElseIfToken = new SymbolToken(TokenKind.KeywordElseIf, "elif");

	private static readonly Token kwElseToken = new SymbolToken(TokenKind.KeywordElse, "else");

	private static readonly Token kwExceptToken = new SymbolToken(TokenKind.KeywordExcept, "except");

	private static readonly Token kwExecToken = new SymbolToken(TokenKind.KeywordExec, "exec");

	private static readonly Token kwFinallyToken = new SymbolToken(TokenKind.KeywordFinally, "finally");

	private static readonly Token kwForToken = new SymbolToken(TokenKind.KeywordFor, "for");

	private static readonly Token kwFromToken = new SymbolToken(TokenKind.KeywordFrom, "from");

	private static readonly Token kwGlobalToken = new SymbolToken(TokenKind.KeywordGlobal, "global");

	private static readonly Token kwIfToken = new SymbolToken(TokenKind.KeywordIf, "if");

	private static readonly Token kwImportToken = new SymbolToken(TokenKind.KeywordImport, "import");

	private static readonly Token kwInToken = new SymbolToken(TokenKind.KeywordIn, "in");

	private static readonly Token kwIsToken = new SymbolToken(TokenKind.KeywordIs, "is");

	private static readonly Token kwLambdaToken = new SymbolToken(TokenKind.KeywordLambda, "lambda");

	private static readonly Token kwNotToken = new SymbolToken(TokenKind.KeywordNot, "not");

	private static readonly Token kwOrToken = new SymbolToken(TokenKind.KeywordOr, "or");

	private static readonly Token kwPassToken = new SymbolToken(TokenKind.KeywordPass, "pass");

	private static readonly Token kwPrintToken = new SymbolToken(TokenKind.KeywordPrint, "print");

	private static readonly Token kwRaiseToken = new SymbolToken(TokenKind.KeywordRaise, "raise");

	private static readonly Token kwReturnToken = new SymbolToken(TokenKind.KeywordReturn, "return");

	private static readonly Token kwTryToken = new SymbolToken(TokenKind.KeywordTry, "try");

	private static readonly Token kwWhileToken = new SymbolToken(TokenKind.KeywordWhile, "while");

	private static readonly Token kwWithToken = new SymbolToken(TokenKind.LastKeyword, "with");

	private static readonly Token kwYieldToken = new SymbolToken(TokenKind.KeywordYield, "yield");

	public static Token AddToken => symAddToken;

	public static Token AddEqualToken => symAddEqualToken;

	public static Token SubtractToken => symSubtractToken;

	public static Token SubtractEqualToken => symSubtractEqualToken;

	public static Token PowerToken => symPowerToken;

	public static Token PowerEqualToken => symPowerEqualToken;

	public static Token MultiplyToken => symMultiplyToken;

	public static Token MultiplyEqualToken => symMultiplyEqualToken;

	public static Token FloorDivideToken => symFloorDivideToken;

	public static Token FloorDivideEqualToken => symFloorDivideEqualToken;

	public static Token DivideToken => symDivideToken;

	public static Token DivideEqualToken => symDivideEqualToken;

	public static Token ModToken => symModToken;

	public static Token ModEqualToken => symModEqualToken;

	public static Token LeftShiftToken => symLeftShiftToken;

	public static Token LeftShiftEqualToken => symLeftShiftEqualToken;

	public static Token RightShiftToken => symRightShiftToken;

	public static Token RightShiftEqualToken => symRightShiftEqualToken;

	public static Token BitwiseAndToken => symBitwiseAndToken;

	public static Token BitwiseAndEqualToken => symBitwiseAndEqualToken;

	public static Token BitwiseOrToken => symBitwiseOrToken;

	public static Token BitwiseOrEqualToken => symBitwiseOrEqualToken;

	public static Token ExclusiveOrToken => symExclusiveOrToken;

	public static Token ExclusiveOrEqualToken => symExclusiveOrEqualToken;

	public static Token LessThanToken => symLessThanToken;

	public static Token GreaterThanToken => symGreaterThanToken;

	public static Token LessThanOrEqualToken => symLessThanOrEqualToken;

	public static Token GreaterThanOrEqualToken => symGreaterThanOrEqualToken;

	public static Token EqualsToken => symEqualsToken;

	public static Token NotEqualsToken => symNotEqualsToken;

	public static Token LessThanGreaterThanToken => symLessThanGreaterThanToken;

	public static Token LeftParenthesisToken => symLeftParenthesisToken;

	public static Token RightParenthesisToken => symRightParenthesisToken;

	public static Token LeftBracketToken => symLeftBracketToken;

	public static Token RightBracketToken => symRightBracketToken;

	public static Token LeftBraceToken => symLeftBraceToken;

	public static Token RightBraceToken => symRightBraceToken;

	public static Token CommaToken => symCommaToken;

	public static Token ColonToken => symColonToken;

	public static Token BackQuoteToken => symBackQuoteToken;

	public static Token SemicolonToken => symSemicolonToken;

	public static Token AssignToken => symAssignToken;

	public static Token TwiddleToken => symTwiddleToken;

	public static Token AtToken => symAtToken;

	public static Token KeywordAndToken => kwAndToken;

	public static Token KeywordAsToken => kwAsToken;

	public static Token KeywordAssertToken => kwAssertToken;

	public static Token KeywordBreakToken => kwBreakToken;

	public static Token KeywordClassToken => kwClassToken;

	public static Token KeywordContinueToken => kwContinueToken;

	public static Token KeywordDefToken => kwDefToken;

	public static Token KeywordDelToken => kwDelToken;

	public static Token KeywordElseIfToken => kwElseIfToken;

	public static Token KeywordElseToken => kwElseToken;

	public static Token KeywordExceptToken => kwExceptToken;

	public static Token KeywordExecToken => kwExecToken;

	public static Token KeywordFinallyToken => kwFinallyToken;

	public static Token KeywordForToken => kwForToken;

	public static Token KeywordFromToken => kwFromToken;

	public static Token KeywordGlobalToken => kwGlobalToken;

	public static Token KeywordIfToken => kwIfToken;

	public static Token KeywordImportToken => kwImportToken;

	public static Token KeywordInToken => kwInToken;

	public static Token KeywordIsToken => kwIsToken;

	public static Token KeywordLambdaToken => kwLambdaToken;

	public static Token KeywordNotToken => kwNotToken;

	public static Token KeywordOrToken => kwOrToken;

	public static Token KeywordPassToken => kwPassToken;

	public static Token KeywordPrintToken => kwPrintToken;

	public static Token KeywordRaiseToken => kwRaiseToken;

	public static Token KeywordReturnToken => kwReturnToken;

	public static Token KeywordTryToken => kwTryToken;

	public static Token KeywordWhileToken => kwWhileToken;

	public static Token KeywordWithToken => kwWithToken;

	public static Token KeywordYieldToken => kwYieldToken;
}
