using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class KeyboardIOGraphNavigator<TNode, TEdge, TEdgeRoute> : ControlAdapter where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	private ISelectionContext m_selectionContext;

	private IGraph<TNode, TEdge, TEdgeRoute> m_graph;

	private IPickingAdapter2 m_pickingAdapter;

	private ITransformAdapter m_transformAdapter;

	private Stack<List<TNode>> m_shiftKeySelectionStack = new Stack<List<TNode>>();

	private Keys m_lastKeyWithShift = Keys.None;

	private bool m_changingSelection;

	protected virtual bool IsInputNavigationKey(Keys key)
	{
		return key == Keys.Left;
	}

	protected virtual bool IsOutputNavigationKey(Keys key)
	{
		return key == Keys.Right;
	}

	protected virtual bool IsNavigationKey(Keys key)
	{
		return key == Keys.Up || key == Keys.Right || key == Keys.Down || key == Keys.Left;
	}

	protected virtual bool OppositeNavigationKeys(Keys a, Keys b)
	{
		return (IsOutputNavigationKey(a) && IsInputNavigationKey(b)) || (IsInputNavigationKey(a) && IsOutputNavigationKey(b)) || (a == Keys.Up && b == Keys.Down) || (a == Keys.Down && b == Keys.Up);
	}

	protected override void Bind(AdaptableControl control)
	{
		base.Bind(control);
		control.ContextChanged += control_ContextChanged;
		control.PreviewKeyDown += PreviewKeyDown;
		m_pickingAdapter = control.Cast<IPickingAdapter2>();
		m_transformAdapter = control.As<ITransformAdapter>();
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.ContextChanged -= control_ContextChanged;
		control.PreviewKeyDown -= PreviewKeyDown;
		m_pickingAdapter = null;
		m_transformAdapter = null;
		m_shiftKeySelectionStack.Clear();
		m_lastKeyWithShift = Keys.None;
		base.Unbind(control);
	}

	protected virtual TNode FindNearestNode(TNode startNode, Keys key, Keys modifiers)
	{
		TNode result = null;
		int num = int.MaxValue;
		Rectangle bounds = m_pickingAdapter.GetBounds(new TNode[1] { startNode });
		foreach (TNode node in m_graph.Nodes)
		{
			if (node != startNode)
			{
				Rectangle bounds2 = m_pickingAdapter.GetBounds(new TNode[1] { node });
				int num2 = WinFormsUtil.CalculateDistance(bounds, key, bounds2);
				if (num2 < num)
				{
					num = num2;
					result = node;
				}
			}
		}
		return result;
	}

	protected virtual IEnumerable<TNode> FindConnectedNodes(TNode startNode, Keys key, Keys modifiers)
	{
		if (IsOutputNavigationKey(key))
		{
			foreach (TNode outputNode in m_graph.GetOutputNodes(startNode))
			{
				yield return outputNode;
			}
		}
		else
		{
			if (!IsInputNavigationKey(key))
			{
				yield break;
			}
			foreach (TNode inputNode in m_graph.GetInputNodes(startNode))
			{
				yield return inputNode;
			}
		}
	}

	protected virtual void PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		Keys keyData = e.KeyData;
		Keys keys = keyData & Keys.Modifiers;
		keyData &= Keys.KeyCode;
		if (!IsNavigationKey(keyData))
		{
			return;
		}
		List<TNode> selection;
		if (keys == Keys.Shift && OppositeNavigationKeys(m_lastKeyWithShift, keyData) && m_shiftKeySelectionStack.Count > 0)
		{
			selection = m_shiftKeySelectionStack.Pop();
			ChangeSelection(selection);
			return;
		}
		selection = new List<TNode>(m_selectionContext.SelectionCount);
		selection.AddRange(m_selectionContext.GetSelection<TNode>());
		bool flag = false;
		if (keys == Keys.Shift)
		{
			if (m_lastKeyWithShift != Keys.None && m_lastKeyWithShift != keyData)
			{
				m_lastKeyWithShift = Keys.None;
				m_shiftKeySelectionStack.Clear();
			}
			flag = true;
		}
		else
		{
			m_lastKeyWithShift = Keys.None;
			m_shiftKeySelectionStack.Clear();
		}
		HashSet<TNode> hashSet = new HashSet<TNode>();
		foreach (TNode item in m_selectionContext.GetSelection<TNode>())
		{
			foreach (TNode item2 in FindConnectedNodes(item, keyData, keys))
			{
				hashSet.Add(item2);
			}
		}
		if (hashSet.Count == 0)
		{
			foreach (TNode item3 in m_selectionContext.GetSelection<TNode>())
			{
				TNode val = FindNearestNode(item3, keyData, keys);
				if (val != null)
				{
					hashSet.Add(val);
				}
			}
		}
		if (hashSet.Count <= 0)
		{
			return;
		}
		keys &= ~Keys.Control;
		HashSet<TNode> hashSet2 = new HashSet<TNode>(selection);
		KeysUtil.Select(hashSet2, hashSet, keys);
		if (!hashSet2.SetEquals(selection))
		{
			ChangeSelection(hashSet2);
			if (flag)
			{
				m_lastKeyWithShift = keyData;
				m_shiftKeySelectionStack.Push(new List<TNode>(selection));
			}
			if (m_transformAdapter != null)
			{
				Rectangle bounds = m_pickingAdapter.GetBounds(hashSet2.OfType<object>());
				m_transformAdapter.PanToRect(bounds);
			}
		}
	}

	private void ChangeSelection(IEnumerable<TNode> selection)
	{
		try
		{
			m_changingSelection = true;
			m_selectionContext.Selection = selection.Cast<object>();
		}
		finally
		{
			m_changingSelection = false;
		}
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanged -= m_selectionContext_SelectionChanged;
		}
		m_selectionContext = base.AdaptedControl.ContextCast<ISelectionContext>();
		m_selectionContext.SelectionChanged += m_selectionContext_SelectionChanged;
		m_graph = base.AdaptedControl.ContextCast<IGraph<TNode, TEdge, TEdgeRoute>>();
	}

	private void m_selectionContext_SelectionChanged(object sender, EventArgs args)
	{
		if (!m_changingSelection)
		{
			m_shiftKeySelectionStack.Clear();
			m_lastKeyWithShift = Keys.None;
		}
	}
}
