using System.Windows;

namespace Sce.Atf.Wpf.Behaviors;

public class DataPipe : Freezable
{
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(DataPipe), new FrameworkPropertyMetadata(null, OnSourceChanged));

	public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(object), typeof(DataPipe), new FrameworkPropertyMetadata(null));

	public object Source
	{
		get
		{
			return GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	public object Target
	{
		get
		{
			return GetValue(TargetProperty);
		}
		set
		{
			SetValue(TargetProperty, value);
		}
	}

	private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataPipe)d).OnSourceChanged(e);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DataPipe();
	}

	protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
	{
		Target = e.NewValue;
	}
}
