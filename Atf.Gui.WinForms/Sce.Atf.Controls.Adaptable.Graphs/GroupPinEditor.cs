using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class GroupPinEditor : DraggingControlAdapter, IItemDragAdapter, IDisposable
{
	[Flags]
	private enum MeasurePinNode
	{
		DesiredLocation = 1,
		MaximumNameWidth = 2
	}

	public Func<ICircuitElement, int, bool, int> GetPinOffset;

	private IGraph<Element, Wire, ICircuitPin> m_graph = CircuitUtil.EmptyCircuit;

	private ITransformAdapter m_transformAdapter;

	private List<Element> m_draggingNodes = new List<Element>();

	private List<GroupPin> m_draggingGroupPins = new List<GroupPin>();

	private int[] m_originalPinY;

	private ILayoutContext m_layoutContext;

	private ISelectionContext m_selectionContext;

	public GroupPinEditor(ITransformAdapter transformAdapter)
	{
		m_transformAdapter = transformAdapter;
	}

	void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
	{
		ActiveCollection<Element> activeCollection = new ActiveCollection<Element>();
		foreach (Element item in m_selectionContext.GetSelection<Element>())
		{
			activeCollection.Add(item);
		}
		m_draggingNodes.AddRange(activeCollection.GetSnapshot());
		ActiveCollection<GroupPin> activeCollection2 = new ActiveCollection<GroupPin>();
		foreach (GroupPin item2 in m_selectionContext.GetSelection<GroupPin>())
		{
			activeCollection2.Add(item2);
		}
		m_draggingGroupPins.AddRange(activeCollection2.GetSnapshot());
		if (m_draggingGroupPins.Any())
		{
			m_originalPinY = new int[m_draggingGroupPins.Count];
			for (int i = 0; i < m_originalPinY.Length; i++)
			{
				m_originalPinY[i] = m_draggingGroupPins[i].Bounds.Location.Y;
			}
		}
	}

	void IItemDragAdapter.EndingDrag()
	{
	}

	void IItemDragAdapter.EndDrag()
	{
		m_draggingGroupPins.Clear();
		m_draggingNodes.Clear();
	}

	public void Dispose()
	{
	}

	protected override void Bind(AdaptableControl control)
	{
		control.ContextChanged += control_ContextChanged;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.ContextChanged += control_ContextChanged;
		control.ContextChanged -= control_ContextChanged;
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		Point point = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.CurrentPoint);
		Point point2 = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.FirstPoint);
		Point offset = new Point(point.X - point2.X, point.Y - point2.Y);
		if (m_draggingNodes.Any() && !offset.IsEmpty)
		{
			AdjustLayout(m_selectionContext.GetSelection<Element>(), m_draggingGroupPins, offset);
		}
		if (m_draggingGroupPins.Any() && offset.Y != 0)
		{
			for (int i = 0; i < m_originalPinY.Length; i++)
			{
				m_draggingGroupPins[i].Bounds = new Rectangle(m_draggingGroupPins[i].Bounds.Location.X, Constrain(m_originalPinY[i] + offset.Y), m_draggingGroupPins[i].Bounds.Width, m_draggingGroupPins[i].Bounds.Height);
			}
			base.AdaptedControl.Invalidate();
		}
	}

	public void UpdateTranslateMinMax(CanvasAdapter canvasAdapter)
	{
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		IGraph<Element, Wire, ICircuitPin> graph = base.AdaptedControl.ContextAs<IGraph<Element, Wire, ICircuitPin>>();
		if (graph == null)
		{
			graph = CircuitUtil.EmptyCircuit;
		}
		if (m_graph != graph)
		{
			m_graph = graph;
		}
		m_layoutContext = base.AdaptedControl.ContextAs<ILayoutContext>();
		if (m_layoutContext != null)
		{
			m_selectionContext = base.AdaptedControl.ContextCast<ISelectionContext>();
		}
	}

	private void MeasureFakePins(IEnumerable<GroupPin> pinNodes, MeasurePinNode measureParts, Point offset, bool inputSide)
	{
		if (!pinNodes.Any() || (measureParts & MeasurePinNode.DesiredLocation) == 0)
		{
			return;
		}
		List<GroupPin> list = (from x in pinNodes
			orderby x.InternalElement.Position.Y, x.InternalElement.PinDisplayOrder(x.InternalPinIndex, inputSide)
			select x).ToList();
		foreach (GroupPin item in list)
		{
			int num = GetPinOffset(item.InternalElement, item.InternalPinIndex, inputSide);
			item.DesiredLocation = new Point(offset.X, item.InternalElement.Bounds.Location.Y + num - 8);
		}
		int y = list[0].DesiredLocation.Y;
		for (int num2 = 1; num2 < list.Count; num2++)
		{
			GroupPin groupPin = list[num2];
			int num3 = groupPin.DesiredLocation.Y - y - (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin);
			if (num3 < 0)
			{
				list[num2].DesiredLocation = new Point(0, list[num2].DesiredLocation.Y - num3);
			}
			y = groupPin.DesiredLocation.Y;
		}
	}

	public void AdjustLayout(IEnumerable<Element> nodesMoved, IEnumerable<GroupPin> floatingPinsMoved, Point offset)
	{
		List<Element> movedNodes = nodesMoved.ToList();
		Group obj = m_graph.As<Group>();
		if (obj == null || movedNodes.Count == 0)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			List<GroupPin> list = ((i == 0) ? obj.InputGroupPins : obj.OutputGroupPins).ToList();
			List<GroupPin> list2 = (from x in list.Where((GroupPin x) => !x.Info.Pinned).Except(m_draggingGroupPins)
				where movedNodes.Contains(x.InternalElement)
				select x).ToList();
			List<GroupPin> againstPinNodes = (from y in list.Except(list2)
				orderby y.Bounds.Location.Y
				select y).ToList();
			MeasureFakePins(list2, MeasurePinNode.DesiredLocation, offset, i == 0);
			int num = int.MinValue;
			List<GroupPin> list3 = list2.OrderBy((GroupPin x) => x.DesiredLocation.Y).ToList();
			for (int num2 = 0; num2 < list3.Count; num2++)
			{
				GroupPin groupPin = list3[num2];
				PositioningFloatigPin(againstPinNodes, groupPin, num);
				num = groupPin.Bounds.Location.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin;
				int num3 = groupPin.Bounds.Location.Y - groupPin.DesiredLocation.Y;
				for (int num4 = num2 + 1; num4 < list3.Count; num4++)
				{
					list3[num4].DesiredLocation = new Point(list3[num4].DesiredLocation.X, Math.Max(list3[num4].DesiredLocation.Y + num3, num));
				}
			}
		}
	}

	private void PositioningFloatigPin(IList<GroupPin> againstPinNodes, GroupPin floatingPin, int minY)
	{
		bool flag = false;
		for (int i = 0; i < againstPinNodes.Count; i++)
		{
			GroupPin groupPin = againstPinNodes[i];
			if (groupPin.Bounds.Location.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin < minY || !PinOverlap(floatingPin, groupPin))
			{
				continue;
			}
			flag = true;
			for (int j = i; j < againstPinNodes.Count - 1; j++)
			{
				if (againstPinNodes[j + 1].Bounds.Location.Y - againstPinNodes[j].Bounds.Location.Y >= 2 * (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin))
				{
					int y = Constrain(againstPinNodes[j].Bounds.Location.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin);
					floatingPin.Bounds = new Rectangle(floatingPin.Bounds.Location.X, y, floatingPin.Bounds.Width, floatingPin.Bounds.Height);
					return;
				}
			}
		}
		if (flag)
		{
			GroupPin groupPin2 = againstPinNodes[0];
			int val = Constrain(groupPin2.Bounds.Location.Y - (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin));
			val = Math.Max(val, minY);
			floatingPin.DesiredLocation = new Point(floatingPin.DesiredLocation.X, val);
			if (val > minY && !PinOverlap(floatingPin, groupPin2))
			{
				floatingPin.Bounds = new Rectangle(floatingPin.Bounds.Location.X, val, floatingPin.Bounds.Width, floatingPin.Bounds.Height);
				return;
			}
			GroupPin groupPin3 = againstPinNodes[againstPinNodes.Count - 1];
			val = Constrain(groupPin3.Bounds.Location.Y + (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin));
			val = Math.Max(val, minY);
			floatingPin.DesiredLocation = new Point(floatingPin.DesiredLocation.X, val);
			while (PinOverlap(floatingPin, groupPin3))
			{
				val++;
				floatingPin.DesiredLocation = new Point(floatingPin.DesiredLocation.X, val);
			}
			floatingPin.Bounds = new Rectangle(floatingPin.Bounds.Location.X, val, floatingPin.Bounds.Width, floatingPin.Bounds.Height);
		}
		else
		{
			floatingPin.Bounds = new Rectangle(floatingPin.Bounds.Location.X, Constrain(floatingPin.DesiredLocation.Y), floatingPin.Bounds.Width, floatingPin.Bounds.Height);
		}
	}

	private static bool PinOverlap(GroupPin freePin, GroupPin againstPin)
	{
		Pair<int, int> pair = new Pair<int, int>(freePin.DesiredLocation.Y - CircuitGroupPinInfo.FloatingPinNodeMargin, freePin.DesiredLocation.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin);
		Pair<int, int> pair2 = new Pair<int, int>(againstPin.Bounds.Location.Y - CircuitGroupPinInfo.FloatingPinNodeMargin, againstPin.Bounds.Location.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin);
		return pair2.First < pair.Second && pair.First < pair2.Second;
	}

	private int Constrain(int y)
	{
		return y;
	}
}
