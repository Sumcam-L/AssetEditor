using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class D2dSubgraphAdapter<TNode, TEdge, TEdgeRoute> : D2dGraphAdapter<TNode, TEdge, TEdgeRoute> where TNode : class, ICircuitElement where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, ICircuitPin
{
	private D2dGraphics m_d2dGraphics;

	private readonly D2dSubCircuitRenderer<TNode, TEdge, TEdgeRoute> m_renderer;

	private readonly ITransformAdapter m_transformAdapter;

	private IGraph<TNode, TEdge, TEdgeRoute> m_graph = D2dGraphAdapter<TNode, TEdge, TEdgeRoute>.s_emptyGraph;

	private D2dSolidColorBrush m_scaleBrush;

	private D2dTextFormat m_textFormat;

	public D2dSubgraphAdapter(D2dSubCircuitRenderer<TNode, TEdge, TEdgeRoute> renderer, ITransformAdapter transformAdapter)
		: base((D2dGraphRenderer<TNode, TEdge, TEdgeRoute>)renderer, transformAdapter)
	{
		m_renderer = renderer;
		m_transformAdapter = transformAdapter;
	}

	protected override void Bind(AdaptableControl control)
	{
		base.Bind(control);
		D2dAdaptableControl d2dAdaptableControl = control as D2dAdaptableControl;
		m_d2dGraphics = d2dAdaptableControl.D2dGraphics;
		d2dAdaptableControl.ContextChanged += control_ContextChanged;
		m_scaleBrush = D2dFactory.CreateSolidBrush(control.ForeColor);
		m_textFormat = D2dFactory.CreateTextFormat(d2dAdaptableControl.Font);
	}

	protected override void Unbind(AdaptableControl control)
	{
		D2dAdaptableControl d2dAdaptableControl = control as D2dAdaptableControl;
		d2dAdaptableControl.ContextChanged -= control_ContextChanged;
		base.Unbind(control);
		m_scaleBrush.Dispose();
		m_textFormat.Dispose();
		m_scaleBrush = null;
		m_textFormat = null;
		m_d2dGraphics = null;
	}

	protected override void OnRender()
	{
		if (m_graph == D2dGraphAdapter<TNode, TEdge, TEdgeRoute>.s_emptyGraph)
		{
			return;
		}
		base.OnRender();
		m_renderer.VisibleWorldBounds = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.AdaptedControl.ClientRectangle);
		ICircuitGroupType<TNode, TEdge, TEdgeRoute> circuitGroupType = m_graph.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
		foreach (ICircuitPin item in circuitGroupType.Inputs.Concat(circuitGroupType.Info.HiddenInputPins))
		{
			ICircuitGroupPin<TNode> circuitGroupPin = item.Cast<ICircuitGroupPin<TNode>>();
			DiagramDrawingStyle style = GetStyle(circuitGroupPin);
			m_renderer.DrawFloatingGroupPin(circuitGroupPin, inputSide: true, style, m_d2dGraphics);
		}
		foreach (ICircuitPin item2 in circuitGroupType.Outputs.Concat(circuitGroupType.Info.HiddenOutputPins))
		{
			ICircuitGroupPin<TNode> circuitGroupPin2 = item2.Cast<ICircuitGroupPin<TNode>>();
			DiagramDrawingStyle style2 = GetStyle(circuitGroupPin2);
			m_renderer.DrawFloatingGroupPin(circuitGroupPin2, inputSide: false, style2, m_d2dGraphics);
		}
	}

	public override IEnumerable<object> Pick(Rectangle pickRect)
	{
		Matrix3x2F invXform = Matrix3x2F.Invert(m_d2dGraphics.Transform);
		RectangleF rect = D2dUtil.Transform(invXform, pickRect);
		IEnumerable<object> pickedGraphNodes = base.Pick(pickRect);
		foreach (object item in pickedGraphNodes)
		{
			yield return item;
		}
		List<object> pickedFloatingPins = new List<object>();
		ICircuitElementType circuiElement = m_graph.Cast<ICircuitElementType>();
		foreach (ICircuitPin pin in circuiElement.Inputs)
		{
			ICircuitGroupPin<TNode> grpPIn = pin.Cast<ICircuitGroupPin<TNode>>();
			if (m_renderer.GetBounds(grpPIn, inputSide: true, m_d2dGraphics).IntersectsWith(rect))
			{
				pickedFloatingPins.Add(pin);
			}
		}
		foreach (ICircuitPin pin2 in circuiElement.Outputs)
		{
			ICircuitGroupPin<TNode> grpPIn2 = pin2.Cast<ICircuitGroupPin<TNode>>();
			if (m_renderer.GetBounds(grpPIn2, inputSide: false, m_d2dGraphics).IntersectsWith(rect))
			{
				pickedFloatingPins.Add(pin2);
			}
		}
		foreach (object item2 in pickedFloatingPins)
		{
			yield return item2;
		}
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		IGraph<TNode, TEdge, TEdgeRoute> graph = base.AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
		if (graph == null)
		{
			graph = D2dGraphAdapter<TNode, TEdge, TEdgeRoute>.s_emptyGraph;
		}
		m_graph = graph;
	}
}
