using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("2cd90694-12e2-11dc-9fed-001143a055f9")]
public class RenderTarget : Resource
{
	public const float DefaultStrokeWidth = 1f;

	private float _strokeWidth = 1f;

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

	public AntialiasMode AntialiasMode
	{
		get
		{
			return GetAntialiasMode();
		}
		set
		{
			SetAntialiasMode(value);
		}
	}

	public TextAntialiasMode TextAntialiasMode
	{
		get
		{
			return GetTextAntialiasMode();
		}
		set
		{
			SetTextAntialiasMode(value);
		}
	}

	public RenderingParams TextRenderingParams
	{
		get
		{
			GetTextRenderingParams(out var textRenderingParams);
			return textRenderingParams;
		}
		set
		{
			SetTextRenderingParams(value);
		}
	}

	public PixelFormat PixelFormat => GetPixelFormat();

	public Size2F Size => GetSize();

	public Size2 PixelSize => GetPixelSize();

	public int MaximumBitmapSize => GetMaximumBitmapSize();

	public float StrokeWidth
	{
		get
		{
			return _strokeWidth;
		}
		set
		{
			_strokeWidth = value;
		}
	}

	public Size2F DotsPerInch
	{
		get
		{
			GetDpi(out var dpiX, out var dpiY);
			return new Size2F(dpiX, dpiY);
		}
		set
		{
			SetDpi(value.Width, value.Height);
		}
	}

	public RenderTarget(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderTarget(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new RenderTarget(nativePointer);
		}
		return null;
	}

