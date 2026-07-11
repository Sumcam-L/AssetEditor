using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;

namespace Bespoke.Common.Osc;

public class OscMessage : OscPacket
{
	protected const string AddressPrefix = "/";

	protected const char DefaultTag = ',';

	protected const char IntegerTag = 'i';

	protected const char FloatTag = 'f';

	protected const char LongTag = 'h';

	protected const char DoubleTag = 'd';

	protected const char StringTag = 's';

	protected const char SymbolTag = 'S';

	protected const char BlobTag = 'b';

	protected const char TimeTag = 't';

	protected const char CharacterTag = 'c';

	protected const char ColorTag = 'r';

	protected const char TrueTag = 'T';

	protected const char FalseTag = 'F';

	protected const char NilTag = 'N';

	protected const char InfinitumTag = 'I';

	private string mTypeTag;

	public override bool IsBundle => false;

	public OscMessage(IPEndPoint sourceEndPoint, string address, object value)
		: this(sourceEndPoint, address)
	{
		Append(value);
	}

	public OscMessage(IPEndPoint sourceEndPoint, string address, OscClient client = null)
		: base(sourceEndPoint, address, client)
	{
		Assert.IsTrue(address.StartsWith("/"));
		mTypeTag = ','.ToString();
	}

	public override byte[] ToByteArray()
	{
		List<byte> list = new List<byte>();
		list.AddRange(OscPacket.ValueToByteArray(mAddress));
		OscPacket.PadNull(list);
		list.AddRange(OscPacket.ValueToByteArray(mTypeTag));
		OscPacket.PadNull(list);
		foreach (object mDatum in mData)
		{
			byte[] array = OscPacket.ValueToByteArray(mDatum);
			if (array != null)
			{
				list.AddRange(array);
				if (mDatum is string || mDatum is byte[])
				{
					OscPacket.PadNull(list);
				}
			}
		}
		return list.ToArray();
	}

	public static OscMessage FromByteArray(IPEndPoint sourceEndPoint, byte[] data, ref int start)
	{
		string address = OscPacket.ValueFromByteArray<string>(data, ref start);
		OscMessage oscMessage = new OscMessage(sourceEndPoint, address);
		char[] array = OscPacket.ValueFromByteArray<string>(data, ref start).ToCharArray();
		char[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			object value;
			switch (array2[i])
			{
			case 'i':
				value = OscPacket.ValueFromByteArray<int>(data, ref start);
				break;
			case 'h':
				value = OscPacket.ValueFromByteArray<long>(data, ref start);
				break;
			case 'f':
				value = OscPacket.ValueFromByteArray<float>(data, ref start);
				break;
			case 'd':
				value = OscPacket.ValueFromByteArray<double>(data, ref start);
				break;
			case 'S':
			case 's':
				value = OscPacket.ValueFromByteArray<string>(data, ref start);
				break;
			case 'b':
				value = OscPacket.ValueFromByteArray<byte[]>(data, ref start);
				break;
			case 't':
				value = OscPacket.ValueFromByteArray<OscTimeTag>(data, ref start);
				break;
			case 'c':
				value = OscPacket.ValueFromByteArray<char>(data, ref start);
				break;
			case 'r':
				value = OscPacket.ValueFromByteArray<Color>(data, ref start);
				break;
			case 'T':
				value = true;
				break;
			case 'F':
				value = false;
				break;
			case 'N':
				value = null;
				break;
			case 'I':
				value = float.PositiveInfinity;
				break;
			default:
				continue;
			}
			oscMessage.Append(value);
		}
		return oscMessage;
	}

	public override int Append<T>(T value)
	{
		char c;
		if (value == null)
		{
			c = 'N';
		}
		else
		{
			Type type = value.GetType();
			switch (type.Name)
			{
			case "Int32":
				c = 'i';
				break;
			case "Int64":
				c = 'h';
				break;
			case "Single":
				c = (float.IsPositiveInfinity((float)(object)value) ? 'I' : 'f');
				break;
			case "Single[]":
			{
				float[] array = (float[])(object)value;
				foreach (float value2 in array)
				{
					Append(value2);
				}
				return mData.Count - 1;
			}
			case "Double":
				c = 'd';
				break;
			case "String":
				c = 's';
				break;
			case "Byte[]":
				c = 'b';
				break;
			case "OscTimeTag":
				c = 't';
				break;
			case "Char":
				c = 'c';
				break;
			case "Color":
				c = 'r';
				break;
			case "Boolean":
				c = (((bool)(object)value) ? 'T' : 'F');
				break;
			default:
				throw new Exception("Unsupported data type.");
			}
		}
		mTypeTag += c;
		mData.Add(value);
		return mData.Count - 1;
	}

	public int AppendNil()
	{
		return Append<object>(null);
	}

	public virtual void UpdateDataAt(int index, object value)
	{
		if (mData.Count == 0 || mData.Count <= index)
		{
			throw new ArgumentOutOfRangeException();
		}
		mData[index] = value;
	}

	public void ClearData()
	{
		mTypeTag = ','.ToString();
		mData.Clear();
	}
}
