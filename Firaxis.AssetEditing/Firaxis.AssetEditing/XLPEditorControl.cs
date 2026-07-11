using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.ATF;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class XLPEditorControl : UserControl
{
	private PropertyEditingListControl m_entryEditor;

	private PlatformSelectorControl m_platformSelector;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyEditor;

	private IContainer components;

	private TabPage tabEntries;

	private TabControl tabControl;

	private SplitContainer splitCtl;

	private SplitContainer mainSplitter;

	public XLPEditorControl()
	{
		InitializeComponent();
		m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisableSearchControls | PropertyGridMode.HideResetAllButton);
		m_propertyEditor.Dock = DockStyle.Fill;
		m_propertyEditor.PropertySorting = PropertySorting.Alphabetical;
		splitCtl.Panel1.Controls.Add(m_propertyEditor);
		m_platformSelector = new PlatformSelectorControl();
		m_platformSelector.Dock = DockStyle.Fill;
		splitCtl.Panel2.Controls.Add(m_platformSelector);
		m_entryEditor = new PropertyEditingListControl();
		m_entryEditor.Dock = DockStyle.Fill;
		tabEntries.Controls.Add(m_entryEditor);
		base.VisibleChanged += XLPEditorControl_VisibleChanged;
	}

	private void XLPEditorControl_VisibleChanged(object sender, EventArgs e)
	{
		SkinService.ApplyActiveSkin(m_propertyEditor);
		SkinService.ApplyActiveSkin(m_platformSelector);
		SkinService.ApplyActiveSkin(tabEntries);
	}

	public void Bind(IXLPEditorContext context)
	{
		m_propertyEditor.Bind(context.XLPContext);
		m_entryEditor.Bind(context.XLPEntriesContext);
		m_platformSelector.Bind(context.PlatformSelectorContext);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_platformSelector != null)
			{
				splitCtl.Panel2.Controls.Remove(m_platformSelector);
				m_platformSelector.Dispose();
				m_platformSelector = null;
			}
			if (m_propertyEditor != null)
			{
				splitCtl.Panel1.Controls.Remove(m_propertyEditor);
				m_propertyEditor.Dispose();
				m_propertyEditor = null;
			}
			if (m_entryEditor != null)
			{
				tabEntries.Controls.Remove(m_entryEditor);
				m_entryEditor.Dispose();
				m_entryEditor = null;
			}
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.tabEntries = new System.Windows.Forms.TabPage();
		this.tabControl = new System.Windows.Forms.TabControl();
		this.splitCtl = new System.Windows.Forms.SplitContainer();
		this.mainSplitter = new System.Windows.Forms.SplitContainer();
		this.tabControl.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitCtl).BeginInit();
		this.splitCtl.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.mainSplitter).BeginInit();
		this.mainSplitter.Panel1.SuspendLayout();
		this.mainSplitter.Panel2.SuspendLayout();
		this.mainSplitter.SuspendLayout();
		base.SuspendLayout();
		this.tabEntries.Location = new System.Drawing.Point(4, 22);
		this.tabEntries.Name = "tabEntries";
		this.tabEntries.Padding = new System.Windows.Forms.Padding(3);
		this.tabEntries.Size = new System.Drawing.Size(776, 379);
		this.tabEntries.TabIndex = 0;
		this.tabEntries.Text = "Entries";
		this.tabEntries.UseVisualStyleBackColor = true;
		this.tabControl.Controls.Add(this.tabEntries);
		this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabControl.Location = new System.Drawing.Point(0, 0);
		this.tabControl.Name = "tabControl";
		this.tabControl.SelectedIndex = 0;
		this.tabControl.Size = new System.Drawing.Size(784, 405);
		this.tabControl.TabIndex = 1;
		this.splitCtl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitCtl.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
		this.splitCtl.IsSplitterFixed = true;
		this.splitCtl.Location = new System.Drawing.Point(0, 0);
		this.splitCtl.Name = "splitCtl";
		this.splitCtl.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitCtl.Size = new System.Drawing.Size(784, 148);
		this.splitCtl.SplitterDistance = 90;
		this.splitCtl.TabIndex = 2;
		this.mainSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
		this.mainSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
		this.mainSplitter.IsSplitterFixed = true;
		this.mainSplitter.Location = new System.Drawing.Point(0, 0);
		this.mainSplitter.Name = "mainSplitter";
		this.mainSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.mainSplitter.Panel1.Controls.Add(this.splitCtl);
		this.mainSplitter.Panel2.Controls.Add(this.tabControl);
		this.mainSplitter.Size = new System.Drawing.Size(784, 557);
		this.mainSplitter.SplitterDistance = 148;
		this.mainSplitter.TabIndex = 3;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.mainSplitter);
		base.Name = "XLPEditorControl";
		base.Size = new System.Drawing.Size(784, 557);
		this.tabControl.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitCtl).EndInit();
		this.splitCtl.ResumeLayout(false);
		this.mainSplitter.Panel1.ResumeLayout(false);
		this.mainSplitter.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.mainSplitter).EndInit();
		this.mainSplitter.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
