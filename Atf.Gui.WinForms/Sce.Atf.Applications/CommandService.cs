using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using Sce.Atf.Applications.Controls;
using Sce.Atf.Controls;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

[Export(typeof(ICommandService))]
[Export(typeof(IInitializable))]
[Export(typeof(CommandService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CommandService : CommandServiceBase
{
	public class MenuControls
	{
		public ToolStripMenuItem MenuItem { get; private set; }

		public ToolStrip ToolStrip { get; private set; }

		public MenuControls(ToolStripMenuItem menuItem, ToolStrip toolStrip)
		{
			MenuItem = menuItem;
			ToolStrip = toolStrip;
		}
	}

	public class CommandControls : IDisposable
	{
		public ToolStripMenuItem MenuItem { get; private set; }

		public ToolStripButton Button { get; private set; }

		public CommandControls(ToolStripMenuItem menuItem, ToolStripButton button)
		{
			MenuItem = menuItem;
			Button = button;
		}

		public void Dispose()
		{
			if (MenuItem != null)
			{
				MenuItem.Dispose();
			}
			if (Button != null)
			{
				Button.Dispose();
			}
		}
	}

	private readonly Dictionary<MenuInfo, ToolStrip> m_menuToolStrips = new Dictionary<MenuInfo, ToolStrip>();

	private readonly Dictionary<MenuInfo, ToolStripMenuItem> m_menuToolStripItems = new Dictionary<MenuInfo, ToolStripMenuItem>();

	private readonly Dictionary<CommandInfo, CommandControls> m_commandControls = new Dictionary<CommandInfo, CommandControls>();

	private readonly Multimap<CommandInfo, ICommandClient> m_checkCanDoClients = new Multimap<CommandInfo, ICommandClient>();

	private readonly HashSet<CommandInfo> m_checkCanDoClientsToUpdate = new HashSet<CommandInfo>();

	private readonly Form m_mainForm;

	private ToolStripContainer m_toolStripContainer;

	private readonly MenuStrip m_mainMenuStrip;

	private bool m_commandsSorted;

	private Point m_menuMouseLocation = Point.Empty;

	private CommandInfo m_mouseIsOverCommandIcon;

	private ToolStripEx m_lastHoveringToolStrip;

	private DispatcherTimer m_webHelpTimer;

	public static bool ContextMenuIsTriggering { get; set; }

	public override CommandInfo MouseIsOverCommandIcon => m_mouseIsOverCommandIcon;

	[ImportingConstructor]
	public CommandService(Form mainForm)
	{
		m_mainForm = mainForm;
		m_mainForm.Load += mainForm_Load;
		m_mainMenuStrip = new MenuStrip();
		m_mainMenuStrip.Name = "Main Menu";
		m_mainMenuStrip.Dock = DockStyle.Top;
	}

	public override void Initialize()
	{
		base.Initialize();
		Application.Idle += Application_Idle;
	}

	public override void RegisterCommand(CommandInfo info, ICommandClient client)
	{
		base.RegisterCommand(info, client);
		if (info != null && client != null && info.CheckCanDoClients.Contains(client))
		{
			m_checkCanDoClients.Add(info, client);
			m_checkCanDoClientsToUpdate.Add(info);
		}
		m_commandsSorted = false;
	}

	public override void UnregisterCommand(object commandTag, ICommandClient client)
	{
		CommandInfo commandInfo = GetCommandInfo(commandTag);
		if (commandInfo != null && client != null && commandInfo.CheckCanDoClients.Contains(client))
		{
			m_checkCanDoClients.Remove(commandInfo, client);
			m_checkCanDoClientsToUpdate.Remove(commandInfo);
		}
		base.UnregisterCommand(commandTag, client);
		RemoveToolStripItem(commandTag);
	}

	protected override void DecrementMenuCommandCount(MenuInfo menuInfo)
	{
		base.DecrementMenuCommandCount(menuInfo);
		if (menuInfo.Commands == 0)
		{
			m_menuToolStripItems[menuInfo].Visible = false;
		}
	}

	public ContextMenuStrip CreateContextMenu(IEnumerable<object> commandTags)
	{
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		int count;
		foreach (object commandTag in commandTags)
		{
			if (commandTag == null)
			{
				count = contextMenuStrip.Items.Count;
				if (count > 0 && !(contextMenuStrip.Items[count - 1] is ToolStripSeparator))
				{
					contextMenuStrip.Items.Add(new ToolStripSeparator());
				}
				continue;
			}
			CommandInfo commandInfo = GetCommandInfo(commandTag);
			if (commandInfo != null && (commandInfo.Visibility & CommandVisibility.ContextMenu) != CommandVisibility.None)
			{
				UpdateCommand(commandInfo);
				ToolStripMenuItem menuItem = commandInfo.GetMenuItem();
				if (menuItem.Enabled || !base.ContextMenuAutoCompact)
				{
					ToolStripItemCollection toolStripItemCollection = BuildSubMenus(contextMenuStrip.Items, commandInfo);
					ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = menuItem.Text;
					toolStripMenuItem.Image = menuItem.Image;
					toolStripMenuItem.Name = menuItem.Name;
					toolStripMenuItem.Enabled = menuItem.Enabled;
					toolStripMenuItem.Checked = menuItem.Checked;
					toolStripMenuItem.Tag = menuItem.Tag;
					toolStripMenuItem.ToolTipText = menuItem.ToolTipText;
					toolStripMenuItem.ShortcutKeys = menuItem.ShortcutKeys;
					toolStripMenuItem.ShortcutKeyDisplayString = menuItem.ShortcutKeyDisplayString;
					toolStripMenuItem.Click += contextMenu_itemClick;
					toolStripMenuItem.ForeColor = m_mainMenuStrip.ForeColor;
					toolStripMenuItem.CheckOnClick = commandInfo.CheckOnClick;
					toolStripItemCollection.Add(toolStripMenuItem);
					MaintainSeparateGroups(toolStripItemCollection, toolStripMenuItem, commandInfo.GroupTag);
				}
			}
		}
		count = contextMenuStrip.Items.Count;
		if (count > 0 && contextMenuStrip.Items[count - 1] is ToolStripSeparator)
		{
			contextMenuStrip.Items.RemoveAt(count - 1);
		}
		SkinService.ApplyActiveSkin(contextMenuStrip);
		return contextMenuStrip;
	}

	public override void RunContextMenu(IEnumerable<object> commandTags, Point screenPoint)
	{
		ContextMenuStrip contextMenuStrip = CreateContextMenu(commandTags);
		contextMenuStrip.Show(screenPoint);
		if (contextMenuStrip.Visible)
		{
			contextMenuStrip.Closed += contextMenu_Closed;
		}
		else
		{
			contextMenu_Closed(contextMenuStrip, new ToolStripDropDownClosedEventArgs(ToolStripDropDownCloseReason.CloseCalled));
		}
	}

	public override bool CanDoCommand(object commandTag)
	{
		return CommandId.EditKeyboard.Equals(commandTag);
	}

	public override void DoCommand(object commandTag)
	{
		if (!CommandId.EditKeyboard.Equals(commandTag))
		{
			return;
		}
		List<CustomizeKeyboardDialog.Shortcut> list = new List<CustomizeKeyboardDialog.Shortcut>();
		foreach (KeyValuePair<object, CommandInfo> item2 in m_commandsById)
		{
			if (GetClient(item2.Value.CommandTag) != null && item2.Value.ShortcutsEditable)
			{
				string commandPath = GetCommandPath(item2.Value);
				CustomizeKeyboardDialog.Shortcut item = new CustomizeKeyboardDialog.Shortcut(item2.Value, commandPath);
				list.Add(item);
			}
		}
		CustomizeKeyboardDialog customizeKeyboardDialog = new CustomizeKeyboardDialog(list, m_reservedKeys);
		customizeKeyboardDialog.ShowDialog(m_mainForm);
		if (customizeKeyboardDialog.DialogResult != DialogResult.OK || !customizeKeyboardDialog.Modified)
		{
			return;
		}
		m_shortcuts.Clear();
		foreach (CustomizeKeyboardDialog.Shortcut item3 in list)
		{
			item3.Info.ClearShortcuts();
			foreach (Sce.Atf.Input.Keys key in item3.Keys)
			{
				SetShortcut(key, item3.Info);
			}
		}
	}

	public Image GetProperlySizedImage(string imageName)
	{
		Image result = null;
		if (!string.IsNullOrEmpty(imageName))
		{
			if (m_imageSize == ImageSizes.Size16x16)
			{
				result = ResourceUtil.GetImage16(imageName);
			}
			else if (m_imageSize == ImageSizes.Size24x24)
			{
				result = ResourceUtil.GetImage24(imageName);
			}
			else if (m_imageSize == ImageSizes.Size32x32)
			{
				result = ResourceUtil.GetImage32(imageName);
			}
		}
		return result;
	}

	protected override void OnImageSizeChanged()
	{
		RefreshImages();
	}

	public override void RefreshImages()
	{
		foreach (CommandInfo command in m_commands)
		{
			RefreshImage(command);
		}
	}

	public override void RefreshImage(CommandInfo info)
	{
		Image properlySizedImage = GetProperlySizedImage(info.ImageName);
		if (properlySizedImage != null)
		{
			ToolStripButton button = info.GetButton();
			button.AutoSize = true;
			button.ImageScaling = ToolStripItemImageScaling.None;
			button.Image = properlySizedImage;
			ToolStripMenuItem menuItem = info.GetMenuItem();
			menuItem.Image = properlySizedImage;
		}
	}

	private string GetGroupTagString(CommandInfo cmdInf)
	{
		if (cmdInf == null)
		{
			return "";
		}
		if (cmdInf.GroupTag == null)
		{
			return "";
		}
		return cmdInf.GroupTag.ToString();
	}

	public void BuildDefaultMenusAndToolbars()
	{
		if (m_toolStripContainer == null)
		{
			foreach (Control control in m_mainForm.Controls)
			{
				m_toolStripContainer = control as ToolStripContainer;
				if (m_toolStripContainer != null)
				{
					break;
				}
			}
		}
		m_mainMenuStrip.SuspendLayout();
		m_mainMenuStrip.Items.Clear();
		foreach (MenuInfo menu in m_menus)
		{
			if (menu.Commands > 0)
			{
				ToolStripMenuItem toolStripMenuItem = m_menuToolStripItems[menu];
				toolStripMenuItem.DropDownItems.Add("Dummy");
				toolStripMenuItem.DropDownOpening += menuItem_DropDownOpening;
				toolStripMenuItem.DropDownClosed += menuItem_DropDownClosed;
				m_mainMenuStrip.Items.Add(toolStripMenuItem);
			}
		}
		m_mainMenuStrip.ResumeLayout();
		m_mainMenuStrip.PerformLayout();
		List<ToolStrip> list = new List<ToolStrip>();
		for (int num = m_menus.Count - 1; num >= 0; num--)
		{
			MenuInfo menuInfo = m_menus[num];
			if (menuInfo.Commands > 0)
			{
				ToolStrip toolStrip = menuInfo.GetToolStrip();
				list.Add(toolStrip);
				IEnumerable<CommandInfo> enumerable = m_commands.OrderBy((CommandInfo comInf) => (comInf.GroupTag != null) ? comInf.GroupTag.ToString() : "");
				string text = null;
				foreach (CommandInfo item in enumerable)
				{
					if (CommandServiceBase.TagsEqual(menuInfo.MenuTag, item.MenuTag) && (item.Visibility & CommandVisibility.ApplicationToolbar) != CommandVisibility.None && GetClient(item.CommandTag) != null)
					{
						ToolStripButton button = item.GetButton();
						if (item.CheckOnClick)
						{
							button.CheckOnClick = true;
							item.GetMenuItem().CheckOnClick = true;
							button.CheckedChanged += SynchronizeCheckedState;
							item.GetMenuItem().CheckedChanged += SynchronizeCheckedState;
						}
						if (text != null && text != GetGroupTagString(item))
						{
							ToolStripSeparator toolStripSeparator = new ToolStripSeparator();
							toolStripSeparator.Name = text;
							toolStrip.Items.Add(toolStripSeparator);
						}
						text = GetGroupTagString(item);
						toolStrip.Items.Add(button);
					}
				}
				toolStrip.Dock = DockStyle.None;
				AddCustomizationDropDown(toolStrip);
				toolStrip.Visible = toolStrip.Items.Count > 1;
			}
		}
		if (m_toolStripContainer != null)
		{
			m_toolStripContainer.TopToolStripPanel.Controls.AddRange(list.ToArray());
			m_toolStripContainer.TopToolStripPanel.Controls.Add(m_mainMenuStrip);
		}
		else
		{
			m_mainForm.Controls.Add(m_mainMenuStrip);
		}
	}

	public void ResetToolStrips()
	{
		if (m_toolStripContainer == null)
		{
			foreach (Control control in m_mainForm.Controls)
			{
				m_toolStripContainer = control as ToolStripContainer;
				if (m_toolStripContainer != null)
				{
					break;
				}
			}
		}
		List<ToolStrip> list = new List<ToolStrip>();
		for (int num = m_menus.Count - 1; num >= 0; num--)
		{
			MenuInfo menuInfo = m_menus[num];
			if (menuInfo.Commands > 0)
			{
				ToolStrip toolStrip = menuInfo.GetToolStrip();
				toolStrip.Items.Clear();
				list.Add(toolStrip);
				IEnumerable<CommandInfo> enumerable = m_commands.OrderBy((CommandInfo comInf) => (comInf.GroupTag != null) ? comInf.GroupTag.ToString() : "");
				string text = null;
				foreach (CommandInfo item in enumerable)
				{
					if (CommandServiceBase.TagsEqual(menuInfo.MenuTag, item.MenuTag) && (item.Visibility & CommandVisibility.ApplicationToolbar) != CommandVisibility.None && GetClient(item.CommandTag) != null)
					{
						ToolStripButton button = item.GetButton();
						if (item.CheckOnClick)
						{
							button.CheckOnClick = true;
							item.GetMenuItem().CheckOnClick = true;
							button.CheckedChanged += SynchronizeCheckedState;
							item.GetMenuItem().CheckedChanged += SynchronizeCheckedState;
						}
						if (text != null && text != GetGroupTagString(item))
						{
							ToolStripSeparator toolStripSeparator = new ToolStripSeparator();
							toolStripSeparator.Name = text;
							toolStrip.Items.Add(toolStripSeparator);
						}
						text = GetGroupTagString(item);
						toolStrip.Items.Add(button);
						button.Visible = true;
					}
				}
				toolStrip.Dock = DockStyle.None;
				AddCustomizationDropDown(toolStrip);
				toolStrip.Visible = toolStrip.Items.Count > 1;
			}
		}
		if (m_toolStripContainer == null)
		{
			return;
		}
		m_toolStripContainer.SuspendLayout();
		m_toolStripContainer.TopToolStripPanel.Controls.Clear();
		m_toolStripContainer.LeftToolStripPanel.Controls.Clear();
		m_toolStripContainer.RightToolStripPanel.Controls.Clear();
		m_toolStripContainer.BottomToolStripPanel.Controls.Clear();
		foreach (ToolStrip item2 in list)
		{
			item2.Location = new Point(0, 0);
		}
		m_toolStripContainer.TopToolStripPanel.Controls.AddRange(list.ToArray());
		m_toolStripContainer.TopToolStripPanel.Controls.Add(m_mainMenuStrip);
		m_mainMenuStrip.Location = new Point(0, 0);
		m_toolStripContainer.ResumeLayout();
		m_toolStripContainer.PerformLayout();
	}

	protected override void UpdateCommand(object commandTag)
	{
		CommandInfo commandInfo = GetCommandInfo(commandTag);
		if (commandInfo != null)
		{
			UpdateCommand(commandInfo);
		}
	}

	public void UpdateCommand(CommandInfo info)
	{
		ICommandClient clientOrActiveClient = GetClientOrActiveClient(info.CommandTag);
		UpdateCommand(info, clientOrActiveClient);
	}

	private void UpdateCommand(CommandInfo info, ICommandClient client)
	{
		if (m_mainForm.InvokeRequired)
		{
			m_mainForm.BeginInvoke(new Action<CommandInfo>(UpdateCommand), info);
			return;
		}
		info.GetMenuItemAndButton(out var menuItem, out var button);
		CommandState commandState = new CommandState();
		commandState.Text = info.DisplayedMenuText;
		commandState.Check = menuItem.Checked;
		bool flag = false;
		if (client != null)
		{
			flag = client.CanDoCommand(info.CommandTag);
			if (flag)
			{
				client.UpdateCommand(info.CommandTag, commandState);
			}
		}
		string text = commandState.Text.Trim();
		ToolStripMenuItem toolStripMenuItem = menuItem;
		string text2 = (button.Text = text);
		toolStripMenuItem.Text = text2;
		ToolStripMenuItem toolStripMenuItem2 = menuItem;
		bool flag2 = (button.Checked = commandState.Check);
		toolStripMenuItem2.Checked = flag2;
		ToolStripMenuItem toolStripMenuItem3 = menuItem;
		flag2 = (button.Enabled = flag);
		toolStripMenuItem3.Enabled = flag2;
	}

	private void SynchronizeCheckedState(object sender, EventArgs e)
	{
		if (sender is ToolStripButton)
		{
			KeyValuePair<CommandInfo, CommandControls> keyValuePair = m_commandControls.FirstOrDefault((KeyValuePair<CommandInfo, CommandControls> x) => x.Value.Button == sender);
			if (keyValuePair.Value != null)
			{
				keyValuePair.Value.MenuItem.Checked = keyValuePair.Value.Button.Checked;
			}
		}
		else if (sender is ToolStripMenuItem)
		{
			KeyValuePair<CommandInfo, CommandControls> keyValuePair2 = m_commandControls.FirstOrDefault((KeyValuePair<CommandInfo, CommandControls> x) => x.Value.MenuItem == sender);
			if (keyValuePair2.Value != null)
			{
				keyValuePair2.Value.Button.Checked = keyValuePair2.Value.MenuItem.Checked;
			}
		}
	}

	private void mainForm_Load(object sender, EventArgs e)
	{
		BuildDefaultMenusAndToolbars();
	}

	private void menuItem_DropDownOpening(object sender, EventArgs e)
	{
		if (!m_commandsSorted)
		{
			m_commands.Sort(new CommandComparer());
			m_commandsSorted = true;
		}
		ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
		toolStripMenuItem.DropDownItems.Clear();
		MenuInfo menuInfo = GetMenuInfo(toolStripMenuItem.Tag);
		foreach (CommandInfo command in m_commands)
		{
			if (CommandServiceBase.TagsEqual(command.MenuTag, menuInfo.MenuTag) && (command.Visibility & CommandVisibility.ApplicationMenu) != CommandVisibility.None && GetClient(command.CommandTag) != null)
			{
				AddMenuCommand(toolStripMenuItem.DropDownItems, command);
				toolStripMenuItem.DropDown.BackColor = m_mainMenuStrip.BackColor;
			}
		}
	}

	private void menuItem_DropDownClosed(object sender, EventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
		toolStripMenuItem.DropDownItems.Clear();
		toolStripMenuItem.DropDownItems.Add("Dummy");
	}

	private void item_Click(object sender, EventArgs e)
	{
		base.IconClicked = IsMouseOverIcon(sender as ToolStripItem);
		if (m_statusService != null)
		{
			m_statusService.ShowStatus(string.Empty);
		}
		ToolStripItem toolStripItem = sender as ToolStripItem;
		object tag = toolStripItem.Tag;
		if (tag != null)
		{
			ICommandClient clientOrActiveClient = GetClientOrActiveClient(tag);
			if (clientOrActiveClient != null && clientOrActiveClient.CanDoCommand(tag))
			{
				clientOrActiveClient.DoCommand(tag);
			}
		}
		base.IconClicked = false;
	}

	private void contextMenu_itemClick(object sender, EventArgs e)
	{
		ContextMenuIsTriggering = true;
		item_Click(sender, e);
		ContextMenuIsTriggering = false;
	}

	private void menuItem_MouseEnter(object sender, EventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
		object tag = toolStripMenuItem.Tag;
		if (tag != null)
		{
			CommandInfo commandInfo = m_commandsById[tag];
			if (m_statusService != null)
			{
				m_statusService.ShowStatus(commandInfo.Description);
			}
		}
	}

	private void menuItem_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		m_menuMouseLocation = e.Location;
		if (sender is ToolStripMenuItem { Tag: { } tag } toolStripMenuItem && tag is IPinnable)
		{
			if (IsMouseOverIcon(toolStripMenuItem))
			{
				m_mouseIsOverCommandIcon = m_commandsById[tag];
			}
			else
			{
				m_mouseIsOverCommandIcon = null;
			}
			CommandState state = new CommandState(toolStripMenuItem.Text, toolStripMenuItem.Checked);
			UpdatePinnableCommand(tag, state);
		}
	}

	private void menuItem_MouseLeave(object sender, EventArgs e)
	{
		if (m_statusService != null)
		{
			m_statusService.ShowStatus(string.Empty);
		}
		m_menuMouseLocation = Point.Empty;
		m_mouseIsOverCommandIcon = null;
		if (sender is ToolStripMenuItem { Tag: { } tag } toolStripMenuItem && tag is IPinnable)
		{
			CommandState state = new CommandState(toolStripMenuItem.Text, toolStripMenuItem.Checked);
			UpdatePinnableCommand(tag, state);
		}
	}

	private void UpdatePinnableCommand(object commandTag, CommandState state)
	{
		ICommandClient clientOrActiveClient = GetClientOrActiveClient(commandTag);
		if (clientOrActiveClient != null && clientOrActiveClient.CanDoCommand(commandTag))
		{
			clientOrActiveClient.UpdateCommand(commandTag, state);
		}
	}

	private bool IsMouseOverIcon(ToolStripItem menuItem)
	{
		if (menuItem != null && m_menuMouseLocation != Point.Empty && menuItem.Image != null)
		{
			Rectangle contentRectangle = menuItem.ContentRectangle;
			Rectangle rectangle = new Rectangle(new Point(contentRectangle.Left, contentRectangle.Top), SystemInformation.MenuButtonSize);
			if (m_menuMouseLocation.X > rectangle.Left && m_menuMouseLocation.X <= rectangle.Right)
			{
				return true;
			}
		}
		return false;
	}

	private long m_lastIdleMs;
	private const int IdleThrottleMs = 100;

	private void Application_Idle(object sender, EventArgs e)
	{
		long nowMs = System.Diagnostics.Stopwatch.GetTimestamp() * 1000 / System.Diagnostics.Stopwatch.Frequency;
		if (nowMs - m_lastIdleMs < IdleThrottleMs)
			return;
		m_lastIdleMs = nowMs;

		for (int i = 0; i < m_commands.Count; i++)
		{
			CommandInfo commandInfo = m_commands[i];
			ICommandClient clientOrActiveClient = GetClientOrActiveClient(commandInfo.CommandTag);
			if (clientOrActiveClient == null || !m_checkCanDoClients.ContainsKeyValue(commandInfo, clientOrActiveClient))
			{
				UpdateCommand(commandInfo, clientOrActiveClient);
			}
		}
		if (m_checkCanDoClientsToUpdate.Count <= 0)
		{
			return;
		}
		CommandInfo[] array = m_checkCanDoClientsToUpdate.ToArray();
		CommandInfo[] array2 = array;
		foreach (CommandInfo commandInfo2 in array2)
		{
			if (commandInfo2.CommandService != null)
			{
				UpdateCommand(commandInfo2);
			}
		}
		m_checkCanDoClientsToUpdate.Clear();
	}

	private void OnCheckCanDo(object sender, EventArgs e)
	{
		m_checkCanDoClientsToUpdate.Add((CommandInfo)sender);
	}

	private void contextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
	{
		OnContextMenuClosed(sender, e);
	}

	private void AddCustomizationDropDown(ToolStrip toolStrip)
	{
		string text = "Customize".Localize();
		foreach (ToolStripItem item in toolStrip.Items)
		{
			if (item.Text == text)
			{
				return;
			}
		}
		ToolStripDropDownButton toolStripDropDownButton = new ToolStripDropDownButton(text);
		toolStripDropDownButton.Name = toolStrip.Name;
		toolStripDropDownButton.Overflow = ToolStripItemOverflow.Always;
		toolStripDropDownButton.DropDownOpening += button_DropDownOpening;
		toolStrip.Items.Add(toolStripDropDownButton);
	}

	private void button_DropDownOpening(object sender, EventArgs e)
	{
		ToolStripDropDownButton toolStripDropDownButton = sender as ToolStripDropDownButton;
		toolStripDropDownButton.DropDownItems.Clear();
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		contextMenuStrip.ItemClicked += contextMenu_DropDownItemClicked;
		contextMenuStrip.Closing += contextMenu_Closing;
		ToolStrip owner = toolStripDropDownButton.Owner;
		foreach (ToolStripItem item in owner.Items)
		{
			if (!(item is ToolStripSeparator) && item != toolStripDropDownButton)
			{
				ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(item.Text);
				toolStripMenuItem.Checked = item.Visible;
				toolStripMenuItem.Tag = item;
				contextMenuStrip.Items.Add(toolStripMenuItem);
			}
		}
		toolStripDropDownButton.DropDown = contextMenuStrip;
	}

	private void contextMenu_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = e.ClickedItem as ToolStripMenuItem;
		ToolStripItem toolStripItem = e.ClickedItem.Tag as ToolStripItem;
		toolStripItem.Visible = !toolStripItem.Visible;
		toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
	}

	private void contextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
	{
		if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
		{
			e.Cancel = true;
			return;
		}
		ContextMenuStrip contextMenuStrip = sender as ContextMenuStrip;
		ToolStripDropDownButton toolStripDropDownButton = contextMenuStrip.OwnerItem as ToolStripDropDownButton;
		toolStripDropDownButton.DropDown = null;
	}

	private void RemoveToolStripItem(object commandTag)
	{
		if (m_toolStripContainer != null)
		{
			RemoveToolStripItem(commandTag, m_toolStripContainer.LeftToolStripPanel);
			RemoveToolStripItem(commandTag, m_toolStripContainer.TopToolStripPanel);
			RemoveToolStripItem(commandTag, m_toolStripContainer.RightToolStripPanel);
			RemoveToolStripItem(commandTag, m_toolStripContainer.BottomToolStripPanel);
		}
	}

	private void RemoveToolStripItem(object commandTag, ToolStripPanel panel)
	{
		foreach (ToolStrip control in panel.Controls)
		{
			foreach (ToolStripItem item in control.Items)
			{
				if (item.Tag == commandTag)
				{
					control.Items.Remove(item);
					return;
				}
			}
		}
	}

	private void AddMenuCommand(ToolStripItemCollection commands, CommandInfo info)
	{
		commands = BuildSubMenus(commands, info);
		ToolStripMenuItem menuItem = info.GetMenuItem();
		menuItem.BackColor = m_mainMenuStrip.BackColor;
		menuItem.ForeColor = m_mainMenuStrip.ForeColor;
		commands.Add(menuItem);
		MaintainSeparateGroups(commands, menuItem, info.GroupTag);
	}

	private ToolStripItemCollection BuildSubMenus(ToolStripItemCollection commands, CommandInfo info)
	{
		string menuText = info.MenuText;
		string[] array = ((menuText[0] != '@') ? menuText.Split(CommandServiceBase.s_pathDelimiters, 8) : new string[1] { menuText.Substring(1, menuText.Length - 1) });
		for (int i = 0; i < array.Length - 1; i++)
		{
			string text = array[i];
			ToolStripMenuItem toolStripMenuItem = null;
			for (int j = 0; j < commands.Count; j++)
			{
				if (text == commands[j].Text)
				{
					toolStripMenuItem = commands[j] as ToolStripMenuItem;
					if (toolStripMenuItem != null)
					{
						break;
					}
				}
			}
			if (toolStripMenuItem == null)
			{
				toolStripMenuItem = new ToolStripMenuItem(text);
				toolStripMenuItem.Name = text;
				commands.Add(toolStripMenuItem);
				MaintainSeparateGroups(commands, toolStripMenuItem, info.GroupTag);
			}
			toolStripMenuItem.BackColor = m_mainMenuStrip.BackColor;
			toolStripMenuItem.ForeColor = m_mainMenuStrip.ForeColor;
			commands = toolStripMenuItem.DropDownItems;
		}
		return commands;
	}

	private void MaintainSeparateGroups(ToolStripItemCollection commands, ToolStripItem item, object groupTag)
	{
		int num = commands.IndexOf(item);
		if (num > 0)
		{
			ToolStripItem toolStripItem = commands[num - 1];
			object tag = toolStripItem.Tag;
			while (tag == null && toolStripItem is ToolStripMenuItem { DropDownItems: var dropDownItems })
			{
				toolStripItem = dropDownItems[dropDownItems.Count - 1];
				tag = toolStripItem.Tag;
			}
			CommandInfo commandInfo = GetCommandInfo(tag);
			if (commandInfo != null && !CommandServiceBase.TagsEqual(groupTag, commandInfo.GroupTag))
			{
				commands.Insert(num, new ToolStripSeparator());
			}
		}
	}

	protected sealed override void RegisterMenuInfo(MenuInfo info)
	{
		base.RegisterMenuInfo(info);
		if (!m_menuToolStrips.TryGetValue(info, out var value))
		{
			value = new ToolStripEx();
			value.MouseHover += ToolStripOnMouseHover;
			m_menuToolStrips.Add(info, value);
		}
		string text = info.MenuText.Replace("&", "");
		text = text.Replace(";", "");
		value.Name = text + "_toolbar";
		value.AllowItemReorder = true;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(info.MenuText);
		toolStripMenuItem.Visible = false;
		toolStripMenuItem.Name = info.MenuText + "_menu";
		toolStripMenuItem.Tag = info.MenuTag;
		toolStripMenuItem.Alignment = ((info.Alignment != MenuInfo.MenuAlignment.Left) ? ToolStripItemAlignment.Right : ToolStripItemAlignment.Left);
		m_menuToolStripItems.Add(info, toolStripMenuItem);
		info.CommandService = this;
	}

	public override bool ProcessKey(Sce.Atf.Input.Keys key)
	{
		if (key == Sce.Atf.Input.Keys.F1 && m_lastHoveringToolStrip != null)
		{
			foreach (object item in m_lastHoveringToolStrip.Items)
			{
				if (!(item is ToolStripButton { Selected: not false } toolStripButton))
				{
					continue;
				}
				foreach (KeyValuePair<CommandInfo, CommandControls> commandControl in m_commandControls)
				{
					if (commandControl.Value.Button != toolStripButton || string.IsNullOrEmpty(commandControl.Key.HelpUrl))
					{
						continue;
					}
					WebHelp.SupressHelpRequests = true;
					Process.Start(commandControl.Key.HelpUrl);
					if (m_webHelpTimer == null)
					{
						m_webHelpTimer = new DispatcherTimer
						{
							Interval = TimeSpan.FromMilliseconds(1000.0)
						};
						m_webHelpTimer.Tick += delegate
						{
							WebHelp.SupressHelpRequests = false;
							m_webHelpTimer.Stop();
						};
					}
					m_webHelpTimer.Start();
					return true;
				}
			}
		}
		return base.ProcessKey(key);
	}

	private void ToolStripOnMouseHover(object sender, EventArgs eventArgs)
	{
		m_lastHoveringToolStrip = (ToolStripEx)sender;
	}

	private static void commandInfo_ShortcutsChanged(object sender, EventArgs e)
	{
		CommandInfo commandInfo = (CommandInfo)sender;
		if (commandInfo == null)
		{
			throw new InvalidOperationException("commandInfo_ShortcutsChanged() - sender was not a CommandInfo");
		}
		commandInfo.RebuildShortcutDisplayString();
	}

	private static void commandInfo_VisibilityChanged(object sender, EventArgs e)
	{
		CommandInfo commandInfo = (CommandInfo)sender;
		ToolStripButton button = commandInfo.GetButton();
		if (button != null)
		{
			button.Visible = (commandInfo.Visibility & CommandVisibility.ApplicationToolbar) != 0;
		}
	}

	protected override void RegisterCommandInfo(CommandInfo info)
	{
		m_commandsSorted = false;
		Image properlySizedImage = GetProperlySizedImage(info.ImageName);
		string commandPath = GetCommandPath(info);
		CommandControls commandControls = new CommandControls(new ToolStripMenuItem(info.DisplayedMenuText, properlySizedImage), new ToolStripButton(info.MenuText, properlySizedImage));
		m_commandControls.Add(info, commandControls);
		info.CommandService = this;
		info.ShortcutsChanged += commandInfo_ShortcutsChanged;
		info.VisibilityChanged += commandInfo_VisibilityChanged;
		base.RegisterCommandInfo(info);
		ToolStripMenuItem menuItem = commandControls.MenuItem;
		menuItem.Name = commandPath;
		menuItem.Tag = info.CommandTag;
		menuItem.MouseEnter += menuItem_MouseEnter;
		menuItem.MouseLeave += menuItem_MouseLeave;
		menuItem.MouseMove += menuItem_MouseMove;
		menuItem.Click += item_Click;
		ToolStripButton button = commandControls.Button;
		button.AutoSize = true;
		button.ImageScaling = ToolStripItemImageScaling.None;
		button.Name = commandPath;
		button.DisplayStyle = ((properlySizedImage == null) ? ToolStripItemDisplayStyle.Text : ToolStripItemDisplayStyle.Image);
		button.Tag = info.CommandTag;
		string text = info.Description;
		if (!string.IsNullOrEmpty(info.HelpUrl))
		{
			text = text + Environment.NewLine + "Press F1 for more info".Localize();
		}
		button.ToolTipText = text;
		button.Click += item_Click;
		info.CheckCanDo += OnCheckCanDo;
	}

	protected override void UnregisterCommandInfo(CommandInfo info)
	{
		info.CheckCanDo -= OnCheckCanDo;
		if (m_commandControls.TryGetValue(info, out var value))
		{
			value.Dispose();
			m_commandControls.Remove(info);
		}
		info.ShortcutsChanged -= commandInfo_ShortcutsChanged;
		info.VisibilityChanged -= commandInfo_VisibilityChanged;
		info.CommandService = null;
		base.UnregisterCommandInfo(info);
	}

	protected override MenuInfo IncrementMenuCommandCount(object menuTag)
	{
		MenuInfo menuInfo = base.IncrementMenuCommandCount(menuTag);
		if (menuInfo != null && menuInfo.Commands == 1)
		{
			m_menuToolStripItems[menuInfo].Visible = true;
		}
		return menuInfo;
	}

	public ToolStripMenuItem GetMenuToolStripItem(MenuInfo info)
	{
		if (info == null)
		{
			throw new NullReferenceException("MenuInfo argument cannot be null");
		}
		return m_menuToolStripItems[info];
	}

	public ToolStrip GetMenuToolStrip(MenuInfo info)
	{
		if (info == null)
		{
			throw new NullReferenceException("MenuInfo argument cannot be null");
		}
		return m_menuToolStrips[info];
	}

	public void SetMenuToolStrip(MenuInfo info, ToolStrip toolStrip)
	{
		if (info == null)
		{
			throw new NullReferenceException("MenuInfo argument cannot be null");
		}
		if (toolStrip == null)
		{
			throw new NullReferenceException("ToolStrip argument cannot be null");
		}
		m_menuToolStrips.Add(info, toolStrip);
	}

	public CommandControls GetCommandControls(CommandInfo info)
	{
		if (info == null)
		{
			throw new NullReferenceException("CommandInfo argument cannot be null");
		}
		return m_commandControls[info];
	}
}
