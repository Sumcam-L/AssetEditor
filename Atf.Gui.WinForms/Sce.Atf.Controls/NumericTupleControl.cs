using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class NumericTupleControl : Control
{
	private Type m_numericType;

	private double m_scaleFactor = 1.0;

	private Array m_lastChange;

	private Array m_lastEdit;

	private int m_component;

	private int[] m_labelWidth;

	private Color[] m_labelColors;

	private bool m_editing;

	private static StringFormat s_lblFormat;

	public bool HideAxisLabel { get; set; }

	public double ScaleFactor
	{
		get
		{
			return m_scaleFactor;
		}
		set
		{
			if (value == 0.0)
			{
				throw new ArgumentException("value must be non-zero");
			}
			m_scaleFactor = value;
			foreach (NumericTextBox control in base.Controls)
			{
				control.ScaleFactor = m_scaleFactor;
			}
		}
	}

	public object Value
	{
		get
		{
			if (m_numericType == typeof(long))
			{
				return GetValue<long>();
			}
			if (m_numericType == typeof(ulong))
			{
				return GetValue<ulong>();
			}
			if (m_numericType == typeof(int))
			{
				return GetValue<int>();
			}
			if (m_numericType == typeof(uint))
			{
				return GetValue<uint>();
			}
			if (m_numericType == typeof(short))
			{
				return GetValue<short>();
			}
			if (m_numericType == typeof(ushort))
			{
				return GetValue<ushort>();
			}
			if (m_numericType == typeof(sbyte))
			{
				return GetValue<sbyte>();
			}
			if (m_numericType == typeof(byte))
			{
				return GetValue<byte>();
			}
			if (m_numericType == typeof(float))
			{
				return GetValue<float>();
			}
			if (m_numericType == typeof(double))
			{
				return GetValue<double>();
			}
			return null;
		}
		set
		{
			if (!(value is Array array))
			{
				throw new ArgumentException("value must be array");
			}
			if (array.Length != base.Controls.Count)
			{
				throw new ArgumentException("array has the wrong dimension");
			}
			m_lastChange = (Array)array.Clone();
			if (!m_editing)
			{
				m_lastEdit = m_lastChange;
			}
			for (int i = 0; i < array.Length; i++)
			{
				NumericTextBox numericTextBox = base.Controls[i] as NumericTextBox;
				object value2 = array.GetValue(i);
				if (value2.GetType() != m_numericType)
				{
					throw new ArgumentException("Tuple coordinate type not supported");
				}
				numericTextBox.Value = value2;
			}
		}
	}

	public Array LastChange => m_lastChange;

	public Array LastEdit => m_lastEdit;

	public int Component => m_component;

	public event EventHandler ValueChanged;

	public event EventHandler ValueEdited;

	public NumericTupleControl()
		: this(typeof(float), new string[3] { "x", "y", "z" })
	{
	}

	public NumericTupleControl(Type numericType, string[] names)
	{
		m_labelColors = new Color[4]
		{
			Color.FromArgb(200, 40, 0),
			Color.FromArgb(100, 160, 0),
			Color.FromArgb(40, 120, 240),
			Color.FromArgb(20, 20, 20)
		};
		Define(numericType, names);
	}

	public void SetLabelBackColors(Color[] color)
	{
		if (color == null || color.Length == 0)
		{
			throw new ArgumentException("color");
		}
		m_labelColors = color;
	}

	public void Define(Type numericType, string[] names)
	{
		if (numericType != typeof(long) && numericType != typeof(ulong) && numericType != typeof(int) && numericType != typeof(uint) && numericType != typeof(short) && numericType != typeof(ushort) && numericType != typeof(sbyte) && numericType != typeof(byte) && numericType != typeof(float) && numericType != typeof(double))
		{
			throw new ArgumentException("Unsupported numeric type");
		}
		if (names == null || names.Length == 0)
		{
			throw new ArgumentException("Must have at least 1 coordinate in the tuple");
		}
		DoubleBuffered = true;
		m_numericType = numericType;
		while (base.Controls.Count > 0)
		{
			base.Controls[0].Dispose();
		}
		if (s_lblFormat == null)
		{
			s_lblFormat = new StringFormat();
			s_lblFormat.Alignment = StringAlignment.Center;
			s_lblFormat.LineAlignment = StringAlignment.Near;
			s_lblFormat.Trimming = StringTrimming.Character;
		}
		m_labelWidth = new int[names.Length];
		m_labelWidth[0] = -1;
		SuspendLayout();
		base.TabStop = false;
		for (int i = 0; i < names.Length; i++)
		{
			NumericTextBox numericTextBox = new NumericTextBox(m_numericType);
			numericTextBox.BorderStyle = BorderStyle.None;
			numericTextBox.TabStop = false;
			numericTextBox.Name = names[i];
			numericTextBox.ScaleFactor = m_scaleFactor;
			numericTextBox.ValueEdited += textBox_ValueEdited;
			base.Controls.Add(numericTextBox);
		}
		ResumeLayout();
	}

	protected override void OnResize(EventArgs e)
	{
		UpdateHeight();
		int num = base.Width / base.Controls.Count;
		SuspendLayout();
		if (HideAxisLabel)
		{
			int num2 = 0;
			foreach (NumericTextBox control2 in base.Controls)
			{
				control2.BorderStyle = BorderStyle.FixedSingle;
				control2.Bounds = new Rectangle(num2, 0, num, control2.Height);
				num2 += num;
			}
		}
		else
		{
			if (m_labelWidth[0] == -1)
			{
				ComputeLabelWidth();
			}
			int num3 = 0;
			int num4 = 0;
			foreach (Control control3 in base.Controls)
			{
				int num5 = m_labelWidth[num4++];
				control3.Bounds = new Rectangle(num3 + num5 + 3, 0, num - num5 - 3, control3.Height);
				num3 += num;
			}
		}
		ResumeLayout(performLayout: true);
		base.OnResize(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (HideAxisLabel)
		{
			return;
		}
		int num = base.Width / base.Controls.Count;
		int num2 = 0;
		int num3 = base.Controls[0].Height + 1;
		using SolidBrush solidBrush = new SolidBrush(Color.Black);
		int num4 = 0;
		foreach (Control control in base.Controls)
		{
			solidBrush.Color = m_labelColors[num4 % m_labelColors.Length];
			Rectangle rectangle = new Rectangle(num2, 1, m_labelWidth[num4], num3);
			e.Graphics.FillRectangle(solidBrush, rectangle);
			e.Graphics.DrawString(control.Name, Font, Brushes.White, rectangle, s_lblFormat);
			num2 += num;
			num4++;
		}
	}

	protected virtual void OnValueChanged(EventArgs e)
	{
		this.ValueChanged.Raise(this, e);
	}

	protected virtual void OnValueEdited(EventArgs e)
	{
		this.ValueEdited.Raise(this, e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		UpdateHeight();
		ComputeLabelWidth();
		PerformLayout();
		Invalidate();
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		NumericTextBox numericTextBox = null;
		foreach (NumericTextBox control in base.Controls)
		{
			if (control.Focused)
			{
				numericTextBox = control;
				break;
			}
		}
		int num = ((numericTextBox == null) ? (-1) : base.Controls.IndexOf(numericTextBox));
		if (keyData == Keys.Tab || keyData == Keys.Return)
		{
			NumericTextBox numericTextBox3 = (NumericTextBox)base.Controls[base.Controls.Count - 1];
			if (numericTextBox != numericTextBox3)
			{
				base.Controls[num + 1].Focus();
				return true;
			}
		}
		else if (keyData == (Keys.Tab | Keys.Shift))
		{
			NumericTextBox numericTextBox4 = (NumericTextBox)base.Controls[0];
			if (numericTextBox != numericTextBox4 && num != -1)
			{
				base.Controls[num - 1].Focus();
				return true;
			}
		}
		return base.ProcessDialogKey(keyData);
	}

	private void UpdateHeight()
	{
		if (base.Controls.Count > 0)
		{
			base.Height = base.Controls[0].Height;
		}
	}

	private void textBox_ValueEdited(object sender, EventArgs e)
	{
		NumericTextBox control = (NumericTextBox)sender;
		m_component = base.Controls.IndexOf(control);
		Array array = Value as Array;
		m_editing = true;
		OnValueChanged(EventArgs.Empty);
		m_lastChange = array;
		if (!base.ContainsFocus && m_editing)
		{
			OnValueEdited(EventArgs.Empty);
			m_lastEdit = array;
			m_editing = false;
		}
	}

	private T[] GetValue<T>()
	{
		T[] array = new T[base.Controls.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (T)((NumericTextBox)base.Controls[i]).Value;
		}
		return array;
	}

	private void ComputeLabelWidth()
	{
		int num = 0;
		foreach (NumericTextBox control in base.Controls)
		{
			m_labelWidth[num++] = TextRenderer.MeasureText(control.Name, Font).Width;
		}
	}
}
