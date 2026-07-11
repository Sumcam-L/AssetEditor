using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Bespoke.Common.Osc;

public abstract class OscPacket
{
	protected static bool sLittleEndianByteOrder;

	protected IPEndPoint mSourceEndPoint;

	protected string mAddress;

	protected List<object> mData;

	protected OscClient mClient;

	private static UdpClient sUdpClient;

	public static bool LittleEndianByteOrder
	{
		get
		{
			return sLittleEndianByteOrder;
		}
		set
		{
			sLittleEndianByteOrder = value;
		}
	}

	public abstract bool IsBundle { get; }

	public IPEndPoint SourceEndPoint => mSourceEndPoint;

	public string Address
	{
		get
		{
			return mAddress;
		}
		set
		{
			Assert.IsFalse(string.IsNullOrEmpty(value));
			mAddress = value;
		}
	}

	public IList<object> Data => mData.AsReadOnly();

	public OscClient Client
	{
		get
		{
			return mClient;
		}
		set
		{
			mClient = value;
		}
	}

	static OscPacket()
	{
		sLittleEndianByteOrder = false;
		sUdpClient = new UdpClient();
	}

	public OscPacket(IPEndPoint sourceEndPoint, string address, OscClient client = null)
	{
		Assert.IsFalse(string.IsNullOrEmpty(address));
		mSourceEndPoint = sourceEndPoint;
		mAddress = address;
		mData = new List<object>();
		mClient = client;
	}

	public abstract int Append<T>(T value);

	public T At<T>(int index)
	{
		if (index > mData.Count || index < 0)
		{
			throw new IndexOutOfRangeException();
		}
		if (!(mData[index] is T))
		{
			throw new InvalidCastException();
		}
		return (T)mData[index];
	}

	public abstract byte[] ToByteArray();

	public static OscPacket FromByteArray(IPEndPoint sourceEndPoint, byte[] data)
	{
		Assert.ParamIsNotNull(data);
		int start = 0;
		return FromByteArray(sourceEndPoint, data, ref start, data.Length);
	}

	public static OscPacket FromByteArray(IPEndPoint sourceEndPoint, byte[] data, ref int start, int end)
	{
		if (data[start] == 35)
		{
			return OscBundle.FromByteArray(sourceEndPoint, data, ref start, end);
		}
		return OscMessage.FromByteArray(sourceEndPoint, data, ref start);
	}

	public static void Send(OscPacket packet, IPEndPoint destination)
	{
		packet.Send(destination);
	}

	public static void Send(OscPacket packet, OscClient client)
	{
		client.Send(packet);
	}

	public void Send(IPEndPoint destination)
	{
		byte[] array = ToByteArray();
		sUdpClient.Send(array, array.Length, destination);
	}

	public void Send(IPEndPoint source, IPEndPoint destination)
	{
		using UdpClient udpClient = new UdpClient(source);
		byte[] array = ToByteArray();
		udpClient.Send(array, array.Length, destination);
	}

	public void Send()
	{
		Assert.ParamIsNotNull(mClient);
		mClient.Send(this);
	}

	public static T ValueFromByteArray<T>(byte[] data, ref int start)
	{
		Type typeFromHandle = typeof(T);
		object obj;
		switch (typeFromHandle.Name)
		{
		case "String":
		{
			int num = 0;
			for (int i = start; i < data.Length && data[i] != 0; i++)
			{
				num++;
			}
			obj = Encoding.ASCII.GetString(data, start, num);
			start += num + 1;
			start = (start + 3) / 4 * 4;
			break;
		}
		case "Byte[]":
		{
			int length2 = ValueFromByteArray<int>(data, ref start);
			byte[] array3 = data.CopySubArray(start, length2);
			obj = array3;
			start += array3.Length + 1;
			start = (start + 3) / 4 * 4;
			break;
		}
		case "OscTimeTag":
		{
			byte[] array2 = data.CopySubArray(start, 8);
			obj = new OscTimeTag(array2);
			start += array2.Length;
			break;
		}
		case "Char":
			obj = Convert.ToChar(ValueFromByteArray<int>(data, ref start));
			break;
		case "Color":
		{
			byte[] array4 = data.CopySubArray(start, 4);
			start += array4.Length;
			obj = Color.FromArgb(array4[3], array4[0], array4[1], array4[2]);
			break;
		}
		default:
		{
			int length;
			switch (typeFromHandle.Name)
			{
			case "Int32":
			case "Single":
				length = 4;
				break;
			case "Int64":
			case "Double":
				length = 8;
				break;
			default:
				throw new Exception("Unsupported data type.");
			}
			byte[] array = data.CopySubArray(start, length);
			start += array.Length;
			if (BitConverter.IsLittleEndian != sLittleEndianByteOrder)
			{
				array = Utility.SwapEndian(array);
			}
			obj = typeFromHandle.Name switch
			{
				"Int32" => BitConverter.ToInt32(array, 0), 
				"Int64" => BitConverter.ToInt64(array, 0), 
				"Single" => BitConverter.ToSingle(array, 0), 
				"Double" => BitConverter.ToDouble(array, 0), 
				_ => throw new Exception("Unsupported data type."), 
			};
			break;
		}
		}
		return (T)obj;
	}

	public static byte[] ValueToByteArray<T>(T value)
	{
		byte[] array = null;
		object obj = value;
		if (obj != null)
		{
			Type type = value.GetType();
			switch (type.Name)
			{
			case "Int32":
				array = BitConverter.GetBytes((int)obj);
				if (BitConverter.IsLittleEndian != sLittleEndianByteOrder)
				{
					array = Utility.SwapEndian(array);
				}
				break;
			case "Int64":
				array = BitConverter.GetBytes((long)obj);
				if (BitConverter.IsLittleEndian != sLittleEndianByteOrder)
				{
					array = Utility.SwapEndian(array);
				}
				break;
			case "Single":
			{
				float num = (float)obj;
				if (!float.IsPositiveInfinity(num))
				{
					array = BitConverter.GetBytes(num);
					if (BitConverter.IsLittleEndian != sLittleEndianByteOrder)
					{
						array = Utility.SwapEndian(array);
					}
				}
				break;
			}
			case "Double":
				array = BitConverter.GetBytes((double)obj);
				if (BitConverter.IsLittleEndian != sLittleEndianByteOrder)
				{
					array = Utility.SwapEndian(array);
				}
				break;
			case "String":
				array = Encoding.ASCII.GetBytes((string)obj);
				break;
			case "Byte[]":
			{
				byte[] array3 = (byte[])obj;
				List<byte> list = new List<byte>();
				list.AddRange(ValueToByteArray(array3.Length));
				list.AddRange(array3);
				array = list.ToArray();
				break;
			}
			case "OscTimeTag":
				array = ((OscTimeTag)obj).ToByteArray();
				break;
			case "Char":
				array = ValueToByteArray(Convert.ToInt32((char)obj));
				break;
			case "Color":
			{
				Color color = (Color)obj;
				byte[] array2 = new byte[4] { color.R, color.G, color.B, color.A };
				array = array2;
				break;
			}
			default:
				throw new Exception("Unsupported data type.");
			case "Boolean":
				break;
			}
		}
		return array;
	}

	public static void PadNull(List<byte> data)
	{
		byte item = 0;
		int num = 4 - data.Count % 4;
		for (int i = 0; i < num; i++)
		{
			data.Add(item);
		}
	}
}
