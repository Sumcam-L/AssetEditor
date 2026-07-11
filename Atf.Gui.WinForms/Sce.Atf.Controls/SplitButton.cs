using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Sce.Atf.Controls;

public class SplitButton : Button
{
	private const int PushButtonWidth = 14;

	private static readonly int BorderSize = SystemInformation.Border3DSize.Width * 2;

	private PushButtonState m_state;

	private bool m_skipNextOpen;

	private Rectangle m_dropDownRectangle;

	private bool m_showSplit = true;

	[DefaultValue(true)]
	public bool ShowSplit
	{
		set
		{
			if (value != m_showSplit)
			{
				m_showSplit = value;
				Invalidate();
				if (base.Parent != null)
				{
					base.Parent.PerformLayout();
				}
			}
		}
	}

	private PushButtonState MState
	{
		get
		{
			return m_state;
		}
		set
		{
			if (!m_state.Equals(value))
			{
				m_state = value;
				Invalidate();
			}
		}
	}

	public SplitButton()
	{
		AutoSize = true;
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		Size preferredSize = base.GetPreferredSize(proposedSize);
		if (m_showSplit && !string.IsNullOrEmpty(Text) && TextRenderer.MeasureText(Text, Font).Width + 14 > preferredSize.Width)
		{
			return preferredSize + new Size(14 + BorderSize * 2, 0);
		}
		return preferredSize;
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (keyData.Equals(Keys.Down) && m_showSplit)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		if (!m_showSplit)
		{
			base.OnGotFocus(e);
		}
		else if (!MState.Equals(PushButtonState.Pressed) && !MState.Equals(PushButtonState.Disabled))
		{
			MState = PushButtonState.Default;
		}
	}

	protected override void OnKeyDown(KeyEventArgs kevent)
	{
		if (m_showSplit)
		{
			if (kevent.KeyCode.Equals(Keys.Down))
			{
				ShowContextMenuStrip();
			}
			else if (kevent.KeyCode.Equals(Keys.Space) && kevent.Modifiers == Keys.None)
			{
				MState = PushButtonState.Pressed;
			}
		}
		base.OnKeyDown(kevent);
	}

