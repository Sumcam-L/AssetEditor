using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class UniquePathIdValidator : IdValidator
{
	private char m_suffixSeparator = '_';

	private readonly HashSet<DomNode> m_added = new HashSet<DomNode>();

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

	protected override void ValidateSubtree()
	{
		UniqueNamer uniqueNamer = new UniqueNamer(m_suffixSeparator);
		foreach (DomNode item in base.DomNode.Subtree)
		{
			foreach (DomNode child in item.Children)
			{
				if (child.Type.IdAttribute != null)
				{
					string id = child.GetId();
					string text = uniqueNamer.Name(id);
					if (id != text)
					{
						OnIdCollision(child, text);
					}
				}
			}
			uniqueNamer.Clear();
		}
	}

	protected override void OnEnding(object sender, EventArgs e)
	{
		base.OnEnding(sender, e);
		m_added.Clear();
	}

	protected override void OnCancelled(object sender, EventArgs e)
	{
		base.OnCancelled(sender, e);
		m_added.Clear();
	}

	protected override void RemoveNodes(HashSet<DomNode> removed, Dictionary<DomNode, string> renamed)
	{
		foreach (DomNode item in removed)
		{
			renamed.Remove(item);
		}
	}

	protected override void AddNodes(HashSet<DomNode> added, Dictionary<DomNode, string> renamed)
	{
		HashSet<DomNode> hashSet = new HashSet<DomNode>();
		foreach (DomNode item in m_added)
		{
			renamed.Remove(item);
			hashSet.Add(item.Parent);
		}
		UniqueNamer uniqueNamer = new UniqueNamer(m_suffixSeparator);
		foreach (DomNode item2 in hashSet)
		{
			foreach (DomNode child in item2.Children)
			{
				if (child.Type.IdAttribute != null && !m_added.Contains(child))
				{
					string id = child.GetId();
					uniqueNamer.Name(id);
				}
			}
			foreach (DomNode child2 in item2.Children)
			{
				if (child2.Type.IdAttribute != null && m_added.Contains(child2))
				{
					NameNode(child2, uniqueNamer);
				}
			}
			uniqueNamer.Clear();
		}
	}

	protected override void RenameNodes(Dictionary<DomNode, string> renamed)
	{
		HashSet<DomNode> hashSet = new HashSet<DomNode>();
		foreach (DomNode key in renamed.Keys)
		{
			if (key.Parent != null)
			{
				hashSet.Add(key.Parent);
			}
		}
		UniqueNamer uniqueNamer = new UniqueNamer(m_suffixSeparator);
		foreach (DomNode item in hashSet)
		{
			foreach (DomNode child in item.Children)
			{
				if (child.Type.IdAttribute != null && !renamed.ContainsKey(child))
				{
					string id = child.GetId();
					uniqueNamer.Name(id);
				}
			}
			foreach (DomNode child2 in item.Children)
			{
				if (child2.Type.IdAttribute != null && renamed.ContainsKey(child2))
				{
					NameNode(child2, uniqueNamer);
				}
			}
			uniqueNamer.Clear();
		}
	}

	protected override void AddNode(DomNode node)
	{
		if (base.Validating && !base.Naming && node.Type.IdAttribute != null)
		{
			m_added.Add(node);
		}
		base.AddNode(node);
	}

	private void NameNode(DomNode node, UniqueNamer namer)
	{
		string id = node.GetId();
		string text = namer.Name(id);
		if (id != text)
		{
			node.SetAttribute(node.Type.IdAttribute, text);
		}
	}
}
