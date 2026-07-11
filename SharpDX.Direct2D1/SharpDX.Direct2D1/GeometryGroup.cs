using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a6-12e2-11dc-9fed-001143a055f9")]
public class GeometryGroup : Geometry
{
	public FillMode FillMode => GetFillMode();

	public int SourceGeometryCount => GetSourceGeometryCount();

	public GeometryGroup(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GeometryGroup(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new GeometryGroup(nativePointer);
		}
		return null;
	}

	internal unsafe FillMode GetFillMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FillMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetSourceGeometryCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetSourceGeometries(Geometry[] geometries, int geometriesCount)
	{
		IntPtr* ptr = stackalloc IntPtr[geometries.Length];
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, ptr, geometriesCount);
		for (int i = 0; i < geometries.Length; i++)
		{
			geometries[i] = ((ptr[i] == IntPtr.Zero) ? null : new Geometry(ptr[i]));
		}
	}

	public GeometryGroup(Factory factory, FillMode fillMode, Geometry[] geometries)
		: base(IntPtr.Zero)
	{
		factory.CreateGeometryGroup(fillMode, geometries, geometries.Length, this);
	}

	public Geometry[] GetSourceGeometry()
	{
		return GetSourceGeometry(SourceGeometryCount);
	}

	public Geometry[] GetSourceGeometry(int geometriesCount)
	{
		Geometry[] array = new Geometry[geometriesCount];
		GetSourceGeometries(array, geometriesCount);
		return array;
	}
}
