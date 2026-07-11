namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public static class LayerInfoFactory
{
	public static LayerInfo Load(PsdBinaryReader reader)
	{
		string text = reader.ReadAsciiChars(4);
		if (text != "8BIM")
		{
			throw new PsdInvalidException("Could not read LayerInfo due to signature mismatch.");
		}
		string text2 = reader.ReadAsciiChars(4);
		int num = reader.ReadInt32();
		long position = reader.BaseStream.Position;
		LayerInfo result;
		switch (text2)
		{
		case "lsct":
		case "lsdk":
			result = new LayerSectionInfo(reader, text2, num);
			break;
		case "luni":
			result = new LayerUnicodeName(reader);
			break;
		default:
			result = new RawLayerInfo(reader, text2, num);
			break;
		}
		long num2 = position + num;
		if (reader.BaseStream.Position < num2)
		{
			reader.BaseStream.Position = num2;
		}
		reader.ReadPadding(position, 4);
		return result;
	}
}
