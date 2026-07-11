using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class GraphNodeEditAdapter<TNode, TEdge, TEdgeRoute> : DraggingControlAdapter, IItemDragAdapter where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	private readonly GraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;

	private readonly IGraphAdapter<TNode, TEdge, TEdgeRoute> m_graphAdapter;

	private readonly ITransformAdapter m_transformAdapter;

	private IAutoTranslateAdapter m_autoTranslateAdapter;

	private IGraph<TNode, TEdge, TEdgeRoute> m_graph;

	private ILayoutContext m_layoutContext;

	private ISelectionContext m_selectionContext;

	private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_mousePick = new GraphHitRecord<TNode, TEdge, TEdgeRoute>();

	private TNode m_hotNode;

	private TNode[] m_draggingNodes;

	private Point[] m_newPositions;

	private Point[] m_oldPositions;

	private TEdge[] m_draggingEdges;

	private Point m_firstPoint;

	private bool m_hotTrack = true;

	private bool m_isDragging;

	private bool m_initiated;

	public bool HotTrack
	{
		get
		{
			return m_hotTrack;
		}
		set
		{
			m_hotTrack = value;
			ResetHotNode();
		}
	}

	public GraphNodeEditAdapter(GraphRenderer<TNode, TEdge, TEdgeRoute> renderer, IGraphAdapter<TNode, TEdge, TEdgeRoute> graphAdapter, ITransformAdapter transformAdapter)
	{
		m_renderer = renderer;
		m_graphAdapter = graphAdapter;
		m_transformAdapter = transformAdapter;
	}

	void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
	{
		m_isDragging = true;
		ActiveCollection<TNode> activeCollection = new ActiveCollection<TNode>();
		List<TEdge> list = new List<TEdge>();
		HashSet<TNode> hashSet = new HashSet<TNode>();
		foreach (TNode item in m_selectionContext.GetSelection<TNode>())
		{
			AddDragNode(item, activeCollection, hashSet);
		}
		foreach (TEdge edge in m_graph.Edges)
		{
			if (hashSet.Contains(edge.FromNode) || hashSet.Contains(edge.ToNode))
			{
				list.Add(edge);
				m_graphAdapter.SetStyle(edge, DiagramDrawingStyle.Ghosted);
			}
		}
		m_draggingNodes = activeCollection.GetSnapshot<TNode>();
		m_newPositions = new Point[m_draggingNodes.Length];
		m_oldPositions = new Point[m_draggingNodes.Length];
		for (int i = 0; i < m_draggingNodes.Length; i++)
		{
			Point location = m_draggingNodes[i].Bounds.Location;
			m_newPositions[i] = location;
			m_oldPositions[i] = location;
		}
		m_draggingEdges = list.ToArray();
	}

	void IItemDragAdapter.EndingDrag()
	{
	}

	void IItemDragAdapter.EndDrag()
	{
		TNode[] draggingNodes = m_draggingNodes;
		foreach (TNode item in draggingNodes)
		{
			m_graphAdapter.ResetStyle(item);
		}
		TEdge[] draggingEdges = m_draggingEdges;
		foreach (TEdge item2 in draggingEdges)
		{
			m_graphAdapter.ResetStyle(item2);
		}
		int num = 0;
		TNode[] draggingNodes2 = m_draggingNodes;
		foreach (TNode node in draggingNodes2)
		{
			MoveNode(node, m_newPositions[num]);
			num++;
		}
		m_draggingNodes = null;
		m_newPositions = null;
		m_oldPositions = null;
		m_draggingEdges = null;
		m_isDragging = false;
	}

	protected override void Bind(AdaptableControl control)
	{
		m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();
		control.ContextChanged += control_ContextChanged;
		control.Paint += control_Paint;
		base.Bind(control);
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.ContextChanged -= control_ContextChanged;
		control.Paint -= control_Paint;
		base.Unbind(control);
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		m_graph = base.AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
		m_layoutContext = base.AdaptedControl.ContextAs<ILayoutContext>();
		if (m_layoutContext != null)
		{
			m_selectionContext = base.AdaptedControl.ContextCast<ISelectionContext>();
		}
	}

	private void control_Paint(object sender, PaintEventArgs e)
	{
		if (m_draggingNodes != null)
		{
			Graphics graphics = e.Graphics;
			Matrix transform = m_transformAdapter.Transform;
			Matrix transform2 = graphics.Transform;
			Region clip = graphics.Clip;
			graphics.Transform = transform;
			Rectangle clip2 = GdiUtil.InverseTransform(graphics.Transform, e.ClipRectangle);
			graphics.SetClip(clip2);
			Point point = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.CurrentPoint);
			Point point2 = new Point(point.X - m_firstPoint.X, point.Y - m_firstPoint.Y);
			for (int i = 0; i < m_draggingNodes.Length; i++)
			{
				TNode item = m_draggingNodes[i];
				m_layoutContext.GetBounds(item, out var bounds);
				bounds.X += point2.X;
				bounds.Y += point2.Y;
				m_newPositions[i] = bounds.Location;
				m_layoutContext.SetBounds(item, bounds, BoundsSpecified.Location);
			}
			TNode[] draggingNodes = m_draggingNodes;
			foreach (TNode node in draggingNodes)
			{
				m_renderer.Draw(node, DiagramDrawingStyle.Normal, graphics);
			}
			TEdge[] draggingEdges = m_draggingEdges;
			foreach (TEdge edge in draggingEdges)
			{
				m_renderer.Draw(edge, DiagramDrawingStyle.Normal, graphics);
			}
			for (int l = 0; l < m_draggingNodes.Length; l++)
			{
				TNode node2 = m_draggingNodes[l];
				MoveNode(node2, m_oldPositions[l]);
			}
			graphics.Transform = transform2;
			graphics.Clip = clip;
		}
	}

	protected override void OnMouseMove(object sender, MouseEventArgs e)
	{
		base.OnMouseMove(sender, e);
		if (e.Button == MouseButtons.None && base.AdaptedControl.Focused && m_hotTrack)
		{
			m_mousePick = m_graphAdapter.Pick(base.CurrentPoint);
			TNode val = null;
			if (m_mousePick.Node != null && m_mousePick.Edge == null && m_mousePick.FromRoute == null && m_mousePick.ToRoute == null)
			{
				val = m_mousePick.Node;
			}
			if (val != m_hotNode)
			{
				ResetHotNode();
				SetHotNode(val);
			}
		}
	}

	protected override void OnMouseLeave(object sender, EventArgs e)
	{
		base.OnMouseLeave(sender, e);
		ResetHotNode();
	}

	private void SetHotNode(TNode hotNode)
	{
		m_hotNode = hotNode;
		if (hotNode != null)
		{
			m_graphAdapter.SetStyle(m_hotNode, DiagramDrawingStyle.Hot);
		}
		if (base.AdaptedControl.Focused)
		{
			base.AdaptedControl.Invalidate();
		}
	}

	private void ResetHotNode()
	{
		if (m_hotNode != null)
		{
			m_graphAdapter.ResetStyle(m_hotNode);
			m_hotNode = null;
			if (base.AdaptedControl.Focused)
			{
				base.AdaptedControl.Invalidate();
			}
		}
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		if (m_layoutContext != null && !m_isDragging)
		{
			ResetHotNode();
			if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Alt) == 0 && !base.AdaptedControl.Capture)
			{
				m_firstPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.FirstPoint);
				m_mousePick = m_graphAdapter.Pick(base.FirstPoint);
				if (m_mousePick.Node != null)
				{
					m_initiated = true;
					foreach (IItemDragAdapter item in base.AdaptedControl.AsAll<IItemDragAdapter>())
					{
						item.BeginDrag(this);
					}
					base.AdaptedControl.Capture = true;
					if (m_autoTranslateAdapter != null)
					{
						m_autoTranslateAdapter.Enabled = true;
					}
				}
			}
		}
		if (m_draggingNodes != null)
		{
			base.AdaptedControl.Invalidate();
		}
	}

	private void AddDragNode(TNode node, ActiveCollection<TNode> draggingNodes, HashSet<TNode> nodes)
	{
		draggingNodes.Add(node);
		if (!nodes.Contains(node))
		{
			nodes.Add(node);
			m_graphAdapter.SetStyle(node, DiagramDrawingStyle.Ghosted);
		}
		IHierarchicalGraphNode<TNode, TEdge, TEdgeRoute> hierarchicalGraphNode = node.As<IHierarchicalGraphNode<TNode, TEdge, TEdgeRoute>>();
		if (hierarchicalGraphNode == null)
		{
			return;
		}
		foreach (TNode subNode in hierarchicalGraphNode.SubNodes)
		{
			AddDragNode(subNode, draggingNodes, nodes);
		}
	}

	protected override void OnMouseUp(object sender, MouseEventArgs e)
	{
		base.OnMouseUp(sender, e);
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		if (m_draggingNodes != null)
		{
			if (m_initiated)
			{
				ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
				context.DoTransaction(delegate
				{
					foreach (IItemDragAdapter item in base.AdaptedControl.AsAll<IItemDragAdapter>())
					{
						item.EndDrag();
					}
				}, "Drag Items".Localize());
				m_initiated = false;
			}
			base.AdaptedControl.Invalidate();
		}
		if (m_autoTranslateAdapter != null)
		{
			m_autoTranslateAdapter.Enabled = false;
		}
	}

	private void MoveNode(TNode node, Point location)
	{
		Rectangle bounds = new Rectangle(location.X, location.Y, 0, 0);
		m_layoutContext.SetBounds(node, bounds, BoundsSpecified.Location);
	}
}
