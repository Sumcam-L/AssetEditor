using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class CommandContextMenu
{
	private static readonly char[] s_pathDelimiters = new char[2] { '/', '\\' };

	private string m_lastGroupName;

	private ContextMenuStrip m_contextMenu;

	private IDictionary<CommandInfo, ICommandClient> m_commandClientMap = new Dictionary<CommandInfo, ICommandClient>();

	public event EventHandler MenuDismissed;

	public CommandContextMenu()
	{
		m_contextMenu = new ContextMenuStrip();
		m_contextMenu.Closed += ContextMenu_Closed;
	}

	public void RegisterCommand(CommandInfo cmdInfo, ICommandClient cmdClient)
	{
		BugSubmitter.Assert(!m_commandClientMap.ContainsKey(cmdInfo), "CommandContextMenu already has command register for info \"{0}\" @summary Duplicate command registration in CommandContextMenu @assign bwhitman", cmdInfo);
		m_commandClientMap[cmdInfo] = cmdClient;
		AddCommand(cmdInfo, cmdClient);
	}

	public void ShowMenu(Point screenPos)
	{
		UpdateAllCommands();
		m_contextMenu.Show(screenPos);
	}

	private void ContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
	{
		if (e.CloseReason != ToolStripDropDownCloseReason.ItemClicked)
		{
			this.MenuDismissed?.Invoke(this, EventArgs.Empty);
		}
	}

	private void AddCommand(CommandInfo cmdInfo, ICommandClient cmdClient)
	{
		Image image = ResourceUtil.GetImage16(cmdInfo.ImageName);
		string menuText = cmdInfo.MenuText;
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
		cmdInfo.DisplayedMenuText = displayedMenuText;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(cmdInfo.MenuText, image);
		toolStripMenuItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
		toolStripMenuItem.Tag = cmdInfo;
		string groupString = GetGroupString(cmdInfo);
		if (m_lastGroupName != null && m_lastGroupName != groupString)
		{
			m_contextMenu.Items.Add(new ToolStripSeparator());
		}
		m_contextMenu.Items.Add(toolStripMenuItem);
		toolStripMenuItem.Click += ToolBarMenuItem_Click;
		m_lastGroupName = groupString;
	}

	private void UpdateAllCommands()
	{
		if (m_contextMenu.InvokeRequired)
		{
			m_contextMenu.BeginInvoke(new Action(UpdateAllCommands));
			return;
		}
		foreach (ToolStripItem item in m_contextMenu.Items)
		{
			if (!(item.Tag is CommandInfo commandInfo) || !(item is ToolStripMenuItem toolStripMenuItem))
			{
				continue;
			}
			CommandState commandState = new CommandState();
			commandState.Text = commandInfo.DisplayedMenuText;
			commandState.Check = toolStripMenuItem.Checked;
			bool flag = false;
			if (m_commandClientMap.ContainsKey(commandInfo))
			{
				flag = m_commandClientMap[commandInfo].CanDoCommand(commandInfo.CommandTag);
				if (flag)
				{
					m_commandClientMap[commandInfo].UpdateCommand(commandInfo.CommandTag, commandState);
				}
			}
			string text = commandState.Text.Trim();
			toolStripMenuItem.Text = text;
			toolStripMenuItem.Checked = commandState.Check;
			toolStripMenuItem.Enabled = flag;
		}
	}

	private void ToolBarMenuItem_Click(object sender, EventArgs e)
	{
		CommandInfo commandInfo = (sender as ToolStripMenuItem).Tag as CommandInfo;
		if (m_commandClientMap.ContainsKey(commandInfo))
		{
			m_commandClientMap[commandInfo].DoCommand(commandInfo.CommandTag);
		}
		this.MenuDismissed?.Invoke(this, EventArgs.Empty);
	}

	private string GetGroupString(CommandInfo ci)
	{
		if (ci.GroupTag == null)
		{
			return string.Empty;
		}
		return ci.GroupTag.ToString();
	}
}
