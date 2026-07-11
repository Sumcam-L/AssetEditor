using System;

namespace Sce.Atf;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class AssemblyBannerAttribute : Attribute
{
	private readonly string m_path;

	public string BannerPath => m_path;

	public AssemblyBannerAttribute()
	{
	}

	public AssemblyBannerAttribute(string path)
	{
		m_path = path;
	}
}
