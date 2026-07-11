using System.Collections.Generic;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class UnicodeAlphaNames : ImageResource
{
	private List<string> channelNames = new List<string>();

	public override ResourceID ID => ResourceID.UnicodeAlphaNames;

	public List<string> ChannelNames => channelNames;

	public UnicodeAlphaNames()
		: base(string.Empty)
	{
	}

	public UnicodeAlphaNames(PsdBinaryReader reader, string name, int resourceDataLength)
		: base(name)
	{
		long num = reader.BaseStream.Position + resourceDataLength;
		while (reader.BaseStream.Position < num)
		{
			string item = reader.ReadUnicodeString();
			ChannelNames.Add(item);
		}
	}
}
