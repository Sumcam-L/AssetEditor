using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class EnvironmentLightEditorControl : EntityEditorControlBase
{
	private EnvironmentLightDisplayControl m_lightDisplay;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyEditor;

	private IContainer components;

	private SplitContainer splitContainer1;

	public EnvironmentLightEditorControl()
	{
		InitializeComponent();
		m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.HideResetAllButton);
		m_propertyEditor.Dock = DockStyle.Fill;
		m_propertyEditor.PropertySorting = PropertySorting.Categorized;
		splitContainer1.Panel1.Controls.Add(m_propertyEditor);
		m_lightDisplay = new EnvironmentLightDisplayControl();
		m_lightDisplay.Dock = DockStyle.Fill;
		splitContainer1.Panel2.Controls.Add(m_lightDisplay);
	}

	public override void Bind(IEntityEditorContext context)
	{
		IEnvironmentLightEditingContext environmentLightEditingContext = context as IEnvironmentLightEditingContext;
		if (environmentLightEditingContext != null)
		{
			if (environmentLightEditingContext.EnvironmentLight.DataFiles.Any())
			{
				string dataFilePath = environmentLightEditingContext.EnvironmentLight.GetDataFilePath(environmentLightEditingContext.EnvironmentLight.DataFiles.First().RelativePath);
				environmentLightEditingContext.SetCubePath(dataFilePath);
			}
			if (!string.IsNullOrEmpty(environmentLightEditingContext.EnvironmentLight.SourceFilePath) && File.Exists(environmentLightEditingContext.EnvironmentLight.SourceFilePath))
			{
				environmentLightEditingContext.SetSourceFile(environmentLightEditingContext.EnvironmentLight.SourceFilePath);
				environmentLightEditingContext.OpenSourceFile(environmentLightEditingContext.EnvironmentLight.SourceFilePath);
			}
		}
		m_propertyEditor.Bind(environmentLightEditingContext);
		m_lightDisplay.Bind(environmentLightEditingContext);
	}

	public void RefreshUIState()
	{
		m_lightDisplay.RefreshUIStateFromContext();
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
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 0);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer1.Size = new System.Drawing.Size(442, 284);
		this.splitContainer1.SplitterDistance = 91;
		this.splitContainer1.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splitContainer1);
		base.Name = "EnvironmentLightEditorControl";
		base.Size = new System.Drawing.Size(442, 284);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
