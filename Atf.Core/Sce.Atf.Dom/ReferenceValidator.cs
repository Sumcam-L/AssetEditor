using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Dom;

public class ReferenceValidator : Validator
{
	private readonly Dictionary<DomNode, List<Pair<DomNode, AttributeInfo>>> m_nodeReferenceLists = new Dictionary<DomNode, List<Pair<DomNode, AttributeInfo>>>();

	private HashSet<DomNode> m_removed;

	public bool Suspended { get; set; }

	public event EventHandler<ReferenceEventArgs> ReferentRemoved;

	public event EventHandler<ReferenceEventArgs> ExternalReferenceAdded;

	public event EventHandler<ReferenceEventArgs> ExternalReferenceRemoved;

	public IEnumerable<Pair<DomNode, AttributeInfo>> GetReferences(DomNode target)
	{
		if (m_nodeReferenceLists.TryGetValue(target, out var value))
		{
			return value;
		}
		return EmptyEnumerable<Pair<DomNode, AttributeInfo>>.Instance;
	}

	protected virtual void OnReferentRemoved(ReferenceEventArgs e)
	{
		DomNode parent = e.Owner.Parent;
		if (parent != null)
		{
			e.Owner.RemoveFromParent();
		}
	}

	protected virtual void OnExternalReferenceAdded(ReferenceEventArgs e)
	{
	}

	protected virtual void OnExternalReferenceRemoved(ReferenceEventArgs e)
	{
		this.ExternalReferenceRemoved.Raise(this, e);
	}

	protected override void OnBeginning(object sender, EventArgs e)
	{
		m_removed = new HashSet<DomNode>();
	}

	protected override void OnEnding(object sender, EventArgs e)
	{
		if (Suspended)
		{
			return;
		}
		while (m_removed.Count > 0)
		{
			DomNode[] array = m_removed.ToArray();
			m_removed.Clear();
			DomNode[] array2 = array;
			foreach (DomNode domNode in array2)
			{
				if (m_nodeReferenceLists.TryGetValue(domNode, out var value))
				{
					Pair<DomNode, AttributeInfo>[] array3 = value.ToArray();
					for (int j = 0; j < array3.Length; j++)
					{
						Pair<DomNode, AttributeInfo> pair = array3[j];
						ReferenceEventArgs e2 = new ReferenceEventArgs(pair.First, pair.Second, domNode);
						this.ReferentRemoved.Raise(this, e2);
						OnReferentRemoved(e2);
					}
					m_nodeReferenceLists.Remove(domNode);
				}
			}
		}
	}

	protected override void OnEnded(object sender, EventArgs e)
	{
		m_removed = null;
	}

	protected override void OnCancelled(object sender, EventArgs e)
	{
		m_removed = null;
	}

	protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (base.Validating && e.AttributeInfo.Type.Type == AttributeTypes.Reference)
		{
			if (e.OldValue is DomNode target)
			{
				RemoveReference(e.DomNode, e.AttributeInfo, target);
			}
			if (e.NewValue is DomNode target2)
			{
				AddReference(e.DomNode, e.AttributeInfo, target2);
			}
		}
	}

	protected override void AddNode(DomNode node)
	{
		foreach (AttributeInfo attribute in node.Type.Attributes)
		{
			if (attribute.Type.Type == AttributeTypes.Reference && node.GetAttribute(attribute) is DomNode target)
			{
				AddReference(node, attribute, target);
			}
		}
		if (base.Validating)
		{
			m_removed.Remove(node);
		}
		base.AddNode(node);
	}

	protected override void RemoveNode(DomNode node)
	{
		foreach (AttributeInfo attribute in node.Type.Attributes)
		{
			if (attribute.Type.Type == AttributeTypes.Reference && node.GetAttribute(attribute) is DomNode target)
			{
				RemoveReference(node, attribute, target);
			}
		}
		if (base.Validating)
		{
			m_removed.Add(node);
		}
		base.RemoveNode(node);
	}

	private void AddReference(DomNode owner, AttributeInfo attributeInfo, DomNode target)
	{
		if (!m_nodeReferenceLists.TryGetValue(target, out var value))
		{
			value = new List<Pair<DomNode, AttributeInfo>>();
			m_nodeReferenceLists.Add(target, value);
		}
		value.Add(new Pair<DomNode, AttributeInfo>(owner, attributeInfo));
		DomNode root = target.GetRoot();
		if (base.DomNode != root)
		{
			ReferenceEventArgs e = new ReferenceEventArgs(owner, attributeInfo, target);
			OnExternalReferenceAdded(e);
			this.ExternalReferenceAdded.Raise(this, e);
		}
	}

	private void RemoveReference(DomNode owner, AttributeInfo attributeInfo, DomNode target)
	{
		if (m_nodeReferenceLists.TryGetValue(target, out var value))
		{
			value.Remove(new Pair<DomNode, AttributeInfo>(owner, attributeInfo));
			if (value.Count == 0)
			{
				m_nodeReferenceLists.Remove(target);
			}
		}
		DomNode root = target.GetRoot();
		if (base.DomNode != root)
		{
			ReferenceEventArgs e = new ReferenceEventArgs(owner, attributeInfo, target);
			OnExternalReferenceRemoved(e);
			this.ExternalReferenceRemoved.Raise(this, e);
		}
	}
}
