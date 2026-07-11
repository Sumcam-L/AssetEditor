using System.ComponentModel;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ModelInstanceUITypeConverter : EnumTypeConverter
{
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		return new StandardValuesCollection(new string[0]);
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
