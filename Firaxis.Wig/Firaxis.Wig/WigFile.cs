namespace Firaxis.Wig;

public class WigFile
{
	public int exporterVersion;

	public string sourceProgam;

	public string sourceFile;

	public HairData hairData;

	public WigFile()
	{
		exporterVersion = 0;
		sourceProgam = string.Empty;
		sourceFile = string.Empty;
		hairData = new HairData();
	}
}
