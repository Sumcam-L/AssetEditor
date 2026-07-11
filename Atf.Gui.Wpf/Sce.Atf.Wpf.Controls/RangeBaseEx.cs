using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls;

public abstract class RangeBaseEx : RangeBase
{
	public static readonly DependencyProperty DefaultValueProperty;

	public static readonly DependencyProperty ShowSliderProperty;

	public static readonly DependencyProperty IsValueEditingProperty;

	public static readonly DependencyProperty IsLogarithmicProperty;

	public static readonly DependencyProperty OrientationProperty;

	public static readonly DependencyProperty SliderBrushProperty;

	public static readonly DependencyProperty DefaultChangeProperty;

	public static readonly DependencyProperty CenterProperty;

	public static readonly DependencyProperty HardMaximumProperty;

	public static readonly DependencyProperty HardMinimumProperty;

	public static readonly DependencyProperty CommitEditCommandProperty;

	private static readonly DependencyProperty CancelEditCommandProperty;

	public double DefaultValue
	{
		get
		{
			return (double)GetValue(DefaultValueProperty);
		}
		set
		{
			SetValue(DefaultValueProperty, value);
		}
	}

	public bool? ShowSlider
	{
		get
		{
			return (bool?)GetValue(ShowSliderProperty);
		}
		set
		{
			SetValue(ShowSliderProperty, value);
		}
	}

	public bool IsValueEditing
	{
		get
		{
			return (bool)GetValue(IsValueEditingProperty);
		}
		set
		{
			SetValue(IsValueEditingProperty, value);
		}
	}

	public bool IsLogarithmic
	{
		get
		{
			return (bool)GetValue(IsLogarithmicProperty);
		}
		set
		{
			SetValue(IsLogarithmicProperty, value);
		}
	}

	public Orientation Orientation
	{
		get
		{
			return (Orientation)GetValue(OrientationProperty);
		}
		set
		{
			SetValue(OrientationProperty, value);
		}
	}

	public Brush SliderBrush
	{
		get
		{
			return (Brush)GetValue(SliderBrushProperty);
		}
		set
		{
			SetValue(SliderBrushProperty, value);
		}
	}

	public double DefaultChange
	{
		get
		{
			return (double)GetValue(DefaultChangeProperty);
		}
		set
		{
			SetValue(DefaultChangeProperty, value);
		}
	}

	public double Center
	{
		get
		{
			return (double)GetValue(CenterProperty);
		}
		set
		{
			SetValue(CenterProperty, value);
		}
	}

	public double HardMaximum
	{
		get
		{
			return (double)GetValue(HardMaximumProperty);
		}
		set
		{
			SetValue(HardMaximumProperty, value);
		}
	}

	public double HardMinimum
	{
		get
		{
			return (double)GetValue(HardMinimumProperty);
		}
		set
		{
			SetValue(HardMinimumProperty, value);
		}
	}

	public ICommand CommitEditCommand
	{
		get
		{
			return (ICommand)GetValue(CommitEditCommandProperty);
		}
		set
		{
			SetValue(CommitEditCommandProperty, value);
		}
	}

	public ICommand CancelEditCommand
	{
		get
		{
			return (ICommand)GetValue(CancelEditCommandProperty);
		}
		set
		{
			SetValue(CancelEditCommandProperty, value);
		}
	}

	protected bool HasCancelledChanges { get; set; }

	protected double SkewFactor { get; private set; }

	protected bool HasRange => base.Minimum > double.MinValue && base.Maximum < double.MaxValue && base.Minimum < base.Maximum;