	protected override void OnKeyUp(KeyEventArgs kevent)
	{
		if (kevent.KeyCode.Equals(Keys.Space) && Control.MouseButtons == MouseButtons.None)
		{
			MState = PushButtonState.Normal;
		}
		base.OnKeyUp(kevent);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		if (!m_showSplit)
		{
			base.OnLostFocus(e);
		}
		else if (!MState.Equals(PushButtonState.Pressed) && !MState.Equals(PushButtonState.Disabled))
		{
			MState = PushButtonState.Normal;
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (!m_showSplit)
		{
			base.OnMouseDown(e);
		}
		else if (m_dropDownRectangle.Contains(e.Location))
		{
			ShowContextMenuStrip();
		}
		else
		{
			MState = PushButtonState.Pressed;
		}
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		if (!m_showSplit)
		{
			base.OnMouseEnter(e);
		}
		else if (!MState.Equals(PushButtonState.Pressed) && !MState.Equals(PushButtonState.Disabled))
		{
			MState = PushButtonState.Hot;
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		if (!m_showSplit)
		{
			base.OnMouseLeave(e);
		}
		else if (!MState.Equals(PushButtonState.Pressed) && !MState.Equals(PushButtonState.Disabled))
		{
			if (Focused)
			{
				MState = PushButtonState.Default;
			}
			else
			{
				MState = PushButtonState.Normal;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs mevent)
	{
		if (!m_showSplit)
		{
			base.OnMouseUp(mevent);
		}
		else if (ContextMenuStrip == null || !ContextMenuStrip.Visible)
		{
			SetButtonDrawState();
			if (base.Bounds.Contains(base.Parent.PointToClient(Cursor.Position)) && !m_dropDownRectangle.Contains(mevent.Location))
			{
				OnClick(new EventArgs());
			}
		}
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		base.OnPaint(pevent);
		if (!m_showSplit)
		{
			return;
		}
		Graphics graphics = pevent.Graphics;
		Rectangle clientRectangle = base.ClientRectangle;
		if (MState != PushButtonState.Pressed && base.IsDefault && !Application.RenderWithVisualStyles)
		{
			Rectangle bounds = clientRectangle;
			bounds.Inflate(-1, -1);
			ButtonRenderer.DrawButton(graphics, bounds, MState);
			graphics.DrawRectangle(SystemPens.WindowFrame, 0, 0, clientRectangle.Width - 1, clientRectangle.Height - 1);
		}
		else
		{
			ButtonRenderer.DrawButton(graphics, clientRectangle, MState);
		}
		m_dropDownRectangle = new Rectangle(clientRectangle.Right - 14 - 1, BorderSize, 14, clientRectangle.Height - BorderSize * 2);
		int borderSize = BorderSize;
		Rectangle rectangle = new Rectangle(borderSize, borderSize, clientRectangle.Width - m_dropDownRectangle.Width - borderSize, clientRectangle.Height - borderSize * 2);
		bool flag = MState == PushButtonState.Hot || MState == PushButtonState.Pressed || !Application.RenderWithVisualStyles;
		if (RightToLeft == RightToLeft.Yes)
		{
			m_dropDownRectangle.X = clientRectangle.Left + 1;
			rectangle.X = m_dropDownRectangle.Right;
			if (flag)
			{
				graphics.DrawLine(SystemPens.ButtonShadow, clientRectangle.Left + 14, BorderSize, clientRectangle.Left + 14, clientRectangle.Bottom - BorderSize);
				graphics.DrawLine(SystemPens.ButtonFace, clientRectangle.Left + 14 + 1, BorderSize, clientRectangle.Left + 14 + 1, clientRectangle.Bottom - BorderSize);
			}
		}
		else if (flag)
		{
			graphics.DrawLine(SystemPens.ButtonShadow, clientRectangle.Right - 14, BorderSize, clientRectangle.Right - 14, clientRectangle.Bottom - BorderSize);
			graphics.DrawLine(SystemPens.ButtonFace, clientRectangle.Right - 14 - 1, BorderSize, clientRectangle.Right - 14 - 1, clientRectangle.Bottom - BorderSize);
		}
		PaintArrow(graphics, m_dropDownRectangle);
		TextFormatFlags textFormatFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
		if (!base.UseMnemonic)
		{
			textFormatFlags |= TextFormatFlags.NoPrefix;
		}
		else if (!ShowKeyboardCues)
		{
			textFormatFlags |= TextFormatFlags.HidePrefix;
		}
		if (!string.IsNullOrEmpty(Text))
		{
			TextRenderer.DrawText(graphics, Text, Font, rectangle, SystemColors.ControlText, textFormatFlags);
		}
		if (MState != PushButtonState.Pressed && Focused)
		{
			ControlPaint.DrawFocusRectangle(graphics, rectangle);
		}
	}

	private void PaintArrow(Graphics g, Rectangle dropDownRect)
	{
		Point point = new Point(Convert.ToInt32(dropDownRect.Left + dropDownRect.Width / 2), Convert.ToInt32(dropDownRect.Top + dropDownRect.Height / 2));
		point.X += dropDownRect.Width % 2;
		Point[] points = new Point[3]
		{
			new Point(point.X - 2, point.Y - 1),
			new Point(point.X + 3, point.Y - 1),
			new Point(point.X, point.Y + 2)
		};
		g.FillPolygon(SystemBrushes.ControlText, points);
	}

	private void ShowContextMenuStrip()
	{
		if (m_skipNextOpen)
		{
			m_skipNextOpen = false;
			return;
		}
		MState = PushButtonState.Pressed;
		if (ContextMenuStrip != null)
		{
			ContextMenuStrip.Closing += ContextMenuStrip_Closing;
			ContextMenuStrip.Show(this, new Point(0, base.Height), ToolStripDropDownDirection.BelowRight);
		}
	}

	private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
	{
		if (sender is ContextMenuStrip contextMenuStrip)
		{
			contextMenuStrip.Closing -= ContextMenuStrip_Closing;
		}
		SetButtonDrawState();
		if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked)
		{
			m_skipNextOpen = m_dropDownRectangle.Contains(PointToClient(Cursor.Position));
		}
	}

	private void SetButtonDrawState()
	{
		if (base.Bounds.Contains(base.Parent.PointToClient(Cursor.Position)))
		{
			MState = PushButtonState.Hot;
		}
		else if (Focused)
		{
			MState = PushButtonState.Default;
		}
		else
		{
			MState = PushButtonState.Normal;
		}
	}
}
