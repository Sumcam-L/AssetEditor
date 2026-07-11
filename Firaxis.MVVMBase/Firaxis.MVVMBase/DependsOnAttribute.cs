using System;

namespace Firaxis.MVVMBase;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DependsOnAttribute : Attribute
{
	public string DependencyName { get; }

	public DependsOnAttribute(string propertyName)
	{
		DependencyName = propertyName;
	}
}
