using System.Drawing;

namespace Sce.Atf.Controls.Adaptable;

public class DiagramVisibilityCheck
{
	private readonly RectangleF m_checkBounds;

	public RectangleF Bounds => m_checkBounds;

	public DiagramVisibilityCheck(RectangleF checkBounds)
	{
		m_checkBounds = checkBounds;
	}
}
