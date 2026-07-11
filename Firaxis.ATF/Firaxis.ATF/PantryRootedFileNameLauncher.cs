using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class PantryRootedFileNameLauncher : UserControl
{
	private string m_selectedData;

	private bool m_userPressedOK;

	private readonly ITypeDescriptorContext m_editingContext;

	private readonly IEnumerable<IDocumentClient> m_documentClients;

	private readonly IFileDialogService m_fileDialogService;

	private readonly IDocumentService m_fileCommands;

	private readonly string m_filter;

	private readonly string m_root;

	private readonly object m_valueToEdit;

	private readonly IWindowsFormsEditorService m_wfes;

	private IContainer components;

	private Button _openEntryButton;

	private Button _browseButton;

	public string SelectedData
	{
		get
		{
			return m_selectedData;
		}
		private set
		{
			m_selectedData = value;
		}
	}

	public bool UserPressedOK
	{
		get
		{
			return m_userPressedOK;
		}
		private set
		{
			m_userPressedOK = value;
		}
	}

	public PantryRootedFileNameLauncher(IWindowsFormsEditorService wfes, ITypeDescriptorContext context, object valueToEdit, IFileDialogService fileDialogService, IDocumentService fileCommands, IEnumerable<IDocumentClient> documentClients, string root, string filter)
	{
		InitializeComponent();
		Image image = ResourceUtil.GetImage16(Resources.GotoFileIcon);
		_openEntryButton.Image = image;
		m_editingContext = context;
		m_valueToEdit = valueToEdit;
		m_wfes = wfes;
		m_fileDialogService = fileDialogService;
		m_fileCommands = fileCommands;
		m_documentClients = documentClients;
		m_root = root;
		m_filter = filter;
		_openEntryButton.Enabled = IsFileNameSet();
	}

	private void _browseButton_Click(object sender, EventArgs e)
	{
		string pathName = GetFileName();
		m_fileDialogService.ForcedInitialDirectory = m_root;
		if (m_fileDialogService.OpenFileName(ref pathName, m_filter) == FileDialogResult.OK)
		{
			UserPressedOK = true;
			SelectedData = pathName;
		}
		m_fileDialogService.ForcedInitialDirectory = null;
	}

	private void _openEntryButton_Click(object sender, EventArgs e)
	{
		string fullFilePath = GetFullFilePath();
		if (!string.IsNullOrEmpty(fullFilePath))
		{
			OpenFile(fullFilePath);
		}
	}

	private string GetFileName()
	{
		if (!(m_valueToEdit is string))
		{
			return string.Empty;
		}
		return m_valueToEdit as string;
	}

	private string GetFullFilePath()
	{
		string text = GetFileName();
		if (!string.IsNullOrEmpty(text) && !text.StartsWith(m_root))
		{
			text = Path.Combine(m_root, text);
		}
		return text;
	}

	private bool IsFileNameSet()
	{
		return !string.IsNullOrEmpty(GetFileName());
	}

	private void OpenFile(string filePath)
	{
		if (File.Exists(filePath))
		{
			IDocumentClient firstClientForPath = m_documentClients.GetFirstClientForPath(filePath);
			if (firstClientForPath != null)
			{
				Uri uri = new Uri(filePath);
				m_fileCommands.OpenExistingDocument(firstClientForPath, uri);
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, "Unable to find a document client for {0}", filePath);
			}
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "{0} does not exist.", filePath);
		}
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
		this._openEntryButton = new System.Windows.Forms.Button();
		this._browseButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this._openEntryButton.Location = new System.Drawing.Point(88, 2);
		this._openEntryButton.Name = "_openEntryButton";
		this._openEntryButton.Size = new System.Drawing.Size(23, 23);
		this._openEntryButton.TabIndex = 3;
		this._openEntryButton.UseVisualStyleBackColor = true;
		this._openEntryButton.Click += new System.EventHandler(_openEntryButton_Click);
		this._browseButton.Location = new System.Drawing.Point(7, 2);
		this._browseButton.Name = "_browseButton";
		this._browseButton.Size = new System.Drawing.Size(75, 23);
		this._browseButton.TabIndex = 2;
		this._browseButton.Text = "Browse...";
		this._browseButton.UseVisualStyleBackColor = true;
		this._browseButton.Click += new System.EventHandler(_browseButton_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this._openEntryButton);
		base.Controls.Add(this._browseButton);
		base.Name = "PantryRootedFileNameLauncher";
		base.Size = new System.Drawing.Size(119, 27);
		base.ResumeLayout(false);
	}
}
