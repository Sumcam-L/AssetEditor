using System;

namespace Sce.Atf;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class IconResourceAttribute : Attribute
{
	private readonly string m_iconName;

	public string IconName => m_iconName;

	public IconResourceAttribute(string iconName)
	{
		m_iconName = iconName;
	}
}
