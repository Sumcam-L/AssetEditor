using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class DomXmlWriter
{
	private readonly XmlSchemaTypeCollection m_typeCollection;

	private DomNode m_root;

	private Uri m_uri;

	private Dictionary<string, string> m_inlinePrefixes;

	private string m_elementNS;

	private string m_elementPrefix;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	public Uri Uri => m_uri;

	public DomNode Root => m_root;

	public bool PreserveSimpleElements { get; set; }

	public bool PersistDefaultAttributes { get; set; }

	public DomXmlWriter(XmlSchemaTypeCollection typeCollection)
	{
		m_typeCollection = typeCollection;
	}

	public virtual void Write(DomNode root, Stream stream, Uri uri)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.IndentChars = "\t";
		xmlWriterSettings.NewLineHandling = NewLineHandling.Replace;
		xmlWriterSettings.NewLineChars = "\r\n";
		Write(root, stream, uri, xmlWriterSettings);
	}

	public virtual void Write(DomNode root, Stream stream, Uri uri, XmlWriterSettings settings)
	{
		try
		{
			m_uri = uri;
			m_root = root;
			m_inlinePrefixes = new Dictionary<string, string>();
			using XmlWriter xmlWriter = XmlWriter.Create(stream, settings);
			xmlWriter.WriteStartDocument();
			WriteElement(root, xmlWriter);
			xmlWriter.WriteEndDocument();
		}
		finally
		{
			m_uri = null;
			m_root = null;
			m_inlinePrefixes = null;
		}
	}

	protected virtual void WriteElement(DomNode node, XmlWriter writer)
	{
		WriteStartElement(node, writer);
		WriteAttributes(node, writer);
		WriteChildElementsRecursive(node, writer);
		writer.WriteEndElement();
	}

	protected void WriteStartElement(DomNode node, XmlWriter writer)
	{
		if (node.ChildInfo == null)
		{
			throw new InvalidOperationException("Please check your document's creation method to ensure that the root DomNode's constructor was given a ChildInfo.");
		}
		m_elementNS = m_typeCollection.TargetNamespace;
		int num = node.ChildInfo.Type.Name.LastIndexOf(':');
		if (num >= 0)
		{
			m_elementNS = node.ChildInfo.Type.Name.Substring(0, num);
		}
		m_elementPrefix = string.Empty;
		if (IsRootNode(node))
		{
			m_elementPrefix = m_typeCollection.GetPrefix(m_elementNS);
			if (m_elementPrefix == null)
			{
				m_elementPrefix = GeneratePrefix(m_elementNS);
			}
			writer.WriteStartElement(m_elementPrefix, node.ChildInfo.Name, m_elementNS);
			writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			XmlQualifiedName[] namespaces = m_typeCollection.Namespaces;
			foreach (XmlQualifiedName xmlQualifiedName in namespaces)
			{
				if (xmlQualifiedName.Name != m_elementPrefix)
				{
					writer.WriteAttributeString("xmlns", xmlQualifiedName.Name, null, xmlQualifiedName.Namespace);
				}
			}
			return;
		}
		ChildInfo childInfo = node.ChildInfo;
		SubstitutionGroupChildRule substitutionGroupChildRule = node.ChildInfo.Rules.OfType<SubstitutionGroupChildRule>().FirstOrDefault();
		if (substitutionGroupChildRule != null)
		{
			ChildInfo childInfo2 = substitutionGroupChildRule.Substitutions.FirstOrDefault((ChildInfo x) => x.Type.IsAssignableFrom(node.Type));
			if (childInfo2 == null)
			{
				throw new InvalidOperationException("No suitable Substitution Group found for node " + node);
			}
			childInfo = childInfo2;
			m_elementNS = m_typeCollection.TargetNamespace;
			num = childInfo2.Type.Name.LastIndexOf(':');
			if (num >= 0)
			{
				m_elementNS = childInfo2.Type.Name.Substring(0, num);
			}
			m_elementPrefix = writer.LookupPrefix(m_elementNS);
			if (m_elementPrefix == null)
			{
				m_elementPrefix = m_typeCollection.GetPrefix(m_elementNS);
			}
		}
		else
		{
			m_elementPrefix = writer.LookupPrefix(m_elementNS);
		}
		if (m_elementPrefix == null)
		{
			m_elementPrefix = GeneratePrefix(m_elementNS);
		}
		writer.WriteStartElement(m_elementPrefix, childInfo.Name, m_elementNS);
	}

	protected virtual string Convert(DomNode node, AttributeInfo attributeInfo)
	{
		string text = null;
		object attribute = node.GetAttribute(attributeInfo);
		if (attributeInfo.Type.Type == AttributeTypes.Reference && attribute is DomNode refNode)
		{
			text = GetNodeReferenceString(refNode, m_root, m_uri);
		}
		if (text == null)
		{
			text = attributeInfo.Type.Convert(attribute);
		}
		return text;
	}

	protected virtual void WriteAttributes(DomNode node, XmlWriter writer)
	{
		DomNodeType type = node.Type;
		if (node.ChildInfo.Type != type && (IsRootNode(node) || node.ChildInfo.Rules.OfType<SubstitutionGroupChildRule>().FirstOrDefault() == null))
		{
			string text = type.Name;
			int num = text.LastIndexOf(':');
			if (num >= 0)
			{
				string text2 = text.Substring(num + 1, type.Name.Length - num - 1);
				string text3 = text.Substring(0, num);
				string text4 = writer.LookupPrefix(text3);
				if (text4 == null)
				{
					text4 = GeneratePrefix(text3);
					writer.WriteAttributeString("xmlns", text4, null, text3);
				}
				text = text2;
				if (text4 != string.Empty)
				{
					text = text4 + ":" + text2;
				}
			}
			writer.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", text);
		}
		AttributeInfo attributeInfo = null;
		List<AttributeInfo> list = new List<AttributeInfo>();
		foreach (AttributeInfo attribute in type.Attributes)
		{
			if (ShouldWriteAttribute(node, attribute))
			{
				if (attribute.Name == string.Empty)
				{
					attributeInfo = attribute;
				}
				else if (PreserveSimpleElements && attribute is XmlAttributeInfo { IsElement: not false } xmlAttributeInfo)
				{
					list.Add(xmlAttributeInfo);
				}
				else
				{
					WriteXmlAttribute(node, attribute, writer);
				}
			}
		}
		if (attributeInfo != null)
		{
			string text5 = Convert(node, attributeInfo);
			writer.WriteString(text5);
		}
		foreach (AttributeInfo item in list)
		{
			writer.WriteStartElement(m_elementPrefix, item.Name, m_elementNS);
			string text6 = Convert(node, item);
			writer.WriteString(text6);
			writer.WriteEndElement();
		}
	}

	protected virtual void WriteXmlAttribute(DomNode node, AttributeInfo attributeInfo, XmlWriter writer)
	{
		string value = Convert(node, attributeInfo);
		writer.WriteAttributeString(attributeInfo.Name, value);
	}

	protected virtual void WriteChildElementsRecursive(DomNode node, XmlWriter writer)
	{
		foreach (ChildInfo child2 in node.Type.Children)
		{
			if (child2.IsList)
			{
				foreach (DomNode child3 in node.GetChildList(child2))
				{
					WriteElement(child3, writer);
				}
			}
			else
			{
				DomNode child = node.GetChild(child2);
				if (child != null)
				{
					WriteElement(child, writer);
				}
			}
		}
	}

	protected virtual string GetNodeReferenceString(DomNode refNode, DomNode root, Uri uri)
	{
		string text = refNode.GetId();
		if (!refNode.IsDescendantOf(root))
		{
			DomNode root2 = refNode.GetRoot();
			IResource resource = root2.As<IResource>();
			if (resource != null)
			{
				Uri uri2 = uri.MakeRelativeUri(resource.Uri);
				text = string.Concat(uri2, "#", text);
			}
		}
		return text;
	}

	protected virtual bool IsRootNode(DomNode node)
	{
		return node == m_root;
	}

	protected virtual bool ShouldWriteAttribute(DomNode node, AttributeInfo attributeInfo)
	{
		return PersistDefaultAttributes || !node.IsAttributeDefault(attributeInfo);
	}

	protected string GeneratePrefix(string ns)
	{
		string value = null;
		if (!string.IsNullOrEmpty(ns) && !m_inlinePrefixes.TryGetValue(ns, out value))
		{
			int count = m_inlinePrefixes.Count;
			value = "_p" + count;
			m_inlinePrefixes.Add(ns, value);
		}
		return value;
	}
}
