using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ArtDefRefElementTypeConverter : EnumTypeConverter
{
	private ArtDefRefFieldValueAdapter GetAdapterFromInstance(ITypeDescriptorContext context)
	{
		if (context == null)
		{
			return null;
		}
		ArtDefRefFieldValueAdapter artDefRefFieldValueAdapter = context.PropertyDescriptor.As<DynamicFieldPropertyDescriptorBase>()?.GetFieldAdapter(context.Instance).As<ArtDefRefFieldValueAdapter>();
		if (artDefRefFieldValueAdapter == null)
		{
			artDefRefFieldValueAdapter = context.PropertyDescriptor.As<FieldPropertyDescriptorBase>()?.GetNode(context.Instance).As<ArtDefRefFieldValueAdapter>();
		}
		if (artDefRefFieldValueAdapter == null)
		{
			artDefRefFieldValueAdapter = context.Instance.As<ArtDefRefFieldValueAdapter>();
		}
		if (artDefRefFieldValueAdapter == null)
		{
			AdaptablePath<object> adaptablePath = context.Instance.As<AdaptablePath<object>>();
			artDefRefFieldValueAdapter = ((adaptablePath != null) ? adaptablePath.Last.As<ArtDefRefFieldValueAdapter>() : null);
		}
		return artDefRefFieldValueAdapter;
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
		if (value is string)
		{
			return base.ConvertTo(context, culture, value, destType);
		}
		return base.ConvertTo(context, culture, (object)((ArtDefReferenceInfo)value).elementName, destType);
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		string[] array = Enumerable.Empty<string>().ToArray();
		ArtDefRefFieldValueAdapter adapterFromInstance = GetAdapterFromInstance(context);
		if (adapterFromInstance != null)
		{
			array = adapterFromInstance.GetElementCollection();
			Array.Sort(array);
			if (adapterFromInstance.Parameter is IArtDefRefParameter { IsNullAllowed: not false } && !array.Contains(string.Empty))
			{
				string[] array2 = new string[array.Length + 1];
				Array.Copy(array, 0, array2, 1, array.Length);
				array2[0] = string.Empty;
				array = array2;
			}
		}
		return new StandardValuesCollection(array);
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return GetAdapterFromInstance(context) != null;
	}
}
