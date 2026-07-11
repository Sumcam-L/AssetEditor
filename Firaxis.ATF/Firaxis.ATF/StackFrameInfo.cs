namespace Firaxis.ATF;

public class StackFrameInfo
{
	public string File { get; set; }

	public int Line { get; set; }

	public string Method { get; set; }

	public StackFrameInfo()
		: this(string.Empty, 0, string.Empty)
	{
	}

	public StackFrameInfo(string file, int line, string method)
	{
		File = file;
		Line = line;
		Method = method;
	}
}
