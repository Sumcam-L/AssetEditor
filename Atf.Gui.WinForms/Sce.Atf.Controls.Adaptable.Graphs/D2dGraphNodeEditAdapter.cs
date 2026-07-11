using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class D2dGraphNodeEditAdapter<TNode, TEdge, TEdgeRoute> : DraggingControlAdapter, IItemDragAdapter where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	private readonly D2dGraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;

	private readonly D2dGraphAdapter<TNode, TEdge, TEdgeRoute> m_graphAdapter;

	private readonly ITransformAdapter m_transformAdapter;

	private IAutoTranslateAdapter m_autoTranslateAdapter;

	private ISelectionPathProvider m_selectionPathProvider;

	private IGraph<TNode, TEdge, TEdgeRoute> m_graph;

	private IEditableGraphContainer<TNode, TEdge, TEdgeRoute> m_editableGraphContainer;

	private ILayoutContext m_layoutContext;

	private ISelectionContext m_selectionContext;

	private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_mousePick = new GraphHitRecord<TNode, TEdge, TEdgeRoute>();

	private TNode[] m_draggingNodes;

	private Point[] m_newPositions;

	private Point[] m_oldPositions;

	private Point m_firstPoint;

	private bool m_draggingSubNodes = true;

	private object m_targetItem;

	private bool m_movingNodesCrossContainer;

	private bool m_resizing;

	private RectangleF m_firstBound;

	private bool m_constrainXDetermined;

	private bool m_constrainX;

	public bool DraggingSubNodes
	{
		get
		{
			return m_draggingSubNodes;
		}
		set
		{
			m_draggingSubNodes = value;
		}
	}

	public D2dGraphNodeEditAdapter(D2dGraphRenderer<TNode, TEdge, TEdgeRoute> renderer, D2dGraphAdapter<TNode, TEdge, TEdgeRoute> graphAdapter, ITransformAdapter transformAdapter)
	{
		m_renderer = renderer;
		m_graphAdapter = graphAdapter;
		m_transformAdapter = transformAdapter;
	}

	public Point? NodeDraggingPosition(TNode node)
	{
		if (m_draggingNodes != null)
		{
			for (int i = 0; i < m_draggingNodes.Length; i++)
			{
				TNode val = m_draggingNodes[i];
				if (val == node)
				{
					return m_newPositions[i];
				}
			}
		}
		return null;
	}

	protected override void Bind(AdaptableControl control)
	{
		m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();
		m_selectionPathProvider = control.As<ISelectionPathProvider>();
		control.ContextChanged += control_ContextChanged;
		control.MouseMove += control_MouseMove;
		control.MouseUp += control_MouseUp;
		base.Bind(control);
	}

	protected override void Unbind(AdaptableControl control)
	{
		base.Unbind(control);
		m_autoTranslateAdapter = null;
		m_selectionPathProvider = null;
		control.ContextChanged -= control_ContextChanged;
		control.MouseMove -= control_MouseMove;
		control.MouseUp -= control_MouseUp;
		m_graph = null;
		m_layoutContext = null;
		m_selectionContext = null;
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		m_graph = base.AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
		m_layoutContext = base.AdaptedControl.ContextAs<ILayoutContext>();
		m_editableGraphContainer = base.AdaptedControl.ContextAs<IEditableGraphContainer<TNode, TEdge, TEdgeRoute>>();
		if (m_layoutContext != null)
		{
			m_selectionContext = base.AdaptedControl.ContextCast<ISelectionContext>();
		}
	}

	private void control_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.None || !base.AdaptedControl.Focused)
		{
			return;
		}
		GraphHitRecord<TNode, TEdge, TEdgeRoute> graphHitRecord = m_graphAdapter.Pick(e.Location);
		if (graphHitRecord.Part.Is<DiagramBorder>())
		{
			DiagramBorder diagramBorder = graphHitRecord.Part.Cast<DiagramBorder>();
			if (m_editableGraphContainer != null && m_editableGraphContainer.CanResize(graphHitRecord.Item, diagramBorder))
			{
				base.AdaptedControl.AutoResetCursor = false;
				if (diagramBorder.Border == DiagramBorder.BorderType.Right)
				{
					base.AdaptedControl.Cursor = Cursors.SizeWE;
				}
				else if (diagramBorder.Border == DiagramBorder.BorderType.Bottom)
				{
					base.AdaptedControl.Cursor = Cursors.SizeNS;
				}
			}
		}
		else if (graphHitRecord.SubPart.Is<DiagramBorder>())
		{
			DiagramBorder diagramBorder2 = graphHitRecord.SubPart.Cast<DiagramBorder>();
			if (m_editableGraphContainer != null && m_editableGraphContainer.CanResize(graphHitRecord.SubItem, diagramBorder2))
			{
				base.AdaptedControl.AutoResetCursor = false;
				if (diagramBorder2.Border == DiagramBorder.BorderType.Right)
				{
					base.AdaptedControl.Cursor = Cursors.SizeWE;
				}
				else if (diagramBorder2.Border == DiagramBorder.BorderType.Bottom)
				{
					base.AdaptedControl.Cursor = Cursors.SizeNS;
				}
			}
		}
		else
		{
			base.AdaptedControl.AutoResetCursor = true;
		}
	}

	private void control_MouseUp(object sender, MouseEventArgs e)
	{
		m_constrainXDetermined = false;
	}

	private bool AllowDragging(TNode node)
	{
		if (m_layoutContext != null)
		{
			BoundsSpecified boundsSpecified = m_layoutContext.CanSetBounds(node);
			if (boundsSpecified == BoundsSpecified.None)
			{
				return false;
			}
			bool flag = (boundsSpecified & BoundsSpecified.X) == 0;
			bool flag2 = (boundsSpecified & BoundsSpecified.Y) == 0;
			bool flag3 = (boundsSpecified & BoundsSpecified.Width) == 0;
			bool flag4 = (boundsSpecified & BoundsSpecified.Height) == 0;
			if (flag && flag2 && flag3 && flag4)
			{
				return false;
			}
		}
		return !m_selectionPathProvider.Ancestry(node).Any((object x) => x.Is<IReference<Group>>());
	}

	private bool CanDragging()
	{
		if (m_mousePick.Node != null || m_mousePick.Item != null)
		{
			if (m_mousePick.Node.Is<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>())
			{
				ICircuitGroupType<TNode, TEdge, TEdgeRoute> circuitGroupType = m_mousePick.Node.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
				if (circuitGroupType.Expanded)
				{
					if (m_mousePick.Part.Is<DiagramBorder>() || m_mousePick.Part.Is<DiagramTitleBar>())
					{
						return true;
					}
					if (m_mousePick.SubItem == null)
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	protected override void OnBeginDrag(MouseEventArgs e)
	{
		base.OnBeginDrag(e);
		if (m_layoutContext == null || e.Button != MouseButtons.Left || (Control.ModifierKeys & Keys.Alt) != Keys.None || base.AdaptedControl.Capture)
		{
			return;
		}
		m_firstPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.FirstPoint);
		m_mousePick = m_graphAdapter.Pick(base.FirstPoint);
		if (!CanDragging())
		{
			return;
		}
		foreach (IItemDragAdapter item in base.AdaptedControl.AsAll<IItemDragAdapter>())
		{
			if (item != this)
			{
				item.BeginDrag(this);
			}
		}
		ActiveCollection<TNode> activeCollection = new ActiveCollection<TNode>();
		HashSet<TNode> nodes = new HashSet<TNode>();
		foreach (TNode item2 in m_selectionContext.GetSelection<TNode>())
		{
			if (AllowDragging(item2))
			{
				AddDragNode(item2, activeCollection, nodes);
			}
		}
		m_draggingNodes = activeCollection.GetSnapshot<TNode>();
		if (m_draggingNodes.Length == 1 && (m_draggingNodes[0] == m_mousePick.Item || m_draggingNodes[0] == m_mousePick.SubItem))
		{
			if (m_mousePick.SubItem != null)
			{
				if (m_mousePick.SubItem.Is<TNode>())
				{
					m_draggingNodes[0] = m_mousePick.SubItem.Cast<TNode>();
				}
				if (m_mousePick.SubPart.Is<DiagramBorder>())
				{
					DiagramBorder borderPart = m_mousePick.SubPart.Cast<DiagramBorder>();
					if (m_editableGraphContainer != null && m_editableGraphContainer.CanResize(m_mousePick.SubItem, borderPart))
					{
						m_firstBound = m_graphAdapter.GetLocalBound(m_mousePick.SubItem.As<TNode>());
						m_resizing = true;
						m_targetItem = m_mousePick.SubItem;
					}
				}
			}
			else if (m_mousePick.Part.Is<DiagramBorder>())
			{
				DiagramBorder borderPart2 = m_mousePick.Part.Cast<DiagramBorder>();
				if (m_editableGraphContainer != null && m_editableGraphContainer.CanResize(m_mousePick.Item, borderPart2))
				{
					m_firstBound = m_graphAdapter.GetLocalBound(m_mousePick.Item.As<TNode>());
					m_resizing = true;
					m_targetItem = m_mousePick.Item;
				}
			}
			else if (m_mousePick.SubPart.Is<DiagramBorder>())
			{
				DiagramBorder borderPart3 = m_mousePick.SubPart.Cast<DiagramBorder>();
				if (m_editableGraphContainer != null && m_editableGraphContainer.CanResize(m_mousePick.SubItem, borderPart3))
				{
					m_firstBound = m_graphAdapter.GetLocalBound(m_mousePick.SubItem.As<TNode>());
					m_resizing = true;
					m_targetItem = m_mousePick.SubItem;
				}
			}
		}
		m_newPositions = new Point[m_draggingNodes.Length];
		m_oldPositions = new Point[m_draggingNodes.Length];
		for (int i = 0; i < m_draggingNodes.Length; i++)
		{
			Point location = m_draggingNodes[i].Bounds.Location;
			m_newPositions[i] = location;
			m_oldPositions[i] = location;
		}
		base.AdaptedControl.Capture = true;
		if (m_autoTranslateAdapter != null)
		{
			m_autoTranslateAdapter.Enabled = true;
		}
	}

	private void ResizingNode()
	{
		Point point = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.CurrentPoint);
		Point point2 = new Point(point.X - m_firstPoint.X, point.Y - m_firstPoint.Y);
		m_editableGraphContainer.Resize(m_targetItem, (int)m_firstBound.Width + point2.X, (int)m_firstBound.Height + point2.Y);
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		if (m_resizing)
		{
			ResizingNode();
			return;
		}
		m_movingNodesCrossContainer = false;
		if (m_draggingNodes == null)
		{
			return;
		}
		TNode[] draggingNodes = m_draggingNodes;
		foreach (TNode node in draggingNodes)
		{
			m_renderer.SetCustomStyle(node, DiagramDrawingStyle.DragSource);
		}
		if (m_editableGraphContainer != null)
		{
			IEnumerable<TNode> nodes = m_graph.Nodes.Except(m_draggingNodes);
			GraphHitRecord<TNode, TEdge, TEdgeRoute> graphHitRecord = m_graphAdapter.Pick(nodes, EmptyEnumerable<TEdge>.Instance, e.Location);
			if (graphHitRecord.Item != null)
			{
				object obj = ChooseActiveTarget(graphHitRecord);
				if (m_targetItem != obj)
				{
					ResetCustomStyle(m_targetItem);
					m_targetItem = obj;
				}
			}
			else
			{
				ResetCustomStyle(m_targetItem);
				m_targetItem = null;
			}
		}
		D2dAdaptableControl d2dAdaptableControl = base.AdaptedControl as D2dAdaptableControl;
		Point point = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.CurrentPoint);
		Point point2 = new Point(point.X - m_firstPoint.X, point.Y - m_firstPoint.Y);
		if (Control.ModifierKeys == Keys.Shift)
		{
			if (!m_constrainXDetermined)
			{
				int num = Math.Abs(point2.X);
				int num2 = Math.Abs(point2.Y);
				Size dragSize = SystemInformation.DragSize;
				if (num > dragSize.Width || num2 > dragSize.Height)
				{
					m_constrainX = num < num2;
					m_constrainXDetermined = true;
				}
			}
			if (m_constrainXDetermined)
			{
				if (m_constrainX)
				{
					point2.X = 0;
				}
				else
				{
					point2.Y = 0;
				}
			}
		}
		else
		{
			m_constrainXDetermined = false;
		}
		for (int j = 0; j < m_draggingNodes.Length; j++)
		{
			TNode item = m_draggingNodes[j];
			BoundsSpecified boundsSpecified = m_layoutContext.CanSetBounds(item);
			if ((boundsSpecified & BoundsSpecified.X) != BoundsSpecified.None && (boundsSpecified & BoundsSpecified.Y) != BoundsSpecified.None)
			{
				m_layoutContext.GetBounds(item, out var bounds);
				bounds.X = m_oldPositions[j].X + point2.X;
				bounds.Y = m_oldPositions[j].Y + point2.Y;
				m_newPositions[j] = bounds.Location;
				m_layoutContext.SetBounds(item, bounds, BoundsSpecified.Location);
			}
		}
		if (m_editableGraphContainer != null && m_editableGraphContainer.CanMove(m_targetItem, m_draggingNodes))
		{
			m_renderer.SetCustomStyle(m_targetItem, DiagramDrawingStyle.DropTarget);
			m_movingNodesCrossContainer = true;
		}
		d2dAdaptableControl.DrawD2d();
	}

	protected override void OnEndDrag(MouseEventArgs e)
	{
		base.OnEndDrag(e);
		if (m_draggingNodes != null && m_draggingNodes.Length != 0)
		{
			TNode[] draggingNodes = m_draggingNodes;
			foreach (TNode item in draggingNodes)
			{
				ResetCustomStyle(item);
			}
			foreach (IItemDragAdapter item2 in base.AdaptedControl.AsAll<IItemDragAdapter>())
			{
				item2.EndingDrag();
			}
			ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
			context.DoTransaction(delegate
			{
				foreach (IItemDragAdapter item3 in base.AdaptedControl.AsAll<IItemDragAdapter>())
				{
					item3.EndDrag();
				}
			}, "Drag Items".Localize());
			if (m_autoTranslateAdapter != null)
			{
				m_autoTranslateAdapter.Enabled = false;
			}
			base.AdaptedControl.Invalidate();
		}
		m_draggingNodes = null;
		m_newPositions = null;
		m_oldPositions = null;
		ResetCustomStyle(m_targetItem);
		m_targetItem = null;
		m_resizing = false;
		base.AdaptedControl.Cursor = Cursors.Default;
	}

	void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
	{
		ActiveCollection<TNode> activeCollection = new ActiveCollection<TNode>();
		HashSet<TNode> nodes = new HashSet<TNode>();
		foreach (TNode item in m_selectionContext.GetSelection<TNode>())
		{
			AddDragNode(item, activeCollection, nodes);
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
	}

	void IItemDragAdapter.EndingDrag()
	{
		if (m_resizing)
		{
			m_editableGraphContainer.Resize(m_targetItem, (int)m_firstBound.Width, (int)m_firstBound.Height);
		}
		else if (m_draggingNodes != null)
		{
			for (int i = 0; i < m_draggingNodes.Length; i++)
			{
				TNode node = m_draggingNodes[i];
				MoveNode(node, m_oldPositions[i]);
			}
		}
	}

	void IItemDragAdapter.EndDrag()
	{
		if (m_draggingNodes == null)
		{
			return;
		}
		int num = 0;
		TNode[] draggingNodes = m_draggingNodes;
		foreach (TNode node in draggingNodes)
		{
			MoveNode(node, m_newPositions[num]);
			num++;
		}
		if (m_movingNodesCrossContainer)
		{
			m_editableGraphContainer.Move(m_targetItem, m_draggingNodes);
			m_movingNodesCrossContainer = false;
		}
		else if (m_resizing)
		{
			Point point = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.CurrentPoint);
			Point point2 = new Point(point.X - m_firstPoint.X, point.Y - m_firstPoint.Y);
			m_editableGraphContainer.Resize(m_targetItem, (int)m_firstBound.Width + point2.X, (int)m_firstBound.Height + point2.Y);
		}
		else if (m_selectionPathProvider != null)
		{
			Point point3 = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.CurrentPoint);
			Point point4 = new Point(point3.X - m_firstPoint.X, point3.Y - m_firstPoint.Y);
			if (point4.X < 0 || point4.Y < 0)
			{
				TNode[] draggingNodes2 = m_draggingNodes;
				foreach (TNode val in draggingNodes2)
				{
					AdaptablePath<object> selectionPath = m_selectionPathProvider.GetSelectionPath(val);
					if (selectionPath == null)
					{
						continue;
					}
					int count = selectionPath.Count;
					if (count <= 1)
					{
						continue;
					}
					object reference = selectionPath[count - 2];
					if (!reference.Is<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>())
					{
						continue;
					}
					ICircuitGroupType<TNode, TEdge, TEdgeRoute> circuitGroupType = reference.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
					if (val.Bounds.Location.X < -circuitGroupType.Info.Offset.X || val.Bounds.Location.Y < -circuitGroupType.Info.Offset.Y)
					{
						m_layoutContext.GetBounds(circuitGroupType, out var bounds);
						if (point4.X < 0)
						{
							bounds.X += point4.X;
						}
						if (point4.Y < 0)
						{
							bounds.Y += point4.Y;
						}
						m_layoutContext.SetBounds(circuitGroupType, bounds, BoundsSpecified.Location);
					}
				}
			}
		}
		m_draggingNodes = null;
	}

	private void AddDragNode(TNode node, ActiveCollection<TNode> draggingNodes, HashSet<TNode> nodes)
	{
		draggingNodes.Add(node);
		if (!nodes.Contains(node))
		{
			nodes.Add(node);
		}
		if (!DraggingSubNodes)
		{
			return;
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

	private void MoveNode(TNode node, Point location)
	{
		Rectangle bounds = new Rectangle(location.X, location.Y, 0, 0);
		m_layoutContext.SetBounds(node, bounds, BoundsSpecified.Location);
	}

	private object ChooseActiveTarget(GraphHitRecord<TNode, TEdge, TEdgeRoute> hitRecord)
	{
		if (hitRecord.SubItem == null)
		{
			return hitRecord.Item;
		}
		if (m_editableGraphContainer != null)
		{
			foreach (TNode item in hitRecord.HitPathInversed)
			{
				if (m_editableGraphContainer.CanMove(item, m_draggingNodes))
				{
					return item;
				}
			}
		}
		return hitRecord.Item;
	}

	private void ResetCustomStyle(object item)
	{
		m_renderer.ClearCustomStyle(item);
	}
}
