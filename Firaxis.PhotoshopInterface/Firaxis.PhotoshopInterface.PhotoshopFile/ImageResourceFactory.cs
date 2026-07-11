namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public static class ImageResourceFactory
{
	public static ImageResource CreateImageResource(PsdBinaryReader reader)
	{
		string signature = reader.ReadAsciiChars(4);
		ushort num = reader.ReadUInt16();
		string name = reader.ReadPascalString(2);
		int num2 = (int)reader.ReadUInt32();
		int num3 = Util.RoundUp(num2, 2);
		long num4 = reader.BaseStream.Position + num3;
		ImageResource imageResource = null;
		ResourceID resourceID = (ResourceID)num;
		switch (resourceID)
		{
		case ResourceID.ResolutionInfo:
			imageResource = new ResolutionInfo(reader, name);
			break;
		case ResourceID.ThumbnailBgr:
		case ResourceID.ThumbnailRgb:
			imageResource = new Thumbnail(reader, resourceID, name, num2);
			break;
		case ResourceID.AlphaChannelNames:
			imageResource = new AlphaChannelNames(reader, name, num2);
			break;
		case ResourceID.UnicodeAlphaNames:
			imageResource = new UnicodeAlphaNames(reader, name, num2);
			break;
		case ResourceID.VersionInfo:
			imageResource = new VersionInfo(reader, name);
			break;
		default:
			imageResource = new RawImageResource(reader, signature, resourceID, name, num2);
			break;
		}
		if (reader.BaseStream.Position < num4)
		{
			reader.BaseStream.Position = num4;
		}
		if (reader.BaseStream.Position > num4)
		{
			throw new PsdInvalidException("Corruption detected in resource.");
		}
		return imageResource;
	}
}
