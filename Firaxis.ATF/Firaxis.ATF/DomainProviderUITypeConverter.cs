using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

public class DomainProviderUITypeConverter<T> : EnumTypeConverter
{
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		List<T> list = new List<T>();
		if (context.Instance != null && context.Instance is IDomainProvider<T> domainProvider)
		{
			list.AddRange(domainProvider.PossibleValues);
		}
		return new StandardValuesCollection(list);
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
