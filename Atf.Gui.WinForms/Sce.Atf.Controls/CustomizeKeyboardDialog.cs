using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Controls;

public class CustomizeKeyboardDialog : Form
{
	public class Shortcut
	{
		private string m_commandPath;

		private List<Sce.Atf.Input.Keys> m_keys;

		public CommandInfo Info { get; private set; }

		public string CommandPath
		{
			get
			{
				return m_commandPath;
			}
			private set
			{
				m_commandPath = value;
				DisplayPath = value;
				DisplayPath = DisplayPath.Replace("&", "");
				DisplayPath = DisplayPath.Replace(".", "");
			}
		}

		public string DisplayPath { get; private set; }

		public IEnumerable<Sce.Atf.Input.Keys> Keys
		{
			get
			{
				return m_keys;
			}
			set
			{
				m_keys = new List<Sce.Atf.Input.Keys>(value);
			}
		}

		public bool KeysChanged => !Keys.SequenceEqual(Info.Shortcuts);

		public bool KeysAreDefault
		{
			get
			{
				if (IsEmptyOrNone(m_keys) && IsEmptyOrNone(Info.DefaultShortcuts))
				{
					return true;
				}
				return m_keys.SequenceEqual(Info.DefaultShortcuts);
			}
		}

		public Shortcut(CommandInfo info, string commandPath)
		{
			Info = info;
			CommandPath = commandPath;
			Keys = info.Shortcuts;
		}

		private static bool IsEmptyOrNone(IEnumerable<Sce.Atf.Input.Keys> shortcuts)
		{
			foreach (Sce.Atf.Input.Keys shortcut in shortcuts)
			{
				if (shortcut != Sce.Atf.Input.Keys.None)
				{
					return false;
				}
			}
			return true;
		}
	}

	private readonly Dictionary<string, Shortcut> m_displayNameToShortcut;

	private readonly IList<Shortcut> m_shortcuts;

	private Shortcut m_currentShortcut;

	private Sce.Atf.Input.Keys m_newKey;

	private readonly Dictionary<Sce.Atf.Input.Keys, string> m_reservedKeys;

	private IContainer components = null;

	private GroupBox grpCommands;

	private ListBox lstCommand;

	private Button btnRemoveShortcut;

	private Button btnAssignShortcut;

	private TextBox txtNewShortcut;

	private GroupBox grpCurKey;

	private GroupBox grpNewShortcut;

	private GroupBox grpShortUsed;

	private Button btnCancel;

	private ContextMenu cxMenu;

	private MenuItem mnuClear;

	private Button btnOK;

	private GroupBox grpDescription;

	private Label lblCmdDescription;

	private Label lblCurShortcut;

	private Label lblUsedBy;

	private Button btnSetToDefault;

	private Button btnAddShortcut;

	private Button btnAllDefault;

	public bool Modified
	{
		get
		{
			foreach (Shortcut shortcut in m_shortcuts)
			{
				if (shortcut.KeysChanged)
				{
					return true;
				}
			}
			return false;
		}
	}

	public CustomizeKeyboardDialog(IList<Shortcut> shortcuts, Dictionary<Sce.Atf.Input.Keys, string> reservedKeysAndExplanations)
	{
		m_shortcuts = shortcuts;
		m_reservedKeys = reservedKeysAndExplanations;
		InitializeComponent();
		lstCommand.Sorted = true;
		m_displayNameToShortcut = new Dictionary<string, Shortcut>(shortcuts.Count);
		foreach (Shortcut shortcut in shortcuts)
		{
			m_displayNameToShortcut[shortcut.DisplayPath] = shortcut;
			lstCommand.Items.Add(shortcut.DisplayPath);
		}
		btnAddShortcut.Enabled = false;
		btnAssignShortcut.Enabled = false;
		if (lstCommand.Items.Count > 0)
		{
			lstCommand.SelectedIndex = 0;
		}
		else
		{
			grpNewShortcut.Enabled = false;
		}
		txtNewShortcut.ContextMenu = cxMenu;
	}

	private void lstCommand_SelectedIndexChanged(object sender, EventArgs e)
	{
		ListBox listBox = (ListBox)sender;
		string key = (string)listBox.SelectedItem;
		m_currentShortcut = m_displayNameToShortcut[key];
		m_newKey = Sce.Atf.Input.Keys.None;
		UpdateControls();
	}

