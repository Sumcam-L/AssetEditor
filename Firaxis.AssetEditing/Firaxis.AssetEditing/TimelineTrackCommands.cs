using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

[Export(typeof(ITimelineTrackCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TimelineTrackCommands : ITimelineTrackCommands, ICommandClient, IInitializable
{
	private enum TrackCommands
	{
		AddTrigger,
		PasteTriggersHere
	}

	private enum TimelineCommands
	{
		AddTimeline,
		DeleteTimeline,
		AddTimelineForAllBoundAnimations,
		AddTrack,
		DeleteTrack
	}

	public static CommandInfo AddTrigger = new CommandInfo(TrackCommands.AddTrigger, StandardMenu.Edit, TimelineCommandGroup.TrackCommands, "Add &Trigger".Localize(), "Add trigger to track".Localize("Add Trigger"), Sce.Atf.Input.Keys.None, Resources.AddTriggerTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo PasteTriggersHere = new CommandInfo(TrackCommands.PasteTriggersHere, StandardMenu.Edit, StandardCommandGroup.EditCut, "Paste &Here".Localize(), "Paste copied triggers into track with adjusted time".Localize("Paste Here"), Sce.Atf.Input.Keys.None, Resources.PasteAtTimeTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo AddTimeline = new CommandInfo(TimelineCommands.AddTimeline, StandardMenu.Edit, TimelineCommandGroup.TimelineCommands, "Add Time&line".Localize(), "Add timeline for a slot".Localize("Add Timeline"), Sce.Atf.Input.Keys.None, Resources.AddTimelineTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo DeleteTimeline = new CommandInfo(TimelineCommands.DeleteTimeline, StandardMenu.Edit, TimelineCommandGroup.TimelineCommands, "Delete Time&line".Localize(), "Delete timeline for a slot".Localize("Delete Timeline"), Sce.Atf.Input.Keys.None, Resources.DeleteIcon, CommandVisibility.ControlDefault);

	public static CommandInfo AddTimelineForAllBoundAnimations = new CommandInfo(TimelineCommands.AddTimelineForAllBoundAnimations, StandardMenu.Edit, TimelineCommandGroup.TimelineCommands, "Add All Bound &Animations".Localize(), "Add a timeline for all bound animation slots that do not already have an existing timeline".Localize(), Sce.Atf.Input.Keys.None, Resources.AnimationFileIcon, CommandVisibility.ControlDefault);

	public static CommandInfo AddTrack = new CommandInfo(TimelineCommands.AddTrack, StandardMenu.Edit, TimelineCommandGroup.TimelineCommands, "Add Trac&k".Localize(), "Add track to timeline".Localize("Add Track"), Sce.Atf.Input.Keys.None, Resources.AddTrackTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo DeleteTrack = new CommandInfo(TimelineCommands.DeleteTrack, StandardMenu.Edit, TimelineCommandGroup.TimelineCommands, "Delete Trac&k".Localize(), "Delete track from timeline".Localize("Delete Track"), Sce.Atf.Input.Keys.None, Resources.DeleteTrackTimelineIcon, CommandVisibility.ControlDefault);

	private ICommandService CommandService { get; set; }

	private IContextRegistry ContextRegistry { get; set; }

	private StandardEditCommands EditCommands { get; set; }

	private ISelectionContext SelectionContext { get; set; }

	private IInstancingContext InstancingContext { get; set; }

	private ITransactionContext TransactionContext { get; set; }

	private IBehaviorProviderAdapter BehaviorAdapter { get; set; }

	private DomNode Selection { get; set; }

	[Import(AllowDefault = true)]
	private MainForm MainWindow { get; set; }

	public float TargetTime { get; set; }

	public IEnumerable<CommandInfo> Commands { get; } = new CommandInfo[7] { AddTrigger, PasteTriggersHere, AddTimeline, AddTimelineForAllBoundAnimations, AddTrack, DeleteTimeline, DeleteTrack };

	private IEnumerable<string> AvailableAnimationSlotNames
	{
		get
		{
			if (BehaviorAdapter?.DSGInst == null)
			{
				return Enumerable.Empty<string>();
			}
			IEnumerable<string> animationSlots = BehaviorAdapter.DSGInst.GetAnimationSlots();
			IEnumerable<string> second = from sBind in BehaviorAdapter.AnimationBindingSet.AnimationBindings
				where !string.IsNullOrEmpty(sBind.AnimationName) && BehaviorAdapter.TimelineSet.Timelines.Any((TimelineAdapter wTL) => wTL.AnimationName == sBind.AnimationName)
				select sBind.SlotName;
			return animationSlots.Except(second);
		}
	}

	private IEnumerable<string> AvailableTimelineSlotNames
	{
		get
		{
			if (BehaviorAdapter?.DSGInst == null)
			{
				return Enumerable.Empty<string>();
			}
			IEnumerable<string> timelineSlots = BehaviorAdapter.DSGInst.GetTimelineSlots();
			IEnumerable<string> second = from sObj in BehaviorAdapter.TimelineBindingSet.TimelineBindings
				where !string.IsNullOrEmpty(sObj.TimelineName)
				select sObj.SlotName;
			return timelineSlots.Except(second);
		}
	}

	[ImportingConstructor]
	public TimelineTrackCommands(ICommandService commandService, IContextRegistry contextRegistry, StandardEditCommands stdEditCmds)
	{
		CommandService = commandService;
		ContextRegistry = contextRegistry;
		EditCommands = stdEditCmds;
	}

	public void Initialize()
	{
		CommandService.RegisterCommand(AddTrigger, this);
		CommandService.RegisterCommand(PasteTriggersHere, this);
		CommandService.RegisterCommand(AddTimeline, this);
		CommandService.RegisterCommand(DeleteTimeline, this);
		CommandService.RegisterCommand(AddTimelineForAllBoundAnimations, this);
		CommandService.RegisterCommand(AddTrack, this);
		CommandService.RegisterCommand(DeleteTrack, this);
		CommandService.UnregisterCommand(StandardCommand.EditDelete, this);
		ContextRegistry.ActiveContextChanged += ContextRegistry_ActiveContextChanged;
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (commandTag is TrackCommands)
		{
			return CanDoTrackCommand((TrackCommands)commandTag);
		}
		if (commandTag is TimelineCommands)
		{
			return CanDoTimelineCommand((TimelineCommands)commandTag);
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is TrackCommands)
		{
			DoTrackCommand((TrackCommands)commandTag);
		}
		else if (commandTag is TimelineCommands)
		{
			DoTimelineCommand((TimelineCommands)commandTag);
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void ContextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		Bind(ContextRegistry.ActiveContext);
	}

	private void Bind(object adapter)
	{
		if (SelectionContext != null)
		{
			Selection = null;
			SelectionContext.SelectionChanged -= SelectionContext_SelectionChanged;
		}
		TargetTime = 0f;
		TransactionContext = adapter.As<ITransactionContext>();
		SelectionContext = adapter.As<ISelectionContext>();
		InstancingContext = adapter.As<IInstancingContext>();
		BehaviorAdapter = adapter.As<IBehaviorProviderAdapter>();
		if (SelectionContext != null)
		{
			Selection = SelectionContext.LastSelected as DomNode;
			SelectionContext.SelectionChanged += SelectionContext_SelectionChanged;
		}
	}

	private void SelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		Selection = SelectionContext.LastSelected as DomNode;
	}

	private bool IsBehaviorProviderBound()
	{
		return BehaviorAdapter != null;
	}

	private bool BehaviorHasDSG()
	{
		return BehaviorAdapter?.DSGInst != null;
	}

	private bool AnyBoundAnimationsWithoutTimelines()
	{
		if (BehaviorAdapter != null)
		{
			return BehaviorAdapter.AnimationBindingSet.AnimationBindings.Any((AnimationBindingAdapter binding) => !string.IsNullOrEmpty(binding.AnimationName) && BehaviorAdapter.TimelineSet.FindTimeline(binding.SlotName) == null);
		}
		return false;
	}

	private void AddTimelinesForBoundAnimations()
	{
		IEnumerable<AnimationBindingAdapter> timelinesToAdd = BehaviorAdapter.AnimationBindingSet.AnimationBindings.Where((AnimationBindingAdapter binding) => !string.IsNullOrEmpty(binding.AnimationName) && BehaviorAdapter.TimelineSet.FindTimeline(binding.SlotName) == null);
		TransactionContext.DoTransaction(delegate
		{
			AnimationBindingAdapter[] array = timelinesToAdd.ToArray();
			foreach (AnimationBindingAdapter animationBindingAdapter in array)
			{
				BehaviorAdapter.TimelineSet.AddTimeline(animationBindingAdapter.SlotName).AnimationName = animationBindingAdapter.AnimationName;
			}
		}, "Add animation timelines".Localize());
	}

	private bool IsTimelineSelected()
	{
		if (Selection != null && BehaviorAdapter != null)
		{
			AnimationBindingAdapter aba = Selection.As<AnimationBindingAdapter>();
			if (aba != null)
			{
				return BehaviorAdapter.TimelineSet.Timelines.Any((TimelineAdapter item) => item.Name == aba.SlotName);
			}
			TimelineBindingAdapter tba = Selection.As<TimelineBindingAdapter>();
			if (tba != null)
			{
				return BehaviorAdapter.TimelineSet.Timelines.Any((TimelineAdapter item) => item.Name == tba.SlotName);
			}
		}
		return false;
	}

	private bool IsTrackSelected()
	{
		return Selection?.Is<TrackAdapter>() ?? false;
	}

	private bool IsTriggerSelected()
	{
		if (Selection == null)
		{
			return false;
		}
		if (Selection.Is<TriggerAdapter>())
		{
			return true;
		}
		return Selection.Is<IEnumerable<TriggerAdapter>>();
	}

	private TrackAdapter GetSelectedTrack()
	{
		if (IsTrackSelected())
		{
			return Selection.As<TrackAdapter>();
		}
		if (IsTriggerSelected())
		{
			return Selection.As<TriggerAdapter>().TrackAdapter;
		}
		return null;
	}

	private bool CanDoTrackCommand(TrackCommands trackCommand)
	{
		switch (trackCommand)
		{
		default:
			BugSubmitter.Assert(condition: false, "Unknown TimelineTreeCommand enum! Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum");
			return false;
		case TrackCommands.PasteTriggersHere:
			return EditCommands.CanPaste();
		case TrackCommands.AddTrigger:
			if (!IsTrackSelected())
			{
				return IsTriggerSelected();
			}
			return true;
		}
	}

	private bool CanDoTimelineCommand(TimelineCommands timelineCommand)
	{
		switch (timelineCommand)
		{
		case TimelineCommands.AddTimeline:
			if (IsBehaviorProviderBound() && BehaviorHasDSG())
			{
				if (!AvailableAnimationSlotNames.Any())
				{
					return AvailableTimelineSlotNames.Any();
				}
				return true;
			}
			return false;
		case TimelineCommands.DeleteTimeline:
			return EditCommands.CanDelete();
		case TimelineCommands.AddTimelineForAllBoundAnimations:
			return AnyBoundAnimationsWithoutTimelines();
		case TimelineCommands.AddTrack:
			if (!IsTimelineSelected() && !IsTrackSelected())
			{
				return IsTriggerSelected();
			}
			return true;
		case TimelineCommands.DeleteTrack:
			if (IsTrackSelected())
			{
				return EditCommands.CanDelete();
			}
			return false;
		default:
			BugSubmitter.Assert(condition: false, "Unknown TimelineTreeCommand enum! Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum");
			return false;
		}
	}

	private void DoTrackCommand(TrackCommands trackCommand)
	{
		switch (trackCommand)
		{
		default:
			BugSubmitter.Assert(condition: false, "Unknown TimelineTreeCommand enum! Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum");
			break;
		case TrackCommands.PasteTriggersHere:
		{
			BugSubmitter.Assert(TransactionContext != null, "No transaction context available");
			ITriggerInstancingContext triggerInstancer = BehaviorAdapter.As<ITriggerInstancingContext>();
			BugSubmitter.Assert(triggerInstancer != null, "BehaviorAdapter can not provide ITriggerInstancingContext interface");
			TransactionContext.DoTransaction(delegate
			{
				triggerInstancer.InsertAtTime(TargetTime, EditCommands.Clipboard);
			}, "Paste Triggers at Time".Localize());
			break;
		}
		case TrackCommands.AddTrigger:
		{
			BugSubmitter.Assert(TransactionContext != null, "No transaction context available");
			TrackAdapter targetTrack = GetSelectedTrack();
			TransactionContext.DoTransaction(delegate
			{
				TriggerAdapter triggerAdapter = targetTrack.AddTrigger(TargetTime);
				if (triggerAdapter != null && SelectionContext != null)
				{
					SelectionContext.Set(triggerAdapter.DomNode);
				}
			}, "Add Trigger".Localize());
			break;
		}
		}
	}

	private string UserSelectedSlotName()
	{
		IEnumerable<string> enumerable = AvailableAnimationSlotNames.Union(AvailableTimelineSlotNames);
		if (enumerable.Any())
		{
			SelectTimelineSlotDialog selectTimelineSlotDialog = new SelectTimelineSlotDialog(enumerable);
			if (selectTimelineSlotDialog.ShowDialog(MainWindow) == DialogResult.OK)
			{
				return selectTimelineSlotDialog.SelectedSlot;
			}
		}
		return string.Empty;
	}

	private TriggerType GetTriggerTypeFromUser()
	{
		SelectTriggerTypeDialog selectTriggerTypeDialog = new SelectTriggerTypeDialog();
		if (selectTriggerTypeDialog.ShowDialog(MainWindow) == DialogResult.OK)
		{
			return selectTriggerTypeDialog.SelectedType;
		}
		return TriggerType.TT_COUNT;
	}

	private void AddTimelineForSlot(string slotName)
	{
		TransactionContext.DoTransaction(delegate
		{
			AnimationBindingAdapter animationBindingAdapter = BehaviorAdapter.AnimationBindingSet.FindOrCreateAnimationBinding(slotName);
			if (string.IsNullOrEmpty(animationBindingAdapter.AnimationName))
			{
				BehaviorAdapter.TimelineBindingSet.FindOrCreateBinding(slotName, slotName).TimelineName = slotName;
			}
			TimelineAdapter timelineAdapter = BehaviorAdapter.TimelineSet.AddTimeline(slotName);
			if (!string.IsNullOrEmpty(animationBindingAdapter.AnimationName))
			{
				timelineAdapter.AnimationName = animationBindingAdapter.AnimationName;
			}
		}, "Add Timeline".Localize());
	}

	private void AddTrackLikeSelection()
	{
		TrackAdapter baseTrack = GetSelectedTrack();
		TransactionContext.DoTransaction(delegate
		{
			baseTrack.Timeline.AddTrack(baseTrack.TriggerType);
		}, "Add Track (Like)".Localize());
	}

	private void AddTrackForType(TriggerType tt)
	{
		ITimelineBindingAdapter timelineBindingAdapter = Selection.As<ITimelineBindingAdapter>();
		TimelineAdapter timeline = BehaviorAdapter.TimelineSet.FindTimeline(timelineBindingAdapter.SlotName);
		TransactionContext.DoTransaction(delegate
		{
			timeline.AddTrack(tt);
		}, "Add Track (UI)".Localize());
	}

	private void DoTimelineCommand(TimelineCommands timelineCommand)
	{
		switch (timelineCommand)
		{
		case TimelineCommands.AddTimeline:
		{
			BugSubmitter.Assert(TransactionContext != null, "No transaction context available");
			string text = UserSelectedSlotName();
			if (!string.IsNullOrEmpty(text))
			{
				AddTimelineForSlot(text);
			}
			break;
		}
		case TimelineCommands.DeleteTimeline:
			EditCommands.Delete();
			break;
		case TimelineCommands.AddTimelineForAllBoundAnimations:
			BugSubmitter.Assert(TransactionContext != null, "No transaction context available");
			AddTimelinesForBoundAnimations();
			break;
		case TimelineCommands.AddTrack:
			if (IsTrackSelected() || IsTriggerSelected())
			{
				AddTrackLikeSelection();
			}
			else if (IsTimelineSelected())
			{
				TriggerType triggerTypeFromUser = GetTriggerTypeFromUser();
				if (triggerTypeFromUser != TriggerType.TT_COUNT)
				{
					AddTrackForType(triggerTypeFromUser);
				}
			}
			break;
		case TimelineCommands.DeleteTrack:
			EditCommands.Delete();
			break;
		default:
			BugSubmitter.Assert(condition: false, "Unknown TimelineTreeCommand enum! Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum");
			break;
		}
	}
}
