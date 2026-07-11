using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public class ArtDefSetControl : UserControl
{
	private IArtDefSetContext m_artDefSetContext;

	private readonly ICivTechService m_civTechService;

	private readonly IFileDialogService m_fileDialog;

	private IContainer components;

	private Label lblTemplateName;

	private ComboBox listArtDefTemplate;

	private SplitContainer artDefSplitter;

	private SplitContainer treeSplitContainer;

	private Label helpLabel;

	public Panel PanelTreeArea => treeSplitContainer.Panel1;

	public Panel PropertyArea => treeSplitContainer.Panel2;

	public bool TemplateReadOnly
	{
		get
		{
			return !listArtDefTemplate.Enabled;
		}
		internal set
		{
			listArtDefTemplate.Enabled = !value;
		}
	}

	public ArtDefSetControl(IFileDialogService fileDialog, ICivTechService civTechSvc)
	{
		InitializeComponent();
		m_fileDialog = fileDialog;
		m_civTechService = civTechSvc;
		RefreshTemplateList();
	}

	public void Bind(IArtDefSetContext controlContext)
	{
		m_artDefSetContext = controlContext;
		m_artDefSetContext.TemplateChanged += ControlContext_TemplateChanged;
		RefreshSelectedTemplate(m_artDefSetContext.TemplateName);
		m_artDefSetContext.As<ArtDefSetAdapter>();
	}

	public void SetDescription(string descriptionText)
	{
		if (string.IsNullOrEmpty(descriptionText))
		{
			descriptionText = "No help available.";
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

	private void ControlContext_TemplateChanged(object sender, ItemChangedEventArgs<string> e)
	{
		RefreshSelectedTemplate(e.Item);
	}

	private void listArtDefTemplate_SelectedIndexChanged(object sender, EventArgs e)
	{
		IArtDefTemplate artDefTemplate = listArtDefTemplate.SelectedItem as IArtDefTemplate;
		m_artDefSetContext.As<ArtDefSetAdapter>().TemplateName = artDefTemplate.Name;
		m_artDefSetContext.TemplateName = artDefTemplate.Name;
	}

	private void RefreshSelectedTemplate(string newName)
	{
		listArtDefTemplate.SelectedIndex = listArtDefTemplate.FindString(newName);
		if (listArtDefTemplate.SelectedItem == null)
		{
			Outputs.WriteLine(OutputMessageType.Error, "{0} is not a supported art def template", newName);
		}
		else
		{
			IArtDefTemplate artDefTemplate = listArtDefTemplate.SelectedItem as IArtDefTemplate;
			SetDescription(artDefTemplate.Description);
		}
	}

	private void RefreshTemplateList()
	{
		listArtDefTemplate.Items.Clear();
		IArtDefTemplate[] array = m_civTechService.PrimaryProject.Config.ArtDefTemplates.Items.OrderBy((IArtDefTemplate x) => x.Name).ToArray();
		ComboBox.ObjectCollection items = listArtDefTemplate.Items;
		object[] array2 = array;
		object[] items2 = array2;
		items.AddRange(items2);
	}

	private void InitializeComponent()
	{
		this.lblTemplateName = new System.Windows.Forms.Label();
		this.helpLabel = new System.Windows.Forms.Label();
		this.listArtDefTemplate = new System.Windows.Forms.ComboBox();
		this.artDefSplitter = new System.Windows.Forms.SplitContainer();
		this.treeSplitContainer = new System.Windows.Forms.SplitContainer();
		((System.ComponentModel.ISupportInitialize)this.artDefSplitter).BeginInit();
		this.artDefSplitter.Panel1.SuspendLayout();
		this.artDefSplitter.Panel2.SuspendLayout();
		this.artDefSplitter.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.treeSplitContainer).BeginInit();
		this.treeSplitContainer.SuspendLayout();
		base.SuspendLayout();
		this.lblTemplateName.AutoSize = true;
		this.lblTemplateName.Location = new System.Drawing.Point(6, 7);
		this.lblTemplateName.Name = "lblTemplateName";
		this.lblTemplateName.Size = new System.Drawing.Size(54, 13);
		this.lblTemplateName.TabIndex = 4;
		this.lblTemplateName.Text = "Template:";
		this.helpLabel.AutoSize = true;
		this.helpLabel.Dock = System.Windows.Forms.DockStyle.Right;
		this.helpLabel.Location = new System.Drawing.Point(922, 16);
		this.helpLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
		this.helpLabel.Name = "helpLabel";
		this.helpLabel.Size = new System.Drawing.Size(19, 13);
		this.helpLabel.TabIndex = 6;
		this.helpLabel.Text = "[?]";
		this.listArtDefTemplate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.listArtDefTemplate.DisplayMember = "Name";
		this.listArtDefTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.listArtDefTemplate.FormattingEnabled = true;
		this.listArtDefTemplate.Location = new System.Drawing.Point(66, 3);
		this.listArtDefTemplate.Name = "listArtDefTemplate";
		this.listArtDefTemplate.Size = new System.Drawing.Size(882, 21);
		this.listArtDefTemplate.TabIndex = 2;
		this.listArtDefTemplate.SelectedIndexChanged += new System.EventHandler(listArtDefTemplate_SelectedIndexChanged);
		this.artDefSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
		this.artDefSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
		this.artDefSplitter.IsSplitterFixed = true;
		this.artDefSplitter.Location = new System.Drawing.Point(0, 0);
		this.artDefSplitter.Name = "artDefSplitter";
		this.artDefSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.artDefSplitter.Panel1.Controls.Add(this.lblTemplateName);
		this.artDefSplitter.Panel1.Controls.Add(this.listArtDefTemplate);
		this.artDefSplitter.Panel1.Padding = new System.Windows.Forms.Padding(3);
		this.artDefSplitter.Panel2.Controls.Add(this.treeSplitContainer);
		this.artDefSplitter.Panel2.Padding = new System.Windows.Forms.Padding(3);
		this.artDefSplitter.Size = new System.Drawing.Size(954, 600);
		this.artDefSplitter.SplitterDistance = 28;
		this.artDefSplitter.SplitterWidth = 1;
		this.artDefSplitter.TabIndex = 5;
		this.treeSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.treeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeSplitContainer.Location = new System.Drawing.Point(3, 3);
		this.treeSplitContainer.Margin = new System.Windows.Forms.Padding(5);
		this.treeSplitContainer.Name = "treeSplitContainer";
		this.treeSplitContainer.Panel1.Margin = new System.Windows.Forms.Padding(3);
		this.treeSplitContainer.Panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		this.treeSplitContainer.Panel2.Margin = new System.Windows.Forms.Padding(3);
		this.treeSplitContainer.Size = new System.Drawing.Size(948, 565);
		this.treeSplitContainer.SplitterDistance = 354;
		this.treeSplitContainer.TabIndex = 7;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.artDefSplitter);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f);
		base.Name = "ArtDefSetControl";
		base.Size = new System.Drawing.Size(954, 600);
		this.artDefSplitter.Panel1.ResumeLayout(false);
		this.artDefSplitter.Panel1.PerformLayout();
		this.artDefSplitter.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.artDefSplitter).EndInit();
		this.artDefSplitter.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.treeSplitContainer).EndInit();
		this.treeSplitContainer.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
