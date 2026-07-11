using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Sce.Atf.Controls.DataEditing;

public class ColorDataEditor : DataEditor
{
	public Color Value;

	private Color m_startValue;

	private static int[] s_customColors = new int[0];

	public ColorDataEditor(DataEditorTheme theme)
		: base(theme)
	{
	}

	public override void Parse(string s)
	{
		Value = (int.TryParse(s, out var result) ? Color.FromArgb(result) : Color.FromName(s));
	}

	public override string ToString()
	{
		return Value.ToArgb().ToString(CultureInfo.InvariantCulture);
	}

	public override void PaintValue(Graphics g, Rectangle area)
	{
		base.Theme.SolidBrush.Color = Value;
		area.X += base.Theme.Padding.Left;
		area.Width -= base.Theme.Padding.Left + base.Theme.Padding.Right;
		if (area.Width > 0)
		{
			g.FillRectangle(base.Theme.SolidBrush, area);
		}
	}

	public override void SetEditingMode(Point p)
	{
		if (base.Bounds.Contains(p))
		{
			base.EditingMode = EditMode.ByExternalControl;
		}
		else
		{
			base.EditingMode = EditMode.None;
		}
	}

	public override void BeginDataEdit()
	{
		m_startValue = Value;
		if (base.EditingMode == EditMode.ByExternalControl)
		{
			ColorDialog colorDialog = new ColorDialog();
			colorDialog.SolidColorOnly = false;
			colorDialog.AnyColor = true;
			colorDialog.CustomColors = s_customColors;
			colorDialog.Color = Value;
			DialogResult dialogResult = colorDialog.ShowDialog();
			s_customColors = colorDialog.CustomColors;
			if (dialogResult == DialogResult.OK)
			{
				Value = colorDialog.Color;
			}
			if (FinishDataEdit != null)
			{
				FinishDataEdit();
			}
		}
	}

	public override bool EndDataEdit()
	{
		return m_startValue != Value;
	}
}
