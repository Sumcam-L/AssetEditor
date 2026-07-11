using System;

namespace Sce.Atf.Wpf.Applications.WebServices;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public class ProjectMappingAttribute : Attribute
{
	private string m_mapping;

	public string Mapping
	{
		get
		{
			return m_mapping;
		}
		set
		{
			m_mapping = value;
		}
	}

	public ProjectMappingAttribute(string mapping)
	{
		m_mapping = mapping;
	}
}
