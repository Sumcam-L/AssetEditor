using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class SliderValueEditorContext : NotifyPropertyChangedBase, IDisposable
{
	private double m_cachedValue;

	private double m_doubleValue;

	private ICommand m_commitEditCommand;

	private ICommand m_cancelEditCommand;

	private readonly double m_max = double.MaxValue;

	private readonly double m_min = double.MinValue;

	private readonly double m_center = double.NaN;

	private readonly double m_default = double.NaN;

	private readonly double m_hardMin = double.NaN;

	private readonly double m_hardMax = double.NaN;

	private readonly double m_scale = 1.0;

	private readonly bool m_isLogarithmic;

	private readonly double m_smallChange = 0.1;

	private readonly double m_defaultChange = 1.0;

	private readonly double m_largeChange = 10.0;

	private readonly string m_formatString = "0.#";

	private readonly string m_units;

	private bool m_showSlider;

	public PropertyNode Node { get; private set; }

	public ICommand CommitEditCommand => m_commitEditCommand ?? (m_commitEditCommand = new DelegateCommand(CommitEdit));

	public ICommand CancelEditCommand => m_cancelEditCommand ?? (m_cancelEditCommand = new DelegateCommand(CancelEdit));

	public double DoubleValue
	{
		get
		{
			return m_cachedValue;
		}
		set
		{
			m_cachedValue = value;
		}
	}

	public double Max => m_max;

	public double Min => m_min;

	public double Default => m_default;

	public double HardMin => m_hardMin;

	public double HardMax => m_hardMax;

	public double Center => m_center;

	public double Scale => m_scale;

	public bool IsLogarithmic => m_isLogarithmic;

	public double SmallChange => m_smallChange;

	public double DefaultChange => m_defaultChange;

	public double LargeChange => m_largeChange;

	public string FormatString => m_formatString;

	public string Units => m_units;

	public bool ShowSlider
	{
		get
		{
			return m_showSlider;
		}
		set
		{
			if (m_showSlider != value)
			{
				m_showSlider = value;
				RaisePropertyChanged("ShowSlider");
			}
		}
	}

	public SliderValueEditorContext(PropertyNode node)
	{
		Node = node;
		Node.ValueChanged += OnNodeOnValueChanged;
		if (Node.Descriptor.Attributes[typeof(NumberRangesAttribute)] is NumberRangesAttribute numberRangesAttribute)
		{
			m_max = numberRangesAttribute.Maximum;
			m_min = numberRangesAttribute.Minimum;
			m_center = numberRangesAttribute.Center;
			m_hardMin = numberRangesAttribute.HardMinimum;
			m_hardMax = numberRangesAttribute.HardMaximum;
		}
		else if (Node.Descriptor.Attributes[typeof(RangeAttribute)] is RangeAttribute rangeAttribute)
		{
			m_max = Convert.ToDouble(rangeAttribute.Minimum);
			m_min = Convert.ToDouble(rangeAttribute.Maximum);
			m_center = (m_max - m_min) / 2.0;
		}
		if (Node.Descriptor.Attributes[typeof(DefaultValueAttribute)] is DefaultValueAttribute defaultValueAttribute && defaultValueAttribute.Value.GetType().IsValueType)
		{
			m_default = Convert.ToDouble(defaultValueAttribute.Value);
		}
		if (Node.Descriptor.Attributes[typeof(NumberIncrementsAttribute)] is NumberIncrementsAttribute numberIncrementsAttribute)
		{
			m_smallChange = numberIncrementsAttribute.SmallChange;
			m_defaultChange = numberIncrementsAttribute.DefaultChange;
			m_largeChange = numberIncrementsAttribute.LargeChange;
			m_isLogarithmic = numberIncrementsAttribute.IsLogarithimc;
		}
		if (Node.Descriptor.Attributes[typeof(DisplayUnitsAttribute)] is DisplayUnitsAttribute displayUnitsAttribute)
		{
			m_units = displayUnitsAttribute.Units;
		}
		if (Node.Descriptor.Attributes[typeof(NumberFormatAttribute)] is NumberFormatAttribute numberFormatAttribute)
		{
			m_scale = numberFormatAttribute.Scale ?? 1.0;
			m_formatString = numberFormatAttribute.FormatString;
		}
		Update();
	}

	public virtual void CommitEdit()
	{
		if (NumericUtil.AreClose(m_doubleValue, m_cachedValue))
		{
			return;
		}
		m_doubleValue = m_cachedValue;
		Type propertyType = Node.Descriptor.PropertyType;
		if (propertyType == typeof(short))
		{
			Node.Value = Convert.ToInt16(m_doubleValue);
			return;
		}
		if (propertyType == typeof(ushort))
		{
			Node.Value = Convert.ToUInt16(m_doubleValue);
			return;
		}
		if (propertyType == typeof(int))
		{
			Node.Value = Convert.ToInt32(m_doubleValue);
			return;
		}
		if (propertyType == typeof(uint))
		{
			Node.Value = Convert.ToUInt32(m_doubleValue);
			return;
		}
		if (propertyType == typeof(long))
		{
			Node.Value = Convert.ToInt64(m_doubleValue);
			return;
		}
		if (propertyType == typeof(ulong))
		{
			Node.Value = Convert.ToUInt64(m_doubleValue);
			return;
		}
		if (propertyType == typeof(float))
		{
			Node.Value = Convert.ToSingle(m_doubleValue);
			return;
		}
		if (propertyType == typeof(double))
		{
			Node.Value = m_doubleValue;
			return;
		}
		throw new NotImplementedException("Type conversion not yet supported");
	}

	public virtual void CancelEdit()
	{
	}

	public virtual void Dispose(bool disposing)
	{
		if (Node != null)
		{
			Node.ValueChanged -= OnNodeOnValueChanged;
		}
	}

	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Update()
	{
		try
		{
			bool showSlider = true;
			if (Node.Value == null)
			{
				showSlider = false;
			}
			else
			{
				double num = Convert.ToDouble(Node.Value);
				if (!NumericUtil.AreClose(m_doubleValue, num))
				{
					m_doubleValue = num;
					m_cachedValue = num;
					RaisePropertyChanged("DoubleValue");
				}
			}
			ShowSlider = showSlider;
		}
		catch (FormatException)
		{
		}
		catch (InvalidCastException)
		{
		}
		catch (OverflowException)
		{
		}
	}

	private void OnNodeOnValueChanged(object s, EventArgs e)
	{
		Update();
	}
}
