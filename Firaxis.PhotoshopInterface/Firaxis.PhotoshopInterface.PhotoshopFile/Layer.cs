using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

[DebuggerDisplay("Name = {Name}")]
public class Layer
{
	private string blendModeKey;

	private static int protectTransBit = BitVector32.CreateMask();

	private static int visibleBit = BitVector32.CreateMask(protectTransBit);

	private BitVector32 flags = default(BitVector32);

	internal PsdFile PsdFile { get; private set; }

	public Rectangle Rect { get; set; }

	public ChannelList Channels { get; private set; }

	public Channel AlphaChannel
	{
		get
		{
			if (Channels.ContainsId(-1))
			{
				return Channels.GetId(-1);
			}
			return null;
		}
	}

	public string BlendModeKey
	{
		get
		{
			return blendModeKey;
		}
		set
		{
			if (value.Length != 4)
			{
				throw new ArgumentException("Key length must be 4");
			}
			blendModeKey = value;
		}
	}

	public byte Opacity { get; set; }

	public bool Clipping { get; set; }

	public bool Visible
	{
		get
		{
			return !flags[visibleBit];
		}
		set
		{
			flags[visibleBit] = !value;
		}
	}

	public bool ProtectTrans
	{
		get
		{
			return flags[protectTransBit];
		}
		set
		{
			flags[protectTransBit] = value;
		}
	}

	public string Name { get; set; }

	public BlendingRanges BlendingRangesData { get; set; }

	public MaskInfo Masks { get; set; }

	public List<LayerInfo> AdditionalInfo { get; set; }

	public Layer(PsdFile psdFile)
	{
		PsdFile = psdFile;
		Rect = Rectangle.Empty;
		Channels = new ChannelList();
		BlendModeKey = "norm";
		AdditionalInfo = new List<LayerInfo>();
	}

	public Layer(PsdBinaryReader reader, PsdFile psdFile)
		: this(psdFile)
	{
		Rect = reader.ReadRectangle();
		int num = reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			Channel item = new Channel(reader, this);
			Channels.Add(item);
		}
		string text = reader.ReadAsciiChars(4);
		if (text != "8BIM")
		{
			throw new PsdInvalidException("Invalid signature in layer header.");
		}
		BlendModeKey = reader.ReadAsciiChars(4);
		Opacity = reader.ReadByte();
		Clipping = reader.ReadBoolean();
		byte data = reader.ReadByte();
		flags = new BitVector32(data);
		reader.ReadByte();
		uint num2 = reader.ReadUInt32();
		long position = reader.BaseStream.Position;
		Masks = new MaskInfo(reader, this);
		BlendingRangesData = new BlendingRanges(reader, this);
		Name = reader.ReadPascalString(4);
		long num3 = position + num2;
		while (reader.BaseStream.Position < num3)
		{
			LayerInfo item2 = LayerInfoFactory.Load(reader);
			AdditionalInfo.Add(item2);
		}
		foreach (LayerInfo item3 in AdditionalInfo)
		{
			string key = item3.Key;
			if (key == "luni")
			{
				Name = ((LayerUnicodeName)item3).Name;
			}
		}
	}

	public unsafe void CreateMissingChannels()
	{
		short num = PsdFile.ColorMode.ChannelCount();
		for (short num2 = 0; num2 < num; num2++)
		{
			if (!Channels.ContainsId(num2))
			{
				int num3 = Rect.Height * Rect.Width;
				Channel channel = new Channel(num2, this);
				channel.ImageData = new byte[num3];
				fixed (byte* ptr = &channel.ImageData[0])
				{
					Util.Fill(ptr, ptr + num3, byte.MaxValue);
				}
				Channels.Add(channel);
			}
		}
	}
}
