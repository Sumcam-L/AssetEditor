using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class CircuitValidator : Validator
{
	private bool m_undoingOrRedoing;

	private HashSet<Group> m_subGraphs;

	private HashSet<Circuit> m_circuits;

	private HashSet<HistoryContext> m_historyContexts;

	private Multimap<Group, Element> m_nodesInserted = new Multimap<Group, Element>();

	private Multimap<DomNode, DomNode> m_templateInstances = new Multimap<DomNode, DomNode>();

	protected abstract AttributeInfo ElementLabelAttribute { get; }

	protected abstract AttributeInfo PinNameAttributeAttribute { get; }

	internal bool Suspended { get; set; }

	internal bool MovingCrossContainer { get; set; }

	internal HistoryContext ActiveHistoryContext { get; set; }

	protected override void OnNodeSet()
	{
		m_subGraphs = new HashSet<Group>();
		m_circuits = new HashSet<Circuit>();
		m_historyContexts = new HashSet<HistoryContext>();
		foreach (DomNode item in base.DomNode.Subtree)
		{
			if (CircuitUtil.IsGroupTemplateInstance(item))
			{
				Group groupTemplate = CircuitUtil.GetGroupTemplate(item);
				if (groupTemplate != null)
				{
					m_templateInstances.Add(groupTemplate.DomNode, item);
					m_subGraphs.Add(groupTemplate);
				}
			}
			else if (item.Is<Group>())
			{
				m_subGraphs.Add(item.Cast<Group>());
			}
			else if (item.Is<Circuit>())
			{
				m_circuits.Add(item.Cast<Circuit>());
			}
		}
		base.OnNodeSet();
		if (m_templateInstances.Keys.Any())
		{
			UpdateWires(m_subGraphs);
			UpdateWires(m_circuits);
		}
	}

	protected override void AddNode(DomNode node)
	{
		foreach (HistoryContext item in node.AsAll<HistoryContext>())
		{
			item.PendingSetOperationLifetime = TimeSpan.Zero;
			m_historyContexts.Add(item);
		}
		base.AddNode(node);
	}

	protected override void RemoveNode(DomNode node)
	{
		foreach (HistoryContext item in node.AsAll<HistoryContext>())
		{
			m_historyContexts.Remove(item);
		}
		base.RemoveNode(node);
	}

	protected override void OnBeginning(object sender, EventArgs e)
	{
		ActiveHistoryContext = sender.Cast<HistoryContext>();
		m_undoingOrRedoing = m_historyContexts.Any((HistoryContext h) => h.UndoingOrRedoing);
		m_nodesInserted.Clear();
		ReferenceValidator referenceValidator = base.DomNode.As<ReferenceValidator>();
		if (referenceValidator != null)
		{
			referenceValidator.Suspended = m_undoingOrRedoing;
		}
		MovingCrossContainer = false;
	}

	protected override void OnChildInserted(object sender, ChildEventArgs e)
	{
		AddSubtree(e.Child);
		if (!m_undoingOrRedoing && e.Child.Is<Wire>())
		{
			UpdateGroupPinConnectivity(e.Child.Cast<Wire>());
		}
	}

	protected override void OnChildRemoved(object sender, ChildEventArgs e)
	{
		RemoveSubtree(e.Child);
		if (!m_undoingOrRedoing && e.Child.Is<Wire>())
		{
			UpdateGroupPinConnectivity(e.Child.Cast<Wire>());
		}
	}

	protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (base.Validating && !m_undoingOrRedoing)
		{
			if (e.DomNode.Parent.Is<Group>() && e.AttributeInfo == ElementLabelAttribute)
			{
				Group obj = e.DomNode.Parent.Cast<Group>();
				if (e.DomNode.Is<Element>())
				{
					SyncGroupPinNamesFromModuleName(obj, e.DomNode);
				}
			}
			else if (e.DomNode.Is<GroupPin>() && e.AttributeInfo == PinNameAttributeAttribute && e.DomNode.Parent != null)
			{
				Group obj2 = e.DomNode.Parent.Cast<Group>();
				UniqueNamer uniqueNamer = new UniqueNamer();
				GroupPin groupPin = e.DomNode.Cast<GroupPin>();
				foreach (GroupPin inputGroupPin in obj2.InputGroupPins)
				{
					if (inputGroupPin != groupPin)
					{
						uniqueNamer.Name(inputGroupPin.Name);
					}
				}
				foreach (GroupPin outputGroupPin in obj2.OutputGroupPins)
				{
					if (outputGroupPin != groupPin)
					{
						uniqueNamer.Name(outputGroupPin.Name);
					}
				}
				string text = uniqueNamer.Name(groupPin.Name);
				if (text != groupPin.Name)
				{
					groupPin.Name = text;
				}
				string text2 = groupPin.DefaultName(groupPin.IsInputSide);
				uniqueNamer.Parse(text, out var root, out var _);
				groupPin.IsDefaultName = text2 == root;
				UpdateParentGroupPinName(groupPin.IsInputSide, groupPin);
			}
		}
		base.OnAttributeChanged(sender, e);
	}

	private void UpdateParentGroupPinName(bool inputSide, GroupPin childPin)
	{
		GroupPin groupPin = childPin.GetAncestry(inputSide).FirstOrDefault();
		if (groupPin != null && !childPin.IsDefaultName && groupPin.IsDefaultName)
		{
			groupPin.Name = childPin.Name;
			groupPin.IsDefaultName = false;
		}
	}

	private void SyncGroupPinNamesFromModuleName(Group group, DomNode node)
	{
		foreach (GroupPin input in group.Inputs)
		{
			if (input.InternalElement.DomNode == node && input.IsDefaultName)
			{
				input.Name = input.InternalElement.Name + ":" + input.InternalElement.InputPin(input.InternalPinIndex).Name;
			}
		}
		foreach (GroupPin output in group.Outputs)
		{
			if (output.InternalElement.DomNode == node && output.IsDefaultName)
			{
				output.Name = output.InternalElement.Name + ":" + output.InternalElement.OutputPin(output.InternalPinIndex).Name;
			}
		}
	}

	protected override void OnEnding(object sender, EventArgs e)
	{
		ReferenceValidator referenceValidator = base.DomNode.As<ReferenceValidator>();
		if (referenceValidator != null)
		{
			referenceValidator.Suspended = false;
		}
		if (m_undoingOrRedoing)
		{
			foreach (Group item in from s in m_subGraphs
				where s.Dirty
				orderby s.Level descending
				select s)
			{
				item.Dirty = false;
				foreach (Wire wire in item.Wires)
				{
					wire.InputPinTarget = null;
					wire.OutputPinTarget = null;
				}
				item.UpdateGroupPinInfo();
				item.OnChanged(EventArgs.Empty);
			}
			{
				foreach (Circuit circuit in m_circuits)
				{
					foreach (Wire wire2 in circuit.Wires)
					{
						wire2.InputPinTarget = null;
						wire2.OutputPinTarget = null;
					}
					circuit.Dirty = false;
				}
				return;
			}
		}
		List<ICircuitContainer> list = new List<ICircuitContainer>();
		list.AddRange(m_subGraphs.Where((Group g) => g.Dirty).AsIEnumerable<ICircuitContainer>());
		list.AddRange(m_circuits.Where((Circuit g) => g.Dirty).AsIEnumerable<ICircuitContainer>());
		while (m_subGraphs.Any((Group n) => n.Dirty) || m_circuits.Any((Circuit n) => n.Dirty))
		{
			foreach (Group item2 in m_subGraphs.OrderByDescending((Group s) => s.Level))
			{
				item2.IgnoreFanInOut = MovingCrossContainer;
				item2.Update();
				if (item2.IgnoreFanInOut)
				{
					item2.IgnoreFanInOut = false;
					item2.Dirty = true;
				}
			}
			UpdateWires(list);
			MovingCrossContainer = false;
			foreach (Circuit circuit2 in m_circuits)
			{
				circuit2.Update();
			}
		}
		foreach (Group item3 in from s in list.AsIEnumerable<Group>()
			orderby s.Level descending
			select s)
		{
			item3.UpdateGroupPinInfo();
		}
		foreach (Group key in m_nodesInserted.Keys)
		{
			IEnumerable<Element> enumerable = m_nodesInserted[key];
			if (enumerable.Any())
			{
				ViewingContext viewingContext = key.Cast<ViewingContext>();
				if (viewingContext.Control != null)
				{
					viewingContext.Control.As<GroupPinEditor>()?.AdjustLayout(enumerable, EmptyEnumerable<GroupPin>.Instance, new Point(0, 0));
				}
			}
		}
	}

	private void UpdateWires(IEnumerable<ICircuitContainer> containers)
	{
		foreach (ICircuitContainer container in containers)
		{
			Wire[] array = container.Wires.ToArray();
			foreach (Wire wire in array)
			{
				bool flag = container.Elements.Contains(wire.InputElement);
				bool flag2 = container.Elements.Contains(wire.OutputElement);
				if (flag && flag2)
				{
					if (wire.InputElement.Type is MissingElementType || wire.OutputElement.Type is MissingElementType)
					{
						continue;
					}
					Pair<Element, ICircuitPin> pair = default(Pair<Element, ICircuitPin>);
					foreach (Element element in container.Elements)
					{
						pair = element.FullyMatchPinTarget(wire.InputPinTarget, inputSide: true);
						if (pair.First != null)
						{
							break;
						}
					}
					Pair<Element, ICircuitPin> pair2 = default(Pair<Element, ICircuitPin>);
					foreach (Element element2 in container.Elements)
					{
						pair2 = element2.FullyMatchPinTarget(wire.OutputPinTarget, inputSide: false);
						if (pair2.First != null)
						{
							break;
						}
					}
					if (pair.First != null && pair2.First != null)
					{
						wire.InputElement = pair.First;
						wire.InputPin = pair.Second;
						wire.OutputElement = pair2.First;
						wire.OutputPin = pair2.Second;
						continue;
					}
				}
				if (MovingCrossContainer)
				{
					DomNode node = wire.InputPinTarget.InstancingNode ?? wire.InputPinTarget.LeafDomNode;
					DomNode node2 = wire.OutputPinTarget.InstancingNode ?? wire.OutputPinTarget.LeafDomNode;
					DomNode lowestCommonAncestor = DomNode.GetLowestCommonAncestor(node, node2);
					if (lowestCommonAncestor != null)
					{
						ICircuitContainer circuitContainer = lowestCommonAncestor.Cast<ICircuitContainer>();
						Pair<Element, ICircuitPin> pair3 = circuitContainer.FullyMatchPinTarget(wire.InputPinTarget, inputSide: true);
						Pair<Element, ICircuitPin> pair4 = circuitContainer.FullyMatchPinTarget(wire.OutputPinTarget, inputSide: false);
						if (circuitContainer.Is<Group>())
						{
							GroupPin groupPin = pair3.Second.Cast<GroupPin>();
							wire.InputElement = groupPin.InternalElement;
							wire.InputPin = groupPin.InternalElement.InputPin(groupPin.InternalPinIndex);
						}
						else
						{
							wire.InputElement = pair3.First;
							wire.InputPin = pair3.Second;
						}
						if (circuitContainer.Is<Group>())
						{
							GroupPin groupPin2 = pair4.Second.Cast<GroupPin>();
							wire.OutputElement = groupPin2.InternalElement;
							wire.OutputPin = groupPin2.InternalElement.OutputPin(groupPin2.InternalPinIndex);
						}
						else
						{
							wire.OutputElement = pair4.First;
							wire.OutputPin = pair4.Second;
						}
						if (container != circuitContainer)
						{
							container.Wires.Remove(wire);
							circuitContainer.Wires.Add(wire);
						}
					}
				}
				else
				{
					container.Wires.Remove(wire);
				}
			}
		}
	}

	protected override void OnEnded(object sender, EventArgs e)
	{
		ActiveHistoryContext = null;
		m_undoingOrRedoing = false;
	}

	private static void ValidateEdges(IGraph<Element, Wire, ICircuitPin> graph)
	{
		foreach (Wire edge in graph.Edges)
		{
			if (edge.InputElement.Type is MissingElementType || edge.OutputElement.Type is MissingElementType)
			{
				continue;
			}
			if (edge.InputElement.Is<Group>())
			{
				Group obj;
				if (edge.InputElement.Is<IReference<Group>>())
				{
					IReference<Group> reference = edge.InputElement.As<IReference<Group>>();
					obj = reference.Target;
				}
				else
				{
					obj = edge.InputElement.Cast<Group>();
				}
				GroupPin groupPin = obj.InputGroupPins.First((GroupPin x) => x.Index == edge.InputPin.Index);
				if (!(groupPin.PinTarget == edge.InputPinTarget))
				{
					string name = groupPin.PinTarget.LeafDomNode.Cast<Element>().Name;
					string name2 = edge.InputPinTarget.LeafDomNode.Cast<Element>().Name;
					bool flag = name == name2;
				}
			}
			else if (edge.InputPinTarget != null && edge.InputPinTarget.InstancingNode != null)
			{
			}
			if (edge.OutputElement.Is<Group>())
			{
				Group obj2;
				if (edge.OutputElement.Is<IReference<Group>>())
				{
					IReference<Group> reference2 = edge.OutputElement.As<IReference<Group>>();
					obj2 = reference2.Target;
				}
				else
				{
					obj2 = edge.OutputElement.Cast<Group>();
				}
				GroupPin groupPin2 = obj2.OutputGroupPins.First((GroupPin x) => x.Index == edge.OutputPin.Index);
				if (!(groupPin2.PinTarget == edge.OutputPinTarget))
				{
					string name3 = groupPin2.PinTarget.LeafDomNode.Cast<Element>().Name;
					string name4 = edge.OutputPinTarget.LeafDomNode.Cast<Element>().Name;
					bool flag2 = name3 == name4;
				}
			}
			else if (edge.OutputPinTarget != null && edge.OutputPinTarget.InstancingNode != null)
			{
			}
		}
	}

	private void AddSubtree(DomNode root)
	{
		foreach (DomNode item in root.Subtree)
		{
			if (CircuitUtil.IsGroupTemplateInstance(item))
			{
				Group groupTemplate = CircuitUtil.GetGroupTemplate(item);
				if (groupTemplate != null)
				{
					m_templateInstances.Add(groupTemplate.DomNode, item);
					m_subGraphs.Add(groupTemplate);
				}
			}
			else if (item.Is<Group>())
			{
				m_subGraphs.Add(item.Cast<Group>());
			}
			else if (item.Is<Circuit>())
			{
				m_circuits.Add(item.Cast<Circuit>());
			}
		}
	}

	private void RemoveSubtree(DomNode root)
	{
		foreach (DomNode item in root.Subtree)
		{
			if (CircuitUtil.IsGroupTemplateInstance(item))
			{
				Group groupTemplate = CircuitUtil.GetGroupTemplate(item);
				if (groupTemplate != null)
				{
					m_templateInstances.Remove(groupTemplate.DomNode, item);
				}
			}
			else if (item.Is<Group>())
			{
				m_subGraphs.Remove(item.Cast<Group>());
			}
			else if (item.Is<Circuit>())
			{
				m_circuits.Remove(item.Cast<Circuit>());
			}
		}
	}

	public void UpdateTemplateInfo(Group template)
	{
		if (template == null)
		{
			return;
		}
		List<ICircuitContainer> list = new List<ICircuitContainer>();
		foreach (DomNode item in m_templateInstances[template.DomNode])
		{
			ICircuitContainer circuitContainer = item.Parent.As<ICircuitContainer>();
			if (circuitContainer != null)
			{
				list.Add(circuitContainer);
			}
		}
		foreach (GroupPin inputGroupPin in template.InputGroupPins)
		{
			inputGroupPin.Info.ExternalConnected = false;
			foreach (ICircuitContainer item2 in list)
			{
				if (inputGroupPin.Info.ExternalConnected)
				{
					break;
				}
				foreach (Wire wire in item2.Wires)
				{
					if (wire.InputPinTarget == inputGroupPin.PinTarget)
					{
						inputGroupPin.Info.ExternalConnected = true;
						inputGroupPin.Visible = true;
						break;
					}
				}
			}
		}
		foreach (GroupPin outputGroupPin in template.OutputGroupPins)
		{
			outputGroupPin.Info.ExternalConnected = false;
			foreach (ICircuitContainer item3 in list)
			{
				if (outputGroupPin.Info.ExternalConnected)
				{
					break;
				}
				foreach (Wire wire2 in item3.Wires)
				{
					if (wire2.OutputPinTarget == outputGroupPin.PinTarget)
					{
						outputGroupPin.Info.ExternalConnected = true;
						outputGroupPin.Visible = true;
						break;
					}
				}
			}
		}
	}

	private void UpdateGroupPinConnectivity(Wire wire)
	{
		if (wire.InputElement == null || wire.OutputElement == null)
		{
			return;
		}
		List<DomNode> list = new List<DomNode>();
		if (CircuitUtil.IsGroupTemplateInstance(wire.InputElement.DomNode))
		{
			Group groupTemplate = CircuitUtil.GetGroupTemplate(wire.InputElement.DomNode);
			if (groupTemplate != null)
			{
				list.Add(groupTemplate.DomNode);
				UpdateTemplateInfo(groupTemplate);
			}
		}
		else if (wire.InputElement.DomNode.Is<Group>())
		{
			list.Add(wire.InputElement.DomNode);
			wire.InputElement.DomNode.Cast<Group>().UpdateGroupPinInfo();
		}
		if (CircuitUtil.IsGroupTemplateInstance(wire.OutputElement.DomNode))
		{
			Group groupTemplate2 = CircuitUtil.GetGroupTemplate(wire.OutputElement.DomNode);
			if (groupTemplate2 != null)
			{
				list.Add(groupTemplate2.DomNode);
				UpdateTemplateInfo(groupTemplate2);
			}
		}
		else if (wire.OutputElement.DomNode.Is<Group>())
		{
			list.Add(wire.OutputElement.DomNode);
			wire.OutputElement.DomNode.Cast<Group>().UpdateGroupPinInfo();
		}
		foreach (DomNode item in list)
		{
			item.As<CircuitEditingContext>()?.NotifyObjectChanged(item);
		}
	}
}
