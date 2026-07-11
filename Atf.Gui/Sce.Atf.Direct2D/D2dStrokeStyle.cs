using System;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public class D2dStrokeStyle : D2dResource
{
	private StrokeStyle m_strokeStyle;

	internal StrokeStyle NativeStrokeStyle => m_strokeStyle;

	internal D2dStrokeStyle(StrokeStyle strokeStyle)
	{
		if (strokeStyle == null)
		{
			throw new ArgumentNullException("strokeStyle");
		}
		m_strokeStyle = strokeStyle;
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed)
		{
			m_strokeStyle.Dispose();
			base.Dispose(disposing);
		}
	}
}
