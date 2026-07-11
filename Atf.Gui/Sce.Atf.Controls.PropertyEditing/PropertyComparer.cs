using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyComparer<T> : IComparer<T>
{
	private readonly bool _descending;

	private readonly PropertyDescriptor _property;

	public bool Descending => _descending;

	public PropertyDescriptor Property => _property;

	public PropertyComparer(PropertyDescriptor property, bool descending)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		_descending = descending;
		_property = property;
	}

	public int Compare(T x, T y)
	{
		int num = Comparer.Default.Compare(_property.GetValue(x), _property.GetValue(y));
		return _descending ? (-num) : num;
	}
}
