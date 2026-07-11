using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetPreviewing;

public class CustomNumericPropertyEditor<T> : IActivatablePropertyEditor where T : IComparable, new()
{
	protected class BoundFloatControl : FloatInputControl, ICacheablePropertyControl
	{
		private PropertyDescriptor m_prop;

		private IValueKnob<T> m_knob;

		private float m_value;

		private bool m_refreshing;

		public virtual bool Cacheable => true;

		public BoundFloatControl(PropertyDescriptor prop, IValueKnob<T> knob, float min, float max)
			: base(min, min, max)
		{
			m_prop = prop;
			m_knob = knob;
			Type type = knob.Value.GetType();
			object obj = knob.Value;
			if (knob.Value.GetType() == typeof(float))
			{
				base.Value = (float)obj;
				m_value = base.Value;
			}
			base.DrawBorder = false;
			DoubleBuffered = true;
			RefreshValue();
		}

		protected override void OnValueChanged(EventArgs e)
		{
			if (!m_refreshing)
			{
				m_prop.SetValue(m_knob, base.Value);
			}
			base.OnValueChanged(e);
		}

		public override void Refresh()
		{
			RefreshValue();
			base.Refresh();
		}

		private void RefreshValue()
		{
			try
			{
				m_refreshing = true;
				object obj = m_value;
				if (obj == null)
				{
					base.Enabled = false;
					return;
				}
				base.Value = (float)obj;
				base.Enabled = true;
			}
			finally
			{
				m_refreshing = false;
			}
		}
	}

	protected class BoundIntControl : IntInputControl, ICacheablePropertyControl
	{
		private PropertyDescriptor m_prop;

		private IValueKnob<T> m_knob;

		private int m_value;

		private bool m_refreshing;

		public virtual bool Cacheable => true;

		public BoundIntControl(PropertyDescriptor prop, IValueKnob<T> knob, int min, int max)
			: base(min, min, max)
		{
			m_prop = prop;
			m_knob = knob;
			Type type = knob.Value.GetType();
			object obj = knob.Value;
			if (knob.Value.GetType() == typeof(int))
			{
				base.Value = (int)obj;
				m_value = base.Value;
			}
			base.DrawBorder = false;
			DoubleBuffered = true;
			RefreshValue();
		}

		protected override void OnValueChanged(EventArgs e)
		{
			if (!m_refreshing)
			{
				m_prop.SetValue(m_knob, base.Value);
			}
			base.OnValueChanged(e);
		}

		public override void Refresh()
		{
			RefreshValue();
			base.Refresh();
		}

		private void RefreshValue()
		{
			try
			{
				m_refreshing = true;
				object obj = m_value;
				if (obj == null)
				{
					base.Enabled = false;
					return;
				}
				base.Value = (int)obj;
				base.Enabled = true;
			}
			finally
			{
				m_refreshing = false;
			}
		}
	}

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

	private TypeConverter Converter;

	private IValueKnob<T> m_numericKnob;

	private T m_initialValue;

	private TextBox m_textBox;

	private Control m_activeControl;

	private ICacheablePropertyControl m_boundControl;

	public bool IsInline => true;

	public bool Active => m_activeControl != null;

	public Control PropertyControl => m_activeControl;

	public event EventHandler ValueCommitted;

	public CustomNumericPropertyEditor(PropertyDescriptor prop, IValueKnob<T> knob)
	{
		Property = prop;
		Converter = prop.Converter;
		m_numericKnob = knob;
		BugSubmitter.Assert(Converter != null, "Converter required to get input from string to T and vice versus");
	}

