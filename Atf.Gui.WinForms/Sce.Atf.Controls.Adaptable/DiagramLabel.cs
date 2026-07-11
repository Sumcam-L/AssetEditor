using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class DiagramLabel
{
	private readonly TextFormatFlags m_labelFormat;

	private readonly Rectangle m_labelBounds;

	public TextFormatFlags Format => m_labelFormat;

	public Rectangle Bounds => m_labelBounds;

	public DiagramLabel(Rectangle labelBounds, TextFormatFlags labelFormat)
	{
		m_labelBounds = labelBounds;
		m_labelFormat = labelFormat;
	}
}
