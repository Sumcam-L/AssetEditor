using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class GraphNodeOptions
{
	private Color m_fillColor = Color.LightSteelBlue;

	public Color FillColor
	{
		get
		{
			return m_fillColor;
		}
		set
		{
			m_fillColor = value;
		}
	}

	public bool Pinned { get; set; }
}
