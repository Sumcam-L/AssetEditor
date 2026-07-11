using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class UniqueIdValidator : IdValidator
{
	private UniqueNamer m_uniqueNamer;

	private char m_suffixSeparator = '_';

	protected char SuffixSeparator
	{
		get
		{
			return m_suffixSeparator;
		}
		set
		{
			m_suffixSeparator = value;
		}
	}

	protected override void OnNodeSet()
	{
		m_uniqueNamer = new UniqueNamer(m_suffixSeparator);
		base.OnNodeSet();
	}

	protected override void ValidateSubtree()
	{
		foreach (DomNode item in base.DomNode.Subtree)
		{
			if (item.Type.IdAttribute != null)
			{
				string id = item.GetId();
				string text = m_uniqueNamer.Name(id);
				if (id != text)
				{
					OnIdCollision(item, text);
				}
			}
		}
	}

	protected override void RemoveNodes(HashSet<DomNode> removed, Dictionary<DomNode, string> renamed)
	{
		foreach (DomNode item in removed)
		{
			if (renamed.TryGetValue(item, out var value))
			{
				renamed.Remove(item);
			}
			else
			{
				value = item.GetId();
			}
			m_uniqueNamer.Retire(value);
		}
	}

	protected override void AddNodes(HashSet<DomNode> added, Dictionary<DomNode, string> renamed)
	{
		foreach (DomNode item in added)
		{
			renamed.Remove(item);
			NameNode(item);
		}
	}

	protected override void RenameNodes(Dictionary<DomNode, string> renamed)
	{
		foreach (KeyValuePair<DomNode, string> item in renamed)
		{
			m_uniqueNamer.Retire(item.Value);
			NameNode(item.Key);
		}
	}

	private void NameNode(DomNode node)
	{
		string id = node.GetId();
		if (node.Type.IdAttribute != null)
		{
			string text = m_uniqueNamer.Name(id);
			if (id != text)
			{
				node.SetAttribute(node.Type.IdAttribute, text);
			}
		}
	}
}