	static RangeBaseEx()
	{
		DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(double), typeof(RangeBaseEx), new FrameworkPropertyMetadata(0.0, DefaultValueChanged));
		ShowSliderProperty = DependencyProperty.Register("ShowSlider", typeof(bool?), typeof(RangeBaseEx), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceShowSlider));
		IsValueEditingProperty = DependencyProperty.Register("IsValueEditing", typeof(bool), typeof(RangeBaseEx), new UIPropertyMetadata(false, IsValueEditingChanged));
		IsLogarithmicProperty = DependencyProperty.Register("IsLogarithmic", typeof(bool), typeof(RangeBaseEx), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(RangeBaseEx), new UIPropertyMetadata(Orientation.Horizontal, OnOrientationChanged));
		SliderBrushProperty = DependencyProperty.Register("SliderBrush", typeof(Brush), typeof(RangeBaseEx), new FrameworkPropertyMetadata(Brushes.Pink, FrameworkPropertyMetadataOptions.AffectsRender));
		DefaultChangeProperty = DependencyProperty.Register("DefaultChange", typeof(double), typeof(RangeBaseEx), new FrameworkPropertyMetadata(0.01, DefaultChangeChanged));
		CenterProperty = DependencyProperty.Register("Center", typeof(double), typeof(RangeBaseEx), new FrameworkPropertyMetadata(double.NaN, OnCenterChanged, OnCoerceCenter));
		HardMaximumProperty = DependencyProperty.Register("HardMaximum", typeof(double), typeof(RangeBaseEx), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender, HardMaximumChanged, CoerceHardMaximum));
		HardMinimumProperty = DependencyProperty.Register("HardMinimum", typeof(double), typeof(RangeBaseEx), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender, HardMinimumChanged, CoerceHardMinimum));
		CommitEditCommandProperty = DependencyProperty.Register("CommitEditCommand", typeof(ICommand), typeof(RangeBaseEx), new PropertyMetadata(null));
		CancelEditCommandProperty = DependencyProperty.Register("CancelEditCommand", typeof(ICommand), typeof(RangeBaseEx), new PropertyMetadata(null));
		RangeBase.SmallChangeProperty.OverrideMetadata(typeof(RangeBaseEx), new FrameworkPropertyMetadata(0.001));
		RangeBase.LargeChangeProperty.OverrideMetadata(typeof(RangeBaseEx), new FrameworkPropertyMetadata(0.1));
	}

	protected RangeBaseEx()
	{
		SkewFactor = 1.0;
	}

	protected virtual double OnCoerceCenter(double value)
	{
		return (value < base.Minimum) ? base.Minimum : ((value > base.Maximum) ? base.Maximum : value);
	}

	protected double EnforceHardLimits(double value)
	{
		return Math.Max(HardMinimum, Math.Min(HardMaximum, value));
	}

	protected double SnapTo(double value, double change)
	{
		return Math.Round(value / change) * change;
	}

	protected double ProportionOfLengthToValue(double proportion)
	{
		if (Math.Abs(SkewFactor - 1.0) > 1E-06 && proportion > 0.0)
		{
			proportion = Math.Exp(Math.Log(proportion) / SkewFactor);
		}
		return base.Minimum + (base.Maximum - base.Minimum) * proportion;
	}

	protected double ValueToProportionOfLength(double value)
	{
		double num = (value - base.Minimum) / (base.Maximum - base.Minimum);
		return (Math.Abs(SkewFactor - 1.0) < 1E-06) ? num : Math.Pow(num, SkewFactor);
	}

	protected double Log(double inputMinimum, double inputMaximum, double outputMinimum, double outputMaximum, double value)
	{
		double num = (Math.Exp(2.0) - 1.0) / (inputMaximum - inputMinimum);
		double num2 = outputMaximum - outputMinimum;
		return outputMinimum + num2 * (Math.Log(1.0 + num * (value - inputMinimum)) / 2.0);
	}

	protected double Exp(double inputMinimum, double inputMaximum, double outputMinimum, double outputMaximum, double value)
	{
		double num = 2.0 / (inputMaximum - inputMinimum);
		double num2 = outputMaximum - outputMinimum;
		return outputMinimum + num2 * ((Math.Exp(num * (value - inputMinimum)) - 1.0) / (Math.Exp(2.0) - 1.0));
	}

	protected virtual double? ParseDoubleFromValueAsString(string stringValue)
	{
		double? num = null;
		try
		{
			double num2 = Convert.ToDouble(stringValue);
			if (!double.IsNaN(num2))
			{
				num = num2;
			}
		}
		catch (FormatException)
		{
			return num;
		}
		catch (Exception)
		{
			return num;
		}
		return (!num.HasValue) ? num : new double?(EnforceHardLimits(num.Value));
	}

	protected virtual void OnCommitChanges()
	{
		ICommand commitEditCommand = CommitEditCommand;
		if (commitEditCommand != null && commitEditCommand.CanExecute(this))
		{
			commitEditCommand.Execute(this);
		}
	}

	protected virtual void OnCancelChanges()
	{
		ICommand cancelEditCommand = CancelEditCommand;
		if (cancelEditCommand != null && cancelEditCommand.CanExecute(this))
		{
			cancelEditCommand.Execute(this);
		}
	}

	protected override void OnValueChanged(double oldValue, double newValue)
	{
		CoerceValue(RangeBase.MinimumProperty);
		CoerceValue(RangeBase.MaximumProperty);
		base.OnValueChanged(oldValue, newValue);
	}

	protected override void OnMinimumChanged(double oldValue, double newValue)
	{
		CoerceValue(HardMinimumProperty);
		CoerceValue(ShowSliderProperty);
		CoerceValue(CenterProperty);
		base.OnMinimumChanged(oldValue, newValue);
		RecalculateSkewFactor(Center, base.Minimum, base.Maximum);
	}

	protected override void OnMaximumChanged(double oldValue, double newValue)
	{
		CoerceValue(HardMaximumProperty);
		CoerceValue(ShowSliderProperty);
		CoerceValue(CenterProperty);
		base.OnMaximumChanged(oldValue, newValue);
		RecalculateSkewFactor(Center, base.Minimum, base.Maximum);
	}

	protected virtual void OnCenterChanged(double oldValue, double newValue)
	{
		RecalculateSkewFactor(newValue, base.Minimum, base.Maximum);
	}

	protected virtual void OnOrientationChanged(Orientation oldValue, Orientation newValue)
	{
	}

	private static void DefaultValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RangeBaseEx rangeBaseEx = d as RangeBaseEx;
		if (rangeBaseEx == null)
		{
		}
	}

	private static object CoerceShowSlider(DependencyObject target, object value)
	{
		if (target is RangeBaseEx rangeBaseEx && !(value as bool?).HasValue)
		{
			return rangeBaseEx.HasRange;
		}
		return value;
	}

	private static void IsValueEditingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (!(o is RangeBaseEx rangeBaseEx))
		{
			return;
		}
		if (!rangeBaseEx.IsValueEditing)
		{
			if (rangeBaseEx.HasCancelledChanges)
			{
				rangeBaseEx.OnCancelChanges();
			}
			else
			{
				rangeBaseEx.OnCommitChanges();
			}
		}
		rangeBaseEx.HasCancelledChanges = false;
	}

	private static void OnOrientationChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (o is RangeBaseEx rangeBaseEx)
		{
			rangeBaseEx.OnOrientationChanged((Orientation)e.OldValue, (Orientation)e.NewValue);
		}
	}

	private static void DefaultChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is RangeBaseEx rangeBaseEx)
		{
			rangeBaseEx.CoerceValue(RangeBase.SmallChangeProperty);
			rangeBaseEx.CoerceValue(RangeBase.LargeChangeProperty);
		}
	}

	private static void OnCenterChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (o is RangeBaseEx rangeBaseEx)
		{
			rangeBaseEx.OnCenterChanged((double)e.OldValue, (double)e.NewValue);
		}
	}

	private static object OnCoerceCenter(DependencyObject o, object value)
	{
		if (o is RangeBaseEx rangeBaseEx)
		{
			return rangeBaseEx.OnCoerceCenter((double)value);
		}
		return value;
	}

	private static object CoerceHardMaximum(DependencyObject target, object value)
	{
		if (target is RangeBaseEx rangeBaseEx)
		{
			double d = (double)value;
			if (double.IsNaN(d))
			{
				return rangeBaseEx.Maximum;
			}
		}
		return value;
	}

	private static void HardMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is RangeBaseEx rangeBaseEx)
		{
			rangeBaseEx.CoerceValue(ShowSliderProperty);
		}
	}

	private static object CoerceHardMinimum(DependencyObject target, object value)
	{
		if (target is RangeBaseEx rangeBaseEx)
		{
			double d = (double)value;
			if (double.IsNaN(d))
			{
				return rangeBaseEx.Minimum;
			}
		}
		return value;
	}

	private static void HardMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is RangeBaseEx rangeBaseEx)
		{
			rangeBaseEx.CoerceValue(ShowSliderProperty);
		}
	}

	private void RecalculateSkewFactor(double centerValue, double minimum, double maximum)
	{
		if (!double.IsNaN(centerValue) && maximum > minimum)
		{
			double num = (centerValue - minimum) / (maximum - minimum);
			if (num > 0.0)
			{
				SkewFactor = Math.Log(0.5) / Math.Log((centerValue - minimum) / (maximum - minimum));
			}
			else
			{
				SkewFactor = 1.0;
			}
		}
	}
}
