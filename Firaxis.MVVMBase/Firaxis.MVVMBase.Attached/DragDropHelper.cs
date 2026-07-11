using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Firaxis.MVVMBase.Helpers;

namespace Firaxis.MVVMBase.Attached;

public class DragDropHelper
{
	public class DropData
	{
		public string Format { get; }

		public object Data { get; }

		public DropData(string format, object data)
		{
			Format = format;
			Data = data;
		}
	}

	private const double _DragStartDistance = 20.0;

	private static Point _MouseDownLocation;

	public static readonly DependencyProperty DragDataObjectProperty = DependencyProperty.RegisterAttached("DragDataObject", typeof(IDataObject), typeof(DragDropHelper), new PropertyMetadata(null, DragDataObjectChanged));

	public static readonly DependencyProperty DragDataObjectFactoryProperty = DependencyProperty.RegisterAttached("DragDataObjectFactory", typeof(Func<IDataObject>), typeof(DragDropHelper), new PropertyMetadata(null, DragDataObjectFactoryChanged));

	public static readonly DependencyProperty DragObjectProperty = DependencyProperty.RegisterAttached("DragObject", typeof(object), typeof(DragDropHelper), new PropertyMetadata(null, DragObjectChanged));

	public static readonly DependencyProperty DragObjectFactoryProperty = DependencyProperty.RegisterAttached("DragObjectFactory", typeof(Func<object>), typeof(DragDropHelper), new PropertyMetadata(null, DragObjectFactoryChanged));

	public static readonly DependencyProperty DragEffectsProperty = DependencyProperty.RegisterAttached("DragEffects", typeof(DragDropEffects), typeof(DragDropHelper), new PropertyMetadata(DragDropEffects.Copy));

	public static readonly DependencyProperty DragFormatProperty = DependencyProperty.RegisterAttached("DragFormat", typeof(string), typeof(DragDropHelper), new PropertyMetadata("Text"));

	private static readonly DependencyPropertyKey PotentialDragStartedPropertyKey = DependencyProperty.RegisterAttachedReadOnly("PotentialDragStarted", typeof(bool), typeof(DragDropHelper), new PropertyMetadata(false));

	private static readonly DependencyProperty PotentialDragStartedProperty = PotentialDragStartedPropertyKey.DependencyProperty;

	public static readonly DependencyProperty DropDataCommandProperty = DependencyProperty.RegisterAttached("DropDataCommand", typeof(RelayCommand<DropData>), typeof(DragDropHelper), new PropertyMetadata(null, DropDataCommandChanged));

	public static readonly DependencyProperty DropDataObjectCommandProperty = DependencyProperty.RegisterAttached("DropDataObjectCommand", typeof(RelayCommand<IDataObject>), typeof(DragDropHelper), new PropertyMetadata(null, DropDataObjectCommandChanged));

	public static readonly DependencyProperty DropFormatProperty = DependencyProperty.RegisterAttached("DropFormat", typeof(string), typeof(DragDropHelper), new PropertyMetadata(null));

	public static readonly DependencyProperty DropFormatsProperty = DependencyProperty.RegisterAttached("DropFormats", typeof(IEnumerable<string>), typeof(DragDropHelper), new PropertyMetadata(null));

	public static readonly DependencyProperty DropEffectsProperty = DependencyProperty.RegisterAttached("DropEffects", typeof(DragDropEffects), typeof(DragDropHelper), new PropertyMetadata(DragDropEffects.Copy));

	public static IDataObject GetDragDataObject(FrameworkElement target)
	{
		return (IDataObject)target.GetValue(DragDataObjectProperty);
	}

	public static void SetDragDataObject(FrameworkElement target, IDataObject value)
	{
		target.SetValue(DragDataObjectProperty, value);
	}

