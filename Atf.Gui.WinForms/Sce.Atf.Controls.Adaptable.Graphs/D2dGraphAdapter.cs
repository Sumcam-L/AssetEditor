using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class D2dGraphAdapter<TNode, TEdge, TEdgeRoute> : ControlAdapter, IPickingAdapter2, IDisposable where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	public enum DrawEdgePolicy
	{
		Associated,
		AllFirst
	}

	protected class EmptyGraph : IGraph<TNode, TEdge, TEdgeRoute>
	{
		public IEnumerable<TNode> Nodes => EmptyEnumerable<TNode>.Instance;

		public IEnumerable<TEdge> Edges => EmptyEnumerable<TEdge>.Instance;
	}

	private D2dAdaptableControl m_d2dControl;

	private D2dGraphics m_d2dGraphics;

	private readonly D2dGraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;

	private object m_hoverObject;

	private object m_hoverSubObject;

	private ISelectionPathProvider m_selectionPathProvider;

	private TEdge m_hiddenEdge;

	private IGraph<TNode, TEdge, TEdgeRoute> m_graph = s_emptyGraph;

	private IObservableContext m_observableContext;

	private ISelectionContext m_selectionContext;

	private IVisibilityContext m_visibilityContext;

	private Multimap<TNode, TEdge> m_fromNodeEdges = new Multimap<TNode, TEdge>();

	private Multimap<TNode, TEdge> m_toNodeEdges = new Multimap<TNode, TEdge>();

	private Dictionary<TEdge, int> m_edgeNodeEncounter = new Dictionary<TEdge, int>();

	protected static readonly EmptyGraph s_emptyGraph = new EmptyGraph();

	public DrawEdgePolicy EdgeRenderPolicy { get; set; }

	public D2dGraphRenderer<TNode, TEdge, TEdgeRoute> Renderer => m_renderer;

	protected D2dAdaptableControl D2DAdaptableControl => m_d2dControl;

	protected D2dGraphics D2dGraphics => m_d2dGraphics;

	protected IGraph<TNode, TEdge, TEdgeRoute> Graph => m_graph;

	protected TEdge HiddenEdge => m_hiddenEdge;

	public D2dGraphAdapter(D2dGraphRenderer<TNode, TEdge, TEdgeRoute> renderer, ITransformAdapter transformAdapter)
	{
		m_renderer = renderer;
		m_renderer.Redraw += renderer_Redraw;
		m_renderer.GetStyle = GetStyle;
	}

	public void Dispose()
	{
		m_renderer.Redraw -= renderer_Redraw;
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
	}

	public virtual DiagramDrawingStyle GetStyle(object item)
	{
		DiagramDrawingStyle customStyle = m_renderer.GetCustomStyle(item);
		if (customStyle != DiagramDrawingStyle.None)
		{
			return customStyle;
		}
		customStyle = DiagramDrawingStyle.Normal;
		if (m_visibilityContext != null && !m_visibilityContext.IsVisible(item))
		{
			customStyle = DiagramDrawingStyle.Hidden;
		}
		else if (item == m_hoverObject || item == m_hoverSubObject)
		{
			customStyle = (CircuitUtil.IsGroupTemplateInstance(item) ? DiagramDrawingStyle.TemplatedInstance : ((!item.Is<Group>()) ? DiagramDrawingStyle.Hot : DiagramDrawingStyle.CopyInstance));
		}
		else if (m_selectionContext != null && m_selectionContext.SelectionContains(item))
		{
			customStyle = ((!m_selectionContext.LastSelected.Equals(item)) ? DiagramDrawingStyle.Selected : DiagramDrawingStyle.LastSelected);
		}
		else if (m_selectionPathProvider != null && m_selectionPathProvider.IncludedPath(item) != null)
		{
			customStyle = (CircuitUtil.IsGroupTemplateInstance(item) ? DiagramDrawingStyle.TemplatedInstance : ((!item.Is<Group>()) ? DiagramDrawingStyle.Hot : DiagramDrawingStyle.CopyInstance));
		}
		else if (m_renderer.RouteConnecting != null && item.Is<TEdge>())
		{
			TEdge val = item.Cast<TEdge>();
			if (m_renderer.RouteConnecting.StartNode.Equals(val.FromNode) || m_renderer.RouteConnecting.StartNode.Equals(val.ToNode))
			{
				customStyle = DiagramDrawingStyle.Hot;
			}
		}
		return customStyle;
	}

	public void HideEdge(TEdge edge)
	{
		m_hiddenEdge = edge;
	}

	public GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(Point p)
	{
		return (GraphHitRecord<TNode, TEdge, TEdgeRoute>)((IPickingAdapter2)this).Pick(p);
	}

	DiagramHitRecord IPickingAdapter2.Pick(Point pickPoint)
	{
		Matrix3x2F mat = Matrix3x2F.Invert(m_d2dGraphics.Transform);
		PointF p = Matrix3x2F.TransformPoint(mat, pickPoint);
		TEdge priorityEdge = null;
		if (m_selectionContext != null)
		{
			priorityEdge = m_selectionContext.GetLastSelected<TEdge>();
		}
		return m_renderer.Pick(m_graph, priorityEdge, p, m_d2dGraphics);
	}

	public GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(IEnumerable<TNode> nodes, IEnumerable<TEdge> edges, Point pickPoint)
	{
		Matrix3x2F mat = Matrix3x2F.Invert(m_d2dGraphics.Transform);
		PointF p = Matrix3x2F.TransformPoint(mat, pickPoint);
		TEdge priorityEdge = null;
		if (m_selectionContext != null)
		{
			priorityEdge = m_selectionContext.GetLastSelected<TEdge>();
		}
		return m_renderer.Pick(nodes, edges, priorityEdge, p, m_d2dGraphics);
	}

	public virtual IEnumerable<object> Pick(Rectangle pickRect)
	{
		Matrix3x2F matrix = Matrix3x2F.Invert(m_d2dGraphics.Transform);
		RectangleF rect = D2dUtil.Transform(matrix, pickRect);
		return m_renderer.Pick(m_graph, rect, m_d2dGraphics);
	}

	public virtual Rectangle GetBounds(IEnumerable<object> items)
	{
		RectangleF bounds = m_renderer.GetBounds(items.AsIEnumerable<TNode>(), m_d2dGraphics);
		bounds = D2dUtil.Transform(m_d2dGraphics.Transform, bounds);
		return Rectangle.Truncate(bounds);
	}

	public virtual RectangleF GetLocalBound(TNode elem)
	{
		return m_renderer.GetBounds(elem, m_d2dGraphics);
	}

	protected override void Bind(AdaptableControl control)
	{
		if (m_d2dControl != null)
		{
			throw new InvalidOperationException("We can only bind to one D2dAdaptableControl at a time");
		}
		m_d2dControl = (D2dAdaptableControl)control;
		m_d2dGraphics = m_d2dControl.D2dGraphics;
		m_d2dControl.ContextChanged += control_ContextChanged;
		m_d2dControl.DrawingD2d += control_Paint;
		m_d2dControl.MouseDown += d2dControl_MouseDown;
		m_d2dControl.MouseMove += d2dControl_MouseMove;
		m_d2dControl.MouseLeave += d2dControl_MouseLeave;
		m_selectionPathProvider = control.As<ISelectionPathProvider>();
	}

	protected override void Unbind(AdaptableControl control)
	{
		if (m_d2dControl != control)
		{
			throw new InvalidOperationException("We can only unbind from a D2dAdaptableControl that we previously were bound to");
		}
		m_d2dControl.ContextChanged -= control_ContextChanged;
		m_d2dControl.DrawingD2d -= control_Paint;
		m_d2dControl.MouseDown -= d2dControl_MouseDown;
		m_d2dControl.MouseMove -= d2dControl_MouseMove;
		m_d2dControl.MouseLeave -= d2dControl_MouseLeave;
		m_d2dGraphics = null;
		m_d2dControl = null;
		m_selectionPathProvider = null;
	}

	protected virtual void OnRender()
	{
		m_d2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
		Matrix3x2F transform = m_d2dGraphics.Transform;
		transform.Invert();
		RectangleF clipBounds = Matrix3x2F.Transform(transform, base.AdaptedControl.ClientRectangle);
		if (EdgeRenderPolicy == DrawEdgePolicy.AllFirst)
		{
			foreach (TEdge edge in m_graph.Edges)
			{
				DrawEdge(edge, clipBounds);
			}
		}
		else
		{
			m_fromNodeEdges.Clear();
			m_toNodeEdges.Clear();
			m_edgeNodeEncounter.Clear();
			foreach (TEdge edge2 in m_graph.Edges)
			{
				m_fromNodeEdges.Add(edge2.FromNode, edge2);
				m_toNodeEdges.Add(edge2.ToNode, edge2);
				m_edgeNodeEncounter.Add(edge2, 0);
			}
		}
		TNode val = null;
		List<TNode> list = new List<TNode>();
		List<TNode> list2 = new List<TNode>();
		List<TNode> list3 = new List<TNode>();
		foreach (TNode node in m_graph.Nodes)
		{
			RectangleF bounds = m_renderer.GetBounds(node, m_d2dGraphics);
			if (clipBounds.IntersectsWith(bounds))
			{
				DiagramDrawingStyle style = GetStyle(node);
				if (style == DiagramDrawingStyle.DragSource)
				{
					list.Add(node);
					continue;
				}
				if (style == DiagramDrawingStyle.Selected || style == DiagramDrawingStyle.LastSelected || (style == DiagramDrawingStyle.Hot && m_selectionContext != null && m_selectionContext.SelectionContains(node)))
				{
					list2.Add(node);
					continue;
				}
				bool flag = false;
				ICircuitGroupType<TNode, TEdge, TEdgeRoute> circuitGroupType = node.As<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
				if (circuitGroupType != null)
				{
					circuitGroupType.Info.PickingPriority = 0;
					if (circuitGroupType.Expanded)
					{
						if (node == ActiveContainer())
						{
							val = node;
						}
						else
						{
							list3.Add(node);
						}
						flag = true;
					}
				}
				if (!flag)
				{
					m_renderer.Draw(node, style, m_d2dGraphics);
					if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
					{
						TryDrawAssociatedEdges(node, clipBounds);
					}
				}
			}
			else if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
			{
				TryDrawAssociatedEdges(node, clipBounds);
			}
		}
		int num = 0;
		foreach (TNode item in list3)
		{
			ICircuitGroupType<TNode, TEdge, TEdgeRoute> circuitGroupType2 = item.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
			circuitGroupType2.Info.PickingPriority = num++;
			m_renderer.Draw(item, GetStyle(item), m_d2dGraphics);
			if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
			{
				TryDrawAssociatedEdges(item, clipBounds);
			}
		}
		if (val != null)
		{
			ICircuitGroupType<TNode, TEdge, TEdgeRoute> circuitGroupType3 = val.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
			circuitGroupType3.Info.PickingPriority = num++;
			m_renderer.Draw(val, GetStyle(val), m_d2dGraphics);
			if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
			{
				TryDrawAssociatedEdges(val, clipBounds);
			}
		}
		foreach (TNode item2 in list2)
		{
			ICircuitGroupType<TNode, TEdge, TEdgeRoute> circuitGroupType4 = item2.As<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
			if (circuitGroupType4 != null)
			{
				circuitGroupType4.Info.PickingPriority = num++;
			}
			m_renderer.Draw(item2, GetStyle(item2), m_d2dGraphics);
			if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
			{
				TryDrawAssociatedEdges(item2, clipBounds);
			}
		}
		foreach (TNode item3 in list)
		{
			m_renderer.Draw(item3, DiagramDrawingStyle.DragSource, m_d2dGraphics);
			if (EdgeRenderPolicy == DrawEdgePolicy.Associated)
			{
				TryDrawAssociatedEdges(item3, clipBounds);
			}
		}
	}

	private object ActiveContainer()
	{
		if (m_selectionContext != null)
		{
			return m_selectionPathProvider.Parent(m_selectionContext.LastSelected);
		}
		return null;
	}

	private void TryDrawAssociatedEdges(TNode nodeDrawn, RectangleF clipBounds)
	{
		if (m_fromNodeEdges.ContainsKey(nodeDrawn))
		{
			IEnumerable<TEdge> enumerable = m_fromNodeEdges[nodeDrawn];
			foreach (TEdge item in enumerable)
			{
				m_edgeNodeEncounter[item] += 1;
				if (m_edgeNodeEncounter[item] == 2)
				{
					DrawEdge(item, clipBounds);
				}
			}
		}
		if (!m_toNodeEdges.ContainsKey(nodeDrawn))
		{
			return;
		}
		IEnumerable<TEdge> enumerable2 = m_toNodeEdges[nodeDrawn];
		foreach (TEdge item2 in enumerable2)
		{
			m_edgeNodeEncounter[item2] += 1;
			if (m_edgeNodeEncounter[item2] == 2)
			{
				DrawEdge(item2, clipBounds);
			}
		}
	}

	private void DrawEdge(TEdge edge, RectangleF clipBounds)
	{
		if (edge != m_hiddenEdge)
		{
			RectangleF bounds = m_renderer.GetBounds(edge, m_d2dGraphics);
			if (clipBounds.IntersectsWith(bounds))
			{
				DiagramDrawingStyle style = GetStyle(edge);
				m_renderer.Draw(edge, style, m_d2dGraphics);
			}
		}
	}

	private void control_Paint(object sender, EventArgs e)
	{
		OnRender();
	}

	private void d2dControl_MouseLeave(object sender, EventArgs e)
	{
		m_hoverObject = null;
		ResetCustomStyle(m_hoverSubObject);
		m_hoverSubObject = null;
	}

	private void d2dControl_MouseDown(object sender, MouseEventArgs e)
	{
		m_hoverObject = null;
		ResetCustomStyle(m_hoverSubObject);
		m_hoverSubObject = null;
	}

	private void d2dControl_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.None && base.AdaptedControl.Focused)
		{
			DiagramHitRecord diagramHitRecord = ((IPickingAdapter2)this).Pick(e.Location);
			object item = diagramHitRecord.Item;
			bool flag = false;
			if (m_hoverObject != item)
			{
				m_hoverObject = item;
				flag = true;
			}
			if (m_hoverSubObject != diagramHitRecord.SubItem)
			{
				ResetCustomStyle(m_hoverSubObject);
				m_hoverSubObject = diagramHitRecord.SubItem;
				m_renderer.SetCustomStyle(m_hoverSubObject, DiagramDrawingStyle.DragSource);
				flag = true;
			}
			if (flag)
			{
				Invalidate();
			}
		}
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		IGraph<TNode, TEdge, TEdgeRoute> graph = base.AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
		if (graph == null)
		{
			graph = s_emptyGraph;
		}
		if (m_graph == graph)
		{
			return;
		}
		if (m_graph != null)
		{
			if (m_observableContext != null)
			{
				m_observableContext.ItemInserted -= graph_ObjectInserted;
				m_observableContext.ItemRemoved -= graph_ObjectRemoved;
				m_observableContext.ItemChanged -= graph_ObjectChanged;
				m_observableContext.Reloaded -= graph_Reloaded;
				m_observableContext = null;
			}
			if (m_selectionContext != null)
			{
				m_selectionContext.SelectionChanged -= selection_Changed;
				m_selectionContext = null;
			}
			m_visibilityContext = null;
		}
		m_graph = graph;
		if (m_graph != null)
		{
			m_observableContext = base.AdaptedControl.ContextAs<IObservableContext>();
			if (m_observableContext != null)
			{
				m_observableContext.ItemInserted += graph_ObjectInserted;
				m_observableContext.ItemRemoved += graph_ObjectRemoved;
				m_observableContext.ItemChanged += graph_ObjectChanged;
				m_observableContext.Reloaded += graph_Reloaded;
			}
			m_selectionContext = base.AdaptedControl.ContextAs<ISelectionContext>();
			if (m_selectionContext != null)
			{
				m_selectionContext.SelectionChanged += selection_Changed;
			}
			m_visibilityContext = base.AdaptedControl.ContextAs<IVisibilityContext>();
		}
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void graph_ObjectInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		m_renderer.OnGraphObjectInserted(sender, e);
		Invalidate();
	}

	private void graph_ObjectRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		m_renderer.OnGraphObjectRemoved(sender, e);
		Invalidate();
	}

	private void graph_ObjectChanged(object sender, ItemChangedEventArgs<object> e)
	{
		m_renderer.OnGraphObjectChanged(sender, e);
		Invalidate();
	}

	private void graph_Reloaded(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void renderer_Redraw(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void Invalidate()
	{
		if (base.AdaptedControl is D2dAdaptableControl d2dAdaptableControl)
		{
			d2dAdaptableControl.Invalidate();
		}
	}

	private void ResetCustomStyle(object item)
	{
		m_renderer.ClearCustomStyle(item);
	}
}
