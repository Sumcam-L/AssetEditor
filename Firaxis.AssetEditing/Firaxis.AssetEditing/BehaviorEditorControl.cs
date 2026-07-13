using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.AssetEditing;

public class BehaviorEditorControl : EntityEditorControlBase
{
	private struct SubControlInfo
	{
		public string Label;

		public string Icon;

		public Control Ctl;

		public DockState DockState;
	}

	private IBehaviorEditorContext m_context;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyEditor;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_cookParameterPropertyEditor;

	private CommandControl m_cookParameterSetEditor;

	private PropertyEditingListControl m_geometryReferenceSetEditor;

	private PropertyEditingListControl m_attachmentEditor;

	private PropertyEditingListControl m_animationSetEditor;

	private PropertyEditingListControl m_behaviorSetEditor;

	private IDictionary<Control, Firaxis.ATF.DockContent> m_dockContent;

	private DockPanel m_dockPanel;

	private IThemeService m_themeService;

	private IContainer components;

	public BehaviorEditorControl(IThemeService themeSvc)
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
		m_dockPanel.ActiveContentChanged += InnerDockPanel_ActiveContentChanged;
		base.Controls.Add(m_dockPanel);
		m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisableSearchControls | PropertyGridMode.HideResetAllButton);
		m_propertyEditor.Dock = DockStyle.Fill;
		m_propertyEditor.PropertySorting = PropertySorting.None;
		AddDockContext(m_propertyEditor, "Properties", string.Empty, DockState.DockTop);
		m_cookParameterPropertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisplayDescriptions | PropertyGridMode.HideResetAllButton);
		m_cookParameterPropertyEditor.BuildPropertiesWhenHidden = true;
		m_cookParameterPropertyEditor.PropertyGridView.ShowRowStriping = false;
		m_cookParameterSetEditor = new CommandControl();
		m_cookParameterSetEditor.Dock = DockStyle.Fill;
		m_cookParameterPropertyEditor.Dock = DockStyle.Fill;
		m_cookParameterSetEditor.ChildControls.Add(m_cookParameterPropertyEditor);
		AddDockContext(m_cookParameterSetEditor, "Cook Params", Resources.CookParametersCategoryIcon, DockState.Document);
		m_geometryReferenceSetEditor = new PropertyEditingListControl(PropertySorting.None);
		m_geometryReferenceSetEditor.Dock = DockStyle.Fill;
		AddDockContext(m_geometryReferenceSetEditor, "Geometries", Resources.GeometryCategoryIcon, DockState.Document);
		m_attachmentEditor = new PropertyEditingListControl(PropertyCategorySettings.HideText, PropertySorting.Categorized);
		m_attachmentEditor.Dock = DockStyle.Fill;
		AddDockContext(m_attachmentEditor, "Attachments", Resources.AttachmentsCategoryIcon, DockState.Document);
		m_animationSetEditor = new PropertyEditingListControl(PropertySorting.None);
		m_animationSetEditor.Dock = DockStyle.Fill;
		AddDockContext(m_animationSetEditor, "Animations", Resources.AnimationsCategoryIcon, DockState.Document);
		m_behaviorSetEditor = new PropertyEditingListControl(PropertySorting.None);
		m_behaviorSetEditor.Dock = DockStyle.Fill;
		AddDockContext(m_behaviorSetEditor, "Behaviors", Resources.BehaviorCategoryIcon, DockState.Document);
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
		m_dockContent[key].VisibleChanged += InnerDockContent_VisibleChanged;
		m_dockContent[key].Show(m_dockPanel, state);
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		PaintTimingLog.Write("EntityEditorUI: BehaviorEditorControl VisibleChanged visible={0}, activeInner={1}", Visible, GetActiveInnerContentName());
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		PaintTimingLog.Write("EntityEditorUI: BehaviorEditorControl PaintBackground visible={0}, activeInner={1}", Visible, GetActiveInnerContentName());
		base.OnPaintBackground(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PaintTimingLog.Write("EntityEditorUI: BehaviorEditorControl Paint visible={0}, activeInner={1}", Visible, GetActiveInnerContentName());
		base.OnPaint(e);
	}

	private void InnerDockPanel_ActiveContentChanged(object sender, EventArgs e)
	{
		PaintTimingLog.Write("EntityEditorUI: BehaviorEditorControl InnerActiveContentChanged activeInner={0}", GetActiveInnerContentName());
	}

	private void InnerDockContent_VisibleChanged(object sender, EventArgs e)
	{
		Firaxis.ATF.DockContent dockContent = sender as Firaxis.ATF.DockContent;
		PaintTimingLog.Write("EntityEditorUI: BehaviorEditorControl InnerDockContentVisible name={0}, visible={1}, activeInner={2}", dockContent?.Name ?? "null", dockContent?.Visible ?? false, GetActiveInnerContentName());
	}

	private string GetActiveInnerContentName()
	{
		return (m_dockPanel?.ActiveContent as Firaxis.ATF.DockContent)?.Name ?? "null";
	}

	public override void Bind(IEntityEditorContext context)
	{
		if (m_context != null)
		{
			m_context.Reloaded -= ContextReloaded;
			m_context = null;
		}
		m_context = (IBehaviorEditorContext)context;
		m_propertyEditor.Bind(m_context?.EntityContext);
		m_attachmentEditor.Bind(m_context?.AttachmentsContext);
		BindCookParameters();
		BindGeometryReferenceSet();
		BindAnimationSet();
		BindBehaviorSet();
		if (m_context != null)
		{
			m_context.Reloaded += ContextReloaded;
		}
	}

	private void BindCookParameters()
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

	private void BindGeometryReferenceSet()
	{
		if (m_context != null && m_context.HasGeometryReferences)
		{
			m_dockContent[m_geometryReferenceSetEditor].DockState = DockState.Document;
			m_geometryReferenceSetEditor.Bind(m_context.GeometryReferenceSetContext);
		}
		else
		{
			m_dockContent[m_geometryReferenceSetEditor].DockState = DockState.Hidden;
		}
	}

	private void BindAnimationSet()
	{
		if (m_context != null && m_context.HasAnimations)
		{
			m_dockContent[m_animationSetEditor].DockState = DockState.Document;
			m_animationSetEditor.Bind(m_context.AnimationSetContext);
		}
		else
		{
			m_dockContent[m_animationSetEditor].DockState = DockState.Hidden;
		}
	}

	private void BindBehaviorSet()
	{
		if (m_context != null && m_context.HasBehaviors)
		{
			m_dockContent[m_behaviorSetEditor].DockState = DockState.Document;
			m_behaviorSetEditor.Bind(m_context.BehaviorSetContext);
		}
		else
		{
			m_dockContent[m_behaviorSetEditor].DockState = DockState.Hidden;
		}
	}

	private void ContextReloaded(object sender, EventArgs e)
	{
		BindCookParameters();
		BindGeometryReferenceSet();
		BindAnimationSet();
		BindBehaviorSet();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_context != null)
			{
				m_context.Reloaded -= ContextReloaded;
				m_context = null;
			}
			foreach (KeyValuePair<Control, Firaxis.ATF.DockContent> item in m_dockContent)
			{
				item.Value.VisibleChanged -= InnerDockContent_VisibleChanged;
				item.Value.Hide();
				item.Value.DockState = DockState.Unknown;
				item.Value.Controls.Remove(item.Key);
				item.Value.Dispose();
				item.Key.Dispose();
			}
			m_dockContent.Clear();
			if (m_dockPanel != null)
			{
				m_dockPanel.ActiveContentChanged -= InnerDockPanel_ActiveContentChanged;
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
		this.components = new System.ComponentModel.Container();
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	}
}
