using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class Thumbnail : RawImageResource
{
	public Bitmap Image { get; private set; }

	public Thumbnail(ResourceID id, string name)
		: base(id, name)
	{
	}

	public Thumbnail(PsdBinaryReader psdReader, ResourceID id, string name, int numBytes)
		: base(psdReader, "8BIM", id, name, numBytes)
	{
		using MemoryStream stream = new MemoryStream(base.Data);
		using PsdBinaryReader psdBinaryReader = new PsdBinaryReader(stream, psdReader);
		uint num = psdBinaryReader.ReadUInt32();
		uint width = psdBinaryReader.ReadUInt32();
		uint height = psdBinaryReader.ReadUInt32();
		uint num2 = psdBinaryReader.ReadUInt32();
		uint num3 = psdBinaryReader.ReadUInt32();
		uint num4 = psdBinaryReader.ReadUInt32();
		ushort num5 = psdBinaryReader.ReadUInt16();
		ushort num6 = psdBinaryReader.ReadUInt16();
		switch (num)
		{
		case 0u:
			Image = new Bitmap((int)width, (int)height, PixelFormat.Format24bppRgb);
			break;
		case 1u:
		{
			byte[] buffer = psdBinaryReader.ReadBytes(numBytes - 28);
			using (MemoryStream stream2 = new MemoryStream(buffer))
			{
				Bitmap bitmap = new Bitmap(stream2);
				Image = (Bitmap)bitmap.Clone();
			}
			if (id != ResourceID.ThumbnailBgr)
			{
			}
			break;
		}
		default:
			throw new PsdInvalidException("Unknown thumbnail format.");
		}
	}
}
