using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

internal class SkinEditor : Form
{
	private ToolStripMenuItem m_openFolderMenu = new ToolStripMenuItem();

	private ToolStripMenuItem m_saveMenu = new ToolStripMenuItem();

	private ToolStripMenuItem m_saveAsMenu = new ToolStripMenuItem();

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_PropertyGrid;

	private TreeControlAdapter m_treeControlAdapter;

	private TreeControl m_treeControl;

	private MenuStrip m_menu;

	private SkinDocument m_activeDocument;

	private SkinServiceEditor SkinServiceEditor { get; set; }

	private SkinServiceCommands SkinServiceCommands { get; set; }

	public SkinDocument ActiveDocument => m_activeDocument;

	public event EventHandler<DocumentEventArgs> SkinChanged;

	public SkinEditor(SkinServiceEditor editor, SkinServiceCommands sknCmds)
	{
		SkinServiceEditor = editor;
		SkinServiceCommands = sknCmds;
		Init();
	}

	public void OpenSkin(string fileName)
	{
		if (SkinServiceEditor.Open(new Uri(fileName)) is SkinDocument activeDocument)
		{
			SetActiveDocument(activeDocument);
			m_activeDocument.Uri = new Uri(fileName);
		}
	}

	public Stream GetCurrentSkin()
	{
		if (m_activeDocument == null)
		{
			return Stream.Null;
		}
		MemoryStream memoryStream = new MemoryStream();
		m_activeDocument.Write(memoryStream);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return memoryStream;
	}

