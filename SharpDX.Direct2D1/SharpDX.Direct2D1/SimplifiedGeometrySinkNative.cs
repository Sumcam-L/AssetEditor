using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd9069e-12e2-11dc-9fed-001143a055f9")]
internal class SimplifiedGeometrySinkNative : ComObjectCallback, SimplifiedGeometrySink, ICallbackable, IDisposable
{
	public SimplifiedGeometrySinkNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SimplifiedGeometrySinkNative(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new SimplifiedGeometrySinkNative(nativePointer);
		}
		return null;
	}

	internal unsafe void SetFillMode_(FillMode fillMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (int)fillMode);
	}

	internal unsafe void SetSegmentFlags_(PathSegment vertexFlags)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)vertexFlags);
	}

	internal unsafe void BeginFigure_(Vector2 startPoint, FigureBegin figureBegin)
	{
		((delegate* unmanaged[Stdcall]<void*, Vector2, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, startPoint, (int)figureBegin);
	}

	internal unsafe void AddLines_(Vector2[] ointsRef, int pointsCount)
	{
		fixed (IntPtr* ptr = ointsRef)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr, pointsCount);
		}
	}

	internal unsafe void AddBeziers_(BezierSegment[] beziers, int beziersCount)
	{
		fixed (IntPtr* ptr = beziers)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr, beziersCount);
		}
	}

	internal unsafe void EndFigure_(FigureEnd figureEnd)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)figureEnd);
	}

	internal unsafe void Close_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public void AddBeziers(BezierSegment[] beziers)
	{
		AddBeziers_(beziers, beziers.Length);
	}

	public void AddLines(Vector2[] points)
	{
		AddLines_(points, points.Length);
	}

	public void BeginFigure(Vector2 startPoint, FigureBegin figureBegin)
	{
		BeginFigure_(startPoint, figureBegin);
	}

	public void Close()
	{
		Close_();
	}

	public void EndFigure(FigureEnd figureEnd)
	{
		EndFigure_(figureEnd);
	}

	public void SetFillMode(FillMode fillMode)
	{
		SetFillMode_(fillMode);
	}

	public void SetSegmentFlags(PathSegment vertexFlags)
	{
		SetSegmentFlags_(vertexFlags);
	}
}
