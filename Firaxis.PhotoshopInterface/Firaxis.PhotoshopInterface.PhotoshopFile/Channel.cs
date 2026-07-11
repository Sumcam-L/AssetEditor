using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

[DebuggerDisplay("ID = {ID}")]
public class Channel
{
	private byte[] data;

	private bool dataDecompressed;

	private byte[] imageData;

	public Layer Layer { get; private set; }

	public short ID { get; set; }

	public Rectangle Rect => ID switch
	{
		-2 => Layer.Masks.LayerMask.Rect, 
		-3 => Layer.Masks.UserMask.Rect, 
		_ => Layer.Rect, 
	};

	public int Length { get; set; }

	public byte[] Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
			dataDecompressed = false;
			imageData = null;
		}
	}

	public byte[] ImageData
	{
		get
		{
			return imageData;
		}
		set
		{
			imageData = value;
			data = null;
			dataDecompressed = true;
		}
	}

	public ImageCompression ImageCompression { get; set; }

	public byte[] RleHeader { get; set; }

	internal Channel(short id, Layer layer)
	{
		ID = id;
		Layer = layer;
	}

	internal Channel(PsdBinaryReader reader, Layer layer)
	{
		ID = reader.ReadInt16();
		Length = reader.ReadInt32();
		Layer = layer;
	}

	internal void LoadPixelData(PsdBinaryReader reader)
	{
		long num = reader.BaseStream.Position + Length;
		ImageCompression = (ImageCompression)reader.ReadInt16();
		int num2 = Length - 2;
		switch (ImageCompression)
		{
		case ImageCompression.Raw:
			ImageData = reader.ReadBytes(num2);
			break;
		case ImageCompression.Rle:
		{
			RleHeader = reader.ReadBytes(2 * Rect.Height);
			int count = num2 - 2 * Rect.Height;
			Data = reader.ReadBytes(count);
			break;
		}
		case ImageCompression.Zip:
		case ImageCompression.ZipPrediction:
			Data = reader.ReadBytes(num2);
			break;
		}
	}

	public void DecompressImageData()
	{
		if (dataDecompressed)
		{
			return;
		}
		Rectangle rect = Rect;
		int num = Util.BytesPerRow(rect, Layer.PsdFile.BitDepth);
		int num2 = rect.Height * num;
		if (ImageCompression != ImageCompression.Raw)
		{
			imageData = new byte[num2];
			MemoryStream memoryStream = new MemoryStream(Data);
			switch (ImageCompression)
			{
			case ImageCompression.Rle:
			{
				RleReader rleReader = new RleReader(memoryStream);
				for (int i = 0; i < rect.Height; i++)
				{
					int offset = i * num;
					rleReader.Read(imageData, offset, num);
				}
				break;
			}
			case ImageCompression.Zip:
			case ImageCompression.ZipPrediction:
			{
				memoryStream.ReadByte();
				memoryStream.ReadByte();
				DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
				int num3 = deflateStream.Read(imageData, 0, num2);
				break;
			}
			}
		}
		if (Layer.PsdFile.BitDepth == 16 || (Layer.PsdFile.BitDepth == 32 && ImageCompression != ImageCompression.ZipPrediction))
		{
			ReverseEndianness(imageData, rect);
		}
		if (ImageCompression == ImageCompression.ZipPrediction)
		{
			UnpredictImageData(rect);
		}
		dataDecompressed = true;
	}

	private void ReverseEndianness(byte[] buffer, Rectangle rect)
	{
		int num = Util.BytesFromBitDepth(Layer.PsdFile.BitDepth);
		int num2 = rect.Width * rect.Height;
		if (num2 != 0)
		{
			if (num == 2)
			{
				Util.SwapByteArray2(buffer, 0, num2);
			}
			else if (num == 4)
			{
				Util.SwapByteArray4(buffer, 0, num2);
			}
			else if (num > 1)
			{
				throw new NotImplementedException("Byte-swapping implemented only for 16-bit and 32-bit depths.");
			}
		}
	}

	private unsafe void UnpredictImageData(Rectangle rect)
	{
		if (Layer.PsdFile.BitDepth == 16)
		{
			byte[] array = new byte[imageData.Length];
			fixed (byte* ptr = &imageData[0])
			{
				for (int i = 0; i < rect.Height; i++)
				{
					ushort* ptr2 = (ushort*)(ptr + i * rect.Width * 2);
					ushort* ptr3 = (ushort*)(ptr + (i + 1) * rect.Width * 2);
					for (ptr2++; ptr2 < ptr3; ptr2++)
					{
						*ptr2 += *(ptr2 - 1);
					}
				}
			}
			return;
		}
		if (Layer.PsdFile.BitDepth == 32)
		{
			byte[] array2 = new byte[imageData.Length];
			fixed (byte* ptr4 = &imageData[0])
			{
				for (int j = 0; j < rect.Height; j++)
				{
					byte* ptr5 = ptr4 + j * rect.Width * 4;
					byte* ptr6 = ptr4 + (j + 1) * rect.Width * 4;
					for (ptr5++; ptr5 < ptr6; ptr5++)
					{
						*ptr5 += *(ptr5 - 1);
					}
				}
				int width = rect.Width;
				int num = 2 * width;
				int num2 = 3 * width;
				fixed (byte* ptr7 = &array2[0])
				{
					for (int k = 0; k < rect.Height; k++)
					{
						byte* ptr8 = ptr7 + k * rect.Width * 4;
						byte* ptr9 = ptr7 + (k + 1) * rect.Width * 4;
						byte* ptr10 = ptr4 + k * rect.Width * 4;
						while (ptr8 < ptr9)
						{
							*(ptr8++) = ptr10[num2];
							*(ptr8++) = ptr10[num];
							*(ptr8++) = ptr10[width];
							*(ptr8++) = *ptr10;
							ptr10++;
						}
					}
				}
			}
			imageData = array2;
			return;
		}
		throw new PsdInvalidException("ZIP with prediction is only available for 16 and 32 bit depths.");
	}
}
