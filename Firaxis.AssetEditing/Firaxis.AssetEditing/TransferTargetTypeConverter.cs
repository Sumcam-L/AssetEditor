using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class TransferTargetTypeConverter : TypeConverter
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
		string[] values = Enumerable.Empty<string>().ToArray();
		TransferTriggerAdapter transferTriggerAdapter = context.Instance.As<TransferTriggerAdapter>();
		if (transferTriggerAdapter != null)
		{
			values = (from sObj in transferTriggerAdapter.TimelineAdapter.Triggers
				where sObj.TriggerType == TriggerType.TT_ASSET_VFX || sObj.TriggerType == TriggerType.TT_ARTDEF_VFX
				select sObj.Name).ToArray();
		}
		return new StandardValuesCollection(values);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (!(sourceType == typeof(string)))
		{
			return base.CanConvertFrom(context, sourceType);
		}
		return true;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!(destinationType == typeof(string)))
		{
			return base.CanConvertTo(context, destinationType);
		}
		return true;
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
		if (destinationType == typeof(string) && (value is int || value is string))
		{
			int id = 0;
			if (value is int)
			{
				id = (int)value;
			}
			else if (value is string && !int.TryParse((string)value, out id))
			{
				return "None (0)";
			}
			TransferTriggerAdapter transferTriggerAdapter = context.Instance.As<TransferTriggerAdapter>();
			if (transferTriggerAdapter != null)
			{
				TriggerAdapter triggerAdapter = transferTriggerAdapter.TimelineAdapter.Triggers.FirstOrDefault(delegate(TriggerAdapter fObj)
				{
					int result = 0;
					return (int.TryParse(fObj.Name, out result) && result == id) ? true : false;
				});
				if (triggerAdapter != null)
				{
					string text;
					if ((text = triggerAdapter.As<AssetFXTriggerAdapter>()?.VFXAsset) == null)
					{
						text = triggerAdapter.As<ArtDefFXTriggerAdapter>()?.VFXElement;
					}
					return text + " (" + transferTriggerAdapter.Name + ")";
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
