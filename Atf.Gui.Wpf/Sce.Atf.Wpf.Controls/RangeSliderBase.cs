using System.Windows;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Controls;

public class RangeSliderBase : RangeBaseEx
{
	public static readonly DependencyProperty RangeEnabledProperty = DependencyProperty.Register("RangeEnabled", typeof(bool), typeof(RangeSliderBase), new UIPropertyMetadata(false));

	public static readonly DependencyProperty RangeStartProperty = DependencyProperty.Register("RangeStart", typeof(double), typeof(RangeSliderBase), new UIPropertyMetadata(0.0, OnRangeStartChanged, OnCoerceRangeStart));

	public static readonly DependencyProperty RangeStopProperty = DependencyProperty.Register("RangeStop", typeof(double), typeof(RangeSliderBase), new UIPropertyMetadata(0.0, OnRangeStopChanged, OnCoerceRangeStop));

	public bool RangeEnabled
	{
		get
		{
			return (bool)GetValue(RangeEnabledProperty);
		}
		set
		{
			SetValue(RangeEnabledProperty, value);
		}
	}

	public double RangeStart
	{
		get
		{
			return (double)GetValue(RangeStartProperty);
		}
		set
		{
			SetValue(RangeStartProperty, value);
		}
	}

	public double RangeStop
	{
		get
		{
			return (double)GetValue(RangeStopProperty);
		}
		set
		{
			SetValue(RangeStopProperty, value);
		}
	}

	protected virtual void OnRangeStartChanged(double oldValue, double newValue)
	{
		CoerceValue(RangeBase.MinimumProperty);
		CoerceValue(RangeStopProperty);
	}

	protected virtual double OnCoerceRangeStart(double value)
	{
		return (value > RangeStop) ? RangeStop : value;
	}

	protected virtual double OnCoerceRangeStop(double value)
	{
		return (value < RangeStart) ? RangeStart : value;
	}

	protected virtual void OnRangeStopChanged(double oldValue, double newValue)
	{
		CoerceValue(RangeStartProperty);
		CoerceValue(RangeBase.MaximumProperty);
	}

	private static void OnRangeStartChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (o is RangeSliderBase rangeSliderBase)
		{
			rangeSliderBase.OnRangeStartChanged((double)e.OldValue, (double)e.NewValue);
		}
	}

	private static void OnRangeStopChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (o is RangeSliderBase rangeSliderBase)
		{
			rangeSliderBase.OnRangeStopChanged((double)e.OldValue, (double)e.NewValue);
		}
	}

	private static object OnCoerceRangeStart(DependencyObject o, object value)
	{
		if (o is RangeSliderBase rangeSliderBase)
		{
			return rangeSliderBase.OnCoerceRangeStart((double)value);
		}
		return value;
	}

	private static object OnCoerceRangeStop(DependencyObject o, object value)
	{
		if (o is RangeSliderBase rangeSliderBase)
		{
			return rangeSliderBase.OnCoerceRangeStop((double)value);
		}
		return value;
	}
}
