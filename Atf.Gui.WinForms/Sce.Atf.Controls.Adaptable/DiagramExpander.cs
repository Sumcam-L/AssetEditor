using System.Drawing;

namespace Sce.Atf.Controls.Adaptable;

public class DiagramExpander
{
	private readonly RectangleF m_expanderBounds;

	public RectangleF Bounds => m_expanderBounds;

	public DiagramExpander(RectangleF expanderBounds)
	{
		m_expanderBounds = expanderBounds;
	}
}
