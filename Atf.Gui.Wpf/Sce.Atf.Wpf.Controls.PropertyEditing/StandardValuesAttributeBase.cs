using System;
using System.Collections;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property)]
public abstract class StandardValuesAttributeBase : Attribute
{
	public abstract object[] GetValues(IEnumerable components);
}
