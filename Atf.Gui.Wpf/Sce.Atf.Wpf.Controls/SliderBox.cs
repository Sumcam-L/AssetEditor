using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Controls;

public partial class SliderBox : UserControl, IComponentConnector
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(SliderBox), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register("StringFormat", typeof(string), typeof(SliderBox), new UIPropertyMetadata("{0:0.00}"));

	public static readonly DependencyProperty DeferDragUpdateProperty = DependencyProperty.Register("DeferDragUpdate", typeof(bool), typeof(SliderBox), new UIPropertyMetadata(false));

	public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(SliderBox), new UIPropertyMetadata(100.0));

	public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(SliderBox), new UIPropertyMetadata(0.0));

	public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(SliderBox), new UIPropertyMetadata(1.0));

	public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register("LargeChange", typeof(double), typeof(SliderBox), new UIPropertyMetadata(10.0));

	private bool m_dragging;

	public double Value
	{
		get
		{
			return (double)GetValue(ValueProperty);
		}
		set
		{
			SetValue(ValueProperty, value);
		}
	}

	public string StringFormat
	{
		get
		{
			return (string)GetValue(StringFormatProperty);
		}
		set
		{
			SetValue(StringFormatProperty, value);
		}
	}

	public bool DeferDragUpdate
	{
		get
		{
			return (bool)GetValue(DeferDragUpdateProperty);
		}
		set
		{
			SetValue(DeferDragUpdateProperty, value);
		}
	}

	public double Maximum
	{
		get
		{
			return (double)GetValue(MaximumProperty);
		}
		set
		{
			SetValue(MaximumProperty, value);
		}
	}

	public double Minimum
	{
		get
		{
			return (double)GetValue(MinimumProperty);
		}
		set
		{
			SetValue(MinimumProperty, value);
		}
	}

	public double SmallChange
	{
		get
		{
			return (double)GetValue(SmallChangeProperty);
		}
		set
		{
			SetValue(SmallChangeProperty, value);
		}
	}

	public double LargeChange
	{
		get
		{
			return (double)GetValue(LargeChangeProperty);
		}
		set
		{
			SetValue(LargeChangeProperty, value);
		}
	}

	public SliderBox()
	{
		InitializeComponent();
		PositionSlider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(Slider_DragStarted));
		PositionSlider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(Slider_DragCompleted));
		PositionSlider.ValueChanged += PositionSlider_ValueChanged;
	}

	private void Slider_DragStarted(object sender, DragStartedEventArgs e)
	{
		m_dragging = true;
	}

	private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
	{
		m_dragging = false;
		if (DeferDragUpdate)
		{
			UpdateValueTarget(this);
		}
	}

	private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (DeferDragUpdate)
		{
			if (!m_dragging)
			{
				UpdateValueTarget(this);
			}
		}
		else
		{
			UpdateValueTarget(this);
		}
	}

	private static void UpdateValueTarget(SliderBox sliderBox)
	{
		sliderBox.PositionSlider.GetBindingExpression(RangeBase.ValueProperty)?.UpdateSource();
	}

}