	private static void DragDataObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			SetupDragEvents(frameworkElement, e);
		}
	}

	public static Func<IDataObject> GetDragDataObjectFactory(FrameworkElement target)
	{
		return (Func<IDataObject>)target.GetValue(DragDataObjectFactoryProperty);
	}

	public static void SetDragDataObjectFactory(FrameworkElement target, Func<IDataObject> value)
	{
		target.SetValue(DragDataObjectFactoryProperty, value);
	}

	private static void DragDataObjectFactoryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			SetupDragEvents(frameworkElement, e);
		}
	}

	public static object GetDragObject(FrameworkElement target)
	{
		return target.GetValue(DragObjectProperty);
	}

	public static void SetDragObject(FrameworkElement target, object value)
	{
		target.SetValue(DragObjectProperty, value);
	}

	private static void DragObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			SetupDragEvents(frameworkElement, e);
		}
	}

	public static Func<object> GetDragObjectFactory(FrameworkElement target)
	{
		return (Func<object>)target.GetValue(DragObjectFactoryProperty);
	}

	public static void SetDragObjectFactory(FrameworkElement target, Func<object> value)
	{
		target.SetValue(DragObjectFactoryProperty, value);
	}

	private static void DragObjectFactoryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			SetupDragEvents(frameworkElement, e);
		}
	}

	public static DragDropEffects GetDragEffects(FrameworkElement target)
	{
		return (DragDropEffects)target.GetValue(DragEffectsProperty);
	}

	public static void SetDragEffects(FrameworkElement target, DragDropEffects value)
	{
		target.SetValue(DragEffectsProperty, value);
	}

	public static string GetDragFormat(FrameworkElement target)
	{
		return (string)target.GetValue(DragFormatProperty);
	}

	public static void SetDragFormat(FrameworkElement target, string value)
	{
		target.SetValue(DragFormatProperty, value);
	}

	private static bool GetPotentialDragStarted(FrameworkElement target)
	{
		return (bool)target.GetValue(PotentialDragStartedProperty);
	}

	private static void SetPotentialDragStarted(FrameworkElement target, bool value)
	{
		target.SetValue(PotentialDragStartedPropertyKey, value);
	}

	private static void SetupDragEvents(FrameworkElement frameworkElement, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue != null)
		{
			frameworkElement.PreviewMouseDown -= FrameworkElement_PreviewMouseDown;
			frameworkElement.PreviewMouseMove -= FrameworkElement_PreviewMouseMove;
			frameworkElement.MouseLeave -= FrameworkElement_MouseLeave;
			frameworkElement.PreviewMouseUp -= FrameworkElement_PreviewMouseUp;
			frameworkElement.PreviewMouseDown += FrameworkElement_PreviewMouseDown;
			frameworkElement.PreviewMouseMove += FrameworkElement_PreviewMouseMove;
			frameworkElement.MouseLeave += FrameworkElement_MouseLeave;
			frameworkElement.PreviewMouseUp += FrameworkElement_PreviewMouseUp;
		}
		else
		{
			frameworkElement.PreviewMouseDown -= FrameworkElement_PreviewMouseDown;
			frameworkElement.PreviewMouseMove -= FrameworkElement_PreviewMouseMove;
			frameworkElement.MouseLeave -= FrameworkElement_MouseLeave;
			frameworkElement.PreviewMouseUp -= FrameworkElement_PreviewMouseUp;
		}
	}

	private static void StartDragDrop(FrameworkElement frameworkElement)
	{
		IDataObject dataObject = GetDragDataObject(frameworkElement);
		if (dataObject == null)
		{
			Func<IDataObject> dataObjectFactory = GetDragDataObjectFactory(frameworkElement);
			if (dataObjectFactory != null)
			{
				ApplicationHelper.InvokeIfNeeded(delegate
				{
					dataObject = dataObjectFactory();
				});
			}
			if (dataObject == null)
			{
				object data = GetDragObject(frameworkElement);
				if (data == null)
				{
					Func<object> dataFactory = GetDragObjectFactory(frameworkElement);
					if (dataFactory != null)
					{
						ApplicationHelper.InvokeIfNeeded(delegate
						{
							data = dataFactory();
						});
					}
				}
				if (data == null)
				{
					return;
				}
				string text = GetDragFormat(frameworkElement);
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "Text";
				}
				dataObject = new DataObject(text, data);
			}
		}
		DragDrop.DoDragDrop(frameworkElement, dataObject, GetDragEffects(frameworkElement));
		SetPotentialDragStarted(frameworkElement, value: false);
	}

	private static void FrameworkElement_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (!(sender is FrameworkElement frameworkElement))
		{
			return;
		}
		Visual visual = e.OriginalSource as Visual;
		while (visual != null && visual != frameworkElement)
		{
			if (visual is ScrollBar)
			{
				return;
			}
			visual = VisualTreeHelper.GetParent(visual) as Visual;
		}
		SetPotentialDragStarted(frameworkElement, value: true);
		_MouseDownLocation = frameworkElement.PointToScreen(e.GetPosition(frameworkElement));
	}

	private static void FrameworkElement_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement && GetPotentialDragStarted(frameworkElement))
		{
			Point point = frameworkElement.PointToScreen(e.GetPosition(frameworkElement));
			if (!(Point.Subtract(_MouseDownLocation, point).Length < 20.0))
			{
				StartDragDrop(frameworkElement);
			}
		}
	}

	private static void FrameworkElement_MouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement && GetPotentialDragStarted(frameworkElement))
		{
			StartDragDrop(frameworkElement);
		}
	}

	private static void FrameworkElement_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (sender is FrameworkElement target)
		{
			SetPotentialDragStarted(target, value: false);
		}
	}

	public static RelayCommand<DropData> GetDropDataCommand(FrameworkElement target)
	{
		return (RelayCommand<DropData>)target.GetValue(DropDataCommandProperty);
	}

	public static void SetDropDataCommand(FrameworkElement target, RelayCommand<DropData> value)
	{
		target.SetValue(DropDataCommandProperty, value);
	}

	private static void DropDataCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			SetupDropEvents(frameworkElement, e);
		}
	}

	public static RelayCommand<IDataObject> GetDropDataObjectCommand(FrameworkElement target)
	{
		return (RelayCommand<IDataObject>)target.GetValue(DropDataObjectCommandProperty);
	}

	public static void SetDropDataObjectCommand(FrameworkElement target, RelayCommand<IDataObject> value)
	{
		target.SetValue(DropDataObjectCommandProperty, value);
	}

	private static void DropDataObjectCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			SetupDropEvents(frameworkElement, e);
		}
	}

	public static string GetDropFormat(FrameworkElement target)
	{
		return (string)target.GetValue(DropFormatProperty);
	}

	public static void SetDropFormat(FrameworkElement target, string value)
	{
		target.SetValue(DropFormatProperty, value);
	}

	public static IEnumerable<string> GetDropFormats(FrameworkElement target)
	{
		return (IEnumerable<string>)target.GetValue(DropFormatsProperty);
	}

	public static void SetDropFormats(FrameworkElement target, IEnumerable<string> value)
	{
		target.SetValue(DropFormatsProperty, value);
	}

	public static DragDropEffects GetDropEffects(FrameworkElement target)
	{
		return (DragDropEffects)target.GetValue(DropEffectsProperty);
	}

	public static void SetDropEffects(FrameworkElement target, DragDropEffects value)
	{
		target.SetValue(DropEffectsProperty, value);
	}

	private static void SetupDropEvents(FrameworkElement frameworkElement, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue != null)
		{
			frameworkElement.Drop -= FrameworkElement_Drop;
			frameworkElement.Drop += FrameworkElement_Drop;
			Binding binding = BindingOperations.GetBinding(frameworkElement, UIElement.AllowDropProperty);
			if (binding == null)
			{
				frameworkElement.AllowDrop = true;
			}
		}
		else
		{
			frameworkElement.Drop -= FrameworkElement_Drop;
			Binding binding2 = BindingOperations.GetBinding(frameworkElement, UIElement.AllowDropProperty);
			if (binding2 == null)
			{
				frameworkElement.AllowDrop = false;
			}
		}
	}

	private static void FrameworkElement_Drop(object sender, DragEventArgs e)
	{
		if (!(sender is FrameworkElement target))
		{
			return;
		}
		IDataObject data = e.Data;
		RelayCommand<IDataObject> dropDataObjectCommand = GetDropDataObjectCommand(target);
		if (dropDataObjectCommand != null && dropDataObjectCommand.CanExecute(data))
		{
			dropDataObjectCommand.Execute(data);
		}
		RelayCommand<DropData> dropDataCommand = GetDropDataCommand(target);
		string dropFormat = GetDropFormat(target);
		IEnumerable<string> dropFormats = GetDropFormats(target);
		if (dropDataCommand == null || (dropFormat == null && dropFormats == null))
		{
			return;
		}
		if (dropFormat != null && data.GetDataPresent(dropFormat))
		{
			DropData parameter = new DropData(dropFormat, data.GetData(dropFormat));
			if (dropDataCommand.CanExecute(parameter))
			{
				dropDataCommand.Execute(parameter);
			}
		}
		foreach (string item in dropFormats)
		{
			if (data.GetDataPresent(item))
			{
				DropData parameter2 = new DropData(item, data.GetData(item));
				if (dropDataCommand.CanExecute(parameter2))
				{
					dropDataCommand.Execute(parameter2);
				}
			}
		}
	}
}
