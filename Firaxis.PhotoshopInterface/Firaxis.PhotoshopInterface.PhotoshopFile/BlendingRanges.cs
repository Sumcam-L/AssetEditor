namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class BlendingRanges
{
	public Layer Layer { get; private set; }

	public byte[] Data { get; set; }

	public BlendingRanges(Layer layer)
	{
		Layer = layer;
		Data = new byte[0];
	}

	public BlendingRanges(PsdBinaryReader reader, Layer layer)
	{
		Layer = layer;
		int num = reader.ReadInt32();
		if (num > 0)
		{
			Data = reader.ReadBytes(num);
		}
	}
}
