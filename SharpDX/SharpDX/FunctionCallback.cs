using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential)]
internal class FunctionCallback
{
	public IntPtr Pointer;

	public FunctionCallback(IntPtr pointer)
	{
		Pointer = pointer;
	}

	public unsafe FunctionCallback(void* pointer)
	{
		Pointer = new IntPtr(pointer);
	}

	public static explicit operator IntPtr(FunctionCallback value)
	{
		return value.Pointer;
	}

	public static implicit operator FunctionCallback(IntPtr value)
	{
		return new FunctionCallback(value);
	}

	public unsafe static implicit operator void*(FunctionCallback value)
	{
		return (void*)value.Pointer;
	}

	public unsafe static explicit operator FunctionCallback(void* value)
	{
		return new FunctionCallback(value);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}", new object[1] { Pointer });
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(CultureInfo.CurrentCulture, "{0}", new object[1] { Pointer.ToString(format) });
	}

	public override int GetHashCode()
	{
		return Pointer.ToInt32();
	}

	public bool Equals(FunctionCallback other)
	{
		return Pointer == other.Pointer;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(FunctionCallback)))
		{
			return false;
		}
		return Equals((FunctionCallback)value);
	}
}
