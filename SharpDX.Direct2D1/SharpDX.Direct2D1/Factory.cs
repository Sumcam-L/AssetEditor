using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("06152247-6f50-465a-9245-118bfd3b6007")]
public class Factory : ComObject
{
	public Size2F DesktopDpi
	{
		get
		{
			GetDesktopDpi(out var dpiX, out var dpiY);
			return new Size2F(dpiX, dpiY);
		}
	}

	public Factory(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Factory(nativePointer);
		}
		return null;
	}

	public unsafe void ReloadSystemMetrics()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void GetDesktopDpi(out float dpiX, out float dpiY)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref dpiX))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref dpiY))
			{
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
	}

	internal unsafe void CreateRectangleGeometry(RectangleF rectangle, RectangleGeometry rectangleGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &rectangle, &zero);
		rectangleGeometry.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateRoundedRectangleGeometry(ref RoundedRectangle roundedRectangle, RoundedRectangleGeometry roundedRectangleGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RoundedRectangle, IntPtr>(ref roundedRectangle))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr, &zero);
		}
		roundedRectangleGeometry.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateEllipseGeometry(Ellipse ellipse, EllipseGeometry ellipseGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &ellipse, &zero);
		ellipseGeometry.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGeometryGroup(FillMode fillMode, Geometry[] geometries, int geometriesCount, GeometryGroup geometryGroup)
	{
		IntPtr* ptr = null;
		if (geometries != null)
		{
			IntPtr* ptr2 = stackalloc IntPtr[geometries.Length];
			ptr = ptr2;
			for (int i = 0; i < geometries.Length; i++)
			{
				ptr[i] = ((geometries[i] == null) ? IntPtr.Zero : geometries[i].NativePointer);
			}
		}
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)fillMode, ptr, geometriesCount, &zero);
		geometryGroup.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGeometryGroup(FillMode fillMode, ComArray<Geometry> geometries, int geometriesCount, GeometryGroup geometryGroup)
	{
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		IntPtr intPtr = geometries?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, (int)fillMode, (void*)intPtr, geometriesCount, &zero);
		geometryGroup.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateTransformedGeometry(Geometry sourceGeometry, ref Matrix3x2 transform, TransformedGeometry transformedGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref transform))
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = sourceGeometry?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, ptr, &zero);
		}
		transformedGeometry.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreatePathGeometry(PathGeometry athGeometryRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero);
		athGeometryRef.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateStrokeStyle(ref StrokeStyleProperties strokeStyleProperties, float[] dashes, int dashesCount, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<StrokeStyleProperties, IntPtr>(ref strokeStyleProperties))
		{
			fixed (IntPtr* ptr2 = dashes)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2, dashesCount, &zero);
			}
		}
		strokeStyle.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDrawingStateBlock(DrawingStateDescription? drawingStateDescription, RenderingParams textRenderingParams, DrawingStateBlock drawingStateBlock)
	{
		DrawingStateDescription value = default(DrawingStateDescription);
		if (drawingStateDescription.HasValue)
		{
			value = drawingStateDescription.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr = (drawingStateDescription.HasValue ? (&value) : ((void*)IntPtr.Zero));
		IntPtr intPtr2 = textRenderingParams?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)intPtr2, &zero);
		drawingStateBlock.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateWicBitmapRenderTarget(SharpDX.WIC.Bitmap target, ref RenderTargetProperties renderTargetProperties, RenderTarget renderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RenderTargetProperties, IntPtr>(ref renderTargetProperties))
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = target?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, ptr, &zero);
		}
		renderTarget.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateHwndRenderTarget(ref RenderTargetProperties renderTargetProperties, HwndRenderTargetProperties hwndRenderTargetProperties, WindowRenderTarget hwndRenderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RenderTargetProperties, IntPtr>(ref renderTargetProperties))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr, &hwndRenderTargetProperties, &zero);
		}
		hwndRenderTarget.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDxgiSurfaceRenderTarget(Surface dxgiSurface, ref RenderTargetProperties renderTargetProperties, RenderTarget renderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RenderTargetProperties, IntPtr>(ref renderTargetProperties))
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = dxgiSurface?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, ptr, &zero);
		}
		renderTarget.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDCRenderTarget(ref RenderTargetProperties renderTargetProperties, DeviceContextRenderTarget dcRenderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RenderTargetProperties, IntPtr>(ref renderTargetProperties))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, ptr, &zero);
		}
		dcRenderTarget.NativePointer = zero;
		result.CheckError();
	}

	public Factory()
		: this(FactoryType.SingleThreaded)
	{
	}

	public Factory(FactoryType factoryType)
		: this(factoryType, DebugLevel.None)
	{
	}

	public Factory(FactoryType factoryType, DebugLevel debugLevel)
		: base(IntPtr.Zero)
	{
		FactoryOptions? factoryOptionsRef = null;
		if (debugLevel != DebugLevel.None)
		{
			factoryOptionsRef = new FactoryOptions
			{
				DebugLevel = debugLevel
			};
		}
		D2D1.CreateFactory(factoryType, Utilities.GetGuidFromType(GetType()), factoryOptionsRef, out var iFactoryOut);
		FromTemp(iFactoryOut);
	}
}
