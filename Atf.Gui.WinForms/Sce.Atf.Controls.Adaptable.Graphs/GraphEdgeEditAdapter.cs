using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class GraphEdgeEditAdapter<TNode, TEdge, TEdgeRoute> : DraggingControlAdapter where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	private readonly GraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;

	private readonly IGraphAdapter<TNode, TEdge, TEdgeRoute> m_graphAdapter;

	private readonly ITransformAdapter m_transformAdapter;

	private IAutoTranslateAdapter m_autoTranslateAdapter;

	private IGraph<TNode, TEdge, TEdgeRoute> m_graph;

	private IEditableGraph<TNode, TEdge, TEdgeRoute> m_editableGraph;

	private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_mousePick = new GraphHitRecord<TNode, TEdge, TEdgeRoute>();

	private TEdge m_hotEdge;

	private TNode m_dragFromNode;

	private TEdgeRoute m_dragFromRoute;

	private TNode m_dragToNode;

	private TEdgeRoute m_dragToRoute;

	private TEdge m_existingEdge;

	private TEdge m_disconnectEdge;

	private Point m_edgeDragPoint;

	private Cursor m_oldCursor;

	private bool m_hotTrack = true;

	private bool m_isDragging;

	private bool m_dragEdgeReversed;

	public bool HotTrack
	{
		get
		{
			return m_hotTrack;
		}
		set
		{
			m_hotTrack = value;
			ResetHotEdge();
		}
	}

	public bool IsDraggingEdge => m_isDragging;

	public GraphEdgeEditAdapter(GraphRenderer<TNode, TEdge, TEdgeRoute> renderer, IGraphAdapter<TNode, TEdge, TEdgeRoute> graphAdapter, ITransformAdapter transformAdapter)
	{
		m_renderer = renderer;
		m_graphAdapter = graphAdapter;
		m_transformAdapter = transformAdapter;
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
		m_editableGraph = base.AdaptedControl.ContextAs<IEditableGraph<TNode, TEdge, TEdgeRoute>>();
	}

	private void control_Paint(object sender, PaintEventArgs e)
	{
		if (m_isDragging)
		{
			Graphics graphics = e.Graphics;
			Matrix transform = m_transformAdapter.Transform;
			Matrix transform2 = graphics.Transform;
			Region clip = graphics.Clip;
			graphics.Transform = transform;
			Rectangle clip2 = GdiUtil.InverseTransform(graphics.Transform, e.ClipRectangle);
			graphics.SetClip(clip2);
			if (m_disconnectEdge != null)
			{
				m_renderer.Draw(m_disconnectEdge, DiagramDrawingStyle.Ghosted, graphics);
			}
			string label = ((m_existingEdge != null) ? m_existingEdge.Label : null);
			m_renderer.Draw(m_dragFromNode, m_dragFromRoute, m_dragToNode, m_dragToRoute, label, m_edgeDragPoint, graphics);
			graphics.Transform = transform2;
			graphics.Clip = clip;
		}
	}

	protected override void OnMouseMove(object sender, MouseEventArgs e)
	{
		base.OnMouseMove(sender, e);
		if (e.Button != MouseButtons.None || !base.AdaptedControl.Focused)
		{
			return;
		}
		m_mousePick = m_graphAdapter.Pick(base.CurrentPoint);
		if (m_hotTrack)
		{
			TEdge val = null;
			if (m_mousePick.Edge != null)
			{
				val = m_mousePick.Edge;
			}
			if (val != m_hotEdge)
			{
				ResetHotEdge();
				SetHotEdge(val);
			}
		}
		if ((m_mousePick.FromRoute != null || m_mousePick.ToRoute != null) && base.AdaptedControl.Cursor == Cursors.Default)
		{
			base.AdaptedControl.Cursor = Cursors.UpArrow;
		}
	}

	protected override void OnMouseLeave(object sender, EventArgs e)
	{
		base.OnMouseLeave(sender, e);
		ResetHotEdge();
	}

	private void SetHotEdge(TEdge hotEdge)
	{
		m_hotEdge = hotEdge;
		if (hotEdge != null)
		{
			m_graphAdapter.SetStyle(m_hotEdge, DiagramDrawingStyle.Hot);
		}
		if (base.AdaptedControl.Focused)
		{
			base.AdaptedControl.Invalidate();
		}
	}

	private void ResetHotEdge()
	{
		if (m_hotEdge != null)
		{
			m_graphAdapter.ResetStyle(m_hotEdge);
			m_hotEdge = null;
			if (base.AdaptedControl.Focused)
			{
				base.AdaptedControl.Invalidate();
			}
		}
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		if (m_editableGraph != null && !m_isDragging)
		{
			ResetHotEdge();
			if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Alt) == 0 && !base.AdaptedControl.Capture)
			{
				m_mousePick = m_graphAdapter.Pick(base.FirstPoint);
				if (m_mousePick.Node != null)
				{
					Cursor cursor = base.AdaptedControl.Cursor;
					m_edgeDragPoint = base.FirstPoint;
					m_existingEdge = m_mousePick.Edge;
					bool dragEdgeReversed = m_mousePick.FromRoute == null;
					if (m_existingEdge == null)
					{
						if (m_mousePick.FromRoute != null && !m_mousePick.FromRoute.AllowFanOut)
						{
							m_existingEdge = GetFirstEdgeFrom(m_mousePick.Node, m_mousePick.FromRoute);
							dragEdgeReversed = false;
						}
						else if (m_mousePick.ToRoute != null && !m_mousePick.ToRoute.AllowFanIn)
						{
							m_existingEdge = GetFirstEdgeTo(m_mousePick.Node, m_mousePick.ToRoute);
							dragEdgeReversed = true;
						}
					}
					if (m_existingEdge != null)
					{
						if (m_editableGraph.CanDisconnect(m_existingEdge))
						{
							m_dragFromNode = m_existingEdge.FromNode;
							m_dragFromRoute = m_existingEdge.FromRoute;
							m_dragToNode = m_existingEdge.ToNode;
							m_dragToRoute = m_existingEdge.ToRoute;
							m_graphAdapter.SetStyle(m_existingEdge, DiagramDrawingStyle.Ghosted);
							m_dragEdgeReversed = dragEdgeReversed;
							m_isDragging = true;
							cursor = Cursors.UpArrow;
						}
					}
					else if (m_mousePick.FromRoute != null)
					{
						m_dragFromNode = m_mousePick.Node;
						m_dragFromRoute = m_mousePick.FromRoute;
						m_dragEdgeReversed = true;
						m_isDragging = true;
						cursor = Cursors.UpArrow;
					}
					else if (m_mousePick.ToRoute != null)
					{
						m_dragToNode = m_mousePick.Node;
						m_dragToRoute = m_mousePick.ToRoute;
						m_dragEdgeReversed = false;
						m_isDragging = true;
						cursor = Cursors.UpArrow;
					}
					if (m_isDragging)
					{
						m_oldCursor = base.AdaptedControl.Cursor;
						base.AdaptedControl.Cursor = cursor;
						base.AdaptedControl.Capture = true;
						if (m_autoTranslateAdapter != null)
						{
							m_autoTranslateAdapter.Enabled = true;
						}
					}
				}
			}
		}
		if (!m_isDragging)
		{
			return;
		}
		m_edgeDragPoint = base.CurrentPoint;
		m_mousePick = m_graphAdapter.Pick(base.CurrentPoint);
		if (m_disconnectEdge != null)
		{
			m_graphAdapter.ResetStyle(m_disconnectEdge);
			m_disconnectEdge = null;
		}
		if (m_dragEdgeReversed)
		{
			if (CanConnectTo())
			{
				m_dragToNode = m_mousePick.Node;
				m_dragToRoute = m_mousePick.ToRoute;
				m_disconnectEdge = GetDisconnectEdgeTo();
				base.AdaptedControl.Cursor = Cursors.UpArrow;
			}
			else
			{
				m_dragToNode = null;
				m_dragToRoute = null;
				base.AdaptedControl.Cursor = Cursors.No;
			}
		}
		else if (CanConnectFrom())
		{
			m_dragFromNode = m_mousePick.Node;
			m_dragFromRoute = m_mousePick.FromRoute;
			m_disconnectEdge = GetDisconnectEdgeFrom();
			base.AdaptedControl.Cursor = Cursors.UpArrow;
		}
		else
		{
			m_dragFromNode = null;
			m_dragFromRoute = null;
			base.AdaptedControl.Cursor = Cursors.No;
		}
		if (m_disconnectEdge != null)
		{
			m_graphAdapter.SetStyle(m_disconnectEdge, DiagramDrawingStyle.Ghosted);
		}
		base.AdaptedControl.Invalidate();
	}

	protected override void OnMouseUp(object sender, MouseEventArgs e)
	{
		base.OnMouseUp(sender, e);
		if (e.Button == MouseButtons.Left && m_isDragging)
		{
			if (m_existingEdge != null)
			{
				m_graphAdapter.ResetStyle(m_existingEdge);
			}
			if (m_disconnectEdge != null)
			{
				m_graphAdapter.ResetStyle(m_disconnectEdge);
			}
			if (m_existingEdge == null || m_existingEdge.ToNode != m_dragToNode || m_existingEdge.ToRoute != m_dragToRoute || m_existingEdge.FromNode != m_dragFromNode || m_existingEdge.FromRoute != m_dragFromRoute)
			{
				ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
				context.DoTransaction(delegate
				{
					if (m_disconnectEdge != null)
					{
						m_editableGraph.Disconnect(m_disconnectEdge);
					}
					if (m_existingEdge != null)
					{
						m_editableGraph.Disconnect(m_existingEdge);
					}
					if (m_dragToNode != null && m_dragToRoute != null && m_dragFromNode != null && m_dragFromRoute != null)
					{
						m_editableGraph.Connect(m_dragFromNode, m_dragFromRoute, m_dragToNode, m_dragToRoute, m_existingEdge);
					}
				}, "Drag Edge".Localize());
			}
			base.AdaptedControl.Invalidate();
		}
		if (m_autoTranslateAdapter != null)
		{
			m_autoTranslateAdapter.Enabled = false;
		}
		m_isDragging = false;
		m_dragFromNode = null;
		m_dragFromRoute = null;
		m_dragToNode = null;
		m_dragToRoute = null;
		m_existingEdge = null;
		m_disconnectEdge = null;
		base.AdaptedControl.Cursor = m_oldCursor;
	}

	private bool CanConnectTo()
	{
		return m_mousePick.Node != null && m_mousePick.ToRoute != null && m_editableGraph != null && m_editableGraph.CanConnect(m_dragFromNode, m_dragFromRoute, m_mousePick.Node, m_mousePick.ToRoute);
	}

	private bool CanConnectFrom()
	{
		return m_mousePick.Node != null && m_mousePick.FromRoute != null && m_editableGraph != null && m_editableGraph.CanConnect(m_mousePick.Node, m_mousePick.FromRoute, m_dragToNode, m_dragToRoute);
	}

	private TEdge GetDisconnectEdgeTo()
	{
		TEdge val = null;
		if (!m_mousePick.ToRoute.AllowFanIn)
		{
			val = GetFirstEdgeTo(m_mousePick.Node, m_mousePick.ToRoute);
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
			val = GetFirstEdgeFrom(m_mousePick.Node, m_mousePick.FromRoute);
			if (!CanDisconnect(val))
			{
				val = null;
			}
		}
		return val;
	}

	private bool CanDisconnect(TEdge edge)
	{
		return edge != m_existingEdge && m_editableGraph.CanDisconnect(edge);
	}

	private TEdge GetFirstEdgeTo(TNode node, object toRoute)
	{
		foreach (TEdge edge in m_graph.Edges)
		{
			if (edge.ToNode == node && edge.ToRoute.Equals(toRoute))
			{
				return edge;
			}
		}
		return null;
	}

	private TEdge GetFirstEdgeFrom(TNode node, object fromRoute)
	{
		foreach (TEdge edge in m_graph.Edges)
		{
			if (edge.FromNode == node && edge.FromRoute.Equals(fromRoute))
			{
				return edge;
			}
		}
		return null;
	}
}
