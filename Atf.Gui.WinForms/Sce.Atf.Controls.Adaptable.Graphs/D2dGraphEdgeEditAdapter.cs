using System;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class D2dGraphEdgeEditAdapter<TNode, TEdge, TEdgeRoute> : DraggingControlAdapter where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	public class EdgeDraggingContext
	{
		private PointF m_toRoutePos;

		private D2dGraphEdgeEditAdapter<TNode, TEdge, TEdgeRoute> m_edgeEditAdapter;

		public TNode DragFromNode { get; set; }

		public TEdgeRoute DragFromRoute { get; set; }

		public TNode DragToNode { get; set; }

		public TEdgeRoute DragToRoute { get; set; }

		public bool FromSourceToDestination { get; set; }

		public TEdge ExistingEdge { get; set; }

		public TEdge DisconnectEdge { get; set; }

		public GraphHitRecord<TNode, TEdge, TEdgeRoute> MousePick { get; set; }

		internal IEditableGraph<TNode, TEdge, TEdgeRoute> EditableGraph
		{
			get
			{
				TNode reference = HitPathsGetLowestCommonAncestor();
				IEditableGraph<TNode, TEdge, TEdgeRoute> editableGraph = reference.As<IEditableGraph<TNode, TEdge, TEdgeRoute>>();
				return editableGraph ?? m_edgeEditAdapter.m_mainEditableGraph;
			}
		}

		internal AdaptablePath<object> DragFromNodeHitPath { get; set; }

		internal AdaptablePath<object> DragToNodeHitPath { get; set; }

		public PointF FromRoutePos { get; set; }

		public PointF ToRoutePos
		{
			get
			{
				return m_toRoutePos;
			}
			set
			{
				if (value == PointF.Empty)
				{
				}
				m_toRoutePos = value;
			}
		}

		public EdgeDraggingContext(D2dGraphEdgeEditAdapter<TNode, TEdge, TEdgeRoute> edgeEditAdapter)
		{
			m_edgeEditAdapter = edgeEditAdapter;
		}

		internal TNode ActualFromNode()
		{
			if (DragFromNode == null)
			{
				return null;
			}
			if (DragFromNode == DragToNode || DragToNode == null)
			{
				return DragFromNode;
			}
			if (DragFromNodeHitPath == null)
			{
				return DragFromNode;
			}
			if (DragToNodeHitPath == null)
			{
				return DragFromNodeHitPath[0].As<TNode>();
			}
			TNode val = HitPathsGetLowestCommonAncestor();
			if (val == null)
			{
				return DragFromNodeHitPath[0].As<TNode>();
			}
			int num = DragFromNodeHitPath.IndexOf(val);
			return DragFromNodeHitPath[num + 1].As<TNode>();
		}

		internal TNode ActualToNode()
		{
			if (DragToNode == null)
			{
				return null;
			}
			if (DragFromNode == DragToNode || DragFromNode == null)
			{
				return DragToNode;
			}
			if (DragToNodeHitPath == null)
			{
				return DragToNode;
			}
			if (DragFromNodeHitPath == null)
			{
				return DragToNodeHitPath[0].As<TNode>();
			}
			TNode val = HitPathsGetLowestCommonAncestor();
			if (val == null)
			{
				return DragToNodeHitPath[0].As<TNode>();
			}
			int num = DragToNodeHitPath.IndexOf(val);
			return DragToNodeHitPath[num + 1].As<TNode>();
		}

		internal TEdgeRoute ActualFromRoute(TNode actualFromNode)
		{
			if (DragFromNodeHitPath == null || DragFromRoute == null)
			{
				return DragFromRoute;
			}
			if (actualFromNode == DragFromNode)
			{
				return DragFromRoute;
			}
			if (m_edgeEditAdapter.EdgeRouteTraverser != null)
			{
				return m_edgeEditAdapter.EdgeRouteTraverser(DragFromNodeHitPath, actualFromNode, DragFromRoute);
			}
			return null;
		}

		internal TEdgeRoute ActualToRoute(TNode actualToNode)
		{
			if (DragToNodeHitPath == null || DragToRoute == null)
			{
				return DragToRoute;
			}
			if (actualToNode == DragToNode)
			{
				return DragToRoute;
			}
			if (m_edgeEditAdapter.EdgeRouteTraverser != null)
			{
				return m_edgeEditAdapter.EdgeRouteTraverser(DragToNodeHitPath, actualToNode, DragToRoute);
			}
			return null;
		}

		private TNode HitPathsGetLowestCommonAncestor()
		{
			if (DragToNodeHitPath == null || DragFromNodeHitPath == null)
			{
				return null;
			}
			for (int num = DragToNodeHitPath.Count - 1; num >= 0; num--)
			{
				if (DragFromNodeHitPath.Contains(DragToNodeHitPath[num]))
				{
					return DragToNodeHitPath[num].As<TNode>();
				}
			}
			return null;
		}
	}

	public Func<AdaptablePath<object>, object, TEdgeRoute, TEdgeRoute> EdgeRouteTraverser;

	private readonly EdgeDraggingContext m_draggingContext;

	private readonly D2dGraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;

	private readonly D2dGraphAdapter<TNode, TEdge, TEdgeRoute> m_graphAdapter;

	private IAutoTranslateAdapter m_autoTranslateAdapter;

	private IGraph<TNode, TEdge, TEdgeRoute> m_mainGraph;

	private IEditableGraph<TNode, TEdge, TEdgeRoute> m_mainEditableGraph;

	private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_mousePick = new GraphHitRecord<TNode, TEdge, TEdgeRoute>();

	private Point m_edgeDragPoint;

	private Cursor m_oldCursor;

	private bool m_isConnecting;

	private bool m_dragEdgeReversed;

	public bool IsDraggingEdge => m_isConnecting;

	public Cursor OverRouteCursor { get; set; }

	public Cursor FromPlaceCursor { get; set; }

	public Cursor ToPlaceCursor { get; set; }

	public Cursor InadmissibleCursor { get; set; }

	protected EdgeDraggingContext DraggingContext
	{
		get
		{
			m_draggingContext.MousePick = m_mousePick;
			return m_draggingContext;
		}
	}

	public D2dGraphEdgeEditAdapter(D2dGraphRenderer<TNode, TEdge, TEdgeRoute> renderer, D2dGraphAdapter<TNode, TEdge, TEdgeRoute> graphAdapter, ITransformAdapter transformAdapter)
	{
		m_renderer = renderer;
		m_graphAdapter = graphAdapter;
		m_draggingContext = new EdgeDraggingContext(this);
		OverRouteCursor = Cursors.Cross;
		FromPlaceCursor = Cursors.PanWest;
		ToPlaceCursor = Cursors.PanEast;
		InadmissibleCursor = Cursors.Cross;
	}

	protected override void Bind(AdaptableControl control)
	{
		m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();
		D2dAdaptableControl d2dAdaptableControl = control as D2dAdaptableControl;
		d2dAdaptableControl.ContextChanged += control_ContextChanged;
		d2dAdaptableControl.DrawingD2d += control_Paint;
		base.Bind(control);
	}

	protected override void Unbind(AdaptableControl control)
	{
		D2dAdaptableControl d2dAdaptableControl = control as D2dAdaptableControl;
		d2dAdaptableControl.ContextChanged -= control_ContextChanged;
		d2dAdaptableControl.DrawingD2d -= control_Paint;
		base.Unbind(control);
	}

	protected virtual void control_ContextChanged(object sender, EventArgs e)
	{
		m_mainGraph = base.AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
		m_mainEditableGraph = base.AdaptedControl.ContextAs<IEditableGraph<TNode, TEdge, TEdgeRoute>>();
	}

	protected virtual void control_Paint(object sender, EventArgs e)
	{
		if (m_isConnecting)
		{
			D2dAdaptableControl d2dAdaptableControl = base.AdaptedControl as D2dAdaptableControl;
			D2dGraphics d2dGraphics = d2dAdaptableControl.D2dGraphics;
			string label = ((m_draggingContext.ExistingEdge != null) ? m_draggingContext.ExistingEdge.Label : null);
			TNode val = m_draggingContext.ActualFromNode();
			TEdgeRoute val2 = m_draggingContext.ActualFromRoute(val);
			TNode val3 = m_draggingContext.ActualToNode();
			TEdgeRoute val4 = m_draggingContext.ActualToRoute(val3);
			PointF startPoint = ((val2 == null) ? ((PointF)m_edgeDragPoint) : m_draggingContext.FromRoutePos);
			PointF endPoint = ((val4 == null) ? ((PointF)m_edgeDragPoint) : m_draggingContext.ToRoutePos);
			m_renderer.DrawPartialEdge(val, val2, val3, val4, label, startPoint, endPoint, d2dGraphics);
		}
	}

	protected override void OnMouseMove(object sender, MouseEventArgs e)
	{
		base.OnMouseMove(sender, e);
		if (m_isConnecting)
		{
			ConnectWires(e);
		}
		if (!m_isConnecting && e.Button == MouseButtons.None && base.AdaptedControl.Focused)
		{
			m_mousePick = m_graphAdapter.Pick(base.CurrentPoint);
			if ((m_mousePick.FromRoute != null || m_mousePick.ToRoute != null) && base.AdaptedControl.Cursor == Cursors.Default)
			{
				base.AdaptedControl.Cursor = OverRouteCursor;
			}
		}
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		ConnectWires(e);
	}

	protected override void OnMouseUp(object sender, MouseEventArgs e)
	{
		DoMouseClick(e);
	}

	protected override void OnMouseClick(object sender, MouseEventArgs e)
	{
	}

	protected virtual void MakeConnection()
	{
		if (m_draggingContext.DisconnectEdge != null)
		{
			DraggingContext.EditableGraph.Disconnect(m_draggingContext.DisconnectEdge);
		}
		if (m_draggingContext.ExistingEdge != null)
		{
			DraggingContext.EditableGraph.Disconnect(m_draggingContext.ExistingEdge);
		}
		TNode val = m_draggingContext.ActualFromNode();
		TEdgeRoute val2 = m_draggingContext.ActualFromRoute(val);
		TNode val3 = m_draggingContext.ActualToNode();
		TEdgeRoute val4 = m_draggingContext.ActualToRoute(val3);
		if (val3 != null && val4 != null && val != null && val2 != null)
		{
			DraggingContext.EditableGraph.Connect(val, val2, val3, val4, m_draggingContext.ExistingEdge);
		}
	}

	protected void DoMouseClick(MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left || (Control.ModifierKeys & Keys.Alt) != Keys.None)
		{
			return;
		}
		if (!m_isConnecting)
		{
			ConnectWires(e);
			return;
		}
		if (m_dragEdgeReversed)
		{
			if (CanConnectTo())
			{
				m_draggingContext.DisconnectEdge = GetDisconnectEdgeTo();
			}
		}
		else if (CanConnectFrom())
		{
			m_draggingContext.DisconnectEdge = GetDisconnectEdgeFrom();
		}
		if (m_draggingContext.DragToNode != null && m_draggingContext.DragToRoute != null && m_draggingContext.DragFromNode != null && m_draggingContext.DragFromRoute != null && (m_draggingContext.ExistingEdge == null || m_draggingContext.ExistingEdge.ToNode != m_draggingContext.DragToNode || m_draggingContext.ExistingEdge.ToRoute != m_draggingContext.DragToRoute || m_draggingContext.ExistingEdge.FromNode != m_draggingContext.DragFromNode || m_draggingContext.ExistingEdge.FromRoute != m_draggingContext.DragFromRoute))
		{
			ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
			context.DoTransaction(MakeConnection, "Drag Edge".Localize());
		}
		if (m_autoTranslateAdapter != null)
		{
			m_autoTranslateAdapter.Enabled = false;
		}
		m_isConnecting = false;
		m_draggingContext.DragFromNode = null;
		m_draggingContext.DragFromRoute = null;
		m_draggingContext.DragToNode = null;
		m_draggingContext.DragToRoute = null;
		m_draggingContext.ExistingEdge = null;
		m_draggingContext.DisconnectEdge = null;
		m_graphAdapter.HideEdge(null);
		base.AdaptedControl.AutoResetCursor = true;
		base.AdaptedControl.Cursor = m_oldCursor;
		m_renderer.RouteConnecting = null;
		base.AdaptedControl.Invalidate();
	}

	private void ConnectWires(MouseEventArgs e)
	{
		if (m_mainEditableGraph != null && !m_isConnecting && e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Alt) == 0 && !base.AdaptedControl.Capture)
		{
			m_draggingContext.DisconnectEdge = null;
			m_draggingContext.DragFromNode = null;
			m_draggingContext.DragToNode = null;
			m_draggingContext.DragFromRoute = null;
			m_draggingContext.DragToRoute = null;
			m_mousePick = m_graphAdapter.Pick(base.FirstPoint);
			if (m_mousePick.Node != null)
			{
				Cursor cursor = base.AdaptedControl.Cursor;
				m_edgeDragPoint = base.FirstPoint;
				m_draggingContext.FromSourceToDestination = false;
				if (m_draggingContext.ExistingEdge == null)
				{
					if (m_mousePick.FromRoute != null && !m_mousePick.FromRoute.AllowFanOut)
					{
						m_draggingContext.ExistingEdge = GetFirstEdgeFrom(m_draggingContext.ActualFromNode(), m_mousePick.FromRoute);
					}
					else if (m_mousePick.ToRoute != null && !m_mousePick.ToRoute.AllowFanIn)
					{
						m_draggingContext.ExistingEdge = GetFirstEdgeTo(m_draggingContext.ActualToNode(), m_mousePick.ToRoute);
					}
				}
				TNode val = null;
				TEdgeRoute startRoute = null;
				if (m_mousePick.FromRoute != null)
				{
					EdgeDraggingContext draggingContext = m_draggingContext;
					TNode obj = m_mousePick.SubNode ?? m_mousePick.Node;
					TNode val2 = obj;
					draggingContext.DragFromNode = obj;
					val = val2;
					m_draggingContext.DragFromNodeHitPath = m_mousePick.HitPath;
					TEdgeRoute val3 = (m_draggingContext.DragFromRoute = m_mousePick.FromRoute);
					startRoute = val3;
					m_draggingContext.FromRoutePos = m_mousePick.FromRoutePos;
					m_dragEdgeReversed = true;
					m_draggingContext.FromSourceToDestination = true;
					m_isConnecting = true;
					cursor = OverRouteCursor;
				}
				else if (m_mousePick.ToRoute != null)
				{
					EdgeDraggingContext draggingContext2 = m_draggingContext;
					TNode obj2 = m_mousePick.SubNode ?? m_mousePick.Node;
					TNode val2 = obj2;
					draggingContext2.DragToNode = obj2;
					val = val2;
					m_draggingContext.DragToNodeHitPath = m_mousePick.HitPath;
					TEdgeRoute val3 = (m_draggingContext.DragToRoute = m_mousePick.ToRoute);
					startRoute = val3;
					m_draggingContext.ToRoutePos = m_mousePick.ToRoutePos;
					m_dragEdgeReversed = false;
					m_isConnecting = true;
					cursor = OverRouteCursor;
				}
				if (m_isConnecting)
				{
					m_oldCursor = base.AdaptedControl.Cursor;
					base.AdaptedControl.AutoResetCursor = false;
					base.AdaptedControl.Cursor = cursor;
					base.AdaptedControl.Capture = true;
					if (val != null)
					{
						D2dGraphRenderer<TNode, TEdge, TEdgeRoute>.RouteConnectingInfo routeConnecting = new D2dGraphRenderer<TNode, TEdge, TEdgeRoute>.RouteConnectingInfo
						{
							EditableGraph = DraggingContext.EditableGraph,
							StartNode = val,
							StartRoute = startRoute
						};
						m_renderer.RouteConnecting = routeConnecting;
					}
					else
					{
						m_renderer.RouteConnecting = null;
					}
					if (m_autoTranslateAdapter != null)
					{
						m_autoTranslateAdapter.Enabled = true;
					}
				}
			}
			m_graphAdapter.HideEdge(m_draggingContext.ExistingEdge);
		}
		if (!m_isConnecting)
		{
			return;
		}
		m_edgeDragPoint = base.CurrentPoint;
		m_mousePick = m_graphAdapter.Pick(base.CurrentPoint);
		Cursor cursor2;
		if (m_dragEdgeReversed)
		{
			if (CanConnectTo())
			{
				m_draggingContext.DragToNode = m_mousePick.SubNode ?? m_mousePick.Node;
				m_draggingContext.DragToNodeHitPath = m_mousePick.HitPath;
				m_draggingContext.DragToRoute = m_mousePick.ToRoute;
				m_draggingContext.ToRoutePos = m_mousePick.ToRoutePos;
				cursor2 = ToPlaceCursor;
			}
			else
			{
				m_draggingContext.DragToNode = null;
				m_draggingContext.DragToRoute = null;
				cursor2 = InadmissibleCursor;
			}
		}
		else if (CanConnectFrom())
		{
			m_draggingContext.DragFromNode = m_mousePick.SubNode ?? m_mousePick.Node;
			m_draggingContext.DragFromNodeHitPath = m_mousePick.HitPath;
			m_draggingContext.DragFromRoute = m_mousePick.FromRoute;
			m_draggingContext.FromRoutePos = m_mousePick.FromRoutePos;
			cursor2 = FromPlaceCursor;
		}
		else
		{
			m_draggingContext.DragFromNode = null;
			m_draggingContext.DragFromRoute = null;
			cursor2 = InadmissibleCursor;
		}
		base.AdaptedControl.AutoResetCursor = false;
		base.AdaptedControl.Cursor = cursor2;
		D2dAdaptableControl d2dAdaptableControl = base.AdaptedControl as D2dAdaptableControl;
		d2dAdaptableControl.DrawD2d();
	}

	protected virtual bool CanConnectTo()
	{
		if (m_mainEditableGraph == null || m_mousePick.Node == null || m_mousePick.ToRoute == null)
		{
			return false;
		}
		m_draggingContext.DragToNode = m_mousePick.SubNode ?? m_mousePick.Node;
		m_draggingContext.DragToRoute = m_mousePick.ToRoute;
		m_draggingContext.ToRoutePos = m_mousePick.ToRoutePos;
		m_draggingContext.DragToNodeHitPath = m_mousePick.HitPath;
		TNode val = m_draggingContext.ActualFromNode();
		TEdgeRoute fromRoute = m_draggingContext.ActualFromRoute(val);
		TNode val2 = m_draggingContext.ActualToNode();
		TEdgeRoute toRoute = m_draggingContext.ActualToRoute(val2);
		return DraggingContext.EditableGraph.CanConnect(val, fromRoute, val2, toRoute);
	}

	protected virtual bool CanConnectFrom()
	{
		if (m_mainEditableGraph == null || m_mousePick.Node == null || m_mousePick.FromRoute == null)
		{
			return false;
		}
		m_draggingContext.DragFromNode = m_mousePick.SubNode ?? m_mousePick.Node;
		m_draggingContext.DragFromRoute = m_mousePick.FromRoute;
		m_draggingContext.FromRoutePos = m_mousePick.FromRoutePos;
		m_draggingContext.DragFromNodeHitPath = m_mousePick.HitPath;
		TNode val = m_draggingContext.ActualFromNode();
		TEdgeRoute fromRoute = m_draggingContext.ActualFromRoute(val);
		TNode val2 = m_draggingContext.ActualToNode();
		TEdgeRoute toRoute = m_draggingContext.ActualToRoute(val2);
		return DraggingContext.EditableGraph.CanConnect(val, fromRoute, val2, toRoute);
	}

	private TEdge GetDisconnectEdgeTo()
	{
		TEdge val = null;
		if (!m_mousePick.ToRoute.AllowFanIn)
		{
			TNode node = m_draggingContext.ActualToNode();
			val = GetFirstEdgeTo(node, m_mousePick.ToRoute);
			if (!CanDisconnect(val))
			{
				val = null;
			}
		}
		return val;
	}

	private TEdge GetDisconnectEdgeFrom()
	{
		TEdge val = null;
		if (!m_mousePick.FromRoute.AllowFanOut)
		{
			val = GetFirstEdgeFrom(m_draggingContext.ActualFromNode(), m_mousePick.FromRoute);
			if (!CanDisconnect(val))
			{
				val = null;
			}
		}
		return val;
	}

	protected bool CanDisconnect(TEdge edge)
	{
		return edge != m_draggingContext.ExistingEdge && DraggingContext.EditableGraph.CanDisconnect(edge);
	}

	protected TEdge GetFirstEdgeTo(TNode node, object toRoute)
	{
		if (m_mousePick.SubNode == null)
		{
			return FindFirstEdgeInGraph(m_mainGraph, node, toRoute as TEdgeRoute, fromEdge: false);
		}
		IGraph<TNode, TEdge, TEdgeRoute> graph = null;
		object reference = null;
		for (int num = m_mousePick.HitPath.Count - 1; num >= 0; num--)
		{
			object obj = m_mousePick.HitPath[num];
			if (obj == node)
			{
				if (num > 0)
				{
					reference = m_mousePick.HitPath[num - 1];
				}
				break;
			}
		}
		graph = reference.As<IGraph<TNode, TEdge, TEdgeRoute>>();
		if (graph == null)
		{
			return null;
		}
		if (EdgeRouteTraverser != null)
		{
			TEdgeRoute route = EdgeRouteTraverser(m_mousePick.HitPath, node, toRoute.Cast<TEdgeRoute>());
			if (graph != null)
			{
				return FindFirstEdgeInGraph(graph, node, route, fromEdge: false);
			}
		}
		return null;
	}

	protected TEdge GetFirstEdgeFrom(TNode node, object fromRoute)
	{
		if (m_mousePick.SubNode == null)
		{
			return FindFirstEdgeInGraph(m_mainGraph, node, fromRoute as TEdgeRoute, fromEdge: true);
		}
		IGraph<TNode, TEdge, TEdgeRoute> graph = null;
		object obj = null;
		for (int num = m_mousePick.HitPath.Count - 1; num >= 0; num--)
		{
			object obj2 = m_mousePick.HitPath[num];
			if (obj2 == node)
			{
				if (num > 0)
				{
					obj = m_mousePick.HitPath[num - 1];
				}
				break;
			}
		}
		graph = obj.As<IGraph<TNode, TEdge, TEdgeRoute>>();
		if (graph == null)
		{
			return null;
		}
		if (EdgeRouteTraverser != null)
		{
			TEdgeRoute route = EdgeRouteTraverser(m_mousePick.HitPath, obj, fromRoute.Cast<TEdgeRoute>());
			if (graph != null)
			{
				return FindFirstEdgeInGraph(graph, node, route, fromEdge: true);
			}
		}
		return null;
	}

	private TEdge FindFirstEdgeInGraph(IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node, TEdgeRoute route, bool fromEdge)
	{
		if (fromEdge)
		{
			foreach (TEdge edge in graph.Edges)
			{
				if (edge.FromNode == node && edge.FromRoute.Equals(route))
				{
					return edge;
				}
			}
		}
		else
		{
			foreach (TEdge edge2 in graph.Edges)
			{
				if (edge2.ToNode == node && edge2.ToRoute.Equals(route))
				{
					return edge2;
				}
			}
		}
		return null;
	}
}
