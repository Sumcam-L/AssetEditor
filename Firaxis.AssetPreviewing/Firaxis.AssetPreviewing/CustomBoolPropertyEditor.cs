using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.AssetPreviewing;

public class CustomBoolPropertyEditor : IPaintedPropertyEditor
{
	private PropertyDescriptor Property;

	public CustomBoolPropertyEditor(PropertyDescriptor prop)
	{
		Property = prop;
	}

	private Rectangle ComputeCheckBoxRect(Rectangle valueRc)
	{
		int num = Math.Min(valueRc.Width, valueRc.Height);
		Rectangle result = new Rectangle(0, valueRc.Top, num, num);
		result.Offset(valueRc.Right - result.Width - 1, 0);
		result.Inflate(-1, -1);
		return result;
	}

	public Rectangle HandleMouseDown(MouseButtons downBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseUp(MouseButtons upBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseMove(MouseButtons pressedBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		Rectangle rectangle = ComputeCheckBoxRect(valueRc);
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
		Rectangle result = ComputeCheckBoxRect(valueRc);
		if (valueRc.Contains(clickPt))
		{
			Property.SetValue(null, !(bool)Property.GetValue(null));
			return result;
		}
		return Rectangle.Empty;
	}

	public void PaintValue(PaintedStyleInfo colors, PaintedState state, object value, Graphics canvas, Rectangle rectangle)
	{
		Rectangle rectangle2 = ComputeCheckBoxRect(rectangle);
		if ((bool)value)
		{
			ControlPaint.DrawCheckBox(canvas, rectangle2, ButtonState.Checked | ButtonState.Flat);
		}
		else
		{
			ControlPaint.DrawCheckBox(canvas, rectangle2, ButtonState.Flat);
		}
	}
}
