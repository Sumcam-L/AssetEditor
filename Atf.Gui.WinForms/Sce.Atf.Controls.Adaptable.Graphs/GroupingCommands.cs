using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Sce.Atf.Controls.Adaptable.Graphs;

[InheritedExport(typeof(IInitializable))]
[InheritedExport(typeof(IContextMenuCommandProvider))]
[InheritedExport(typeof(GroupingCommands))]
[PartCreationPolicy(CreationPolicy.Any)]
public abstract class GroupingCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
{
	public enum GroupCreationOptions
	{
		None,
		HideUnconnectedPins
	}

	internal enum PlacementMode
	{
		Center,
		UpperLeft
	}

	private enum CommandTag
	{
		ShowExpandedGroupPins,
		TogglePinVisibility,
		ResetGroupPinNames,
		EdgeStyleDefault,
		EdgeStyleDirectCurve
	}

	private ICommandService m_commandService;

	private IContextRegistry m_contextRegistry;

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService = null;

	private Group.PinOrderStyle m_defaultPinOrderStyle;

	private WeakReference m_targetRef;

	protected abstract DomNodeType GroupType { get; }

	public Group.PinOrderStyle DefaultPinOrderStyle
	{
		get
		{
			return m_defaultPinOrderStyle;
		}
		set
		{
			m_defaultPinOrderStyle = value;
		}
	}

	public GroupCreationOptions CreationOptions { get; set; }

	internal static PlacementMode Placement { get; set; }

	protected IContextRegistry ContextRegistry => m_contextRegistry;

