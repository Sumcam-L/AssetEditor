using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Sce.Atf.Dom;

public class DomXmlReader
{
	private static readonly char[] s_trimChars = new char[1] { '|' };

	private readonly XmlSchemaTypeLoader m_typeLoader;

	private DomNode m_root;

	private Uri m_uri;

	private readonly Dictionary<string, DomNode> m_nodeDictionary = new Dictionary<string, DomNode>();

	private List<XmlNodeReference> m_nodeReferences = new List<XmlNodeReference>();

	public XmlSchemaTypeLoader TypeLoader => m_typeLoader;

	public Uri Uri => m_uri;

	public DomNode Root => m_root;

	public IDictionary<string, DomNode> NodeDictionary => m_nodeDictionary;

	public IEnumerable<XmlNodeReference> UnresolvedReferences
	{
		get
		{
			return m_nodeReferences;
		}
		protected set
		{
			m_nodeReferences = value.ToList();
		}
	}

	protected IList<XmlNodeReference> NodeReferences => m_nodeReferences;

	public DomXmlReader(XmlSchemaTypeLoader typeLoader)
	{
		m_typeLoader = typeLoader;
	}

	public virtual DomNode Read(Stream stream, Uri uri)
	{
		m_uri = uri;
		m_root = null;
		m_nodeDictionary.Clear();
		m_nodeReferences.Clear();
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = true;
		xmlReaderSettings.IgnoreProcessingInstructions = true;
		using (XmlReader xmlReader = XmlReader.Create(stream, xmlReaderSettings))
		{
			xmlReader.MoveToContent();
			ChildInfo childInfo = CreateRootElement(xmlReader, m_uri);
			if (childInfo == null)
			{
				throw new InvalidOperationException("No root element was found in the XML document, probably due to a namespace mismatch with the schema file");
			}
			m_root = ReadElement(childInfo, xmlReader);
			ResolveReferences();
		}
		return m_root;
	}

	protected virtual ChildInfo CreateRootElement(XmlReader reader, Uri rootUri)
	{
		string text = reader.NamespaceURI;
		if (string.IsNullOrEmpty(text))
		{
			using IEnumerator<XmlSchemaTypeCollection> enumerator = m_typeLoader.GetTypeCollections().GetEnumerator();
			if (enumerator.MoveNext())
			{
				XmlSchemaTypeCollection current = enumerator.Current;
				text = current.DefaultNamespace;
			}
		}
		return m_typeLoader.GetRootElement(text + ":" + reader.LocalName);
	}

	protected virtual void ReadAttribute(DomNode node, AttributeInfo attributeInfo, string valueString)
	{
		if (IsReferenceAttribute(attributeInfo))
		{
			m_nodeReferences.Add(new XmlNodeReference(node, attributeInfo, valueString));
			return;
		}
		object value = attributeInfo.Type.Convert(valueString);
		node.SetAttribute(attributeInfo, value);
	}

