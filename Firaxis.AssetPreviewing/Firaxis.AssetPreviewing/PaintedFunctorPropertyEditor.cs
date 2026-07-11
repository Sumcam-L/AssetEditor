using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.AssetPreviewing;

public class PaintedFunctorPropertyEditor : IPaintedPropertyEditor
{
	private static readonly StringFormat kCenterAligned = new StringFormat
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center,
		FormatFlags = StringFormatFlags.NoWrap
	};

	private PropertyDescriptor Property;

	private IFunctionKnob Knob;

	private bool m_pressed = false;

	public PaintedFunctorPropertyEditor(PropertyDescriptor prop, IFunctionKnob knob)
	{
		Property = prop;
		Knob = knob;
	}

	private Rectangle ComputeButtonRect(Rectangle valueRc)
	{
		Rectangle result = valueRc;
		result.Inflate(-24, 0);
		return result;
	}

	public Rectangle HandleMouseDown(MouseButtons downBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		Rectangle result = ComputeButtonRect(valueRc);
		if (result.Contains(clickPt))
		{
			Cursor.Current = Cursors.Hand;
			m_pressed = true;
			return result;
		}
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseUp(MouseButtons upBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		Rectangle result = ComputeButtonRect(valueRc);
		if (result.Contains(clickPt))
		{
			Knob.CallFunction();
		}
		Cursor.Current = Cursors.Arrow;
		m_pressed = false;
		return result;
	}

	public Rectangle HandleMouseMove(MouseButtons pressedBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		if (ComputeButtonRect(valueRc).Contains(clickPt))
		{
			Cursor.Current = Cursors.Hand;
		}
		else if (Cursor.Current == Cursors.Hand)
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
		Rectangle rectangle2 = ComputeButtonRect(rectangle);
		if (m_pressed)
		{
			ControlPaint.DrawButton(canvas, rectangle2, ButtonState.Pushed);
		}
		else
		{
			ControlPaint.DrawButton(canvas, rectangle2, ButtonState.Normal);
		}
		canvas.DrawString(Knob.Label, colors.Font, SystemBrushes.ControlText, rectangle2, kCenterAligned);
	}
}
