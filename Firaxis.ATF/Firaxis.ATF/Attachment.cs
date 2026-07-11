namespace Firaxis.ATF;

public class Attachment
{
	public string File { get; set; }

	public string MD5 { get; set; }

	public Attachment()
		: this(string.Empty, string.Empty)
	{
	}

	public Attachment(string file, string md5)
	{
		File = file;
		MD5 = md5;
	}
}
