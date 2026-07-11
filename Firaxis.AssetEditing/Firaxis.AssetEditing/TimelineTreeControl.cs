using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.Asset.Properties;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;
using Firaxis.Controls;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TimelineTreeControl : UserControl
{
	private EditingContext m_context;

	private IObservableContext m_observableContext;

	private IBehaviorProviderAdapter m_behaviorAdapter;

	private CommandControl m_commandControl;

	private TimeLineControl m_timeline;

	private TimelineTreeCommands m_treeCommands;

	private TimelinePreviewCommands m_previewCommands;

	private BufferedObservableHandler m_observableBuffer = new BufferedObservableHandler();

	private static IDictionary<TriggerType, Image> s_trackImages;

	private static readonly char[] s_pathDelimiters;

	private ICommandService CommandService { get; set; }

	private IAnimationKnobService AnimationKnobService { get; set; }

	private ITimelineTrackCommands TimelineTrackCommands { get; set; }

	private ITimelinePlaybackService TimelinePlaybackService { get; set; }

	private StandardEditCommands StandardEditCommands { get; set; }

	static TimelineTreeControl()
	{
		s_trackImages = new Dictionary<TriggerType, Image>();
		s_pathDelimiters = new char[2] { '/', '\\' };
		s_trackImages[TriggerType.TT_SOUND] = Firaxis.Asset.Properties.Resources.trig_slist;
		s_trackImages[TriggerType.TT_LIGHT] = ResourceUtil.GetImage16(Resources.AnalyticLightFileIcon);
		s_trackImages[TriggerType.TT_ASSET_VFX] = ResourceUtil.GetImage16(Resources.AssetFileIcon);
		s_trackImages[TriggerType.TT_ARTDEF_VFX] = ResourceUtil.GetImage16(Resources.ArtDefFileIcon);
		s_trackImages[TriggerType.TT_TRANSFER] = Firaxis.Asset.Properties.Resources.file_refresh;
		s_trackImages[TriggerType.TT_ACTION] = Firaxis.Asset.Properties.Resources.flagged;
	}

	public TimelineTreeControl(ICommandService cmdSvc, IAnimationKnobService animKnobSvc, ITimelinePlaybackService tlPlyBckSvc, ITimelineTrackCommands timeTrackCmds, StandardEditCommands stdEditCmds)
	{
		CommandService = cmdSvc;
		StandardEditCommands = stdEditCmds;
		TimelineTrackCommands = timeTrackCmds;
		AnimationKnobService = animKnobSvc;
		TimelinePlaybackService = tlPlyBckSvc;
		if (AnimationKnobService != null)
		{
			AnimationKnobService.CurrentTimeChanged += AnimationKnobService_CurrentTimeChanged;
		}
		InitializeControls();
		RegisterCommands();
		base.VisibleChanged += TimelineTreeControl_VisibleChanged;
		m_timeline.ScrollableTree.SelectedNodes.AddedItem += SelectedNodes_AddedItem;
		m_timeline.TimeRuler.UserTimeChanged += TimeRuler_UserTimeChanged;
		m_timeline.ScrollableTree.SelectedItemChanged += ScrollableTree_SelectedItemChanged;
		m_timeline.ScrollableTree.ContextMenuItem += ScrollableTree_ContextMenuItem;
		m_timeline.TimeTrack.SelectedKeys.ItemCountChanged += SelectedKeys_ItemCountChanged;
		m_timeline.TimeTrack.MovingKeyBegin += TimeTrack_MovingKeyBegin;
		m_timeline.TimeTrack.MovingKeyEnd += TimeTrack_MovingKeyEnd;
		m_timeline.TimeTrack.ContextMenuKey += TimeTrack_ContextMenuKey;
		m_timeline.ScrollableTree.MouseDoubleClick += Timeline_doubleclick;
		if (AnimationKnobService != null)
		{
			AnimationKnobService.KnobControllerCreated += AnimationKnobService_KnobControllerCreated;
		}
	}

	private void AnimationKnobService_KnobControllerCreated(object sender, EventArgs e)
	{
		if ((sender as IAnimationKnobService).TimelineStateTransitions.Count != 0 && m_timeline.ScrollableTree.Root.Count > 0)
		{
			m_timeline.ScrollableTree.Clear();
			BindTimelines();
		}
	}

	private void Timeline_doubleclick(object sender, MouseEventArgs e)
	{
		if (TimelinePlaybackService != null)
		{
			UpdateSelectionContext();
			TimelinePlaybackService.Playing = false;
			TimelinePlaybackService.CurrentTime = 0f;
			TimelinePlaybackService.Playing = true;
		}
	}

	private void RegisterCommands()
	{
		if (m_previewCommands != null)
		{
			m_commandControl.RegisterCommandClient(m_previewCommands, m_previewCommands.Commands);
		}
		m_commandControl.RegisterCommandClient(TimelineTrackCommands, TimelineTrackCommands.Commands);
	}

	private void SelectedNodes_AddedItem(object sender, ListEvent<ScrollableTree.TreeNode>.ListEventArgs e)
	{
		BugSubmitter.SilentAssert(e.Item != null, "Added null TreeNode to ScrollableTree! @assign bwhitman");
	}

	private void InitializeControls()
	{
		SuspendLayout();
		m_timeline = new TimeLineControl();
		m_timeline.Dock = DockStyle.Fill;
		m_timeline.AllowEdit = true;
		m_timeline.ScrollableTree.EnableExpandDoubleClick = false;
		m_commandControl = new CommandControl();
		m_commandControl.Dock = DockStyle.Fill;
		if (AnimationKnobService != null && TimelinePlaybackService != null)
		{
			m_previewCommands = new TimelinePreviewCommands(CommandService, AnimationKnobService, TimelinePlaybackService, m_commandControl);
		}
		m_treeCommands = new TimelineTreeCommands(CommandService, m_timeline);
		m_commandControl.ChildControls.Add(m_timeline);
		base.Controls.Add(m_commandControl);
		ResumeLayout(performLayout: false);
		PerformLayout();
	}

	private void TimelineTreeControl_VisibleChanged(object sender, EventArgs e)
	{
		if (base.Visible)
		{
			ApplyActiveSkin();
		}
	}

	private void ApplyActiveSkin()
	{
		SkinService.ApplyActiveSkin(this);
		foreach (ScrollableTree.TreeNode item in m_timeline.ScrollableTree.Root)
		{
			ApplyActiveSkinToNode(item);
		}
	}

	private void ApplyActiveSkinToNode(ScrollableTree.TreeNode node)
	{
		SkinService.ApplyActiveSkin(node.Item);
		foreach (ScrollableTree.TreeNode child in node.Children)
		{
			ApplyActiveSkinToNode(child);
		}
	}

	public void Bind(EditingContext context)
	{
		if (m_observableContext != null)
		{
			m_observableContext.Reloaded -= BehaviorAdapter_Reloaded;
			m_observableContext.ItemChanged -= BehaviorAdapter_ItemChanged;
			m_observableContext.ItemInserted -= BehaviorAdapter_ItemInserted;
			m_observableContext.ItemRemoved -= BehaviorAdapter_ItemRemoved;
		}
		UnbindTimelines();
		m_context = context;
		m_observableContext = m_context.As<IObservableContext>();
		m_behaviorAdapter = m_context.As<IBehaviorProviderAdapter>();
		m_previewCommands?.Bind(context);
		BindTimelines();
		if (m_observableContext != null)
		{
			m_observableContext.ItemChanged += BehaviorAdapter_ItemChanged;
			m_observableContext.ItemInserted += BehaviorAdapter_ItemInserted;
			m_observableContext.ItemRemoved += BehaviorAdapter_ItemRemoved;
			m_observableContext.Reloaded += BehaviorAdapter_Reloaded;
		}
	}

	private void BindTimelines()
	{
		if (m_behaviorAdapter == null)
		{
			return;
		}
		m_timeline.ScrollableTree.BeginUpdate();
		foreach (TimelineAdapter timeline in m_behaviorAdapter.TimelineSet.Timelines)
		{
			BindTimelineAdapterImpl(timeline);
		}
		SortTimelines(m_timeline.ScrollableTree.Root);
		m_timeline.ScrollableTree.EndUpdate();
	}

	private void BindTimelineAdapter(TimelineAdapter timeline)
	{
		m_timeline.ScrollableTree.BeginUpdate();
		BindTimelineAdapterImpl(timeline);
		SortTimelines(m_timeline.ScrollableTree.Root);
		m_timeline.ScrollableTree.EndUpdate();
	}

	private void BindTimelineAdapterImpl(TimelineAdapter timeline)
	{
		AnimationBindingAdapter animationBindingAdapter = m_behaviorAdapter.AnimationBindingSet.AnimationBindings.FirstOrDefault((AnimationBindingAdapter ab) => ab.SlotName == timeline.Name && !string.IsNullOrEmpty(ab.AnimationName) && ab.AnimationName == timeline.AnimationName);
		if (animationBindingAdapter != null)
		{
			BindAnimation(timeline, animationBindingAdapter);
			return;
		}
		TimelineBindingAdapter timelineBindingAdapter = m_behaviorAdapter.TimelineBindingSet.TimelineBindings.FirstOrDefault((TimelineBindingAdapter tb) => tb.TimelineName == timeline.Name);
		if (timelineBindingAdapter != null)
		{
			BindTimeline(timeline, timelineBindingAdapter);
		}
		else
		{
			BindTimeline(timeline, null);
		}
	}

	private void BehaviorAdapter_Reloaded(object sender, EventArgs e)
	{
		if (e is TimelineReloadEvent { Timeline: var timeline })
		{
			RemoveTimelineFromTree(timeline);
			BindTimelineAdapter(timeline);
			m_timeline.ScrollableTree.Invalidate();
			m_timeline.TimeTrack.Invalidate();
		}
	}

	private void BehaviorAdapter_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		if (e.Item.Is<TriggerAdapter>())
		{
			m_timeline.TimeTrack.Invalidate();
		}
		else if (e.Item.Is<TrackAdapter>())
		{
			TimelineAdapter timeline = e.Parent.As<TimelineAdapter>();
			TrackAdapter track = e.Item.As<TrackAdapter>();
			AddTrackToTree(timeline, track);
		}
		else if (e.Item.Is<TimelineAdapter>())
		{
			BindTimelineAdapter(e.Item.As<TimelineAdapter>());
		}
	}

	private void BehaviorAdapter_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		if (e.Item.Is<TriggerAdapter>())
		{
			m_timeline.TimeTrack.Invalidate();
		}
		else if (e.Item.Is<TrackAdapter>())
		{
			TimelineAdapter timeline = e.Parent.As<TimelineAdapter>();
			TrackAdapter track = e.Item.As<TrackAdapter>();
			RemoveTrackFromTree(timeline, track);
		}
		else if (e.Item.Is<TimelineAdapter>())
		{
			RemoveTimelineFromTree(e.Item.As<TimelineAdapter>());
		}
	}

	private void AddTrackToTree(TimelineAdapter timeline, TrackAdapter track)
	{
		ScrollableTree.TreeNode treeNode = FindTimelineTreeNode(timeline);
		if (treeNode != null)
		{
			m_timeline.ScrollableTree.BeginUpdate();
			ScrollableTree.TreeNode node = AddTrackToTreeImpl(treeNode, timeline, track);
			SortTimelineChidren(treeNode);
			m_timeline.ScrollableTree.EndUpdate();
			ApplyActiveSkinToNode(node);
			m_timeline.ScrollableTree.EnsureVisible(node);
		}
	}

	private ScrollableTree.TreeNode AddTrackToTreeImpl(ScrollableTree.TreeNode parentNode, TimelineAdapter timeline, TrackAdapter track)
	{
		return m_timeline.ScrollableTree.Add(parentNode, new ScrollableItemTrack(m_context, timeline, track, Font, s_trackImages[track.TriggerType]));
	}

	private void RemoveTimelineFromTree(TimelineAdapter timeline)
	{
		ScrollableTree.TreeNode treeNode = FindTimelineTreeNode(timeline);
		BugSubmitter.Assert(treeNode != null, "Failed to find node in tree node");
		m_timeline.ScrollableTree.BeginUpdate();
		m_timeline.ScrollableTree.Root.Remove(treeNode);
		m_timeline.ScrollableTree.EndUpdate();
	}

	private void RemoveTrackFromTree(TimelineAdapter timeline, TrackAdapter track)
	{
		ScrollableTree.TreeNode treeNode = FindTimelineTreeNode(timeline);
		if (treeNode != null)
		{
			m_timeline.ScrollableTree.BeginUpdate();
			RemoveTrackFromTreeImpl(treeNode, timeline, track);
			SortTimelineChidren(treeNode);
			m_timeline.ScrollableTree.EndUpdate();
		}
	}

	private void RemoveTrackFromTreeImpl(ScrollableTree.TreeNode parentNode, TimelineAdapter timeline, TrackAdapter track)
	{
		ScrollableTree.TreeNode item = FindTrackTreeNode(parentNode, track);
		parentNode.Children.Remove(item);
	}

	private ScrollableTree.TreeNode FindTimelineTreeNode(TimelineAdapter timeline)
	{
		return m_timeline.ScrollableTree.Root.FirstOrDefault((ScrollableTree.TreeNode node) => node.Item.As<ITimelineBoundItem>()?.Timeline == timeline);
	}

	private ScrollableTree.TreeNode FindTrackTreeNode(ScrollableTree.TreeNode timelineNode, TrackAdapter trackAdapter)
	{
		return timelineNode.Children.FirstOrDefault((ScrollableTree.TreeNode node) => node.Item.As<ITrackAdapterTreeItem>()?.Adapter == trackAdapter);
	}

	private void BehaviorAdapter_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		if (e.Item.Is<TriggerAdapter>() || e.Item.Is<TimelineAdapter>() || e.Item.Is<AnimationBindingAdapter>() || e.Item.Is<TimelineBindingAdapter>())
		{
			m_timeline.TimeTrack.Invalidate();
			return;
		}
		DomNode adaptable = e.Item.As<DomNode>()?.Parent;
		if (adaptable.Is<TrackAdapter>() || adaptable.Is<TimelineAdapter>() || adaptable.Is<AnimationBindingAdapter>() || adaptable.Is<TimelineBindingAdapter>())
		{
			m_timeline.ScrollableTree.Invalidate();
		}
	}

	private bool IsTrackSelected()
	{
		if (m_context == null)
		{
			return false;
		}
		if (m_context.SelectionCount == 1)
		{
			return m_context.LastSelected.Is<TrackAdapter>();
		}
		return false;
	}

	private bool AtLeastOneTriggersSelected()
	{
		if (m_context == null)
		{
			return false;
		}
		if (m_context.SelectionCount < 1)
		{
			return false;
		}
		return m_context.Selection.All((object sel) => sel.Is<TriggerAdapter>());
	}

	private bool IsTimelineSelected()
	{
		if (m_context == null)
		{
			return false;
		}
		if (m_context.SelectionCount == 1)
		{
			if (!m_context.LastSelected.Is<ITimelineBindingAdapter>())
			{
				return m_context.LastSelected.Is<TimelineAdapter>();
			}
			return true;
		}
		return false;
	}

	private void AnimationKnobService_CurrentTimeChanged(object sender, EventArgs e)
	{
		if (TimelinePlaybackService != null && m_timeline.TimeRuler.CurrentTime != TimelinePlaybackService.CurrentTime)
		{
			TimeRulerControl timeRuler = m_timeline.TimeRuler;
			float num = (TimelineTrackCommands.TargetTime = TimelinePlaybackService.CurrentTime);
			float currentTime2 = num;
			timeRuler.CurrentTime = currentTime2;
		}
	}

	private void TimeRuler_UserTimeChanged(object sender, TimeRulerControl.TimeRulerEventArgs e)
	{
		if ((IsTimelineSelected() || IsTrackSelected() || AtLeastOneTriggersSelected()) && TimelinePlaybackService != null)
		{
			if (TimelinePlaybackService.Playing)
			{
				TimelinePlaybackService.Playing = false;
			}
			ITimelineTrackCommands timelineTrackCommands = TimelineTrackCommands;
			float num = (TimelinePlaybackService.CurrentTime = m_timeline.TimeRuler.CurrentTime);
			float targetTime = num;
			timelineTrackCommands.TargetTime = targetTime;
		}
	}

	private void SelectedKeys_ItemCountChanged(object sender, ListEvent<IKey>.ListEventArgs e)
	{
		UpdateSelectionContext();
	}

	private void ScrollableTree_SelectedItemChanged(object sender, ScrollableTree.TreeNodeEventArgs e)
	{
		UpdateSelectionContext();
	}

	private void ScrollableTree_ContextMenuItem(object sender, ScrollableTree.TreeNodeEventArgs e)
	{
		ShowTriggerContextMenu(Cursor.Position, setRulerTime: false);
	}

	private void TimeTrack_ContextMenuKey(object sender, ScrollableTree.TreeNode node, EventArgs e)
	{
		ShowTriggerContextMenu(Cursor.Position, setRulerTime: true);
	}

	private void ShowTriggerContextMenu(Point location, bool setRulerTime)
	{
		List<CommandInfo> list = new List<CommandInfo>
		{
			CommandInfo.EditCopy,
			CommandInfo.EditCut,
			CommandInfo.EditPaste,
			CommandInfo.EditDelete
		};
		if (TimelineTrackCommands.Commands.Any())
		{
			list.AddRange(TimelineTrackCommands.Commands);
		}
		if (setRulerTime)
		{
			int num = m_timeline.TimeRuler.PointToClient(location).X;
			ITimelineTrackCommands timelineTrackCommands = TimelineTrackCommands;
			float num2 = (m_timeline.TimeRuler.CurrentTime = m_timeline.TimeRuler.XToTime(num));
			float targetTime = num2;
			timelineTrackCommands.TargetTime = targetTime;
		}
		IEnumerable<object> commandTags = from cmd in list
			orderby cmd.GroupTag.ToString()
			select cmd.CommandTag;
		CommandService.RunContextMenu(commandTags, location);
	}

	private bool SelectedTreeNodeOwnsSelectedKeys()
	{
		DomNode adaptable = (from dnti in m_timeline.ScrollableTree.SelectedNodes
			where dnti.Item.As<IDomNodeTreeItem>()?.DomNode != null
			select dnti.Item.As<IDomNodeTreeItem>().DomNode).FirstOrDefault();
		IEnumerable<DomNode> source = from tkey in m_timeline.TimeTrack.SelectedKeys
			where tkey is ITriggerAdapterKey
			select (tkey as ITriggerAdapterKey).Adapter.DomNode;
		TrackAdapter track = adaptable.As<TrackAdapter>();
		if (track != null)
		{
			return source.All((DomNode dn) => track.Triggers.Contains(dn.As<TriggerAdapter>()));
		}
		return false;
	}

	private void UpdateSelectionContext()
	{
		bool flag = m_timeline.ScrollableTree.SelectedNodes.Any();
		bool flag2 = m_timeline.TimeTrack.SelectedKeys.Any();
		ISelectionContext selectionContext = m_context.As<ISelectionContext>();
		if (flag && flag2)
		{
			if (SelectedTreeNodeOwnsSelectedKeys())
			{
				IEnumerable<DomNode> items = from dnti in m_timeline.ScrollableTree.SelectedNodes
					where dnti.Item.As<IDomNodeTreeItem>()?.DomNode != null
					select dnti.Item.As<IDomNodeTreeItem>().DomNode;
				selectionContext?.SetRange(items);
				IEnumerable<DomNode> enumerable = from tkey in m_timeline.TimeTrack.SelectedKeys
					where tkey is ITriggerAdapterKey
					select (tkey as ITriggerAdapterKey).Adapter.DomNode;
				BugSubmitter.SilentAssert(enumerable.Any(), "Why? @assign bwhitman");
				selectionContext?.SetRange(enumerable);
			}
			else
			{
				IEnumerable<DomNode> items2 = from dnti in m_timeline.ScrollableTree.SelectedNodes
					where dnti.Item.As<IDomNodeTreeItem>()?.DomNode != null
					select dnti.Item.As<IDomNodeTreeItem>().DomNode;
				selectionContext?.SetRange(items2);
			}
		}
		else if (flag && !flag2)
		{
			IEnumerable<DomNode> items3 = from dnti in m_timeline.ScrollableTree.SelectedNodes
				where dnti.Item.As<IDomNodeTreeItem>()?.DomNode != null
				select dnti.Item.As<IDomNodeTreeItem>().DomNode;
			selectionContext?.SetRange(items3);
		}
		else
		{
			if (flag2)
			{
				m_timeline.TimeTrack.SelectedKeys.Clear();
			}
			selectionContext?.Clear();
		}
	}

	private void TimeTrack_MovingKeyBegin(object sender, EventArgs e)
	{
		ITransactionContext transactionContext = m_context.As<ITransactionContext>();
		try
		{
			transactionContext.Begin("Drag Start Time".Localize());
		}
		catch (InvalidTransactionException ex)
		{
			transactionContext?.Cancel();
			if (ex.ReportError)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			}
		}
	}

	private void TimeTrack_MovingKeyEnd(object sender, EventArgs e)
	{
		ITransactionContext transactionContext = m_context.As<ITransactionContext>();
		try
		{
			transactionContext.End();
		}
		catch (InvalidTransactionException ex)
		{
			transactionContext?.Cancel();
			if (ex.ReportError)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			}
		}
	}

	private void BindAnimation(TimelineAdapter timeline, AnimationBindingAdapter binding)
	{
		ScrollableItemAnimation item = new ScrollableItemAnimation(m_context, AnimationKnobService, timeline, binding, Font, ResourceUtil.GetImage16(Resources.AnimationFileIcon));
		ScrollableTree.TreeNode treeNode = m_timeline.ScrollableTree.Add(item);
		BindTimelineTracks(timeline, treeNode);
		ApplyActiveSkinToNode(treeNode);
	}

	private void BindTimeline(TimelineAdapter timeline, TimelineBindingAdapter binding)
	{
		ScrollableItemTimeline item = new ScrollableItemTimeline(m_context, timeline, binding, Font, Firaxis.Asset.Properties.Resources.trigger);
		ScrollableTree.TreeNode treeNode = m_timeline.ScrollableTree.Add(item);
		BindTimelineTracks(timeline, treeNode);
		ApplyActiveSkinToNode(treeNode);
	}

	private void SortTimelines(ScrollableTree.TreeNodeCollection timelineRoots)
	{
		timelineRoots.Sort(delegate(ScrollableTree.TreeNode a, ScrollableTree.TreeNode b)
		{
			ITimelineBoundItem timelineBoundItem = a.Item as ITimelineBoundItem;
			ITimelineBoundItem timelineBoundItem2 = b.Item as ITimelineBoundItem;
			return (!(timelineBoundItem.Timeline.Name == timelineBoundItem2.Timeline.Name)) ? timelineBoundItem.Timeline.Name.CompareTo(timelineBoundItem2.Timeline.Name) : timelineBoundItem.Timeline.AnimationName.CompareTo(timelineBoundItem2.Timeline.AnimationName);
		});
	}

	private void SortTimelineChidren(ScrollableTree.TreeNode timelineNode)
	{
		timelineNode.Children.Sort(delegate(ScrollableTree.TreeNode a, ScrollableTree.TreeNode b)
		{
			ITrackAdapterTreeItem trackAdapterTreeItem = a.Item as ITrackAdapterTreeItem;
			ITrackAdapterTreeItem trackAdapterTreeItem2 = b.Item as ITrackAdapterTreeItem;
			if (trackAdapterTreeItem.Adapter.TriggerType == trackAdapterTreeItem2.Adapter.TriggerType)
			{
				return trackAdapterTreeItem.Adapter.Name.CompareTo(trackAdapterTreeItem2.Adapter.Name);
			}
			return (trackAdapterTreeItem.Adapter.TriggerType >= trackAdapterTreeItem2.Adapter.TriggerType) ? 1 : (-1);
		});
	}

	private void BindTimelineTracks(TimelineAdapter timeline, ScrollableTree.TreeNode parentNode)
	{
		foreach (TrackAdapter track in timeline.Tracks)
		{
			AddTrackToTreeImpl(parentNode, timeline, track);
		}
		SortTimelineChidren(parentNode);
	}

	private void UnbindTimelines()
	{
		if (m_context != null)
		{
			m_context = null;
		}
		m_timeline.ScrollableTree.Clear();
	}
}
