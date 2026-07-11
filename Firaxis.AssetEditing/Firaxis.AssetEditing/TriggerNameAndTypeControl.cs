using System;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class TriggerNameAndTypeControl : Control
{
	private PropertyEditorControlContext ControlContext { get; set; }

	public TriggerNameAndTypeControl(PropertyEditorControlContext context)
	{
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		ControlContext = context;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		using Brush brush = new SolidBrush(ForeColor);
		e.Graphics.DrawString(ControlContext.GetPropertyText().ToString(), Font, brush, 0f, 0f);
	}
}
