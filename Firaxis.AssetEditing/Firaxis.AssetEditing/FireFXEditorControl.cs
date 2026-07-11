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

public class FireFXEditorControl : EntityEditorControlBase
{
	private struct SubControlInfo
	{
		public string Label;

		public string Icon;

		public Control Ctl;

		public DockState DockState;
	}

	private IFireFXEditorContext m_context;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyEditor;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_cookParameterPropertyEditor;

	private CommandControl m_cookParameterSetEditor;

	private FireFXScriptControl m_fireFXScriptEditor;

	private DockPanel m_dockPanel;

	private IThemeService m_themeService;

	private IDictionary<Control, Firaxis.ATF.DockContent> m_dockContent;

	private IContainer components;

	public string EditorLayoutState
	{
		get
		{
			return GetFireFXEditorLayoutState();
		}
		set
		{
			SetFireFXEditorLayoutState(value);
		}
	}

	public FireFXEditorControl(string layoutState, IThemeService themeSvc)
	{
		InitializeComponent();
		m_themeService = themeSvc;
		m_themeService.ThemeChanged += ThemeService_ThemeChanged;
		m_dockContent = new Dictionary<Control, Firaxis.ATF.DockContent>();
		m_dockPanel = new DockPanel();
		m_dockPanel.Theme = m_themeService.ActiveTheme;
		m_dockPanel.Dock = DockStyle.Fill;
		m_dockPanel.ShowDocumentIcon = true;
		m_dockPanel.LargeDocumentIcon = false;
		m_dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
		base.Controls.Add(m_dockPanel);
		m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisableSearchControls | PropertyGridMode.HideResetAllButton);
		m_propertyEditor.Dock = DockStyle.Fill;
		m_propertyEditor.PropertySorting = PropertySorting.None;
		AddDockContext(m_propertyEditor, "Properties", string.Empty, DockState.DockTop);
		m_cookParameterPropertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisplayDescriptions | PropertyGridMode.HideResetAllButton);
		m_cookParameterPropertyEditor.PropertyGridView.ShowRowStriping = false;
		m_cookParameterSetEditor = new CommandControl();
		m_cookParameterSetEditor.Dock = DockStyle.Fill;
		m_cookParameterPropertyEditor.Dock = DockStyle.Fill;
		m_cookParameterSetEditor.ChildControls.Add(m_cookParameterPropertyEditor);
		AddDockContext(m_cookParameterSetEditor, "Cook Params", Resources.CookParametersCategoryIcon, DockState.Document);
		m_fireFXScriptEditor = new FireFXScriptControl();
		m_fireFXScriptEditor.Dock = DockStyle.Fill;
		AddDockContext(m_fireFXScriptEditor, "Script", Resources.BaseCategoryIcon, DockState.Document);
		EditorLayoutState = layoutState;
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

	private IDockContent StringToDockContent(string id)
	{
		foreach (Firaxis.ATF.DockContent value in m_dockContent.Values)
		{
			if (value.Name == id)
			{
				return value;
			}
		}
		return null;
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

	private void SetFireFXEditorLayoutState(string value)
	{
		m_dockPanel.SetLayoutState(value, m_dockContent);
	}

	private string GetFireFXEditorLayoutState()
	{
		return m_dockPanel.GetLayoutState();
	}

	public override void Bind(IEntityEditorContext context)
	{
		if (m_context != null)
		{
			m_context.Reloaded -= FireFXContext_Reloaded;
			m_context = null;
		}
		m_context = (IFireFXEditorContext)context;
		m_propertyEditor.Bind(m_context?.EntityContext);
		BindCookParameterEditor();
		BindFireFXScriptEditor();
		if (m_context != null)
		{
			m_context.Reloaded += FireFXContext_Reloaded;
		}
	}

	private void FireFXContext_Reloaded(object sender, EventArgs e)
	{
		BindCookParameterEditor();
		BindFireFXScriptEditor();
	}

	private void BindCookParameterEditor()
	{
		if (m_context != null && m_context.HasCookParameters)
		{
			m_dockContent[m_cookParameterSetEditor].DockState = DockState.Document;
			m_cookParameterSetEditor.Bind(m_context.CookParametersContext);
			m_cookParameterPropertyEditor.Bind(m_context.CookParametersContext);
		}
		else
		{
			m_dockContent[m_cookParameterSetEditor].DockState = DockState.Hidden;
		}
	}

	private void BindFireFXScriptEditor()
	{
		if (m_context != null && m_context.HasScript)
		{
			m_dockContent[m_fireFXScriptEditor].DockState = DockState.Document;
			m_fireFXScriptEditor.Bind(m_context.ScriptResource);
			m_dockContent[m_fireFXScriptEditor].Activate();
		}
		else
		{
			m_dockContent[m_fireFXScriptEditor].DockState = DockState.Hidden;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_context != null)
			{
				m_context.Reloaded -= FireFXContext_Reloaded;
				m_context = null;
			}
			foreach (KeyValuePair<Control, Firaxis.ATF.DockContent> item in m_dockContent)
			{
				item.Value.Controls.Remove(item.Key);
				item.Value.Dispose();
				item.Key.Dispose();
			}
			m_dockContent.Clear();
			if (m_dockPanel != null)
			{
				m_dockPanel.Dispose();
				m_dockPanel = null;
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
	}
}
