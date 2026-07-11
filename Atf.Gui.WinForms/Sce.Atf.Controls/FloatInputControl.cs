using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class FloatInputControl : UserControl
{
	private float m_value;

	private float m_lastChange;

	private float m_lastEdit;

	private float m_min;

	private float m_max;

	private bool m_drawBorder = true;

	private CompactSpinner m_spinner;

	private NumericTextBox m_textBox;

	public float Value
	{
		get
		{
			return m_value;
		}
		set
		{
			SetValue(value, forceNewValue: false, notifyOfChange: true);
		}
	}

	public float LastChange => m_lastChange;

	public float LastEdit => m_lastEdit;

	public float Min
	{
		get
		{
			return m_min;
		}
		set
		{
			if (value >= m_max)
			{
				throw new ArgumentException("Min");
			}
			m_min = value;
			if (m_value < m_min)
			{
				Value = m_min;
			}
			Invalidate();
		}
	}

	public float Max
	{
		get
		{
			return m_max;
		}
		set
		{
			if (value <= m_min)
			{
				throw new ArgumentException("Max");
			}
			m_max = value;
			if (m_value > m_max)
			{
				Value = m_max;
			}
			Invalidate();
		}
	}

	public bool DrawBorder
	{
		get
		{
			return m_drawBorder;
		}
		set
		{
			m_drawBorder = value;
		}
	}

	public event EventHandler ValueChanged;

	public event EventHandler ValueEdited;

	public FloatInputControl()
		: this(0f, 0f, 100f)
	{
	}

	public FloatInputControl(float value, float min, float max)
	{
		if (min >= max)
		{
			throw new ArgumentException("min must be less than max");
		}
		DoubleBuffered = true;
		m_min = min;
		m_max = max;
		m_value = MathUtil.Clamp(value, m_min, m_max);
		m_lastChange = m_value;
		m_lastEdit = m_value;
		m_textBox = new NumericTextBox();
		m_textBox.BorderStyle = BorderStyle.None;
		m_textBox.Name = "m_textBox";
		m_spinner = new CompactSpinner();
		m_spinner.GotFocus += delegate
		{
			m_textBox.Focus();
		};
		SuspendLayout();
		UpdateTextBox();
		base.Controls.Add(m_textBox);
		base.Controls.Add(m_spinner);
		ResumeLayout(performLayout: false);
		PerformLayout();
		m_textBox.ValueEdited += delegate
		{
			float value2 = (float)m_textBox.Value;
			SetValue(value2, forceNewValue: false, notifyOfChange: true);
			EndEdit(forceNewValue: true);
		};
		m_spinner.Changing += delegate(object sender, SpinDirectionEventArgs e)
		{
			float num = (m_max - m_min) / 100f;
			float value2 = Value + (float)e.Value * num;
			SetValue(value2, forceNewValue: false, notifyOfChange: false);
		};
		m_spinner.Changed += delegate(object sender, SpinDirectionEventArgs e)
		{
			float num = (m_max - m_min) / 100f;
			float value2 = Value + (float)e.Value * num;
			SetValue(value2, forceNewValue: false, notifyOfChange: true);
		};
		m_textBox.SizeChanged += delegate
		{
			base.Height = m_textBox.Height + 3;
		};
		base.SizeChanged += delegate
		{
			m_spinner.Bounds = new Rectangle(0, 0, base.Height, base.Height);
			m_textBox.Bounds = new Rectangle(m_spinner.Width, 0, base.Width - m_spinner.Width, m_textBox.Height);
		};
	}

	protected virtual float GetValue(float position)
	{
		return Constrain(m_min + position * (m_max - m_min));
	}

	protected virtual float GetPosition(float value)
	{
		value = Constrain(value);
		return (value - m_min) / (m_max - m_min);
	}

	protected virtual void OnValueChanged(EventArgs e)
	{
		this.ValueChanged.Raise(this, e);
	}

	protected virtual void OnValueEdited(EventArgs e)
	{
		this.ValueEdited.Raise(this, e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		float num = (m_value - m_min) / (m_max - m_min);
		int num2 = (int)(num * (float)m_textBox.Width);
		Rectangle rect = new Rectangle(m_textBox.Location.X, m_textBox.Height, num2, 3);
		e.Graphics.FillRectangle(Brushes.LightBlue, rect);
		if (m_drawBorder)
		{
			ControlPaint.DrawBorder3D(e.Graphics, base.ClientRectangle, Border3DStyle.Flat);
		}
	}

	private void EndEdit(bool forceNewValue)
	{
		if (forceNewValue || m_value != m_lastEdit)
		{
			OnValueEdited(EventArgs.Empty);
			m_lastEdit = m_value;
		}
	}

	private void SetValue(float value, bool forceNewValue, bool notifyOfChange)
	{
		value = MathUtil.Clamp(value, m_min, m_max);
		if (forceNewValue || notifyOfChange || value != m_value)
		{
			m_value = value;
			if (notifyOfChange)
			{
				OnValueChanged(EventArgs.Empty);
			}
			m_lastChange = value;
		}
		UpdateTextBox();
		Invalidate();
	}

	private void UpdateTextBox()
	{
		m_textBox.Value = m_value;
	}

	private float Constrain(float value)
	{
		return MathUtil.Clamp(value, m_min, m_max);
	}
}
