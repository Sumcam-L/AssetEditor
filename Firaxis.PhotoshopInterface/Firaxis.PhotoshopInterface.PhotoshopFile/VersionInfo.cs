namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class VersionInfo : ImageResource
{
	public override ResourceID ID => ResourceID.VersionInfo;

	public uint Version { get; set; }

	public bool HasRealMergedData { get; set; }

	public string ReaderName { get; set; }

	public string WriterName { get; set; }

	public uint FileVersion { get; set; }

	public VersionInfo()
		: base(string.Empty)
	{
	}

	public VersionInfo(PsdBinaryReader reader, string name)
		: base(name)
	{
		Version = reader.ReadUInt32();
		HasRealMergedData = reader.ReadBoolean();
		ReaderName = reader.ReadUnicodeString();
		WriterName = reader.ReadUnicodeString();
		FileVersion = reader.ReadUInt32();
	}
}
