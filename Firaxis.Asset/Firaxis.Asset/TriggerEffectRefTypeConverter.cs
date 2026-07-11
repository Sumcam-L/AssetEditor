using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Firaxis.Asset.Trigger;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class TriggerEffectRefTypeConverter : TypeConverter
{
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}

	private bool MultipleTriggers(object objIns)
	{
		return objIns is object[];
	}

	private bool TriggersHaveSameOwner(object objIns)
	{
		object[] array = (object[])objIns;
		ITriggerSystem triggerSystem = null;
		object[] array2 = array;
		foreach (object obj in array2)
		{
			if (!(obj is ITrigger trigger))
			{
				return false;
			}
			if (triggerSystem == null)
			{
				triggerSystem = trigger.Owner;
			}
			if (triggerSystem != trigger.Owner)
			{
				return false;
			}
		}
		return true;
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		List<string> list = new List<string>();
		list.Add("None (0)");
		ITrigger trigger = null;
		if (MultipleTriggers(context.Instance))
		{
			if (TriggersHaveSameOwner(context.Instance))
			{
				trigger = (ITrigger)((object[])context.Instance)[0];
			}
		}
		else
		{
			trigger = (ITrigger)context.Instance;
		}
		if (trigger != null)
		{
			string trackID = ((ITriggerTrackInfo)trigger).TrackID;
			foreach (ITrigger trigger2 in trigger.Owner.Triggers)
			{
				if (((ITriggerTrackInfo)trigger2).TrackID == trackID && trigger2 is ITriggerEffect)
				{
					list.Add($"{((ITriggerEffect)trigger2).Event} ({trigger2.ID})");
				}
			}
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
			return GetRefId((string)value);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is int)
		{
			int num = (int)value;
			ITrigger trigger = null;
			if (MultipleTriggers(context.Instance))
			{
				if (TriggersHaveSameOwner(context.Instance))
				{
					trigger = (ITrigger)((object[])context.Instance)[0];
				}
			}
			else
			{
				trigger = (ITrigger)context.Instance;
			}
			if (trigger != null)
			{
				foreach (ITrigger trigger2 in trigger.Owner.Triggers)
				{
					if (trigger2.ID == num)
					{
						return $"{((ITriggerEffect)trigger2).Event} ({trigger2.ID})";
					}
				}
			}
			return "None (0)";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private int GetRefId(string value)
	{
		int result = 0;
		if (!string.IsNullOrEmpty(value) && !int.TryParse(value, out result))
		{
			int num = value.IndexOf('(');
			if (num != -1)
			{
				int num2 = value.IndexOf(')');
				if (num2 != -1)
				{
					result = Transpose.FromString<int>(value.Substring(num + 1, num2 - num - 1));
				}
			}
		}
		return result;
	}
}
