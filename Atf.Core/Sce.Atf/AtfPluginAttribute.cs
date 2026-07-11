using System;

namespace Sce.Atf;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public class AtfPluginAttribute : Attribute
{
	private readonly string m_info;

	public string Info => m_info;

	public AtfPluginAttribute()
	{
	}

	public AtfPluginAttribute(string info)
	{
		m_info = info;
	}
}
