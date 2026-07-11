using System;
using System.Drawing;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public class D2dLinearGradientBrush : D2dBrush
{
	private PointF m_start;

	private PointF m_end;

	private readonly D2dGradientStop[] m_gradientStops;

	private readonly D2dExtendMode m_extendMode;

	private readonly D2dGamma m_gamma;

	public PointF StartPoint
	{
		get
		{
			return m_start;
		}
		set
		{
			m_start = value;
			LinearGradientBrush linearGradientBrush = (LinearGradientBrush)base.NativeBrush;
			linearGradientBrush.StartPoint = value.ToSharpDX();
		}
	}

	public PointF EndPoint
	{
		get
		{
			return m_end;
		}
		set
		{
			m_end = value;
			LinearGradientBrush linearGradientBrush = (LinearGradientBrush)base.NativeBrush;
			linearGradientBrush.EndPoint = value.ToSharpDX();
		}
	}

	internal D2dLinearGradientBrush(D2dGraphics owner, PointF pt1, PointF pt2, D2dGradientStop[] gradientStops, D2dExtendMode extendMode, D2dGamma gamma)
		: base(owner)
	{
		m_start = pt1;
		m_end = pt2;
		m_gradientStops = new D2dGradientStop[gradientStops.Length];
		Array.Copy(gradientStops, m_gradientStops, m_gradientStops.Length);
		m_gamma = gamma;
		m_extendMode = extendMode;
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
		using GradientStopCollection gradientStopCollection = new GradientStopCollection(base.Owner.D2dRenderTarget, array, (Gamma)m_gamma, (ExtendMode)m_extendMode);
		LinearGradientBrushProperties linearGradientBrushProperties = new LinearGradientBrushProperties
		{
			StartPoint = m_start.ToSharpDX(),
			EndPoint = m_end.ToSharpDX()
		};
		base.NativeBrush = new LinearGradientBrush(base.Owner.D2dRenderTarget, linearGradientBrushProperties, gradientStopCollection);
	}
}
