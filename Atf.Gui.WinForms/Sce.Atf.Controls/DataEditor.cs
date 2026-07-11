using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public abstract class DataEditor
{
	public enum EditMode
	{
		None,
		ByTextBox,
		BySlider,
		ByExternalControl
	}

	public Action FinishDataEdit;

	public object Owner;

	public string Name;

	public bool ReadOnly;

	private readonly DataEditorTheme m_theme;

	public DataEditorTheme Theme => m_theme;

	public EditMode EditingMode { get; set; }

	public TextBox TextBox { get; set; }

	public Rectangle Bounds { get; set; }

	protected DataEditor(DataEditorTheme theme)
	{
		m_theme = theme;
		EditingMode = EditMode.None;
	}

	public virtual bool WantsMouseTracking()
	{
		return false;
	}

	public virtual SizeF Measure(Graphics g, SizeF availableSize)
	{
		return availableSize;
	}

	public virtual SizeF Arrange(SizeF finalSize)
	{
		return finalSize;
	}

	public virtual void PaintValue(Graphics g, Rectangle area)
	{
	}

	public virtual void SetEditingMode(Point p)
	{
	}

	public virtual void BeginDataEdit()
	{
	}

	public virtual bool EndDataEdit()
	{
		return false;
	}

	public virtual void OnMouseMove(MouseEventArgs e)
	{
	}

	public virtual void OnMouseDown(MouseEventArgs e)
	{
	}

	public abstract void Parse(string s);
}
