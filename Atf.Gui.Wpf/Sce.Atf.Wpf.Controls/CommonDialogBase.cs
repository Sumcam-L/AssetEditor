using System;
using System.Windows;
using System.Windows.Threading;

namespace Sce.Atf.Wpf.Controls;

public class CommonDialogBase : Window
{
	public static readonly DependencyProperty HeightAdjustmentProperty = DependencyProperty.Register("HeightAdjustment", typeof(double), typeof(CommonDialogBase), new FrameworkPropertyMetadata(36.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

	private const string SizingDecoratorName = "PART_SizingDecorator";

	private SizeToContentDecorator m_sizingDecorator;

	public double HeightAdjustment
	{
		get
		{
			return (double)GetValue(HeightAdjustmentProperty);
		}
		set
		{
			SetValue(HeightAdjustmentProperty, value);
		}
	}

	protected virtual bool IsOverridingWindowsChrome => true;

	public CommonDialogBase()
	{
		base.WindowStartupLocation = WindowStartupLocation.CenterOwner;
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(base.InvalidateMeasure));
		SizeToContentDecorator sizingDecorator = m_sizingDecorator;
		m_sizingDecorator = GetTemplateChild("PART_SizingDecorator") as SizeToContentDecorator;
		if (sizingDecorator != m_sizingDecorator)
		{
			if (sizingDecorator != null)
			{
				sizingDecorator.DesiredSizeChanged -= OnContentDesiredSizeChanged;
			}
			if (m_sizingDecorator != null)
			{
				m_sizingDecorator.DesiredSizeChanged += OnContentDesiredSizeChanged;
			}
		}
	}

	protected virtual void OnContentDesiredSizeChanged()
	{
		InvalidateMeasure();
	}

	private void OnContentDesiredSizeChanged(object sender, RoutedEventArgs args)
	{
		OnContentDesiredSizeChanged();
	}
}
