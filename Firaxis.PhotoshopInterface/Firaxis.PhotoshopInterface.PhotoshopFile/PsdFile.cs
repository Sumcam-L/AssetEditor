using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class PsdFile
{
	private class DecompressChannelContext
	{
		private Channel ch;

		public DecompressChannelContext(Channel ch)
		{
			this.ch = ch;
		}

		public void DecompressChannel(object context)
		{
			ch.DecompressImageData();
		}
	}

	private short channelCount;

	private int bitDepth;

	public byte[] ColorModeData = new byte[0];

	private byte[] GlobalLayerMaskData = new byte[0];

	public Layer BaseLayer { get; set; }

	public ImageCompression ImageCompression { get; set; }

	public short Version { get; private set; }

	public short ChannelCount
	{
		get
		{
			return channelCount;
		}
		set
		{
			if (value < 1 || value > 56)
			{
				throw new ArgumentException("Number of channels must be from 1 to 56.");
			}
			channelCount = value;
		}
	}

	public int RowCount
	{
		get
		{
			return BaseLayer.Rect.Height;
		}
		set
		{
			if (value < 0 || value > 30000)
			{
				throw new ArgumentException("Number of rows must be from 1 to 30000.");
			}
			BaseLayer.Rect = new Rectangle(0, 0, BaseLayer.Rect.Width, value);
		}
	}

	public int ColumnCount
	{
		get
		{
			return BaseLayer.Rect.Width;
		}
		set
		{
			if (value < 0 || value > 30000)
			{
				throw new ArgumentException("Number of columns must be from 1 to 30000.");
			}
			BaseLayer.Rect = new Rectangle(0, 0, value, BaseLayer.Rect.Height);
		}
	}

	public int BitDepth
	{
		get
		{
			return bitDepth;
		}
		set
		{
			switch (value)
			{
			case 1:
			case 8:
			case 16:
			case 32:
				bitDepth = value;
				break;
			default:
				throw new NotImplementedException("Invalid bit depth.");
			}
		}
	}

	public PsdColorMode ColorMode { get; set; }

	public ImageResources ImageResources { get; set; }

	public ResolutionInfo Resolution
	{
		get
		{
			return (ResolutionInfo)ImageResources.Get(ResourceID.ResolutionInfo);
		}
		set
		{
			ImageResources.Set(value);
		}
	}

	public List<Layer> Layers { get; private set; }

	public List<LayerInfo> AdditionalInfo { get; private set; }

	public bool AbsoluteAlpha { get; set; }

	public PsdFile()
	{
		Version = 1;
		BaseLayer = new Layer(this);
		BaseLayer.Rect = new Rectangle(0, 0, 0, 0);
		ImageResources = new ImageResources();
		Layers = new List<Layer>();
		AdditionalInfo = new List<LayerInfo>();
	}

	public void Load(string fileName, Encoding encoding)
	{
		using FileStream stream = new FileStream(fileName, FileMode.Open);
		Load(stream, encoding);
	}

	public void Load(Stream stream, Encoding encoding)
	{
		using PsdBinaryReader reader = new PsdBinaryReader(stream, encoding);
		LoadHeader(reader);
		LoadColorModeData(reader);
		LoadImageResources(reader);
		LoadLayerAndMaskInfo(reader);
	}

	private void LoadHeader(PsdBinaryReader reader)
	{
		string text = reader.ReadAsciiChars(4);
		if (text != "8BPS")
		{
			throw new PsdInvalidException("The given stream is not a valid PSD file");
		}
		Version = reader.ReadInt16();
		if (Version != 1)
		{
			throw new PsdInvalidException("The PSD file has an unknown version");
		}
		reader.BaseStream.Position += 6L;
		ChannelCount = reader.ReadInt16();
		RowCount = reader.ReadInt32();
		ColumnCount = reader.ReadInt32();
		BitDepth = reader.ReadInt16();
		ColorMode = (PsdColorMode)reader.ReadInt16();
	}

	private void LoadColorModeData(PsdBinaryReader reader)
	{
		uint num = reader.ReadUInt32();
		if (num != 0)
		{
			ColorModeData = reader.ReadBytes((int)num);
		}
	}

	private void LoadImageResources(PsdBinaryReader reader)
	{
		ImageResources.Clear();
		uint num = reader.ReadUInt32();
		if (num != 0)
		{
			long position = reader.BaseStream.Position;
			long num2 = position + num;
			while (reader.BaseStream.Position < num2)
			{
				ImageResource item = ImageResourceFactory.CreateImageResource(reader);
				ImageResources.Add(item);
			}
			reader.BaseStream.Position = position + num;
		}
	}

	private void LoadLayerAndMaskInfo(PsdBinaryReader reader)
	{
		uint num = reader.ReadUInt32();
		if (num == 0)
		{
			return;
		}
		long position = reader.BaseStream.Position;
		long num2 = position + num;
		LoadLayers(reader, hasHeader: true);
		LoadGlobalLayerMask(reader);
		while (reader.BaseStream.Position < num2)
		{
			LayerInfo layerInfo = LayerInfoFactory.Load(reader);
			AdditionalInfo.Add(layerInfo);
			if (!(layerInfo is RawLayerInfo))
			{
				continue;
			}
			RawLayerInfo rawLayerInfo = (RawLayerInfo)layerInfo;
			switch (layerInfo.Key)
			{
			case "Layr":
			case "Lr16":
			case "Lr32":
			{
				using (MemoryStream stream = new MemoryStream(rawLayerInfo.Data))
				{
					using PsdBinaryReader reader2 = new PsdBinaryReader(stream, reader);
					LoadLayers(reader2, hasHeader: false);
				}
				break;
			}
			case "LMsk":
				GlobalLayerMaskData = rawLayerInfo.Data;
				break;
			}
		}
		reader.BaseStream.Position = position + num;
	}

	private void LoadLayers(PsdBinaryReader reader, bool hasHeader)
	{
		uint num = 0u;
		if (hasHeader)
		{
			num = reader.ReadUInt32();
			if (num == 0)
			{
				return;
			}
		}
		long position = reader.BaseStream.Position;
		short num2 = reader.ReadInt16();
		if (num2 < 0)
		{
			AbsoluteAlpha = true;
			num2 = Math.Abs(num2);
		}
		Layers.Clear();
		if (num2 == 0)
		{
			return;
		}
		for (int i = 0; i < num2; i++)
		{
			Layer item = new Layer(reader, this);
			Layers.Add(item);
		}
		foreach (Layer layer in Layers)
		{
			foreach (Channel channel in layer.Channels)
			{
				channel.LoadPixelData(reader);
			}
		}
		if (num != 0)
		{
			long num3 = position + num;
			long num4 = reader.BaseStream.Position - num3;
			if (reader.BaseStream.Position < num3)
			{
				reader.BaseStream.Position = num3;
			}
		}
	}

	private void DecompressImages()
	{
		IEnumerable<Layer> enumerable = Layers.Concat(new List<Layer> { BaseLayer });
		foreach (Layer item in enumerable)
		{
			foreach (Channel channel in item.Channels)
			{
				DecompressChannelContext decompressChannelContext = new DecompressChannelContext(channel);
				decompressChannelContext.DecompressChannel(null);
				WaitCallback waitCallback = decompressChannelContext.DecompressChannel;
			}
		}
		foreach (Layer layer in Layers)
		{
			foreach (Channel channel2 in layer.Channels)
			{
				if (channel2.ID == -2)
				{
					layer.Masks.LayerMask.ImageData = channel2.ImageData;
				}
				else if (channel2.ID == -3)
				{
					layer.Masks.UserMask.ImageData = channel2.ImageData;
				}
			}
		}
	}

	public void VerifyLayerSections()
	{
		int num = 0;
		foreach (Layer item in Enumerable.Reverse(Layers))
		{
			LayerInfo layerInfo = item.AdditionalInfo.SingleOrDefault((LayerInfo x) => x is LayerSectionInfo);
			if (layerInfo == null)
			{
				continue;
			}
			LayerSectionInfo layerSectionInfo = (LayerSectionInfo)layerInfo;
			switch (layerSectionInfo.SectionType)
			{
			case LayerSectionType.OpenFolder:
			case LayerSectionType.ClosedFolder:
				num++;
				break;
			case LayerSectionType.SectionDivider:
				num--;
				if (num < 0)
				{
					throw new PsdInvalidException("Layer section ended without matching start marker.");
				}
				break;
			default:
				throw new PsdInvalidException("Unrecognized layer section type.");
			}
		}
		if (num != 0)
		{
			throw new PsdInvalidException("Layer section not closed by end marker.");
		}
	}

	public void SetVersionInfo()
	{
		VersionInfo versionInfo = (VersionInfo)ImageResources.Get(ResourceID.VersionInfo);
		if (versionInfo == null)
		{
			versionInfo = new VersionInfo();
			ImageResources.Set(versionInfo);
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Version version = executingAssembly.GetName().Version;
			string text = version.Major + "." + version.Minor + "." + version.Build;
			versionInfo.Version = 1u;
			versionInfo.HasRealMergedData = true;
			versionInfo.ReaderName = "Paint.NET PSD Plugin";
			versionInfo.WriterName = "Paint.NET PSD Plugin " + text;
			versionInfo.FileVersion = 1u;
		}
	}

	private void LoadGlobalLayerMask(PsdBinaryReader reader)
	{
		uint num = reader.ReadUInt32();
		if (num != 0)
		{
			GlobalLayerMaskData = reader.ReadBytes((int)num);
		}
	}

	private void LoadImage(PsdBinaryReader reader)
	{
		BaseLayer.Rect = new Rectangle(0, 0, ColumnCount, RowCount);
		ImageCompression = (ImageCompression)reader.ReadInt16();
		switch (ImageCompression)
		{
		case ImageCompression.Raw:
		{
			int num3 = RowCount * Util.BytesPerRow(BaseLayer.Rect, BitDepth);
			for (short num4 = 0; num4 < ChannelCount; num4++)
			{
				Channel channel2 = new Channel(num4, BaseLayer);
				channel2.ImageCompression = ImageCompression;
				channel2.Length = num3;
				channel2.ImageData = reader.ReadBytes(num3);
				BaseLayer.Channels.Add(channel2);
			}
			break;
		}
		case ImageCompression.Rle:
		{
			for (short num = 0; num < ChannelCount; num++)
			{
				Channel channel = new Channel(num, BaseLayer);
				channel.RleHeader = reader.ReadBytes(2 * RowCount);
				int num2 = 0;
				using (MemoryStream stream = new MemoryStream(channel.RleHeader))
				{
					using PsdBinaryReader psdBinaryReader = new PsdBinaryReader(stream, Encoding.ASCII);
					for (int i = 0; i < RowCount; i++)
					{
						num2 += psdBinaryReader.ReadUInt16();
					}
				}
				channel.ImageCompression = ImageCompression;
				channel.Length = num2;
				BaseLayer.Channels.Add(channel);
			}
			foreach (Channel channel4 in BaseLayer.Channels)
			{
				channel4.Data = reader.ReadBytes(channel4.Length);
			}
			break;
		}
		}
		if (ChannelCount == ColorMode.ChannelCount() + 1)
		{
			Channel channel3 = BaseLayer.Channels.Last();
			channel3.ID = -1;
		}
	}
}
