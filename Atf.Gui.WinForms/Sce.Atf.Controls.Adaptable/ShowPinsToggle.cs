using System.Drawing;

namespace Sce.Atf.Controls.Adaptable;

public class ShowPinsToggle
{
	private readonly RectangleF m_bounds;

	public RectangleF Bounds => m_bounds;

	public ShowPinsToggle(RectangleF bounds)
	{
		m_bounds = bounds;
	}
}
