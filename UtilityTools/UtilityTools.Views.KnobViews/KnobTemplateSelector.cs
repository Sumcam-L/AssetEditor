using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UtilityTools.Views.KnobViews;

public class KnobTemplateSelector : DataTemplateSelector
{
	public DataTemplate FunctionKnobContentTemplate { get; set; }

	public DataTemplate ValueContentTemplate { get; set; }

	public DataTemplate BoolValueContentTemplate { get; set; }

	public DataTemplate ColorContentTemplate { get; set; }

	public DataTemplate RangeKnobContentTemplate { get; set; }

	public DataTemplate ContainerKnobContentTemplate { get; set; }

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		Type type = item.GetType();
		if (type.Name.Contains("Function"))
		{
			return FunctionKnobContentTemplate;
		}
		if (type.Name.Contains("Range"))
		{
			return RangeKnobContentTemplate;
		}
		if (type.Name.Contains("Container"))
		{
			return ContainerKnobContentTemplate;
		}
		if (type.Name.Contains("Value"))
		{
			if (type.Name.Contains("Color"))
			{
				return ColorContentTemplate;
			}
			if (type.IsGenericType)
			{
				Type type2 = type.GetGenericArguments().First();
				if (type2 == typeof(bool))
				{
					return BoolValueContentTemplate;
				}
				return ValueContentTemplate;
			}
		}
		return FunctionKnobContentTemplate;
	}
}
