using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

public struct Variant
{
	[StructLayout(LayoutKind.Explicit)]
	private struct VariantValue
	{
		public struct CurrencyLowHigh
		{
			public uint LowValue;

			public int HighValue;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct CurrencyValue
		{
			[FieldOffset(0)]
			public CurrencyLowHigh LowHigh;

			[FieldOffset(0)]
			public long longValue;
		}

		public struct RecordValue
		{
			public IntPtr RecordInfo;

			public IntPtr RecordPointer;
		}

		[FieldOffset(0)]
		public byte byteValue;

		[FieldOffset(0)]
		public sbyte signedByteValue;

		[FieldOffset(0)]
		public ushort ushortValue;

		[FieldOffset(0)]
		public short shortValue;

		[FieldOffset(0)]
		public uint uintValue;

		[FieldOffset(0)]
		public int intValue;

		[FieldOffset(0)]
		public ulong ulongValue;

		[FieldOffset(0)]
		public long longValue;

		[FieldOffset(0)]
		public float floatValue;

		[FieldOffset(0)]
		public double doubleValue;

		[FieldOffset(0)]
		public IntPtr pointerValue;

		[FieldOffset(0)]
		public CurrencyValue currencyValue;

		[FieldOffset(0)]
		public RecordValue recordValue;
	}

	private ushort vt;

	private ushort reserved1;

	private ushort reserved2;

	private ushort reserved3;

	private VariantValue variantValue;

	public VariantElementType ElementType
	{
		get
		{
			return (VariantElementType)(vt & 0xFFF);
		}
		set
		{
			vt = (ushort)((uint)(vt & 0xF000) | (uint)value);
		}
	}

	public VariantType Type
	{
		get
		{
			return (VariantType)(vt & 0xF000);
		}
		set
		{
			vt = (ushort)((uint)(vt & 0xFFF) | (uint)value);
		}
	}

