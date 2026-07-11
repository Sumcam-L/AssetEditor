using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Sce.Atf.Controls;

public class FloatDataEditor : DataEditor
{
	public float Value;

	public float Min;

	public float Max;

	public bool ShowSlider;

	private float m_startValue;

	private int m_sliderWidth;

	public float Epsilon { get; set; }

	public int SliderWidth
	{
		get
		{
			return m_sliderWidth;
		}
		set
		{
			m_sliderWidth = value;
		}
	}

	public FloatDataEditor(DataEditorTheme theme)
		: base(theme)
	{
		SliderWidth = theme.DefaultSliderWidth;
		Epsilon = 1E-06f;
	}

	public override SizeF Measure(Graphics g, SizeF availableSize)
	{
		SizeF result = g.MeasureString(Value.ToString("F"), base.Theme.Font);
		result.Width += base.Theme.Padding.Left;
		if (ShowSlider)
		{
			result.Width += SliderWidth + base.Theme.Padding.Left;
		}
		return result;
	}

	public override void PaintValue(Graphics g, Rectangle area)
	{
		int num = 0;
		int num2 = area.Left + base.Theme.Padding.Left;
		if (ShowSlider)
		{
			float num3 = area.Y + area.Height / 2;
			g.DrawLine(base.Theme.SliderTrackPen, num2, num3, num2 + SliderWidth, num3);
			float num4 = (Value - Min) / (Max - Min);
			float num5 = (float)num2 + num4 * (float)SliderWidth;
			Rectangle bounds = new Rectangle((int)num5 - 8, area.Top, 18, 18);
			TrackBarRenderer.DrawBottomPointingThumb(g, bounds, TrackBarThumbState.Normal);
			num = SliderWidth + base.Theme.Padding.Left;
		}
		string s = ToString();
		g.DrawString(s, base.Theme.Font, base.Theme.TextBrush, num2 + num, area.Top);
	}

	public override void SetEditingMode(Point p)
	{
		int num = p.X - base.Bounds.Left;
		if (num >= base.Theme.Padding.Left && num <= base.Theme.Padding.Left + SliderWidth)
		{
			base.EditingMode = EditMode.BySlider;
		}
		else
		{
			base.EditingMode = EditMode.ByTextBox;
		}
	}

	public override void BeginDataEdit()
	{
		m_startValue = Value;
		if (base.EditingMode == EditMode.ByTextBox)
		{
			base.TextBox.Text = Value.ToString("F");
			base.TextBox.Bounds = new Rectangle(base.Bounds.Left + SliderWidth + base.Theme.Padding.Left, base.Bounds.Top, base.Bounds.Width - SliderWidth - base.Theme.Padding.Left, base.Bounds.Height);
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
		return m_startValue != Value && !(Math.Abs(Value - m_startValue) < Epsilon);
	}

	public override void OnMouseMove(MouseEventArgs e)
	{
		if (base.EditingMode == EditMode.BySlider)
		{
			Value = GetSliderFloatValue(e.X);
		}
	}

	public override void OnMouseDown(MouseEventArgs e)
	{
		if (base.EditingMode == EditMode.BySlider)
		{
			Value = GetSliderFloatValue(e.X);
		}
	}

	private float GetSliderFloatValue(int x)
	{
		float num = (float)(x - base.Bounds.Left - base.Theme.Padding.Left) / (float)SliderWidth;
		float value = Min + num * (Max - Min);
		return MathUtil.Clamp(value, Min, Max);
	}

	public override bool WantsMouseTracking()
	{
		return base.EditingMode == EditMode.BySlider;
	}

	public override string ToString()
	{
		return Value.ToString("F");
	}

	public override void Parse(string s)
	{
		if (float.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out var result))
		{
			if (Min == 0f && Max == 0f)
			{
				Value = result;
			}
			else
			{
				Value = MathUtil.Clamp(result, Min, Max);
			}
		}
	}
}
