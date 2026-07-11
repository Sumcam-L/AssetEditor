using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ArtDefRefCollectionTypeConverter : EnumTypeConverter
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

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		ArtDefRefFieldValueAdapter adapterFromInstance = GetAdapterFromInstance(context);
		string[] values = Enumerable.Empty<string>().ToArray();
		if (adapterFromInstance != null)
		{
			values = CivTechRegistry.ArtDefRegistry.GetSuitableCollections(adapterFromInstance.Value, adapterFromInstance.Parameter);
		}
		return new StandardValuesCollection(values);
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
