using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class ViewingContext : Validator, IViewingContext, ILayoutContext
{
	private IGraph<IGraphNode, IGraphEdge<IGraphNode, IEdgeRoute>, IEdgeRoute> m_graph;

	private ICircuitContainer m_graphContainer;

	private AdaptableControl m_control;

	private IEnumerable<ILayoutConstraint> m_layoutConstraints;

	private D2dGraphNodeEditAdapter<Element, Wire, ICircuitPin> m_moduleEditAdapter;

	public AdaptableControl Control
	{
		get
		{
			return m_control;
		}
		set
		{
			if (m_control != null)
			{
				m_control.SizeChanged -= control_SizeChanged;
				m_control.VisibleChanged -= control_VisibleChanged;
			}
			m_control = value;
			m_layoutConstraints = EmptyEnumerable<ILayoutConstraint>.Instance;
			m_moduleEditAdapter = null;
			if (m_control != null)
			{
				m_layoutConstraints = m_control.AsAll<ILayoutConstraint>();
				m_moduleEditAdapter = m_control.As<D2dGraphNodeEditAdapter<Element, Wire, ICircuitPin>>();
				m_control.SizeChanged += control_SizeChanged;
				m_control.VisibleChanged += control_VisibleChanged;
			}
			SetCanvasBounds();
		}
	}

	protected override void OnNodeSet()
	{
		m_graph = base.DomNode.As<IGraph<IGraphNode, IGraphEdge<IGraphNode, IEdgeRoute>, IEdgeRoute>>();
		m_graphContainer = base.DomNode.As<ICircuitContainer>();
		base.OnNodeSet();
	}

	public override object GetAdapter(Type type)
	{
		if (type == typeof(AdaptableControl))
		{
			return Control;
		}
		return base.GetAdapter(type);
	}

	public Rectangle GetBounds(object item)
	{
		return GetBounds(new object[1] { item });
	}

	public Rectangle GetBounds(IEnumerable<object> items)
	{
		Rectangle rectangle = default(Rectangle);
		foreach (IPickingAdapter2 item in m_control.AsAll<IPickingAdapter2>())
		{
			Rectangle bounds = item.GetBounds(items);
			if (!bounds.IsEmpty)
			{
				rectangle = ((!rectangle.IsEmpty) ? Rectangle.Union(rectangle, bounds) : bounds);
			}
		}
		return rectangle;
	}

	public Rectangle GetBounds()
	{
		List<object> list = new List<object>();
		if (m_graphContainer != null)
		{
			list.AddRange(m_graphContainer.Elements.AsIEnumerable<object>());
			if (m_graphContainer.Annotations != null)
			{
				list.AddRange(m_graphContainer.Annotations.AsIEnumerable<object>());
			}
		}
		else if (m_graph != null)
		{
			list.AddRange(m_graph.Nodes.AsIEnumerable<IGraphNode>().AsIEnumerable<object>());
		}
		Rectangle rectangle = GetBounds(list);
		if (base.DomNode.Is<Group>())
		{
			Group obj = base.DomNode.Cast<Group>();
			int num = int.MaxValue;
			int num2 = int.MinValue;
			foreach (GroupPin inputGroupPin in obj.InputGroupPins)
			{
				GroupPin groupPin = inputGroupPin.Cast<GroupPin>();
				if (groupPin.Bounds.Location.Y < num)
				{
					num = groupPin.Bounds.Location.Y;
				}
				if (groupPin.Bounds.Location.Y > num2)
				{
					num2 = groupPin.Bounds.Location.Y;
				}
			}
			foreach (GroupPin outputGroupPin in obj.OutputGroupPins)
			{
				GroupPin groupPin2 = outputGroupPin.Cast<GroupPin>();
				if (groupPin2.Bounds.Location.Y < num)
				{
					num = groupPin2.Bounds.Location.Y;
				}
				if (groupPin2.Bounds.Location.Y > num2)
				{
					num2 = groupPin2.Bounds.Location.Y;
				}
			}
			if (num != int.MaxValue && num2 != int.MinValue)
			{
				ITransformAdapter transformAdapter = m_control.Cast<ITransformAdapter>();
				PointF pointF = D2dUtil.TransformVector(transformAdapter.Transform, new PointF(num, num2));
				num = (int)Math.Min(pointF.X, pointF.Y);
				num2 = (int)Math.Max(pointF.X, pointF.Y);
				int width = rectangle.Width;
				int height = num2 - num + 1;
				rectangle = Rectangle.Union(rectangle, new Rectangle(rectangle.Location.X, num, width, height));
			}
		}
		return rectangle;
	}

	public IEnumerable<object> GetVisibleItems()
	{
		Rectangle windowBounds = m_control.As<ICanvasAdapter>().WindowBounds;
		foreach (IPickingAdapter2 pickingAdapter in m_control.AsAll<IPickingAdapter2>())
		{
			foreach (object item in pickingAdapter.Pick(windowBounds))
			{
				yield return item;
			}
		}
	}

	public bool CanFrame(IEnumerable<object> items)
	{
		return m_control.As<IViewingContext>().CanFrame(items);
	}

	public void Frame(IEnumerable<object> items)
	{
		m_control.As<IViewingContext>().Frame(items);
	}

	public bool CanEnsureVisible(IEnumerable<object> items)
	{
		return m_control.As<IViewingContext>().CanFrame(items);
	}

	public void EnsureVisible(IEnumerable<object> items)
	{
		m_control.As<IViewingContext>().EnsureVisible(items);
	}

	BoundsSpecified ILayoutContext.GetBounds(object item, out Rectangle bounds)
	{
		Element element = item.As<Element>();
		if (element != null)
		{
			bounds = GetBounds(element);
			ITransformAdapter transformAdapter = m_control.Cast<ITransformAdapter>();
			bounds = GdiUtil.InverseTransform(transformAdapter.Transform, bounds);
			return BoundsSpecified.All;
		}
		Annotation annotation = item.As<Annotation>();
		if (annotation != null)
		{
			bounds = annotation.Bounds;
			return BoundsSpecified.All;
		}
		bounds = default(Rectangle);
		return BoundsSpecified.None;
	}

	BoundsSpecified ILayoutContext.CanSetBounds(object item)
	{
		if (item.Is<Group>())
		{
			return BoundsSpecified.All;
		}
		if (item.Is<Element>())
		{
			return BoundsSpecified.Location;
		}
		if (item.Is<Annotation>())
		{
			return BoundsSpecified.All;
		}
		return BoundsSpecified.None;
	}

	void ILayoutContext.SetBounds(object item, Rectangle bounds, BoundsSpecified specified)
	{
		Element element = item.As<Element>();
		if (!IsDraggingSubNode(element))
		{
			bounds = ConstrainBounds(bounds, specified);
		}
		if (element != null)
		{
			element.Bounds = WinFormsUtil.UpdateBounds(element.Bounds, bounds, specified);
			return;
		}
		Annotation annotation = item.As<Annotation>();
		if (annotation != null)
		{
			annotation.Bounds = WinFormsUtil.UpdateBounds(annotation.Bounds, bounds, specified);
		}
	}

	private bool IsDraggingSubNode(Element element)
	{
		if (element != null && m_moduleEditAdapter != null && m_moduleEditAdapter.NodeDraggingPosition(element).HasValue && element.DomNode.Parent.Is<Group>())
		{
			return true;
		}
		return false;
	}

	private Rectangle ConstrainBounds(Rectangle bounds, BoundsSpecified specified)
	{
		if (m_layoutConstraints != null)
		{
			foreach (ILayoutConstraint layoutConstraint in m_layoutConstraints)
			{
				if (layoutConstraint.Enabled)
				{
					bounds = layoutConstraint.Constrain(bounds, specified);
				}
			}
		}
		return bounds;
	}

	protected override void OnEnded(object sender, EventArgs e)
	{
		SetCanvasBounds();
		base.OnEnded(sender, e);
	}

	private void control_VisibleChanged(object sender, EventArgs e)
	{
		SetCanvasBounds();
	}

	private void control_SizeChanged(object sender, EventArgs e)
	{
		SetCanvasBounds();
	}

	protected virtual void SetCanvasBounds()
	{
		if (m_control != null && m_control.Visible)
		{
			Rectangle bounds = GetBounds();
			ITransformAdapter transformAdapter = m_control.As<ITransformAdapter>();
			bounds = GdiUtil.InverseTransform(transformAdapter.Transform, bounds);
			Rectangle rectangle = GdiUtil.InverseTransform(transformAdapter.Transform, m_control.ClientRectangle);
			bounds.Width = Math.Max(bounds.Width * 2, rectangle.Width * 2);
			bounds.Height = Math.Max(bounds.Height * 2, rectangle.Height * 2);
			ICanvasAdapter canvasAdapter = m_control.As<ICanvasAdapter>();
			if (canvasAdapter != null)
			{
				canvasAdapter.Bounds = bounds;
			}
		}
	}
}
