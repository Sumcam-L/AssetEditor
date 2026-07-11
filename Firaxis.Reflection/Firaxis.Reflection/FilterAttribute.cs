using System;

namespace Firaxis.Reflection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class FilterAttribute : Attribute
{
	private string filter;

	public string Filter => filter;

	public FilterAttribute()
	{
		filter = "";
	}

	public FilterAttribute(string filter)
	{
		this.filter = filter;
	}
}
