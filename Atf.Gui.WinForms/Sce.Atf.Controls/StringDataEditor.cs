using System.Drawing;

namespace Sce.Atf.Controls;

public class StringDataEditor : DataEditor
{
	public string Value;

	public StringDataEditor(DataEditorTheme theme)
		: base(theme)
	{
	}

	public override SizeF Measure(Graphics g, SizeF availableSize)
	{
		SizeF result = g.MeasureString(Value, base.Theme.Font);
		result.Width += base.Theme.Padding.Left;
		return result;
	}

	public override void PaintValue(Graphics g, Rectangle area)
	{
		if (ReadOnly)
		{
			g.DrawString(Value, base.Theme.Font, base.Theme.ReadonlyBrush, area.Left + base.Theme.Padding.Left, area.Top);
		}
		else
		{
			g.DrawString(Value, base.Theme.Font, base.Theme.TextBrush, area.Left + base.Theme.Padding.Left, area.Top);
		}
	}

	public override void SetEditingMode(Point p)
	{
		if (base.Bounds.Contains(p))
		{
			base.EditingMode = EditMode.ByTextBox;
		}
	}

	public override void BeginDataEdit()
	{
		if (base.EditingMode == EditMode.ByTextBox)
		{
			base.TextBox.Text = Value;
			base.TextBox.Bounds = base.Bounds;
			base.TextBox.SelectAll();
			base.TextBox.Show();
			base.TextBox.Focus();
		}
	}

	public override bool EndDataEdit()
	{
		if (base.EditingMode == EditMode.ByTextBox)
		{
			Parse(base.TextBox.Text);
		}
		return true;
	}

	public override string ToString()
	{
		return Value;
	}

	public override void Parse(string s)
	{
		Value = s;
	}
}
