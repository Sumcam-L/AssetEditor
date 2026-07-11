using System;
using System.Drawing;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public class D2dRadialGradientBrush : D2dBrush
{
	private PointF m_center;

	private PointF m_gradientOriginOffset;

	private float m_radiusX;

	private float m_radiusY;

	private D2dGradientStop[] m_gradientStops;

	public PointF Center
	{
		get
		{
			return m_center;
		}
		set
		{
			m_center = value;
			RadialGradientBrush radialGradientBrush = (RadialGradientBrush)base.NativeBrush;
			radialGradientBrush.Center = value.ToSharpDX();
		}
	}

	public PointF GradientOriginOffset
	{
		get
		{
			return m_gradientOriginOffset;
		}
		set
		{
			m_gradientOriginOffset = value;
			RadialGradientBrush radialGradientBrush = (RadialGradientBrush)base.NativeBrush;
			radialGradientBrush.GradientOriginOffset = value.ToSharpDX();
		}
	}

	public float RadiusX
	{
		get
		{
			return m_radiusX;
		}
		set
		{
			m_radiusX = value;
			RadialGradientBrush radialGradientBrush = (RadialGradientBrush)base.NativeBrush;
			radialGradientBrush.RadiusX = value;
		}
	}

	public float RadiusY
	{
		get
		{
			return m_radiusY;
		}
		set
		{
			m_radiusY = value;
			RadialGradientBrush radialGradientBrush = (RadialGradientBrush)base.NativeBrush;
			radialGradientBrush.RadiusY = value;
		}
	}

	internal D2dRadialGradientBrush(D2dGraphics owner, PointF center, PointF gradientOriginOffset, float radiusX, float radiusY, params D2dGradientStop[] gradientStops)
		: base(owner)
	{
		m_center = center;
		m_gradientOriginOffset = gradientOriginOffset;
		m_radiusX = radiusX;
		m_radiusY = radiusY;
		m_gradientStops = new D2dGradientStop[gradientStops.Length];
		Array.Copy(gradientStops, m_gradientStops, m_gradientStops.Length);
		Create();
	}

	internal override void Create()
	{
		if (base.NativeBrush != null)
		{
			base.NativeBrush.Dispose();
		}
		GradientStop[] array = new GradientStop[m_gradientStops.Length];
		for (int i = 0; i < m_gradientStops.Length; i++)
		{
			array[i].Color = m_gradientStops[i].Color.ToColor4();
			array[i].Position = m_gradientStops[i].Position;
		}
		RadialGradientBrushProperties radialGradientBrushProperties = new RadialGradientBrushProperties
		{
			Center = m_center.ToSharpDX(),
			GradientOriginOffset = m_gradientOriginOffset.ToSharpDX(),
			RadiusX = m_radiusX,
			RadiusY = m_radiusY
		};
		using GradientStopCollection gradientStopCollection = new GradientStopCollection(base.Owner.D2dRenderTarget, array, Gamma.StandardRgb, ExtendMode.Clamp);
		base.NativeBrush = new RadialGradientBrush(base.Owner.D2dRenderTarget, radialGradientBrushProperties, gradientStopCollection);
	}
}
