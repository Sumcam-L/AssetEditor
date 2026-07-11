using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property)]
public class StringFilterAttribute : Attribute
{
	public IStringValueFilter Filter { get; set; }

	public StringFilterAttribute(string filter)
	{
	}
}
