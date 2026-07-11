using System.Drawing;

namespace Sce.Atf.Controls.Adaptable;

public class DiagramPin
{
	private readonly RectangleF m_pinBounds;

	public RectangleF Bounds => m_pinBounds;

	public DiagramPin(RectangleF pinBounds)
	{
		m_pinBounds = pinBounds;
	}
}
