using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

internal class SettingsLoadSaveDialog : Form
{
	private readonly SettingsService m_settingsService;

	private readonly SaveFileDialog m_saveDialog;

	private readonly OpenFileDialog m_openDialog;

	private IContainer components = null;

	private Label lblTitle;

	private RadioButton m_saveRadioButton;

	private RadioButton m_loadRadioButton;

	private Button m_btnCancel;

	private Button m_btnProceed;

	private Label m_lblExport;

	private Label m_lblImport;

	private GroupBox m_grpImportExport;

	public SettingsLoadSaveDialog(SettingsService settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException();
		}
		m_settingsService = settings;
		InitializeComponent();
		m_saveDialog = new SaveFileDialog();
		m_saveDialog.OverwritePrompt = true;
		m_saveDialog.Title = "Export settings".Localize();
		m_saveDialog.Filter = "Setting file".Localize() + "(*.xml)|*.xml";
		m_saveDialog.InitialDirectory = m_settingsService.DefaultSettingsPath;
		m_openDialog = new OpenFileDialog();
		m_openDialog.Title = "Import settings".Localize();
		m_openDialog.CheckFileExists = true;
		m_openDialog.CheckPathExists = true;
		m_openDialog.Multiselect = false;
		m_openDialog.Filter = m_saveDialog.Filter;
		m_openDialog.InitialDirectory = m_settingsService.DefaultSettingsPath;
	}

	private void m_btnProceed_Click(object sender, EventArgs e)
	{
		if (m_saveRadioButton.Checked)
		{
			if (m_saveDialog.ShowDialog(this) == DialogResult.OK)
			{
				SaveSettings(m_saveDialog.FileName);
			}
		}
		else if (m_loadRadioButton.Checked && m_openDialog.ShowDialog(this) == DialogResult.OK && LoadSettings(m_openDialog.FileName))
		{
		}
	}

	private void SaveSettings(string fullFileName)
	{
		Stream stream = null;
		try
		{
			stream = File.Create(fullFileName);
			m_settingsService.Serialize(stream);
		}
		finally
		{
			stream?.Close();
		}
	}

	private bool LoadSettings(string fullFileName)
	{
		bool flag = false;
		Stream stream = null;
		try
		{
			stream = File.OpenRead(fullFileName);
			flag = m_settingsService.Deserialize(stream);
			if (flag)
			{
				m_settingsService.SaveSettings();
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
		finally
		{
			stream?.Close();
		}
		return flag;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.SettingsLoadSaveDialog));
		this.lblTitle = new System.Windows.Forms.Label();
		this.m_saveRadioButton = new System.Windows.Forms.RadioButton();
		this.m_loadRadioButton = new System.Windows.Forms.RadioButton();
		this.m_btnCancel = new System.Windows.Forms.Button();
		this.m_btnProceed = new System.Windows.Forms.Button();
		this.m_lblExport = new System.Windows.Forms.Label();
		this.m_lblImport = new System.Windows.Forms.Label();
		this.m_grpImportExport = new System.Windows.Forms.GroupBox();
		this.m_grpImportExport.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this.lblTitle, "lblTitle");
		this.lblTitle.Name = "lblTitle";
		resources.ApplyResources(this.m_saveRadioButton, "m_saveRadioButton");
		this.m_saveRadioButton.Name = "m_saveRadioButton";
		this.m_saveRadioButton.TabStop = true;
		this.m_saveRadioButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.m_loadRadioButton, "m_loadRadioButton");
		this.m_loadRadioButton.Name = "m_loadRadioButton";
		this.m_loadRadioButton.TabStop = true;
		this.m_loadRadioButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
		this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_btnCancel.Name = "m_btnCancel";
		this.m_btnCancel.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.m_btnProceed, "m_btnProceed");
		this.m_btnProceed.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnProceed.Name = "m_btnProceed";
		this.m_btnProceed.UseVisualStyleBackColor = true;
		this.m_btnProceed.Click += new System.EventHandler(m_btnProceed_Click);
		resources.ApplyResources(this.m_lblExport, "m_lblExport");
		this.m_lblExport.Name = "m_lblExport";
		this.m_lblExport.Tag = "";
		resources.ApplyResources(this.m_lblImport, "m_lblImport");
		this.m_lblImport.Name = "m_lblImport";
		this.m_grpImportExport.Controls.Add(this.m_saveRadioButton);
		this.m_grpImportExport.Controls.Add(this.m_lblImport);
		this.m_grpImportExport.Controls.Add(this.m_loadRadioButton);
		this.m_grpImportExport.Controls.Add(this.m_lblExport);
		resources.ApplyResources(this.m_grpImportExport, "m_grpImportExport");
		this.m_grpImportExport.Name = "m_grpImportExport";
		this.m_grpImportExport.TabStop = false;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_grpImportExport);
		base.Controls.Add(this.m_btnProceed);
		base.Controls.Add(this.m_btnCancel);
		base.Controls.Add(this.lblTitle);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "SettingsLoadSaveDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.m_grpImportExport.ResumeLayout(false);
		this.m_grpImportExport.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
