using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906bb-12e2-11dc-9fed-001143a055f9")]
public class TransformedGeometry : Geometry
{
	public Geometry SourceGeometry
	{
		get
		{
			GetSourceGeometry(out var sourceGeometry);
			return sourceGeometry;
		}
	}

	public Matrix3x2 Transform
	{
		get
		{
			GetTransform(out var transform);
			return transform;
		}
	}

	public TransformedGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TransformedGeometry(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new TransformedGeometry(nativePointer);
		}
		return null;
	}

	internal unsafe void GetSourceGeometry(out Geometry sourceGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, &zero);
		sourceGeometry = ((zero == IntPtr.Zero) ? null : new Geometry(zero));
	}

	internal unsafe void GetTransform(out Matrix3x2 transform)
	{
		transform = default(Matrix3x2);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref transform))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	public TransformedGeometry(Factory factory, Geometry geometrySource, Matrix3x2 matrix3X2)
		: base(IntPtr.Zero)
	{
		factory.CreateTransformedGeometry(geometrySource, ref matrix3X2, this);
	}
}
