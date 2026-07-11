using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Firaxis.MVVMBase.Helpers;

namespace Firaxis.MVVMBase.Controls;

public class AutoAdjustingStackPanelSplitter : Thumb
{
	private AutoAdjustingStackPanel _parent;

	private DragAdorner _dragAdorner;

	private double _min;

	private double _max;

	public static readonly DependencyProperty SeparationProperty;

	public static readonly DependencyProperty DragColorProperty;

	public double Separation
	{
		get
		{
			return (double)GetValue(SeparationProperty);
		}
		set
		{
			SetValue(SeparationProperty, value);
		}
	}

	public Color DragColor
	{
		get
		{
			return (Color)GetValue(DragColorProperty);
		}
		set
		{
			SetValue(DragColorProperty, value);
		}
	}

	static AutoAdjustingStackPanelSplitter()
	{
		SeparationProperty = DependencyProperty.Register("Separation", typeof(double), typeof(AutoAdjustingStackPanelSplitter), new FrameworkPropertyMetadata(5.0));
		DragColorProperty = DependencyProperty.Register("DragColor", typeof(Color), typeof(AutoAdjustingStackPanelSplitter), new FrameworkPropertyMetadata(Color.FromArgb(180, 0, 0, 0)));
		ApplicationHelper.ImportResourceDictionary(typeof(AutoAdjustingStackPanelSplitter), "Shared.xaml");
		EventManager.RegisterClassHandler(typeof(AutoAdjustingStackPanelSplitter), Thumb.DragStartedEvent, new DragStartedEventHandler(OnDragStarted));
		EventManager.RegisterClassHandler(typeof(AutoAdjustingStackPanelSplitter), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
		EventManager.RegisterClassHandler(typeof(AutoAdjustingStackPanelSplitter), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
		FrameworkElement.CursorProperty.OverrideMetadata(typeof(AutoAdjustingStackPanelSplitter), new FrameworkPropertyMetadata(null, CoerceCursor));
	}

	private void EnsureParentSet()
	{
		_parent = base.Parent as AutoAdjustingStackPanel;
	}

	private void SetupDragAdorner()
	{
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_parent);
		if (adornerLayer != null)
		{
			if (_dragAdorner == null)
			{
				_dragAdorner = new DragAdorner(this);
				adornerLayer.Add(_dragAdorner);
			}
			else
			{
				_dragAdorner.AllowRender = true;
			}
		}
	}

	public void UpdateCursor()
	{
		CoerceValue(FrameworkElement.CursorProperty);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		if (_parent == null)
		{
			_parent = base.Parent as AutoAdjustingStackPanel;
			if (_parent == null)
			{
				return default(Size);
			}
		}
		return new Size(_parent.IsHorizontal ? Separation : constraint.Width, _parent.IsHorizontal ? constraint.Height : Separation);
	}

	private static void OnDragStarted(object sender, DragStartedEventArgs e)
	{
		if (sender is AutoAdjustingStackPanelSplitter autoAdjustingStackPanelSplitter)
		{
			autoAdjustingStackPanelSplitter.EnsureParentSet();
			autoAdjustingStackPanelSplitter._parent.GetMinMaxDrag(autoAdjustingStackPanelSplitter, out autoAdjustingStackPanelSplitter._min, out autoAdjustingStackPanelSplitter._max);
			autoAdjustingStackPanelSplitter.SetupDragAdorner();
		}
	}

	private static void OnDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (sender is AutoAdjustingStackPanelSplitter autoAdjustingStackPanelSplitter)
		{
			if (autoAdjustingStackPanelSplitter._parent.IsHorizontal)
			{
				autoAdjustingStackPanelSplitter._dragAdorner.X = Math.Min(Math.Max(e.HorizontalChange, autoAdjustingStackPanelSplitter._min), autoAdjustingStackPanelSplitter._max);
			}
			else
			{
				autoAdjustingStackPanelSplitter._dragAdorner.Y = Math.Min(Math.Max(e.VerticalChange, autoAdjustingStackPanelSplitter._min), autoAdjustingStackPanelSplitter._max);
			}
		}
	}

	private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
	{
		if (sender is AutoAdjustingStackPanelSplitter autoAdjustingStackPanelSplitter)
		{
			autoAdjustingStackPanelSplitter._dragAdorner.AllowRender = false;
			if (!e.Canceled)
			{
				autoAdjustingStackPanelSplitter._parent.ResizeSurroundingSplitter(autoAdjustingStackPanelSplitter, Math.Min(Math.Max(e.HorizontalChange, autoAdjustingStackPanelSplitter._min), autoAdjustingStackPanelSplitter._max), Math.Min(Math.Max(e.VerticalChange, autoAdjustingStackPanelSplitter._min), autoAdjustingStackPanelSplitter._max));
			}
		}
	}

	private static object CoerceCursor(DependencyObject o, object value)
	{
		if (!(o is AutoAdjustingStackPanelSplitter autoAdjustingStackPanelSplitter))
		{
			return value;
		}
		autoAdjustingStackPanelSplitter.EnsureParentSet();
		return autoAdjustingStackPanelSplitter._parent.IsHorizontal ? Cursors.SizeWE : Cursors.SizeNS;
	}
}
