using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetPreviewing;

public class CustomEnumPropertyEditor : IPaintedPropertyEditor, IActivatablePropertyEditor
{
	private static readonly StringFormat kLeftAlignedText = new StringFormat
	{
		LineAlignment = StringAlignment.Center,
		Alignment = StringAlignment.Near,
		FormatFlags = StringFormatFlags.NoWrap
	};

	private static readonly StringFormat kRightAlignedText = new StringFormat
	{
		LineAlignment = StringAlignment.Center,
		Alignment = StringAlignment.Far,
		FormatFlags = StringFormatFlags.NoWrap
	};

	private PropertyDescriptor Property;

	private bool m_pressed;

	private IContainerKnob<string> m_enumKnob;

	private string m_initialValue;

	private HotTrackListBox m_listBox;

	public bool IsInline => false;

	public bool Active => m_listBox != null;

	public Control PropertyControl => m_listBox;

	public event EventHandler ValueCommitted;

	public CustomEnumPropertyEditor(PropertyDescriptor prop, IContainerKnob<string> container)
	{
		Property = prop;
		m_enumKnob = container;
	}

	private Rectangle ComputeDropdownBoxRect(Rectangle valueRc)
	{
		int num = Math.Min(valueRc.Width, valueRc.Height);
		Rectangle result = new Rectangle(0, valueRc.Top, num, num);
		result.Offset(valueRc.Right - result.Width, 0);
		result.Inflate(-1, -1);
		return result;
	}

	private Rectangle ComputeTextRect(Rectangle valueRc)
	{
		Rectangle rect = ComputeDropdownBoxRect(valueRc);
		BugSubmitter.Assert(valueRc.Contains(rect), "Value rect must contain the drop rect in its entierty");
		int num = valueRc.Left + (int)((float)valueRc.Width / 2f + 0.5f);
		if (num <= rect.Left)
		{
			return new Rectangle(valueRc.Left, valueRc.Top, valueRc.Width - rect.Width - 1, valueRc.Height);
		}
		return new Rectangle(rect.Right + 1, valueRc.Top, valueRc.Width - rect.Width - 1, valueRc.Height);
	}

	public Rectangle HandleMouseDown(MouseButtons downBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		Rectangle result = ComputeDropdownBoxRect(valueRc);
		if (result.Contains(clickPt))
		{
			m_pressed = true;
			return result;
		}
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseUp(MouseButtons upBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		m_pressed = false;
		return ComputeDropdownBoxRect(valueRc);
	}

	public Rectangle HandleMouseMove(MouseButtons pressedBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		if (valueRc.Contains(clickPt))
		{
			Cursor.Current = Cursors.Hand;
		}
		else
		{
			Cursor.Current = Cursors.Arrow;
		}
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseClick(MouseButtons btns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		return Rectangle.Empty;
	}

	public void PaintValue(PaintedStyleInfo colors, PaintedState state, object value, Graphics canvas, Rectangle rectangle)
	{
		Rectangle rectangle2 = ComputeDropdownBoxRect(rectangle);
		Rectangle rectangle3 = ComputeTextRect(rectangle);
		using (SolidBrush brush = (state.Selected ? new SolidBrush(colors.SelectedForeColor) : new SolidBrush(colors.ForeColor)))
		{
			if (rectangle3.Left < rectangle2.Left)
			{
				canvas.DrawString(m_enumKnob.Value, colors.Font, brush, rectangle3, kLeftAlignedText);
			}
			else
			{
				canvas.DrawString(m_enumKnob.Value, colors.Font, brush, rectangle3, kRightAlignedText);
			}
		}
		if (m_pressed)
		{
			ControlPaint.DrawComboButton(canvas, rectangle2, ButtonState.Pushed);
		}
		else
		{
			ControlPaint.DrawComboButton(canvas, rectangle2, ButtonState.Normal);
		}
	}

	public Control ActivatePropertyControl(object component, PropertyDescriptor prop)
	{
		BugSubmitter.Assert(m_listBox == null, $"{typeof(HotTrackListBox)} already active!");
		if (m_listBox != null)
		{
			return m_listBox;
		}
		m_listBox = new HotTrackListBox();
		m_initialValue = m_enumKnob.Value;
		int num = -1;
		foreach (string value in m_enumKnob.Values)
		{
			int num2 = m_listBox.Items.Add(value);
			if (value.Equals(m_initialValue))
			{
				num = num2;
			}
		}
		if (num > 0)
		{
			m_listBox.SelectedIndex = num;
		}
		m_listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
		m_listBox.MouseClick += ListBox_MouseClick;
		m_listBox.KeyDown += ListBox_KeyDown;
		m_listBox.LostFocus += ListBox_LostFocus;
		return m_listBox;
	}

	private void ListBox_LostFocus(object sender, EventArgs e)
	{
		this.ValueCommitted.Raise(this, EventArgs.Empty);
	}

	private void ListBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			Property.SetValue(m_enumKnob, m_initialValue);
			this.ValueCommitted.Raise(this, EventArgs.Empty);
		}
		else if (e.KeyCode == Keys.Return)
		{
			this.ValueCommitted.Raise(this, EventArgs.Empty);
		}
	}

	private void ListBox_MouseClick(object sender, MouseEventArgs e)
	{
		this.ValueCommitted.Raise(this, EventArgs.Empty);
	}

	private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_listBox.SelectedIndex != -1)
		{
			string value = (string)m_listBox.Items[m_listBox.SelectedIndex];
			Property.SetValue(m_enumKnob, value);
		}
		else
		{
			Property.SetValue(m_enumKnob, string.Empty);
		}
	}

	public void DeactivateControl()
	{
		BugSubmitter.Assert(m_listBox != null, $"{typeof(HotTrackListBox)} not active!");
		m_listBox.SelectedIndexChanged -= ListBox_SelectedIndexChanged;
		m_listBox.MouseClick -= ListBox_MouseClick;
		m_listBox.KeyDown -= ListBox_KeyDown;
		m_listBox.Dispose();
		m_listBox = null;
	}
}