	internal unsafe void CreateBitmap(Size2 size, IntPtr srcData, int pitch, BitmapProperties bitmapProperties, Bitmap bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, Size2, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, size, (void*)srcData, pitch, &bitmapProperties, &zero);
		bitmap.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapFromWicBitmap(BitmapSource wicBitmapSource, BitmapProperties? bitmapProperties, out Bitmap bitmap)
	{
		BitmapProperties value = default(BitmapProperties);
		if (bitmapProperties.HasValue)
		{
			value = bitmapProperties.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(wicBitmapSource?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (bitmapProperties.HasValue ? (&value) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, &zero);
		bitmap = ((zero == IntPtr.Zero) ? null : new Bitmap(zero));
		result.CheckError();
	}

	internal unsafe void CreateSharedBitmap(Guid riid, IntPtr data, BitmapProperties? bitmapProperties, Bitmap bitmap)
	{
		BitmapProperties value = default(BitmapProperties);
		if (bitmapProperties.HasValue)
		{
			value = bitmapProperties.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		Guid* num = &riid;
		void* intPtr = (void*)data;
		void* intPtr2 = (bitmapProperties.HasValue ? (&value) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, num, intPtr, intPtr2, &zero);
		bitmap.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBitmapBrush(Bitmap bitmap, BitmapBrushProperties? bitmapBrushProperties, BrushProperties? brushProperties, BitmapBrush bitmapBrush)
	{
		BitmapBrushProperties value = default(BitmapBrushProperties);
		if (bitmapBrushProperties.HasValue)
		{
			value = bitmapBrushProperties.Value;
		}
		BrushProperties value2 = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value2 = brushProperties.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(bitmap?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (bitmapBrushProperties.HasValue ? (&value) : ((void*)IntPtr.Zero));
		void* intPtr3 = (brushProperties.HasValue ? (&value2) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, &zero);
		bitmapBrush.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateSolidColorBrush(Color4 color, BrushProperties? brushProperties, SolidColorBrush solidColorBrush)
	{
		BrushProperties value = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value = brushProperties.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		Color4* num = &color;
		void* intPtr = (brushProperties.HasValue ? (&value) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, num, intPtr, &zero);
		solidColorBrush.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGradientStopCollection(GradientStop[] gradientStops, int gradientStopsCount, Gamma colorInterpolationGamma, ExtendMode extendMode, GradientStopCollection gradientStopCollection)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = gradientStops)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr, gradientStopsCount, (int)colorInterpolationGamma, (int)extendMode, &zero);
		}
		gradientStopCollection.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateLinearGradientBrush(LinearGradientBrushProperties linearGradientBrushProperties, BrushProperties? brushProperties, GradientStopCollection gradientStopCollection, LinearGradientBrush linearGradientBrush)
	{
		BrushProperties value = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value = brushProperties.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		LinearGradientBrushProperties* num = &linearGradientBrushProperties;
		void* intPtr = (brushProperties.HasValue ? (&value) : ((void*)IntPtr.Zero));
		IntPtr intPtr2 = gradientStopCollection?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, num, intPtr, (void*)intPtr2, &zero);
		linearGradientBrush.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateRadialGradientBrush(ref RadialGradientBrushProperties radialGradientBrushProperties, BrushProperties? brushProperties, GradientStopCollection gradientStopCollection, RadialGradientBrush radialGradientBrush)
	{
		BrushProperties value = default(BrushProperties);
		if (brushProperties.HasValue)
		{
			value = brushProperties.Value;
		}
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RadialGradientBrushProperties, IntPtr>(ref radialGradientBrushProperties))
		{
			void* nativePointer = _nativePointer;
			void* intPtr = (brushProperties.HasValue ? (&value) : ((void*)IntPtr.Zero));
			IntPtr intPtr2 = gradientStopCollection?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, ptr, intPtr, (void*)intPtr2, &zero);
		}
		radialGradientBrush.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateCompatibleRenderTarget(Size2F? desiredSize, Size2? desiredPixelSize, PixelFormat? desiredFormat, CompatibleRenderTargetOptions options, BitmapRenderTarget bitmapRenderTarget)
	{
		Size2F value = default(Size2F);
		if (desiredSize.HasValue)
		{
			value = desiredSize.Value;
		}
		Size2 value2 = default(Size2);
		if (desiredPixelSize.HasValue)
		{
			value2 = desiredPixelSize.Value;
		}
		PixelFormat value3 = default(PixelFormat);
		if (desiredFormat.HasValue)
		{
			value3 = desiredFormat.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr = (desiredSize.HasValue ? (&value) : ((void*)IntPtr.Zero));
		void* intPtr2 = (desiredPixelSize.HasValue ? (&value2) : ((void*)IntPtr.Zero));
		void* intPtr3 = (desiredFormat.HasValue ? (&value3) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, (int)options, &zero);
		bitmapRenderTarget.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateLayer(Size2F? size, Layer layer)
	{
		Size2F value = default(Size2F);
		if (size.HasValue)
		{
			value = size.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr = (size.HasValue ? (&value) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, intPtr, &zero);
		layer.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateMesh(Mesh mesh)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, &zero);
		mesh.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void DrawLine(Vector2 point0, Vector2 point1, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(brush?.NativePointer ?? IntPtr.Zero);
		IntPtr intPtr2 = strokeStyle?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, Vector2, Vector2, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, point0, point1, intPtr, strokeWidth, (void*)intPtr2);
	}

	public unsafe void DrawRectangle(RectangleF rect, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		void* nativePointer = _nativePointer;
		RectangleF* num = &rect;
		void* intPtr = (void*)(brush?.NativePointer ?? IntPtr.Zero);
		IntPtr intPtr2 = strokeStyle?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(nativePointer, num, intPtr, strokeWidth, (void*)intPtr2);
	}

	public unsafe void FillRectangle(RectangleF rect, Brush brush)
	{
		void* nativePointer = _nativePointer;
		RectangleF* num = &rect;
		IntPtr intPtr = brush?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(nativePointer, num, (void*)intPtr);
	}

	public unsafe void DrawRoundedRectangle(ref RoundedRectangle roundedRect, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RoundedRectangle, IntPtr>(ref roundedRect))
		{
			void* nativePointer = _nativePointer;
			void* intPtr = (void*)(brush?.NativePointer ?? IntPtr.Zero);
			IntPtr intPtr2 = strokeStyle?.NativePointer ?? IntPtr.Zero;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(nativePointer, ptr, intPtr, strokeWidth, (void*)intPtr2);
		}
	}

	public unsafe void FillRoundedRectangle(ref RoundedRectangle roundedRect, Brush brush)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RoundedRectangle, IntPtr>(ref roundedRect))
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = brush?.NativePointer ?? IntPtr.Zero;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(nativePointer, ptr, (void*)intPtr);
		}
	}

	public unsafe void DrawEllipse(Ellipse ellipse, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		void* nativePointer = _nativePointer;
		Ellipse* num = &ellipse;
		void* intPtr = (void*)(brush?.NativePointer ?? IntPtr.Zero);
		IntPtr intPtr2 = strokeStyle?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(nativePointer, num, intPtr, strokeWidth, (void*)intPtr2);
	}

	public unsafe void FillEllipse(Ellipse ellipse, Brush brush)
	{
		void* nativePointer = _nativePointer;
		Ellipse* num = &ellipse;
		IntPtr intPtr = brush?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(nativePointer, num, (void*)intPtr);
	}

	public unsafe void DrawGeometry(Geometry geometry, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(geometry?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (void*)(brush?.NativePointer ?? IntPtr.Zero);
		IntPtr intPtr3 = strokeStyle?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, strokeWidth, (void*)intPtr3);
	}

	public unsafe void FillGeometry(Geometry geometry, Brush brush, Brush opacityBrush)
	{
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(geometry?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (void*)(brush?.NativePointer ?? IntPtr.Zero);
		IntPtr intPtr3 = opacityBrush?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, (void*)intPtr3);
	}

	public unsafe void FillMesh(Mesh mesh, Brush brush)
	{
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(mesh?.NativePointer ?? IntPtr.Zero);
		IntPtr intPtr2 = brush?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)intPtr2);
	}

	public unsafe void FillOpacityMask(Bitmap opacityMask, Brush brush, OpacityMaskContent content, RectangleF? destinationRectangle, RectangleF? sourceRectangle)
	{
		RectangleF value = default(RectangleF);
		if (destinationRectangle.HasValue)
		{
			value = destinationRectangle.Value;
		}
		RectangleF value2 = default(RectangleF);
		if (sourceRectangle.HasValue)
		{
			value2 = sourceRectangle.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(opacityMask?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (void*)(brush?.NativePointer ?? IntPtr.Zero);
		void* intPtr3 = (destinationRectangle.HasValue ? (&value) : ((void*)IntPtr.Zero));
		void* intPtr4 = (sourceRectangle.HasValue ? (&value2) : ((void*)IntPtr.Zero));
		((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, (int)content, intPtr3, intPtr4);
	}

	public unsafe void DrawBitmap(Bitmap bitmap, RectangleF? destinationRectangle, float opacity, BitmapInterpolationMode interpolationMode, RectangleF? sourceRectangle)
	{
		RectangleF value = default(RectangleF);
		if (destinationRectangle.HasValue)
		{
			value = destinationRectangle.Value;
		}
		RectangleF value2 = default(RectangleF);
		if (sourceRectangle.HasValue)
		{
			value2 = sourceRectangle.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(bitmap?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (destinationRectangle.HasValue ? (&value) : ((void*)IntPtr.Zero));
		void* intPtr3 = (sourceRectangle.HasValue ? (&value2) : ((void*)IntPtr.Zero));
		((delegate* unmanaged[Stdcall]<void*, void*, void*, float, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, opacity, (int)interpolationMode, intPtr3);
	}

	public unsafe void DrawText(string text, int stringLength, TextFormat textFormat, RectangleF layoutRect, Brush defaultForegroundBrush, DrawTextOptions options, MeasuringMode measuringMode)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(text);
		void* nativePointer = _nativePointer;
		void* intPtr2 = (void*)intPtr;
		void* intPtr3 = (void*)(textFormat?.NativePointer ?? IntPtr.Zero);
		RectangleF* num = &layoutRect;
		IntPtr intPtr4 = defaultForegroundBrush?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(nativePointer, intPtr2, stringLength, intPtr3, num, (void*)intPtr4, (int)options, (int)measuringMode);
		Marshal.FreeHGlobal(intPtr);
	}

	public unsafe void DrawTextLayout(Vector2 origin, TextLayout textLayout, Brush defaultForegroundBrush, DrawTextOptions options)
	{
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(textLayout?.NativePointer ?? IntPtr.Zero);
		IntPtr intPtr2 = defaultForegroundBrush?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, Vector2, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(nativePointer, origin, intPtr, (void*)intPtr2, (int)options);
	}

	public unsafe void DrawGlyphRun(Vector2 baselineOrigin, GlyphRun glyphRun, Brush foregroundBrush, MeasuringMode measuringMode)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		glyphRun.__MarshalTo(ref @ref);
		void* nativePointer = _nativePointer;
		GlyphRun.__Native* num = &@ref;
		IntPtr intPtr = foregroundBrush?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, Vector2, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(nativePointer, baselineOrigin, num, (void*)intPtr, (int)measuringMode);
		glyphRun.__MarshalFree(ref @ref);
	}

	internal unsafe void SetTransform(ref Matrix3x2 transform)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref transform))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	internal unsafe void GetTransform(out Matrix3x2 transform)
	{
		transform = default(Matrix3x2);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref transform))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	internal unsafe void SetAntialiasMode(AntialiasMode antialiasMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, (int)antialiasMode);
	}

	internal unsafe AntialiasMode GetAntialiasMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, AntialiasMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetTextAntialiasMode(TextAntialiasMode textAntialiasMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, (int)textAntialiasMode);
	}

	internal unsafe TextAntialiasMode GetTextAntialiasMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, TextAntialiasMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetTextRenderingParams(RenderingParams textRenderingParams)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = textRenderingParams?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr);
	}

	internal unsafe void GetTextRenderingParams(out RenderingParams textRenderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, &zero);
		textRenderingParams = ((zero == IntPtr.Zero) ? null : new RenderingParams(zero));
	}

	public unsafe void SetTags(long tag1, long tag2)
	{
		((delegate* unmanaged[Stdcall]<void*, long, long, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, tag1, tag2);
	}

	public unsafe void GetTags(out long tag1, out long tag2)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<long, IntPtr>(ref tag1))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<long, IntPtr>(ref tag2))
			{
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
	}

	public unsafe void PushLayer(ref LayerParameters layerParameters, Layer layer)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<LayerParameters, IntPtr>(ref layerParameters))
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = layer?.NativePointer ?? IntPtr.Zero;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(nativePointer, ptr, (void*)intPtr);
		}
	}

	public unsafe void PopLayer()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Flush(out long tag1, out long tag2)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<long, IntPtr>(ref tag1))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<long, IntPtr>(ref tag2))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void SaveDrawingState(DrawingStateBlock drawingStateBlock)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = drawingStateBlock?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr);
	}

	public unsafe void RestoreDrawingState(DrawingStateBlock drawingStateBlock)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = drawingStateBlock?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr);
	}

	public unsafe void PushAxisAlignedClip(RectangleF clipRect, AntialiasMode antialiasMode)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, &clipRect, (int)antialiasMode);
	}

	public unsafe void PopAxisAlignedClip()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Clear(Color4? clearColor)
	{
		Color4 value = default(Color4);
		if (clearColor.HasValue)
		{
			value = clearColor.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (clearColor.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(nativePointer, intPtr);
	}

	public unsafe void BeginDraw()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)48 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void EndDraw(out long tag1, out long tag2)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<long, IntPtr>(ref tag1))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<long, IntPtr>(ref tag2))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)49 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe PixelFormat GetPixelFormat()
	{
		PixelFormat result = default(PixelFormat);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)50 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe void SetDpi(float dpiX, float dpiY)
	{
		((delegate* unmanaged[Stdcall]<void*, float, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)51 * (nint)sizeof(void*))))(_nativePointer, dpiX, dpiY);
	}

	internal unsafe void GetDpi(out float dpiX, out float dpiY)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref dpiX))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref dpiY))
			{
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)52 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
	}

	internal unsafe Size2F GetSize()
	{
		Size2F result = default(Size2F);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)53 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe Size2 GetPixelSize()
	{
		Size2 result = default(Size2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)54 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe int GetMaximumBitmapSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)55 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe Bool IsSupported(ref RenderTargetProperties renderTargetProperties)
	{
		Bool result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RenderTargetProperties, IntPtr>(ref renderTargetProperties))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, Bool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)56 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		return result;
	}

	public RenderTarget(Factory factory, Surface dxgiSurface, RenderTargetProperties properties)
		: base(IntPtr.Zero)
	{
		factory.CreateDxgiSurfaceRenderTarget(dxgiSurface, ref properties, this);
	}

	public void DrawBitmap(Bitmap bitmap, float opacity, BitmapInterpolationMode interpolationMode)
	{
		DrawBitmap(bitmap, null, opacity, interpolationMode, null);
	}

	public void DrawBitmap(Bitmap bitmap, RectangleF destinationRectangle, float opacity, BitmapInterpolationMode interpolationMode)
	{
		DrawBitmap(bitmap, destinationRectangle, opacity, interpolationMode, null);
	}

	public void DrawBitmap(Bitmap bitmap, float opacity, BitmapInterpolationMode interpolationMode, RectangleF sourceRectangle)
	{
		DrawBitmap(bitmap, null, opacity, interpolationMode, sourceRectangle);
	}

	public void DrawEllipse(Ellipse ellipse, Brush brush)
	{
		DrawEllipse(ellipse, brush, StrokeWidth, null);
	}

	public void DrawEllipse(Ellipse ellipse, Brush brush, float strokeWidth)
	{
		DrawEllipse(ellipse, brush, strokeWidth, null);
	}

	public void DrawGeometry(Geometry geometry, Brush brush)
	{
		DrawGeometry(geometry, brush, StrokeWidth, null);
	}

	public void DrawGeometry(Geometry geometry, Brush brush, float strokeWidth)
	{
		DrawGeometry(geometry, brush, strokeWidth, null);
	}

	public void DrawLine(Vector2 point0, Vector2 point1, Brush brush)
	{
		DrawLine(point0, point1, brush, StrokeWidth, null);
	}

	public void DrawLine(Vector2 point0, Vector2 point1, Brush brush, float strokeWidth)
	{
		DrawLine(point0, point1, brush, strokeWidth, null);
	}

	public void DrawRectangle(RectangleF rect, Brush brush)
	{
		DrawRectangle(rect, brush, StrokeWidth, null);
	}

	public void DrawRectangle(RectangleF rect, Brush brush, float strokeWidth)
	{
		DrawRectangle(rect, brush, strokeWidth, null);
	}

	public void DrawRoundedRectangle(RoundedRectangle roundedRect, Brush brush)
	{
		DrawRoundedRectangle(ref roundedRect, brush, StrokeWidth, null);
	}

	public void DrawRoundedRectangle(RoundedRectangle roundedRect, Brush brush, float strokeWidth)
	{
		DrawRoundedRectangle(ref roundedRect, brush, strokeWidth, null);
	}

	public void DrawRoundedRectangle(RoundedRectangle roundedRect, Brush brush, float strokeWidth, StrokeStyle strokeStyle)
	{
		DrawRoundedRectangle(ref roundedRect, brush, strokeWidth, strokeStyle);
	}

	public void DrawText(string text, TextFormat textFormat, RectangleF layoutRect, Brush defaultForegroundBrush)
	{
		DrawText(text, text.Length, textFormat, layoutRect, defaultForegroundBrush, DrawTextOptions.None, MeasuringMode.Natural);
	}

	public void DrawText(string text, TextFormat textFormat, RectangleF layoutRect, Brush defaultForegroundBrush, DrawTextOptions options)
	{
		DrawText(text, text.Length, textFormat, layoutRect, defaultForegroundBrush, options, MeasuringMode.Natural);
	}

	public void DrawText(string text, TextFormat textFormat, RectangleF layoutRect, Brush defaultForegroundBrush, DrawTextOptions options, MeasuringMode measuringMode)
	{
		DrawText(text, text.Length, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode);
	}

	public void DrawTextLayout(Vector2 origin, TextLayout textLayout, Brush defaultForegroundBrush)
	{
		DrawTextLayout(origin, textLayout, defaultForegroundBrush, DrawTextOptions.None);
	}

	public void EndDraw()
	{
		EndDraw(out var _, out var _);
	}

	public void FillGeometry(Geometry geometry, Brush brush)
	{
		FillGeometry(geometry, brush, null);
	}

	public void FillOpacityMask(Bitmap opacityMask, Brush brush, OpacityMaskContent content)
	{
		FillOpacityMask(opacityMask, brush, content, null, null);
	}

	public void FillRoundedRectangle(RoundedRectangle roundedRect, Brush brush)
	{
		FillRoundedRectangle(ref roundedRect, brush);
	}

	public void Flush()
	{
		Flush(out var _, out var _);
	}
}
