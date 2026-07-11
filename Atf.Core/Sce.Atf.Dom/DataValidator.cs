using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class DataValidator : Validator
{
	private HashSet<Pair<Pair<DomNode, DomNode>, ChildInfo>> m_childChanges = new HashSet<Pair<Pair<DomNode, DomNode>, ChildInfo>>();

	private Dictionary<AttributeInfo, object> m_attributeChanges = new Dictionary<AttributeInfo, object>();

	protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (base.Validating)
		{
			m_attributeChanges[e.AttributeInfo] = e.NewValue;
		}
	}

	protected override void OnChildInserted(object sender, ChildEventArgs e)
	{
		if (base.Validating)
		{
			m_childChanges.Add(new Pair<Pair<DomNode, DomNode>, ChildInfo>(new Pair<DomNode, DomNode>(e.Parent, e.Child), e.ChildInfo));
		}
	}

	protected override void OnChildRemoved(object sender, ChildEventArgs e)
	{
		if (base.Validating)
		{
			m_childChanges.Add(new Pair<Pair<DomNode, DomNode>, ChildInfo>(new Pair<DomNode, DomNode>(e.Parent, e.Child), e.ChildInfo));
		}
	}

	protected override void OnEnded(object sender, EventArgs e)
	{
		foreach (KeyValuePair<AttributeInfo, object> attributeChange in m_attributeChanges)
		{
			AttributeInfo key = attributeChange.Key;
			object value = attributeChange.Value;
			if (!key.Validate(value))
			{
				throw new InvalidTransactionException("invalid attribute value");
			}
		}
		m_attributeChanges.Clear();
		foreach (Pair<Pair<DomNode, DomNode>, ChildInfo> childChange in m_childChanges)
		{
			DomNode first = childChange.First.First;
			DomNode second = childChange.First.Second;
			ChildInfo second2 = childChange.Second;
			if (!second2.Validate(first, second))
			{
				throw new InvalidTransactionException("invalid child removal or insertion");
			}
		}
		m_childChanges.Clear();
	}

	protected override void OnCancelled(object sender, EventArgs e)
	{
		m_childChanges.Clear();
		m_attributeChanges.Clear();
	}
}
