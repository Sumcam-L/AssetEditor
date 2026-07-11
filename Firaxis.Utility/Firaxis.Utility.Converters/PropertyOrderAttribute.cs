using System;

namespace Firaxis.Utility.Converters;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyOrderAttribute : Attribute
{
	private int m_order;

	public int Order => m_order;

	public PropertyOrderAttribute(int order)
	{
		m_order = order;
	}
}
