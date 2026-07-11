using System;

namespace Sce.Atf.Dom;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ObservableDomChildAttribute : Attribute
{
	public string ChildName { get; set; }

	public ObservableDomChildAttribute(string childName)
	{
		ChildName = childName;
	}
}
