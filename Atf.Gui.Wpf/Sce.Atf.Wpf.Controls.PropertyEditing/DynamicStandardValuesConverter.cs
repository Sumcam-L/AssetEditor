using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class DynamicStandardValuesConverter : TypeConverter
{
	private readonly Func<ITypeDescriptorContext, Array> m_getValues;

	public DynamicStandardValuesConverter(Func<ITypeDescriptorContext, Array> getValues)
	{
		Requires.NotNull(getValues, "getValues");
		m_getValues = getValues;
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		Array source = m_getValues(context);
		HashSet<object> hashSet = new HashSet<object>();
		return source.Cast<object>().All(hashSet.Add);
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		return new StandardValuesCollection(m_getValues(context));
	}
}
