using System;

namespace Sce.Atf;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public class GroupAttribute : Attribute
{
	private readonly string m_groupName;

	private string m_readOnlyProperties;

	private string m_externalEditorProperties;

	public string GroupName => m_groupName;

	public string Header { get; set; }

	public string ReadOnlyProperties
	{
		get
		{
			return m_readOnlyProperties;
		}
		set
		{
			m_readOnlyProperties = value;
		}
	}

	public string ExternalEditorProperties
	{
		get
		{
			return m_externalEditorProperties;
		}
		set
		{
			m_externalEditorProperties = value;
		}
	}

	public GroupAttribute(string groupName)
	{
		m_groupName = groupName;
	}
}
