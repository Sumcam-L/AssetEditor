using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.Controls;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

[Export(typeof(IInitializable))]
[Export(typeof(TimelineTreeCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TimelineTreeCommands : IInitializable, ICommandClient
{
	private enum TreeCommands
	{
		ExpandAll,
		ExpandAllWithTriggers,
		CollapseAll,
		CollapseAllButSelected
	}

	public static CommandInfo ExpandAll = new CommandInfo(TreeCommands.ExpandAll, StandardMenu.View, TreeCommandGroups.TreeCommands, "Timeline\\Expand All".Localize(), "Expand all timelines in the timeline editor window".Localize("Expand all timelines"), Sce.Atf.Input.Keys.None, Resources.ExpandAllTimelineIcon);

	public static CommandInfo ExpandAllWithTriggers = new CommandInfo(TreeCommands.ExpandAllWithTriggers, StandardMenu.View, TreeCommandGroups.TreeCommands, "Timeline\\Expand All with Triggers".Localize(), "Expand all timelines that have triggers in the timeline editor window".Localize("Expand all timelines with triggers"), Sce.Atf.Input.Keys.None, Resources.ExpandAllWithTriggersTimelineIcon);

	public static CommandInfo CollapseAll = new CommandInfo(TreeCommands.CollapseAll, StandardMenu.View, TreeCommandGroups.TreeCommands, "Timeline\\Collapse All".Localize(), "Collapse all timelines in the timeline editor window".Localize("Collapse all timelines"), Sce.Atf.Input.Keys.None, Resources.CollapseAllTimelineIcon);

	public static CommandInfo CollapseAllButSelected = new CommandInfo(TreeCommands.CollapseAllButSelected, StandardMenu.View, TreeCommandGroups.TreeCommands, "Timeline\\Collapse All But Selected".Localize(), "Collapse all timelines in the timeline editor window except the current selection".Localize("Collapse all timelines except selected"), Sce.Atf.Input.Keys.None, Resources.CollapseAllButSelectedTimelineIcon);

	private static readonly char[] s_pathDelimiters = new char[2] { '/', '\\' };

	private ICommandService CommandService { get; set; }

	private TimeLineControl Control { get; set; }

	private ISelectionContext SelectionContext { get; set; }

	public bool ShowCommandText { get; set; }

	public IEnumerable<CommandInfo> Commands { get; } = new CommandInfo[4] { ExpandAll, ExpandAllWithTriggers, CollapseAll, CollapseAllButSelected };

	private IEnumerable<ScrollableTree.TreeNode> SelectedRootNodes => Control.ScrollableTree.SelectedNodes.Select((ScrollableTree.TreeNode selObj) => selObj.Parent ?? selObj);

	private IEnumerable<ScrollableTree.TreeNode> NonSelectedRootNodes => Control.ScrollableTree.Root.Except(SelectedRootNodes);

	private IEnumerable<ScrollableTree.TreeNode> RootNodesWithTriggers => Control.ScrollableTree.Root.Where((ScrollableTree.TreeNode wObj) => wObj.Item.As<ITimelineBoundItem>()?.Timeline.Triggers.Any() ?? false);

	[ImportingConstructor]
	public TimelineTreeCommands(ICommandService cmdSvc, TimeLineControl timelineControl)
	{
		CommandService = cmdSvc;
		Control = timelineControl;
		Application.Idle += Application_Idle;
		UpdateBuiltInToolStrip(timelineControl);
		AddCommandsToToolStrip(timelineControl.ToolStrip);
	}

	public void Initialize()
	{
		RegisterCommands();
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (commandTag is TreeCommands)
		{
			switch ((TreeCommands)commandTag)
			{
			case TreeCommands.ExpandAll:
				return AreAnyItemsCollapsed();
			case TreeCommands.ExpandAllWithTriggers:
				return AreAnyItemsWithTriggersCollapsed();
			case TreeCommands.CollapseAll:
				return AreAnyItemsExpanded();
			case TreeCommands.CollapseAllButSelected:
				if (Control.ScrollableTree.SelectedNodes.Count > 0)
				{
					return AreAnyNonSelectedItemsExpanded();
				}
				return false;
			}
			BugSubmitter.Assert(condition: false, "Unknown TimelineTreeCommand enum! Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum");
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (!(commandTag is TreeCommands))
		{
			return;
		}
		switch ((TreeCommands)commandTag)
		{
		case TreeCommands.ExpandAll:
			ExpandNodes(Control.ScrollableTree.Root);
			EnsureSelectionVisible();
			break;
		case TreeCommands.ExpandAllWithTriggers:
			ExpandNodes(RootNodesWithTriggers);
			EnsureSelectionVisible();
			break;
		case TreeCommands.CollapseAll:
			SelectTimelineIfTrackSelected();
			CollapseNodes(Control.ScrollableTree.Root);
			EnsureSelectionVisible();
			break;
		case TreeCommands.CollapseAllButSelected:
			CollapseNodes(NonSelectedRootNodes.Where((ScrollableTree.TreeNode whereNode) => Control.ScrollableTree.GetNodeStyle(whereNode) == ScrollableItemStyle.Expanded));
			EnsureSelectionVisible();
			break;
		default:
			BugSubmitter.Assert(condition: false, "Unknown TimelineTreeCommand enum! Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum");
			break;
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void UpdateBuiltInToolStrip(TimeLineControl timeLineControl)
	{
		ToolStripTrackBar obj = timeLineControl.ToolStrip.Items[0] as ToolStripTrackBar;
		obj.Padding = new Padding(2, 0, 2, 0);
		obj.AutoSize = false;
		obj.Size = new Size(80, 22);
		obj.TrackBar.TickStyle = TickStyle.None;
	}

	private long m_lastIdleMs;

	private void Application_Idle(object sender, EventArgs e)
	{
		long nowMs = System.Diagnostics.Stopwatch.GetTimestamp() * 1000 / System.Diagnostics.Stopwatch.Frequency;
		if (nowMs - m_lastIdleMs < 100)
			return;
		m_lastIdleMs = nowMs;

		UpdateAllCommands();
	}

	private void UpdateAllCommands()
	{
		if (Control.InvokeRequired)
		{
			Control.BeginInvoke(new Action(UpdateAllCommands));
			return;
		}
		foreach (ToolStripItem item in Control.ToolStrip.Items)
		{
			if (item.Tag is CommandInfo commandInfo && item is ToolStripButton toolStripButton)
			{
				CommandState commandState = new CommandState();
				commandState.Text = commandInfo.DisplayedMenuText;
				commandState.Check = toolStripButton.Checked;
				bool flag = CanDoCommand(commandInfo.CommandTag);
				if (flag)
				{
					UpdateCommand(commandInfo.CommandTag, commandState);
				}
				string text = commandState.Text.Trim();
				toolStripButton.Text = text;
				toolStripButton.Checked = commandState.Check;
				toolStripButton.Enabled = flag;
			}
		}
	}

	private void AddCommandsToToolStrip(ToolStrip toolStrip)
	{
		string text = null;
		if (Commands.Any())
		{
			toolStrip.Items.Add(new ToolStripSeparator());
		}
		foreach (CommandInfo command in Commands)
		{
			Image image = ResourceUtil.GetImage16(command.ImageName);
			string menuText = command.MenuText;
			if (string.IsNullOrEmpty(menuText))
			{
				throw new ArgumentException("menuText is null or empty");
			}
			int num = 1;
			if (menuText[0] != '@')
			{
				num += menuText.LastIndexOfAny(s_pathDelimiters);
			}
			string displayedMenuText = menuText.Substring(num, menuText.Length - num);
			command.DisplayedMenuText = displayedMenuText;
			ToolStripButton toolStripButton = new ToolStripButton(command.MenuText, image);
			toolStripButton.DisplayStyle = (ShowCommandText ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image);
			toolStripButton.Tag = command;
			string groupString = GetGroupString(command);
			if (text != null && text != groupString)
			{
				toolStrip.Items.Add(new ToolStripSeparator());
			}
			toolStrip.Items.Add(toolStripButton);
			toolStripButton.Click += ToolBarButton_Click;
			text = groupString;
		}
	}

	private void ToolBarButton_Click(object sender, EventArgs e)
	{
		CommandInfo commandInfo = (sender as ToolStripButton).Tag as CommandInfo;
		DoCommand(commandInfo.CommandTag);
	}

	private string GetGroupString(CommandInfo ci)
	{
		if (ci.GroupTag == null)
		{
			return string.Empty;
		}
		return ci.GroupTag.ToString();
	}

	private void CollapseNodes(IEnumerable<ScrollableTree.TreeNode> nodes)
	{
		if (nodes.Any())
		{
			Control.ScrollableTree.BeginUpdate();
			nodes.ForEach(delegate(ScrollableTree.TreeNode node)
			{
				Control.ScrollableTree.CollapseNode(node, bRecursive: true);
			});
			Control.ScrollableTree.EndUpdate();
		}
	}

	private void ExpandNodes(IEnumerable<ScrollableTree.TreeNode> nodes)
	{
		if (nodes.Any())
		{
			Control.ScrollableTree.BeginUpdate();
			nodes.ForEach(delegate(ScrollableTree.TreeNode node)
			{
				Control.ScrollableTree.ExpandNode(node, bRecursive: true);
			});
			Control.ScrollableTree.EndUpdate();
		}
	}

	private void SelectTimelineIfTrackSelected()
	{
		if (Control.ScrollableTree.SelectedNodes.Count != 0)
		{
			ScrollableTree.TreeNode treeNode = Control.ScrollableTree.SelectedNodes[0];
			while (treeNode != null && !Control.ScrollableTree.Root.Contains(treeNode))
			{
				treeNode = treeNode.Parent;
			}
			Control.ScrollableTree.SelectedNodes.Clear();
			Control.ScrollableTree.SelectedNodes.Add(treeNode);
		}
	}

	private void EnsureSelectionVisible()
	{
		if (Control.ScrollableTree.SelectedNodes.Count != 0)
		{
			ScrollableTree.TreeNode node = Control.ScrollableTree.SelectedNodes[0];
			bool num = Control.ScrollableTree.GetNodeStyle(node) == ScrollableItemStyle.Expanded;
			Control.ScrollableTree.EnsureVisible(node);
			if (!num)
			{
				Control.ScrollableTree.CollapseNode(node, bRecursive: true);
			}
		}
	}

	private bool AreAnyNonSelectedItemsExpanded()
	{
		return NonSelectedRootNodes.Any((ScrollableTree.TreeNode node) => Control.ScrollableTree.GetNodeStyle(node) == ScrollableItemStyle.Expanded);
	}

	private bool AreAnyItemsCollapsed()
	{
		return Control.ScrollableTree.Root.Any((ScrollableTree.TreeNode node) => Control.ScrollableTree.GetNodeStyle(node) == ScrollableItemStyle.Collapsed);
	}

	private bool AreAnyItemsWithTriggersCollapsed()
	{
		return RootNodesWithTriggers.Any((ScrollableTree.TreeNode node) => Control.ScrollableTree.GetNodeStyle(node) == ScrollableItemStyle.Collapsed);
	}

	private bool AreAnyItemsExpanded()
	{
		return Control.ScrollableTree.Root.Any((ScrollableTree.TreeNode node) => Control.ScrollableTree.GetNodeStyle(node) == ScrollableItemStyle.Expanded);
	}

	private void RegisterCommands()
	{
		CommandService.RegisterCommand(ExpandAll, this);
		CommandService.RegisterCommand(ExpandAllWithTriggers, this);
		CommandService.RegisterCommand(CollapseAll, this);
		CommandService.RegisterCommand(CollapseAllButSelected, this);
	}

	private void UnregisterCommands()
	{
		CommandService.UnregisterCommand(CollapseAllButSelected, this);
		CommandService.UnregisterCommand(CollapseAll, this);
		CommandService.UnregisterCommand(ExpandAllWithTriggers, this);
		CommandService.UnregisterCommand(ExpandAll, this);
	}
}
