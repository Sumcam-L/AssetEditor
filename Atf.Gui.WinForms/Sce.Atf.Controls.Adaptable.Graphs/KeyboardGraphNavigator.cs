using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class KeyboardGraphNavigator<TNode, TEdge, TEdgeRoute> : ControlAdapter where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	private ISelectionContext m_selectionContext;

	private IGraph<TNode, TEdge, TEdgeRoute> m_graph;

	protected override void Bind(AdaptableControl control)
	{
		control.ContextChanged += control_ContextChanged;
		control.PreviewKeyDown += control_PreviewKeyDown;
		base.Bind(control);
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.ContextChanged -= control_ContextChanged;
		control.PreviewKeyDown -= control_PreviewKeyDown;
		base.Unbind(control);
	}

	protected virtual TNode FindNearestElement(TNode startNode, Keys arrow, out Rectangle nearestRect)
	{
		TNode result = null;
		int num = int.MaxValue;
		IPickingAdapter2 pickingAdapter = base.AdaptedControl.Cast<IPickingAdapter2>();
		nearestRect = default(Rectangle);
		Rectangle bounds = pickingAdapter.GetBounds(new TNode[1] { startNode });
		foreach (TNode node in m_graph.Nodes)
		{
			if (node != startNode)
			{
				Rectangle bounds2 = pickingAdapter.GetBounds(new TNode[1] { node });
				int num2 = WinFormsUtil.CalculateDistance(bounds, arrow, bounds2);
				if (num2 < num)
				{
					num = num2;
					result = node;
					nearestRect = bounds2;
				}
			}
		}
		return result;
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		m_selectionContext = base.AdaptedControl.ContextCast<ISelectionContext>();
		m_graph = base.AdaptedControl.ContextCast<IGraph<TNode, TEdge, TEdgeRoute>>();
	}

	private void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		Keys keyData = e.KeyData;
		Keys modifiers = keyData & Keys.Modifiers;
		keyData &= Keys.KeyCode;
		if (keyData != Keys.Up && keyData != Keys.Right && keyData != Keys.Down && keyData != Keys.Left)
		{
			return;
		}
		TNode val = m_selectionContext.LastSelected.As<TNode>();
		if (val != null)
		{
			Rectangle nearestRect;
			TNode val2 = FindNearestElement(val, keyData, out nearestRect);
			if (val2 != null)
			{
				List<TNode> list = new List<TNode>(m_selectionContext.SelectionCount);
				list.AddRange(m_selectionContext.GetSelection<TNode>());
				KeysUtil.Select(list, val2, modifiers);
				m_selectionContext.Selection = list.Cast<object>();
				base.AdaptedControl.As<ITransformAdapter>()?.PanToRect(nearestRect);
			}
		}
	}
}