	public bool ConfirmCloseActiveDocument(bool closeActiveSkin = true)
	{
		bool flag = true;
		if (m_activeDocument != null && m_activeDocument.Dirty)
		{
			switch (MessageBox.Show(this, "Would you like to save the changes?".Localize(), "Unsaved changes".Localize(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1))
			{
			case DialogResult.Yes:
				if (m_activeDocument.Uri != null)
				{
					SaveActiveDocument();
				}
				else
				{
					flag = SaveAsActiveDocument();
				}
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		if (flag && closeActiveSkin)
		{
			m_activeDocument = null;
			m_treeControlAdapter.TreeView = null;
			m_PropertyGrid.Bind(null);
		}
		return flag;
	}

	protected virtual void OnSkinChanged()
	{
		DocumentEventArgs e = new DocumentEventArgs(m_activeDocument);
		this.SkinChanged.Raise(this, e);
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		e.Cancel = !ConfirmCloseActiveDocument();
	}

	private void NewToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!ConfirmCloseActiveDocument())
		{
			return;
		}
		Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sce.Atf.Applications.SkinService.Skin.tmpl");
		if (manifestResourceStream == null)
		{
			return;
		}
		try
		{
			SkinDocument skinDocument = SkinServiceEditor.Open(manifestResourceStream) as SkinDocument;
			skinDocument.Uri = new Uri("res://Sce.Atf.Applications.SkinService.Skin.tmpl");
			SetActiveDocument(skinDocument);
		}
		finally
		{
			manifestResourceStream.Close();
		}
	}

	private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!ConfirmCloseActiveDocument(closeActiveSkin: false))
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Skin (*.skn)|*.skn";
		openFileDialog.CheckFileExists = true;
		openFileDialog.InitialDirectory = SkinServiceCommands.SkinsDirectory;
		if (openFileDialog.ShowDialog(this) == DialogResult.OK)
		{
			string fileName = openFileDialog.FileName;
			OpenSkin(fileName);
		}
	}

	private void OpenFolderMenu_Click(object sender, EventArgs e)
	{
		string text = ((m_activeDocument != null && m_activeDocument.Uri != null) ? m_activeDocument.Uri.LocalPath : null);
		if (text != null)
		{
			string directoryName = Path.GetDirectoryName(text);
			Process.Start("explorer.exe", directoryName);
		}
	}

	private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (m_activeDocument != null)
		{
			if (m_activeDocument.Uri != null)
			{
				SaveActiveDocument();
			}
			else
			{
				SaveAsActiveDocument();
			}
		}
	}

	private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (m_activeDocument != null)
		{
			SaveAsActiveDocument();
		}
	}

	private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private bool SaveAsActiveDocument()
	{
		bool result = false;
		using (SaveFileDialog saveFileDialog = new SaveFileDialog())
		{
			saveFileDialog.OverwritePrompt = true;
			saveFileDialog.Filter = "Skin (*.skn)|*.skn";
			if (m_activeDocument.Uri != null)
			{
				saveFileDialog.InitialDirectory = Path.GetDirectoryName(m_activeDocument.Uri.LocalPath);
				saveFileDialog.FileName = Path.GetFileName(m_activeDocument.Uri.LocalPath);
			}
			else
			{
				saveFileDialog.InitialDirectory = SkinServiceCommands.SkinsDirectory;
			}
			if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				result = true;
				m_activeDocument.Uri = new Uri(saveFileDialog.FileName);
				SaveActiveDocument();
			}
		}
		return result;
	}

	private void SaveActiveDocument()
	{
		if (m_activeDocument != null && !(m_activeDocument.Uri == null))
		{
			using (FileStream stream = new FileStream(m_activeDocument.Uri.LocalPath, FileMode.Create))
			{
				m_activeDocument.Write(stream);
			}
			m_activeDocument.Dirty = false;
			OnSkinChanged();
		}
	}

	private void Init()
	{
		m_PropertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.PropertySorting | PropertyGridMode.DisplayDescriptions | PropertyGridMode.HideResetAllButton);
		m_treeControl = new TreeControl();
		m_menu = new MenuStrip();
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem();
		SplitContainer splitContainer = new SplitContainer();
		m_menu.SuspendLayout();
		splitContainer.BeginInit();
		splitContainer.Panel1.SuspendLayout();
		splitContainer.Panel2.SuspendLayout();
		splitContainer.SuspendLayout();
		SuspendLayout();
		m_menu.Location = new Point(0, 0);
		m_menu.Name = "m_menu";
		m_menu.TabIndex = 0;
		m_menu.Text = "m_menu";
		m_menu.Items.Add(toolStripMenuItem);
		toolStripMenuItem.Name = "fileToolStripMenuItem";
		toolStripMenuItem.Size = new Size(37, 20);
		toolStripMenuItem.Text = "File".Localize();
		toolStripMenuItem.DropDownOpening += fileMenu_DropDownOpening;
		toolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[6] { toolStripMenuItem2, toolStripMenuItem3, m_openFolderMenu, m_saveMenu, m_saveAsMenu, toolStripMenuItem4 });
		toolStripMenuItem2.Name = "newToolStripMenuItem";
		toolStripMenuItem2.ShortcutKeys = Keys.N | Keys.Control;
		toolStripMenuItem2.Text = "New".Localize();
		toolStripMenuItem2.Click += NewToolStripMenuItem_Click;
		toolStripMenuItem3.Name = "openToolStripMenuItem";
		toolStripMenuItem3.ShortcutKeys = Keys.O | Keys.Control;
		toolStripMenuItem3.Text = "Open...".Localize();
		toolStripMenuItem3.Click += OpenToolStripMenuItem_Click;
		m_openFolderMenu.Name = "OpenFolderMenu";
		m_openFolderMenu.Text = "Open Containing Folder".Localize();
		m_openFolderMenu.Click += OpenFolderMenu_Click;
		m_saveMenu.Name = "saveToolStripMenuItem";
		m_saveMenu.ShortcutKeys = Keys.S | Keys.Control;
		m_saveMenu.Text = "Save".Localize();
		m_saveMenu.Click += SaveToolStripMenuItem_Click;
		m_saveAsMenu.Name = "saveAsToolStripMenuItem";
		m_saveAsMenu.Text = "Save As...".Localize();
		m_saveAsMenu.Click += SaveAsToolStripMenuItem_Click;
		toolStripMenuItem4.Name = "exitToolStripMenuItem";
		toolStripMenuItem4.Text = "Exit".Localize();
		toolStripMenuItem4.Click += ExitToolStripMenuItem_Click;
		m_treeControl.Dock = DockStyle.Fill;
		m_treeControl.Name = "m_treeControl";
		m_treeControl.TabIndex = 1;
		m_treeControl.Width = 150;
		m_treeControl.ShowRoot = false;
		m_treeControl.AllowDrop = false;
		m_treeControl.SelectionMode = SelectionMode.One;
		m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
		m_PropertyGrid.Dock = DockStyle.Fill;
		m_PropertyGrid.Name = "propertyGrid1";
		m_PropertyGrid.TabIndex = 3;
		m_PropertyGrid.PropertySorting = PropertySorting.None;
		splitContainer.Dock = DockStyle.Fill;
		splitContainer.Name = "splitContainer1";
		splitContainer.Panel1.Controls.Add(m_treeControl);
		splitContainer.Panel2.Controls.Add(m_PropertyGrid);
		splitContainer.SplitterDistance = 100;
		splitContainer.TabIndex = 1;
		base.AutoScaleMode = AutoScaleMode.Font;
		base.ClientSize = new Size(600, 400);
		base.Controls.Add(splitContainer);
		base.Controls.Add(m_menu);
		base.MainMenuStrip = m_menu;
		base.Name = "SkinEditor";
		base.Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Resources.AtfIconImage));
		UpdateTitleText();
		m_menu.ResumeLayout(performLayout: false);
		m_menu.PerformLayout();
		splitContainer.Panel1.ResumeLayout(performLayout: false);
		splitContainer.Panel2.ResumeLayout(performLayout: false);
		splitContainer.EndInit();
		splitContainer.ResumeLayout(performLayout: false);
		ResumeLayout(performLayout: false);
		PerformLayout();
		splitContainer.SplitterDistance = 170;
	}

	private void fileMenu_DropDownOpening(object sender, EventArgs e)
	{
		m_saveMenu.Enabled = m_activeDocument != null && m_activeDocument.Dirty;
		m_saveAsMenu.Enabled = m_activeDocument != null;
		m_openFolderMenu.Enabled = m_activeDocument != null && m_activeDocument.Uri != null;
	}

	private void SetActiveDocument(SkinDocument actDoc)
	{
		m_activeDocument = actDoc;
		SkinEditingContext context = actDoc.DomNode.As<SkinEditingContext>();
		m_treeControlAdapter.TreeView = context;
		m_PropertyGrid.Bind(null);
		context.SelectionChanged += delegate
		{
			DomNode lastSelected = context.GetLastSelected<DomNode>();
			CustomPropertyEditingContext context2 = new CustomPropertyEditingContext(lastSelected, m_activeDocument);
			m_PropertyGrid.Bind(context2);
		};
		context.Ended += Context_Refresh;
		context.Cancelled += Context_Refresh;
		m_activeDocument.UriChanged += delegate
		{
			UpdateTitleText();
		};
		m_activeDocument.DirtyChanged += delegate
		{
			UpdateTitleText();
		};
		UpdateTitleText();
		OnSkinChanged();
	}

	private void Context_Refresh(object sender, EventArgs e)
	{
	}

	private void UpdateTitleText()
	{
		string text = "Skin Editor".Localize();
		if (m_activeDocument != null)
		{
			text += ((m_activeDocument.Uri != null) ? (": " + m_activeDocument.Uri.LocalPath) : ": Untitled");
			if (m_activeDocument.Dirty)
			{
				text += "*";
			}
		}
		Text = text;
	}
}
