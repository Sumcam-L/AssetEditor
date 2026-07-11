using System.Collections.Specialized;
using System.Drawing;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class Mask
{
	private byte backgroundColor;

	private static int positionVsLayerBit = BitVector32.CreateMask();

	private static int disabledBit = BitVector32.CreateMask(positionVsLayerBit);

	private static int invertOnBlendBit = BitVector32.CreateMask(disabledBit);

	private BitVector32 flags;

	public Layer Layer { get; private set; }

	public Rectangle Rect { get; set; }

	public byte BackgroundColor
	{
		get
		{
			return backgroundColor;
		}
		set
		{
			if (value != 0 && value != byte.MaxValue)
			{
				throw new PsdInvalidException("Mask background must be fully-opaque or fully-transparent.");
			}
			backgroundColor = value;
		}
	}

	public BitVector32 Flags => flags;

	public bool PositionVsLayer
	{
		get
		{
			return flags[positionVsLayerBit];
		}
		set
		{
			flags[positionVsLayerBit] = value;
		}
	}

	public bool Disabled
	{
		get
		{
			return flags[disabledBit];
		}
		set
		{
			flags[disabledBit] = value;
		}
	}

	public bool InvertOnBlend
	{
		get
		{
			return flags[invertOnBlendBit];
		}
		set
		{
			flags[invertOnBlendBit] = value;
		}
	}

	public byte[] ImageData { get; set; }

	public Mask(Layer layer)
	{
		Layer = layer;
		flags = default(BitVector32);
	}

	public Mask(Layer layer, Rectangle rect, byte color, BitVector32 flags)
	{
		Layer = layer;
		Rect = rect;
		BackgroundColor = color;
		this.flags = flags;
	}
}
