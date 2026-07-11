using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class TransactionReporter : Validator
{
	private enum EventType
	{
		AttributeChanged,
		ChildInserted,
		ChildRemoved,
		None
	}

	private class AttributeComparer : IEqualityComparer<Pair<DomNode, AttributeInfo>>
	{
		public bool Equals(Pair<DomNode, AttributeInfo> x, Pair<DomNode, AttributeInfo> y)
		{
			return x.First.Equals(y.First) && x.Second.Equivalent(y.Second);
		}

		public int GetHashCode(Pair<DomNode, AttributeInfo> obj)
		{
			return obj.First.GetHashCode() ^ obj.Second.GetEquivalentHashCode();
		}
	}

	private bool m_inTransaction;

	private readonly List<Pair<EventType, EventArgs>> m_events = new List<Pair<EventType, EventArgs>>();

	private readonly Dictionary<Pair<DomNode, AttributeInfo>, int> m_attributeChanges = new Dictionary<Pair<DomNode, AttributeInfo>, int>(s_comparer);

	private readonly Dictionary<DomNode, int> m_inserted = new Dictionary<DomNode, int>();

	private static readonly IEqualityComparer<Pair<DomNode, AttributeInfo>> s_comparer = new AttributeComparer();

	public event EventHandler<AttributeEventArgs> TransactionFinishedAttributeChanged;

	public event EventHandler<ChildEventArgs> TransactionFinishedChildInserted;

	public event EventHandler<ChildEventArgs> TransactionFinishedChildRemoved;

	protected virtual void OnTransactionFinishedAttributeChanged(AttributeEventArgs attributeEventArgs)
	{
		this.TransactionFinishedAttributeChanged.Raise(this, attributeEventArgs);
	}

	protected virtual void OnTransactionFinishedChildInserted(ChildEventArgs childEventArgs)
	{
		this.TransactionFinishedChildInserted.Raise(this, childEventArgs);
	}

	protected virtual void OnTransactionFinishedChildRemoved(ChildEventArgs childEventArgs)
	{
		this.TransactionFinishedChildRemoved.Raise(this, childEventArgs);
	}

	protected sealed override void OnAttributeChanged(object sender, AttributeEventArgs attributeEventArgs)
	{
		if (m_inTransaction)
		{
			foreach (DomNode item in attributeEventArgs.DomNode.Lineage)
			{
				if (m_inserted.ContainsKey(item))
				{
					return;
				}
			}
			Pair<DomNode, AttributeInfo> key = new Pair<DomNode, AttributeInfo>(attributeEventArgs.DomNode, attributeEventArgs.AttributeInfo);
			if (m_attributeChanges.TryGetValue(key, out var value))
			{
				AttributeEventArgs e = (AttributeEventArgs)m_events[value].Second;
				AttributeEventArgs second = new AttributeEventArgs(e.DomNode, e.AttributeInfo, e.OldValue, attributeEventArgs.NewValue);
				m_events[value] = new Pair<EventType, EventArgs>(EventType.AttributeChanged, second);
			}
			else
			{
				m_attributeChanges.Add(key, m_events.Count);
				m_events.Add(new Pair<EventType, EventArgs>(EventType.AttributeChanged, attributeEventArgs));
			}
		}
		else
		{
			OnTransactionFinishedAttributeChanged(attributeEventArgs);
		}
	}

	protected sealed override void OnChildInserted(object sender, ChildEventArgs childEventArgs)
	{
		if (m_inTransaction)
		{
			foreach (DomNode item in childEventArgs.Parent.Lineage)
			{
				if (m_inserted.ContainsKey(item))
				{
					return;
				}
			}
			m_inserted[childEventArgs.Child] = m_events.Count;
			m_events.Add(new Pair<EventType, EventArgs>(EventType.ChildInserted, childEventArgs));
		}
		else
		{
			OnTransactionFinishedChildInserted(childEventArgs);
		}
	}

	protected sealed override void OnChildRemoved(object sender, ChildEventArgs childEventArgs)
	{
		if (m_inTransaction)
		{
			List<Pair<DomNode, AttributeInfo>> list = new List<Pair<DomNode, AttributeInfo>>();
			foreach (DomNode item in childEventArgs.Child.Subtree)
			{
				foreach (KeyValuePair<Pair<DomNode, AttributeInfo>, int> attributeChange in m_attributeChanges)
				{
					if (attributeChange.Key.First == item)
					{
						list.Add(attributeChange.Key);
						m_events[attributeChange.Value] = new Pair<EventType, EventArgs>(EventType.None, null);
					}
				}
				if (list.Count <= 0)
				{
					continue;
				}
				foreach (Pair<DomNode, AttributeInfo> item2 in list)
				{
					m_attributeChanges.Remove(item2);
				}
				list.Clear();
			}
			if (m_inserted.TryGetValue(childEventArgs.Child, out var value))
			{
				m_events[value] = new Pair<EventType, EventArgs>(EventType.None, null);
				m_inserted.Remove(childEventArgs.Child);
				return;
			}
			foreach (DomNode item3 in childEventArgs.Parent.Lineage)
			{
				if (m_inserted.ContainsKey(item3))
				{
					return;
				}
			}
			m_events.Add(new Pair<EventType, EventArgs>(EventType.ChildRemoved, childEventArgs));
		}
		else
		{
			OnTransactionFinishedChildRemoved(childEventArgs);
		}
	}

	protected sealed override void OnBeginning(object sender, EventArgs e)
	{
		ClearRecording();
		m_inTransaction = true;
	}

	protected sealed override void OnCancelled(object sender, EventArgs e)
	{
		ClearRecording();
		m_inTransaction = false;
	}

	protected sealed override void OnEnded(object sender, EventArgs e)
	{
		m_inTransaction = false;
		foreach (Pair<EventType, EventArgs> @event in m_events)
		{
			switch (@event.First)
			{
			case EventType.AttributeChanged:
			{
				AttributeEventArgs e2 = (AttributeEventArgs)@event.Second;
				if (!e2.AttributeInfo.Type.AreEqual(e2.OldValue, e2.NewValue))
				{
					OnTransactionFinishedAttributeChanged(e2);
				}
				break;
			}
			case EventType.ChildInserted:
				OnTransactionFinishedChildInserted((ChildEventArgs)@event.Second);
				break;
			case EventType.ChildRemoved:
				OnTransactionFinishedChildRemoved((ChildEventArgs)@event.Second);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case EventType.None:
				break;
			}
		}
		ClearRecording();
	}

	private void ClearRecording()
	{
		m_events.Clear();
		m_attributeChanges.Clear();
		m_inserted.Clear();
	}
}
