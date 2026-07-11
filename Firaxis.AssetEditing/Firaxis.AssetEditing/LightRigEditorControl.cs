using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.ATF;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class LightRigEditorControl : EntityEditorControlBase
{
	private PropertyEditingListControl m_animationSetEditor;

	private PropertyEditingListControl m_analyticLightEditor;

	private ILightRigEditorContext m_context;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_cookParameterPropertyEditor;

	private CommandControl m_cookParameterSetEditor;

	private PropertyEditingListControl m_environmentLightEditor;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyEditor;

	private IContainer components;

	private SplitContainer splitContainer1;

	private Panel pnlProps;

	private TabPage tabCook;

	private TabControl tabComponents;

	private TabPage tabAnim;

	private TabPage tabAnalyticLights;

	private TabPage tabEnvironment;

	public LightRigEditorControl()
	{
		InitializeComponent();
		m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisableSearchControls | PropertyGridMode.HideResetAllButton);
		m_propertyEditor.Dock = DockStyle.Fill;
		m_propertyEditor.PropertySorting = PropertySorting.Categorized;
		pnlProps.Controls.Add(m_propertyEditor);
		m_cookParameterPropertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.HideResetAllButton);
		m_cookParameterSetEditor = new CommandControl();
		m_cookParameterSetEditor.Dock = DockStyle.Fill;
		m_cookParameterPropertyEditor.Dock = DockStyle.Fill;
		m_cookParameterSetEditor.ChildControls.Add(m_cookParameterPropertyEditor);
		tabCook.Controls.Add(m_cookParameterSetEditor);
		m_analyticLightEditor = new PropertyEditingListControl();
		m_analyticLightEditor.Dock = DockStyle.Fill;
		tabAnalyticLights.Controls.Add(m_analyticLightEditor);
		m_animationSetEditor = new PropertyEditingListControl();
		m_animationSetEditor.Dock = DockStyle.Fill;
		tabAnim.Controls.Add(m_animationSetEditor);
		m_environmentLightEditor = new PropertyEditingListControl();
		m_environmentLightEditor.Dock = DockStyle.Fill;
		tabEnvironment.Controls.Add(m_environmentLightEditor);
	}

	public override void Bind(IEntityEditorContext context)
	{
		if (m_context != null)
		{
			m_context.Reloaded -= AssetContext_Reloaded;
			m_context = null;
		}
		m_context = (ILightRigEditorContext)context;
		m_propertyEditor.Bind(m_context?.EntityContext);
		BindCookParameters();
		m_analyticLightEditor.Bind(m_context?.AnalyticLightContext);
		m_animationSetEditor.Bind(m_context?.AnimationSetContext);
		m_environmentLightEditor.Bind(m_context?.EnvironmentLightContext);
		if (m_context != null)
		{
			m_context.Reloaded += AssetContext_Reloaded;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_context != null)
			{
				m_context.Reloaded -= AssetContext_Reloaded;
				m_context = null;
			}
			if (m_propertyEditor != null)
			{
				pnlProps.Controls.Remove(m_propertyEditor);
				m_propertyEditor.Dispose();
				m_propertyEditor = null;
			}
			if (m_cookParameterPropertyEditor != null)
			{
				m_cookParameterSetEditor.Controls.Remove(m_cookParameterPropertyEditor);
				m_cookParameterPropertyEditor.Dispose();
				m_cookParameterPropertyEditor = null;
			}
			if (m_cookParameterSetEditor != null)
			{
				tabCook.Controls.Remove(m_cookParameterSetEditor);
				m_cookParameterSetEditor.Dispose();
				m_cookParameterSetEditor = null;
			}
			if (m_analyticLightEditor != null)
			{
				tabAnalyticLights.Controls.Remove(m_analyticLightEditor);
				m_analyticLightEditor.Dispose();
				m_analyticLightEditor = null;
			}
			if (m_animationSetEditor != null)
			{
				tabAnim.Controls.Remove(m_animationSetEditor);
				m_animationSetEditor.Dispose();
				m_animationSetEditor = null;
			}
			if (m_environmentLightEditor != null)
			{
				tabEnvironment.Controls.Remove(m_environmentLightEditor);
				m_environmentLightEditor.Dispose();
				m_environmentLightEditor = null;
			}
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void AssetContext_Reloaded(object sender, EventArgs e)
	{
		BindCookParameters();
	}

	private void BindCookParameters()
	{
		if (m_context != null && m_context.HasCookParameters)
		{
			if (!tabComponents.Controls.Contains(tabCook))
			{
				tabComponents.Controls.Add(tabCook);
				tabComponents.Controls.SetChildIndex(tabCook, 0);
			}
			m_cookParameterSetEditor.Bind(m_context.CookParametersContext);
			m_cookParameterPropertyEditor.Bind(m_context.CookParametersContext);
		}
		else if (tabComponents.Controls.Contains(tabCook))
		{
			tabComponents.Controls.Remove(tabCook);
		}
	}

	private void InitializeComponent()
	{
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.pnlProps = new System.Windows.Forms.Panel();
		this.tabCook = new System.Windows.Forms.TabPage();
		this.tabEnvironment = new System.Windows.Forms.TabPage();
		this.tabAnalyticLights = new System.Windows.Forms.TabPage();
		this.tabAnim = new System.Windows.Forms.TabPage();
		this.tabComponents = new System.Windows.Forms.TabControl();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		this.tabComponents.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 0);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer1.Panel1.Controls.Add(this.pnlProps);
		this.splitContainer1.Panel2.Controls.Add(this.tabComponents);
		this.splitContainer1.Size = new System.Drawing.Size(479, 437);
		this.splitContainer1.SplitterDistance = 170;
		this.splitContainer1.TabIndex = 3;
		this.pnlProps.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlProps.Location = new System.Drawing.Point(0, 0);
		this.pnlProps.Name = "pnlProps";
		this.pnlProps.Size = new System.Drawing.Size(479, 170);
		this.pnlProps.TabIndex = 2;
		this.tabCook.Location = new System.Drawing.Point(4, 22);
		this.tabCook.Name = "tabCook";
		this.tabCook.Padding = new System.Windows.Forms.Padding(3);
		this.tabCook.Size = new System.Drawing.Size(471, 237);
		this.tabCook.TabIndex = 0;
		this.tabCook.Text = "Cook Parameters";
		this.tabCook.UseVisualStyleBackColor = true;
		this.tabEnvironment.Location = new System.Drawing.Point(4, 22);
		this.tabEnvironment.Name = "tabEnvironment";
		this.tabEnvironment.Size = new System.Drawing.Size(471, 237);
		this.tabEnvironment.TabIndex = 4;
		this.tabEnvironment.Text = "Environment Lights";
		this.tabEnvironment.UseVisualStyleBackColor = true;
		this.tabAnalyticLights.Location = new System.Drawing.Point(4, 22);
		this.tabAnalyticLights.Name = "tabAnalyticLights";
		this.tabAnalyticLights.Size = new System.Drawing.Size(471, 237);
		this.tabAnalyticLights.TabIndex = 3;
		this.tabAnalyticLights.Text = "Analytic Lights";
		this.tabAnalyticLights.UseVisualStyleBackColor = true;
		this.tabAnim.Location = new System.Drawing.Point(4, 22);
		this.tabAnim.Name = "tabAnim";
		this.tabAnim.Size = new System.Drawing.Size(471, 237);
		this.tabAnim.TabIndex = 2;
		this.tabAnim.Text = "Animation Bindings";
		this.tabAnim.UseVisualStyleBackColor = true;
		this.tabComponents.Controls.Add(this.tabAnim);
		this.tabComponents.Controls.Add(this.tabAnalyticLights);
		this.tabComponents.Controls.Add(this.tabEnvironment);
		this.tabComponents.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabComponents.Location = new System.Drawing.Point(0, 0);
		this.tabComponents.Name = "tabComponents";
		this.tabComponents.SelectedIndex = 0;
		this.tabComponents.Size = new System.Drawing.Size(479, 263);
		this.tabComponents.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splitContainer1);
		base.Name = "LightRigEditorControl";
		base.Size = new System.Drawing.Size(479, 437);
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.tabComponents.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
