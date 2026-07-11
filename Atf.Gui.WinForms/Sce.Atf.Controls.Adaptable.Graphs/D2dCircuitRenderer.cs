using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;
using Sce.Atf.Rendering;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class D2dCircuitRenderer<TElement, TWire, TPin> : D2dGraphRenderer<TElement, TWire, TPin>, IDisposable where TElement : class, ICircuitElement where TWire : class, IGraphEdge<TElement, TPin> where TPin : class, ICircuitPin
{
	public enum PinStyle
	{
		Default,
		OnBorderFilled
	}

	protected class ElementTypeInfo
	{
		public string Title;

		public int TitleLength;

		public float TitleWidth;

		public float ContentWidth;

		public Size Size;

		public Rectangle Interior;

		public int[] OutputLeftX;

		public int numInputs;

		public int numOutputs;

		public ElementTypeInfo()
		{
		}

		public ElementTypeInfo(ElementTypeInfo orig)
		{
			Title = orig.Title;
			TitleLength = orig.TitleLength;
			TitleWidth = orig.TitleWidth;
			ContentWidth = orig.ContentWidth;
			Size = orig.Size;
			Interior = orig.Interior;
			OutputLeftX = new int[orig.OutputLeftX.Length];
			Array.Copy(orig.OutputLeftX, OutputLeftX, orig.OutputLeftX.Length);
			numInputs = orig.numInputs;
			numOutputs = orig.numOutputs;
		}
	}

	public class ElementSizeInfo
	{
		private readonly Size m_size;

		private readonly Rectangle m_interior;

		private readonly IEnumerable<int> m_outputLeftX;

		public float TitleWidth { get; set; }

		public float ContentWidth { get; set; }

		public Size Size => m_size;

		public Rectangle Interior => m_interior;

		public IEnumerable<int> OutputLeftX => m_outputLeftX;

		public ElementSizeInfo(Size size, Rectangle interior, IEnumerable<int> outputLeftX)
		{
			m_size = size;
			m_interior = interior;
			m_outputLeftX = outputLeftX;
		}
	}

	private class GraphHelper
	{
		private class ElementHelper
		{
			private readonly ICircuitGroupType<TElement, TWire, TPin> m_owningGroup;

			private readonly List<TWire> m_inputWires = new List<TWire>();

			private readonly List<TWire> m_outputWires = new List<TWire>();

			public IList<TWire> InputWires => m_inputWires;

			public IList<TWire> OutputWires => m_outputWires;

			public ICircuitGroupType<TElement, TWire, TPin> OwningGroup => m_owningGroup;

			public ElementHelper(ICircuitGroupType<TElement, TWire, TPin> owningGroup)
			{
				m_owningGroup = owningGroup;
			}
		}

		private readonly Dictionary<TElement, ElementHelper> m_elementToHelper = new Dictionary<TElement, ElementHelper>();

		public IGraph<TElement, TWire, TPin> Graph { get; private set; }

		public GraphHelper(IGraph<TElement, TWire, TPin> graph)
		{
			Graph = graph;
		}

		public ICircuitGroupType<TElement, TWire, TPin> GetOwningGroup(TElement element)
		{
			RebuildIfDirty();
			if (m_elementToHelper.TryGetValue(element, out var value))
			{
				return value.OwningGroup;
			}
			return null;
		}

		public IList<TWire> GetInputWires(TElement element)
		{
			RebuildIfDirty();
			if (m_elementToHelper.TryGetValue(element, out var value))
			{
				return value.InputWires;
			}
			return EmptyArray<TWire>.Instance;
		}

		public IList<TWire> GetOutputWires(TElement element)
		{
			RebuildIfDirty();
			if (m_elementToHelper.TryGetValue(element, out var value))
			{
				return value.OutputWires;
			}
			return EmptyArray<TWire>.Instance;
		}

		public void Invalidate()
		{
			m_elementToHelper.Clear();
		}

		private void RebuildIfDirty()
		{
			if (m_elementToHelper.Count != 0 || Graph == null)
			{
				return;
			}
			foreach (TElement node in Graph.Nodes)
			{
				AddElementsRecursively(node, null);
			}
			foreach (TWire edge in Graph.Edges)
			{
				AddWire(edge);
			}
			foreach (TElement node2 in Graph.Nodes)
			{
				AddWiresOfGroups(node2);
			}
		}

		private void AddElementsRecursively(TElement element, ICircuitGroupType<TElement, TWire, TPin> owningGroup)
		{
			ElementHelper value = new ElementHelper(owningGroup);
			m_elementToHelper[element] = value;
			ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.As<ICircuitGroupType<TElement, TWire, TPin>>();
			if (circuitGroupType == null)
			{
				return;
			}
			foreach (TElement subNode in circuitGroupType.SubNodes)
			{
				AddElementsRecursively(subNode, circuitGroupType);
			}
		}

		private void AddWiresOfGroups(TElement element)
		{
			ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.As<ICircuitGroupType<TElement, TWire, TPin>>();
			if (circuitGroupType == null)
			{
				return;
			}
			foreach (TWire subEdge in circuitGroupType.SubEdges)
			{
				AddWire(subEdge);
			}
			foreach (TElement subNode in circuitGroupType.SubNodes)
			{
				AddWiresOfGroups(subNode);
			}
		}

		private void AddWire(TWire wire)
		{
			ElementHelper elementHelper = m_elementToHelper[wire.ToNode];
			elementHelper.InputWires.Add(wire);
			ICircuitGroupPin<TElement> circuitGroupPin = wire.ToRoute.As<ICircuitGroupPin<TElement>>();
			if (circuitGroupPin != null)
			{
				do
				{
					TElement internalElement = circuitGroupPin.InternalElement;
					m_elementToHelper[internalElement].InputWires.Add(wire);
					circuitGroupPin = internalElement.Type.GetInputPin(circuitGroupPin.InternalPinIndex).As<ICircuitGroupPin<TElement>>();
				}
				while (circuitGroupPin != null);
			}
			elementHelper = m_elementToHelper[wire.FromNode];
			elementHelper.OutputWires.Add(wire);
			circuitGroupPin = wire.FromRoute.As<ICircuitGroupPin<TElement>>();
			if (circuitGroupPin != null)
			{
				do
				{
					TElement internalElement2 = circuitGroupPin.InternalElement;
					m_elementToHelper[internalElement2].OutputWires.Add(wire);
					circuitGroupPin = internalElement2.Type.GetOutputPin(circuitGroupPin.InternalPinIndex).As<ICircuitGroupPin<TElement>>();
				}
				while (circuitGroupPin != null);
			}
		}
	}

	protected const int ExpanderSize = 8;

	private GraphHelper m_graphHelper = new GraphHelper(null);

	private D2dBrush m_subGraphPinBrush;

	private D2dRoundedRect m_elementBody = default(D2dRoundedRect);

	private D2dDiagramTheme m_theme;

	private int m_rowSpacing;

	private int m_pinSize = 8;

	private int m_pinOffset;

	private int m_pinMargin = 2;

	private Point m_subContentOffset;

	private bool m_subContentOffseExternalSet;

	private int m_groupPinExpandedOffset;

	private Dictionary<ICircuitElementType, ElementTypeInfo> m_elementTypeCache = new Dictionary<ICircuitElementType, ElementTypeInfo>();

	private readonly Stack<TElement> m_graphPath = new Stack<TElement>();

	private readonly Dictionary<IDocument, Dictionary<ICircuitElementType, ElementTypeInfo>> m_cachePerDocument = new Dictionary<IDocument, Dictionary<ICircuitElementType, ElementTypeInfo>>();

	private readonly IDocumentRegistry m_documentRegistry;

	private PinStyle m_pinDrawStyle;

	private int m_maxCollapsedGroupPinNameLength;

	private int m_truncatedPinNameSubstringLength;

	private const int MinElementWidth = 4;

	private const int MinElementHeight = 4;

	private const int MaxNameOverhang = 64;

	public PinStyle PinDrawStyle
	{
		get
		{
			return m_pinDrawStyle;
		}
		set
		{
			m_pinDrawStyle = value;
		}
	}

	protected Dictionary<ICircuitElementType, ElementTypeInfo> ElementTypeCache => m_elementTypeCache;

	public bool TitleBackgroundFilled { get; set; }

	public bool RoundedBorder { get; set; }

	public int DetailsThresholdSize { get; set; }

	public bool RectangleSelectsWires { get; set; }

	public D2dDiagramTheme Theme
	{
		get
		{
			return m_theme;
		}
		set
		{
			if (m_theme != value)
			{
				if (m_theme != null)
				{
					m_theme.Redraw -= theme_Redraw;
				}
				SetPinSpacing();
				m_theme = value;
				if (m_theme != null)
				{
					m_theme.Redraw += theme_Redraw;
				}
			}
		}
	}

	public D2dBrush SubGraphPinBrush
	{
		get
		{
			return m_subGraphPinBrush;
		}
		set
		{
			m_subGraphPinBrush.Dispose();
			m_subGraphPinBrush = value;
		}
	}

	public Point SubContentOffset
	{
		get
		{
			return m_subContentOffset;
		}
		set
		{
			m_subContentOffset = value;
			m_subContentOffseExternalSet = true;
		}
	}

	public int GroupPinExpandedOffset
	{
		get
		{
			return m_groupPinExpandedOffset;
		}
		set
		{
			m_groupPinExpandedOffset = value;
		}
	}

	public int TitleHeight => m_rowSpacing + m_pinMargin;

	public int LabelHeight => m_rowSpacing + m_pinMargin;

	public Point CurrentWorldOffset => WorldOffset(m_graphPath);

	public int MaxCollapsedGroupPinNameLength
	{
		get
		{
			return m_maxCollapsedGroupPinNameLength;
		}
		set
		{
			if (value < 5)
			{
				throw new ArgumentOutOfRangeException("the minimum value is 5");
			}
			m_maxCollapsedGroupPinNameLength = value;
			m_truncatedPinNameSubstringLength = (value - 3) / 2;
		}
	}

	private int TitleBarPadding => 3 * m_pinMargin + 32 + 3;

	public D2dCircuitRenderer(D2dDiagramTheme theme, IDocumentRegistry documentRegistry = null)
	{
		m_theme = theme;
		m_theme.Redraw += theme_Redraw;
		SetPinSpacing();
		m_elementBody.RadiusX = 6f;
		m_elementBody.RadiusY = 6f;
		base.EdgeThickness = 2f;
		m_subGraphPinBrush = D2dFactory.CreateSolidBrush(Color.SandyBrown);
		MaxCollapsedGroupPinNameLength = 25;
		m_documentRegistry = documentRegistry;
		if (m_documentRegistry != null)
		{
			m_documentRegistry.DocumentRemoved += DocumentRegistryOnDocumentRemoved;
			m_documentRegistry.ActiveDocumentChanging += DocumentRegistryOnActiveDocumentChanging;
			m_documentRegistry.ActiveDocumentChanged += DocumentRegistryOnActiveDocumentChanged;
		}
		RoundedBorder = true;
	}

	protected virtual string GetElementTitle(TElement element)
	{
		return element.Type.Name;
	}

	protected virtual string GetElementDisplayName(TElement element)
	{
		return element.Name;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_theme.Redraw -= theme_Redraw;
			m_subGraphPinBrush.Dispose();
		}
		base.Dispose(disposing);
	}

	public override void OnGraphObjectChanged(object sender, ItemChangedEventArgs<object> e)
	{
		if (e.Item.Is<ICircuitElementType>())
		{
			Invalidate(e.Item.Cast<ICircuitElementType>());
		}
		m_graphHelper.Invalidate();
	}

	public override void OnGraphObjectRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		m_graphHelper.Invalidate();
	}

	public override void OnGraphObjectInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		m_graphHelper.Invalidate();
	}

	public void Invalidate(ICircuitElementType elementType)
	{
		m_elementTypeCache.Remove(elementType);
		OnRedraw();
	}

	public override void Draw(TElement element, DiagramDrawingStyle style, D2dGraphics g)
	{
		DiagramDrawingStyle customStyle = GetCustomStyle(element);
		if (customStyle != DiagramDrawingStyle.None)
		{
			style = customStyle;
		}
		switch (style)
		{
		case DiagramDrawingStyle.Normal:
			Draw(element, g, outline: false);
			break;
		default:
			DrawOutline(element, m_theme.GetOutLineBrush(style), g);
			Draw(element, g, outline: false);
			break;
		case DiagramDrawingStyle.Ghosted:
		case DiagramDrawingStyle.Hidden:
			DrawGhost(element, g);
			break;
		}
	}

	public override void Draw(TWire edge, DiagramDrawingStyle style, D2dGraphics g)
	{
		TPin toRoute = edge.ToRoute;
		TPin fromRoute = edge.FromRoute;
		if (RectangleSelectsWires && style == DiagramDrawingStyle.LastSelected)
		{
			style = DiagramDrawingStyle.Selected;
		}
		D2dBrush pen = ((style == DiagramDrawingStyle.Normal) ? GetPen(toRoute) : m_theme.GetOutLineBrush(style));
		if (edge.Is<IEdgeStyleProvider>())
		{
			DrawEdgeUsingStyleInfo(edge.Cast<IEdgeStyleProvider>(), pen, g);
		}
		else
		{
			DrawWire(edge.FromNode, fromRoute, edge.ToNode, toRoute, g, pen);
		}
	}

	private void DrawEdgeUsingStyleInfo(IEdgeStyleProvider edgeStyleProvider, D2dBrush pen, D2dGraphics g)
	{
		foreach (EdgeStyleData datum in edgeStyleProvider.GetData(this, WorldOffset(m_graphPath), g))
		{
			if (datum.ShapeType == EdgeStyleData.EdgeShape.Bezier)
			{
				BezierCurve2F bezierCurve2F = datum.EdgeData.As<BezierCurve2F>();
				g.DrawBezier(bezierCurve2F.P1, bezierCurve2F.P2, bezierCurve2F.P3, bezierCurve2F.P4, pen, datum.Thickness);
			}
			else if (datum.ShapeType == EdgeStyleData.EdgeShape.Line)
			{
				PointF[] array = datum.EdgeData.As<PointF[]>();
				if (array != null)
				{
					g.DrawLine(array[0], array[1], pen, datum.Thickness);
				}
			}
			else if (datum.ShapeType == EdgeStyleData.EdgeShape.Polyline)
			{
				PointF[] array2 = datum.EdgeData.As<PointF[]>();
				if (array2 != null)
				{
					g.DrawLines(array2, pen, datum.Thickness);
				}
			}
			else if (datum.ShapeType == EdgeStyleData.EdgeShape.BezierSpline)
			{
				IEnumerable<BezierCurve2F> enumerable = datum.EdgeData.As<IEnumerable<BezierCurve2F>>();
				foreach (BezierCurve2F item in enumerable)
				{
					g.DrawBezier(item.P1, item.P2, item.P3, item.P4, pen, datum.Thickness);
				}
			}
			else if (datum.ShapeType == EdgeStyleData.EdgeShape.None)
			{
				break;
			}
		}
	}

	public Point GetGroupPinPosition(ICircuitGroupType<TElement, TWire, TPin> group, ICircuitGroupPin<TElement> groupPin, bool inputSide, D2dGraphics g)
	{
		if (inputSide)
		{
			Point location = group.Bounds.Location;
			location.Y += GetPinOffset(group.Cast<TElement>(), groupPin.Index, inputSide: true);
			return location;
		}
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(group.Cast<TElement>(), g);
		Point location2 = group.Bounds.Location;
		location2.X += elementTypeInfo.Size.Width;
		location2.Y += GetPinOffset(group.Cast<TElement>(), groupPin.Index, inputSide: false);
		return location2;
	}

	public Point GetPinPosition(TElement element, int pinIndex, bool inputSide, D2dGraphics g)
	{
		if (inputSide)
		{
			Point location = element.Bounds.Location;
			location.Y += GetPinOffset(element, pinIndex, inputSide: true);
			if (m_pinDrawStyle == PinStyle.OnBorderFilled)
			{
				location.X -= m_pinSize / 2;
			}
			return location;
		}
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		Point location2 = element.Bounds.Location;
		location2.X += elementTypeInfo.Size.Width;
		if (m_pinDrawStyle == PinStyle.OnBorderFilled)
		{
			location2.X += m_pinSize / 2;
		}
		location2.Y += GetPinOffset(element, pinIndex, inputSide: false);
		return location2;
	}

	public override void Draw(TElement outputElement, TPin outputPin, TElement inputElement, TPin inputPin, string label, Point endPoint, D2dGraphics g)
	{
		if (inputElement == null || inputPin == null)
		{
			DrawWire(outputElement, outputPin, endPoint, fromOutput: true, g);
		}
		else if (outputElement == null || outputPin == null)
		{
			DrawWire(inputElement, inputPin, endPoint, fromOutput: false, g);
		}
		else
		{
			DrawWire(outputElement, outputPin, inputElement, inputPin, g, GetPen(outputPin));
		}
	}

	public override void DrawPartialEdge(TElement outputElement, TPin outputPin, TElement inputElement, TPin inputPin, string label, PointF startPoint, PointF endPoint, D2dGraphics g)
	{
		D2dBrush pen = ((outputPin != null) ? GetPen(outputPin) : GetPen(inputPin));
		Matrix3x2F transform = g.Transform;
		transform.Invert();
		PointF pointF = startPoint;
		PointF pointF2 = endPoint;
		if (outputPin == null)
		{
			pointF = Matrix3x2F.TransformPoint(transform, startPoint);
		}
		if (inputPin == null)
		{
			pointF2 = Matrix3x2F.TransformPoint(transform, endPoint);
		}
		DrawWire(g, pen, pointF.X, pointF.Y, pointF2.X, pointF2.Y, 0f, null);
		DrawWire(g, pen, pointF.X, pointF.Y, pointF2.X, pointF2.Y, 0f, null);
	}

	public override RectangleF GetBounds(TElement element, D2dGraphics g)
	{
		RectangleF elementBounds = GetElementBounds(element, g);
		if (!string.IsNullOrEmpty(element.Name))
		{
			elementBounds.Height += LabelHeight;
		}
		return elementBounds;
	}

	public override IEnumerable<object> Pick(IGraph<TElement, TWire, TPin> graph, RectangleF rect, D2dGraphics g)
	{
		object[] array = base.Pick(graph, rect, g).ToArray();
		if (!RectangleSelectsWires || array.Length != 0)
		{
			if (array.Length == 1 && array[0].Is<ICircuitGroupType<TElement, TWire, TPin>>())
			{
				ICircuitGroupType<TElement, TWire, TPin> pickedElement = array[0].Cast<ICircuitGroupType<TElement, TWire, TPin>>();
				TElement[] array2 = PickSubItems(pickedElement, rect, g).ToArray();
				if (array2.Length != 0)
				{
					return array2;
				}
			}
			return array;
		}
		List<object> list = new List<object>();
		if (RectangleSelectsWires)
		{
			foreach (TWire edge in graph.Edges)
			{
				if (GetBounds(edge, g).IntersectsWith(rect))
				{
					list.Add(edge);
				}
			}
		}
		return list;
	}

	public override GraphHitRecord<TElement, TWire, TPin> Pick(IGraph<TElement, TWire, TPin> graph, TWire priorityEdge, PointF p, D2dGraphics g)
	{
		return Pick(graph.Nodes.Reverse(), graph.Edges, priorityEdge, p, g);
	}

	public override GraphHitRecord<TElement, TWire, TPin> Pick(IEnumerable<TElement> nodes, IEnumerable<TWire> edges, TWire priorityEdge, PointF p, D2dGraphics g)
	{
		TElement val = null;
		TPin val2 = null;
		TPin val3 = null;
		TWire val4 = null;
		if (priorityEdge != null && PickEdge(priorityEdge, p, g))
		{
			val4 = priorityEdge;
		}
		else
		{
			foreach (TWire edge in edges)
			{
				if (PickEdge(edge, p, g))
				{
					val4 = edge;
					break;
				}
			}
		}
		Point point = default(Point);
		Point point2 = default(Point);
		foreach (TElement node in nodes)
		{
			if (!Pick(node, g, p))
			{
				continue;
			}
			if (val != null && node.Is<ICircuitGroupType<TElement, TWire, TPin>>())
			{
				bool flag = true;
				ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = node.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
				if (val.Is<ICircuitGroupType<TElement, TWire, TPin>>())
				{
					ICircuitGroupType<TElement, TWire, TPin> circuitGroupType2 = val.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
					if (circuitGroupType.Info.PickingPriority <= circuitGroupType2.Info.PickingPriority)
					{
						flag = false;
					}
				}
				if (circuitGroupType.Expanded && flag)
				{
					val = node;
					val2 = PickInput(node, g, p);
					val3 = PickOutput(node, g, p);
				}
			}
			else if (val == null)
			{
				val = node;
				val2 = PickInput(node, g, p);
				val3 = PickOutput(node, g, p);
			}
			if (val2 != null)
			{
				point = GetPinPosition(val, val2.Index, inputSide: true, g);
			}
			if (val3 != null)
			{
				point2 = GetPinPosition(val, val3.Index, inputSide: false, g);
			}
		}
		DiagramLabel diagramLabel = null;
		Pair<IEnumerable<TElement>, object> pair = default(Pair<IEnumerable<TElement>, object>);
		DiagramBorder diagramBorder = new DiagramBorder(val);
		TPin pickedSubInput = null;
		TPin pickedSubOutput = null;
		if (val != null)
		{
			RectangleF elementBounds = GetElementBounds(val, g);
			RectangleF rectangleF = new RectangleF(elementBounds.Left, elementBounds.Bottom + (float)m_pinMargin, elementBounds.Width, m_rowSpacing);
			GraphHitRecord<TElement, TWire, TPin> graphHitRecord = PickShowPinsToggle(val, p, g);
			if (graphHitRecord != null)
			{
				return graphHitRecord;
			}
			diagramLabel = new DiagramLabel(new Rectangle((int)rectangleF.Left, (int)rectangleF.Top, (int)rectangleF.Width, (int)rectangleF.Height), TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine);
			if (rectangleF.Contains(p))
			{
				return new GraphHitRecord<TElement, TWire, TPin>(val, diagramLabel);
			}
			if (val.Is<ICircuitGroupType<TElement, TWire, TPin>>())
			{
				graphHitRecord = PickExpander(val.Cast<ICircuitGroupType<TElement, TWire, TPin>>(), p, g);
				if (graphHitRecord != null)
				{
					return graphHitRecord;
				}
				if (new RectangleF(elementBounds.Left - (float)m_theme.PickTolerance, elementBounds.Y, elementBounds.Width, TitleHeight).Contains(p))
				{
					return new GraphHitRecord<TElement, TWire, TPin>(val, new DiagramTitleBar(val));
				}
				if (val4 == null && val3 == null && val2 == null)
				{
					RectangleF rectangleF2 = new RectangleF(elementBounds.Left - (float)m_theme.PickTolerance, elementBounds.Y, 2 * m_theme.PickTolerance, elementBounds.Height);
					if (rectangleF2.Contains(p))
					{
						diagramBorder.Border = DiagramBorder.BorderType.Left;
					}
					else
					{
						rectangleF2.Offset(elementBounds.Width, 0f);
						if (rectangleF2.Contains(p))
						{
							diagramBorder.Border = DiagramBorder.BorderType.Right;
						}
						else
						{
							rectangleF2 = new RectangleF(elementBounds.Left, elementBounds.Y - (float)m_theme.PickTolerance, elementBounds.Width, 2 * m_theme.PickTolerance);
							if (rectangleF2.Contains(p))
							{
								diagramBorder.Border = DiagramBorder.BorderType.Top;
							}
							else
							{
								rectangleF2.Offset(0f, elementBounds.Height);
								if (rectangleF2.Contains(p))
								{
									diagramBorder.Border = DiagramBorder.BorderType.Bottom;
								}
							}
						}
					}
				}
				pair = PickSubItem(val.Cast<ICircuitGroupType<TElement, TWire, TPin>>(), p, g, out pickedSubInput, out pickedSubOutput);
			}
		}
		if (pickedSubInput != null)
		{
			val2 = pickedSubInput;
			point = GetPinPosition(pair.First.First().Cast<TElement>(), pickedSubInput.Index, inputSide: true, g);
			point.Offset(ParentWorldOffset(pair.First));
		}
		if (pickedSubOutput != null)
		{
			val3 = pickedSubOutput;
			point2 = GetPinPosition(pair.First.First().Cast<TElement>(), pickedSubOutput.Index, inputSide: false, g);
			point2.Offset(ParentWorldOffset(pair.First));
		}
		if (val4 != null && val != null && val2 == null && val3 == null && pickedSubInput == null && pickedSubOutput == null)
		{
			RectangleF elementBounds2 = GetElementBounds(val, g);
			RectangleF wireBounds = GetWireBounds(val4, g);
			if (!elementBounds2.Contains(wireBounds))
			{
				return new GraphHitRecord<TElement, TWire, TPin>(null, val4, null, null);
			}
		}
		GraphHitRecord<TElement, TWire, TPin> graphHitRecord2 = new GraphHitRecord<TElement, TWire, TPin>(val, val4, val3, val2);
		if (val != null && val4 == null && val3 == null && val2 == null)
		{
			graphHitRecord2.Part = ((diagramBorder.Border == DiagramBorder.BorderType.None) ? null : diagramBorder);
		}
		graphHitRecord2.DefaultPart = diagramLabel;
		graphHitRecord2.SubItem = ((pair.First == null) ? null : pair.First.First());
		graphHitRecord2.SubPart = pair.Second;
		graphHitRecord2.ToRoutePos = point;
		graphHitRecord2.FromRoutePos = point2;
		if (pair.Second.Is<TWire>())
		{
			graphHitRecord2.SubItem = pair.Second;
		}
		graphHitRecord2.HitPathInversed = pair.First;
		return graphHitRecord2;
	}

	protected virtual bool PickEdge(TWire edge, PointF p, D2dGraphics g, float xOffset = 0f, float yOffset = 0f)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(edge.FromNode, g);
		TPin toRoute = edge.ToRoute;
		TPin fromRoute = edge.FromRoute;
		Point location = edge.FromNode.Bounds.Location;
		float num = (float)(location.X + elementTypeInfo.Size.Width) + xOffset;
		float y = (float)(location.Y + GetPinOffset(edge.FromNode, fromRoute.Index, inputSide: false)) + yOffset;
		Point location2 = edge.ToNode.Bounds.Location;
		float num2 = (float)location2.X + xOffset;
		float y2 = (float)(location2.Y + GetPinOffset(edge.ToNode, toRoute.Index, inputSide: true)) + yOffset;
		float tangentLength = GetTangentLength(num, num2);
		BezierCurve2F curve = new BezierCurve2F(new Vec2F(num, y), new Vec2F(num + tangentLength, y), new Vec2F(num2 - tangentLength, y2), new Vec2F(num2, y2));
		Vec2F hitPoint = default(Vec2F);
		return BezierCurve2F.Pick(curve, new Vec2F(p.X, p.Y), m_theme.PickTolerance, ref hitPoint);
	}

	private void theme_Redraw(object sender, EventArgs e)
	{
		m_elementTypeCache.Clear();
		OnRedraw();
	}

	private void Draw(TElement element, D2dGraphics g, bool outline)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		Point location = element.Bounds.Location;
		location.Offset(WorldOffset(m_graphPath));
		RectangleF rectangleF = new RectangleF(location, elementTypeInfo.Size);
		ICircuitElementType type = element.Type;
		bool flag = false;
		ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.As<ICircuitGroupType<TElement, TWire, TPin>>();
		if (circuitGroupType != null)
		{
			flag = circuitGroupType.Expanded;
		}
		if (base.RouteConnecting != null)
		{
			bool flag2 = false;
			IEnumerable<ICircuitPin> enumerable = (flag ? circuitGroupType.Inputs.Concat(circuitGroupType.Info.HiddenInputPins).Concat(circuitGroupType.Outputs).Concat(circuitGroupType.Info.HiddenOutputPins) : type.Inputs.Concat(type.Outputs));
			foreach (TPin item in enumerable)
			{
				EdgeRouteDrawMode edgeRouteDrawMode = GetEdgeRouteDrawMode(element, item);
				if (edgeRouteDrawMode != EdgeRouteDrawMode.CannotConnect)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				DrawGhost(element, g);
				if (!flag)
				{
					return;
				}
			}
		}
		float m = g.Transform.M11;
		bool flag3 = RoundedBorder && m * m_elementBody.RadiusX > 3f;
		bool flag4 = !flag && m * (float)m_pinSize > (float)DetailsThresholdSize;
		bool flag5 = m_theme.TextFormat.FontHeight * m > (float)DetailsThresholdSize;
		int titleHeight = TitleHeight;
		m_elementBody.Rect = rectangleF;
		RectangleF rect = new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, titleHeight);
		D2dBrush d2dBrush = m_theme.GetCustomOrDefaultBrush(type.Name);
		if (!element.ElementInfo.IsValid)
		{
			d2dBrush = m_theme.ErrorBrush;
		}
		D2dBrush fillTitleBrush = m_theme.GetFillTitleBrush(type.Name);
		if (d2dBrush is D2dLinearGradientBrush d2dLinearGradientBrush)
		{
			d2dLinearGradientBrush.StartPoint = rectangleF.Location;
			d2dLinearGradientBrush.EndPoint = new PointF(rectangleF.Left, rectangleF.Bottom);
		}
		if (flag3)
		{
			g.FillRoundedRectangle(m_elementBody, d2dBrush);
			if (TitleBackgroundFilled)
			{
				D2dRoundedRect elementBody = m_elementBody;
				elementBody.Rect = new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, titleHeight);
				if (fillTitleBrush is D2dLinearGradientBrush d2dLinearGradientBrush2)
				{
					d2dLinearGradientBrush2.StartPoint = elementBody.Rect.Location;
					d2dLinearGradientBrush2.EndPoint = new PointF(elementBody.Rect.Right, elementBody.Rect.Bottom);
				}
				g.FillRoundedRectangle(elementBody, fillTitleBrush);
				RectangleF rect2 = new RectangleF(rectangleF.X, rectangleF.Y + (float)titleHeight * 0.5f, rectangleF.Width, (float)titleHeight * 0.5f);
				g.FillRectangle(rect2, fillTitleBrush);
				g.DrawRoundedRectangle(m_elementBody, fillTitleBrush);
			}
			if (outline)
			{
				g.DrawRoundedRectangle(m_elementBody, m_theme.OutlineBrush);
			}
		}
		else
		{
			g.FillRectangle(rectangleF, d2dBrush);
			if (TitleBackgroundFilled)
			{
				if (fillTitleBrush is D2dLinearGradientBrush d2dLinearGradientBrush3)
				{
					d2dLinearGradientBrush3.StartPoint = rect.Location;
					d2dLinearGradientBrush3.EndPoint = new PointF(rect.Right, rect.Bottom);
				}
				g.FillRectangle(rect, fillTitleBrush);
				g.DrawRectangle(rectangleF, fillTitleBrush);
			}
			if (outline)
			{
				g.DrawRectangle(rectangleF, m_theme.OutlineBrush);
			}
		}
		if (!TitleBackgroundFilled && elementTypeInfo.Size.Height > TitleHeight + 2 * m_pinMargin)
		{
			g.DrawLine(location.X, location.Y + titleHeight, location.X + elementTypeInfo.Size.Width, location.Y + titleHeight, m_theme.OutlineBrush);
		}
		if (flag4)
		{
			IList<ICircuitPin> visibleInputPins = GetVisibleInputPins(element);
			IList<ICircuitPin> visibleOutputPins = GetVisibleOutputPins(element);
			int num = titleHeight + m_pinMargin;
			foreach (TPin item2 in visibleInputPins)
			{
				EdgeRouteDrawMode edgeRouteDrawMode2 = GetEdgeRouteDrawMode(element, item2);
				if (edgeRouteDrawMode2 != EdgeRouteDrawMode.CannotConnect)
				{
					D2dBrush pen = GetPen(item2);
					if (PinDrawStyle == PinStyle.Default)
					{
						g.DrawRectangle(new RectangleF(location.X, location.Y + num + m_pinOffset, m_pinSize, m_pinSize), pen);
					}
					else if (PinDrawStyle == PinStyle.OnBorderFilled)
					{
						g.FillRectangle(new RectangleF((float)location.X - (float)m_pinSize * 0.5f, location.Y + num + m_pinOffset, m_pinSize, m_pinSize), pen);
					}
					string text = item2.Name;
					if (!flag)
					{
						text = TruncatePinText(text);
					}
					g.DrawText(text, m_theme.TextFormat, new PointF(location.X + m_pinSize + m_pinMargin, location.Y + num), m_theme.TextBrush);
				}
				num += m_rowSpacing;
			}
			num = titleHeight + m_pinMargin;
			int num2 = 0;
			foreach (TPin item3 in visibleOutputPins)
			{
				EdgeRouteDrawMode edgeRouteDrawMode3 = GetEdgeRouteDrawMode(element, item3);
				if (edgeRouteDrawMode3 != EdgeRouteDrawMode.CannotConnect)
				{
					D2dBrush pen2 = GetPen(item3);
					if (PinDrawStyle == PinStyle.Default)
					{
						g.DrawRectangle(new RectangleF(rectangleF.Right - (float)m_pinSize, location.Y + num + m_pinOffset, m_pinSize, m_pinSize), pen2);
					}
					else if (PinDrawStyle == PinStyle.OnBorderFilled)
					{
						g.FillRectangle(new RectangleF(rectangleF.Right - (float)m_pinSize * 0.5f, location.Y + num + m_pinOffset, m_pinSize, m_pinSize), pen2);
					}
					string text2 = item3.Name;
					if (!flag)
					{
						text2 = TruncatePinText(text2);
					}
					g.DrawText(text2, m_theme.TextFormat, new PointF(location.X + elementTypeInfo.OutputLeftX[num2], location.Y + num), m_theme.TextBrush);
				}
				num += m_rowSpacing;
				num2++;
			}
		}
		Image image = type.Image;
		if (image != null)
		{
			D2dBitmap bitmap = m_theme.GetBitmap(type);
			if (bitmap == null)
			{
				m_theme.RegisterBitmap(type, image);
				bitmap = m_theme.GetBitmap(type);
			}
			if (bitmap != null)
			{
				g.DrawBitmap(bitmap, new RectangleF(location.X + elementTypeInfo.Interior.X, location.Y + elementTypeInfo.Interior.Y, elementTypeInfo.Interior.Width, elementTypeInfo.Interior.Height));
			}
		}
		if (flag5)
		{
			string elementTitle = GetElementTitle(element);
			if (!string.IsNullOrEmpty(elementTitle))
			{
				D2dTextAlignment textAlignment = m_theme.TextFormat.TextAlignment;
				D2dParagraphAlignment paragraphAlignment = m_theme.TextFormat.ParagraphAlignment;
				m_theme.TextFormat.TextAlignment = D2dTextAlignment.Center;
				m_theme.TextFormat.ParagraphAlignment = D2dParagraphAlignment.Center;
				g.DrawText(layoutRect: new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), text: elementTitle, textFormat: m_theme.TextFormat, brush: m_theme.TextBrush);
				m_theme.TextFormat.TextAlignment = textAlignment;
				m_theme.TextFormat.ParagraphAlignment = paragraphAlignment;
			}
			string elementDisplayName = GetElementDisplayName(element);
			if (!string.IsNullOrEmpty(elementDisplayName))
			{
				RectangleF layoutRect = new RectangleF(rectangleF.Left - 64f, rectangleF.Bottom + (float)m_pinMargin, rectangleF.Width + 128f, m_rowSpacing);
				D2dTextAlignment textAlignment2 = m_theme.TextFormat.TextAlignment;
				m_theme.TextFormat.TextAlignment = D2dTextAlignment.Center;
				g.DrawText(elementDisplayName, m_theme.TextFormat, layoutRect, m_theme.TextBrush);
				m_theme.TextFormat.TextAlignment = textAlignment2;
			}
		}
		if (element.Is<IReference<TElement>>())
		{
			g.DrawLink((float)location.X + rectangleF.Width - (float)m_pinMargin - 2f - 16f, location.Y + 2 * m_pinMargin + 1, 8f, m_theme.HotBrush);
		}
		if (circuitGroupType != null)
		{
			RectangleF expanderRect = GetExpanderRect(location);
			g.DrawExpander(expanderRect.X, expanderRect.Y, expanderRect.Width, m_theme.OutlineBrush, circuitGroupType.Expanded);
			if (circuitGroupType.Expanded)
			{
				DrawExpandedGroup(element, g);
			}
		}
		if (HasShowUnconnectedPinsToggle(element))
		{
			RectangleF showPinsRect = GetShowPinsRect(rectangleF);
			bool showUnconnectedPins = element.ElementInfo.ShowUnconnectedPins;
			g.DrawEyeIcon(showPinsRect, showUnconnectedPins ? m_theme.TextBrush : m_theme.HiddenBrush, 0.5f);
		}
	}

	private string TruncatePinText(string pinText)
	{
		if (pinText.Length < MaxCollapsedGroupPinNameLength)
		{
			return pinText;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(pinText.Substring(0, m_truncatedPinNameSubstringLength));
		stringBuilder.Append("...");
		stringBuilder.Append(pinText.Substring(pinText.Length - m_truncatedPinNameSubstringLength));
		return stringBuilder.ToString();
	}

	protected void DrawExpandedGroup(TElement element, D2dGraphics g)
	{
		m_graphPath.Push(element);
		ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
		DrawExpandedGroupPins(element, g);
		TElement[] array = circuitGroupType.SubNodes.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			TElement val = array[i];
			DiagramDrawingStyle customStyle = GetCustomStyle(val);
			if (customStyle == DiagramDrawingStyle.DropTarget)
			{
				array[i] = array[0];
				array[0] = val;
			}
		}
		TElement[] array2 = array;
		foreach (TElement val2 in array2)
		{
			DiagramDrawingStyle customStyle2 = GetCustomStyle(val2);
			if (customStyle2 == DiagramDrawingStyle.None)
			{
				if (GetStyle != null)
				{
					Draw(val2, GetStyle(val2), g);
				}
				else
				{
					Draw(val2, g, outline: false);
				}
			}
			else
			{
				Draw(val2, customStyle2, g);
			}
		}
		foreach (TWire subEdge in circuitGroupType.SubEdges)
		{
			DiagramDrawingStyle style = GetStyle(subEdge);
			Draw(subEdge, style, g);
		}
		m_graphPath.Pop();
	}

	public override EdgeRouteDrawMode GetEdgeRouteDrawMode(TElement element, TPin pin)
	{
		RouteConnectingInfo routeConnecting = base.RouteConnecting;
		if (routeConnecting != null)
		{
			if (routeConnecting.StartNode == element && routeConnecting.StartRoute == pin)
			{
				return EdgeRouteDrawMode.CanConnect;
			}
			return (EdgeRouteDrawMode)(routeConnecting.EditableGraph.CanConnect(routeConnecting.StartNode, routeConnecting.StartRoute, element, pin) ? EdgeRouteDrawMode.CanConnect : EdgeRouteDrawMode.CannotConnect);
		}
		return EdgeRouteDrawMode.Normal;
	}

	protected virtual bool Pick(TElement element, D2dGraphics g, PointF p)
	{
		RectangleF bounds = GetBounds(element, g);
		int pickTolerance = m_theme.PickTolerance;
		bounds.Inflate(pickTolerance, pickTolerance);
		return bounds.Contains(p.X, p.Y);
	}

	private void DrawExpandedGroupPins(TElement element, D2dGraphics g)
	{
		ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
		if (!circuitGroupType.Info.ShowExpandedGroupPins)
		{
			return;
		}
		Point point = SubGraphOffset(element);
		foreach (ICircuitPin input in circuitGroupType.Inputs)
		{
			ICircuitGroupPin<TElement> circuitGroupPin = input.Cast<ICircuitGroupPin<TElement>>();
			Point location = circuitGroupType.Bounds.Location;
			location.Offset(ParentWorldOffset(m_graphPath));
			int x = location.X;
			int num = location.Y + circuitGroupPin.Bounds.Location.Y + m_groupPinExpandedOffset + point.Y;
			g.DrawRectangle(new RectangleF(x - m_pinSize / 2, num - m_pinSize / 2, m_pinSize, m_pinSize), circuitGroupPin.Info.Color);
			if (!circuitGroupPin.Info.ExternalConnected && CircuitDefaultStyle.ShowVirtualLinks)
			{
				Point location2 = circuitGroupPin.InternalElement.Bounds.Location;
				location2.Offset(WorldOffset(m_graphPath));
				int num2 = location2.Y + GetPinOffset(circuitGroupPin.InternalElement, circuitGroupPin.InternalPinIndex, inputSide: true);
				int x2 = location2.X;
				DrawWire(g, SubGraphPinBrush, x, num, x2, num2, 1f, null);
			}
		}
		foreach (ICircuitPin output in circuitGroupType.Outputs)
		{
			ICircuitGroupPin<TElement> circuitGroupPin2 = output.Cast<ICircuitGroupPin<TElement>>();
			ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
			Point location3 = circuitGroupType.Bounds.Location;
			location3.Offset(ParentWorldOffset(m_graphPath));
			int x = location3.X + elementTypeInfo.Size.Width;
			int num = location3.Y + circuitGroupPin2.Bounds.Location.Y + m_groupPinExpandedOffset + point.Y;
			g.DrawRectangle(new RectangleF(x - m_pinSize / 2, num - m_pinSize / 2, m_pinSize, m_pinSize), circuitGroupPin2.Info.Color);
			if (!circuitGroupPin2.Info.ExternalConnected && CircuitDefaultStyle.ShowVirtualLinks)
			{
				elementTypeInfo = GetElementTypeInfo(circuitGroupPin2.InternalElement, g);
				Point location4 = circuitGroupPin2.InternalElement.Bounds.Location;
				location4.Offset(WorldOffset(m_graphPath));
				int num3 = location4.Y + GetPinOffset(circuitGroupPin2.InternalElement, circuitGroupPin2.InternalPinIndex, inputSide: false);
				int num4 = location4.X + elementTypeInfo.Size.Width;
				DrawWire(g, SubGraphPinBrush, num4, num3, x, num, 1f, null);
			}
		}
	}

	private void DrawGhost(TElement element, D2dGraphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		Point location = element.Bounds.Location;
		location.Offset(WorldOffset(m_graphPath));
		Rectangle rectangle = new Rectangle(location, elementTypeInfo.Size);
		m_elementBody.Rect = rectangle;
		if (RoundedBorder)
		{
			g.FillRoundedRectangle(m_elementBody, m_theme.GhostBrush);
		}
		else
		{
			g.FillRectangle(rectangle, m_theme.GhostBrush);
		}
	}

	private void DrawOutline(TElement element, D2dBrush pen, D2dGraphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		Point location = element.Bounds.Location;
		RectangleF rect = new RectangleF(location, elementTypeInfo.Size);
		rect.Offset(WorldOffset(m_graphPath));
		rect.Inflate(1f, 1f);
		float m = g.Transform.M11;
		if (RoundedBorder && m * m_elementBody.RadiusX > 3f)
		{
			m_elementBody.Rect = rect;
			g.DrawRoundedRectangle(m_elementBody, pen, 2f);
		}
		else
		{
			g.DrawRectangle(rect, pen, 2f);
		}
	}

	private TPin PickInput(TElement element, D2dGraphics g, PointF p)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		return PickPin(element, inputSide: true, element.Bounds.X, element.Bounds.Location.Y, elementTypeInfo, p);
	}

	private TPin PickOutput(TElement element, D2dGraphics g, PointF p)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		int pinX = ((base.RouteConnecting != null) ? (element.Bounds.X + elementTypeInfo.Size.Width / 2) : ((PinDrawStyle != PinStyle.OnBorderFilled) ? (element.Bounds.X + elementTypeInfo.Size.Width - m_pinSize) : (element.Bounds.X + elementTypeInfo.Size.Width - m_pinSize / 2)));
		return PickPin(element, inputSide: false, pinX, element.Bounds.Location.Y, elementTypeInfo, p);
	}

	private TPin PickPin(TElement element, bool inputSide, int pinX, int elementY, ElementTypeInfo info, PointF p)
	{
		IList<ICircuitPin> visibleInputPins = GetVisibleInputPins(element);
		IList<ICircuitPin> visibleOutputPins = GetVisibleOutputPins(element);
		IList<ICircuitPin> list;
		IList<ICircuitPin> list2;
		if (inputSide)
		{
			list = visibleInputPins;
			list2 = element.Type.Inputs;
		}
		else
		{
			list = visibleOutputPins;
			list2 = element.Type.Outputs;
		}
		int pickTolerance = m_theme.PickTolerance;
		if (base.RouteConnecting == null || (element.Is<ICircuitGroupType<TElement, TWire, TPin>>() && element.Cast<ICircuitGroupType<TElement, TWire, TPin>>().Expanded))
		{
			int num = 0;
			int num2 = 0;
			foreach (TPin item in list2)
			{
				if (num2 == list.Count)
				{
					break;
				}
				if (item == list[num2])
				{
					num2++;
					int num3 = elementY + GetPinOffset(element, num, inputSide);
					RectangleF rectangleF = new RectangleF(pinX, num3, m_pinSize, m_pinSize);
					rectangleF.Inflate(pickTolerance, pickTolerance);
					if (rectangleF.Contains(p.X, p.Y))
					{
						return item;
					}
				}
				num++;
			}
		}
		else
		{
			int num4 = 0;
			int num5 = 0;
			foreach (TPin item2 in list2)
			{
				if (num5 == list.Count)
				{
					break;
				}
				if (item2 == list[num5])
				{
					num5++;
					int num6 = elementY + GetPinOffset(element, num4, inputSide);
					EdgeRouteDrawMode edgeRouteDrawMode = GetEdgeRouteDrawMode(element, item2);
					if (edgeRouteDrawMode == EdgeRouteDrawMode.CanConnect)
					{
						RectangleF rectangleF2 = new RectangleF(pinX, num6, info.Size.Width, m_pinSize);
						rectangleF2.Inflate(pickTolerance, pickTolerance);
						if (rectangleF2.Contains(p.X, p.Y))
						{
							return item2;
						}
					}
				}
				num4++;
			}
		}
		return null;
	}

	private GraphHitRecord<TElement, TWire, TPin> PickExpander(ICircuitGroupType<TElement, TWire, TPin> pickedElement, PointF p, D2dGraphics g)
	{
		Stack<TElement> stack = new Stack<TElement>();
		stack.Push(pickedElement.Cast<TElement>());
		bool flag = false;
		do
		{
			TElement val = stack.Peek();
			Point location = val.Bounds.Location;
			location.Offset(ParentWorldOffset(stack));
			RectangleF expanderRect = GetExpanderRect(location);
			expanderRect.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
			if (expanderRect.Contains(p))
			{
				DiagramExpander part = new DiagramExpander(expanderRect);
				GraphHitRecord<TElement, TWire, TPin> graphHitRecord = new GraphHitRecord<TElement, TWire, TPin>(pickedElement.Cast<TElement>(), part);
				if (stack.Count > 1)
				{
					graphHitRecord.SubItem = val;
					graphHitRecord.HitPathInversed = stack;
				}
				return graphHitRecord;
			}
			ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = val.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
			flag = false;
			foreach (ICircuitGroupType<TElement, TWire, TPin> item in circuitGroupType.SubNodes.AsIEnumerable<ICircuitGroupType<TElement, TWire, TPin>>())
			{
				TElement node = item.Cast<TElement>();
				RectangleF bounds = GetBounds(node, g);
				bounds.Offset(WorldOffset(stack));
				bounds.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
				if (bounds.Contains(p.X, p.Y))
				{
					stack.Push(item.Cast<TElement>());
					flag = true;
					break;
				}
			}
		}
		while (flag);
		return null;
	}

	private GraphHitRecord<TElement, TWire, TPin> PickShowPinsToggle(TElement pickedElement, PointF p, D2dGraphics g)
	{
		Stack<TElement> stack = new Stack<TElement>();
		stack.Push(pickedElement);
		bool flag;
		do
		{
			TElement val = stack.Peek();
			RectangleF bounds = GetBounds(val, g);
			bounds.Offset(ParentWorldOffset(stack));
			ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = val.As<ICircuitGroupType<TElement, TWire, TPin>>();
			if (HasShowUnconnectedPinsToggle(val))
			{
				RectangleF showPinsRect = GetShowPinsRect(bounds);
				showPinsRect.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
				if (showPinsRect.Contains(p))
				{
					ShowPinsToggle part = new ShowPinsToggle(showPinsRect);
					GraphHitRecord<TElement, TWire, TPin> graphHitRecord = new GraphHitRecord<TElement, TWire, TPin>(pickedElement, part);
					if (stack.Count > 1)
					{
						graphHitRecord.SubItem = val;
						graphHitRecord.HitPathInversed = stack;
					}
					return graphHitRecord;
				}
			}
			flag = false;
			if (circuitGroupType == null)
			{
				continue;
			}
			foreach (TElement subNode in circuitGroupType.SubNodes)
			{
				RectangleF bounds2 = GetBounds(subNode, g);
				bounds2.Offset(WorldOffset(stack));
				bounds2.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
				if (bounds2.Contains(p.X, p.Y))
				{
					stack.Push(subNode.Cast<TElement>());
					flag = true;
					break;
				}
			}
		}
		while (flag);
		return null;
	}

	private Pair<IEnumerable<TElement>, object> PickSubItem(ICircuitGroupType<TElement, TWire, TPin> pickedElement, PointF p, D2dGraphics g, out TPin pickedSubInput, out TPin pickedSubOutput)
	{
		pickedSubInput = null;
		pickedSubOutput = null;
		Pair<IEnumerable<TElement>, object> result = default(Pair<IEnumerable<TElement>, object>);
		if (!pickedElement.Expanded)
		{
			return result;
		}
		Stack<TElement> stack = new Stack<TElement>();
		stack.Push(pickedElement.Cast<TElement>());
		Stack<TElement> stack2 = new Stack<TElement>();
		stack2.Push(pickedElement.Cast<TElement>());
		TWire val = null;
		bool flag;
		do
		{
			TElement reference = stack.Peek();
			ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = reference.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
			flag = false;
			bool flag2 = false;
			PointF pos = WorldOffset(stack2);
			foreach (TElement subNode in circuitGroupType.SubNodes)
			{
				RectangleF bounds = GetBounds(subNode, g);
				bounds.Offset(pos);
				bounds.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
				if (bounds.Contains(p.X, p.Y))
				{
					stack.Push(subNode.Cast<TElement>());
					flag2 = true;
					if (subNode.Is<ICircuitGroupType<TElement, TWire, TPin>>())
					{
						stack2.Push(subNode.Cast<TElement>());
						flag = subNode.Cast<ICircuitGroupType<TElement, TWire, TPin>>().Expanded;
						break;
					}
				}
			}
			if (flag2)
			{
				continue;
			}
			foreach (TWire subEdge in circuitGroupType.SubEdges)
			{
				if (PickEdge(subEdge, p, g, pos.X, pos.Y))
				{
					val = subEdge;
					break;
				}
			}
		}
		while (flag);
		if (val != null)
		{
			result.First = stack;
			result.Second = val;
		}
		else if (stack.Peek() != pickedElement.Cast<TElement>())
		{
			TElement val2 = stack.Peek();
			result.First = stack;
			RectangleF elementBounds = GetElementBounds(val2, g);
			elementBounds.Offset(ParentWorldOffset(stack));
			ElementTypeInfo elementTypeInfo = GetElementTypeInfo(val2, g);
			if (PinDrawStyle == PinStyle.OnBorderFilled)
			{
				pickedSubInput = PickPin(val2, inputSide: true, (int)elementBounds.X - m_pinSize / 2, (int)elementBounds.Y, elementTypeInfo, p);
				pickedSubOutput = PickPin(val2, inputSide: false, (int)elementBounds.X + elementTypeInfo.Size.Width - m_pinSize / 2, (int)elementBounds.Y, elementTypeInfo, p);
			}
			else
			{
				pickedSubInput = PickPin(val2, inputSide: true, (int)elementBounds.X, (int)elementBounds.Y, elementTypeInfo, p);
				pickedSubOutput = PickPin(val2, inputSide: false, (int)elementBounds.X + elementTypeInfo.Size.Width - m_pinSize, (int)elementBounds.Y, elementTypeInfo, p);
			}
			result.Second = pickedSubInput ?? pickedSubOutput;
			if (result.Second == null)
			{
				RectangleF rectangleF = new RectangleF(elementBounds.Left - (float)m_theme.PickTolerance, elementBounds.Y, 2 * m_theme.PickTolerance, elementBounds.Height);
				if (rectangleF.Contains(p))
				{
					result.Second = new DiagramBorder(val2, DiagramBorder.BorderType.Left);
				}
				else
				{
					rectangleF.Offset(elementBounds.Width, 0f);
					if (rectangleF.Contains(p))
					{
						result.Second = new DiagramBorder(val2, DiagramBorder.BorderType.Right);
					}
					else
					{
						rectangleF = new RectangleF(elementBounds.Left, elementBounds.Y - (float)m_theme.PickTolerance, elementBounds.Width, 2 * m_theme.PickTolerance);
						if (rectangleF.Contains(p))
						{
							result.Second = new DiagramBorder(val2, DiagramBorder.BorderType.Top);
						}
						else
						{
							rectangleF.Offset(0f, elementBounds.Height);
							if (rectangleF.Contains(p))
							{
								result.Second = new DiagramBorder(val2, DiagramBorder.BorderType.Bottom);
							}
						}
					}
				}
			}
		}
		return result;
	}

	private IEnumerable<TElement> PickSubItems(ICircuitGroupType<TElement, TWire, TPin> pickedElement, RectangleF rect, D2dGraphics g)
	{
		if (!pickedElement.Expanded)
		{
			return EmptyEnumerable<TElement>.Instance;
		}
		Stack<TElement> stack = new Stack<TElement>();
		stack.Push(pickedElement.Cast<TElement>());
		Stack<TElement> stack2 = new Stack<TElement>();
		stack2.Push(pickedElement.Cast<TElement>());
		bool flag;
		do
		{
			TElement reference = stack.Pop();
			ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = reference.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
			ICircuitGroupType<TElement, TWire, TPin> reference2 = null;
			flag = false;
			int num = 0;
			foreach (TElement subNode in circuitGroupType.SubNodes)
			{
				IVisible visible = subNode.As<IVisible>();
				if (visible != null && !visible.Visible)
				{
					continue;
				}
				RectangleF bounds = GetBounds(subNode, g);
				bounds.Offset(WorldOffset(stack2));
				bounds.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
				if (!bounds.IntersectsWith(rect))
				{
					continue;
				}
				stack.Push(subNode.Cast<TElement>());
				if (subNode.Is<ICircuitGroupType<TElement, TWire, TPin>>())
				{
					stack2.Push(subNode.Cast<TElement>());
					if (subNode.Cast<ICircuitGroupType<TElement, TWire, TPin>>().Expanded)
					{
						reference2 = subNode.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
						num++;
					}
				}
			}
			if (num == 1)
			{
				stack.Clear();
				stack.Push(reference2.Cast<TElement>());
				flag = true;
			}
		}
		while (flag);
		return stack;
	}

	protected ElementTypeInfo GetElementTypeInfo(TElement element, D2dGraphics g)
	{
		ICircuitElementType type = element.Type;
		string elementTitle = GetElementTitle(element);
		ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.As<ICircuitGroupType<TElement, TWire, TPin>>();
		if (m_elementTypeCache.TryGetValue(type, out var value))
		{
			if (value.numInputs == type.Inputs.Count && value.numOutputs == type.Outputs.Count)
			{
				if (elementTitle.Length > value.TitleLength)
				{
					float num = (float)(elementTitle.Length - value.TitleLength + 1) * value.TitleWidth / (float)value.TitleLength;
					float num2 = value.TitleWidth + num + (float)TitleBarPadding;
					if (num2 > value.ContentWidth)
					{
						ElementTypeInfo elementTypeInfo = new ElementTypeInfo(value);
						elementTypeInfo.Size.Width = (int)num2;
						for (int i = 0; i < elementTypeInfo.OutputLeftX.Length; i++)
						{
							elementTypeInfo.OutputLeftX[i] += (int)(num2 - (float)value.Size.Width);
						}
						value = elementTypeInfo;
					}
				}
				else if (elementTitle.Length < value.TitleLength)
				{
					float num3 = (float)(value.TitleLength - elementTitle.Length) * value.TitleWidth / (float)value.TitleLength;
					float num4 = value.TitleWidth - num3 + (float)TitleBarPadding;
					if (num4 > value.ContentWidth)
					{
						ElementTypeInfo elementTypeInfo2 = new ElementTypeInfo(value);
						elementTypeInfo2.Size.Width = (int)num4;
						for (int j = 0; j < elementTypeInfo2.OutputLeftX.Length; j++)
						{
							elementTypeInfo2.OutputLeftX[j] -= (int)((float)value.Size.Width - num4);
						}
						value = elementTypeInfo2;
					}
				}
				if (circuitGroupType == null)
				{
					IList<ICircuitPin> visibleInputPins = GetVisibleInputPins(element);
					IList<ICircuitPin> visibleOutputPins = GetVisibleOutputPins(element);
					if (visibleInputPins.Count != type.Inputs.Count || visibleOutputPins.Count != type.Outputs.Count)
					{
						value = new ElementTypeInfo(value);
						int num5 = value.Size.Width - value.Interior.Right;
						int left = value.Interior.Left;
						value.Size.Height = CalculateElementHeight(visibleInputPins.Count, visibleOutputPins.Count, type, num5 < left);
						value.Interior.Y = value.Size.Height - type.InteriorSize.Height;
					}
				}
				return value;
			}
			Invalidate(type);
		}
		if (circuitGroupType != null)
		{
			return GetHierarchicalElementTypeInfo(circuitGroupType, g);
		}
		ElementSizeInfo elementSizeInfo = GetElementSizeInfo(type, g, elementTitle);
		ElementTypeInfo elementTypeInfo3 = new ElementTypeInfo
		{
			TitleLength = elementTitle.Length,
			TitleWidth = elementSizeInfo.TitleWidth,
			ContentWidth = elementSizeInfo.ContentWidth,
			Size = elementSizeInfo.Size,
			Interior = elementSizeInfo.Interior,
			OutputLeftX = elementSizeInfo.OutputLeftX.ToArray(),
			numInputs = type.Inputs.Count,
			numOutputs = type.Outputs.Count
		};
		m_elementTypeCache.Add(type, elementTypeInfo3);
		return elementTypeInfo3;
	}

	protected virtual ElementSizeInfo GetElementSizeInfo(ICircuitElementType type, D2dGraphics g, string title = null)
	{
		SizeF sizeF = default(SizeF);
		if (title != null)
		{
			sizeF = g.MeasureText(title, m_theme.TextFormat);
		}
		else
		{
			g.MeasureText(type.Name, m_theme.TextFormat);
		}
		int val = (int)sizeF.Width + TitleBarPadding;
		bool flag = type.Is<ICircuitGroupType<TElement, TWire, TPin>>();
		IList<ICircuitPin> list;
		IList<ICircuitPin> list2;
		if (flag)
		{
			TElement element = type.Cast<TElement>();
			list = GetVisibleInputPins(element);
			list2 = GetVisibleOutputPins(element);
		}
		else
		{
			list = type.Inputs;
			list2 = type.Outputs;
		}
		int count = list.Count;
		int count2 = list2.Count;
		int[] array = new int[count2];
		int num = Math.Max(count, count2);
		bool flag2 = true;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			num2 = 2 * m_pinMargin;
			if (count > i)
			{
				string text = list[i].Name;
				if (flag)
				{
					text = TruncatePinText(text);
				}
				num2 += g.MeasureText(text, m_theme.TextFormat).Width + (float)m_pinSize + (float)m_pinMargin;
			}
			else
			{
				flag2 = false;
			}
			if (count2 > i)
			{
				string text2 = list2[i].Name;
				if (flag)
				{
					text2 = TruncatePinText(text2);
				}
				SizeF sizeF2 = g.MeasureText(text2, m_theme.TextFormat);
				array[i] = (int)sizeF2.Width;
				num2 += sizeF2.Width + (float)m_pinSize + (float)m_pinMargin;
			}
			num2 += (float)type.InteriorSize.Width;
			val = Math.Max(val, (int)num2);
		}
		if (count == count2)
		{
			val = Math.Max(val, type.InteriorSize.Width + 2);
		}
		val = Math.Max(val, 4);
		int num3 = CalculateElementHeight(count, count2, type, flag2);
		Size size = new Size(val, num3);
		Rectangle interior = new Rectangle((!flag2) ? 1 : (val - type.InteriorSize.Width), num3 - type.InteriorSize.Height, type.InteriorSize.Width, type.InteriorSize.Height);
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = val - m_pinMargin - m_pinSize - array[j];
		}
		ElementSizeInfo elementSizeInfo = new ElementSizeInfo(size, interior, array);
		elementSizeInfo.TitleWidth = sizeF.Width;
		elementSizeInfo.ContentWidth = num2;
		return elementSizeInfo;
	}

	private int CalculateElementHeight(int inputCount, int outputCount, ICircuitElementType type, bool imageRight)
	{
		int num;
		int num2;
		if (imageRight)
		{
			num = outputCount;
			num2 = inputCount;
		}
		else
		{
			num = inputCount;
			num2 = outputCount;
		}
		int num3 = m_rowSpacing + 2 * m_pinMargin;
		num3 += Math.Max(num2 * m_rowSpacing, num * m_rowSpacing + type.InteriorSize.Height - m_pinMargin);
		return Math.Max(num3, 4);
	}

	private ElementTypeInfo GetHierarchicalElementTypeInfo(ICircuitGroupType<TElement, TWire, TPin> group, D2dGraphics g)
	{
		ElementSizeInfo hierarchicalElementSizeInfo = GetHierarchicalElementSizeInfo(group, g);
		ElementTypeInfo elementTypeInfo = new ElementTypeInfo
		{
			TitleLength = GetElementTitle(group.As<TElement>()).Length,
			Size = hierarchicalElementSizeInfo.Size,
			Interior = hierarchicalElementSizeInfo.Interior,
			OutputLeftX = hierarchicalElementSizeInfo.OutputLeftX.ToArray(),
			numInputs = group.Inputs.Count,
			numOutputs = group.Outputs.Count
		};
		m_elementTypeCache.Add(group, elementTypeInfo);
		return elementTypeInfo;
	}

	protected virtual ElementSizeInfo GetHierarchicalElementSizeInfo(ICircuitGroupType<TElement, TWire, TPin> group, D2dGraphics g)
	{
		string elementTitle = GetElementTitle(group.As<TElement>());
		if (!group.Expanded)
		{
			return GetElementSizeInfo(group, g, elementTitle);
		}
		Rectangle a;
		if (group.AutoSize)
		{
			a = default(Rectangle);
			foreach (TElement subNode in group.SubNodes)
			{
				ElementSizeInfo elementSizeInfo = (subNode.Is<ICircuitGroupType<TElement, TWire, TPin>>() ? GetHierarchicalElementSizeInfo(subNode.Cast<ICircuitGroupType<TElement, TWire, TPin>>(), g) : GetElementSizeInfo(subNode.Type, g, GetElementTitle(subNode)));
				if (a.Width == 0 && a.Height == 0)
				{
					a.Location = subNode.Bounds.Location;
					a.Size = elementSizeInfo.Size;
				}
				else
				{
					a = Rectangle.Union(a, new Rectangle(subNode.Bounds.Location, elementSizeInfo.Size));
				}
			}
			if (a.Width == 0)
			{
				a.Width = (int)g.MeasureText(elementTitle, m_theme.TextFormat).Width + TitleBarPadding;
			}
			int num = int.MaxValue;
			int num2 = int.MinValue;
			foreach (ICircuitPin input in group.Inputs)
			{
				ICircuitGroupPin<TElement> circuitGroupPin = input.Cast<ICircuitGroupPin<TElement>>();
				if (circuitGroupPin.Bounds.Location.Y < num)
				{
					num = circuitGroupPin.Bounds.Location.Y;
				}
				if (circuitGroupPin.Bounds.Location.Y > num2)
				{
					num2 = circuitGroupPin.Bounds.Location.Y;
				}
			}
			if (num == int.MaxValue)
			{
				num = a.Y;
			}
			foreach (ICircuitPin output in group.Outputs)
			{
				ICircuitGroupPin<TElement> circuitGroupPin2 = output.Cast<ICircuitGroupPin<TElement>>();
				if (circuitGroupPin2.Bounds.Location.Y < num)
				{
					num = circuitGroupPin2.Bounds.Location.Y;
				}
				if (circuitGroupPin2.Bounds.Location.Y > num2)
				{
					num2 = circuitGroupPin2.Bounds.Location.Y;
				}
			}
			if (num2 == int.MinValue)
			{
				num2 = a.Y + a.Height;
			}
			int num3 = a.Width + m_subContentOffset.X;
			int num4 = Math.Max(num2 - num, a.Height);
			num4 += LabelHeight;
			num4 += TitleHeight;
			if (group.Info.MinimumSize.Width > num3)
			{
				num3 = group.Info.MinimumSize.Width;
			}
			if (group.Info.MinimumSize.Height > num4)
			{
				num4 = group.Info.MinimumSize.Height;
			}
			a = Rectangle.Union(a, new Rectangle(a.Location.X, num, num3, num4));
		}
		else
		{
			a = group.Bounds;
		}
		int count = group.Outputs.Count;
		int[] array = new int[count];
		for (int i = 0; i < array.Length; i++)
		{
			SizeF sizeF = g.MeasureText(group.Outputs[i].Name, m_theme.TextFormat);
			array[i] = a.Size.Width - m_pinMargin - m_pinSize - (int)sizeF.Width;
		}
		if (group.AutoSize)
		{
			bool flag = group.Info.MinimumSize.IsEmpty;
			bool flag2 = flag;
			if (!group.Info.MinimumSize.IsEmpty)
			{
				if (group.Info.MinimumSize.Width >= a.Size.Width)
				{
					flag = false;
				}
				if (group.Info.MinimumSize.Height >= a.Size.Height)
				{
					flag2 = false;
				}
			}
			if (flag || flag2)
			{
				return new ElementSizeInfo(new Size(a.Size.Width + (flag ? m_subContentOffset.X : 0), a.Size.Height + (flag2 ? m_subContentOffset.Y : 0)), a, array);
			}
			return new ElementSizeInfo(a.Size, a, array);
		}
		return new ElementSizeInfo(a.Size, new Rectangle(0, 0, 0, 0), array);
	}

	private IList<ICircuitPin> GetVisibleInputPins(ICircuitElement element)
	{
		ICircuitElementType type = element.Type;
		IList<ICircuitPin> list = type.Inputs;
		if (!element.ElementInfo.ShowUnconnectedPins)
		{
			list = new List<ICircuitPin>();
			IList<TWire> inputWires = m_graphHelper.GetInputWires((TElement)element);
			foreach (ICircuitPin input in element.Type.Inputs)
			{
				int num = inputWires.Count;
				while (--num >= 0)
				{
					if (inputWires[num].ToRoute == input)
					{
						list.Add(input);
						break;
					}
					ICircuitGroupType<TElement, TWire, TPin> owningGroup = m_graphHelper.GetOwningGroup((TElement)element);
					if (owningGroup == null)
					{
						continue;
					}
					bool flag = false;
					IList<ICircuitPin> inputs = owningGroup.Inputs;
					int num2 = inputs.Count;
					while (--num2 >= 0)
					{
						ICircuitGroupPin<TElement> circuitGroupPin = (ICircuitGroupPin<TElement>)inputs[num2];
						if (circuitGroupPin.InternalElement.Type.GetInputPin(circuitGroupPin.InternalPinIndex) == input)
						{
							list.Add(input);
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	private IList<ICircuitPin> GetVisibleOutputPins(ICircuitElement element)
	{
		ICircuitElementType type = element.Type;
		IList<ICircuitPin> list = type.Outputs;
		if (!element.ElementInfo.ShowUnconnectedPins)
		{
			list = new List<ICircuitPin>();
			IList<TWire> outputWires = m_graphHelper.GetOutputWires((TElement)element);
			foreach (ICircuitPin output in element.Type.Outputs)
			{
				int num = outputWires.Count;
				while (--num >= 0)
				{
					if (outputWires[num].FromRoute == output)
					{
						list.Add(output);
						break;
					}
					ICircuitGroupType<TElement, TWire, TPin> owningGroup = m_graphHelper.GetOwningGroup((TElement)element);
					if (owningGroup == null)
					{
						continue;
					}
					bool flag = false;
					IList<ICircuitPin> outputs = owningGroup.Outputs;
					int num2 = outputs.Count;
					while (--num2 >= 0)
					{
						ICircuitGroupPin<TElement> circuitGroupPin = (ICircuitGroupPin<TElement>)outputs[num2];
						if (circuitGroupPin.InternalElement.Type.GetOutputPin(circuitGroupPin.InternalPinIndex) == output)
						{
							list.Add(output);
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	private void DrawWire(TElement outputElement, TPin outputPin, TElement inputElement, TPin inputPin, D2dGraphics g, D2dBrush pen)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(outputElement, g);
		Point location = outputElement.Bounds.Location;
		location.Offset(WorldOffset(m_graphPath));
		int num = location.X + elementTypeInfo.Size.Width;
		if (PinDrawStyle == PinStyle.OnBorderFilled)
		{
			num += m_pinSize / 2;
		}
		int num2 = location.Y + GetPinOffset(outputElement, outputPin.Index, inputSide: false);
		Point location2 = inputElement.Bounds.Location;
		location2.Offset(WorldOffset(m_graphPath));
		int num3 = location2.X;
		if (PinDrawStyle == PinStyle.OnBorderFilled)
		{
			num3 -= m_pinSize / 2;
		}
		int num4 = location2.Y + GetPinOffset(inputElement, inputPin.Index, inputSide: true);
		DrawWire(g, pen, num, num2, num3, num4, 0f, null);
	}

	private void DrawWire(TElement element, TPin pin, Point p, bool fromOutput, D2dGraphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		PointF pointF = element.Bounds.Location;
		float num = pointF.X;
		if (PinDrawStyle == PinStyle.OnBorderFilled)
		{
			num += (float)m_pinSize * 0.5f;
		}
		float num2 = pointF.Y + (float)GetPinOffset(element, pin.Index, !fromOutput);
		if (fromOutput)
		{
			num += (float)elementTypeInfo.Size.Width;
		}
		Matrix3x2F transform = g.Transform;
		transform.Invert();
		PointF pointF2 = Matrix3x2F.TransformPoint(transform, p);
		D2dBrush pen = GetPen(pin);
		if (fromOutput)
		{
			DrawWire(g, pen, num, num2, pointF2.X, pointF2.Y, 0f, null);
		}
		else
		{
			DrawWire(g, pen, pointF2.X, pointF2.Y, num, num2, 0f, null);
		}
	}

	protected void DrawWire(D2dGraphics g, D2dBrush pen, float x1, float y1, float x2, float y2, float strokeWidth, D2dStrokeStyle strokeStyle)
	{
		float strokeWidth2 = ((strokeWidth == 0f) ? base.EdgeThickness : strokeWidth);
		float tangentLength = GetTangentLength(x1, x2);
		g.DrawBezier(new PointF(x1, y1), new PointF(x1 + tangentLength, y1), new PointF(x2 - tangentLength, y2), new PointF(x2, y2), pen, strokeWidth2, strokeStyle);
	}

	private float GetTangentLength(float x1, float x2)
	{
		float val = Math.Abs(x1 - x2) / 2f;
		return Math.Max(val, 32f);
	}

	protected virtual D2dBrush GetPen(TPin pin)
	{
		D2dBrush d2dBrush = m_theme.GetCustomBrush(pin.TypeName);
		if (d2dBrush == null)
		{
			d2dBrush = m_theme.GhostBrush;
		}
		return d2dBrush;
	}

	public virtual int GetPinOffset(ICircuitElement element, int pinIndex, bool inputSide)
	{
		if (inputSide)
		{
			ICircuitPin inputPin = element.Type.GetInputPin(pinIndex);
			if (inputPin.Is<ICircuitGroupPin<TElement>>())
			{
				ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
				if (circuitGroupType.Expanded)
				{
					ICircuitGroupPin<TElement> circuitGroupPin = inputPin.Cast<ICircuitGroupPin<TElement>>();
					return circuitGroupPin.Bounds.Location.Y + m_groupPinExpandedOffset + circuitGroupType.Info.Offset.Y;
				}
			}
			IList<ICircuitPin> visibleInputPins = GetVisibleInputPins(element);
			pinIndex = visibleInputPins.IndexOf(inputPin);
		}
		else
		{
			ICircuitPin outputPin = element.Type.GetOutputPin(pinIndex);
			if (outputPin.Is<ICircuitGroupPin<TElement>>())
			{
				ICircuitGroupType<TElement, TWire, TPin> circuitGroupType2 = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
				if (circuitGroupType2.Expanded)
				{
					ICircuitGroupPin<TElement> circuitGroupPin2 = outputPin.Cast<ICircuitGroupPin<TElement>>();
					return circuitGroupPin2.Bounds.Location.Y + m_groupPinExpandedOffset + circuitGroupType2.Info.Offset.Y;
				}
			}
			IList<ICircuitPin> visibleOutputPins = GetVisibleOutputPins(element);
			pinIndex = visibleOutputPins.IndexOf(outputPin);
		}
		return m_rowSpacing + 2 * m_pinMargin + pinIndex * m_rowSpacing + m_pinOffset + m_pinSize / 2;
	}

	private void SetPinSpacing()
	{
		m_pinMargin = m_theme.PinMargin;
		m_rowSpacing = m_theme.RowSpacing;
		m_pinOffset = m_theme.PinOffset;
		m_pinSize = m_theme.PinSize;
		m_groupPinExpandedOffset = 2 * m_rowSpacing;
		if (!m_subContentOffseExternalSet)
		{
			m_subContentOffset = new Point(m_rowSpacing + 4 * m_pinMargin, m_rowSpacing + 4 * m_pinMargin);
		}
	}

	private RectangleF GetElementBounds(TElement element, D2dGraphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		return new RectangleF(element.Bounds.Location, elementTypeInfo.Size);
	}

	private RectangleF GetWireBounds(TWire wire, D2dGraphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(wire.FromNode, g);
		RectangleF rectangleF = default(RectangleF);
		if (wire.Is<IEdgeStyleProvider>())
		{
			bool flag = true;
			foreach (EdgeStyleData datum in wire.Cast<IEdgeStyleProvider>().GetData(this, WorldOffset(m_graphPath), g))
			{
				if (datum.ShapeType == EdgeStyleData.EdgeShape.Bezier)
				{
					BezierCurve2F bezierCurve2F = datum.EdgeData.As<BezierCurve2F>();
					PointF[] points = new PointF[4] { bezierCurve2F.P1, bezierCurve2F.P2, bezierCurve2F.P3, bezierCurve2F.P4 };
					if (flag)
					{
						rectangleF = GetPointsBounds(points);
						flag = false;
					}
					else
					{
						rectangleF = RectangleF.Union(rectangleF, GetPointsBounds(points));
					}
				}
				else if (datum.ShapeType == EdgeStyleData.EdgeShape.Line)
				{
					PointF[] array = datum.EdgeData.As<PointF[]>();
					if (array != null)
					{
						if (flag)
						{
							rectangleF = D2dUtil.MakeRectangle(array[0], array[1]);
							flag = false;
						}
						else
						{
							rectangleF = RectangleF.Union(rectangleF, D2dUtil.MakeRectangle(array[0], array[1]));
						}
					}
				}
				else if (datum.ShapeType == EdgeStyleData.EdgeShape.Polyline)
				{
					PointF[] array2 = datum.EdgeData.As<PointF[]>();
					if (array2 != null)
					{
						if (flag)
						{
							rectangleF = GetPointsBounds(array2);
							flag = false;
						}
						else
						{
							rectangleF = RectangleF.Union(rectangleF, GetPointsBounds(array2));
						}
					}
				}
				else
				{
					if (datum.ShapeType != EdgeStyleData.EdgeShape.BezierSpline)
					{
						continue;
					}
					IEnumerable<BezierCurve2F> enumerable = datum.EdgeData.As<IEnumerable<BezierCurve2F>>();
					foreach (BezierCurve2F item in enumerable)
					{
						PointF[] points2 = new PointF[4] { item.P1, item.P2, item.P3, item.P4 };
						if (flag)
						{
							rectangleF = GetPointsBounds(points2);
							flag = false;
						}
						else
						{
							rectangleF = RectangleF.Union(rectangleF, GetPointsBounds(points2));
						}
					}
				}
			}
		}
		else
		{
			Point location = wire.FromNode.Bounds.Location;
			location.Offset(WorldOffset(m_graphPath));
			int num = location.X + elementTypeInfo.Size.Width;
			if (PinDrawStyle == PinStyle.OnBorderFilled)
			{
				num += m_pinSize / 2;
			}
			int num2 = location.Y + GetPinOffset(wire.FromNode, wire.FromRoute.Index, inputSide: false);
			Point location2 = wire.ToNode.Bounds.Location;
			location2.Offset(WorldOffset(m_graphPath));
			int num3 = location2.X;
			if (PinDrawStyle == PinStyle.OnBorderFilled)
			{
				num3 -= m_pinSize / 2;
			}
			int num4 = location2.Y + GetPinOffset(wire.ToNode, wire.ToRoute.Index, inputSide: true);
			rectangleF = D2dUtil.MakeRectangle(new PointF(num, num2), new PointF(num3, num4));
		}
		return rectangleF;
	}

	private RectangleF GetPointsBounds(IEnumerable<PointF> points)
	{
		float num = points.Min((PointF p) => p.X);
		float num2 = points.Min((PointF p) => p.Y);
		float num3 = points.Max((PointF p) => p.X);
		float num4 = points.Max((PointF p) => p.Y);
		return new RectangleF(new PointF(num, num2), new SizeF(num3 - num, num4 - num2));
	}

	protected RectangleF GetExpanderRect(PointF p)
	{
		return new RectangleF(p.X + (float)m_pinMargin + 1f, p.Y + (float)(2 * m_pinMargin) + 1f, 8f, 8f);
	}

	protected RectangleF GetShowPinsRect(RectangleF bounds)
	{
		return new RectangleF(bounds.Right - (float)m_pinMargin - 1f - 8f, bounds.Y + (float)(2 * m_pinMargin) + 1f, 8f, 8f);
	}

	protected bool HasShowUnconnectedPinsToggle(TElement element)
	{
		ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.As<ICircuitGroupType<TElement, TWire, TPin>>();
		return circuitGroupType == null || !circuitGroupType.Expanded;
	}

	private Point SubGraphOffset(TElement element)
	{
		Point result = Point.Empty;
		if (element.Is<ICircuitGroupType<TElement, TWire, TPin>>())
		{
			ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
			result = circuitGroupType.Info.Offset;
		}
		return result;
	}

	public Point WorldOffset(IEnumerable<TElement> graphPath)
	{
		Point result = default(Point);
		Point empty = Point.Empty;
		foreach (TElement item in graphPath)
		{
			empty = SubGraphOffset(item);
			result.X += item.Bounds.Location.X + empty.X;
			result.Y += item.Bounds.Location.Y + empty.Y;
			result.Offset(m_subContentOffset);
		}
		return result;
	}

	private Point ParentWorldOffset(IEnumerable<TElement> graphPath)
	{
		Point result = default(Point);
		Point empty = Point.Empty;
		foreach (TElement item in graphPath.Skip(1))
		{
			empty = SubGraphOffset(item);
			result.X += item.Bounds.Location.X + empty.X;
			result.Y += item.Bounds.Location.Y + empty.Y;
			int num = m_rowSpacing + 4 * m_pinMargin;
			result.Offset(num, num);
		}
		return result;
	}

	private void DocumentRegistryOnDocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> itemRemovedEventArgs)
	{
		m_cachePerDocument.Remove(itemRemovedEventArgs.Item);
	}

	private void DocumentRegistryOnActiveDocumentChanging(object sender, EventArgs eventArgs)
	{
		if (m_documentRegistry.ActiveDocument != null)
		{
			m_cachePerDocument[m_documentRegistry.ActiveDocument] = m_elementTypeCache;
		}
	}

	private void DocumentRegistryOnActiveDocumentChanged(object sender, EventArgs eventArgs)
	{
		if (m_documentRegistry.ActiveDocument == null || !m_cachePerDocument.TryGetValue(m_documentRegistry.ActiveDocument, out m_elementTypeCache))
		{
			m_elementTypeCache = new Dictionary<ICircuitElementType, ElementTypeInfo>();
		}
		m_graphHelper = new GraphHelper(m_documentRegistry.ActiveDocument.As<IGraph<TElement, TWire, TPin>>());
	}
}
