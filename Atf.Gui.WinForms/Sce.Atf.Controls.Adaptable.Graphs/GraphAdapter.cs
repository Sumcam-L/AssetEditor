using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class GraphAdapter<TNode, TEdge, TEdgeRoute> : ControlAdapter, IGraphAdapter<TNode, TEdge, TEdgeRoute>, IPickingAdapter, IPrintingAdapter, IDisposable where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	private class EmptyGraph : IGraph<TNode, TEdge, TEdgeRoute>
	{
		public IEnumerable<TNode> Nodes => EmptyEnumerable<TNode>.Instance;

		public IEnumerable<TEdge> Edges => EmptyEnumerable<TEdge>.Instance;
	}

	private readonly GraphRenderer<TNode, TEdge, TEdgeRoute> m_renderer;

	private readonly ITransformAdapter m_transformAdapter;

	private IGraph<TNode, TEdge, TEdgeRoute> m_graph = s_emptyGraph;

	private IObservableContext m_observableContext;

	private ISelectionContext m_selectionContext;

	private IVisibilityContext m_visibilityContext;

	private Point m_cachedHitPoint;

	private GraphHitRecord<TNode, TEdge, TEdgeRoute> m_cachedHitRecord;

	private readonly Dictionary<object, DiagramDrawingStyle> m_styles = new Dictionary<object, DiagramDrawingStyle>();

	private static readonly EmptyGraph s_emptyGraph = new EmptyGraph();

	public GraphRenderer<TNode, TEdge, TEdgeRoute> Renderer => m_renderer;

	public GraphAdapter(GraphRenderer<TNode, TEdge, TEdgeRoute> renderer, ITransformAdapter transformAdapter)
	{
		m_renderer = renderer;
		m_renderer.Redraw += renderer_Redraw;
		m_transformAdapter = transformAdapter;
	}

	public void Dispose()
	{
		m_renderer.Redraw -= renderer_Redraw;
	}

	public Rectangle GetBounds(TNode node)
	{
		using Graphics graphics = base.AdaptedControl.CreateGraphics();
		graphics.Transform = m_transformAdapter.Transform;
		return m_renderer.GetBounds(node, graphics);
	}

	public Rectangle GetBounds(IEnumerable<TNode> nodes)
	{
		using Graphics graphics = base.AdaptedControl.CreateGraphics();
		graphics.Transform = m_transformAdapter.Transform;
		return m_renderer.GetBounds(nodes, graphics);
	}

	public void Frame(TNode node)
	{
		Rectangle bounds = GetBounds(node);
		m_transformAdapter.Frame(bounds);
	}

	public void Frame(IEnumerable<TNode> nodes)
	{
		Rectangle bounds = GetBounds(nodes);
		m_transformAdapter.Frame(bounds);
	}

	public void EnsureVisible(IEnumerable<TNode> nodes)
	{
		Rectangle bounds = GetBounds(nodes);
		m_transformAdapter.EnsureVisible(bounds);
	}

	public GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(Point p)
	{
		if (m_cachedHitRecord == null || p != m_cachedHitPoint)
		{
			m_cachedHitPoint = p;
			TEdge priorityEdge = null;
			if (m_selectionContext != null)
			{
				priorityEdge = m_selectionContext.GetLastSelected<TEdge>();
			}
			p = GdiUtil.InverseTransform(m_transformAdapter.Transform, p);
			using Graphics g = base.AdaptedControl.CreateGraphics();
			m_cachedHitRecord = m_renderer.Pick(m_graph, priorityEdge, p, g);
		}
		return m_cachedHitRecord;
	}

	public IEnumerable<TNode> Pick(Region pickRegion)
	{
		return Pick<TNode>(pickRegion);
	}

	public virtual IEnumerable<T> Pick<T>(Region pickRegion) where T : class
	{
		List<TNode> list = new List<TNode>();
		using (Graphics g = base.AdaptedControl.CreateGraphics())
		{
			RectangleF bounds = pickRegion.GetBounds(g);
			bounds = GdiUtil.InverseTransform(m_transformAdapter.Transform, bounds);
			foreach (TNode node in m_graph.Nodes)
			{
				if (((RectangleF)m_renderer.GetBounds(node, g)).IntersectsWith(bounds))
				{
					list.Add(node);
				}
			}
		}
		return list.AsIEnumerable<T>();
	}

	public void SetStyle(object item, DiagramDrawingStyle style)
	{
		m_styles[item] = style;
	}

	public void ResetStyle(object item)
	{
		m_styles.Remove(item);
	}

	public DiagramDrawingStyle GetStyle(object item)
	{
		if (!m_styles.TryGetValue(item, out var value))
		{
			if (m_visibilityContext != null && !m_visibilityContext.IsVisible(item))
			{
				value = DiagramDrawingStyle.Hidden;
			}
			if (m_selectionContext != null && m_selectionContext.SelectionContains(item))
			{
				value = ((!m_selectionContext.LastSelected.Equals(item)) ? DiagramDrawingStyle.Selected : DiagramDrawingStyle.LastSelected);
			}
		}
		return value;
	}

	DiagramHitRecord IPickingAdapter.Pick(Point pickPoint)
	{
		return Pick(pickPoint);
	}

	IEnumerable<object> IPickingAdapter.Pick(Region pickRegion)
	{
		return Pick<object>(pickRegion);
	}

	Rectangle IPickingAdapter.GetBounds(IEnumerable<object> items)
	{
		return GetBounds(items.AsIEnumerable<TNode>());
	}

	void IPrintingAdapter.Print(PrintDocument printDocument, Graphics g)
	{
		PrintRange printRange = printDocument.PrinterSettings.PrintRange;
		if (printRange == PrintRange.Selection)
		{
			PrintSelection(g);
		}
		else
		{
			PrintAll(g);
		}
	}

	private void PrintSelection(Graphics g)
	{
		if (m_selectionContext == null)
		{
			return;
		}
		HashSet<TNode> hashSet = new HashSet<TNode>();
		foreach (TNode node in m_graph.Nodes)
		{
			m_renderer.Draw(node, DiagramDrawingStyle.Normal, g);
			hashSet.Add(node);
		}
		foreach (TEdge edge in m_graph.Edges)
		{
			if (hashSet.Contains(edge.FromNode) && hashSet.Contains(edge.ToNode))
			{
				m_renderer.Draw(edge, DiagramDrawingStyle.Normal, g);
			}
		}
	}

	private void PrintAll(Graphics g)
	{
		foreach (TNode node in m_graph.Nodes)
		{
			m_renderer.Draw(node, DiagramDrawingStyle.Normal, g);
		}
		foreach (TEdge edge in m_graph.Edges)
		{
			m_renderer.Draw(edge, DiagramDrawingStyle.Normal, g);
		}
	}

	protected override void Bind(AdaptableControl control)
	{
		control.ContextChanged += control_ContextChanged;
		control.Paint += control_Paint;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.ContextChanged -= control_ContextChanged;
		control.Paint -= control_Paint;
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

	private void control_Paint(object sender, PaintEventArgs e)
	{
		Matrix transform = m_transformAdapter.Transform;
		Matrix transform2 = e.Graphics.Transform;
		Region clip = e.Graphics.Clip;
		e.Graphics.Transform = transform;
		Rectangle clip2 = GdiUtil.InverseTransform(e.Graphics.Transform, e.ClipRectangle);
		e.Graphics.SetClip(clip2);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		foreach (TNode node in m_graph.Nodes)
		{
			DiagramDrawingStyle style = GetStyle(node);
			m_renderer.Draw(node, style, e.Graphics);
		}
		foreach (TEdge edge in m_graph.Edges)
		{
			DiagramDrawingStyle style2 = GetStyle(edge);
			m_renderer.Draw(edge, style2, e.Graphics);
		}
		e.Graphics.Transform = transform2;
		e.Graphics.Clip = clip;
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void graph_ObjectInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		Invalidate();
	}

	private void graph_ObjectRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		Invalidate();
	}

	private void graph_ObjectChanged(object sender, ItemChangedEventArgs<object> e)
	{
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
		m_cachedHitRecord = null;
		base.AdaptedControl.Invalidate();
	}
}