	protected virtual DomNode ReadElement(ChildInfo nodeInfo, XmlReader reader)
	{
		DomNodeType domNodeType = null;
		SubstitutionGroupChildRule substitutionGroupChildRule = nodeInfo.Rules.OfType<SubstitutionGroupChildRule>().FirstOrDefault();
		if (substitutionGroupChildRule != null)
		{
			foreach (ChildInfo substitution in substitutionGroupChildRule.Substitutions)
			{
				if (substitution.Name == reader.LocalName)
				{
					domNodeType = substitution.Type;
					break;
				}
			}
			if (domNodeType == null)
			{
				domNodeType = GetChildType(nodeInfo.Type, reader);
			}
			if (domNodeType == null)
			{
				throw new InvalidOperationException("Could not match substitution group for child " + nodeInfo.Name);
			}
		}
		else
		{
			domNodeType = GetChildType(nodeInfo.Type, reader);
		}
		int length = domNodeType.Name.LastIndexOf(':');
		string text = domNodeType.Name.Substring(0, length);
		DomNode domNode = new DomNode(domNodeType, nodeInfo);
		while (reader.MoveToNextAttribute())
		{
			if (reader.Prefix == string.Empty || reader.LookupNamespace(reader.Prefix) == text)
			{
				AttributeInfo attributeInfo = domNodeType.GetAttributeInfo(reader.LocalName);
				if (attributeInfo != null)
				{
					ReadAttribute(domNode, attributeInfo, reader.Value);
				}
			}
		}
		if (domNode.Type.IdAttribute != null)
		{
			string id = domNode.GetId();
			if (!string.IsNullOrEmpty(id))
			{
				m_nodeDictionary[id] = domNode;
			}
		}
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					ChildInfo childInfo = domNodeType.GetChildInfo(reader.LocalName);
					if (childInfo == null)
					{
						childInfo = GetSubsitutionGroup(domNodeType, reader.LocalName);
					}
					if (childInfo != null)
					{
						DomNode domNode2 = ReadElement(childInfo, reader);
						if (domNode2 != null)
						{
							if (childInfo.IsList)
							{
								domNode.GetChildList(childInfo).Add(domNode2);
							}
							else
							{
								domNode.SetChild(childInfo, domNode2);
							}
						}
						continue;
					}
					AttributeInfo attributeInfo2 = domNodeType.GetAttributeInfo(reader.LocalName);
					if (attributeInfo2 != null)
					{
						reader.MoveToElement();
						if (reader.IsEmptyElement)
						{
							continue;
						}
						while (reader.Read())
						{
							if (reader.NodeType == XmlNodeType.Text)
							{
								ReadAttribute(domNode, attributeInfo2, reader.Value);
								reader.Skip();
								break;
							}
							if (reader.NodeType == XmlNodeType.EndElement)
							{
								break;
							}
						}
						reader.MoveToContent();
					}
					else
					{
						reader.Skip();
						if (reader.NodeType == XmlNodeType.EndElement)
						{
							break;
						}
					}
				}
				else if (reader.NodeType == XmlNodeType.Text)
				{
					AttributeInfo attributeInfo3 = domNodeType.GetAttributeInfo(string.Empty);
					if (attributeInfo3 != null)
					{
						ReadAttribute(domNode, attributeInfo3, reader.Value);
					}
				}
				else if (reader.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
			}
		}
		reader.MoveToContent();
		return domNode;
	}

	private ChildInfo GetSubsitutionGroup(DomNodeType type, string localName)
	{
		foreach (ChildInfo child in type.Children)
		{
			SubstitutionGroupChildRule substitutionGroupChildRule = child.Rules.OfType<SubstitutionGroupChildRule>().FirstOrDefault();
			if (substitutionGroupChildRule == null)
			{
				continue;
			}
			foreach (ChildInfo substitution in substitutionGroupChildRule.Substitutions)
			{
				if (substitution.Name == localName)
				{
					return child;
				}
			}
		}
		return null;
	}

	protected virtual bool IsReferenceAttribute(AttributeInfo attributeInfo)
	{
		return attributeInfo.Type.Type == AttributeTypes.Reference;
	}

	protected virtual DomNodeType GetDerivedType(DomNodeType baseType, string ns, string typeName)
	{
		return m_typeLoader.GetNodeType(ns + ":" + typeName);
	}

	protected virtual void ResolveReferences()
	{
		List<XmlNodeReference> list = new List<XmlNodeReference>();
		foreach (XmlNodeReference nodeReference in m_nodeReferences)
		{
			string stringToUnescape = nodeReference.Value.TrimStart('#');
			stringToUnescape = Uri.UnescapeDataString(stringToUnescape);
			stringToUnescape = stringToUnescape.TrimStart(s_trimChars);
			if (m_nodeDictionary.TryGetValue(stringToUnescape, out var value))
			{
				nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, value);
				continue;
			}
			list.Add(nodeReference);
			object value2 = nodeReference.AttributeInfo.Type.Convert(nodeReference.Value);
			nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, value2);
		}
		m_nodeReferences = list;
	}

	protected DomNodeType GetChildType(DomNodeType type, XmlReader reader)
	{
		DomNodeType domNodeType = type;
		string text = reader.GetAttribute("xsi:type");
		if (text != null)
		{
			string prefix = string.Empty;
			int num = text.IndexOf(':');
			if (num >= 0)
			{
				prefix = text.Substring(0, num);
				num++;
				text = text.Substring(num, text.Length - num);
			}
			string text2 = reader.LookupNamespace(prefix);
			domNodeType = GetDerivedType(domNodeType, text2, text);
			if (domNodeType == null)
			{
				string arg = ((type != null) ? type.Name : "<none>");
				throw new InvalidOperationException($"No type was found with the name {text} in namespace {text2} that derives from {arg}");
			}
		}
		return domNodeType;
	}
}
