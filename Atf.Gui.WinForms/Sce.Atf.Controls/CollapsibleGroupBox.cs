using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Sce.Atf.Controls;

public class CollapsibleGroupBox : GroupBox
{
	private Button m_btn;

	private Font m_btnFont;

	private Size m_prevMinSize;

	private StringFormat m_btnFormat;

	private const int ButtonWidth = 16;

	private const int ButtonHeight = 16;

	private const int CollapseHeight = 16;

	private const string PaddingString = "   ";

	public new int Height
	{
		get
		{
			return IsCollapsed ? 16 : base.Height;
		}
		set
		{
			if (IsCollapsed)
			{
				LastHeight = value;
			}
			else
			{
				base.Height = value;
			}
		}
	}

	public override string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			base.Text = (base.DesignMode ? value : value.Insert(0, "   "));
		}
	}

	public int LastHeight { get; private set; }

	public bool IsCollapsed { get; private set; }

	public event EventHandler Collapsing;

	public event EventHandler Collapsed;

	public event EventHandler Expanding;

	public event EventHandler Expanded;

	public CollapsibleGroupBox()
	{
		m_btn = new Button
		{
			Location = new Point(0, 0),
			Size = new Size(16, 16)
		};
		m_btn.Paint += BtnPaint;
		m_btn.Click += BtnClick;
		m_btnFont = new Font(m_btn.Font.FontFamily, 8f);
		m_btnFormat = new StringFormat
		{
			Alignment = StringAlignment.Center
		};
		base.Controls.Add(m_btn);
	}

	public void Collapse()
	{
		if (IsCollapsed)
		{
			return;
		}
		this.Collapsing.Raise(this, EventArgs.Empty);
		foreach (Control control in base.Controls)
		{
			if (control != m_btn)
			{
				control.Hide();
			}
		}
		m_prevMinSize = new Size(MinimumSize.Width, MinimumSize.Height);
		MinimumSize = new Size(MinimumSize.Width, 16);
		LastHeight = base.Height;
		base.Height = 16;
		IsCollapsed = true;
		Invalidate();
		m_btn.Invalidate();
		this.Collapsed.Raise(this, EventArgs.Empty);
	}

	public void Expand()
	{
		if (!IsCollapsed)
		{
			return;
		}
		this.Expanding.Raise(this, EventArgs.Empty);
		foreach (Control control in base.Controls)
		{
			if (control != m_btn)
			{
				control.Show();
			}
		}
		MinimumSize = new Size(m_prevMinSize.Width, m_prevMinSize.Height);
		base.Height = LastHeight;
		LastHeight = 16;
		IsCollapsed = false;
		Invalidate();
		m_btn.Invalidate();
		this.Expanded.Raise(this, EventArgs.Empty);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_btn != null)
			{
				m_btn.Dispose();
				m_btn = null;
			}
			if (m_btnFont != null)
			{
				m_btnFont.Dispose();
				m_btnFont = null;
			}
			if (m_btnFormat != null)
			{
				m_btnFormat.Dispose();
				m_btnFormat = null;
			}
		}
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		GroupBoxRenderer.DrawGroupBox(e.Graphics, clientRectangle, Text, Font, ForeColor, TextFormatFlags.Default, base.Enabled ? GroupBoxState.Normal : GroupBoxState.Disabled);
	}

	private void BtnPaint(object sender, PaintEventArgs e)
	{
		e.Graphics.DrawString(IsCollapsed ? "+" : "-", m_btnFont, SystemBrushes.ControlText, new RectangleF(0f, 0f, m_btn.Width, m_btn.Height), m_btnFormat);
	}

	private void BtnClick(object sender, EventArgs e)
	{
		if (IsCollapsed)
		{
			Expand();
		}
		else
		{
			Collapse();
		}
	}
}
