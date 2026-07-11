using System;

namespace Sce.Atf.Dom;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ObservableDomPropertyAttribute : Attribute
{
	public string AttributeName { get; set; }

	public ObservableDomPropertyAttribute(string attributeName)
	{
		AttributeName = attributeName;
	}
}
