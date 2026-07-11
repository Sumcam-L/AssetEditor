using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

internal class SettingsDialog : Form
{
	private readonly SettingsService m_settingsService;

	private readonly IWin32Window m_dialogOwner;

	private readonly object m_originalState;

	private readonly TreeControl m_treeControl;

	private readonly TreeControlAdapter m_treeControlAdapter;

	private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;

	private Button cancelButton;

	private Button okButton;

	private Button defaultsButton;

	private Panel buttonPanel;

	private Panel upperPanel;

	private SplitContainer splitContainer;

	private Panel treePanel;

	private Panel propertiesPanel;

	private readonly Container components = null;

	public string Settings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement xmlElement = xmlDocument.CreateElement("SettingsDialog");
			xmlElement.SetAttribute("SplitterRatio", SplitterRatio.ToString());
			string settings = m_propertyGrid.Settings;
			xmlElement.SetAttribute("PropertyGrid", settings);
			xmlDocument.AppendChild(xmlElement);
			return xmlDocument.InnerXml;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(value);
			XmlElement documentElement = xmlDocument.DocumentElement;
			if (documentElement != null && !(documentElement.Name != "SettingsDialog"))
			{
				string attribute = documentElement.GetAttribute("SplitterRatio");
				if (float.TryParse(attribute, out var result))
				{
					SplitterRatio = result;
				}
				attribute = documentElement.GetAttribute("PropertyGrid");
				m_propertyGrid.Settings = attribute;
			}
		}
	}

	public TreeControl TreeControl => m_treeControl;

	private float SplitterRatio
	{
		get
		{
			return (float)splitContainer.SplitterDistance / (float)splitContainer.Width;
		}
		set
		{
			splitContainer.SplitterDistance = (int)(value * (float)splitContainer.Width);
		}
	}

	public SettingsDialog(SettingsService settingsService, IWin32Window dialogOwner, string pathName)
	{
		InitializeComponent();
		SplitterRatio = 0.33f;
		m_settingsService = settingsService;
		m_dialogOwner = dialogOwner;
		m_originalState = m_settingsService.UserState;
		m_treeControl = new TreeControl(TreeControl.Style.SimpleTree);
		m_treeControl.Dock = DockStyle.Fill;
		m_treeControl.SelectionMode = SelectionMode.One;
		m_treeControl.ShowRoot = false;
		m_treeControl.ImageList = ResourceUtil.GetImageList16();
		m_treeControl.ExpandAll();
		m_treeControl.NodeSelectedChanged += treeControl_NodeSelectedChanged;
		m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
		m_treeControlAdapter.TreeView = settingsService.UserSettings;
		treePanel.Controls.Add(m_treeControl);
		m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
		m_propertyGrid.Dock = DockStyle.Fill;
		propertiesPanel.Controls.Add(m_propertyGrid);
		TreeControl.Node node = ((pathName == null) ? m_treeControl.ExpandToFirstLeaf() : m_treeControlAdapter.ExpandPath(m_settingsService.GetSettingsPath(pathName)));
		node.Selected = true;
		ShowProperties(m_settingsService.GetProperties((Tree<object>)node.Tag));
		defaultsButton.Click += DefaultsButton_Click;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void treeControl_NodeSelectedChanged(object sender, TreeControl.NodeEventArgs e)
	{
		if (e.Node.Selected)
		{
			ShowProperties(m_settingsService.GetProperties((Tree<object>)e.Node.Tag));
		}
	}

	private void DefaultsButton_Click(object sender, EventArgs e)
	{
		ConfirmationDialog confirmationDialog = new ConfirmationDialog("Reset All Preferences".Localize("Reset all preferences to their default values?"), "Reset all preferences to their default values?".Localize());
		DialogResult dialogResult = confirmationDialog.ShowDialog(m_dialogOwner);
		if (dialogResult == DialogResult.Yes)
		{
			m_settingsService.SetDefaults();
			m_propertyGrid.Refresh();
		}
	}

	private void SettingsDialog_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (base.DialogResult != DialogResult.OK)
		{
			m_settingsService.UserState = m_originalState;
		}
	}

	private void ShowProperties(List<PropertyDescriptor> properties)
	{
		if (properties != null)
		{
			m_propertyGrid.Bind(new PropertyCollectionWrapper(properties.ToArray()));
		}
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.SettingsDialog));
		this.cancelButton = new System.Windows.Forms.Button();
		this.okButton = new System.Windows.Forms.Button();
		this.defaultsButton = new System.Windows.Forms.Button();
		this.buttonPanel = new System.Windows.Forms.Panel();
		this.upperPanel = new System.Windows.Forms.Panel();
		this.splitContainer = new System.Windows.Forms.SplitContainer();
		this.treePanel = new System.Windows.Forms.Panel();
		this.propertiesPanel = new System.Windows.Forms.Panel();
		this.buttonPanel.SuspendLayout();
		this.upperPanel.SuspendLayout();
		this.splitContainer.Panel1.SuspendLayout();
		this.splitContainer.Panel2.SuspendLayout();
		this.splitContainer.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this.cancelButton, "cancelButton");
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Name = "cancelButton";
		resources.ApplyResources(this.okButton, "okButton");
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Name = "okButton";
		resources.ApplyResources(this.defaultsButton, "defaultsButton");
		this.defaultsButton.Name = "defaultsButton";
		this.buttonPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.buttonPanel.Controls.Add(this.defaultsButton);
		this.buttonPanel.Controls.Add(this.okButton);
		this.buttonPanel.Controls.Add(this.cancelButton);
		resources.ApplyResources(this.buttonPanel, "buttonPanel");
		this.buttonPanel.Name = "buttonPanel";
		this.upperPanel.Controls.Add(this.splitContainer);
		resources.ApplyResources(this.upperPanel, "upperPanel");
		this.upperPanel.Name = "upperPanel";
		resources.ApplyResources(this.splitContainer, "splitContainer1");
		this.splitContainer.Name = "splitContainer1";
		this.splitContainer.Panel1.Controls.Add(this.treePanel);
		this.splitContainer.Panel2.Controls.Add(this.propertiesPanel);
		resources.ApplyResources(this.treePanel, "treePanel");
		this.treePanel.Name = "treePanel";
		resources.ApplyResources(this.propertiesPanel, "propertiesPanel");
		this.propertiesPanel.Name = "propertiesPanel";
		resources.ApplyResources(this, "$this");
		base.Controls.Add(this.upperPanel);
		base.Controls.Add(this.buttonPanel);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SettingsDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(SettingsDialog_FormClosing);
		this.buttonPanel.ResumeLayout(false);
		this.upperPanel.ResumeLayout(false);
		this.splitContainer.Panel1.ResumeLayout(false);
		this.splitContainer.Panel2.ResumeLayout(false);
		this.splitContainer.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
