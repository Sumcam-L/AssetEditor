using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.AssetEditing;

public class EntityEditorControl : EntityEditorControlBase
{
	private struct SubControlInfo
	{
		public string Label;

		public string Icon;

		public Control Ctl;

		public DockState DockState;
	}

	private IThemeService m_themeService;

	private DockPanel m_dockPanel;

	private IDictionary<Control, Firaxis.ATF.DockContent> m_dockContent;

	private IEntityEditorContext m_context;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_cookParameterPropertyEditor;

	private CommandControl m_cookParameterSetEditor;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyEditor;

	private IContainer components;

	public EntityEditorControl(IThemeService themeSvc)
	{
		InitializeComponent();
		m_themeService = themeSvc;
		m_themeService.ThemeChanged += ThemeService_ThemeChanged;
		m_dockContent = new Dictionary<Control, Firaxis.ATF.DockContent>();
		m_dockPanel = new DockPanel();
		m_dockPanel.Theme = m_themeService.ActiveTheme;
		m_dockPanel.Dock = DockStyle.Fill;
		m_dockPanel.ShowDocumentIcon = true;
		m_dockPanel.LargeDocumentIcon = true;
		m_dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
		base.Controls.Add(m_dockPanel);
		m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.HideResetAllButton);
		m_propertyEditor.Dock = DockStyle.Fill;
		m_propertyEditor.PropertySorting = PropertySorting.Categorized;
		AddDockContext(m_propertyEditor, "Properties", string.Empty, DockState.DockTop);
		m_cookParameterPropertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisplayDescriptions | PropertyGridMode.HideResetAllButton);
		m_cookParameterPropertyEditor.BuildPropertiesWhenHidden = true;
		m_cookParameterPropertyEditor.PropertyGridView.ShowRowStriping = false;
		m_cookParameterSetEditor = new CommandControl();
		m_cookParameterSetEditor.Dock = DockStyle.Fill;
		m_cookParameterPropertyEditor.Dock = DockStyle.Fill;
		m_cookParameterSetEditor.ChildControls.Add(m_cookParameterPropertyEditor);
		AddDockContext(m_cookParameterSetEditor, "Cook Params", Resources.CookParametersCategoryIcon, DockState.Document);
	}

	private IEnumerable<SubControlInfo> DetachControlInfos()
	{
		IList<SubControlInfo> list = new List<SubControlInfo>();
		KeyValuePair<Control, Firaxis.ATF.DockContent>[] array = m_dockContent.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<Control, Firaxis.ATF.DockContent> keyValuePair = array[i];
			list.Add((SubControlInfo)keyValuePair.Key.Tag);
			keyValuePair.Value.Controls.Remove(keyValuePair.Key);
			keyValuePair.Value.Hide();
			keyValuePair.Value.Dispose();
		}
		m_dockContent.Clear();
		return list;
	}

	private void ReattachControlInfos(IEnumerable<SubControlInfo> infos)
	{
		foreach (SubControlInfo info in infos)
		{
			AddDockContext(info.Ctl, info.Label, info.Icon, info.DockState);
		}
	}

	private void ThemeService_ThemeChanged(object sender, EventArgs e)
	{
		IEnumerable<SubControlInfo> infos = DetachControlInfos();
		m_dockPanel.Theme = m_themeService.ActiveTheme;
		ReattachControlInfos(infos);
	}

	private Icon GetComponentIcon(string iconName)
	{
		if (string.IsNullOrEmpty(iconName))
		{
			return null;
		}
		return ResourceUtil.GetIcon(iconName);
	}

	private void AddDockContext(Control key, string text, string iconName, DockState state)
	{
		key.Tag = new SubControlInfo
		{
			Ctl = key,
			Label = text,
			Icon = iconName,
			DockState = state
		};
		m_dockContent[key] = new Firaxis.ATF.DockContent();
		m_dockContent[key].Name = text;
		m_dockContent[key].Text = text;
		m_dockContent[key].ToolTipText = text;
		m_dockContent[key].Controls.Add(key);
		m_dockContent[key].Icon = GetComponentIcon(iconName);
		m_dockContent[key].ShowIcon = true;
		m_dockContent[key].ShowTabText = false;
		m_dockContent[key].CloseButtonVisible = false;
		m_dockContent[key].CloseButton = false;
		m_dockContent[key].Show(m_dockPanel, state);
	}

	public override void Bind(IEntityEditorContext context)
	{
		if (m_context != null)
		{
			m_context.Reloaded -= Context_Reloaded;
			m_context = null;
		}
		m_context = context;
		if (m_context != null)
		{
			m_propertyEditor.Bind(m_context.EntityContext);
			BindCookParameters();
			m_context.Reloaded += Context_Reloaded;
		}
		else
		{
			m_propertyEditor.Bind(null);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_context != null)
			{
				m_context.Reloaded -= Context_Reloaded;
				m_context = null;
			}
			if (m_propertyEditor != null)
			{
				m_propertyEditor.Dispose();
				m_propertyEditor = null;
			}
			if (m_cookParameterPropertyEditor != null)
			{
				m_cookParameterSetEditor.Controls.Remove(m_cookParameterPropertyEditor);
				m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyChanged -= PropertyGridView_SelectedPropertyChanged;
				m_cookParameterPropertyEditor.Dispose();
				m_cookParameterPropertyEditor = null;
			}
			if (m_cookParameterSetEditor != null)
			{
				m_cookParameterSetEditor.Dispose();
				m_cookParameterSetEditor = null;
			}
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void BindCookParameters()
	{
		if (m_context != null && m_context.HasCookParameters)
		{
			m_dockContent[m_propertyEditor].DockState = DockState.DockTop;
			m_dockContent[m_propertyEditor].ShowIcon = true;
			m_dockContent[m_propertyEditor].ShowTabText = false;
			m_dockPanel.ShowDocumentIcon = true;
			m_dockPanel.LargeDocumentIcon = true;
			m_dockPanel.DockTopPortion = 0.5;
			m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyChanged -= PropertyGridView_SelectedPropertyChanged;
			m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyChanged += PropertyGridView_SelectedPropertyChanged;
			m_dockContent[m_cookParameterSetEditor].DockState = DockState.Document;
			m_cookParameterSetEditor.Bind(m_context.CookParametersContext);
			m_cookParameterPropertyEditor.Bind(m_context.CookParametersContext);
		}
		else
		{
			m_dockContent[m_propertyEditor].DockState = DockState.Document;
			m_dockContent[m_propertyEditor].ShowIcon = false;
			m_dockContent[m_propertyEditor].ShowTabText = true;
			m_dockPanel.ShowDocumentIcon = false;
			m_dockPanel.LargeDocumentIcon = false;
			m_dockContent[m_cookParameterSetEditor].DockState = DockState.Hidden;
		}
	}

	private void PropertyGridView_SelectedPropertyChanged(object sender, EventArgs e)
	{
	}

	private void Context_Reloaded(object sender, EventArgs e)
	{
		BindCookParameters();
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Name = "EntityEditorControl";
		base.Size = new System.Drawing.Size(479, 437);
		base.ResumeLayout(false);
	}
}
