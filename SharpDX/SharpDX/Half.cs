using System.Globalization;
using SharpDX.Serialization;

namespace SharpDX;

[DynamicSerializer("TKH1")]
public struct Half : IDataSerializable
{
	public const int PrecisionDigits = 3;

	public const int MantissaBits = 11;

	public const int MaximumDecimalExponent = 4;

	public const int MaximumBinaryExponent = 15;

	public const int MinimumDecimalExponent = -4;

	public const int MinimumBinaryExponent = -14;

	public const int ExponentRadix = 2;

	public const int AdditionRounding = 1;

	private ushort value;

	public static readonly float Epsilon;

	public static readonly float MaxValue;

	public static readonly float MinValue;

	public ushort RawValue
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public Half(float value)
	{
		this.value = HalfUtils.Pack(value);
	}

	public Half(ushort rawvalue)
	{
		value = rawvalue;
	}

	public static float[] ConvertToFloat(Half[] values)
	{
		float[] array = new float[values.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HalfUtils.Unpack(values[i].RawValue);
		}
		return array;
	}

	public static Half[] ConvertToHalf(float[] values)
	{
		Half[] array = new Half[values.Length];
		for (int i = 0; i < array.Length; i++)
		{
			ref Half reference = ref array[i];
			reference = new Half(values[i]);
		}
		return array;
	}

	public static implicit operator Half(float value)
	{
		return new Half(value);
	}

	public static implicit operator float(Half value)
	{
		return HalfUtils.Unpack(value.value);
	}

	public static bool operator ==(Half left, Half right)
	{
		return left.value == right.value;
	}

	public static bool operator !=(Half left, Half right)
	{
		return left.value != right.value;
	}

	public override string ToString()
	{
		return ((float)this).ToString(CultureInfo.CurrentCulture);
	}

	public override int GetHashCode()
	{
		ushort num = value;
		return (num * 3 / 2) ^ num;
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(value);
		}
		else
		{
			value = serializer.Reader.ReadUInt16();
		}
	}

	public static bool Equals(ref Half value1, ref Half value2)
	{
		return value1.value == value2.value;
	}

	public bool Equals(Half other)
	{
		return other.value == value;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(obj.GetType(), typeof(Half)))
		{
			return false;
		}
		return ((Half)obj).value == value;
	}

	static Half()
	{
		Epsilon = 0.0004887581f;
		MaxValue = 65504f;
		MinValue = 6.103516E-05f;
	}
}
