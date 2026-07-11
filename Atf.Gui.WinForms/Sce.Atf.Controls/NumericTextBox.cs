using System;
using System.Globalization;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class NumericTextBox : TextBox
{
	private bool m_selectAll;

	private readonly Type m_numericType;

	private double m_scaleFactor = 1.0;

	private long m_int64Value;

	private ulong m_uint64Value;

	private int m_int32Value;

	private uint m_uint32Value;

	private short m_int16Value;

	private ushort m_uint16Value;

	private sbyte m_sByteValue;

	private byte m_byteValue;

	private float m_singleValue;

	private double m_doubleValue;

	private object m_lastEdit;

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
			Value = Value;
		}
	}

	public object Value
	{
		get
		{
			if (m_numericType == typeof(long))
			{
				return m_int64Value;
			}
			if (m_numericType == typeof(ulong))
			{
				return m_uint64Value;
			}
			if (m_numericType == typeof(int))
			{
				return m_int32Value;
			}
			if (m_numericType == typeof(uint))
			{
				return m_uint32Value;
			}
			if (m_numericType == typeof(short))
			{
				return m_int16Value;
			}
			if (m_numericType == typeof(ushort))
			{
				return m_uint16Value;
			}
			if (m_numericType == typeof(sbyte))
			{
				return m_sByteValue;
			}
			if (m_numericType == typeof(byte))
			{
				return m_byteValue;
			}
			if (m_numericType == typeof(float))
			{
				return m_singleValue;
			}
			if (m_numericType == typeof(double))
			{
				return m_doubleValue;
			}
			return null;
		}
		set
		{
			if (m_numericType != value.GetType())
			{
				ThrowInvalidTypeException();
			}
			if (m_numericType == typeof(float))
			{
				value = (float)value * (float)m_scaleFactor;
			}
			else if (m_numericType == typeof(double))
			{
				value = (double)value * m_scaleFactor;
			}
			m_lastEdit = value;
			string text = (Text = ((IFormattable)value).ToString(null, CultureInfo.CurrentCulture));
			TryValidateText(text);
		}
	}

	public object LastEdit => m_lastEdit;

	public event EventHandler ValueEdited;

	public NumericTextBox()
		: this(typeof(float))
	{
	}

	public NumericTextBox(Type numericType)
	{
		if (numericType != typeof(long) && numericType != typeof(ulong) && numericType != typeof(int) && numericType != typeof(uint) && numericType != typeof(short) && numericType != typeof(ushort) && numericType != typeof(sbyte) && numericType != typeof(byte) && numericType != typeof(float) && numericType != typeof(double))
		{
			ThrowInvalidTypeException();
		}
		m_numericType = numericType;
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData)
		{
		case Keys.Return:
			Flush();
			break;
		case Keys.Escape:
			Cancel();
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (keyData == Keys.Up || keyData == Keys.Down || (keyData & Keys.Tab) == Keys.Tab)
		{
			return false;
		}
		return base.IsInputKey(keyData);
	}

	protected virtual void OnValueEdited(EventArgs e)
	{
		this.ValueEdited.Raise(this, e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		Flush();
		base.OnLostFocus(e);
	}

	protected override void OnEnter(EventArgs e)
	{
		base.OnEnter(e);
		m_selectAll = Control.MouseButtons == MouseButtons.Left;
		if (Control.MouseButtons == MouseButtons.None)
		{
			SelectAll();
		}
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		m_selectAll = false;
	}

	protected override void OnMouseUp(MouseEventArgs mevent)
	{
		base.OnMouseUp(mevent);
		if (m_selectAll && SelectionLength == 0)
		{
			m_selectAll = false;
			SelectAll();
			Focus();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && e.Clicks == 2)
		{
			SelectAll();
		}
		else
		{
			base.OnMouseDown(e);
		}
	}

	private void Flush()
	{
		string text = base.Text;
		if (TryValidateText(text))
		{
			object value = Value;
			if (!((m_numericType == typeof(float)) ? (m_lastEdit != null && MathUtil.AreApproxEqual((float)m_lastEdit, (float)value, 1E-06)) : ((!(m_numericType == typeof(double))) ? (m_lastEdit != null && m_lastEdit.Equals(value)) : (m_lastEdit != null && MathUtil.AreApproxEqual((double)m_lastEdit, (double)value, 1E-06)))))
			{
				OnValueEdited(EventArgs.Empty);
				m_lastEdit = value;
			}
		}
		else
		{
			Cancel();
		}
		base.SelectionStart = 0;
		SelectionLength = 0;
	}

	private void Cancel()
	{
		if (m_lastEdit != null)
		{
			Text = m_lastEdit.ToString();
		}
		else
		{
			Text = Value.ToString();
		}
	}

	private bool TryValidateText(string text)
	{
		bool result = false;
		if (m_numericType == typeof(long))
		{
			result = long.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_int64Value);
		}
		else if (m_numericType == typeof(ulong))
		{
			result = ulong.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_uint64Value);
		}
		else if (m_numericType == typeof(int))
		{
			result = int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_int32Value);
		}
		else if (m_numericType == typeof(uint))
		{
			result = uint.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_uint32Value);
		}
		else if (m_numericType == typeof(short))
		{
			result = short.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_int16Value);
		}
		else if (m_numericType == typeof(ushort))
		{
			result = ushort.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_uint16Value);
		}
		else if (m_numericType == typeof(sbyte))
		{
			result = sbyte.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_sByteValue);
		}
		else if (m_numericType == typeof(byte))
		{
			result = byte.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out m_byteValue);
		}
		else if (m_numericType == typeof(float))
		{
			result = float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out m_singleValue);
			m_singleValue /= (float)m_scaleFactor;
		}
		else if (m_numericType == typeof(double))
		{
			result = double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out m_doubleValue);
			m_doubleValue /= m_scaleFactor;
		}
		return result;
	}

	private static void ThrowInvalidTypeException()
	{
		throw new ArgumentException("Unsupported numeric type");
	}
}
