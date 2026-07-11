using System;
using System.ComponentModel;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ArtDefRefCollectionUITypeEditor : EnumUITypeEditor
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
			artDefRefFieldValueAdapter = context.Instance.As<ArtDefRefFieldValueAdapter>();
		}
		if (artDefRefFieldValueAdapter == null)
		{
			AdaptablePath<object> adaptablePath = (AdaptablePath<object>)context.Instance;
			artDefRefFieldValueAdapter = ((adaptablePath != null) ? adaptablePath.Last.As<ArtDefRefFieldValueAdapter>() : null);
		}
		return artDefRefFieldValueAdapter;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		ArtDefRefFieldValueAdapter adapterFromInstance = GetAdapterFromInstance(context);
		if (adapterFromInstance != null)
		{
			string[] suitableCollections = CivTechRegistry.ArtDefRegistry.GetSuitableCollections(adapterFromInstance.Value, adapterFromInstance.Parameter);
			DefineEnum(suitableCollections);
		}
		return base.EditValue(context, provider, value);
	}
}
