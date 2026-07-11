using System.Collections.Generic;
using System.Xml;

namespace Sce.Atf.Dom;

public class XmlSchemaTypeCollection
{
	private readonly string m_targetNamespace;

	private readonly XmlQualifiedName[] m_namespaces;

	private readonly XmlSchemaTypeLoader m_loader;

	private readonly string m_defaultNamespace = string.Empty;

	public XmlQualifiedName[] Namespaces => m_namespaces;

	public string TargetNamespace => m_targetNamespace;

	public string DefaultNamespace => m_defaultNamespace;

	internal XmlSchemaTypeCollection(XmlQualifiedName[] namespaces, string targetNamespace, XmlSchemaTypeLoader loader)
	{
		m_namespaces = namespaces;
		m_targetNamespace = targetNamespace;
		m_loader = loader;
		XmlQualifiedName[] namespaces2 = m_namespaces;
		foreach (XmlQualifiedName xmlQualifiedName in namespaces2)
		{
			if (xmlQualifiedName.Name == string.Empty)
			{
				m_defaultNamespace = xmlQualifiedName.Namespace;
				break;
			}
		}
	}

	public string GetPrefix(string ns)
	{
		if (ns == m_defaultNamespace)
		{
			return string.Empty;
		}
		XmlQualifiedName[] namespaces = m_namespaces;
		foreach (XmlQualifiedName xmlQualifiedName in namespaces)
		{
			if (xmlQualifiedName.Namespace == ns)
			{
				return xmlQualifiedName.Name;
			}
		}
		return null;
	}

	public IEnumerable<AttributeType> GetAttributeTypes()
	{
		return m_loader.GetAttributeTypes(m_targetNamespace);
	}

	public AttributeType GetAttributeType(string name)
	{
		return m_loader.GetAttributeType(m_targetNamespace + ":" + name);
	}

	public IEnumerable<DomNodeType> GetNodeTypes()
	{
		return m_loader.GetNodeTypes(m_targetNamespace);
	}

	public IEnumerable<DomNodeType> GetNodeTypes(string ns)
	{
		return m_loader.GetNodeTypes(ns);
	}

	public DomNodeType GetNodeType(string name)
	{
		return m_loader.GetNodeType(m_targetNamespace + ":" + name);
	}

	public DomNodeType GetNodeType(string targetNamespace, string name)
	{
		return m_loader.GetNodeType(targetNamespace + ":" + name);
	}

	public ChildInfo GetRootElement(string name)
	{
		return m_loader.GetRootElement(m_targetNamespace + ":" + name);
	}

	public ChildInfo GetRootElement(string targetNamespace, string name)
	{
		return m_loader.GetRootElement(targetNamespace + ":" + name);
	}

	public IEnumerable<ChildInfo> GetRootElements()
	{
		return m_loader.GetRootElements();
	}
}