	[ImportingConstructor]
	public GroupingCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
		Placement = PlacementMode.UpperLeft;
	}

	public static void CreateGroup(Group newGroup, IEnumerable<object> elementsToGroup, ICircuitContainer graphContainer)
	{
		HashSet<Element> hashSet = new HashSet<Element>();
		List<Wire> list = new List<Wire>();
		List<Wire> list2 = new List<Wire>();
		CircuitUtil.GetSubGraph(graphContainer, elementsToGroup, hashSet, list, list2, list2);
		graphContainer.Elements.Add(newGroup);
		foreach (Element item in hashSet)
		{
			graphContainer.Elements.Remove(item);
			newGroup.Elements.Add(item);
		}
		newGroup.UpdateGroupPins(hashSet, list, list2);
		foreach (Wire item2 in list)
		{
			graphContainer.Wires.Remove(item2);
			newGroup.Wires.Add(item2);
		}
		newGroup.InitializeGroupPinIndexes(list);
		if (graphContainer.Is<Group>())
		{
			Group obj = graphContainer.Cast<Group>();
			foreach (GroupPin inputGroupPin in obj.InputGroupPins)
			{
				if (!hashSet.Contains(inputGroupPin.InternalElement))
				{
					continue;
				}
				foreach (GroupPin inputGroupPin2 in newGroup.InputGroupPins)
				{
					if (inputGroupPin2.InternalElement.DomNode == inputGroupPin.InternalElement.DomNode && inputGroupPin2.InternalPinIndex == inputGroupPin.InternalPinIndex)
					{
						inputGroupPin.InternalPinIndex = inputGroupPin2.Index;
						inputGroupPin2.Name = inputGroupPin.Name;
						break;
					}
				}
				inputGroupPin.InternalElement = newGroup;
			}
			foreach (GroupPin outputGroupPin in obj.OutputGroupPins)
			{
				if (!hashSet.Contains(outputGroupPin.InternalElement))
				{
					continue;
				}
				foreach (GroupPin outputGroupPin2 in newGroup.OutputGroupPins)
				{
					if (outputGroupPin2.InternalElement.DomNode == outputGroupPin.InternalElement.DomNode && outputGroupPin2.InternalPinIndex == outputGroupPin.InternalPinIndex)
					{
						outputGroupPin.InternalPinIndex = outputGroupPin2.Index;
						outputGroupPin2.Name = outputGroupPin.Name;
						break;
					}
				}
				outputGroupPin.InternalElement = newGroup;
			}
		}
		newGroup.OnChanged(EventArgs.Empty);
		foreach (Wire item3 in list2)
		{
			GroupPin groupPin = newGroup.MatchedGroupPin(item3.InputElement, item3.InputPin.Index, inputSide: true);
			if (groupPin != null)
			{
				groupPin.SetPinTarget(inputSide: true);
				item3.SetInput(newGroup, groupPin);
				item3.InputPinTarget = groupPin.PinTarget;
			}
			GroupPin groupPin2 = newGroup.MatchedGroupPin(item3.OutputElement, item3.OutputPin.Index, inputSide: false);
			if (groupPin2 != null)
			{
				groupPin2.SetPinTarget(inputSide: false);
				item3.SetOutput(newGroup, groupPin2);
				item3.OutputPinTarget = groupPin2.PinTarget;
			}
		}
		Point point = new Point(int.MaxValue, int.MaxValue);
		foreach (Element element in newGroup.Elements)
		{
			if (point.X > element.Bounds.Location.X)
			{
				point.X = element.Bounds.Location.X;
			}
			if (point.Y > element.Bounds.Location.Y)
			{
				point.Y = element.Bounds.Location.Y;
			}
		}
		foreach (Element element2 in newGroup.Elements)
		{
			Point location = element2.Bounds.Location;
			location.Offset(-point.X, -point.Y);
			element2.Bounds = new Rectangle(location, element2.Bounds.Size);
			element2.Position = element2.Bounds.Location;
		}
	}

	public static void UngroupGroup(Group group, ICircuitContainer circuitContainer)
	{
		foreach (Wire wire in circuitContainer.Wires)
		{
			if (wire.InputElement.As<Group>() == group)
			{
				GroupPin groupPin = wire.InputPin.As<GroupPin>();
				Element internalElement = groupPin.InternalElement;
				wire.SetInput(internalElement, internalElement.InputPin(groupPin.InternalPinIndex));
			}
			else if (wire.OutputElement.As<Group>() == group)
			{
				GroupPin groupPin2 = wire.OutputPin.As<GroupPin>();
				Element internalElement2 = groupPin2.InternalElement;
				wire.SetOutput(internalElement2, internalElement2.OutputPin(groupPin2.InternalPinIndex));
			}
		}
		IList<Element> elements = group.Elements;
		for (int num = elements.Count - 1; num >= 0; num--)
		{
			Element element = elements[num];
			elements.RemoveAt(num);
			element.Bounds = new Rectangle(element.Bounds.Location.X + group.Bounds.Location.X - group.Info.Offset.X, element.Bounds.Location.Y + group.Bounds.Location.Y - group.Info.Offset.Y, element.Bounds.Width, element.Bounds.Height);
			circuitContainer.Elements.Add(element);
		}
		IList<Wire> wires = group.Wires;
		for (int num2 = wires.Count - 1; num2 >= 0; num2--)
		{
			Wire item = wires[num2];
			wires.RemoveAt(num2);
			circuitContainer.Wires.Add(item);
		}
		circuitContainer.Elements.Remove(group);
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.EditGroup, this);
		m_commandService.RegisterCommand(CommandInfo.EditUngroup, this);
		CommandInfo commandInfo = m_commandService.RegisterCommand(CommandTag.ShowExpandedGroupPins, StandardMenu.Edit, StandardCommandGroup.EditOther, "Show Expanded Group Pins".Localize(), "Show Expanded Group Pins".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		commandInfo.CheckOnClick = true;
		m_commandService.RegisterCommand(CommandTag.ResetGroupPinNames, StandardMenu.Edit, StandardCommandGroup.EditOther, "Reset Group Pin Names".Localize(), "Reset Group Pin Names".Localize(), Keys.None, null, CommandVisibility.ContextMenu, this);
		if (m_scriptingService != null)
		{
			m_scriptingService.SetVariable("grpCmds", this);
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		bool flag = false;
		CircuitEditingContext activeContext = m_contextRegistry.GetActiveContext<CircuitEditingContext>();
		if (activeContext != null)
		{
			ISelectionContext activeContext2 = m_contextRegistry.GetActiveContext<ISelectionContext>();
			if (commandTag is StandardCommand)
			{
				if (StandardCommand.EditGroup.Equals(commandTag))
				{
					Element lastSelected = activeContext2.GetLastSelected<Element>();
					flag = lastSelected != null && activeContext2.Selection.All((object x) => x.Is<Element>() && x.Cast<DomNode>().Parent == lastSelected.DomNode.Parent);
					if (flag)
					{
						flag = activeContext2.Selection.All((object x) => !CircuitUtil.IsTemplateTargetMissing(x));
					}
					if (flag && !activeContext.SupportsNestedGroup)
					{
						flag = activeContext2.Selection.All((object x) => !x.Is<Group>());
						if (flag)
						{
							flag = lastSelected.DomNode.Ancestry.All((DomNode x) => !x.Is<Group>());
						}
					}
				}
				else if (StandardCommand.EditUngroup.Equals(commandTag))
				{
					flag = activeContext2.Selection.Any() && activeContext2.Selection.All((object x) => x.Is<Group>() && !x.Is<IReference<DomNode>>());
				}
			}
			else if (commandTag is CommandTag)
			{
				if (CommandTag.ShowExpandedGroupPins.Equals(commandTag))
				{
					flag = m_targetRef != null && m_targetRef.Target.Is<Group>() && m_targetRef.Target.Cast<Group>().Expanded;
				}
				else if (CommandTag.ResetGroupPinNames.Equals(commandTag))
				{
					flag = m_targetRef != null && m_targetRef.Target.Is<Group>() && m_targetRef.Target is IReference<DomNode>;
					if (flag)
					{
						flag = !CircuitUtil.IsTemplateTargetMissing(m_targetRef.Target);
					}
				}
				else if (CommandTag.TogglePinVisibility.Equals(commandTag))
				{
					flag = m_targetRef != null && CanDoTogglePinVisibility(activeContext, m_targetRef.Target);
				}
				else if (CommandTag.EdgeStyleDefault.Equals(commandTag))
				{
					flag = true;
				}
				else if (CommandTag.EdgeStyleDirectCurve.Equals(commandTag))
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	public virtual void DoCommand(object commandTag)
	{
		CircuitEditingContext circuitEditingContext = m_contextRegistry.GetActiveContext<CircuitEditingContext>();
		ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
		ViewingContext viewingContext = m_contextRegistry.GetActiveContext<ViewingContext>();
		ITransactionContext transactionContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
		if (transactionContext == null)
		{
			transactionContext = circuitEditingContext;
		}
		if (commandTag is StandardCommand)
		{
			if (StandardCommand.EditGroup.Equals(commandTag))
			{
				transactionContext.DoTransaction(delegate
				{
					CreateGroup(selectionContext, viewingContext);
				}, "Group".Localize("a verb"));
			}
			else if (StandardCommand.EditUngroup.Equals(commandTag))
			{
				transactionContext.DoTransaction(delegate
				{
					UngroupGroups(circuitEditingContext, selectionContext);
				}, "Ungroup".Localize("a verb"));
			}
		}
		else
		{
			if (!(commandTag is CommandTag))
			{
				return;
			}
			if (CommandTag.ShowExpandedGroupPins.Equals(commandTag))
			{
				transactionContext.DoTransaction(delegate
				{
					ToggleShowExpandedGroupPins(viewingContext);
				}, "Toggle Show Group Pins When Expanded".Localize());
			}
			else if (CommandTag.ResetGroupPinNames.Equals(commandTag))
			{
				transactionContext.DoTransaction(delegate
				{
					GroupResetPinNames();
				}, "Reset Group Pin Names".Localize());
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
		if (!(commandTag is CommandTag))
		{
			return;
		}
		if (commandTag.Equals(CommandTag.ResetGroupPinNames))
		{
			if (m_targetRef != null && m_targetRef.Target != null)
			{
				object target = m_targetRef.Target;
				if (target.Is<Group>() && !CircuitUtil.IsTemplateTargetMissing(target) && !target.Is<IReference<DomNode>>())
				{
					Group obj = target.Cast<Group>();
					state.Text = string.Format("Reset Pin Names on \"{0}\"".Localize(), obj.Name);
				}
			}
		}
		else if (commandTag.Equals(CommandTag.ShowExpandedGroupPins) && m_targetRef != null && m_targetRef.Target != null)
		{
			object target2 = m_targetRef.Target;
			if (target2.Is<Group>())
			{
				Group obj2 = target2.Cast<Group>();
				state.Check = obj2.Info.ShowExpandedGroupPins;
				state.Text = string.Format("Show Expanded Group Pins on \"{0}\"".Localize(), obj2.Name);
			}
		}
	}

	IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
	{
		m_targetRef = null;
		if (context.Is<CircuitEditingContext>())
		{
			m_targetRef = new WeakReference(target);
			if (CanDoCommand(StandardCommand.EditGroup))
			{
				yield return StandardCommand.EditGroup;
			}
			if (CanDoCommand(StandardCommand.EditUngroup))
			{
				yield return StandardCommand.EditUngroup;
			}
			if (CanToggleShowExpandedGroupPins(context, target))
			{
				yield return CommandTag.ShowExpandedGroupPins;
			}
			if (target.Is<Group>())
			{
				yield return CommandTag.ResetGroupPinNames;
			}
			if (CanDoTogglePinVisibility(context, target))
			{
				yield return CommandTag.TogglePinVisibility;
			}
			yield return CommandTag.EdgeStyleDefault;
			yield return CommandTag.EdgeStyleDirectCurve;
		}
	}

	private void CreateGroup(ISelectionContext selectionContext, ViewingContext viewingContext)
	{
		Group obj = new DomNode(GroupType).As<Group>();
		obj.DefaultPinOrder = DefaultPinOrderStyle;
		obj.DomNode.Type.SetTag((ICircuitElementType)obj);
		obj.Id = "Group".Localize("a noun");
		obj.Name = obj.Id;
		obj.ShowExpandedGroupPins = CircuitDefaultStyle.ShowExpandedGroupPins;
		DomNode domNode = selectionContext.LastSelected.As<DomNode>();
		DomDocument adaptable = domNode.GetRoot().Cast<DomDocument>();
		CircuitValidator circuitValidator = adaptable.Cast<CircuitValidator>();
		circuitValidator.Suspended = true;
		CircuitEditingContext circuitEditingContext = domNode.Parent.Cast<CircuitEditingContext>();
		if (Placement == PlacementMode.Center)
		{
			Rectangle bounds = viewingContext.GetBounds(selectionContext.Selection.AsIEnumerable<Element>());
			circuitEditingContext.Center(new object[1] { obj }, new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2));
		}
		else
		{
			Point point = new Point(int.MaxValue, int.MaxValue);
			foreach (Element item in selectionContext.Selection.AsIEnumerable<Element>())
			{
				if (point.X > item.Bounds.Location.X)
				{
					point.X = item.Bounds.Location.X;
				}
				if (point.Y > item.Bounds.Location.Y)
				{
					point.Y = item.Bounds.Location.Y;
				}
			}
			obj.Bounds = new Rectangle(point.X, point.Y, obj.Bounds.Width, obj.Bounds.Height);
			obj.Position = obj.Bounds.Location;
		}
		CreateGroup(obj, selectionContext.Selection, circuitEditingContext.CircuitContainer);
		if (CreationOptions == GroupCreationOptions.HideUnconnectedPins)
		{
			obj.UpdateGroupPinInfo();
			foreach (GroupPin inputGroupPin in obj.InputGroupPins)
			{
				inputGroupPin.Visible = inputGroupPin.Info.ExternalConnected;
			}
			foreach (GroupPin outputGroupPin in obj.OutputGroupPins)
			{
				outputGroupPin.Visible = outputGroupPin.Info.ExternalConnected;
			}
		}
		circuitEditingContext.Selection.Set(obj);
		circuitValidator.Suspended = false;
	}

	private static void UngroupGroups(CircuitEditingContext circuitEditingContext, ISelectionContext selectionContext)
	{
		ICircuitContainer circuitContainer = circuitEditingContext.CircuitContainer;
		List<object> list = new List<object>();
		foreach (Group item in selectionContext.Selection.AsIEnumerable<Group>())
		{
			list.AddRange(item.Elements);
			UngroupGroup(item, circuitContainer);
		}
		selectionContext.SetRange(list);
	}

	private bool CanToggleShowExpandedGroupPins(object context, object target)
	{
		if (target.Is<Group>())
		{
			ViewingContext viewingContext = context.Cast<CircuitEditingContext>().DomNode.Cast<ViewingContext>();
			if (viewingContext.Control != null)
			{
				ContextMenuAdapter contextMenuAdapter = viewingContext.Control.As<ContextMenuAdapter>();
				if (CommandService.ContextMenuIsTriggering && contextMenuAdapter != null)
				{
					foreach (IPickingAdapter2 item in viewingContext.Control.AsAll<IPickingAdapter2>())
					{
						DiagramHitRecord diagramHitRecord = item.Pick(contextMenuAdapter.TriggeringLocation.GetValueOrDefault());
						if (diagramHitRecord.SubItem.Is<Group>())
						{
							m_targetRef = new WeakReference(diagramHitRecord.SubItem);
							return true;
						}
					}
				}
			}
			return true;
		}
		return false;
	}

	private void ToggleShowExpandedGroupPins(ViewingContext viewingContext)
	{
		if (m_targetRef != null && m_targetRef.Target != null)
		{
			Group obj = m_targetRef.Target.Cast<Group>();
			obj.Info.ShowExpandedGroupPins = !obj.Info.ShowExpandedGroupPins;
			viewingContext.Control.Invalidate();
		}
	}

	private void GroupResetPinNames()
	{
		if (m_targetRef == null || m_targetRef.Target == null)
		{
			return;
		}
		Group obj = m_targetRef.Target.Cast<Group>();
		foreach (GroupPin inputGroupPin in obj.InputGroupPins)
		{
			inputGroupPin.IsDefaultName = true;
			inputGroupPin.Name = inputGroupPin.DefaultName(inputSide: true);
		}
		foreach (GroupPin outputGroupPin in obj.OutputGroupPins)
		{
			outputGroupPin.IsDefaultName = true;
			outputGroupPin.Name = outputGroupPin.DefaultName(inputSide: false);
		}
	}

	private bool CanDoTogglePinVisibility(object context, object target)
	{
		if (target.Is<Group>())
		{
			if (CircuitUtil.IsTemplateTargetMissing(target))
			{
				return false;
			}
			ViewingContext viewingContext = context.Cast<CircuitEditingContext>().DomNode.Cast<ViewingContext>();
			if (viewingContext.Control != null)
			{
				ContextMenuAdapter contextMenuAdapter = viewingContext.Control.As<ContextMenuAdapter>();
				if (CommandService.ContextMenuIsTriggering && contextMenuAdapter != null)
				{
					foreach (IPickingAdapter2 item in viewingContext.Control.AsAll<IPickingAdapter2>())
					{
						DiagramHitRecord diagramHitRecord = item.Pick(contextMenuAdapter.TriggeringLocation.GetValueOrDefault());
						if (diagramHitRecord.Part.Is<GroupPin>())
						{
							return true;
						}
						if (diagramHitRecord.SubPart.Is<GroupPin>())
						{
							return true;
						}
						if (diagramHitRecord.SubPart.Is<ElementType.Pin>())
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}
}
