using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class SourceObjectTypeConverter : EnumTypeConverter
{
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		ISourceObjectAdapter sourceObjectAdapter = context.Instance.As<ISourceObjectAdapter>();
		if (sourceObjectAdapter == null)
		{
			return new StandardValuesCollection(new string[0]);
		}
		return new StandardValuesCollection(sourceObjectAdapter.GetCurrentSourceObjects());
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return context.Instance != null;
	}
}