	private void btnSetToDefault_Click(object sender, EventArgs e)
	{
		if (m_currentShortcut == null)
		{
			return;
		}
		IEnumerable<Sce.Atf.Input.Keys> defaultShortcuts = m_currentShortcut.Info.DefaultShortcuts;
		foreach (Sce.Atf.Input.Keys item in defaultShortcuts)
		{
			RemoveShortcut(item);
		}
		m_currentShortcut.Keys = defaultShortcuts;
		m_newKey = Sce.Atf.Input.Keys.None;
		UpdateControls();
	}

	private void btnAllDefault_Click(object sender, EventArgs e)
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Shortcut shortcut in m_shortcuts)
		{
			if (!shortcut.KeysAreDefault)
			{
				stringBuilder.AppendLine(shortcut.CommandPath);
				num++;
			}
		}
		if (num > 0)
		{
			if (num < 10)
			{
				stringBuilder.Insert(0, "These commands currently do not use their default shortcuts:".Localize() + Environment.NewLine);
			}
			else
			{
				stringBuilder.Length = 0;
				stringBuilder.Append(string.Format("{0} commands currently do not use their default shortcuts.".Localize("{0} is a number"), num));
			}
			if (MessageBox.Show(this, stringBuilder.ToString(), "Reset all commands to the default shortcuts?".Localize(), MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				foreach (Shortcut shortcut2 in m_shortcuts)
				{
					shortcut2.Keys = shortcut2.Info.DefaultShortcuts;
				}
			}
		}
		m_newKey = Sce.Atf.Input.Keys.None;
		UpdateControls();
	}

	private void RemoveShortcut(Sce.Atf.Input.Keys key)
	{
		if (key == Sce.Atf.Input.Keys.None)
		{
			return;
		}
		foreach (Shortcut shortcut in m_shortcuts)
		{
			if (shortcut.Keys.Contains(key))
			{
				List<Sce.Atf.Input.Keys> list = new List<Sce.Atf.Input.Keys>(shortcut.Keys);
				list.Remove(key);
				shortcut.Keys = list;
			}
		}
	}

	private void btnAddShortcut_Click(object sender, EventArgs e)
	{
		SetShortcuts(addShortcut: true);
	}

	private void btnRemoveShortcut_Click(object sender, EventArgs e)
	{
		m_currentShortcut.Keys = new Sce.Atf.Input.Keys[1];
		UpdateControls();
	}

	private void btnAssignShortcut_Click(object sender, EventArgs e)
	{
		SetShortcuts(addShortcut: false);
	}

	private void SetShortcuts(bool addShortcut)
	{
		if (m_newKey == Sce.Atf.Input.Keys.None)
		{
			return;
		}
		if (m_reservedKeys.ContainsKey(m_newKey))
		{
			string text = KeysUtil.KeysToString(m_newKey, digitOnly: false);
			MessageBox.Show(this, text + " is reserved for " + m_reservedKeys[m_newKey], Text);
			return;
		}
		RemoveShortcut(m_newKey);
		List<Sce.Atf.Input.Keys> list = new List<Sce.Atf.Input.Keys>();
		if (addShortcut)
		{
			foreach (Sce.Atf.Input.Keys key in m_currentShortcut.Keys)
			{
				if (key != Sce.Atf.Input.Keys.None)
				{
					list.Add(key);
				}
			}
		}
		list.Add(m_newKey);
		m_currentShortcut.Keys = list;
		m_newKey = Sce.Atf.Input.Keys.None;
		UpdateControls();
	}

	private void txtNewShortcut_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{
		e.SuppressKeyPress = true;
		m_newKey = KeysUtil.KeyArgToKeys(e);
		m_newKey = KeysUtil.NumPadToNum(m_newKey);
		UpdateControls();
	}

	private void mnuClear_Click(object sender, EventArgs e)
	{
		m_newKey = Sce.Atf.Input.Keys.None;
		UpdateControls();
	}

	private void cxMenue_Popup(object sender, EventArgs e)
	{
		try
		{
			mnuClear.Enabled = txtNewShortcut.Text.Trim().Length > 0;
		}
		catch (Exception ex)
		{
			mnuClear.Enabled = false;
			Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
		}
	}

	private string GetUsedByText(Sce.Atf.Input.Keys key)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (key != Sce.Atf.Input.Keys.None)
		{
			foreach (Shortcut shortcut in m_shortcuts)
			{
				if (shortcut.Keys.Contains(key))
				{
					stringBuilder.AppendLine(shortcut.DisplayPath + "\r\n" + shortcut.Info.Description);
					break;
				}
			}
		}
		return stringBuilder.ToString();
	}

	private void UpdateControls()
	{
		string text = KeysUtil.KeysToString(m_currentShortcut.Keys, digitOnly: false);
		bool flag = !string.IsNullOrEmpty(text);
		btnRemoveShortcut.Enabled = flag;
		lblCurShortcut.Text = (flag ? text : null);
		btnSetToDefault.Enabled = !m_currentShortcut.KeysAreDefault;
		lblCmdDescription.Text = m_currentShortcut.Info.Description;
		if (m_newKey == Sce.Atf.Input.Keys.None)
		{
			txtNewShortcut.Text = string.Empty;
			btnAddShortcut.Enabled = false;
			btnAssignShortcut.Enabled = false;
			lblUsedBy.Text = null;
			grpShortUsed.Enabled = false;
		}
		else
		{
			txtNewShortcut.Text = KeysUtil.KeysToString(m_newKey, digitOnly: false);
			txtNewShortcut.SelectionStart = txtNewShortcut.Text.Length;
			btnAddShortcut.Enabled = true;
			btnAssignShortcut.Enabled = true;
			string usedByText = GetUsedByText(m_newKey);
			lblUsedBy.Text = usedByText;
			grpShortUsed.Enabled = usedByText.Length > 0;
		}
		bool flag2 = true;
		foreach (Shortcut shortcut in m_shortcuts)
		{
			if (!shortcut.KeysAreDefault)
			{
				flag2 = false;
				break;
			}
		}
		btnAllDefault.Enabled = !flag2;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.CustomizeKeyboardDialog));
		this.grpCommands = new System.Windows.Forms.GroupBox();
		this.lstCommand = new System.Windows.Forms.ListBox();
		this.btnRemoveShortcut = new System.Windows.Forms.Button();
		this.btnAssignShortcut = new System.Windows.Forms.Button();
		this.txtNewShortcut = new System.Windows.Forms.TextBox();
		this.grpCurKey = new System.Windows.Forms.GroupBox();
		this.btnSetToDefault = new System.Windows.Forms.Button();
		this.lblCurShortcut = new System.Windows.Forms.Label();
		this.grpNewShortcut = new System.Windows.Forms.GroupBox();
		this.btnAddShortcut = new System.Windows.Forms.Button();
		this.grpShortUsed = new System.Windows.Forms.GroupBox();
		this.lblUsedBy = new System.Windows.Forms.Label();
		this.btnCancel = new System.Windows.Forms.Button();
		this.cxMenu = new System.Windows.Forms.ContextMenu();
		this.mnuClear = new System.Windows.Forms.MenuItem();
		this.btnOK = new System.Windows.Forms.Button();
		this.grpDescription = new System.Windows.Forms.GroupBox();
		this.lblCmdDescription = new System.Windows.Forms.Label();
		this.btnAllDefault = new System.Windows.Forms.Button();
		this.grpCommands.SuspendLayout();
		this.grpCurKey.SuspendLayout();
		this.grpNewShortcut.SuspendLayout();
		this.grpShortUsed.SuspendLayout();
		this.grpDescription.SuspendLayout();
		base.SuspendLayout();
		this.grpCommands.Controls.Add(this.lstCommand);
		resources.ApplyResources(this.grpCommands, "grpCommands");
		this.grpCommands.Name = "grpCommands";
		this.grpCommands.TabStop = false;
		resources.ApplyResources(this.lstCommand, "lstCommand");
		this.lstCommand.FormattingEnabled = true;
		this.lstCommand.Name = "lstCommand";
		this.lstCommand.SelectedIndexChanged += new System.EventHandler(lstCommand_SelectedIndexChanged);
		resources.ApplyResources(this.btnRemoveShortcut, "btnRemoveShortcut");
		this.btnRemoveShortcut.Name = "btnRemoveShortcut";
		this.btnRemoveShortcut.UseVisualStyleBackColor = true;
		this.btnRemoveShortcut.Click += new System.EventHandler(btnRemoveShortcut_Click);
		resources.ApplyResources(this.btnAssignShortcut, "btnAssignShortcut");
		this.btnAssignShortcut.Name = "btnAssignShortcut";
		this.btnAssignShortcut.UseVisualStyleBackColor = true;
		this.btnAssignShortcut.Click += new System.EventHandler(btnAssignShortcut_Click);
		resources.ApplyResources(this.txtNewShortcut, "txtNewShortcut");
		this.txtNewShortcut.Name = "txtNewShortcut";
		this.txtNewShortcut.KeyDown += new System.Windows.Forms.KeyEventHandler(txtNewShortcut_KeyDown);
		this.grpCurKey.Controls.Add(this.btnSetToDefault);
		this.grpCurKey.Controls.Add(this.lblCurShortcut);
		this.grpCurKey.Controls.Add(this.btnRemoveShortcut);
		resources.ApplyResources(this.grpCurKey, "grpCurKey");
		this.grpCurKey.Name = "grpCurKey";
		this.grpCurKey.TabStop = false;
		resources.ApplyResources(this.btnSetToDefault, "btnSetToDefault");
		this.btnSetToDefault.Name = "btnSetToDefault";
		this.btnSetToDefault.UseVisualStyleBackColor = true;
		this.btnSetToDefault.Click += new System.EventHandler(btnSetToDefault_Click);
		this.lblCurShortcut.BackColor = System.Drawing.SystemColors.Control;
		resources.ApplyResources(this.lblCurShortcut, "lblCurShortcut");
		this.lblCurShortcut.Name = "lblCurShortcut";
		this.grpNewShortcut.Controls.Add(this.btnAddShortcut);
		this.grpNewShortcut.Controls.Add(this.txtNewShortcut);
		this.grpNewShortcut.Controls.Add(this.btnAssignShortcut);
		resources.ApplyResources(this.grpNewShortcut, "grpNewShortcut");
		this.grpNewShortcut.Name = "grpNewShortcut";
		this.grpNewShortcut.TabStop = false;
		resources.ApplyResources(this.btnAddShortcut, "btnAddShortcut");
		this.btnAddShortcut.Name = "btnAddShortcut";
		this.btnAddShortcut.UseVisualStyleBackColor = true;
		this.btnAddShortcut.Click += new System.EventHandler(btnAddShortcut_Click);
		this.grpShortUsed.Controls.Add(this.lblUsedBy);
		resources.ApplyResources(this.grpShortUsed, "grpShortUsed");
		this.grpShortUsed.Name = "grpShortUsed";
		this.grpShortUsed.TabStop = false;
		this.lblUsedBy.BackColor = System.Drawing.SystemColors.Control;
		resources.ApplyResources(this.lblUsedBy, "lblUsedBy");
		this.lblUsedBy.Name = "lblUsedBy";
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		resources.ApplyResources(this.btnCancel, "btnCancel");
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.cxMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[1] { this.mnuClear });
		this.cxMenu.Popup += new System.EventHandler(cxMenue_Popup);
		this.mnuClear.Index = 0;
		resources.ApplyResources(this.mnuClear, "mnuClear");
		this.mnuClear.Click += new System.EventHandler(mnuClear_Click);
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		resources.ApplyResources(this.btnOK, "btnOK");
		this.btnOK.Name = "btnOK";
		this.btnOK.UseVisualStyleBackColor = true;
		this.grpDescription.Controls.Add(this.lblCmdDescription);
		resources.ApplyResources(this.grpDescription, "grpDescription");
		this.grpDescription.Name = "grpDescription";
		this.grpDescription.TabStop = false;
		resources.ApplyResources(this.lblCmdDescription, "lblCmdDescription");
		this.lblCmdDescription.Name = "lblCmdDescription";
		resources.ApplyResources(this.btnAllDefault, "btnAllDefault");
		this.btnAllDefault.Name = "btnAllDefault";
		this.btnAllDefault.UseVisualStyleBackColor = true;
		this.btnAllDefault.Click += new System.EventHandler(btnAllDefault_Click);
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.btnAllDefault);
		base.Controls.Add(this.grpDescription);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.grpShortUsed);
		base.Controls.Add(this.grpNewShortcut);
		base.Controls.Add(this.grpCurKey);
		base.Controls.Add(this.grpCommands);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "CustomizeKeyboardDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.grpCommands.ResumeLayout(false);
		this.grpCurKey.ResumeLayout(false);
		this.grpNewShortcut.ResumeLayout(false);
		this.grpNewShortcut.PerformLayout();
		this.grpShortUsed.ResumeLayout(false);
		this.grpDescription.ResumeLayout(false);
		this.grpDescription.PerformLayout();
		base.ResumeLayout(false);
	}
}
