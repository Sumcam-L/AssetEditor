using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumDisplayNameAttribute : Attribute
{
	private readonly string m_value;

	public string Value => m_value;

	public EnumDisplayNameAttribute()
	{
	}

	public EnumDisplayNameAttribute(string displayName)
	{
		m_value = displayName;
	}
}
