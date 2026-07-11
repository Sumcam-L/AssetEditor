using System;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ArtDefRefElementUITypeEditor : EnumUITypeEditor
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

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		ArtDefRefFieldValueAdapter adapterFromInstance = GetAdapterFromInstance(context);
		if (adapterFromInstance != null)
		{
			string[] array = adapterFromInstance.GetElementCollection();
			Array.Sort(array);
			if (adapterFromInstance.Parameter is IArtDefRefParameter { IsNullAllowed: not false } && !array.Contains(string.Empty))
			{
				string[] array2 = new string[array.Length + 1];
				Array.Copy(array, 0, array2, 1, array.Length);
				array2[0] = string.Empty;
				array = array2;
			}
			DefineEnum(array);
		}
		return base.EditValue(context, provider, value);
	}
}
