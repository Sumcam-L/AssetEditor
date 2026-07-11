using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Sce.Atf.Wpf.ValueConverters;

namespace Sce.Atf.Wpf.Markup;

public class ResourceKeyBinding : MarkupExtension
{
	public Binding Binding { get; set; }

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		object result = null;
		if (Binding != null)
		{
			if (Binding.Converter == null)
			{
				IProvideValueTarget provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
				if (provideValueTarget.TargetObject is DependencyObject)
				{
					FrameworkElement element = provideValueTarget.TargetObject as FrameworkElement;
					Binding.Converter = new ResourceLookupConverter(element);
					result = Binding.ProvideValue(serviceProvider);
				}
				else
				{
					result = this;
				}
			}
			else
			{
				result = Binding.ProvideValue(serviceProvider);
			}
		}
		return result;
	}
}
