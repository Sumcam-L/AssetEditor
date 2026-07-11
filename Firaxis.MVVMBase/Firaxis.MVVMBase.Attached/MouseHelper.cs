using System;
using System.Windows;
using System.Windows.Input;

namespace Firaxis.MVVMBase.Attached;

public class MouseHelper
{
	private static readonly TimeSpan _maxMultiClickTime = TimeSpan.FromMilliseconds(500.0);

	private static DateTime _lastDownTime;

	private const double _maxClickDistance = 20.0;

	private static Point _lastDownLocation;

	private static bool _inputOngoing;

	private static int _clicks;

	public static readonly DependencyProperty CommandCollectionProperty = DependencyProperty.RegisterAttached("CommandCollection", typeof(MouseCommandCollection), typeof(MouseHelper), new PropertyMetadata(null, CommandCollectionChanged));

	public static MouseCommandCollection GetCommandCollection(FrameworkElement target)
	{
		return (MouseCommandCollection)target.GetValue(CommandCollectionProperty);
	}

	public static void SetCommandCollection(FrameworkElement target, MouseCommandCollection value)
	{
		target.SetValue(CommandCollectionProperty, value);
	}

	private static void CommandCollectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(sender is FrameworkElement frameworkElement))
		{
			return;
		}
		frameworkElement.PreviewMouseUp -= FrameworkElement_MouseEvent;
		frameworkElement.PreviewMouseDown -= FrameworkElement_MouseEvent;
		frameworkElement.MouseUp -= FrameworkElement_MouseEvent;
		frameworkElement.MouseDown -= FrameworkElement_MouseEvent;
		MouseCommandCollection commandCollection = GetCommandCollection(frameworkElement);
		if (commandCollection == null || commandCollection.Count == 0)
		{
			if (e.OldValue is MouseCommandCollection mouseCommandCollection)
			{
				mouseCommandCollection.Owner = null;
			}
		}
		else
		{
			commandCollection.Owner = frameworkElement;
			frameworkElement.PreviewMouseUp += FrameworkElement_PreviewMouseEvent;
			frameworkElement.PreviewMouseDown += FrameworkElement_PreviewMouseEvent;
			frameworkElement.MouseUp += FrameworkElement_MouseEvent;
			frameworkElement.MouseDown += FrameworkElement_MouseEvent;
		}
	}

	private static void FrameworkElement_PreviewMouseEvent(object sender, MouseButtonEventArgs e)
	{
		if (!(sender is FrameworkElement frameworkElement))
		{
			return;
		}
		MouseCommandCollection commandCollection = GetCommandCollection(frameworkElement);
		if (commandCollection == null)
		{
			return;
		}
		Point point = frameworkElement.PointToScreen(e.GetPosition(frameworkElement));
		if (_inputOngoing && (DateTime.Now - _lastDownTime > _maxMultiClickTime || Point.Subtract(point, _lastDownLocation).Length > 20.0))
		{
			_inputOngoing = false;
			_lastDownTime = DateTime.MinValue;
			_lastDownLocation = default(Point);
			_clicks = 0;
		}
		bool flag = false;
		if (e.ButtonState == MouseButtonState.Pressed)
		{
			_lastDownTime = DateTime.Now;
			_lastDownLocation = point;
			_inputOngoing = true;
			_clicks++;
		}
		else if (_inputOngoing)
		{
			flag = true;
		}
		foreach (MouseCommand item in commandCollection)
		{
			if (e.ChangedButton == item.Button && ((e.ButtonState == MouseButtonState.Pressed && item.InputStyle == MouseInputStyle.Down) || (e.ButtonState == MouseButtonState.Released && item.InputStyle == MouseInputStyle.Up) || (flag && item.InputStyle == MouseInputStyle.Click)) && _clicks == item.Clicks && item.OnPreview && item.Command.CanExecute(null))
			{
				item.Command.Execute(null);
			}
		}
	}

	private static void FrameworkElement_MouseEvent(object sender, MouseButtonEventArgs e)
	{
		if (!(sender is FrameworkElement target))
		{
			return;
		}
		MouseCommandCollection commandCollection = GetCommandCollection(target);
		if (commandCollection == null)
		{
			return;
		}
		bool flag = e.ButtonState == MouseButtonState.Released && _inputOngoing;
		foreach (MouseCommand item in commandCollection)
		{
			if (e.ChangedButton == item.Button && ((e.ButtonState == MouseButtonState.Pressed && item.InputStyle == MouseInputStyle.Down) || (e.ButtonState == MouseButtonState.Released && item.InputStyle == MouseInputStyle.Up) || (flag && item.InputStyle == MouseInputStyle.Click)) && _clicks == item.Clicks && !item.OnPreview && item.Command.CanExecute(null))
			{
				item.Command.Execute(null);
			}
		}
	}
}
