using System.Drawing;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public class D2dSolidColorBrush : D2dBrush
{
	private Color m_color;

	public Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
			SolidColorBrush solidColorBrush = (SolidColorBrush)base.NativeBrush;
			solidColorBrush.Color = value.ToColor4();
		}
	}

	internal override void Create()
	{
		if (base.NativeBrush != null)
		{
			base.NativeBrush.Dispose();
		}
		base.NativeBrush = new SolidColorBrush(base.Owner.D2dRenderTarget, m_color.ToColor4());
	}

	internal D2dSolidColorBrush(D2dGraphics owner, Color color)
		: base(owner)
	{
		m_color = color;
		Create();
	}
}
