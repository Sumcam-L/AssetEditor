using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public abstract class IdValidator : Validator
{
	private HashSet<DomNode> m_added;

	private HashSet<DomNode> m_removed;

	private Dictionary<DomNode, string> m_renamed;

	private bool m_naming;

	protected bool Naming => m_naming;

	protected override void OnNodeSet()
	{
		ValidateSubtree();
		base.OnNodeSet();
	}

	protected abstract void ValidateSubtree();

	protected virtual void OnIdCollision(DomNode node, string uniqueId)
	{
		throw new InvalidOperationException("2 nodes have the same id");
	}

	protected override void OnBeginning(object sender, EventArgs e)
	{
		m_added = new HashSet<DomNode>();
		m_removed = new HashSet<DomNode>();
		m_renamed = new Dictionary<DomNode, string>();
	}

	protected override void OnEnding(object sender, EventArgs e)
	{
		try
		{
			RemoveNodes(m_removed, m_renamed);
			m_naming = true;
			AddNodes(m_added, m_renamed);
			RenameNodes(m_renamed);
		}
		finally
		{
			m_naming = false;
			m_added = null;
			m_removed = null;
			m_renamed = null;
		}
	}

	protected override void OnCancelled(object sender, EventArgs e)
	{
		m_naming = false;
		m_added = null;
		m_removed = null;
		m_renamed = null;
		base.OnCancelled(sender, e);
	}

	protected abstract void RemoveNodes(HashSet<DomNode> removed, Dictionary<DomNode, string> renamed);

	protected abstract void AddNodes(HashSet<DomNode> added, Dictionary<DomNode, string> renamed);

	protected abstract void RenameNodes(Dictionary<DomNode, string> renamed);

	protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (base.Validating && !m_naming && e.AttributeInfo.Equivalent(e.DomNode.Type.IdAttribute) && !m_renamed.ContainsKey(e.DomNode))
		{
			m_renamed.Add(e.DomNode, e.OldValue.ToString());
		}
	}

	protected override void AddNode(DomNode node)
	{
		if (base.Validating && !m_naming && node.Type.IdAttribute != null && !m_removed.Remove(node))
		{
			m_added.Add(node);
		}
		base.AddNode(node);
	}

	protected override void RemoveNode(DomNode node)
	{
		if (base.Validating && !m_naming && node.Type.IdAttribute != null && !m_added.Remove(node))
		{
			m_removed.Add(node);
		}
		base.RemoveNode(node);
	}
}
