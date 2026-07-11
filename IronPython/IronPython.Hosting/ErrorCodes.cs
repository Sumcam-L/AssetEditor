namespace IronPython.Hosting;

public static class ErrorCodes
{
	public const int IncompleteMask = 15;

	public const int IncompleteStatement = 1;

	public const int IncompleteToken = 2;

	public const int ErrorMask = 2147483632;

	public const int SyntaxError = 16;

	public const int IndentationError = 32;

	public const int TabError = 48;

	public const int NoCaret = 64;
}
