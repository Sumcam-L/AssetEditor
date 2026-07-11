using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class CategoryUniqueIdValidator : IdValidator
{
	private UniqueNamer m_defaultUniqueNamer;

	private Dictionary<object, UniqueNamer> m_uniqueNamers = new Dictionary<object, UniqueNamer>();

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

	protected virtual object GetIdCategory(DomNode node)
	{
		return null;
	}

	protected override void OnNodeSet()
	{
		m_defaultUniqueNamer = new UniqueNamer(m_suffixSeparator);
		base.OnNodeSet();
	}

	protected override void ValidateSubtree()
	{
		foreach (DomNode item in base.DomNode.Subtree)
		{
			if (item.Type.IdAttribute != null)
			{
				string id = item.GetId();
				UniqueNamer uniqueNamer = GetUniqueNamer(GetIdCategory(item));
				string text = uniqueNamer.Name(id);
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
			UniqueNamer uniqueNamer = GetUniqueNamer(GetIdCategory(item));
			uniqueNamer.Retire(value);
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
			UniqueNamer uniqueNamer = GetUniqueNamer(GetIdCategory(item.Key));
			uniqueNamer.Retire(item.Value);
			NameNode(item.Key);
		}
	}

	private void NameNode(DomNode node)
	{
		string id = node.GetId();
		if (node.Type.IdAttribute != null)
		{
			UniqueNamer uniqueNamer = GetUniqueNamer(GetIdCategory(node));
			string text = uniqueNamer.Name(id);
			if (id != text)
			{
				node.SetAttribute(node.Type.IdAttribute, text);
			}
		}
	}

	private UniqueNamer GetUniqueNamer(object category)
	{
		if (category == null)
		{
			return m_defaultUniqueNamer;
		}
		if (!m_uniqueNamers.TryGetValue(category, out var _))
		{
			m_uniqueNamers.Add(category, new UniqueNamer(m_suffixSeparator));
		}
		return m_uniqueNamers[category];
	}
}
