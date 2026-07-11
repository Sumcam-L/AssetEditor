using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public class BoundEnumTypeConverter : TypeConverter
{
	private IEnumParameter EnumParameter { get; set; }

	private string[] EnumTextArray { get; set; }

	public BoundEnumTypeConverter(IEnumParameter enumParam)
	{
		EnumParameter = enumParam;
		EnumTextArray = Enumerable.Empty<string>().ToArray();
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
	{
		return false;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
	{
		return destType == typeof(string);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		if (value is int && destType == typeof(string))
		{
			int num = (int)value;
			if (num < 0)
			{
				return string.Empty;
			}
			IInstanceEntityAdapter enumeratorInstance = ((!(context.Instance is DomNodeAdapter domNodeAdapter)) ? null : domNodeAdapter.DomNode?.GetRoot()?.As<IInstanceEntityAdapter>());
			using ((EnumParameter is IDynamicEnum dynamicEnum) ? dynamicEnum.SetEnumeratorInstance(enumeratorInstance) : null)
			{
				EnumTextArray = EnumParameter.GetEnumerations().ToArray();
			}
			if (num >= EnumTextArray.Length)
			{
				return string.Empty;
			}
			return EnumTextArray[num];
		}
		return base.ConvertTo(context, culture, value, destType);
	}
}
