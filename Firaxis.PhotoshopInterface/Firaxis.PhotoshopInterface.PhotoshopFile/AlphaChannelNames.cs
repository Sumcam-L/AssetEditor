using System.Collections.Generic;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class AlphaChannelNames : ImageResource
{
	private List<string> channelNames = new List<string>();

	public override ResourceID ID => ResourceID.AlphaChannelNames;

	public List<string> ChannelNames => channelNames;

	public AlphaChannelNames()
		: base(string.Empty)
	{
	}

	public AlphaChannelNames(PsdBinaryReader reader, string name, int resourceDataLength)
		: base(name)
	{
		long num = reader.BaseStream.Position + resourceDataLength;
		while (reader.BaseStream.Position < num)
		{
			string item = reader.ReadPascalString(1);
			ChannelNames.Add(item);
		}
	}
}
