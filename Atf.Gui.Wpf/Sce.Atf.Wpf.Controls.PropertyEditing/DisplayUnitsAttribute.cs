using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property)]
public class DisplayUnitsAttribute : Attribute
{
	public string Units { get; set; }

	public DisplayUnitsAttribute(string units)
	{
		Units = units;
	}
}
