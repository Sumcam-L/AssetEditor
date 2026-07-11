using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class CustomToolStripRenderer : ToolStripProfessionalRenderer
{
	public Color ArrowColor { get; set; }

	public CustomToolStripRenderer()
	{
	}

	public CustomToolStripRenderer(ProfessionalColorTable professionalColorTable)
		: base(professionalColorTable)
	{
	}

	protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
	{
		e.ArrowColor = ArrowColor;
		base.OnRenderArrow(e);
	}
}
