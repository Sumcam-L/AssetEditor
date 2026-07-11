using System;
using System.Collections;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property)]
public class StandardValuesAttribute : StandardValuesAttributeBase
{
	public object[] Values { get; set; }

	public StandardValuesAttribute(object[] values)
	{
		Values = values;
	}

	public override object[] GetValues(IEnumerable components)
	{
		return Values;
	}
}
