using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class StateTransitionNameTypeConverter : TypeConverter
{
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		IStateTransitionNameProvider service = context.GetService<IStateTransitionNameProvider>();
		if (service == null)
		{
			return null;
		}
		if (!(context.Instance is ScrollableItemTriggerAnimation scrollableItemTriggerAnimation))
		{
			return null;
		}
		if (!service.TimelineStateTransitions.TryGetValue(scrollableItemTriggerAnimation.TimelineName, out var value))
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (StateTransitionInfo item in value)
		{
			list.Add(string.Format(item.IsReadOnly ? "{0}:{1}->{2}[{3:R}]" : "{0}:{1}->{2}({3:R})", item.AnimationGraphIndex, item.Source, item.Destination, item.Duration));
		}
		return new StandardValuesCollection(list);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			return GetStateTransitionInfo((string)value);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is StateTransitionInfo)
		{
			StateTransitionInfo stateTransitionInfo = (StateTransitionInfo)value;
			if (!string.IsNullOrEmpty(stateTransitionInfo.Source) || !string.IsNullOrEmpty(stateTransitionInfo.Destination))
			{
				return string.Format(stateTransitionInfo.IsReadOnly ? "{0}:{1}->{2}[{3:R}]" : "{0}:{1}->{2}({3:R})", stateTransitionInfo.AnimationGraphIndex, stateTransitionInfo.Source, stateTransitionInfo.Destination, stateTransitionInfo.Duration);
			}
			return "None";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private StateTransitionInfo GetStateTransitionInfo(string value)
	{
		float result = 0f;
		int result2 = 0;
		string[] array = value.Split(new string[6] { ":", "->", "(", ")", "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 4 || !int.TryParse(array[0].Trim(), out result2) || !float.TryParse(array[3].Trim(), out result))
		{
			return null;
		}
		StateTransitionInfo stateTransitionInfo = new StateTransitionInfo();
		stateTransitionInfo.Source = array[1].Trim();
		stateTransitionInfo.Destination = array[2].Trim();
		stateTransitionInfo.AnimationGraphIndex = result2;
		stateTransitionInfo.Duration = result;
		stateTransitionInfo.IsReadOnly = value.Contains('[');
		return stateTransitionInfo;
	}
}
