using System;

namespace Sce.Atf;

[AttributeUsage(AttributeTargets.Field)]
public sealed class DisplayStringAttribute : Attribute
{
	private readonly string m_value;

	public string Value => m_value;

	public DisplayStringAttribute()
	{
	}

	public DisplayStringAttribute(string v)
	{
		m_value = v;
	}
}
