using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class LockingValidator : Validator
{
	private bool m_locked;

	private ILockingContext m_lockingContext;

	private HashSet<DomNode> m_modified;

	protected override void OnNodeSet()
	{
		m_lockingContext = this.Cast<ILockingContext>();
		base.DomNode.AttributeChanging += OnAttributeChanging;
		base.OnNodeSet();
	}

	protected override void OnBeginning(object sender, EventArgs e)
	{
		m_modified = new HashSet<DomNode>();
	}

	protected override void OnEnding(object sender, EventArgs e)
	{
		try
		{
			HashSet<DomNode> hashSet = new HashSet<DomNode>();
			List<DomNode> list = new List<DomNode>();
			foreach (DomNode item in m_modified)
			{
				foreach (DomNode item2 in item.Lineage)
				{
					if (hashSet.Contains(item2))
					{
						break;
					}
					if (m_lockingContext.IsLocked(item2))
					{
						throw new InvalidTransactionException("item is locked");
					}
					list.Add(item2);
				}
				foreach (DomNode item3 in list)
				{
					hashSet.Add(item3);
				}
				list.Clear();
			}
		}
		finally
		{
			m_modified = null;
		}
	}

	protected override void OnCancelled(object sender, EventArgs e)
	{
		m_modified = null;
	}

	protected virtual void OnAttributeChanging(object sender, AttributeEventArgs e)
	{
		if (base.Validating)
		{
			m_locked = m_lockingContext.IsLocked(e.DomNode);
		}
	}

	protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (base.Validating)
		{
			bool flag = m_lockingContext.IsLocked(e.DomNode);
			if (m_locked == flag)
			{
				m_modified.Add(e.DomNode);
			}
		}
	}

	protected override void OnChildInserted(object sender, ChildEventArgs e)
	{
		if (base.Validating)
		{
			m_modified.Add(e.Child);
		}
	}

	protected override void OnChildRemoved(object sender, ChildEventArgs e)
	{
		if (base.Validating)
		{
			m_modified.Add(e.Child);
		}
	}
}
