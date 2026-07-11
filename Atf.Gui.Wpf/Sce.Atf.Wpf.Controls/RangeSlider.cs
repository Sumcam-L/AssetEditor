using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Controls;

[DefaultEvent("RangeSelectionChanged")]
[TemplatePart(Name = "PART_RangeSliderContainer", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_LeftEdge", Type = typeof(RepeatButton))]
[TemplatePart(Name = "PART_RightEdge", Type = typeof(RepeatButton))]
[TemplatePart(Name = "PART_LeftThumb", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_MiddleThumb", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_RightThumb", Type = typeof(Thumb))]
public class RangeSlider : RangeSliderBase
{
	public static readonly DependencyProperty MinRangeProperty;

	public static readonly RoutedEvent RangeSelectionChangedEvent;

	private const double m_repeatButtonMoveRatio = 0.1;

	private const double m_defaultSplittersThumbWidth = 10.0;

	private Thumb m_centerThumb;

	private bool m_internalUpdate;

	private RepeatButton m_leftButton;

	private Thumb m_leftThumb;

	private RepeatButton m_rightButton;

	private Thumb m_rightThumb;

	private StackPanel m_visualElementsContainer;

	private double m_movableRange;

	private double m_movableWidth;

	public double MinRange
	{
		get
		{
			return (double)GetValue(MinRangeProperty);
		}
		set
		{
			SetValue(MinRangeProperty, value);
		}
	}

	public event RangeSelectionChangedEventHandler RangeSelectionChanged
	{
		add
		{
			AddHandler(RangeSelectionChangedEvent, value);
		}
		remove
		{
			RemoveHandler(RangeSelectionChangedEvent, value);
		}
	}

	public RangeSlider()
	{
		DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualWidthProperty, typeof(RangeSlider)).AddValueChanged(this, delegate
		{
			ReCalculateWidths();
		});
	}

	public void MoveSelection(bool isLeft)
	{
		double num = 0.1 * (base.RangeStop - base.RangeStart) * m_movableWidth / m_movableRange;
		num = (isLeft ? (0.0 - num) : num);
		MoveThumb(m_leftButton, m_rightButton, num);
		ReCalculateRangeSelected(reCalculateStart: true, reCalculateStop: true);
	}

	public void ResetSelection(bool isStart)
	{
		double num = base.Maximum - base.Minimum;
		num = (isStart ? (0.0 - num) : num);
		MoveThumb(m_leftButton, m_rightButton, num);
		ReCalculateRangeSelected(reCalculateStart: true, reCalculateStop: true);
	}

	public void MoveSelection(double span)
	{
		if (span > 0.0)
		{
			if (base.RangeStop + span > base.Maximum)
			{
				span = base.Maximum - base.RangeStop;
			}
		}
		else if (base.RangeStart + span < base.Minimum)
		{
			span = base.Minimum - base.RangeStart;
		}
		if (span != 0.0)
		{
			m_internalUpdate = true;
			base.RangeStart += span;
			base.RangeStop += span;
			ReCalculateWidths();
			m_internalUpdate = false;
			OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
		}
	}

	public void SetSelectedRange(double selectionStart, double selectionStop)
	{
		double val = Math.Max(base.Minimum, selectionStart);
		double val2 = Math.Min(selectionStop, base.Maximum);
		val = Math.Min(val, base.Maximum - MinRange);
		val2 = Math.Max(base.Minimum + MinRange, val2);
		if (val2 >= val + MinRange)
		{
			m_internalUpdate = true;
			base.RangeStart = val;
			base.RangeStop = val2;
			ReCalculateWidths();
			m_internalUpdate = false;
			OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
		}
	}

	public void ZoomToSpan(double span)
	{
		m_internalUpdate = true;
		span = Math.Min(span, base.Maximum - base.Minimum);
		span = Math.Max(span, MinRange);
		if (span != base.RangeStop - base.RangeStart)
		{
			double num = (span - (base.RangeStop - base.RangeStart)) / 2.0;
			double num2 = num;
			if (num > 0.0 && base.RangeStop + num > base.Maximum)
			{
				num2 += num - (base.Maximum - base.RangeStop);
			}
			base.RangeStop = Math.Min(base.RangeStop + num, base.Maximum);
			num = 0.0;
			if (num2 > 0.0 && base.RangeStart - num2 < base.Minimum)
			{
				num = base.Minimum - (base.RangeStart - num2);
			}
			base.RangeStart = Math.Max(base.RangeStart - num2, base.Minimum);
			if (num > 0.0)
			{
				base.RangeStop = Math.Min(base.RangeStop + num, base.Maximum);
			}
			ReCalculateWidths();
			m_internalUpdate = false;
			OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
		}
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		m_visualElementsContainer = EnforceInstance<StackPanel>("PART_RangeSliderContainer");
		m_centerThumb = EnforceInstance<Thumb>("PART_MiddleThumb");
		m_leftButton = EnforceInstance<RepeatButton>("PART_LeftEdge");
		m_rightButton = EnforceInstance<RepeatButton>("PART_RightEdge");
		m_leftThumb = EnforceInstance<Thumb>("PART_LeftThumb");
		m_rightThumb = EnforceInstance<Thumb>("PART_RightThumb");
		InitializeVisualElementsContainer();
		ReCalculateWidths();
	}

	protected override void OnMinimumChanged(double oldValue, double newValue)
	{
		base.OnMinimumChanged(oldValue, newValue);
		if (!m_internalUpdate)
		{
			ReCalculateRanges();
			ReCalculateWidths();
		}
	}

	protected override void OnMaximumChanged(double oldValue, double newValue)
	{
		base.OnMaximumChanged(oldValue, newValue);
		if (!m_internalUpdate)
		{
			ReCalculateRanges();
			ReCalculateWidths();
		}
	}

	protected override void OnRangeStartChanged(double oldValue, double newValue)
	{
		base.OnRangeStartChanged(oldValue, newValue);
		if (!m_internalUpdate)
		{
			ReCalculateWidths();
			OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
		}
	}

	protected override void OnRangeStopChanged(double oldValue, double newValue)
	{
		base.OnRangeStopChanged(oldValue, newValue);
		if (!m_internalUpdate)
		{
			ReCalculateWidths();
			OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
		}
	}

	static RangeSlider()
	{
		MinRangeProperty = DependencyProperty.Register("MinRange", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0.0, delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if ((double)e.NewValue < 0.0)
			{
				throw new ArgumentOutOfRangeException("value", "value for MinRange cannot be less than 0");
			}
			RangeSlider rangeSlider = (RangeSlider)sender;
			if (!rangeSlider.m_internalUpdate)
			{
				rangeSlider.m_internalUpdate = true;
				rangeSlider.RangeStop = Math.Max(rangeSlider.RangeStop, rangeSlider.RangeStart + (double)e.NewValue);
				rangeSlider.Maximum = Math.Max(rangeSlider.Maximum, rangeSlider.RangeStop);
				rangeSlider.m_internalUpdate = false;
				rangeSlider.ReCalculateRanges();
				rangeSlider.ReCalculateWidths();
			}
		}));
		RangeSelectionChangedEvent = EventManager.RegisterRoutedEvent("RangeSelectionChanged", RoutingStrategy.Bubble, typeof(RangeSelectionChangedEventHandler), typeof(RangeSlider));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(typeof(RangeSlider)));
	}

	private void RightThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		MoveThumb(m_centerThumb, m_rightButton, e.HorizontalChange);
		ReCalculateRangeSelected(reCalculateStart: false, reCalculateStop: true);
	}

	private void LeftThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		MoveThumb(m_leftButton, m_centerThumb, e.HorizontalChange);
		ReCalculateRangeSelected(reCalculateStart: true, reCalculateStop: false);
	}

	private void LeftButtonClick(object sender, RoutedEventArgs e)
	{
		MoveSelection(isLeft: true);
	}

	private void RightButtonClick(object sender, RoutedEventArgs e)
	{
		MoveSelection(isLeft: false);
	}

	private void CenterThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		MoveThumb(m_leftButton, m_rightButton, e.HorizontalChange);
		ReCalculateRangeSelected(reCalculateStart: true, reCalculateStop: true);
	}

	private static void MoveThumb(FrameworkElement x, FrameworkElement y, double horizonalChange)
	{
		double num = 0.0;
		if (horizonalChange < 0.0)
		{
			num = GetChangeKeepPositive(x.Width, horizonalChange);
		}
		else if (horizonalChange > 0.0)
		{
			num = 0.0 - GetChangeKeepPositive(y.Width, 0.0 - horizonalChange);
		}
		x.Width += num;
		y.Width -= num;
	}

	private static double GetChangeKeepPositive(double width, double increment)
	{
		return Math.Max(width + increment, 0.0) - width;
	}

	private void ReCalculateRanges()
	{
		m_movableRange = base.Maximum - base.Minimum - MinRange;
	}

	private void ReCalculateWidths()
	{
		if (m_leftButton != null && m_rightButton != null && m_centerThumb != null)
		{
			m_movableWidth = Math.Max(base.ActualWidth - m_rightThumb.ActualWidth - m_leftThumb.ActualWidth - m_centerThumb.MinWidth, 1.0);
			m_leftButton.Width = Math.Max(m_movableWidth * (base.RangeStart - base.Minimum) / m_movableRange, 0.0);
			m_rightButton.Width = Math.Max(m_movableWidth * (base.Maximum - base.RangeStop) / m_movableRange, 0.0);
			m_centerThumb.Width = Math.Max(base.ActualWidth - m_leftButton.Width - m_rightButton.Width - m_rightThumb.ActualWidth - m_leftThumb.ActualWidth, 0.0);
		}
	}

	private void ReCalculateRangeSelected(bool reCalculateStart, bool reCalculateStop)
	{
		m_internalUpdate = true;
		if (reCalculateStart)
		{
			if (m_leftButton.Width == 0.0)
			{
				base.RangeStart = base.Minimum;
			}
			else
			{
				base.RangeStart = Math.Max(base.Minimum, base.Minimum + m_movableRange * m_leftButton.Width / m_movableWidth);
			}
		}
		if (reCalculateStop)
		{
			if (m_rightButton.Width == 0.0)
			{
				base.RangeStop = base.Maximum;
			}
			else
			{
				base.RangeStop = Math.Min(base.Maximum, base.Maximum - m_movableRange * m_rightButton.Width / m_movableWidth);
			}
		}
		m_internalUpdate = false;
		if (reCalculateStart || reCalculateStop)
		{
			OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
		}
	}

	private void OnRangeSelectionChanged(RangeSelectionChangedEventArgs e)
	{
		e.RoutedEvent = RangeSelectionChangedEvent;
		RaiseEvent(e);
	}

	private T EnforceInstance<T>(string partName) where T : FrameworkElement, new()
	{
		return (GetTemplateChild(partName) as T) ?? new T();
	}

	private void InitializeVisualElementsContainer()
	{
		m_visualElementsContainer.Orientation = Orientation.Horizontal;
		m_leftThumb.Width = 10.0;
		m_leftThumb.Tag = "left";
		m_rightThumb.Width = 10.0;
		m_rightThumb.Tag = "right";
		m_centerThumb.DragDelta += CenterThumbDragDelta;
		m_leftThumb.DragDelta += LeftThumbDragDelta;
		m_rightThumb.DragDelta += RightThumbDragDelta;
		m_leftButton.Click += LeftButtonClick;
		m_rightButton.Click += RightButtonClick;
	}
}
