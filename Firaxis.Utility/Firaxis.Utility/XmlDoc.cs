using System;
using System.IO;
using System.Xml;

namespace Firaxis.Utility;

public class XmlDoc
{
	private XmlDocument doc;

	public XmlDocument Source => doc;

	public XmlNode Root => doc;

	public static XmlDoc LoadFromVirtualSpace(string file)
	{
		XmlDoc xmlDoc = new XmlDoc(file);
		if (xmlDoc == null)
		{
			throw new ArgumentException("Could not find file to load.");
		}
		return xmlDoc;
	}

	public XmlDoc()
	{
		doc = new XmlDocument();
	}

	public XmlDoc(XmlDocument doc)
	{
		this.doc = doc;
	}

	public XmlDoc(Stream stream)
	{
		doc = new XmlDocument();
		doc.Load(stream);
	}

	public XmlDoc(string fileName)
	{
		doc = new XmlDocument();
		Load(fileName);
	}

	public void Load(Stream stream)
	{
		doc.Load(stream);
	}

	public void Load(string fileName)
	{
		doc.Load(fileName);
	}

	public void Save(string fileName)
	{
		doc.Save(fileName);
	}

	public void Clear()
	{
		doc.RemoveAll();
	}

	public void Remove(XmlNode node)
	{
		node?.ParentNode?.RemoveChild(node);
	}

	public XmlNode Child(XmlNode node, string tag)
	{
		if (node != null)
		{
			node = node.FirstChild;
			while (node != null && !IsElement(node, tag))
			{
				node = node.NextSibling;
			}
		}
		return node;
	}

	public XmlNode Child(XmlNode node)
	{
		if (node != null)
		{
			node = node.FirstChild;
			while (node != null && !IsElement(node))
			{
				node = node.NextSibling;
			}
		}
		return node;
	}

	public XmlNode Sibling(XmlNode node)
	{
		if (node != null)
		{
			node = node.NextSibling;
		}
		while (node != null && !IsElement(node))
		{
			node = node.NextSibling;
		}
		return node;
	}

	public XmlNode Sibling(XmlNode node, string tag)
	{
		if (node != null)
		{
			node = node.NextSibling;
		}
		while (node != null && !IsElement(node, tag))
		{
			node = node.NextSibling;
		}
		return node;
	}

	public XmlNode Parent(XmlNode node)
	{
		return node?.ParentNode;
	}

	public XmlNode AddRoot(string name)
	{
		if (Child(doc) != null)
		{
			throw new Exception("Xml document already has a root");
		}
		return AddNode(doc, name);
	}

	public XmlNode AddNode(XmlNode parent, string name)
	{
		XmlElement newChild = doc.CreateElement(name);
		return parent.AppendChild(newChild);
	}

	public XmlNode Find(string tag)
	{
		return IterateFind(doc, tag);
	}

	public XmlNode Find(XmlNode node, string tag)
	{
		return IterateFind(node, tag);
	}

	public void SetAttrib(XmlNode node, string name, string value)
	{
		XmlAttribute xmlAttribute = doc.CreateAttribute(name);
		xmlAttribute.Value = value;
		node.Attributes.SetNamedItem(xmlAttribute);
	}

	public void SetAttrib<T>(XmlNode node, string name, T value)
	{
		XmlAttribute xmlAttribute = doc.CreateAttribute(name);
		xmlAttribute.Value = Transpose.ToString(value);
		node.Attributes.SetNamedItem(xmlAttribute);
	}

	public string GetAttrib(XmlNode node, string name)
	{
		return GetAttrib(node, name, "");
	}

	public string GetAttrib(XmlNode node, string name, string value)
	{
		XmlAttribute xmlAttribute = FindAttribute(node, name);
		if (xmlAttribute != null)
		{
			return xmlAttribute.Value;
		}
		return value;
	}

	public T GetAttrib<T>(XmlNode node, string name)
	{
		return GetAttrib(node, name, default(T));
	}

	public T GetAttrib<T>(XmlNode node, string name, T value)
	{
		XmlAttribute xmlAttribute = FindAttribute(node, name);
		if (xmlAttribute != null)
		{
			return Transpose.FromString<T>(xmlAttribute.Value);
		}
		return value;
	}

	private XmlAttribute FindAttribute(XmlNode node, string name)
	{
		if (node != null)
		{
			XmlAttributeCollection attributes = node.Attributes;
			return (XmlAttribute)attributes.GetNamedItem(name);
		}
		return null;
	}

	public bool IsElement(XmlNode node)
	{
		return node != null && node.NodeType == XmlNodeType.Element;
	}

	public bool IsElement(XmlNode node, string tag)
	{
		if (node != null && node.NodeType == XmlNodeType.Element)
		{
			XmlElement xmlElement = (XmlElement)node;
			if (xmlElement.Name.CompareTo(tag) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public string GetText(XmlNode node, string tag)
	{
		return GetText(Find(node, tag));
	}

	public string GetText(XmlNode node)
	{
		if (IsElement(node))
		{
			node = node.FirstChild;
			while (node != null && node.NodeType != XmlNodeType.Text)
			{
				node = node.NextSibling;
			}
			if (node != null)
			{
				return node.Value;
			}
		}
		return "";
	}

	private XmlNode FindTextNode(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Text)
			{
				return childNode;
			}
		}
		return null;
	}

	public void SetText(XmlNode node, string text)
	{
		SetText(node, text, removeIfEmpty: false);
	}

	public void SetText(XmlNode node, string text, bool removeIfEmpty)
	{
		if (IsElement(node))
		{
			XmlNode oldChild;
			while ((oldChild = FindTextNode(node)) != null)
			{
				node.RemoveChild(oldChild);
			}
			if (!removeIfEmpty || text.Length > 0)
			{
				XmlText newChild = doc.CreateTextNode(text);
				node.AppendChild(newChild);
			}
		}
	}

	public void SetText(XmlNode node, string tag, string text)
	{
		SetText(node, tag, text, removeIfEmpty: false);
	}

	public void SetText(XmlNode node, string tag, string text, bool removeIfEmpty)
	{
		XmlNode xmlNode = Find(node, tag);
		if (xmlNode == null)
		{
			xmlNode = AddNode(node, tag);
		}
		SetText(xmlNode, text, removeIfEmpty);
	}

	private XmlNode IterateFind(XmlNode node, string tag)
	{
		if (IsElement(node, tag))
		{
			return node;
		}
		for (node = Child(node); node != null; node = Sibling(node))
		{
			XmlNode xmlNode = IterateFind(node, tag);
			if (xmlNode != null)
			{
				return xmlNode;
			}
		}
		return null;
	}
}
