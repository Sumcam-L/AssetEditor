using System.Windows;

namespace Firaxis.ATF;

public class WpfSkinStyleSetter : IWpfSkinStyleSetter
{
	private readonly DependencyProperty TargetProperty;

	private readonly object PropertyValue;

	public WpfSkinStyleSetter(DependencyProperty targetProperty, object propertyValue)
	{
		TargetProperty = targetProperty;
		PropertyValue = propertyValue;
	}

	public DependencyProperty GetTargetProperty()
	{
		return TargetProperty;
	}

	public void ApplyStyle(DependencyObject target)
	{
		target?.SetValue(TargetProperty, PropertyValue);
	}
}
public class WpfSkinStyleSetter<T> : IWpfSkinStyleSetter
{
	private readonly DependencyProperty TargetProperty;

	private readonly T PropertyValue;

	public WpfSkinStyleSetter(DependencyProperty targetProperty, T propertyValue)
	{
		TargetProperty = targetProperty;
		PropertyValue = propertyValue;
	}

	public DependencyProperty GetTargetProperty()
	{
		return TargetProperty;
	}

	public void ApplyStyle(DependencyObject target)
	{
		target?.SetValue(TargetProperty, PropertyValue);
	}
}
