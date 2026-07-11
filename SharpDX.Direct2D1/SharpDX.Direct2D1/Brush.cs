using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a8-12e2-11dc-9fed-001143a055f9")]
public class Brush : Resource
{
	public float Opacity
	{
		get
		{
			return GetOpacity();
		}
		set
		{
			SetOpacity(value);
		}
	}

	public Matrix3x2 Transform
	{
		get
		{
			GetTransform(out var transform);
			return transform;
		}
		set
		{
			SetTransform(ref value);
		}
	}

	public Brush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Brush(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Brush(nativePointer);
		}
		return null;
	}

	internal unsafe void SetOpacity(float opacity)
	{
		((delegate* unmanaged[Stdcall]<void*, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, opacity);
	}

	internal unsafe void SetTransform(ref Matrix3x2 transform)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref transform))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	internal unsafe float GetOpacity()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetTransform(out Matrix3x2 transform)
	{
		transform = default(Matrix3x2);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref transform))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}
}
