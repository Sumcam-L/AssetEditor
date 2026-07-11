using System.Windows;
using System.Windows.Controls;
using Firaxis.CivTech.AssetObjects;
using UtilityTools.ViewModels;

namespace UtilityTools.Views;

internal class ParameterSetItemTemplateSelector : DataTemplateSelector
{
	public DataTemplate IntParameterContentTemplate { get; set; }

	public DataTemplate FloatParameterContentTemplate { get; set; }

	public DataTemplate BoolParameterContentTemplate { get; set; }

	public DataTemplate RGBParameterContentTemplate { get; set; }

	public DataTemplate EnumParameterContentTemplate { get; set; }

	public DataTemplate StringParameterContentTemplate { get; set; }

	public DataTemplate ObjectParameterContentTemplate { get; set; }

	public DataTemplate MaterialParameterContentTemplate { get; set; }

	public DataTemplate CollectionParameterContentTemplate { get; set; }

	public DataTemplate OtherParameterContentTemplate { get; set; }

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (!(item is IParameterViewModel parameterViewModel))
		{
			return OtherParameterContentTemplate;
		}
		switch (parameterViewModel.Parameter.ParameterType)
		{
		case ParameterType.PT_FLOAT:
			return FloatParameterContentTemplate;
		case ParameterType.PT_INT:
			return IntParameterContentTemplate;
		case ParameterType.PT_BOOLEAN:
			return BoolParameterContentTemplate;
		case ParameterType.PT_RGB:
			return RGBParameterContentTemplate;
		case ParameterType.PT_ENUM:
			return EnumParameterContentTemplate;
		case ParameterType.PT_STRING:
			return StringParameterContentTemplate;
		case ParameterType.PT_OBJECT:
		{
			IObjectParameter objectParameter = parameterViewModel.Parameter as IObjectParameter;
			if (objectParameter.ObjectType == InstanceType.IT_MATERIAL)
			{
				return MaterialParameterContentTemplate;
			}
			return ObjectParameterContentTemplate;
		}
		case ParameterType.PT_COLLECTION:
			return CollectionParameterContentTemplate;
		default:
			return OtherParameterContentTemplate;
		}
	}
}
