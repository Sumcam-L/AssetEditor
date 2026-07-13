using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml;
using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.AssetEditing;

public class AssetEditorControl : EntityEditorControlBase, IControlHostPreShowClient, IControlHostUnregisteringClient
{
	private struct SubControlInfo
	{
		public string Label;

		public string Icon;

		public Control Ctl;

		public DockState DockState;
	}

	private IAssetEditorContext m_context;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyEditor;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_cookParameterPropertyEditor;

	private CommandControl m_cookParameterSetEditor;

	private ModelInstanceStateEditor m_geometrySetEditor;

	private PropertyEditingListControl m_attachmentEditor;

	private PropertyEditingListControl m_animationSetEditor;

	private PropertyEditingListControl m_particleEffectSetEditor;

	private PropertyEditingListControl m_behaviorSetEditor;

	private PropertyEditingListControl m_splineSetEditor;

	private DockPanel m_dockPanel;

	private IThemeService m_themeService;

	private IDictionary<Control, Firaxis.ATF.DockContent> m_dockContent;

	private IContainer components;

	private string m_appliedEditorLayoutState = string.Empty;

	private readonly string m_savedAssetEditorLayoutState;

	private readonly string m_initialEntityClassName;

	private readonly string m_initialEditorLayoutState;

	private bool m_ensureActiveInnerContentPending;

	private bool m_ensureActiveInnerContentWhenHandleCreated;

	private bool m_initialClassLayoutApplied;

	private bool m_disposing;

	private bool m_controlHostUnregistering;

	private Firaxis.ATF.DockContent m_lastActiveInnerContent;

	private string TracePrefix => "EntityEditorUI: AssetEditorControl#" + RuntimeHelpers.GetHashCode(this).ToString("X");

	public string EditorLayoutState
	{
		get
		{
			return GetAssetEditorLayoutState();
		}
		set
		{
			SetAssetEditorLayoutState(value);
		}
	}

	public AssetEditorControl(string layoutState, IThemeService themeSvc)
		: this(layoutState, themeSvc, null)
	{
	}

