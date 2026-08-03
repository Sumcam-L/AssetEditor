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

	private struct PageInfo
	{
		public string Label;
		public string Icon;
		public Control Ctl;
		public Button TabButton;
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

	private IThemeService m_themeService;

	private SplitContainer m_splitContainer;

	private Panel m_tabContainer;

	private FlowLayoutPanel m_tabStrip;

	private Panel m_tabContent;

	private Dictionary<PageKind, Button> m_tabButtons;

	private Dictionary<PageKind, PageInfo> m_pageInfos;

	private HashSet<Control> m_boundPages = new HashSet<Control>();

	private IContainer components;

	private string m_appliedEditorLayoutState = string.Empty;

	private readonly string m_savedAssetEditorLayoutState;

	private readonly string m_initialEntityClassName;

	private readonly string m_initialEditorLayoutState;

	private bool m_disposing;

	private bool m_controlHostUnregistering;

	private PageCapabilities m_pageCapabilities;

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

	private string TracePrefix => "EntityEditorUI: AssetEditorControl#" + RuntimeHelpers.GetHashCode(this).ToString("X");

	public string EditorLayoutState
	{
		get => GetAssetEditorLayoutState();
		set => SetAssetEditorLayoutState(value);
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
		m_tabButtons = new Dictionary<PageKind, Button>();
		m_pageInfos = new Dictionary<PageKind, PageInfo>();
		m_pageCapabilities = default;
		TraceCtor("Layout", () =>
		{
			m_splitContainer = new SplitContainer();
			m_splitContainer.Dock = DockStyle.Fill;
			m_splitContainer.Orientation = Orientation.Horizontal;
			m_splitContainer.SplitterWidth = 4;
			m_splitContainer.FixedPanel = FixedPanel.Panel1;
			m_splitContainer.IsSplitterFixed = false;
			m_splitContainer.SplitterDistance = 200;
			base.Controls.Add(m_splitContainer);
		});

		TraceCtor("Properties", () =>
		{
			m_propertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisableSearchControls | PropertyGridMode.HideResetAllButton);
			m_propertyEditor.Dock = DockStyle.Fill;
			m_propertyEditor.PropertySorting = PropertySorting.None;
			m_splitContainer.Panel1.Controls.Add(m_propertyEditor);
		});

		TraceCtor("TabControl", () =>
		{
			m_tabContainer = new Panel();
			m_tabContainer.Dock = DockStyle.Fill;
			m_tabContainer.BackColor = Color.FromArgb(60, 60, 60);

			m_tabStrip = new FlowLayoutPanel();
			m_tabStrip.Dock = DockStyle.Top;
			m_tabStrip.Height = 24;
			m_tabStrip.FlowDirection = FlowDirection.LeftToRight;
			m_tabStrip.WrapContents = false;
			m_tabStrip.AutoSize = false;
			m_tabStrip.BackColor = Color.FromArgb(60, 60, 60);
			m_tabStrip.Padding = new Padding(0);
			m_tabStrip.Margin = new Padding(0);

			m_tabContent = new Panel();
			m_tabContent.Dock = DockStyle.Fill;
			m_tabContent.BackColor = Color.FromArgb(60, 60, 60);

			m_tabContainer.Controls.Add(m_tabStrip);
			m_tabContainer.Controls.Add(m_tabContent);
			m_splitContainer.Panel2.BackColor = Color.FromArgb(60, 60, 60);
			m_splitContainer.Panel2.Controls.Add(m_tabContainer);
		});

		CreateAllPageCores();

		PaintTimingLog.Write("{0} ctor total={1}ms", TracePrefix, ctorTimer.ElapsedMilliseconds);
	}

	private void TraceCtor(string name, Action body)
	{
		var sw = Stopwatch.StartNew();
		body();
		PaintTimingLog.Write("{0} ctor part={1} elapsed={2}ms", TracePrefix, name, sw.ElapsedMilliseconds);
	}

	private void CreateAllPageCores()
	{
		foreach (PageKind kind in s_pageCreationOrder)
		{
			CreatePageCore(kind);
		}
		// Default Attachments page visible, others hidden until Bind
		UpdateTabVisibility(default(PageCapabilities));
	}

	private void CreatePageCore(PageKind kind)
	{
		Control control = null;
		string label = null;
		string icon = null;

		switch (kind)
		{
			case PageKind.Geometries:
				m_geometrySetEditor = new ModelInstanceStateEditor();
				m_geometrySetEditor.Dock = DockStyle.Fill;
				control = m_geometrySetEditor;
				label = "Geometries";
				icon = Resources.GeometryCategoryIcon;
				break;
			case PageKind.CookParams:
				m_cookParameterPropertyEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisplayDescriptions | PropertyGridMode.HideResetAllButton);
				m_cookParameterPropertyEditor.BuildPropertiesWhenHidden = true;
				m_cookParameterPropertyEditor.PropertyGridView.ShowRowStriping = false;
				m_cookParameterSetEditor = new CommandControl();
				m_cookParameterSetEditor.Dock = DockStyle.Fill;
				m_cookParameterPropertyEditor.Dock = DockStyle.Fill;
				m_cookParameterSetEditor.ChildControls.Add(m_cookParameterPropertyEditor);
				control = m_cookParameterSetEditor;
				label = "Cook Params";
				icon = Resources.CookParametersCategoryIcon;
				break;
			case PageKind.Attachments:
				m_attachmentEditor = new PropertyEditingListControl(PropertyCategorySettings.HideText, PropertySorting.Categorized);
				m_attachmentEditor.Dock = DockStyle.Fill;
				control = m_attachmentEditor;
				label = "Attachments";
				icon = Resources.AttachmentsCategoryIcon;
				break;
			case PageKind.Animations:
				m_animationSetEditor = new PropertyEditingListControl(PropertySorting.None);
				m_animationSetEditor.Dock = DockStyle.Fill;
				control = m_animationSetEditor;
				label = "Animations";
				icon = Resources.AnimationsCategoryIcon;
				break;
			case PageKind.Particles:
				m_particleEffectSetEditor = new PropertyEditingListControl(PropertySorting.None);
				m_particleEffectSetEditor.Dock = DockStyle.Fill;
				control = m_particleEffectSetEditor;
				label = "Particles";
				icon = Resources.ParticlesCategoryIcon;
				break;
			case PageKind.Behaviors:
				m_behaviorSetEditor = new PropertyEditingListControl(PropertySorting.None);
				m_behaviorSetEditor.Dock = DockStyle.Fill;
				control = m_behaviorSetEditor;
				label = "Behaviors";
				icon = Resources.BehaviorCategoryIcon;
				break;
			case PageKind.Splines:
				m_splineSetEditor = new PropertyEditingListControl(PropertyCategorySettings.HideText, PropertySorting.Categorized);
				m_splineSetEditor.Dock = DockStyle.Fill;
				control = m_splineSetEditor;
				label = "Splines";
				icon = Resources.SplineCategoryIcon;
				break;
		}

		if (control == null)
			return;

		control.Visible = false;
		m_tabContent.Controls.Add(control);
		m_pageInfos[kind] = new PageInfo { Label = label, Icon = icon, Ctl = control, TabButton = null };
	}

	private Control GetPageControl(PageKind kind)
	{
		if (m_pageInfos.TryGetValue(kind, out var info))
			return info.Ctl;
		return null;
	}

	

	private bool IsPageCapable(PageKind kind, PageCapabilities capabilities)
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
			return PageKind.Geometries;
		if (capabilities.CookParameters)
			return PageKind.CookParams;
		return PageKind.Attachments;
	}

	private void UpdateTabVisibility(PageCapabilities capabilities)
	{
		m_tabStrip.Controls.Clear();
		m_tabButtons.Clear();

		bool first = true;
		foreach (PageKind kind in s_pageCreationOrder)
		{
			if (!IsPageCapable(kind, capabilities))
				continue;

			var btn = new Button();
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;
			btn.BackColor = Color.FromArgb(60, 60, 60);
			btn.ForeColor = Color.White;
			btn.AutoSize = false;
			btn.Size = new Size(100, 24);
			btn.Padding = new Padding(0);
			btn.Text = m_pageInfos[kind].Label;
			btn.Tag = kind;
			btn.Click += TabButton_Click;

			m_tabStrip.Controls.Add(btn);
			m_tabButtons[kind] = btn;

			if (first)
			{
				btn.BackColor = Color.FromArgb(85, 85, 85);
				var ctl = m_pageInfos[kind].Ctl;
				ctl.Visible = true;
				ctl.Dock = DockStyle.Fill;
				ctl.BringToFront();
				first = false;
			}
		}
	}

	private int GetActiveTabIndex()
	{
		int index = 0;
		foreach (Control ctl in m_tabStrip.Controls)
		{
			if (ctl is Button btn && btn.BackColor == Color.FromArgb(85, 85, 85))
				return index;
			index++;
		}
		return -1;
	}

	private string GetActiveTabText()
	{
		foreach (Control ctl in m_tabStrip.Controls)
		{
			if (ctl is Button btn && btn.BackColor == Color.FromArgb(85, 85, 85))
				return btn.Text;
		}
		return null;
	}

	private string GetInnerDockState()
	{
		return $"activeTab={GetActiveTabText() ?? "null"}, index={GetActiveTabIndex()}";
	}

	public bool IsEditorLayoutStateApplied(string value)
	{
		return !string.IsNullOrEmpty(value) && (m_appliedEditorLayoutState == value || GetAssetEditorLayoutState() == value);
	}

	public void BeforeControlHostShow()
	{
		ApplyInitialClassLayout();
		EnsureActivePage();
	}

	public void BeforeControlHostUnregister()
	{
		m_controlHostUnregistering = true;
		PaintTimingLog.Write("{0} before control host unregister", TracePrefix);
	}

	private void ApplyInitialClassLayout()
	{
		if (!string.IsNullOrEmpty(m_initialEditorLayoutState))
		{
			SetAssetEditorLayoutState(m_initialEditorLayoutState);
		}
	}

	private bool TryGetSavedClassLayout(string className, out string layout)
	{
		layout = null;
		try
		{
			var doc = new XmlDocument();
			doc.LoadXml(m_savedAssetEditorLayoutState);
			foreach (XmlNode node in doc.DocumentElement.GetElementsByTagName("layout"))
			{
				if (node.Attributes["entityclass"]?.Value == className)
				{
					layout = node.InnerXml;
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private void ThemeService_ThemeChanged(object sender, EventArgs e)
	{
		// TabControl and SplitContainer don't need theme change handling
		// Only the PropertyGrid needs theme update
		m_propertyEditor?.PropertyGridView?.Invalidate();
	}

	private void TabButton_Click(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.Tag is PageKind kind)
		{
			// Un-highlight all buttons
			foreach (Control ctl in m_tabStrip.Controls)
			{
				if (ctl is Button b)
					b.BackColor = Color.FromArgb(60, 60, 60);
			}

			// Highlight clicked button
			btn.BackColor = Color.FromArgb(85, 85, 85);

			// Hide all controls, show the selected one
			foreach (Control ctl in m_tabContent.Controls)
				ctl.Visible = false;

			var activeControl = m_pageInfos[kind].Ctl;
			if (activeControl != null)
			{
				activeControl.Visible = true;
				activeControl.Dock = DockStyle.Fill;
				activeControl.BringToFront();
			}

			BindPageForControl(activeControl);
		}
	}

	private void BindPendingPage()
	{
		Control activeControl = null;
		foreach (Control ctl in m_tabContent.Controls)
		{
			if (ctl.Visible)
			{
				activeControl = ctl;
				break;
			}
		}

		if (activeControl == null)
			return;

		BindPageForControl(activeControl);
	}

	private void BindPageForControl(Control control)
	{
		if (control == null || m_boundPages.Contains(control))
			return;

		m_boundPages.Add(control);

		if (control == m_geometrySetEditor)
			BindGeometrySet();
		else if (control == m_cookParameterSetEditor)
			BindCookParameters();
		else if (control == m_attachmentEditor)
			BindAttachments();
		else if (control == m_animationSetEditor)
			BindAnimationSet();
		else if (control == m_behaviorSetEditor)
			BindBehaviorSet();
		else if (control == m_particleEffectSetEditor)
			BindParticleEffectSet();
		else if (control == m_splineSetEditor)
			BindSplineEditor();
	}

	private void EnsureActivePage()
	{
		if (m_disposing || IsDisposed)
			return;

		// Find if any button is already highlighted
		foreach (Control ctl in m_tabStrip.Controls)
		{
			if (ctl is Button btn && btn.BackColor == Color.FromArgb(85, 85, 85))
				return; // Already have an active page
		}

		// No highlighted button; activate the first one
		if (m_tabStrip.Controls.Count > 0 && m_tabStrip.Controls[0] is Button firstButton)
		{
			TabButton_Click(firstButton, EventArgs.Empty);
		}
	}

	private void ScheduleEnsureActivePage()
	{
		if (m_disposing || IsDisposed)
			return;
		if (!IsHandleCreated)
			return;
		EnsureActivePage();
	}

	public override void Bind(IEntityEditorContext context)
	{
		PaintTimingLog.Write("{0} Bind begin handle={1}, visible={2}, parent={3}, {4}", TracePrefix,
			IsHandleCreated, Visible, Parent?.GetType().Name ?? "null", GetInnerDockState());
		if (m_disposing || m_controlHostUnregistering || IsDisposed)
		{
			PaintTimingLog.Write("{0} Bind skipped disposing={1}, unregistering={2}, disposed={3}",
				TracePrefix, m_disposing, m_controlHostUnregistering, IsDisposed);
			return;
		}
		try
		{
			var bindTimer = Stopwatch.StartNew();
			if (m_context != null)
				m_context.Reloaded -= AssetContext_Reloaded;

			m_context = (IAssetEditorContext)context;
			PaintTimingLog.Write("{0} Bind step property editor", TracePrefix);
			m_propertyEditor.Bind(m_context?.EntityContext);
			ConfigurePageBindings(preserveActivePage: false);
			ScheduleEnsureActivePage();
			if (m_context != null)
				m_context.Reloaded += AssetContext_Reloaded;

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

	private void ConfigurePageBindings(bool preserveActivePage)
	{
		ClearOptionalPageBindings();
		m_pageCapabilities = new PageCapabilities(m_context);
		if (m_context != null)
		{
			Control initialPage = preserveActivePage ? GetCurrentSupportedPage() : null;
			if (initialPage == null)
			{
				PageKind initialKind = GetInitialPageKind(m_pageCapabilities);
				initialPage = GetPageControl(initialKind);
			}
			UpdateTabVisibility(m_pageCapabilities);
			EnsureActivePage();
			BindPageForControl(GetCurrentSupportedPage());
		}
		else
		{
			UpdateTabVisibility(m_pageCapabilities);
		}

		if (IsHandleCreated && Visible)
		{
			Invalidate(invalidateChildren: true);
		}
	}

	private Control GetCurrentSupportedPage()
	{
		foreach (Control ctl in m_tabContent.Controls)
		{
			if (ctl.Visible)
				return ctl;
		}
		return null;
	}

	private void ClearOptionalPageBindings()
	{
		m_boundPages.Clear();
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

	private void BindAttachments()
	{
		m_attachmentEditor.Bind(m_context?.AttachmentsContext);
	}

	private void BindCookParameters()
	{
		if (m_context != null && m_context.HasCookParameters)
		{
			m_cookParameterSetEditor.Bind(m_context.CookParametersContext);
			m_cookParameterPropertyEditor.Bind(m_context.CookParametersContext);
			UnsubscribeCookParameterSelection();
			var propertyGridView = m_cookParameterPropertyEditor?.PropertyGridView;
			if (propertyGridView != null)
				propertyGridView.SelectedPropertyChanged += PropertyGridView_SelectedPropertyChanged;
		}
		else
		{
			UnsubscribeCookParameterSelection();
		}
	}

	private void UnsubscribeCookParameterSelection()
	{
		var propertyGridView = m_cookParameterPropertyEditor?.PropertyGridView;
		if (propertyGridView != null)
			propertyGridView.SelectedPropertyChanged -= PropertyGridView_SelectedPropertyChanged;
	}

	private void PropertyGridView_SelectedPropertyChanged(object sender, EventArgs e)
	{
		var adapter = m_context?.CookParametersContext.As<CookParameterSetAdapter>();
		var node = m_cookParameterPropertyEditor.PropertyGridView.SelectedPropertyDescriptor
			.As<FieldPropertyDescriptorBase>()?.GetNode(adapter);
		node?.Parent.As<ISelectionContext>();
		while (node != null && !node.Equals(adapter?.DomNode))
		{
			var parent = node.Parent;
			var selCtx = parent?.As<ISelectionContext>();
			if (node != null)
				selCtx?.Set(node);
			node = parent;
		}
	}

	private void BindGeometrySet()
	{
		if (m_context != null && m_context.HasGeometries)
			m_geometrySetEditor.Bind(m_context.GeometrySetContext);
	}

	private void BindAnimationSet()
	{
		if (m_context != null && m_context.HasAnimations)
			m_animationSetEditor.Bind(m_context.AnimationSetContext);
	}

	private void BindBehaviorSet()
	{
		if (m_context != null && m_context.HasBehaviors)
			m_behaviorSetEditor.Bind(m_context.BehaviorSetContext);
	}

	private void BindParticleEffectSet()
	{
		if (m_context != null && m_context.HasParticleEffects)
			m_particleEffectSetEditor.Bind(m_context.ParticleEffectsContext);
	}

	private void BindSplineEditor()
	{
		if (m_context != null && m_context.HasSplines)
			m_splineSetEditor.Bind(m_context.SplineSetContext);
	}

	private void AssetContext_Reloaded(object sender, EventArgs e)
	{
		if (m_context == null || m_disposing || m_controlHostUnregistering || IsDisposed)
			return;

		try
		{
			ConfigurePageBindings(preserveActivePage: true);
			ScheduleEnsureActivePage();
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
		// TabControl + SplitContainer don't need detach/reattach cycle
		// Just reset splitter distance and ensure active page
		m_splitContainer.SplitterDistance = 200;
		EnsureActivePage();
	}

	private void ResetFailedBinding()
	{
		System.Exception firstCleanupException = null;
		void TryCleanup(Action cleanup)
		{
			try { cleanup(); }
			catch (System.Exception ex) { firstCleanupException ??= ex; }
		}

		TryCleanup(() =>
		{
			if (m_context != null)
				m_context.Reloaded -= AssetContext_Reloaded;
		});
		TryCleanup(ClearOptionalPageBindings);
		TryCleanup(() => m_propertyEditor.Bind(null));
		m_context = null;
		m_pageCapabilities = default;
		TryCleanup(() => UpdateTabVisibility(m_pageCapabilities));

		if (firstCleanupException != null)
			throw firstCleanupException;
	}

	private void SetAssetEditorLayoutState(string value)
	{
		if (string.IsNullOrEmpty(value))
			return;

		if (m_appliedEditorLayoutState == value || GetAssetEditorLayoutState() == value)
		{
			PaintTimingLog.Write("AssetEditorControl: skip identical editor layout");
			m_appliedEditorLayoutState = value;
			return;
		}

		PaintTimingLog.Write("AssetEditorControl: apply editor layout");
		try
		{
			var doc = new XmlDocument();
			doc.LoadXml(value);
			var root = doc.DocumentElement;

			// Restore splitter distance
			if (root.Attributes["splitterDistance"] != null &&
				int.TryParse(root.Attributes["splitterDistance"].Value, out int dist))
			{
				m_splitContainer.SplitterDistance = dist;
			}

			// Restore active tab
			if (root.Attributes["activeTab"] != null)
			{
				string activeTabName = root.Attributes["activeTab"].Value;
				foreach (var kv in m_tabButtons)
				{
					if (kv.Value.Text == activeTabName)
					{
						TabButton_Click(kv.Value, EventArgs.Empty);
						break;
					}
				}
			}
		}
		catch
		{
		}

		m_appliedEditorLayoutState = value;
		EnsureActivePage();
	}

	private string GetAssetEditorLayoutState()
	{
		try
		{
			var doc = new XmlDocument();
			var dockPanel = doc.CreateElement("DockPanel");
			doc.AppendChild(dockPanel);

			var splitterAttr = doc.CreateAttribute("splitterDistance");
			splitterAttr.Value = m_splitContainer.SplitterDistance.ToString();
			dockPanel.Attributes.Append(splitterAttr);

			Button selectedButton = null;
			foreach (var kv in m_tabButtons)
			{
				if (kv.Value.BackColor == Color.FromArgb(85, 85, 85))
				{
					selectedButton = kv.Value;
					break;
				}
			}

			if (selectedButton != null)
			{
				var tabAttr = doc.CreateAttribute("activeTab");
				tabAttr.Value = selectedButton.Text;
				dockPanel.Attributes.Append(tabAttr);
			}

			return doc.OuterXml;
		}
		catch
		{
			return string.Empty;
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		PaintTimingLog.Write("{0} HandleCreated {1}", TracePrefix, GetInnerDockState());
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		PaintTimingLog.Write("{0} VisibleChanged visible={1}, {2}", TracePrefix, Visible, GetInnerDockState());
		if (Visible)
		{
			EnsureActivePage();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PaintTimingLog.Write("{0} Paint {1}", TracePrefix, GetInnerDockState());
		base.OnPaint(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_disposing = true;
			UnsubscribeCookParameterSelection();
			if (m_context != null)
			{
				m_context.Reloaded -= AssetContext_Reloaded;
				m_context = null;
			}
			if (m_themeService != null)
			{
				m_themeService.ThemeChanged -= ThemeService_ThemeChanged;
				m_themeService = null;
			}
			if (m_tabContainer != null)
			{
				m_tabContainer.Dispose();
				m_tabContainer = null;
			}
			if (m_splitContainer != null)
			{
				m_splitContainer.Dispose();
				m_splitContainer = null;
			}
			if (components != null)
				components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
	}
}