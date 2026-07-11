using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ModelInstanceStateEditor : UserControl
{
	private IDictionary<CommandInfo, CommandService.CommandControls> m_commandControls = new Dictionary<CommandInfo, CommandService.CommandControls>();

	private IEnumerable<CommandInfo> m_commands = new List<CommandInfo>();

	private IModelInstanceStateContext m_context;

	private ISelectionContext m_selectionContext;

	private TreeListViewAdapter m_modelListEditor;

	private GridControl m_stateEditor;

	protected static char[] s_pathDelimiters = new char[2] { '/', '\\' };

	private IContainer components;

	private ToolStrip tbCommands;

	private SplitContainer splitterMain;

	public bool ShowCommandText { get; set; }

	protected ICommandClient CommandClient { get; set; }

	protected IEnumerable<CommandInfo> Commands
	{
		get
		{
			return m_commands;
		}
		set
		{
			if (m_commands != value)
			{
				m_commands = value;
				RefreshCommandToolbar();
			}
		}
	}

	public ModelInstanceStateEditor()
	{
		InitializeComponent();
		SuspendLayout();
		ShowCommandText = false;
		m_modelListEditor = new TreeListViewAdapter(new TreeListView(TreeListView.Style.List));
		m_modelListEditor.TreeListView.Control.Dock = DockStyle.Fill;
		m_modelListEditor.TreeListView.HeaderStyle = ColumnHeaderStyle.None;
		m_modelListEditor.TreeListView.Control.SizeChanged += Control_SizeChanged;
		m_modelListEditor.TreeListView.Control.VisibleChanged += Control_VisibleChanged;
		splitterMain.Panel1.Controls.Add(m_modelListEditor.TreeListView);
		m_stateEditor = new GridControl(PropertyGridMode.DisableSearchControls | PropertyGridMode.DisableDragDropColumnHeaders, PropertyCategorySettings.HideText);
		m_stateEditor.PropertySorting = PropertySorting.Categorized;
		m_stateEditor.Dock = DockStyle.Fill;
		m_stateEditor.GridView.AutoScaleColumns = true;
		splitterMain.Panel2.Controls.Add(m_stateEditor);
		ResumeLayout();
		base.SizeChanged += ModelInstanceStateEditor_SizeChanged;
		Application.Idle += Application_Idle;
	}

	public void Bind(IModelInstanceStateContext context)
	{
		m_context = context;
		CommandClient = m_context;
		Commands = m_context.Commands;
		m_stateEditor.Bind(m_context);
		m_stateEditor.GridView.ApplyColumnBestFitAllColumns();
		m_stateEditor.GridView.SortByProperty(context.DefaultSortPropertyName, context.DefaultListSortDirection);
		m_modelListEditor.View = m_context;
		m_selectionContext = m_context.As<ISelectionContext>();
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanged += Context_SelectionChanged;
		}
	}

	public void UpdateCommand(CommandInfo info)
	{
		if (base.InvokeRequired)
		{
			BeginInvoke(new Action<CommandInfo>(UpdateCommand), info);
			return;
		}
		CommandService.CommandControls commandControls = m_commandControls[info];
		ToolStripMenuItem menuItem = commandControls.MenuItem;
		ToolStripButton button = commandControls.Button;
		CommandState commandState = new CommandState
		{
			Text = info.DisplayedMenuText,
			Check = menuItem.Checked
		};
		bool flag = false;
		if (CommandClient != null)
		{
			flag = CommandClient.CanDoCommand(info.CommandTag);
			if (flag)
			{
				CommandClient.UpdateCommand(info.CommandTag, commandState);
			}
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

	private void AddCommandButtonsToToolbar()
	{
		string text = null;
		foreach (CommandInfo command in m_commands)
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
			CommandService.CommandControls commandControls = new CommandService.CommandControls(new ToolStripMenuItem(command.DisplayedMenuText, image), new ToolStripButton(command.MenuText, image));
			commandControls.Button.DisplayStyle = (ShowCommandText ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image);
			commandControls.Button.Tag = command;
			string groupString = GetGroupString(command);
			if (text != null && text != groupString)
			{
				tbCommands.Items.Add(new ToolStripSeparator());
			}
			m_commandControls[command] = commandControls;
			tbCommands.Items.Add(commandControls.Button);
			commandControls.Button.Click += CommandButton_Click;
			text = groupString;
		}
	}

	private void AdjustChildSizing()
	{
		int num = base.ClientRectangle.Width;
		int num2 = base.ClientRectangle.Height;
		splitterMain.Left = 0;
		splitterMain.Width = num;
		if (HasCommands())
		{
			splitterMain.Top = tbCommands.Bottom + 1;
			splitterMain.Height = num2 - tbCommands.Height - 1;
		}
		else
		{
			splitterMain.Top = 0;
			splitterMain.Height = num2;
		}
	}

	private long m_lastIdleMs;

	private void Application_Idle(object sender, EventArgs e)
	{
		long nowMs = System.Diagnostics.Stopwatch.GetTimestamp() * 1000 / System.Diagnostics.Stopwatch.Frequency;
		if (nowMs - m_lastIdleMs < 100)
			return;
		m_lastIdleMs = nowMs;

		if (m_commands != null)
		{
			m_commands.ForEach(delegate(CommandInfo ci)
			{
				UpdateCommand(ci);
			});
		}
	}

	private void CommandButton_Click(object sender, EventArgs e)
	{
		if (CommandClient != null)
		{
			CommandInfo commandInfo = (sender as ToolStripButton).Tag as CommandInfo;
			CommandClient.DoCommand(commandInfo.CommandTag);
		}
	}

	private void Context_SelectionChanged(object sender, EventArgs e)
	{
		m_stateEditor.Bind(m_context);
		m_stateEditor.GridView.ApplyColumnBestFitAllColumns();
	}

	private void Control_SizeChanged(object sender, EventArgs e)
	{
		UpdateModelListColumnWidth();
	}

	private void Control_VisibleChanged(object sender, EventArgs e)
	{
		UpdateModelListColumnWidth();
	}

	private void UpdateModelListColumnWidth()
	{
		if (m_modelListEditor.TreeListView.Columns.Count != 0)
		{
			m_modelListEditor.TreeListView.Columns.First().Width = m_modelListEditor.TreeListView.Control.ClientRectangle.Width;
		}
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
		if (m_commands != null)
		{
			return m_commands.Count() > 0;
		}
		return false;
	}

	private void ModelInstanceStateEditor_SizeChanged(object sender, EventArgs e)
	{
		AdjustChildSizing();
	}

	private void RefreshCommandToolbar()
	{
		tbCommands.Items.Clear();
		m_commandControls.Clear();
		if (m_commands == null || m_commands.Count() == 0)
		{
			tbCommands.Visible = false;
		}
		else
		{
			tbCommands.Visible = true;
			AddCommandButtonsToToolbar();
		}
		AdjustChildSizing();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.tbCommands = new System.Windows.Forms.ToolStrip();
		this.splitterMain = new System.Windows.Forms.SplitContainer();
		((System.ComponentModel.ISupportInitialize)this.splitterMain).BeginInit();
		this.splitterMain.SuspendLayout();
		base.SuspendLayout();
		this.tbCommands.Location = new System.Drawing.Point(0, 0);
		this.tbCommands.Name = "tbCommands";
		this.tbCommands.Size = new System.Drawing.Size(150, 25);
		this.tbCommands.TabIndex = 0;
		this.splitterMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitterMain.Location = new System.Drawing.Point(0, 25);
		this.splitterMain.Name = "splitterMain";
		this.splitterMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitterMain.Size = new System.Drawing.Size(150, 125);
		this.splitterMain.SplitterDistance = 34;
		this.splitterMain.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splitterMain);
		base.Controls.Add(this.tbCommands);
		base.Name = "ModelInstanceStateEditor";
		((System.ComponentModel.ISupportInitialize)this.splitterMain).EndInit();
		this.splitterMain.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
