using System;

namespace SharpDX.Direct2D1;

public struct LayerParameters
{
	public RectangleF ContentBounds;

	internal IntPtr GeometricMaskPointer;

	public AntialiasMode MaskAntialiasMode;

	public Matrix3x2 MaskTransform;

	public float Opacity;

	internal IntPtr OpacityBrushPointer;

	public LayerOptions LayerOptions;

	public Geometry GeometricMask
	{
		set
		{
			GeometricMaskPointer = value.NativePointer;
		}
	}

	public Brush OpacityBrush
	{
		set
		{
			OpacityBrushPointer = value.NativePointer;
		}
	}
}
