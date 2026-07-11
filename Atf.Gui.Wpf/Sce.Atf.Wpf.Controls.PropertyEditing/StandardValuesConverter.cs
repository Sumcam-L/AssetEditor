using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class StandardValuesConverter : TypeConverter
{
	private readonly StandardValuesCollection m_values;

	private readonly bool m_exclusive;

	public StandardValuesConverter(Array values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		m_values = new StandardValuesCollection(values);
		HashSet<object> hashSet = new HashSet<object>();
		m_exclusive = values.Cast<object>().All(hashSet.Add);
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return m_exclusive;
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		return m_values;
	}
}