	public unsafe object Value
	{
		get
		{
			switch (Type)
			{
			case VariantType.Default:
				switch (ElementType)
				{
				case VariantElementType.Empty:
				case VariantElementType.Null:
					return null;
				case VariantElementType.Blob:
				{
					byte[] array17 = new byte[(int)variantValue.recordValue.RecordInfo];
					if (array17.Length > 0)
					{
						Utilities.Read(variantValue.recordValue.RecordPointer, array17, 0, array17.Length);
					}
					return array17;
				}
				case VariantElementType.Bool:
					return variantValue.intValue != 0;
				case VariantElementType.Byte:
					return variantValue.signedByteValue;
				case VariantElementType.UByte:
					return variantValue.byteValue;
				case VariantElementType.UShort:
					return variantValue.ushortValue;
				case VariantElementType.Short:
					return variantValue.shortValue;
				case VariantElementType.UInt:
				case VariantElementType.UInt1:
					return variantValue.uintValue;
				case VariantElementType.Int:
				case VariantElementType.Int1:
					return variantValue.intValue;
				case VariantElementType.ULong:
					return variantValue.ulongValue;
				case VariantElementType.Long:
					return variantValue.longValue;
				case VariantElementType.Float:
					return variantValue.floatValue;
				case VariantElementType.Double:
					return variantValue.doubleValue;
				case VariantElementType.BinaryString:
					throw new NotSupportedException();
				case VariantElementType.StringPointer:
					return Marshal.PtrToStringAnsi(variantValue.pointerValue);
				case VariantElementType.WStringPointer:
					return Marshal.PtrToStringUni(variantValue.pointerValue);
				case VariantElementType.Dispatch:
				case VariantElementType.ComUnknown:
					return new ComObject(variantValue.pointerValue);
				case VariantElementType.Pointer:
				case VariantElementType.IntPointer:
					return variantValue.pointerValue;
				case VariantElementType.FileTime:
					return DateTime.FromFileTime(variantValue.longValue);
				default:
					return null;
				}
			case VariantType.Vector:
			{
				int num = (int)variantValue.recordValue.RecordInfo;
				switch (ElementType)
				{
				case VariantElementType.Bool:
				{
					Bool[] array13 = new Bool[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array13, 0, num);
					return Utilities.ConvertToBoolArray(array13);
				}
				case VariantElementType.Byte:
				{
					sbyte[] array12 = new sbyte[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array12, 0, num);
					return array12;
				}
				case VariantElementType.UByte:
				{
					byte[] array10 = new byte[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array10, 0, num);
					return array10;
				}
				case VariantElementType.UShort:
				{
					ushort[] array9 = new ushort[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array9, 0, num);
					return array9;
				}
				case VariantElementType.Short:
				{
					short[] array7 = new short[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array7, 0, num);
					return array7;
				}
				case VariantElementType.UInt:
				case VariantElementType.UInt1:
				{
					uint[] array6 = new uint[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array6, 0, num);
					return array6;
				}
				case VariantElementType.Int:
				case VariantElementType.Int1:
				{
					int[] array3 = new int[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array3, 0, num);
					return array3;
				}
				case VariantElementType.ULong:
				{
					ulong[] array2 = new ulong[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array2, 0, num);
					return array2;
				}
				case VariantElementType.Long:
				{
					long[] array16 = new long[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array16, 0, num);
					return array16;
				}
				case VariantElementType.Float:
				{
					float[] array15 = new float[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array15, 0, num);
					return array15;
				}
				case VariantElementType.Double:
				{
					double[] array14 = new double[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array14, 0, num);
					return array14;
				}
				case VariantElementType.BinaryString:
					throw new NotSupportedException();
				case VariantElementType.StringPointer:
				{
					string[] array11 = new string[num];
					for (int l = 0; l < num; l++)
					{
						array11[l] = Marshal.PtrToStringAnsi(((IntPtr*)(void*)variantValue.recordValue.RecordPointer)[l]);
					}
					return array11;
				}
				case VariantElementType.WStringPointer:
				{
					string[] array8 = new string[num];
					for (int k = 0; k < num; k++)
					{
						array8[k] = Marshal.PtrToStringUni(((IntPtr*)(void*)variantValue.recordValue.RecordPointer)[k]);
					}
					return array8;
				}
				case VariantElementType.Dispatch:
				case VariantElementType.ComUnknown:
				{
					ComObject[] array5 = new ComObject[num];
					for (int j = 0; j < num; j++)
					{
						array5[j] = new ComObject(((IntPtr*)(void*)variantValue.recordValue.RecordPointer)[j]);
					}
					return array5;
				}
				case VariantElementType.Pointer:
				case VariantElementType.IntPointer:
				{
					IntPtr[] array4 = new IntPtr[num];
					Utilities.Read(variantValue.recordValue.RecordPointer, array4, 0, num);
					return array4;
				}
				case VariantElementType.FileTime:
				{
					DateTime[] array = new DateTime[num];
					for (int i = 0; i < num; i++)
					{
						ref DateTime reference = ref array[i];
						reference = DateTime.FromFileTime(((long*)(void*)variantValue.recordValue.RecordPointer)[i]);
					}
					return array;
				}
				default:
					return null;
				}
			}
			default:
				return null;
			}
		}
		set
		{
			if (value == null)
			{
				Type = VariantType.Default;
				ElementType = VariantElementType.Null;
				return;
			}
			Type type = value.GetType();
			Type = VariantType.Default;
			if (type.IsPrimitive)
			{
				if (type == typeof(int))
				{
					ElementType = VariantElementType.Int;
					variantValue.intValue = (int)value;
					return;
				}
				if (type == typeof(uint))
				{
					ElementType = VariantElementType.UInt;
					variantValue.uintValue = (uint)value;
					return;
				}
				if (type == typeof(long))
				{
					ElementType = VariantElementType.Long;
					variantValue.longValue = (long)value;
					return;
				}
				if (type == typeof(ulong))
				{
					ElementType = VariantElementType.ULong;
					variantValue.ulongValue = (ulong)value;
					return;
				}
				if (type == typeof(short))
				{
					ElementType = VariantElementType.Short;
					variantValue.shortValue = (short)value;
					return;
				}
				if (type == typeof(ushort))
				{
					ElementType = VariantElementType.UShort;
					variantValue.ushortValue = (ushort)value;
					return;
				}
				if (type == typeof(float))
				{
					ElementType = VariantElementType.Float;
					variantValue.floatValue = (float)value;
					return;
				}
				if (type == typeof(double))
				{
					ElementType = VariantElementType.Double;
					variantValue.doubleValue = (double)value;
					return;
				}
			}
			else
			{
				if (value is ComObject)
				{
					ElementType = VariantElementType.ComUnknown;
					variantValue.pointerValue = ((ComObject)value).NativePointer;
					return;
				}
				if (value is DateTime)
				{
					ElementType = VariantElementType.FileTime;
					variantValue.longValue = ((DateTime)value).ToFileTime();
					return;
				}
				if (value is string)
				{
					ElementType = VariantElementType.WStringPointer;
					variantValue.pointerValue = Marshal.StringToCoTaskMemUni((string)value);
					return;
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type [{0}] is not handled", new object[1] { type.Name }));
		}
	}
}
