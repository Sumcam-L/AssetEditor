namespace Firaxis.ATF;

public class ModuleInfo
{
	public uint age { get; set; }

	public ulong baseAddr { get; set; }

	public string file { get; set; }

	public uint guid1 { get; set; }

	public ushort guid2 { get; set; }

	public ushort guid3 { get; set; }

	public byte[] guid4 { get; set; }

	public string pdbPath { get; set; }

	public ulong size { get; set; }

	public ModuleInfo()
	{
		guid4 = new byte[8];
	}
}
