using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class NumericMatrixControl : Control
{
	private Type m_numericType;

	private int m_rows;

	private int m_columns;

	private double m_scaleFactor = 1.0;

	private Array m_lastChange;

	private Array m_lastEdit;

	private int m_component;

	private bool m_editing;

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

	public virtual object Value
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
					throw new ArgumentException("Matrix element type not supported");
				}
				numericTextBox.Value = value2;
			}
		}
	}

	public Array LastChange
	{
		get
		{
			return m_lastChange;
		}
		protected set
		{
			m_lastChange = value;
		}
	}

	public Array LastEdit
	{
		get
		{
			return m_lastEdit;
		}
		protected set
		{
			m_lastEdit = value;
		}
	}

	public bool IsEditing => m_editing;

	protected Type NumericType => m_numericType;

	protected int Component => m_component;

	public event EventHandler ValueChanged;

	public event EventHandler ValueEdited;

	public NumericMatrixControl()
		: this(typeof(float), 4, 4)
	{
	}

	public NumericMatrixControl(Type numericType, int rows, int columns)
	{
		Define(numericType, rows, columns);
	}

	public void Define(Type numericType, int rows, int columns)
	{
		if (numericType != typeof(long) && numericType != typeof(ulong) && numericType != typeof(int) && numericType != typeof(uint) && numericType != typeof(short) && numericType != typeof(ushort) && numericType != typeof(sbyte) && numericType != typeof(byte) && numericType != typeof(float) && numericType != typeof(double))
		{
			throw new ArgumentException("Unsupported numeric type");
		}
		if (rows < 1 || columns < 1)
		{
			throw new ArgumentException("Must have at least 1 row and column in the matrix");
		}
		m_numericType = numericType;
		m_rows = rows;
		m_columns = columns;
		while (base.Controls.Count > 0)
		{
			base.Controls[0].Dispose();
		}
		SuspendLayout();
		base.TabStop = false;
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				NumericTextBox numericTextBox = new NumericTextBox(m_numericType);
				numericTextBox.TabStop = false;
				numericTextBox.BorderStyle = BorderStyle.FixedSingle;
				numericTextBox.Name = "M" + i + j;
				numericTextBox.ScaleFactor = m_scaleFactor;
				numericTextBox.ValueEdited += textBox_ValueEdited;
				base.Controls.Add(numericTextBox);
			}
		}
		ResumeLayout();
	}

	protected override void OnResize(EventArgs e)
	{
		UpdateHeight();
		SuspendLayout();
		int num = base.ClientSize.Width / m_columns;
		int num2 = base.Controls[0].Height;
		int num3 = 0;
		for (int i = 0; i < m_rows; i++)
		{
			int num4 = 0;
			for (int j = 0; j < m_columns; j++)
			{
				int num5 = ((j == m_columns - 1) ? (base.ClientSize.Width - num4) : num);
				NumericTextBox numericTextBox = base.Controls[i * m_columns + j] as NumericTextBox;
				numericTextBox.Bounds = new Rectangle(num4, num3, num5, numericTextBox.Height);
				num4 += num - 1;
			}
			num3 += num2 - 1;
		}
		ResumeLayout(performLayout: true);
		base.OnResize(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		UpdateHeight();
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

	protected virtual void OnValueChanged(EventArgs e)
	{
		this.ValueChanged?.Invoke(this, e);
	}

	protected virtual void OnValueEdited(EventArgs e)
	{
		this.ValueEdited?.Invoke(this, e);
	}

	private void UpdateHeight()
	{
		if (base.Controls.Count > 0)
		{
			base.Height = m_rows * (base.Controls[0].Height - 1) + 1;
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

	private bool AreEqual(Array array1, Array array2)
	{
		if (m_numericType == typeof(float))
		{
			return MathUtil.AreApproxEqual((float[])array1, (float[])array2, 1E-06);
		}
		if (m_numericType == typeof(double))
		{
			return MathUtil.AreApproxEqual((double[])array1, (double[])array2, 1E-06);
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (!array1.GetValue(i).Equals(array2.GetValue(i)))
			{
				return false;
			}
		}
		return true;
	}
}
