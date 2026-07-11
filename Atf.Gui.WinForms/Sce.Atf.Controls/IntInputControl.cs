using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class IntInputControl : UserControl
{
	private int m_value;

	private int m_lastChange;

	private int m_lastEdit;

	private int m_min;

	private int m_max;

	private bool m_drawBorder = true;

	private CompactSpinner m_spinner;

	private NumericTextBox m_textBox;

	public int Value
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

	public int LastChange => m_lastChange;

	public int LastEdit => m_lastEdit;

	public int Min
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

	public int Max
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

	public IntInputControl()
		: this(0, 0, 100)
	{
	}

	public IntInputControl(int value, int min, int max)
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
		m_textBox = new NumericTextBox(typeof(int));
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
			int value2 = (int)m_textBox.Value;
			SetValue(value2, forceNewValue: false, notifyOfChange: true);
			EndEdit(forceNewValue: true);
		};
		m_spinner.Changing += delegate(object sender, SpinDirectionEventArgs e)
		{
			int value2 = Value + e.Value;
			SetValue(value2, forceNewValue: false, notifyOfChange: false);
		};
		m_spinner.Changed += delegate(object sender, SpinDirectionEventArgs e)
		{
			int value2 = Value + e.Value;
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
		float num = (float)(m_value - m_min) / (float)(m_max - m_min);
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

	private void SetValue(int value, bool forceNewValue, bool notifyOfChange)
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
}
