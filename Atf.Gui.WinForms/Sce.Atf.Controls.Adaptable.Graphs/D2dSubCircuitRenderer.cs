using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class D2dSubCircuitRenderer<TElement, TWire, TPin> : D2dCircuitRenderer<TElement, TWire, TPin> where TElement : class, ICircuitElement where TWire : class, IGraphEdge<TElement, TPin> where TPin : class, ICircuitPin
{
	private const int MaxNameOverhang = 64;

	private D2dBrush m_subGraphPinNodePen;

	private D2dBrush m_subGraphPinPen;

	private D2dBrush m_fakeInputLinkPen;

	private D2dBrush m_fakeOutputLinkPen;

	private D2dBrush m_pinBrush;

	private D2dBrush m_visiblePinBrush;

	private D2dBrush m_hiddrenPinBrush;

	private D2dStrokeStyle m_VirtualLinkStrokeStyle;

	public RectangleF VisibleWorldBounds { get; set; }

	public D2dSubCircuitRenderer(D2dDiagramTheme defaultTheme, IDocumentRegistry documentRegistry = null)
		: base(defaultTheme, documentRegistry)
	{
		m_fakeInputLinkPen = D2dFactory.CreateSolidBrush(Color.DarkOrchid);
		m_fakeOutputLinkPen = D2dFactory.CreateSolidBrush(Color.SlateGray);
		m_subGraphPinNodePen = D2dFactory.CreateSolidBrush(Color.SandyBrown);
		m_subGraphPinPen = D2dFactory.CreateSolidBrush(Color.DeepSkyBlue);
		m_pinBrush = D2dFactory.CreateSolidBrush(SystemColors.ControlDarkDark);
		m_visiblePinBrush = D2dFactory.CreateSolidBrush(Color.Black);
		m_hiddrenPinBrush = D2dFactory.CreateSolidBrush(Color.Gray);
		m_VirtualLinkStrokeStyle = D2dFactory.CreateD2dStrokeStyle(new D2dStrokeStyleProperties
		{
			EndCap = D2dCapStyle.Round,
			StartCap = D2dCapStyle.Round,
			DashStyle = D2dDashStyle.DashDot
		});
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_fakeInputLinkPen != null)
			{
				m_fakeInputLinkPen.Dispose();
				m_fakeInputLinkPen = null;
			}
			if (m_fakeOutputLinkPen != null)
			{
				m_fakeOutputLinkPen.Dispose();
				m_fakeOutputLinkPen = null;
			}
			if (m_subGraphPinNodePen != null)
			{
				m_subGraphPinNodePen.Dispose();
				m_subGraphPinNodePen = null;
			}
			if (m_subGraphPinPen != null)
			{
				m_subGraphPinPen.Dispose();
				m_subGraphPinPen = null;
			}
		}
		base.Dispose(disposing);
	}

	public void DrawFloatingGroupPin(ICircuitGroupPin<TElement> grpPin, bool inputSide, DiagramDrawingStyle style, D2dGraphics g)
	{
		SizeF sizeF = g.MeasureText(grpPin.Name, base.Theme.TextFormat);
		PointF groupPinLocation;
		if (inputSide)
		{
			groupPinLocation = GetGroupPinLocation(grpPin, inputSide: true);
			RectangleF rect = new RectangleF(groupPinLocation.X + (float)CircuitGroupPinInfo.FloatingPinBoxWidth - (float)base.Theme.PinSize, grpPin.Bounds.Location.Y + base.Theme.PinMargin + base.Theme.PinOffset, base.Theme.PinSize, base.Theme.PinSize);
			g.DrawRectangle(rect, m_subGraphPinPen);
			if (grpPin.Info.Pinned)
			{
				D2dUtil.DrawPin((int)(groupPinLocation.X + (float)CircuitGroupPinInfo.FloatingPinBoxWidth), (int)groupPinLocation.Y, pinned: true, toLeft: true, m_pinBrush, g);
			}
			else
			{
				D2dUtil.DrawPin((int)(groupPinLocation.X + (float)CircuitGroupPinInfo.FloatingPinBoxWidth), (int)groupPinLocation.Y + base.Theme.PinSize / 2, pinned: false, toLeft: true, m_pinBrush, g);
			}
			RectangleF rectangleF = new RectangleF(groupPinLocation.X, groupPinLocation.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
			RectangleF rectangleF2 = new RectangleF(rectangleF.Left, rectangleF.Bottom + (float)base.Theme.PinMargin, sizeF.Width, base.Theme.RowSpacing);
			D2dTextAlignment textAlignment = base.Theme.TextFormat.TextAlignment;
			base.Theme.TextFormat.TextAlignment = D2dTextAlignment.Leading;
			g.DrawText(grpPin.Name, base.Theme.TextFormat, rectangleF2.Location, base.Theme.TextBrush);
			base.Theme.TextFormat.TextAlignment = textAlignment;
		}
		else
		{
			groupPinLocation = GetGroupPinLocation(grpPin, inputSide: false);
			RectangleF rect2 = new RectangleF(groupPinLocation.X + 1f, grpPin.Bounds.Location.Y + base.Theme.PinMargin + base.Theme.PinOffset, base.Theme.PinSize, base.Theme.PinSize);
			g.DrawRectangle(rect2, m_subGraphPinPen);
			if (grpPin.Info.Pinned)
			{
				D2dUtil.DrawPin((int)groupPinLocation.X, (int)groupPinLocation.Y, pinned: true, toLeft: false, m_pinBrush, g);
			}
			else
			{
				D2dUtil.DrawPin((int)groupPinLocation.X, (int)groupPinLocation.Y + base.Theme.PinSize / 2, pinned: false, toLeft: false, m_pinBrush, g);
			}
			RectangleF rectangleF3 = new RectangleF(groupPinLocation.X, groupPinLocation.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
			RectangleF layoutRect = new RectangleF(rectangleF3.Right - sizeF.Width, rectangleF3.Bottom + (float)base.Theme.PinMargin, sizeF.Width, base.Theme.RowSpacing);
			D2dTextAlignment textAlignment2 = base.Theme.TextFormat.TextAlignment;
			base.Theme.TextFormat.TextAlignment = D2dTextAlignment.Trailing;
			g.DrawText(grpPin.Name, base.Theme.TextFormat, layoutRect, base.Theme.TextBrush);
			base.Theme.TextFormat.TextAlignment = textAlignment2;
		}
		float strokeWidth = base.Theme.StrokeWidth;
		base.Theme.StrokeWidth = 2f;
		if (style == DiagramDrawingStyle.Normal)
		{
			g.DrawRectangle(new RectangleF(groupPinLocation.X, groupPinLocation.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight), m_subGraphPinNodePen);
		}
		else
		{
			g.DrawRectangle(new RectangleF(groupPinLocation.X, groupPinLocation.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight), base.Theme.HotBrush);
		}
		base.Theme.StrokeWidth = strokeWidth;
		if (!grpPin.Info.ExternalConnected)
		{
			RectangleF visibilityCheckRect = GetVisibilityCheckRect(grpPin, inputSide);
			g.DrawEyeIcon(visibilityCheckRect, grpPin.Info.Visible ? m_visiblePinBrush : m_hiddrenPinBrush, 1f);
		}
		DrawGroupPinNodeFakeEdge(grpPin, groupPinLocation, inputSide, style, g);
	}

	public virtual RectangleF GetBounds(ICircuitGroupPin<TElement> pin, bool inputSide, D2dGraphics g)
	{
		PointF groupPinLocation = GetGroupPinLocation(pin, inputSide);
		return new RectangleF(groupPinLocation.X, groupPinLocation.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
	}

	public override GraphHitRecord<TElement, TWire, TPin> Pick(IGraph<TElement, TWire, TPin> graph, TWire priorityEdge, PointF p, D2dGraphics g)
	{
		GraphHitRecord<TElement, TWire, TPin> graphHitRecord = base.Pick(graph, priorityEdge, p, g);
		if (graphHitRecord.Node != null || graphHitRecord.Edge != null || graphHitRecord.Part != null)
		{
			return graphHitRecord;
		}
		ICircuitGroupType<TElement, TWire, TPin> circuitGroupType = graph.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
		foreach (ICircuitPin item in circuitGroupType.Inputs.Concat(circuitGroupType.Info.HiddenInputPins))
		{
			ICircuitGroupPin<TElement> circuitGroupPin = item.Cast<ICircuitGroupPin<TElement>>();
			RectangleF thumbtackRect = GetThumbtackRect(circuitGroupPin, inputSide: true);
			if (thumbtackRect.Contains(p))
			{
				DiagramPin part = new DiagramPin(thumbtackRect);
				return new GraphHitRecord<TElement, TWire, TPin>((TPin)circuitGroupPin, part);
			}
			RectangleF visibilityCheckRect = GetVisibilityCheckRect(circuitGroupPin, inputSide: true);
			if (visibilityCheckRect.Contains(p))
			{
				DiagramVisibilityCheck part2 = new DiagramVisibilityCheck(visibilityCheckRect);
				return new GraphHitRecord<TElement, TWire, TPin>((TPin)circuitGroupPin, part2);
			}
			PointF groupPinLocation = GetGroupPinLocation(circuitGroupPin, inputSide: true);
			RectangleF rectangleF = new RectangleF(groupPinLocation.X, groupPinLocation.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
			SizeF sizeF = g.MeasureText(circuitGroupPin.Name, base.Theme.TextFormat);
			RectangleF rectangleF2 = new RectangleF(rectangleF.Left, rectangleF.Bottom + (float)base.Theme.PinMargin, (int)sizeF.Width, base.Theme.RowSpacing);
			DiagramLabel diagramLabel = new DiagramLabel(new Rectangle((int)rectangleF2.Left, (int)rectangleF2.Top, (int)rectangleF2.Width, (int)rectangleF2.Height), TextFormatFlags.SingleLine);
			if (rectangleF2.Contains(p))
			{
				return new GraphHitRecord<TElement, TWire, TPin>((TPin)circuitGroupPin, diagramLabel);
			}
			if (rectangleF.Contains(p))
			{
				GraphHitRecord<TElement, TWire, TPin> graphHitRecord2 = new GraphHitRecord<TElement, TWire, TPin>((TPin)circuitGroupPin, null);
				graphHitRecord2.DefaultPart = diagramLabel;
				return graphHitRecord2;
			}
		}
		foreach (ICircuitPin item2 in circuitGroupType.Outputs.Concat(circuitGroupType.Info.HiddenOutputPins))
		{
			ICircuitGroupPin<TElement> circuitGroupPin2 = item2.Cast<ICircuitGroupPin<TElement>>();
			RectangleF thumbtackRect2 = GetThumbtackRect(circuitGroupPin2, inputSide: false);
			if (thumbtackRect2.Contains(p))
			{
				DiagramPin part3 = new DiagramPin(thumbtackRect2);
				return new GraphHitRecord<TElement, TWire, TPin>((TPin)circuitGroupPin2, part3);
			}
			RectangleF visibilityCheckRect2 = GetVisibilityCheckRect(circuitGroupPin2, inputSide: false);
			if (visibilityCheckRect2.Contains(p))
			{
				DiagramVisibilityCheck part4 = new DiagramVisibilityCheck(visibilityCheckRect2);
				return new GraphHitRecord<TElement, TWire, TPin>((TPin)circuitGroupPin2, part4);
			}
			PointF groupPinLocation2 = GetGroupPinLocation(circuitGroupPin2, inputSide: false);
			RectangleF rectangleF3 = new RectangleF(groupPinLocation2.X, groupPinLocation2.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
			SizeF sizeF2 = g.MeasureText(circuitGroupPin2.Name, base.Theme.TextFormat);
			RectangleF rectangleF4 = new RectangleF(rectangleF3.Right - (float)(int)sizeF2.Width, rectangleF3.Bottom + (float)base.Theme.PinMargin, (int)sizeF2.Width, base.Theme.RowSpacing);
			DiagramLabel diagramLabel2 = new DiagramLabel(new Rectangle((int)rectangleF4.Left, (int)rectangleF4.Top, (int)rectangleF4.Width, (int)rectangleF4.Height), TextFormatFlags.Right | TextFormatFlags.SingleLine);
			if (rectangleF4.Contains(p))
			{
				return new GraphHitRecord<TElement, TWire, TPin>((TPin)circuitGroupPin2, diagramLabel2);
			}
			if (rectangleF3.Contains(p))
			{
				GraphHitRecord<TElement, TWire, TPin> graphHitRecord3 = new GraphHitRecord<TElement, TWire, TPin>((TPin)circuitGroupPin2, null);
				graphHitRecord3.DefaultPart = diagramLabel2;
				return graphHitRecord3;
			}
		}
		return graphHitRecord;
	}

	private PointF GetGroupPinLocation(ICircuitGroupPin<TElement> grpPin, bool inputSide)
	{
		if (inputSide)
		{
			return new PointF(VisibleWorldBounds.X + (float)CircuitGroupPinInfo.FloatingPinBoxWidth + (float)base.Theme.PinMargin, grpPin.Bounds.Location.Y);
		}
		return new PointF(VisibleWorldBounds.X + VisibleWorldBounds.Width - (float)base.Theme.PinMargin - (float)(2 * CircuitGroupPinInfo.FloatingPinBoxWidth) - 16f, grpPin.Bounds.Location.Y);
	}

	private RectangleF GetThumbtackRect(ICircuitGroupPin<TElement> grpPin, bool inputSide)
	{
		PointF groupPinLocation = GetGroupPinLocation(grpPin, inputSide);
		int num = (inputSide ? CircuitGroupPinInfo.FloatingPinBoxWidth : (-8));
		if (grpPin.Info.Pinned)
		{
			return new RectangleF(groupPinLocation.X + (float)num, groupPinLocation.Y, 8f, 8f);
		}
		return new RectangleF(groupPinLocation.X + (float)num, groupPinLocation.Y + (float)base.Theme.PinSize / 2f, 8f, 8f);
	}

	private RectangleF GetVisibilityCheckRect(ICircuitGroupPin<TElement> grpPin, bool inputSide)
	{
		PointF groupPinLocation = GetGroupPinLocation(grpPin, inputSide);
		float num = CircuitGroupPinInfo.FloatingPinBoxWidth - base.Theme.PinSize - 3;
		RectangleF rectangleF;
		RectangleF result;
		if (inputSide)
		{
			rectangleF = new RectangleF(groupPinLocation.X + (float)CircuitGroupPinInfo.FloatingPinBoxWidth - (float)base.Theme.PinSize, grpPin.Bounds.Location.Y + base.Theme.PinMargin + base.Theme.PinOffset, base.Theme.PinSize, base.Theme.PinSize);
			result = new RectangleF(groupPinLocation.X - 2f - num, rectangleF.Y, num, rectangleF.Height);
		}
		else
		{
			rectangleF = new RectangleF(groupPinLocation.X + 1f, grpPin.Bounds.Location.Y + base.Theme.PinMargin + base.Theme.PinOffset, base.Theme.PinSize, base.Theme.PinSize);
			result = new RectangleF(groupPinLocation.X + (float)CircuitGroupPinInfo.FloatingPinBoxWidth + 2f, rectangleF.Y, num, rectangleF.Height);
		}
		return result;
	}

	private void DrawGroupPinNodeFakeEdge(ICircuitGroupPin<TElement> grpPin, PointF grpPinPos, bool inputSide, DiagramDrawingStyle style, D2dGraphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(grpPin.InternalElement, g);
		if (inputSide)
		{
			PointF pointF = grpPinPos;
			float x = pointF.X + (float)CircuitGroupPinInfo.FloatingPinBoxWidth;
			float y = pointF.Y + (float)(CircuitGroupPinInfo.FloatingPinBoxHeight / 2);
			Point location = grpPin.InternalElement.Bounds.Location;
			float x2 = location.X;
			float y2 = location.Y + GetPinOffset(grpPin.InternalElement, grpPin.InternalPinIndex, inputSide: true);
			DrawWire(g, m_fakeInputLinkPen, x, y, x2, y2, 1f, m_VirtualLinkStrokeStyle);
		}
		else
		{
			Point location2 = grpPin.InternalElement.Bounds.Location;
			float x3 = location2.X + elementTypeInfo.Size.Width;
			float y3 = location2.Y + GetPinOffset(grpPin.InternalElement, grpPin.InternalPinIndex, inputSide: false);
			PointF pointF2 = grpPinPos;
			float x4 = pointF2.X;
			float y4 = pointF2.Y + (float)(CircuitGroupPinInfo.FloatingPinBoxHeight / 2);
			DrawWire(g, m_fakeOutputLinkPen, x3, y3, x4, y4, 1f, m_VirtualLinkStrokeStyle);
		}
	}
}
