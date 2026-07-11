using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Controls.ColorEditing;

namespace Firaxis.AssetPreviewing;

public class CustomColorPropertyEditor : IPaintedPropertyEditor, IActivatablePropertyEditor
{
	private static readonly StringFormat kLeftAlignedText = new StringFormat
	{
		LineAlignment = StringAlignment.Center,
		Alignment = StringAlignment.Near,
		FormatFlags = StringFormatFlags.NoWrap
	};

	private PropertyDescriptor Property;

	private TypeConverter Converter;

	private IValueKnob<Color> m_colorKnob;

	private Color m_initialValue;

	private ColorPickerControl m_colorPicker;

	public bool IsInline => false;

	public bool Active => m_colorPicker != null;

	public Control PropertyControl => m_colorPicker;

	public event EventHandler ValueCommitted;

	public CustomColorPropertyEditor(PropertyDescriptor prop, IValueKnob<Color> knob)
	{
		Property = prop;
		Converter = prop.Converter;
		m_colorKnob = knob;
	}

	private Rectangle ComputeColorSampleRect(Rectangle valueRc)
	{
		Rectangle result = new Rectangle(0, valueRc.Top, Math.Min(valueRc.Height * 2, (int)((float)valueRc.Width / 2f)), valueRc.Height);
		result.Offset(valueRc.Right - result.Width - 1, 0);
		result.Inflate(-1, -1);
		result.Height--;
		return result;
	}

	private Rectangle ComputeTextRect(Rectangle valueRc)
	{
		Rectangle rect = ComputeColorSampleRect(valueRc);
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
		Rectangle result = ComputeColorSampleRect(valueRc);
		if (result.Contains(clickPt))
		{
			return result;
		}
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseUp(MouseButtons upBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		return ComputeColorSampleRect(valueRc);
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
		Rectangle rectangle2 = ComputeColorSampleRect(rectangle);
		Rectangle rectangle3 = ComputeTextRect(rectangle);
		Color value2 = m_colorKnob.Value;
		string s = $"#{value2.A,2:X2}{value2.R,2:X2}{value2.G,2:X2}{value2.B,2:X2}";
		using (SolidBrush brush = (state.Selected ? new SolidBrush(colors.SelectedForeColor) : new SolidBrush(colors.ForeColor)))
		{
			canvas.DrawString(s, colors.Font, brush, rectangle3, kLeftAlignedText);
		}
		ControlPaint.DrawButton(canvas, rectangle2, ButtonState.Normal);
		rectangle2.Inflate(-1, -1);
		using SolidBrush brush2 = new SolidBrush(value2);
		canvas.FillRectangle(brush2, rectangle2);
	}

	public Control ActivatePropertyControl(object component, PropertyDescriptor prop)
	{
		BugSubmitter.Assert(m_colorPicker == null, $"{typeof(ColorPicker)} already active!");
		if (m_colorPicker != null)
		{
			return m_colorPicker;
		}
		m_initialValue = m_colorKnob.Value;
		m_colorPicker = new ColorPickerControl();
		m_colorPicker.SelectedColor = m_initialValue;
		m_colorPicker.ColorChanged += ColorPicker_ColorChanged;
		m_colorPicker.ColorCommited += ColorPicker_ColorCommited;
		m_colorPicker.KeyDown += ColorPicker_KeyDown;
		m_colorPicker.LostFocus += ColorPicker_LostFocus;
		return m_colorPicker;
	}

	private void ColorPicker_ColorChanged(object sender, EventArgs e)
	{
		Property.SetValue(m_colorKnob, m_colorPicker.SelectedColor);
	}

	private void ColorPicker_ColorCommited(object sender, EventArgs e)
	{
		this.ValueCommitted.Raise(this, EventArgs.Empty);
	}

	private void ColorPicker_LostFocus(object sender, EventArgs e)
	{
		this.ValueCommitted.Raise(this, EventArgs.Empty);
	}

	private void ColorPicker_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			Property.SetValue(m_colorKnob, m_initialValue);
			this.ValueCommitted.Raise(this, EventArgs.Empty);
		}
		else if (e.KeyCode == Keys.Return)
		{
			this.ValueCommitted.Raise(this, EventArgs.Empty);
		}
	}

	public void DeactivateControl()
	{
		BugSubmitter.Assert(m_colorPicker != null, $"{typeof(ColorPicker)} not active!");
		m_colorPicker.MouseClick -= ColorPicker_ColorCommited;
		m_colorPicker.KeyDown -= ColorPicker_KeyDown;
		m_colorPicker.LostFocus -= ColorPicker_LostFocus;
		m_colorPicker.Dispose();
		m_colorPicker = null;
	}
}
