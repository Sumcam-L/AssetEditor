using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class CommandControl : UserControl
{
	protected static char[] s_pathDelimiters = new char[2] { '/', '\\' };

	private ICommandContext m_boundContext;

	private IDictionary<CommandInfo, ICommandClient> m_infoMap = new Dictionary<CommandInfo, ICommandClient>();

	private IDictionary<ICommandClient, IEnumerable<CommandInfo>> m_commandMap = new Dictionary<ICommandClient, IEnumerable<CommandInfo>>();

	private IDictionary<CommandInfo, CommandService.CommandControls> m_commandControls = new Dictionary<CommandInfo, CommandService.CommandControls>();

	private bool m_skinApplied;

	private IContainer components;

	private ToolStrip tbCommands;

	private Panel pnlChildren;

	public ControlCollection ChildControls => pnlChildren.Controls;

	public bool ShowCommandText { get; set; }

	public CommandControl()
	{
		InitializeComponent();
		ShowCommandText = false;
		Application.Idle += Application_Idle;
	}

	public CommandControl(IEnumerable<CommandInfo> commands, ICommandClient commandClient)
		: this()
	{
		RegisterCommandClient(commandClient, commands);
	}

	public void GetMenuItem(CommandInfo commandInfo, out ToolStripMenuItem menuItem)
	{
		ToolStripButton button = null;
		GetMenuItemAndButton(commandInfo, out menuItem, out button);
	}

	public void GetButton(CommandInfo commandInfo, out ToolStripButton button)
	{
		ToolStripMenuItem menuItem = null;
		GetMenuItemAndButton(commandInfo, out menuItem, out button);
	}

	public void GetMenuItemAndButton(CommandInfo commandInfo, out ToolStripMenuItem menuItem, out ToolStripButton button)
	{
		if (m_commandControls.ContainsKey(commandInfo))
		{
			CommandService.CommandControls commandControls = m_commandControls[commandInfo];
			menuItem = commandControls.MenuItem;
			button = commandControls.Button;
		}
		else
		{
			menuItem = null;
			button = null;
		}
	}

	public void RegisterCommandClient(ICommandClient client, IEnumerable<CommandInfo> commands)
	{
		BugSubmitter.Assert(!m_commandMap.ContainsKey(client), "Duplicate registration of command client \"{0}\"", client.GetType());
		m_commandMap[client] = commands;
		commands.ForEach(delegate(CommandInfo cmdTag)
		{
			BugSubmitter.SilentAssert(!m_infoMap.ContainsKey(cmdTag), "Command \"{0}\" is serviced by more then one command client @summary Duplication registration because command is serviced by more then one command client!", cmdTag);
			m_infoMap[cmdTag] = client;
		});
		RefreshCommandToolbar();
	}

	public void UnregisterCommandClient(ICommandClient client)
	{
		BugSubmitter.Assert(m_commandMap.ContainsKey(client), "Duplicate unregistration of command client \"{0}\"", client.GetType());
		m_commandMap[client].ForEach(delegate(CommandInfo cmdTag)
		{
			BugSubmitter.SilentAssert(m_infoMap.ContainsKey(cmdTag), "Command \"{0}\" is serviced by more then one command client @summary Duplication unregistration because command is serviced by more then one command client!", cmdTag);
			m_infoMap.Remove(cmdTag);
		});
		m_commandMap.Remove(client);
		RefreshCommandToolbar();
	}

	public virtual void Bind(ICommandContext context)
	{
		if (m_boundContext != null)
		{
			UnregisterCommandClient(m_boundContext.CommandClient);
		}
		m_boundContext = context;
		if (m_boundContext != null)
		{
			RegisterCommandClient(m_boundContext.CommandClient, m_boundContext.Commands);
			SkinService.ApplyActiveSkin(tbCommands);
			m_skinApplied = true;
		}
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (Visible && !m_skinApplied)
		{
			SkinService.ApplyActiveSkin(this);
			m_skinApplied = true;
		}
	}

	protected virtual void OnAddControlButtons(ToolStrip controlToolbar)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	private void UpdateCommand(ICommandClient client, CommandInfo info)
	{
		if (base.InvokeRequired)
		{
			BeginInvoke(new Action<ICommandClient, CommandInfo>(UpdateCommand), client, info);
		}
		else if ((info.Visibility & CommandVisibility.ContextToolbar) != CommandVisibility.None)
		{
			CommandService.CommandControls commandControls = m_commandControls[info];
			ToolStripMenuItem menuItem = commandControls.MenuItem;
			ToolStripButton button = commandControls.Button;
			CommandState commandState = new CommandState
			{
				Text = info.DisplayedMenuText,
				Check = menuItem.Checked
			};
			bool flag = false;
			flag = client.CanDoCommand(info.CommandTag);
			if (flag)
			{
				client.UpdateCommand(info.CommandTag, commandState);
			}
			string text = commandState.Text.Trim();
			string text2 = (button.Text = text);
			string text4 = text2;
			menuItem.Text = text4;
			bool flag2 = (button.Checked = commandState.Check);
			bool flag3 = flag2;
			menuItem.Checked = flag3;
			flag2 = (button.Enabled = flag);
			flag3 = flag2;
			menuItem.Enabled = flag3;
		}
	}

	private void AddCommandButtonsToToolbar()
	{
		string[] array = new string[2];
		foreach (KeyValuePair<ICommandClient, IEnumerable<CommandInfo>> item in m_commandMap)
		{
			foreach (CommandInfo item2 in item.Value)
			{
				if ((item2.Visibility & CommandVisibility.ContextToolbar) == 0)
				{
					continue;
				}
				ToolStripItemAlignment toolStripItemAlignment = ToolStripItemAlignment.Left;
				if ((item2.Visibility & CommandVisibility.RightAlignment) != CommandVisibility.None)
				{
					toolStripItemAlignment = ToolStripItemAlignment.Right;
				}
				Image image = ResourceUtil.GetImage16(item2.ImageName);
				string menuText = item2.MenuText;
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
				item2.DisplayedMenuText = displayedMenuText;
				CommandService.CommandControls commandControls = new CommandService.CommandControls(new ToolStripMenuItem(item2.DisplayedMenuText, image), new ToolStripButton(item2.MenuText, image));
				commandControls.Button.DisplayStyle = (ShowCommandText ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image);
				commandControls.Button.Tag = item2;
				commandControls.Button.Alignment = toolStripItemAlignment;
				string groupString = GetGroupString(item2);
				string text = array[(int)toolStripItemAlignment];
				ToolStripSeparator toolStripSeparator = null;
				if (text != null && text != groupString)
				{
					toolStripSeparator = new ToolStripSeparator();
					toolStripSeparator.Alignment = toolStripItemAlignment;
				}
				if (toolStripItemAlignment == ToolStripItemAlignment.Left)
				{
					if (toolStripSeparator != null)
					{
						tbCommands.Items.Add(toolStripSeparator);
					}
					tbCommands.Items.Add(commandControls.Button);
				}
				else
				{
					tbCommands.Items.Add(commandControls.Button);
					if (toolStripSeparator != null)
					{
						tbCommands.Items.Add(toolStripSeparator);
					}
				}
				m_commandControls[item2] = commandControls;
				commandControls.Button.Click += CommandButton_Click;
				array[(int)toolStripItemAlignment] = groupString;
			}
		}
	}

	private long m_lastIdleMs;

	private void Application_Idle(object sender, EventArgs e)
	{
		long nowMs = System.Diagnostics.Stopwatch.GetTimestamp() * 1000 / System.Diagnostics.Stopwatch.Frequency;
		if (nowMs - m_lastIdleMs < 100)
			return;
		m_lastIdleMs = nowMs;

		if (m_commandMap.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<ICommandClient, IEnumerable<CommandInfo>> item in m_commandMap)
		{
			ICommandClient key = item.Key;
			foreach (CommandInfo item2 in item.Value)
			{
				UpdateCommand(key, item2);
			}
		}
	}

	private void CommandButton_Click(object sender, EventArgs e)
	{
		CommandInfo commandInfo = (sender as ToolStripButton).Tag as CommandInfo;
		BugSubmitter.Assert(m_infoMap.ContainsKey(commandInfo), "Command clicked that had no command client!");
		m_infoMap[commandInfo].DoCommand(commandInfo.CommandTag);
	}

	private string GetGroupString(CommandInfo ci)
	{
		if (ci.GroupTag == null)
		{
			return string.Empty;
		}
		return ci.GroupTag.ToString();
	}

	private bool HasCommands()
	{
		return m_infoMap.Count > 0;
	}

	private void RefreshCommandToolbar()
	{
		tbCommands.Items.Clear();
		m_commandControls.Clear();
		OnAddControlButtons(tbCommands);
		if (HasCommands())
		{
			AddCommandButtonsToToolbar();
		}
		UpdateToolbarVisibility();
	}

	private void UpdateToolbarVisibility()
	{
		if (tbCommands.Items.Count > 0)
		{
			if (!base.Controls.Contains(tbCommands))
			{
				base.Controls.Add(tbCommands);
			}
		}
		else if (base.Controls.Contains(tbCommands))
		{
			base.Controls.Remove(tbCommands);
		}
	}

	private void InitializeComponent()
	{
		this.tbCommands = new System.Windows.Forms.ToolStrip();
		this.pnlChildren = new System.Windows.Forms.Panel();
		base.SuspendLayout();
		this.tbCommands.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.tbCommands.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
		this.tbCommands.Location = new System.Drawing.Point(0, 0);
		this.tbCommands.Name = "tbCommands";
		this.tbCommands.Size = new System.Drawing.Size(342, 25);
		this.tbCommands.Stretch = true;
		this.tbCommands.TabIndex = 0;
		this.pnlChildren.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlChildren.Location = new System.Drawing.Point(0, 25);
		this.pnlChildren.Name = "pnlChildren";
		this.pnlChildren.Size = new System.Drawing.Size(342, 329);
		this.pnlChildren.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.pnlChildren);
		base.Controls.Add(this.tbCommands);
		base.Name = "CommandControl";
		base.Size = new System.Drawing.Size(342, 354);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
