using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

public class DomainProviderUITypeEditor<T> : EnumUITypeEditor
{
	private IDomainProvider<T> m_domainProvider;

	public DomainProviderUITypeEditor(IDomainProvider<T> domainProvider)
	{
		m_domainProvider = domainProvider;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		List<string> list = new List<string>();
		foreach (T possibleValue in m_domainProvider.PossibleValues)
		{
			list.Add(possibleValue.ToString());
		}
		DefineEnum(list.ToArray());
		return base.EditValue(context, provider, value);
	}
}