	public Control ActivatePropertyControl(object component, PropertyDescriptor prop)
	{
		BugSubmitter.Assert(m_activeControl == null, $"{typeof(Control)} already active!");
		if (m_activeControl != null)
		{
			return m_activeControl;
		}
		if (m_numericKnob.Value.GetType() == typeof(float))
		{
			IRangeKnob<float> rangeKnob = m_numericKnob.As<IRangeKnob<float>>();
			if (rangeKnob != null)
			{
				m_boundControl = new BoundFloatControl(prop, m_numericKnob, rangeKnob.MinValue, rangeKnob.MaxValue);
				m_activeControl = m_boundControl as Control;
				return m_boundControl as Control;
			}
		}
		else if (m_numericKnob.Value.GetType() == typeof(int))
		{
			IRangeKnob<int> rangeKnob2 = m_numericKnob.As<IRangeKnob<int>>();
			if (rangeKnob2 != null)
			{
				m_boundControl = new BoundIntControl(prop, m_numericKnob, rangeKnob2.MinValue, rangeKnob2.MaxValue);
				m_activeControl = m_boundControl as Control;
				return m_boundControl as Control;
			}
		}
		m_textBox = new TextBox();
		m_initialValue = m_numericKnob.Value;
		m_textBox.TextAlign = HorizontalAlignment.Right;
		m_textBox.WordWrap = false;
		m_textBox.Text = m_numericKnob.Value.ToString();
		m_textBox.KeyDown += TextBox_KeyDown;
		m_textBox.LostFocus += TextBox_LostFocus;
		m_activeControl = m_textBox;
		return m_textBox;
	}

	private void CommitValue(T value)
	{
		Property.SetValue(m_numericKnob, value);
		this.ValueCommitted.Raise(this, EventArgs.Empty);
	}

	private void RevertValue()
	{
		Property.SetValue(m_numericKnob, m_initialValue);
		this.ValueCommitted.Raise(this, EventArgs.Empty);
	}

	private void TextBox_LostFocus(object sender, EventArgs e)
	{
		TryCommitTextBox();
	}

	private bool TryCommitTextBox()
	{
		if (!Property.Converter.CanConvertTo(typeof(T)))
		{
			RevertValue();
			return false;
		}
		if (!Property.Converter.CanConvertFrom(typeof(string)))
		{
			RevertValue();
			return false;
		}
		T value = (T)Property.Converter.ConvertTo(m_textBox.Text, typeof(T));
		IRangeKnob<T> rangeKnob = m_numericKnob.As<IRangeKnob<T>>();
		if (rangeKnob != null)
		{
			object obj = rangeKnob.MinValue;
			if (value.CompareTo(obj) < 0)
			{
				value = rangeKnob.MinValue;
			}
			object obj2 = rangeKnob.MaxValue;
			if (value.CompareTo(obj2) > 0)
			{
				value = rangeKnob.MaxValue;
			}
		}
		CommitValue(value);
		return true;
	}

	private void TextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			RevertValue();
		}
		else if (e.KeyCode == Keys.Return && TryCommitTextBox())
		{
		}
	}

	public void DeactivateControl()
	{
		BugSubmitter.Assert(m_activeControl != null, $"{typeof(Control)} not active!");
		if (m_textBox != null)
		{
			m_textBox.KeyDown -= TextBox_KeyDown;
			m_textBox.LostFocus -= TextBox_LostFocus;
			m_textBox.Dispose();
			m_textBox = null;
		}
		m_boundControl = null;
		m_activeControl = null;
	}

	private Rectangle ComputeTextRect(Rectangle valueRc)
	{
		Rectangle rect = default(Rectangle);
		BugSubmitter.Assert(valueRc.Contains(rect), "Value rect must contain the drop rect in its entierty");
		int num = valueRc.Left + (int)((float)valueRc.Width / 2f + 0.5f);
		if (num <= rect.Left)
		{
			return new Rectangle(valueRc.Left, valueRc.Top, valueRc.Width - rect.Width - 1, valueRc.Height);
		}
		return new Rectangle(rect.Right + 1, valueRc.Top, valueRc.Width - rect.Width - 1, valueRc.Height);
	}
}
