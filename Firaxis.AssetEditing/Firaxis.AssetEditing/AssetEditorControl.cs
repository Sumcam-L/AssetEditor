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
	private readonly struct PageCapabilities
	{
		public PageCapabilities(IAssetEditorContext context)
		{
			CookParameters = context?.HasCookParameters == true;
			Geometries = context?.HasGeometries == true;
			Animations = context?.HasAnimations == true;
			Particles = context?.HasParticleEffects == true;
			Behaviors = context?.HasBehaviors == true;
			Splines = context?.HasSplines == true;
		}

		public bool CookParameters { get; }
		public bool Geometries { get; }
		public bool Animations { get; }
		public bool Particles { get; }
		public bool Behaviors { get; }
		public bool Splines { get; }
	}

	private enum PageKind
	{
		Geometries,
		CookParams,
		Attachments,
		Animations,
		Behaviors,
		Particles,
		Splines
	}

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

	private readonly AssetPageBindingCoordinator m_pageBindings = new AssetPageBindingCoordinator();

	private PageCapabilities m_pageCapabilities;

	private bool m_configuringPageBindings;

	private bool m_firstPaintCompleted;

	private bool m_pagePrewarmIdleSubscribed;

	private int m_pagePrewarmGeneration;

	private Timer m_pagePrewarmTimer;

	private static readonly PageKind[] s_pageCreationOrder =
	{
		PageKind.Geometries,
		PageKind.CookParams,
		PageKind.Attachments,
		PageKind.Animations,
		PageKind.Behaviors,
		PageKind.Particles,
		PageKind.Splines
	};

	private readonly Queue<PageKind> m_uncreatedPages = new Queue<PageKind>();

	private bool HasUncreatedPages => m_uncreatedPages.Count > 0;

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
		var ctorTimer = Stopwatch.StartNew();
		m_savedAssetEditorLayoutState = layoutState;
		m_initialEntityClassName = entityClassName;
		if (!string.IsNullOrEmpty(m_initialEntityClassName) && !string.IsNullOrEmpty(m_savedAssetEditorLayoutState))
		{
			TryGetSavedClassLayout(m_initialEntityClassName, out m_initialEditorLayoutState);
		}
		m_themeService = themeSvc;
		m_themeService.ThemeChanged += ThemeService_ThemeChanged;
		m_dockContent = new Dictionary<Control, Firaxis.ATF.DockContent>();
		TraceCtor("DockPanel", () =>
		{
			m_dockPanel = new DockPanel();
			m_dockPanel.Theme = m_themeService.ActiveTheme;
			m_dockPanel.Dock = DockStyle.Fill;
			m_dockPanel.ShowDocumentIcon = true;
			m_dockPanel.LargeDocumentIcon = true;
			m_dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
			m_dockPanel.ActiveContentChanged += InnerDockPanel_ActiveContentChanged;
			base.Controls.Add(m_dockPanel);
		});
		m_dockPanel.SuspendLayout(allWindows: true);
		try
		{
			TraceCtor("Properties", () =>
			{
				m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisableSearchControls | PropertyGridMode.HideResetAllButton);
				m_propertyEditor.Dock = DockStyle.Fill;
				m_propertyEditor.PropertySorting = PropertySorting.None;
				AddDockContext(m_propertyEditor, "Properties", string.Empty, DockState.DockTop, show: true, activate: false);
			});
		}
		finally
		{
			var swResume = Stopwatch.StartNew();
			m_dockPanel.ResumeLayout(performLayout: true, allWindows: true);
			PaintTimingLog.Write("{0} ctor ResumeLayout={1}ms", TracePrefix, swResume.ElapsedMilliseconds);
		}
		PaintTimingLog.Write("{0} ctor total={1}ms", TracePrefix, ctorTimer.ElapsedMilliseconds);
	}

	private void TraceCtor(string name, Action body)
	{
		var sw = Stopwatch.StartNew();
		body();
		PaintTimingLog.Write("{0} ctor part={1} elapsed={2}ms", TracePrefix, name, sw.ElapsedMilliseconds);
	}

	private void EnsurePageCreated(PageKind kind)
	{
		if (GetPageControl(kind) != null)
		{
			return;
		}
		var sw = Stopwatch.StartNew();
		m_dockPanel.SuspendLayout(allWindows: true);
		try
		{
			switch (kind)
			{
				case PageKind.Geometries:
					CreateGeometriesPageCore();
					break;
				case PageKind.CookParams:
					CreateCookParamsPageCore();
					break;
				case PageKind.Attachments:
					CreateAttachmentsPageCore();
					break;
				case PageKind.Animations:
					CreateAnimationsPageCore();
					break;
				case PageKind.Behaviors:
					CreateBehaviorsPageCore();
					break;
				case PageKind.Particles:
					CreateParticlesPageCore();
					break;
				case PageKind.Splines:
					CreateSplinesPageCore();
					break;
			}
		}
		finally
		{
			m_dockPanel.ResumeLayout(performLayout: IsHandleCreated, allWindows: true);
		}
		PaintTimingLog.Write("{0} AssetPageCreate page={1} elapsed={2}ms", TracePrefix, kind, sw.ElapsedMilliseconds);
	}

	private void CreateCookParamsPageCore()
	{
		m_cookParameterPropertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisplayDescriptions | PropertyGridMode.HideResetAllButton);
		m_cookParameterPropertyEditor.BuildPropertiesWhenHidden = true;
		m_cookParameterPropertyEditor.PropertyGridView.ShowRowStriping = false;
		m_cookParameterSetEditor = new CommandControl();
		m_cookParameterSetEditor.Dock = DockStyle.Fill;
		m_cookParameterPropertyEditor.Dock = DockStyle.Fill;
		m_cookParameterSetEditor.ChildControls.Add(m_cookParameterPropertyEditor);
		AddDockContext(m_cookParameterSetEditor, "Cook Params", Resources.CookParametersCategoryIcon, DockState.Document, show: true, activate: false);
	}

	private void CreateGeometriesPageCore()
	{
		m_geometrySetEditor = new ModelInstanceStateEditor();
		m_geometrySetEditor.Dock = DockStyle.Fill;
		AddDockContext(m_geometrySetEditor, "Geometries", Resources.GeometryCategoryIcon, DockState.Document, show: true, activate: false);
	}

	private void CreateAttachmentsPageCore()
	{
		m_attachmentEditor = new PropertyEditingListControl(PropertyCategorySettings.HideText, PropertySorting.Categorized);
		m_attachmentEditor.Dock = DockStyle.Fill;
		AddDockContext(m_attachmentEditor, "Attachments", Resources.AttachmentsCategoryIcon, DockState.Document, show: true, activate: false);
	}

	private void CreateAnimationsPageCore()
	{
		m_animationSetEditor = new PropertyEditingListControl(PropertySorting.None);
		m_animationSetEditor.Dock = DockStyle.Fill;
		AddDockContext(m_animationSetEditor, "Animations", Resources.AnimationsCategoryIcon, DockState.Document, show: true, activate: false);
	}

	private void CreateParticlesPageCore()
	{
		m_particleEffectSetEditor = new PropertyEditingListControl(PropertySorting.None);
		m_particleEffectSetEditor.Dock = DockStyle.Fill;
		AddDockContext(m_particleEffectSetEditor, "Particles", Resources.ParticlesCategoryIcon, DockState.Document, show: true, activate: false);
	}

	private void CreateBehaviorsPageCore()
	{
		m_behaviorSetEditor = new PropertyEditingListControl(PropertySorting.None);
		m_behaviorSetEditor.Dock = DockStyle.Fill;
		AddDockContext(m_behaviorSetEditor, "Behaviors", Resources.BehaviorCategoryIcon, DockState.Document, show: true, activate: false);
	}

	private void CreateSplinesPageCore()
	{
		m_splineSetEditor = new PropertyEditingListControl(PropertyCategorySettings.HideText, PropertySorting.Categorized);
		m_splineSetEditor.Dock = DockStyle.Fill;
		AddDockContext(m_splineSetEditor, "Splines", Resources.SplineCategoryIcon, DockState.Document, show: true, activate: false);
	}

	private Control GetPageControl(PageKind kind)
	{
		switch (kind)
		{
			case PageKind.Geometries: return m_geometrySetEditor;
			case PageKind.CookParams: return m_cookParameterSetEditor;
			case PageKind.Attachments: return m_attachmentEditor;
			case PageKind.Animations: return m_animationSetEditor;
			case PageKind.Behaviors: return m_behaviorSetEditor;
			case PageKind.Particles: return m_particleEffectSetEditor;
			case PageKind.Splines: return m_splineSetEditor;
			default: return null;
		}
	}

	private static bool IsPageCapable(PageKind kind, PageCapabilities capabilities)
	{
		switch (kind)
		{
			case PageKind.Geometries: return capabilities.Geometries;
			case PageKind.CookParams: return capabilities.CookParameters;
			case PageKind.Attachments: return true;
			case PageKind.Animations: return capabilities.Animations;
			case PageKind.Behaviors: return capabilities.Behaviors;
			case PageKind.Particles: return capabilities.Particles;
			case PageKind.Splines: return capabilities.Splines;
			default: return false;
		}
	}

	private static PageKind GetInitialPageKind(PageCapabilities capabilities)
	{
		if (capabilities.Geometries)
		{
			return PageKind.Geometries;
		}
		if (capabilities.CookParameters)
		{
			return PageKind.CookParams;
		}
		return PageKind.Attachments;
	}

	private void RebuildUncreatedPageQueue()
	{
		m_uncreatedPages.Clear();
		foreach (PageKind kind in s_pageCreationOrder)
		{
			if (IsPageCapable(kind, m_pageCapabilities) && GetPageControl(kind) == null)
			{
				m_uncreatedPages.Enqueue(kind);
			}
		}
	}

	private AssetPageBindingCoordinator.Page CreatePageBindingFor(PageKind kind)
	{
		switch (kind)
		{
			case PageKind.Geometries: return CreatePageBinding("Geometries", m_geometrySetEditor, BindGeometrySet);
			case PageKind.CookParams: return CreatePageBinding("Cook Params", m_cookParameterSetEditor, BindCookParameters);
			case PageKind.Attachments: return CreatePageBinding("Attachments", m_attachmentEditor, BindAttachments);
			case PageKind.Animations: return CreatePageBinding("Animations", m_animationSetEditor, BindAnimationSet);
			case PageKind.Behaviors: return CreatePageBinding("Behaviors", m_behaviorSetEditor, BindBehaviorSet);
			case PageKind.Particles: return CreatePageBinding("Particles", m_particleEffectSetEditor, BindParticleEffectSet);
			case PageKind.Splines: return CreatePageBinding("Splines", m_splineSetEditor, BindSplineEditor);
			default: throw new ArgumentOutOfRangeException(nameof(kind));
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
		CancelPagePrewarm("unregister");
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
			SchedulePagePrewarmAfterFirstPaint();
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
		if (!m_firstPaintCompleted)
		{
			m_firstPaintCompleted = true;
			SchedulePagePrewarmAfterFirstPaint();
		}
	}

	private void InnerDockPanel_ActiveContentChanged(object sender, EventArgs e)
	{
		Firaxis.ATF.DockContent active = m_dockPanel.ActiveContent as Firaxis.ATF.DockContent;
		BindPendingPageForUser(active);
		PaintTimingLog.Write("{0} InnerActiveContentChanged {1}", TracePrefix, GetInnerDockState());
		if (HasValidActiveInnerContent())
		{
			m_lastActiveInnerContent = (Firaxis.ATF.DockContent)m_dockPanel.ActiveContent;
		}
		if (Visible && ContainsFocus && !HasValidActiveInnerContent())
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
			BindPendingPageForUser(dockContent);
			m_lastActiveInnerContent = dockContent;
		}
	}

	private void BindPendingPageForUser(Firaxis.ATF.DockContent dockContent)
	{
		if (m_configuringPageBindings || dockContent == null || dockContent.Controls.Count == 0 ||
			m_disposing || m_controlHostUnregistering || IsDisposed)
		{
			return;
		}

		m_pageBindings.BindForUser(dockContent.Controls[0]);
		if (!m_pageBindings.HasPending && !HasUncreatedPages)
		{
			m_pagePrewarmTimer?.Stop();
			UnsubscribePagePrewarmIdle();
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
				if (ContainsFocus)
				{
					EnsureActiveInnerContent();
				}
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
			if (control == null)
			{
				continue;
			}
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
		TrySetDockState(m_propertyEditor, DockState.DockTop);
		TrySetDockState(m_cookParameterSetEditor, m_pageCapabilities.CookParameters ? DockState.Document : DockState.Hidden);
		TrySetDockState(m_geometrySetEditor, m_pageCapabilities.Geometries ? DockState.Document : DockState.Hidden);
		TrySetDockState(m_attachmentEditor, DockState.Document);
		TrySetDockState(m_animationSetEditor, m_pageCapabilities.Animations ? DockState.Document : DockState.Hidden);
		TrySetDockState(m_particleEffectSetEditor, m_pageCapabilities.Particles ? DockState.Document : DockState.Hidden);
		TrySetDockState(m_behaviorSetEditor, m_pageCapabilities.Behaviors ? DockState.Document : DockState.Hidden);
		TrySetDockState(m_splineSetEditor, m_pageCapabilities.Splines ? DockState.Document : DockState.Hidden);
	}

	private void TrySetDockState(Control control, DockState state)
	{
		if (control != null && m_dockContent.TryGetValue(control, out Firaxis.ATF.DockContent dockContent))
		{
			dockContent.DockState = state;
		}
	}

	private void ApplyPageCapabilities(PageCapabilities capabilities)
	{
		SetPageAvailable(m_cookParameterSetEditor, capabilities.CookParameters);
		SetPageAvailable(m_geometrySetEditor, capabilities.Geometries);
		SetPageAvailable(m_attachmentEditor, available: true);
		SetPageAvailable(m_animationSetEditor, capabilities.Animations);
		SetPageAvailable(m_particleEffectSetEditor, capabilities.Particles);
		SetPageAvailable(m_behaviorSetEditor, capabilities.Behaviors);
		SetPageAvailable(m_splineSetEditor, capabilities.Splines);
	}

	private void SetPageAvailable(Control control, bool available)
	{
		if (available)
		{
			ShowInnerDocument(control);
		}
		else
		{
			HideInnerDocument(control);
		}
	}

	private IEnumerable<AssetPageBindingCoordinator.Page> CreatePageBindings(PageCapabilities capabilities)
	{
		foreach (PageKind kind in s_pageCreationOrder)
		{
			if (IsPageCapable(kind, capabilities) && GetPageControl(kind) != null)
			{
				yield return CreatePageBindingFor(kind);
			}
		}
	}

	private AssetPageBindingCoordinator.Page CreatePageBinding(string name, Control control, Action bind)
	{
		return new AssetPageBindingCoordinator.Page(name, control, trigger => BindPage(name, trigger, bind));
	}

	private void BindPage(string name, string trigger, Action bind)
	{
		var timer = Stopwatch.StartNew();
		int generation = m_pageBindings.Generation;
		PaintTimingLog.Write("AssetPageBind begin page={0} trigger={1} generation={2}", name, trigger, generation);
		try
		{
			bind();
		}
		finally
		{
			PaintTimingLog.Write("AssetPageBind end page={0} trigger={1} generation={2} elapsed={3}ms",
				name, trigger, generation, timer.ElapsedMilliseconds);
		}
	}

	private void BindAttachments()
	{
		m_attachmentEditor.Bind(m_context?.AttachmentsContext);
	}

	private void ClearOptionalPageBindings()
	{
		UnsubscribeCookParameterSelection();
		m_cookParameterSetEditor?.Bind(null);
		m_cookParameterPropertyEditor?.Bind(null);
		m_geometrySetEditor?.Bind(null);
		m_attachmentEditor?.Bind(null);
		m_animationSetEditor?.Bind(null);
		m_particleEffectSetEditor?.Bind(null);
		m_behaviorSetEditor?.Bind(null);
		m_splineSetEditor?.Bind(null);
	}

	private void ConfigurePageBindings(bool preserveActivePage)
	{
		m_configuringPageBindings = true;
		try
		{
			ClearOptionalPageBindings();
			CancelPagePrewarm("configure");
			m_firstPaintCompleted = false;
			m_pageCapabilities = new PageCapabilities(m_context);
			if (m_context != null)
			{
				Control initialPage = preserveActivePage ? GetCurrentSupportedPage() : null;
				if (initialPage == null)
				{
					PageKind initialKind = GetInitialPageKind(m_pageCapabilities);
					EnsurePageCreated(initialKind);
					initialPage = GetPageControl(initialKind);
				}
				ApplyPageCapabilities(m_pageCapabilities);
				RebuildUncreatedPageQueue();
				m_pageBindings.BeginGeneration(CreatePageBindings(m_pageCapabilities), initialPage);
				ActivateInitialPage(initialPage);
			}
			else
			{
				ApplyPageCapabilities(m_pageCapabilities);
				m_uncreatedPages.Clear();
			}
		}
		finally
		{
			m_configuringPageBindings = false;
		}

		if (IsHandleCreated && Visible)
		{
			Invalidate(invalidateChildren: true);
		}
	}

	private Control GetCurrentSupportedPage()
	{
		Firaxis.ATF.DockContent active = m_dockPanel.ActiveContent as Firaxis.ATF.DockContent;
		if (active == null || active.Controls.Count == 0)
		{
			return null;
		}

		Control control = active.Controls[0];
		return IsSupportedPage(control) ? control : null;
	}

	private bool IsSupportedPage(Control control)
	{
		return control == m_attachmentEditor ||
			(control == m_geometrySetEditor && m_pageCapabilities.Geometries) ||
			(control == m_cookParameterSetEditor && m_pageCapabilities.CookParameters) ||
			(control == m_animationSetEditor && m_pageCapabilities.Animations) ||
			(control == m_particleEffectSetEditor && m_pageCapabilities.Particles) ||
			(control == m_behaviorSetEditor && m_pageCapabilities.Behaviors) ||
			(control == m_splineSetEditor && m_pageCapabilities.Splines);
	}

	private void ActivateInitialPage(Control initialPage)
	{
		if (initialPage == null || !m_dockContent.TryGetValue(initialPage, out Firaxis.ATF.DockContent content))
		{
			return;
		}

		if (content.DockHandler.Pane != null)
		{
			if (content.DockHandler.Pane.ActiveContent != content)
			{
				content.DockHandler.Pane.ActiveContent = content;
			}
			content.Activate();
		}
		else
		{
			content.Show(m_dockPanel, DockState.Document);
		}
		m_lastActiveInnerContent = content;
	}

	private bool BindNextIdle()
	{
		return m_pageBindings.BindNextIdle();
	}

	private bool TryCreateNextUncreatedPage()
	{
		while (m_uncreatedPages.Count > 0)
		{
			PageKind kind = m_uncreatedPages.Dequeue();
			if (!IsPageCapable(kind, m_pageCapabilities) || GetPageControl(kind) != null)
			{
				continue;
			}
			EnsurePageCreated(kind);
			m_pageBindings.AddPage(CreatePageBindingFor(kind));
			return true;
		}
		return false;
	}

	private void SchedulePagePrewarmAfterFirstPaint()
	{
		if (m_pagePrewarmIdleSubscribed ||
			(!m_pageBindings.HasPending && !HasUncreatedPages) ||
			m_disposing || m_controlHostUnregistering || IsDisposed)
		{
			return;
		}

		m_pagePrewarmGeneration = m_pageBindings.Generation;
		Application.Idle += PagePrewarm_Idle;
		m_pagePrewarmIdleSubscribed = true;
		PaintTimingLog.Write("AssetPagePrewarm scheduled generation={0} pending={1} uncreated={2}",
			m_pagePrewarmGeneration, m_pageBindings.PendingCount, m_uncreatedPages.Count);
	}

	private void PagePrewarm_Idle(object sender, EventArgs e)
	{
		UnsubscribePagePrewarmIdle();
		if (m_disposing || m_controlHostUnregistering || IsDisposed)
		{
			return;
		}
		if (m_pagePrewarmGeneration != m_pageBindings.Generation)
		{
			PaintTimingLog.Write("AssetPagePrewarm stale generation={0} currentGeneration={1}",
				m_pagePrewarmGeneration, m_pageBindings.Generation);
			return;
		}
		int generation = m_pagePrewarmGeneration;

		try
		{
			while (TryCreateNextUncreatedPage())
			{
			}
		}
		catch (System.Exception ex)
		{
			PaintTimingLog.Write("AssetPagePrewarm create failed generation={0} error={1}: {2}",
				generation, ex.GetType().Name, ex.Message);
		}
		try
		{
			BindNextIdle();
		}
		catch (System.Exception ex)
		{
			PaintTimingLog.Write("AssetPagePrewarm bind failed generation={0} error={1}: {2}",
				generation, ex.GetType().Name, ex.Message);
		}
		if (generation != m_pageBindings.Generation)
		{
			PaintTimingLog.Write("AssetPagePrewarm stale generation={0} currentGeneration={1}",
				generation, m_pageBindings.Generation);
			return;
		}

		if (!m_pageBindings.HasPending && !HasUncreatedPages)
		{
			return;
		}

		if (m_pagePrewarmTimer == null)
		{
			m_pagePrewarmTimer = new Timer { Interval = 150 };
			m_pagePrewarmTimer.Tick += PagePrewarmTimer_Tick;
		}
		m_pagePrewarmTimer.Stop();
		m_pagePrewarmGeneration = generation;
		m_pagePrewarmTimer.Start();
	}

	private void PagePrewarmTimer_Tick(object sender, EventArgs e)
	{
		m_pagePrewarmTimer.Stop();
		if (m_pagePrewarmGeneration != m_pageBindings.Generation)
		{
			PaintTimingLog.Write("AssetPagePrewarm timer stale generation={0} currentGeneration={1}",
				m_pagePrewarmGeneration, m_pageBindings.Generation);
			return;
		}
		SchedulePagePrewarmAfterFirstPaint();
	}

	private void UnsubscribePagePrewarmIdle()
	{
		if (!m_pagePrewarmIdleSubscribed)
		{
			return;
		}
		Application.Idle -= PagePrewarm_Idle;
		m_pagePrewarmIdleSubscribed = false;
	}

	private void CancelPagePrewarm(string reason)
	{
		m_pagePrewarmTimer?.Stop();
		UnsubscribePagePrewarmIdle();
		m_pageBindings.Cancel();
		m_uncreatedPages.Clear();
		m_pagePrewarmGeneration = 0;
		PaintTimingLog.Write("AssetPagePrewarm canceled generation={0} reason={1}",
			m_pageBindings.Generation, reason);
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
		EnsureActiveInnerContent();
	}

	private string GetAssetEditorLayoutState()
	{
		return m_dockPanel.GetLayoutState();
	}

	private void ResetFailedBinding()
	{
		System.Exception firstCleanupException = null;
		void TryCleanup(Action cleanup)
		{
			try
			{
				cleanup();
			}
			catch (System.Exception ex)
			{
				firstCleanupException ??= ex;
			}
		}

		m_configuringPageBindings = true;
		try
		{
			TryCleanup(() =>
			{
				if (m_context != null)
				{
					m_context.Reloaded -= AssetContext_Reloaded;
				}
			});
			TryCleanup(() => CancelPagePrewarm("bind-failed"));
			TryCleanup(ClearOptionalPageBindings);
			TryCleanup(() => m_propertyEditor.Bind(null));
			m_context = null;
			m_pageCapabilities = default;
			TryCleanup(() => ApplyPageCapabilities(m_pageCapabilities));
		}
		finally
		{
			m_configuringPageBindings = false;
		}

		if (firstCleanupException != null)
		{
			throw firstCleanupException;
		}
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
			if (m_context != null)
			{
				m_context.Reloaded -= AssetContext_Reloaded;
			}
			m_context = (IAssetEditorContext)context;
			PaintTimingLog.Write("{0} Bind step property editor", TracePrefix);
			m_propertyEditor.Bind(m_context?.EntityContext);
			ConfigurePageBindings(preserveActivePage: false);
			ScheduleEnsureActiveInnerContent();
			if (m_context != null)
			{
				m_context.Reloaded += AssetContext_Reloaded;
			}
			PaintTimingLog.Write("{0} Bind end total={1}ms, handle={2}, visible={3}, parent={4}, {5}", TracePrefix,
				bindTimer.ElapsedMilliseconds, IsHandleCreated, Visible, Parent?.GetType().Name ?? "null", GetInnerDockState());
		}
		catch (System.Exception ex)
		{
			PaintTimingLog.Write("{0} Bind exception {1}: {2}\n{3}", TracePrefix, ex.GetType().FullName, ex.Message, ex.StackTrace);
			try
			{
				ResetFailedBinding();
			}
			catch (System.Exception cleanupEx)
			{
				PaintTimingLog.Write("{0} Bind cleanup exception {1}: {2}\n{3}", TracePrefix,
					cleanupEx.GetType().FullName, cleanupEx.Message, cleanupEx.StackTrace);
			}
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
			UnsubscribeCookParameterSelection();
			var propertyGridView = m_cookParameterPropertyEditor?.PropertyGridView;
			if (propertyGridView != null)
			{
				propertyGridView.SelectedPropertyChanged += PropertyGridView_SelectedPropertyChanged;
			}
		}
		else
		{
			UnsubscribeCookParameterSelection();
			HideInnerDocument(m_cookParameterSetEditor);
		}
	}

	private void UnsubscribeCookParameterSelection()
	{
		var propertyGridView = m_cookParameterPropertyEditor?.PropertyGridView;
		if (propertyGridView != null)
		{
			propertyGridView.SelectedPropertyChanged -= PropertyGridView_SelectedPropertyChanged;
		}
	}

	private void ShowInnerDocument(Control control)
	{
		if (control == null || !m_dockContent.TryGetValue(control, out Firaxis.ATF.DockContent dockContent))
		{
			return;
		}
		if (dockContent.DockHandler.Pane == null)
		{
			dockContent.Show(m_dockPanel, DockState.Document);
		}
		else if (dockContent.DockState != DockState.Document)
		{
			dockContent.DockState = DockState.Document;
		}
	}

	private void HideInnerDocument(Control control)
	{
		if (control == null || !m_dockContent.TryGetValue(control, out Firaxis.ATF.DockContent dockContent))
		{
			return;
		}
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
		if (m_context == null || m_disposing || m_controlHostUnregistering || IsDisposed)
		{
			return;
		}

		try
		{
			ConfigurePageBindings(preserveActivePage: true);
			ScheduleEnsureActiveInnerContent();
		}
		catch (System.Exception ex)
		{
			PaintTimingLog.Write("{0} Reload exception {1}: {2}\n{3}", TracePrefix,
				ex.GetType().FullName, ex.Message, ex.StackTrace);
			try
			{
				ResetFailedBinding();
			}
			catch (System.Exception cleanupEx)
			{
				PaintTimingLog.Write("{0} Reload cleanup exception {1}: {2}\n{3}", TracePrefix,
					cleanupEx.GetType().FullName, cleanupEx.Message, cleanupEx.StackTrace);
			}
			throw;
		}
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
			CancelPagePrewarm("dispose");
			if (m_pagePrewarmTimer != null)
			{
				m_pagePrewarmTimer.Tick -= PagePrewarmTimer_Tick;
				m_pagePrewarmTimer.Dispose();
				m_pagePrewarmTimer = null;
			}
			UnsubscribeCookParameterSelection();
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
