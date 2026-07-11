namespace SharpDX;

public class CompilationException : SharpDXException
{
	public CompilationException(string message)
		: base(message)
	{
	}

	public CompilationException(Result errorCode, string message)
		: base(errorCode, message)
	{
	}
}
