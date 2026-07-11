using System.Collections.Specialized;
using System.Drawing;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class MaskInfo
{
	public Mask LayerMask { get; set; }

	public Mask UserMask { get; set; }

	public MaskInfo()
	{
	}

	public MaskInfo(PsdBinaryReader reader, Layer layer)
	{
		uint num = reader.ReadUInt32();
		if (num != 0)
		{
			long position = reader.BaseStream.Position;
			long position2 = position + num;
			Rectangle rect = reader.ReadRectangle();
			byte color = reader.ReadByte();
			byte data = reader.ReadByte();
			LayerMask = new Mask(layer, rect, color, new BitVector32(data));
			if (num == 36)
			{
				byte data2 = reader.ReadByte();
				byte color2 = reader.ReadByte();
				Rectangle rect2 = reader.ReadRectangle();
				UserMask = new Mask(layer, rect2, color2, new BitVector32(data2));
			}
			reader.BaseStream.Position = position2;
		}
	}
}