	public AssetEditorControl(string layoutState, IThemeService themeSvc, string entityClassName)
	{
		InitializeComponent();
		PaintTimingLog.Write("{0} constructed class={1}", TracePrefix, entityClassName ?? "null");
		m_savedAssetEditorLayoutState = layoutState;
		m_initialEntityClassName = entityClassName;
		if (!string.IsNullOrEmpty(m_initialEntityClassName) && !string.IsNullOrEmpty(m_savedAssetEditorLayoutState))
		{
			TryGetSavedClassLayout(m_initialEntityClassName, out m_initialEditorLayoutState);
		}
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
		m_dockPanel.SuspendLayout(allWindows: true);
		try
		{
			m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisableSearchControls | PropertyGridMode.HideResetAllButton);
			m_propertyEditor.Dock = DockStyle.Fill;
			m_propertyEditor.PropertySorting = PropertySorting.None;
			AddDockContext(m_propertyEditor, "Properties", string.Empty, DockState.DockTop, show: true, activate: false);
			m_cookParameterPropertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisplayDescriptions | PropertyGridMode.HideResetAllButton);
			m_cookParameterPropertyEditor.BuildPropertiesWhenHidden = true;
			m_cookParameterPropertyEditor.PropertyGridView.ShowRowStriping = false;
			m_cookParameterSetEditor = new CommandControl();
			m_cookParameterSetEditor.Dock = DockStyle.Fill;
			m_cookParameterPropertyEditor.Dock = DockStyle.Fill;
			m_cookParameterSetEditor.ChildControls.Add(m_cookParameterPropertyEditor);
			AddDockContext(m_cookParameterSetEditor, "Cook Params", Resources.CookParametersCategoryIcon, DockState.Document, show: true, activate: false);
			m_geometrySetEditor = new ModelInstanceStateEditor();
			m_geometrySetEditor.Dock = DockStyle.Fill;
			AddDockContext(m_geometrySetEditor, "Geometries", Resources.GeometryCategoryIcon, DockState.Document, show: true, activate: false);
			m_attachmentEditor = new PropertyEditingListControl(PropertyCategorySettings.HideText, PropertySorting.Categorized);
			m_attachmentEditor.Dock = DockStyle.Fill;
			AddDockContext(m_attachmentEditor, "Attachments", Resources.AttachmentsCategoryIcon, DockState.Document, show: true, activate: false);
			m_animationSetEditor = new PropertyEditingListControl(PropertySorting.None);
			m_animationSetEditor.Dock = DockStyle.Fill;
			AddDockContext(m_animationSetEditor, "Animations", Resources.AnimationsCategoryIcon, DockState.Document, show: true, activate: false);
			m_particleEffectSetEditor = new PropertyEditingListControl(PropertySorting.None);
			m_particleEffectSetEditor.Dock = DockStyle.Fill;
			AddDockContext(m_particleEffectSetEditor, "Particles", Resources.ParticlesCategoryIcon, DockState.Document, show: true, activate: false);
			m_behaviorSetEditor = new PropertyEditingListControl(PropertySorting.None);
			m_behaviorSetEditor.Dock = DockStyle.Fill;
			AddDockContext(m_behaviorSetEditor, "Behaviors", Resources.BehaviorCategoryIcon, DockState.Document, show: true, activate: false);
			m_splineSetEditor = new PropertyEditingListControl(PropertyCategorySettings.HideText, PropertySorting.Categorized);
			m_splineSetEditor.Dock = DockStyle.Fill;
			AddDockContext(m_splineSetEditor, "Splines", Resources.SplineCategoryIcon, DockState.Document, show: true, activate: false);
		}
		finally
		{
			m_dockPanel.ResumeLayout(performLayout: true, allWindows: true);
		}
	}

	public void BeforeControlHostShow()
	{
		if (m_disposing || IsDisposed)
		{
			return;
		}
		ApplyInitialClassLayout();
		EnsureActiveInnerContent();
	}

	public void BeforeControlHostUnregister()
	{
		m_controlHostUnregistering = true;
		PaintTimingLog.Write("{0} before control host unregister", TracePrefix);
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
			AddDockContext(info.Ctl, info.Label, info.Icon, info.DockState, show: true);
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
		AddDockContext(key, text, iconName, state, show: true);
	}

	private void AddDockContext(Control key, string text, string iconName, DockState state, bool show, bool activate = true)
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
		if (show)
		{
			m_dockContent[key].Show(m_dockPanel, state, activate);
		}
	}

	public bool IsEditorLayoutStateApplied(string value)
	{
		return !string.IsNullOrEmpty(value) && (m_appliedEditorLayoutState == value || GetAssetEditorLayoutState() == value);
	}

	private void ApplyInitialClassLayout()
	{
		if (m_initialClassLayoutApplied)
		{
			return;
		}
		PaintTimingLog.Write("AssetEditorControl: skip initial saved inner layout class=" + (m_initialEntityClassName ?? "null"));
		m_initialClassLayoutApplied = true;
	}

	private bool TryGetSavedClassLayout(string className, out string layout)
	{
		layout = null;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(m_savedAssetEditorLayoutState);
		foreach (XmlNode item in xmlDocument.DocumentElement.GetElementsByTagName("layout"))
		{
			if (item.Attributes["entityclass"]?.Value == className)
			{
				layout = item.InnerXml;
				return true;
			}
		}
		return false;
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (m_disposing || IsDisposed)
		{
			return;
		}
		PaintTimingLog.Write("{0} VisibleChanged visible={1}, {2}", TracePrefix, Visible, GetInnerDockState());
		if (Visible)
		{
			ApplyInitialClassLayout();
			EnsureActiveInnerContent();
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		PaintTimingLog.Write("{0} HandleCreated pending={1}, {2}", TracePrefix, m_ensureActiveInnerContentWhenHandleCreated, GetInnerDockState());
		if (m_ensureActiveInnerContentWhenHandleCreated)
		{
			m_ensureActiveInnerContentWhenHandleCreated = false;
			ScheduleEnsureActiveInnerContent();
		}
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		PaintTimingLog.Write("{0} PaintBackground visible={1}, {2}", TracePrefix, Visible, GetInnerDockState());
		base.OnPaintBackground(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PaintTimingLog.Write("{0} Paint visible={1}, {2}", TracePrefix, Visible, GetInnerDockState());
		base.OnPaint(e);
	}

	private void InnerDockPanel_ActiveContentChanged(object sender, EventArgs e)
	{
		PaintTimingLog.Write("{0} InnerActiveContentChanged {1}", TracePrefix, GetInnerDockState());
		if (HasValidActiveInnerContent())
		{
			m_lastActiveInnerContent = (Firaxis.ATF.DockContent)m_dockPanel.ActiveContent;
		}
		if (Visible && !HasValidActiveInnerContent())
		{
			ScheduleEnsureActiveInnerContent();
		}
	}

	private void InnerDockContent_VisibleChanged(object sender, EventArgs e)
	{
		Firaxis.ATF.DockContent dockContent = sender as Firaxis.ATF.DockContent;
		PaintTimingLog.Write("{0} InnerDockContentVisible name={1}, visible={2}, {3}", TracePrefix, dockContent?.Name ?? "null", dockContent?.Visible ?? false, GetInnerDockState());
		if (dockContent != null && dockContent.Visible && dockContent.DockState == DockState.Document && dockContent.DockHandler.Pane?.ActiveContent == dockContent)
		{
			m_lastActiveInnerContent = dockContent;
		}
	}

	private string GetActiveInnerContentName()
	{
		return (m_dockPanel?.ActiveContent as Firaxis.ATF.DockContent)?.Name ?? "null";
	}

	private string GetInnerDockState()
	{
		string activePane = FormatPane(m_dockPanel?.ActivePane);
		string activeDocumentPane = FormatPane(m_dockPanel?.ActiveDocumentPane);
		string focusedType = ContainsFocus ? FindFocusedControl(this)?.GetType().Name ?? "unknown" : "outside";
		return string.Format("activeInner={0}, containsFocus={1}, focused={2}, activePane={3}, activeDocumentPane={4}", GetActiveInnerContentName(), ContainsFocus, focusedType, activePane, activeDocumentPane);
	}

	private static string FormatPane(DockPane pane)
	{
		if (pane == null)
		{
			return "null";
		}
		return string.Format("{0:X}:{1}:active={2}:displaying={3}", RuntimeHelpers.GetHashCode(pane), pane.DockState, (pane.ActiveContent as Firaxis.ATF.DockContent)?.Name ?? "null", pane.DisplayingContents.Count);
	}

	private static Control FindFocusedControl(Control control)
	{
		ContainerControl container = control as ContainerControl;
		while (container?.ActiveControl != null)
		{
			control = container.ActiveControl;
			container = control as ContainerControl;
		}
		return control.Focused ? control : null;
	}

	private bool HasValidActiveInnerContent()
	{
		return m_dockPanel?.ActiveContent is Firaxis.ATF.DockContent activeContent && activeContent.DockState != DockState.Hidden && activeContent.DockState != DockState.Unknown && activeContent.DockHandler.Pane?.ActiveContent == activeContent;
	}

	private void ScheduleEnsureActiveInnerContent()
	{
		if (m_ensureActiveInnerContentPending || IsDisposed || m_dockPanel == null || m_dockPanel.IsDisposed)
		{
			return;
		}
		if (!IsHandleCreated)
		{
			m_ensureActiveInnerContentWhenHandleCreated = true;
			PaintTimingLog.Write("{0} schedule ensure deferred until handle created", TracePrefix);
			return;
		}
		m_ensureActiveInnerContentPending = true;
		try
		{
			BeginInvoke((Action)delegate
			{
				m_ensureActiveInnerContentPending = false;
				EnsureActiveInnerContent();
			});
		}
		catch (InvalidOperationException)
		{
			m_ensureActiveInnerContentPending = false;
			m_ensureActiveInnerContentWhenHandleCreated = true;
			PaintTimingLog.Write("{0} schedule ensure raced handle creation", TracePrefix);
		}
	}

	private void EnsureActiveInnerContent()
	{
		if (!Visible || m_disposing || IsDisposed || m_dockPanel == null || m_dockPanel.IsDisposed)
		{
			return;
		}
		if (HasValidActiveInnerContent())
		{
			return;
		}
		if (!TryActivateFallbackInnerContent())
		{
			PaintTimingLog.Write("{0} no fallback inner available, restoring default dock states", TracePrefix);
			RestoreDefaultDockStates();
			TryActivateFallbackInnerContent();
		}
	}

	private bool TryActivateFallbackInnerContent()
	{
		if (TryActivateInnerContent(m_lastActiveInnerContent))
		{
			return true;
		}
		Control[] preferredControls = new Control[8]
		{
			m_geometrySetEditor,
			m_cookParameterSetEditor,
			m_attachmentEditor,
			m_animationSetEditor,
			m_particleEffectSetEditor,
			m_behaviorSetEditor,
			m_splineSetEditor,
			m_propertyEditor
		};
		foreach (Control control in preferredControls)
		{
			if (m_dockContent.TryGetValue(control, out Firaxis.ATF.DockContent dockContent) && TryActivateInnerContent(dockContent))
			{
				return true;
			}
		}
		return false;
	}

	private bool TryActivateInnerContent(Firaxis.ATF.DockContent dockContent)
	{
		if (dockContent == null || dockContent.DockState == DockState.Hidden || dockContent.DockState == DockState.Unknown)
		{
			return false;
		}
		m_lastActiveInnerContent = dockContent;
		PaintTimingLog.Write("{0} setting fallback inner={1}, {2}", TracePrefix, dockContent.Name, GetInnerDockState());
		if (dockContent.DockHandler.Pane != null)
		{
			if (dockContent.DockHandler.Pane.ActiveContent != dockContent)
			{
				dockContent.DockHandler.Pane.ActiveContent = dockContent;
			}
			dockContent.Activate();
			return true;
		}
		dockContent.Show(m_dockPanel, DockState.Document);
		return true;
	}

	private void RestoreDefaultDockStates()
	{
		m_dockContent[m_propertyEditor].DockState = DockState.DockTop;
		m_dockContent[m_cookParameterSetEditor].DockState = m_context != null && m_context.HasCookParameters ? DockState.Document : DockState.Hidden;
		m_dockContent[m_geometrySetEditor].DockState = m_context != null && m_context.HasGeometries ? DockState.Document : DockState.Hidden;
		m_dockContent[m_attachmentEditor].DockState = DockState.Document;
		m_dockContent[m_animationSetEditor].DockState = m_context != null && m_context.HasAnimations ? DockState.Document : DockState.Hidden;
		m_dockContent[m_particleEffectSetEditor].DockState = m_context != null && m_context.HasParticleEffects ? DockState.Document : DockState.Hidden;
		m_dockContent[m_behaviorSetEditor].DockState = m_context != null && m_context.HasBehaviors ? DockState.Document : DockState.Hidden;
		m_dockContent[m_splineSetEditor].DockState = m_context != null && m_context.HasSplines ? DockState.Document : DockState.Hidden;
	}

	private void SetAssetEditorLayoutState(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		if (m_appliedEditorLayoutState == value || GetAssetEditorLayoutState() == value)
		{
			PaintTimingLog.Write("AssetEditorControl: skip identical editor layout");
			m_appliedEditorLayoutState = value;
			return;
		}
		PaintTimingLog.Write("AssetEditorControl: apply editor layout");
		m_dockPanel.SetLayoutState(value, m_dockContent);
		m_appliedEditorLayoutState = value;
	}

	private string GetAssetEditorLayoutState()
	{
		return m_dockPanel.GetLayoutState();
	}

	public override void Bind(IEntityEditorContext context)
	{
		PaintTimingLog.Write("{0} Bind begin handle={1}, visible={2}, parent={3}, {4}", TracePrefix, IsHandleCreated, Visible, Parent?.GetType().Name ?? "null", GetInnerDockState());
		if (m_disposing || m_controlHostUnregistering || IsDisposed)
		{
			PaintTimingLog.Write("{0} Bind skipped disposing={1}, unregistering={2}, disposed={3}", TracePrefix, m_disposing, m_controlHostUnregistering, IsDisposed);
			return;
		}
		try
		{
			var bindTimer = Stopwatch.StartNew();
			long previousStep = 0;
			if (m_context != null)
			{
				m_context.Reloaded -= AssetContext_Reloaded;
				m_context = null;
			}
			m_context = (IAssetEditorContext)context;
			PaintTimingLog.Write("{0} Bind step property editor", TracePrefix);
			m_propertyEditor.Bind(m_context?.EntityContext);
			PaintTimingLog.Write("{0} Bind timing property={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			m_attachmentEditor.Bind(m_context?.AttachmentsContext);
			PaintTimingLog.Write("{0} Bind timing attachments={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			BindCookParameters();
			PaintTimingLog.Write("{0} Bind timing cookParameters={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			BindGeometrySet();
			PaintTimingLog.Write("{0} Bind timing geometry={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			BindAnimationSet();
			PaintTimingLog.Write("{0} Bind timing animations={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			BindParticleEffectSet();
			PaintTimingLog.Write("{0} Bind timing particles={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			BindBehaviorSet();
			PaintTimingLog.Write("{0} Bind timing behaviors={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			BindSplineEditor();
			PaintTimingLog.Write("{0} Bind timing splines={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			if (m_dockContent[m_geometrySetEditor].DockHandler.Pane != null && m_dockContent[m_geometrySetEditor].DockState != DockState.Hidden)
			{
				m_dockContent[m_geometrySetEditor].Activate();
			}
			PaintTimingLog.Write("{0} Bind timing activateGeometry={1}ms", TracePrefix, bindTimer.ElapsedMilliseconds - previousStep);
			previousStep = bindTimer.ElapsedMilliseconds;
			ScheduleEnsureActiveInnerContent();
			if (m_context != null)
			{
				m_context.Reloaded += AssetContext_Reloaded;
			}
			PaintTimingLog.Write("{0} Bind end total={1}ms, finalize={2}ms, handle={3}, visible={4}, parent={5}, {6}", TracePrefix,
				bindTimer.ElapsedMilliseconds, bindTimer.ElapsedMilliseconds - previousStep, IsHandleCreated, Visible, Parent?.GetType().Name ?? "null", GetInnerDockState());
		}
		catch (System.Exception ex)
		{
			PaintTimingLog.Write("{0} Bind exception {1}: {2}\n{3}", TracePrefix, ex.GetType().FullName, ex.Message, ex.StackTrace);
			throw;
		}
	}

	private void BindCookParameters()
	{
		if (m_context != null && m_context.HasCookParameters)
		{
			ShowInnerDocument(m_cookParameterSetEditor);
			m_cookParameterSetEditor.Bind(m_context.CookParametersContext);
			m_cookParameterPropertyEditor.Bind(m_context.CookParametersContext);
			m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyChanged -= PropertyGridView_SelectedPropertyChanged;
			m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyChanged += PropertyGridView_SelectedPropertyChanged;
		}
		else
		{
			m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyChanged -= PropertyGridView_SelectedPropertyChanged;
			HideInnerDocument(m_cookParameterSetEditor);
		}
	}

	private void ShowInnerDocument(Control control)
	{
		Firaxis.ATF.DockContent dockContent = m_dockContent[control];
		if (dockContent.DockHandler.Pane == null)
		{
			dockContent.Show(m_dockPanel, DockState.Document);
		}
		else
		{
			dockContent.DockState = DockState.Document;
		}
	}

	private void HideInnerDocument(Control control)
	{
		Firaxis.ATF.DockContent dockContent = m_dockContent[control];
		if (dockContent.DockHandler.Pane != null || dockContent.DockState != DockState.Unknown)
		{
			dockContent.DockState = DockState.Hidden;
		}
	}

	private void PropertyGridView_SelectedPropertyChanged(object sender, EventArgs e)
	{
		CookParameterSetAdapter cookParameterSetAdapter = m_context.CookParametersContext.As<CookParameterSetAdapter>();
		DomNode domNode = m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyDescriptor.As<FieldPropertyDescriptorBase>()?.GetNode(cookParameterSetAdapter);
		domNode?.Parent.As<ISelectionContext>();
		while (domNode != null && !domNode.Equals(cookParameterSetAdapter.DomNode))
		{
			domNode = domNode.Parent;
			ISelectionContext selectionContext = domNode?.Parent.As<ISelectionContext>();
			if (domNode != null)
			{
				selectionContext?.Set(domNode);
			}
		}
	}

	private void BindSplineEditor()
	{
		if (m_context != null && m_context.HasSplines)
		{
			ShowInnerDocument(m_splineSetEditor);
			m_splineSetEditor.Bind(m_context.SplineSetContext);
		}
		else
		{
			HideInnerDocument(m_splineSetEditor);
		}
	}

	private void BindAnimationSet()
	{
		if (m_context != null && m_context.HasAnimations)
		{
			ShowInnerDocument(m_animationSetEditor);
			m_animationSetEditor.Bind(m_context.AnimationSetContext);
		}
		else
		{
			HideInnerDocument(m_animationSetEditor);
		}
	}

	private void BindGeometrySet()
	{
		if (m_context != null && m_context.HasGeometries)
		{
			ShowInnerDocument(m_geometrySetEditor);
			m_geometrySetEditor.Bind(m_context.GeometrySetContext);
		}
		else
		{
			HideInnerDocument(m_geometrySetEditor);
		}
	}

	private void BindBehaviorSet()
	{
		if (m_context != null && m_context.HasBehaviors)
		{
			ShowInnerDocument(m_behaviorSetEditor);
			m_behaviorSetEditor.Bind(m_context.BehaviorSetContext);
		}
		else
		{
			HideInnerDocument(m_behaviorSetEditor);
		}
	}

	private void BindParticleEffectSet()
	{
		if (m_context != null && m_context.HasParticleEffects)
		{
			ShowInnerDocument(m_particleEffectSetEditor);
			m_particleEffectSetEditor.Bind(m_context.ParticleEffectsContext);
		}
		else
		{
			HideInnerDocument(m_particleEffectSetEditor);
		}
	}

	private void AssetContext_Reloaded(object sender, EventArgs e)
	{
		BindCookParameters();
		BindGeometrySet();
		BindAnimationSet();
		BindParticleEffectSet();
		BindBehaviorSet();
		BindSplineEditor();
		ScheduleEnsureActiveInnerContent();
	}

	public void ResetControlLayout()
	{
		IEnumerable<SubControlInfo> infos = DetachControlInfos();
		ReattachControlInfos(infos);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_disposing = true;
			m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyChanged -= PropertyGridView_SelectedPropertyChanged;
			if (m_context != null)
			{
				m_context.Reloaded -= AssetContext_Reloaded;
				m_context = null;
			}
			Firaxis.ATF.DockContent activeContent = m_dockPanel?.ActiveContent as Firaxis.ATF.DockContent;
			foreach (KeyValuePair<Control, Firaxis.ATF.DockContent> item in m_dockContent
				.OrderBy(pair => pair.Value == activeContent ? 1 : 0).ToArray())
			{
				item.Value.VisibleChanged -= InnerDockContent_VisibleChanged;
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
	}
}
