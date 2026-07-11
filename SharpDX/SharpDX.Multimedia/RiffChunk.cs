using System;
using System.Globalization;
using System.IO;

namespace SharpDX.Multimedia;

public class RiffChunk
{
	public Stream Stream { get; private set; }

	public FourCC Type { get; private set; }

	public uint Size { get; private set; }

	public uint DataPosition { get; private set; }

	public bool IsList { get; private set; }

	public bool IsHeader { get; private set; }

	public RiffChunk(Stream stream, FourCC type, uint size, uint dataPosition, bool isList = false, bool isHeader = false)
	{
		Stream = stream;
		Type = type;
		Size = size;
		DataPosition = dataPosition;
		IsList = isList;
		IsHeader = isHeader;
	}

	public byte[] GetData()
	{
		byte[] array = new byte[Size];
		Stream.Position = DataPosition;
		Stream.Read(array, 0, (int)Size);
		return array;
	}

	public unsafe T GetDataAs<T>() where T : struct
	{
		T data = default(T);
		fixed (IntPtr* data2 = GetData())
		{
			Utilities.Read((IntPtr)data2, ref data);
		}
		return data;
	}

	public unsafe T[] GetDataAsArray<T>() where T : struct
	{
		int num = Utilities.SizeOf<T>();
		if (Size % num != 0)
		{
			throw new ArgumentException("Size of T is incompatible with size of chunk");
		}
		T[] array = new T[Size / num];
		fixed (IntPtr* data = GetData())
		{
			Utilities.Read((IntPtr)data, array, 0, array.Length);
		}
		return array;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Type: {0}, Size: {1}, Position: {2}, IsList: {3}, IsHeader: {4}", Type, Size, DataPosition, IsList, IsHeader);
	}
}
