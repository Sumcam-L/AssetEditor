using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking.Win32;

namespace WeifenLuo.WinFormsUI.Docking;

public class DockContentHandler : IDisposable, IDockDragSource, IDragSource
{
	private Form m_form;

	private double m_autoHidePortion = 0.25;

	private bool m_closeButton = true;

	private bool m_closeButtonVisible = true;

	private DockAreas m_allowedAreas = DockAreas.Float | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom | DockAreas.Document;

	private DockState m_dockState;

	private DockPanel m_dockPanel;

	private bool m_isHidden = true;

	private bool m_showTabText = true;

	private string m_tabText;

	private DockState m_visibleState;

	private bool m_isFloat;

	private DockPane m_panelPane;

	private DockPane m_floatPane;

	private int m_countSetDockState;

	private DockState m_showHint;

	private bool m_isActivated;

	private IntPtr m_activeWindowHandle = IntPtr.Zero;

	private DockPaneStripBase.Tab m_tab;

	private IDisposable m_autoHideTab;

	private static readonly object DockStateChangedEvent;

	private bool m_flagClipWindow;

	private ContextMenuStrip m_tabPageContextMenuStrip;

	public Form Form => m_form;

	public IDockContent Content => Form as IDockContent;

	public Func<bool> IsDirtyDocument { get; set; } = () => false;

	public Func<bool> IsReadOnlyDocument { get; set; } = () => false;

	public IDockContent PreviousActive { get; internal set; }

	public IDockContent NextActive { get; internal set; }

	private EventHandlerList Events { get; }

	public bool AllowEndUserDocking { get; set; } = true;

	internal bool SuspendAutoHidePortionUpdates { get; set; }

	public double AutoHidePortion
	{
		get
		{
			return m_autoHidePortion;
		}
		set
		{
			if (value <= 0.0)
			{
				throw new ArgumentOutOfRangeException(Strings.DockContentHandler_AutoHidePortion_OutOfRange);
			}
			if (!SuspendAutoHidePortionUpdates && !(Math.Abs(m_autoHidePortion - value) < double.Epsilon))
			{
				m_autoHidePortion = value;
				if (DockPanel != null && DockPanel.ActiveAutoHideContent == Content)
				{
					DockPanel.PerformLayout();
				}
			}
		}
	}

	public bool CloseButton
	{
		get
		{
			return m_closeButton;
		}
		set
		{
			if (m_closeButton != value)
			{
				m_closeButton = value;
				if (IsActiveContentHandler)
				{
					Pane.RefreshChanges();
				}
			}
		}
	}

	public bool CloseButtonVisible
	{
		get
		{
			return m_closeButtonVisible;
		}
		set
		{
			if (m_closeButtonVisible != value)
			{
				m_closeButtonVisible = value;
				if (IsActiveContentHandler)
				{
					Pane.RefreshChanges();
				}
			}
		}
	}

	private bool IsActiveContentHandler
	{
		get
		{
			if (Pane != null && Pane.ActiveContent != null)
			{
				return Pane.ActiveContent.DockHandler == this;
			}
			return false;
		}
	}

	private DockState DefaultDockState
	{
		get
		{
			if (ShowHint != DockState.Unknown && ShowHint != DockState.Hidden)
			{
				return ShowHint;
			}
			if ((DockAreas & DockAreas.Document) != 0)
			{
				return DockState.Document;
			}
			if ((DockAreas & DockAreas.DockRight) != 0)
			{
				return DockState.DockRight;
			}
			if ((DockAreas & DockAreas.DockLeft) != 0)
			{
				return DockState.DockLeft;
			}
			if ((DockAreas & DockAreas.DockBottom) != 0)
			{
				return DockState.DockBottom;
			}
			if ((DockAreas & DockAreas.DockTop) != 0)
			{
				return DockState.DockTop;
			}
			return DockState.Unknown;
		}
	}

	private DockState DefaultShowState
	{
		get
		{
			if (ShowHint != DockState.Unknown)
			{
				return ShowHint;
			}
			if ((DockAreas & DockAreas.Document) != 0)
			{
				return DockState.Document;
			}
			if ((DockAreas & DockAreas.DockRight) != 0)
			{
				return DockState.DockRight;
			}
			if ((DockAreas & DockAreas.DockLeft) != 0)
			{
				return DockState.DockLeft;
			}
			if ((DockAreas & DockAreas.DockBottom) != 0)
			{
				return DockState.DockBottom;
			}
			if ((DockAreas & DockAreas.DockTop) != 0)
			{
				return DockState.DockTop;
			}
			if ((DockAreas & DockAreas.Float) != 0)
			{
				return DockState.Float;
			}
			return DockState.Unknown;
		}
	}

	public DockAreas DockAreas
	{
		get
		{
			return m_allowedAreas;
		}
		set
		{
			if (m_allowedAreas != value)
			{
				if (!DockHelper.IsDockStateValid(DockState, value))
				{
					throw new InvalidOperationException(Strings.DockContentHandler_DockAreas_InvalidValue);
				}
				m_allowedAreas = value;
				if (!DockHelper.IsDockStateValid(ShowHint, m_allowedAreas))
				{
					ShowHint = DockState.Unknown;
				}
			}
		}
	}

	public DockState DockState
	{
		get
		{
			return m_dockState;
		}
		set
		{
			if (m_dockState != value)
			{
				DockPanel.SuspendLayout(allWindows: true);
				if (value == DockState.Hidden)
				{
					IsHidden = true;
				}
				else
				{
					SetDockState(isHidden: false, value, Pane);
				}
				DockPanel.ResumeLayout(performLayout: true, allWindows: true);
			}
		}
	}

	public DockPanel DockPanel
	{
		get
		{
			return m_dockPanel;
		}
		set
		{
			if (m_dockPanel == value)
			{
				return;
			}
			Pane = null;
			if (m_dockPanel != null)
			{
				m_dockPanel.RemoveContent(Content);
			}
			if (m_tab != null)
			{
				m_tab.Dispose();
				m_tab = null;
			}
			if (m_autoHideTab != null)
			{
				m_autoHideTab.Dispose();
				m_autoHideTab = null;
			}
			m_dockPanel = value;
			if (m_dockPanel != null)
			{
				m_dockPanel.AddContent(Content);
				Form.TopLevel = false;
				Form.FormBorderStyle = FormBorderStyle.None;
				Form.ShowInTaskbar = false;
				Form.WindowState = FormWindowState.Normal;
				Content.ApplyTheme();
				if (!Win32Helper.IsRunningOnMono)
				{
					NativeMethods.SetWindowPos(Form.Handle, IntPtr.Zero, 0, 0, 0, 0, FlagsSetWindowPos.SWP_NOSIZE | FlagsSetWindowPos.SWP_NOMOVE | FlagsSetWindowPos.SWP_NOZORDER | FlagsSetWindowPos.SWP_NOACTIVATE | FlagsSetWindowPos.SWP_FRAMECHANGED | FlagsSetWindowPos.SWP_NOOWNERZORDER);
				}
			}
		}
	}

	public Icon Icon => Form.Icon;

	public DockPane Pane
	{
		get
		{
			if (!IsFloat)
			{
				return PanelPane;
			}
			return FloatPane;
		}
		set
		{
			if (Pane != value)
			{
				DockPanel.SuspendLayout(allWindows: true);
				DockPane pane = Pane;
				SuspendSetDockState();
				FloatPane = ((value == null) ? null : (value.IsFloat ? value : FloatPane));
				PanelPane = ((value == null) ? null : (value.IsFloat ? PanelPane : value));
				ResumeSetDockState(IsHidden, value?.DockState ?? DockState.Unknown, pane);
				DockPanel.ResumeLayout(performLayout: true, allWindows: true);
			}
		}
	}

	public bool IsHidden
	{
		get
		{
			return m_isHidden;
		}
		set
		{
			if (m_isHidden != value)
			{
				SetDockState(value, VisibleState, Pane);
			}
		}
	}

	public bool ShowTabText
	{
		get
		{
			return m_showTabText;
		}
		set
		{
			m_showTabText = value;
		}
	}

	public string TabText
	{
		get
		{
			if (m_tabText != null && !(m_tabText == ""))
			{
				return m_tabText;
			}
			return Form.Text;
		}
		set
		{
			if (!(m_tabText == value))
			{
				m_tabText = value;
				if (Pane != null)
				{
					Pane.RefreshChanges();
				}
			}
		}
	}

	public DockState VisibleState
	{
		get
		{
			return m_visibleState;
		}
		set
		{
			if (m_visibleState != value)
			{
				SetDockState(IsHidden, value, Pane);
			}
		}
	}

	public bool IsFloat
	{
		get
		{
			return m_isFloat;
		}
		set
		{
			if (m_isFloat != value)
			{
				DockState dockState = CheckDockState(value);
				if (dockState == DockState.Unknown)
				{
					throw new InvalidOperationException(Strings.DockContentHandler_IsFloat_InvalidValue);
				}
				SetDockState(IsHidden, dockState, Pane);
			}
		}
	}

	public DockPane PanelPane
	{
		get
		{
			return m_panelPane;
		}
		set
		{
			if (m_panelPane != value)
			{
				if (value != null && (value.IsFloat || value.DockPanel != DockPanel))
				{
					throw new InvalidOperationException(Strings.DockContentHandler_DockPane_InvalidValue);
				}
				DockPane pane = Pane;
				if (m_panelPane != null)
				{
					RemoveFromPane(m_panelPane);
				}
				m_panelPane = value;
				if (m_panelPane != null)
				{
					m_panelPane.AddContent(Content);
					SetDockState(IsHidden, IsFloat ? DockState.Float : m_panelPane.DockState, pane);
				}
				else
				{
					SetDockState(IsHidden, DockState.Unknown, pane);
				}
			}
		}
	}

	public DockPane FloatPane
	{
		get
		{
			return m_floatPane;
		}
		set
		{
			if (m_floatPane != value)
			{
				if (value != null && (!value.IsFloat || value.DockPanel != DockPanel))
				{
					throw new InvalidOperationException(Strings.DockContentHandler_FloatPane_InvalidValue);
				}
				DockPane pane = Pane;
				if (m_floatPane != null)
				{
					RemoveFromPane(m_floatPane);
				}
				m_floatPane = value;
				if (m_floatPane != null)
				{
					m_floatPane.AddContent(Content);
					SetDockState(IsHidden, IsFloat ? DockState.Float : VisibleState, pane);
				}
				else
				{
					SetDockState(IsHidden, DockState.Unknown, pane);
				}
			}
		}
	}

	internal bool IsSuspendSetDockState => m_countSetDockState != 0;

	internal string PersistString
	{
		get
		{
			if (GetPersistStringCallback != null)
			{
				return GetPersistStringCallback();
			}
			return Form.GetType().ToString();
		}
	}

	public GetPersistStringCallback GetPersistStringCallback { get; set; }

	public bool HideOnClose { get; set; }

	public DockState ShowHint
	{
		get
		{
			return m_showHint;
		}
		set
		{
			if (!DockHelper.IsDockStateValid(value, DockAreas))
			{
				throw new InvalidOperationException(Strings.DockContentHandler_ShowHint_InvalidValue);
			}
			if (m_showHint != value)
			{
				m_showHint = value;
			}
		}
	}

	public bool IsActivated
	{
		get
		{
			return m_isActivated;
		}
		internal set
		{
			if (m_isActivated != value)
			{
				m_isActivated = value;
			}
		}
	}

	public ContextMenu TabPageContextMenu { get; set; }

	public string ToolTipText { get; set; }

	internal IntPtr ActiveWindowHandle
	{
		get
		{
			return m_activeWindowHandle;
		}
		set
		{
			m_activeWindowHandle = value;
		}
	}

	internal IDisposable AutoHideTab
	{
		get
		{
			return m_autoHideTab;
		}
		set
		{
			m_autoHideTab = value;
		}
	}

	internal bool FlagClipWindow
	{
		get
		{
			return m_flagClipWindow;
		}
		set
		{
			if (m_flagClipWindow != value)
			{
				m_flagClipWindow = value;
				if (m_flagClipWindow)
				{
					Form.Region = new Region(Rectangle.Empty);
				}
				else
				{
					Form.Region = null;
				}
			}
		}
	}

	public ContextMenuStrip TabPageContextMenuStrip
	{
		get
		{
			return m_tabPageContextMenuStrip;
		}
		set
		{
			if (value != m_tabPageContextMenuStrip)
			{
				m_tabPageContextMenuStrip = value;
				ApplyTheme();
			}
		}
	}

	Control IDragSource.DragControl => Form;

	public event EventHandler DockStateChanged
	{
		add
		{
			Events.AddHandler(DockStateChangedEvent, value);
		}
		remove
		{
			Events.RemoveHandler(DockStateChangedEvent, value);
		}
	}

	public DockContentHandler(Form form)
		: this(form, null)
	{
	}

	public DockContentHandler(Form form, GetPersistStringCallback getPersistStringCallback)
	{
		if (!(form is IDockContent))
		{
			throw new ArgumentException(Strings.DockContent_Constructor_InvalidForm, "form");
		}
		m_form = form;
		GetPersistStringCallback = getPersistStringCallback;
		Events = new EventHandlerList();
		Form.Disposed += Form_Disposed;
		Form.TextChanged += Form_TextChanged;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			DockPanel = null;
			if (m_autoHideTab != null)
			{
				m_autoHideTab.Dispose();
			}
			if (m_tab != null)
			{
				m_tab.Dispose();
			}
			Form.Disposed -= Form_Disposed;
			Form.TextChanged -= Form_TextChanged;
			Events.Dispose();
		}
	}

	public DockState CheckDockState(bool isFloat)
	{
		DockState dockState;
		if (isFloat)
		{
			dockState = (IsDockStateValid(DockState.Float) ? DockState.Float : DockState.Unknown);
		}
		else
		{
			dockState = ((PanelPane != null) ? PanelPane.DockState : DefaultDockState);
			if (dockState != DockState.Unknown && !IsDockStateValid(dockState))
			{
				dockState = DockState.Unknown;
			}
		}
		return dockState;
	}

	private void RemoveFromPane(DockPane pane)
	{
		pane.RemoveContent(Content);
		SetPane(null);
		if (pane.Contents.Count == 0)
		{
			pane.Dispose();
		}
	}

	private void SuspendSetDockState()
	{
		m_countSetDockState++;
	}

	private void ResumeSetDockState()
	{
		m_countSetDockState--;
		if (m_countSetDockState < 0)
		{
			m_countSetDockState = 0;
		}
	}

	private void ResumeSetDockState(bool isHidden, DockState visibleState, DockPane oldPane)
	{
		ResumeSetDockState();
		SetDockState(isHidden, visibleState, oldPane);
	}

	internal void SetDockState(bool isHidden, DockState visibleState, DockPane oldPane)
	{
		if (IsSuspendSetDockState)
		{
			return;
		}
		DockTrace.Write(Content, "SetDockState begin hidden={1}->{2}, visible={3}->{4}, dock={5}, oldPane={6:X}, pane={7:X}", m_isHidden, isHidden, m_visibleState, visibleState, m_dockState, oldPane?.GetHashCode() ?? 0, Pane?.GetHashCode() ?? 0);
		DockTrace.WriteUnknownTransition(Content, m_visibleState, visibleState, m_isHidden, isHidden);
		if (DockPanel == null && visibleState != DockState.Unknown)
		{
			throw new InvalidOperationException(Strings.DockContentHandler_SetDockState_NullPanel);
		}
		if (visibleState == DockState.Hidden || (visibleState != DockState.Unknown && !IsDockStateValid(visibleState)))
		{
			throw new InvalidOperationException(Strings.DockContentHandler_SetDockState_InvalidState);
		}
		DockPanel dockPanel = DockPanel;
		dockPanel?.SuspendLayout(allWindows: true);
		SuspendSetDockState();
		DockState dockState = DockState;
		if (m_isHidden != isHidden || dockState == DockState.Unknown)
		{
			m_isHidden = isHidden;
		}
		m_visibleState = visibleState;
		m_dockState = (isHidden ? DockState.Hidden : visibleState);
		bool flag = DockState == DockState.Hidden || DockState == DockState.Unknown || DockHelper.IsDockStateAutoHide(DockState);
		bool? enableContentOrderFix = PatchController.EnableContentOrderFix;
		bool flag2 = true;
		if (enableContentOrderFix == true == flag2 && enableContentOrderFix.HasValue && dockState != DockState && flag && !Win32Helper.IsRunningOnMono)
		{
			DockPanel.ContentFocusManager.RemoveFromList(Content);
		}
		if (visibleState == DockState.Unknown)
		{
			Pane = null;
		}
		else
		{
			m_isFloat = m_visibleState == DockState.Float;
			if (Pane == null)
			{
				Pane = DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, visibleState, show: true);
			}
			else if (Pane.DockState != visibleState)
			{
				if (Pane.Contents.Count == 1)
				{
					Pane.SetDockState(visibleState);
				}
				else
				{
					Pane = DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, visibleState, show: true);
				}
			}
		}
		if (Form.ContainsFocus && (DockState == DockState.Hidden || DockState == DockState.Unknown) && !Win32Helper.IsRunningOnMono)
		{
			DockPanel.ContentFocusManager.GiveUpFocus(Content);
		}
		SetPaneAndVisible(Pane);
		if (oldPane != null && !oldPane.IsDisposed && dockState == oldPane.DockState)
		{
			RefreshDockPane(oldPane);
		}
		if (Pane != null && DockState == Pane.DockState && (Pane != oldPane || (Pane == oldPane && dockState != oldPane.DockState)) && (Pane.DockWindow == null || Pane.DockWindow.Visible || Pane.IsHidden) && !Pane.IsAutoHide)
		{
			RefreshDockPane(Pane);
		}
		if (dockState != DockState)
		{
			if (PatchController.EnableContentOrderFix == true)
			{
				if (!flag && !Win32Helper.IsRunningOnMono)
				{
					DockPanel.ContentFocusManager.AddToList(Content);
				}
			}
			else if (DockState == DockState.Hidden || DockState == DockState.Unknown || DockHelper.IsDockStateAutoHide(DockState))
			{
				if (!Win32Helper.IsRunningOnMono)
				{
					DockPanel.ContentFocusManager.RemoveFromList(Content);
				}
			}
			else if (!Win32Helper.IsRunningOnMono)
			{
				DockPanel.ContentFocusManager.AddToList(Content);
			}
			ResetAutoHidePortion(dockState, DockState);
			OnDockStateChanged(EventArgs.Empty);
		}
		ResumeSetDockState();
		dockPanel?.ResumeLayout(performLayout: true, allWindows: true);
		DockTrace.Write(Content, "SetDockState end hidden={1}, visible={2}, dock={3}, pane={4:X}, formVisible={5}, parent={6}", m_isHidden, m_visibleState, m_dockState, Pane?.GetHashCode() ?? 0, Form.Visible, Form.Parent?.GetType().Name ?? "null");
	}

	private void ResetAutoHidePortion(DockState oldState, DockState newState)
	{
		if (oldState != newState && DockHelper.ToggleAutoHideState(oldState) != newState)
		{
			switch (newState)
			{
			case DockState.DockTopAutoHide:
			case DockState.DockTop:
				AutoHidePortion = DockPanel.DockTopPortion;
				break;
			case DockState.DockLeftAutoHide:
			case DockState.DockLeft:
				AutoHidePortion = DockPanel.DockLeftPortion;
				break;
			case DockState.DockBottomAutoHide:
			case DockState.DockBottom:
				AutoHidePortion = DockPanel.DockBottomPortion;
				break;
			case DockState.DockRightAutoHide:
			case DockState.DockRight:
				AutoHidePortion = DockPanel.DockRightPortion;
				break;
			case DockState.Document:
				break;
			}
		}
	}

	private static void RefreshDockPane(DockPane pane)
	{
		pane.RefreshChanges();
		pane.ValidateActiveContent();
	}

	public bool IsDockStateValid(DockState dockState)
	{
		if (DockPanel != null && dockState == DockState.Document && DockPanel.DocumentStyle == DocumentStyle.SystemMdi)
		{
			return false;
		}
		return DockHelper.IsDockStateValid(dockState, DockAreas);
	}

	public void Activate()
	{
		DockTrace.Write(Content, "Activate begin hidden={1}, dock={2}, pane={3:X}, paneActive={4}", IsHidden, DockState, Pane?.GetHashCode() ?? 0, Pane?.ActiveContent?.DockHandler?.Form?.Name ?? "null");
		if (DockPanel == null)
		{
			Form.Activate();
			return;
		}
		if (Pane == null)
		{
			Show(DockPanel);
			return;
		}
		IsHidden = false;
		Pane.ActiveContent = Content;
		if (DockState == DockState.Document && DockPanel.DocumentStyle == DocumentStyle.SystemMdi)
		{
			Form.Activate();
		}
		else if (DockHelper.IsDockStateAutoHide(DockState) && DockPanel.ActiveAutoHideContent != Content)
		{
			DockPanel.ActiveAutoHideContent = null;
		}
		else if (!Form.ContainsFocus && !Win32Helper.IsRunningOnMono)
		{
			DockPanel.ContentFocusManager.Activate(Content);
		}
		DockTrace.Write(Content, "Activate end hidden={1}, dock={2}, pane={3:X}, paneActive={4}, formVisible={5}", IsHidden, DockState, Pane?.GetHashCode() ?? 0, Pane?.ActiveContent?.DockHandler?.Form?.Name ?? "null", Form.Visible);
	}

	public void GiveUpFocus()
	{
		if (!Win32Helper.IsRunningOnMono)
		{
			DockPanel.ContentFocusManager.GiveUpFocus(Content);
		}
	}

	public void Hide()
	{
		DockTrace.Write(Content, "Hide called hidden={1}, dock={2}, pane={3:X}, active={4}", IsHidden, DockState, Pane?.GetHashCode() ?? 0, Pane?.ActiveContent?.DockHandler?.Form?.Name ?? "null");
		IsHidden = true;
	}

	internal void SetPaneAndVisible(DockPane pane)
	{
		SetPane(pane);
		SetVisible();
	}

	private void SetPane(DockPane pane)
	{
		if (pane != null && pane.DockState == DockState.Document && DockPanel.DocumentStyle == DocumentStyle.DockingMdi)
		{
			if (Form.Parent is DockPane)
			{
				SetParent(null);
			}
			if (Form.MdiParent != DockPanel.ParentForm)
			{
				FlagClipWindow = true;
				bool? enableFontInheritanceFix = PatchController.EnableFontInheritanceFix;
				bool flag = true;
				if (enableFontInheritanceFix == true == flag && enableFontInheritanceFix.HasValue && Form.Font == Control.DefaultFont)
				{
					Form.MdiParent = DockPanel.ParentForm;
					Form.Font = DockPanel.Font;
				}
				else
				{
					Form.MdiParent = DockPanel.ParentForm;
				}
			}
		}
		else
		{
			FlagClipWindow = true;
			if (Form.MdiParent != null)
			{
				Form.MdiParent = null;
			}
			if (Form.TopLevel)
			{
				Form.TopLevel = false;
			}
			SetParent(pane);
		}
	}

	internal void SetVisible()
	{
		bool flag = !IsHidden && ((Pane != null && Pane.DockState == DockState.Document && DockPanel.DocumentStyle == DocumentStyle.DockingMdi) || (Pane != null && Pane.ActiveContent == Content) || ((Pane == null || Pane.ActiveContent == Content) && Form.Visible));
		if (Form.Visible != flag)
		{
			DockTrace.Write(Content, "SetVisible formVisible={1}->{2}, hidden={3}, dock={4}, pane={5:X}, paneActive={6}, parent={7}", Form.Visible, flag, IsHidden, DockState, Pane?.GetHashCode() ?? 0, Pane?.ActiveContent?.DockHandler?.Form?.Name ?? "null", Form.Parent?.GetType().Name ?? "null");
			Form.Visible = flag;
		}
	}

	private void SetParent(Control value)
	{
		if (Form.Parent == value)
		{
			return;
		}
		bool flag = false;
		if (Form.ContainsFocus)
		{
			if (value == null && !IsFloat)
			{
				if (!Win32Helper.IsRunningOnMono)
				{
					DockPanel.ContentFocusManager.GiveUpFocus(Content);
				}
			}
			else
			{
				DockPanel.SaveFocus();
				flag = true;
			}
		}
		bool flag2 = value != Form.Parent;
		Form.Parent = value;
		if (PatchController.EnableMainWindowFocusLostFix == true && flag2)
		{
			Form.Focus();
		}
		if (flag && !Win32Helper.IsRunningOnMono)
		{
			Activate();
		}
	}

	public void Show()
	{
		DockTrace.Write(Content, "Show called hidden={1}, dock={2}, pane={3:X}", IsHidden, DockState, Pane?.GetHashCode() ?? 0);
		if (DockPanel == null)
		{
			Form.Show();
		}
		else
		{
			Show(DockPanel);
		}
	}

	public void Show(DockPanel dockPanel)
	{
		DockTrace.Write(Content, "Show(panel) begin target={1:X}, currentPanel={2:X}, hidden={3}, dock={4}", dockPanel?.GetHashCode() ?? 0, DockPanel?.GetHashCode() ?? 0, IsHidden, DockState);
		if (dockPanel == null)
		{
			throw new ArgumentNullException(Strings.DockContentHandler_Show_NullDockPanel);
		}
		if (DockState == DockState.Unknown)
		{
			Show(dockPanel, DefaultShowState);
		}
		else if (DockPanel != dockPanel)
		{
			Show(dockPanel, (DockState == DockState.Hidden) ? m_visibleState : DockState);
		}
		else
		{
			Activate();
		}
	}

	public void Show(DockPanel dockPanel, DockState dockState)
	{
		Show(dockPanel, dockState, activate: true);
	}

	public void Show(DockPanel dockPanel, DockState dockState, bool activate)
	{
		if (dockPanel == null)
		{
			throw new ArgumentNullException(Strings.DockContentHandler_Show_NullDockPanel);
		}
		if (dockState == DockState.Unknown || dockState == DockState.Hidden)
		{
			throw new ArgumentException(Strings.DockContentHandler_Show_InvalidDockState);
		}
		dockPanel.SuspendLayout(allWindows: true);
		DockPanel = dockPanel;
		if (dockState == DockState.Float)
		{
			if (FloatPane == null)
			{
				Pane = DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, DockState.Float, show: true);
			}
		}
		else if (PanelPane == null)
		{
			DockPane dockPane = null;
			foreach (DockPane pane in DockPanel.Panes)
			{
				if (pane.DockState == dockState)
				{
					if (dockPane == null || pane.IsActivated)
					{
						dockPane = pane;
					}
					if (pane.IsActivated)
					{
						break;
					}
				}
			}
			if (dockPane == null)
			{
				Pane = DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, dockState, show: true);
			}
			else
			{
				Pane = dockPane;
			}
		}
		DockState = dockState;
		dockPanel.ResumeLayout(performLayout: true, allWindows: true);
		if (activate)
		{
			Activate();
		}
	}

	public void Show(DockPanel dockPanel, Rectangle floatWindowBounds)
	{
		if (dockPanel == null)
		{
			throw new ArgumentNullException(Strings.DockContentHandler_Show_NullDockPanel);
		}
		dockPanel.SuspendLayout(allWindows: true);
		DockPanel = dockPanel;
		if (FloatPane == null)
		{
			IsHidden = true;
			FloatPane = DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, DockState.Float, show: false);
			FloatPane.FloatWindow.StartPosition = FormStartPosition.Manual;
		}
		FloatPane.FloatWindow.Bounds = floatWindowBounds;
		Show(dockPanel, DockState.Float);
		Activate();
		dockPanel.ResumeLayout(performLayout: true, allWindows: true);
	}

	public void Show(DockPane pane, IDockContent beforeContent)
	{
		if (pane == null)
		{
			throw new ArgumentNullException(Strings.DockContentHandler_Show_NullPane);
		}
		if (beforeContent != null && pane.Contents.IndexOf(beforeContent) == -1)
		{
			throw new ArgumentException(Strings.DockContentHandler_Show_InvalidBeforeContent);
		}
		pane.DockPanel.SuspendLayout(allWindows: true);
		DockPanel = pane.DockPanel;
		Pane = pane;
		pane.SetContentIndex(Content, pane.Contents.IndexOf(beforeContent));
		Show();
		pane.DockPanel.ResumeLayout(performLayout: true, allWindows: true);
	}

	public void Show(DockPane previousPane, DockAlignment alignment, double proportion)
	{
		if (previousPane == null)
		{
			throw new ArgumentException(Strings.DockContentHandler_Show_InvalidPrevPane);
		}
		if (DockHelper.IsDockStateAutoHide(previousPane.DockState))
		{
			throw new ArgumentException(Strings.DockContentHandler_Show_InvalidPrevPane);
		}
		previousPane.DockPanel.SuspendLayout(allWindows: true);
		DockPanel = previousPane.DockPanel;
		DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, previousPane, alignment, proportion, show: true);
		Show();
		previousPane.DockPanel.ResumeLayout(performLayout: true, allWindows: true);
	}

	public void Close()
	{
		DockPanel dockPanel = DockPanel;
		dockPanel?.SuspendLayout(allWindows: true);
		Form.Close();
		dockPanel?.ResumeLayout(performLayout: true, allWindows: true);
	}

	internal DockPaneStripBase.Tab GetTab(DockPaneStripBase dockPaneStrip)
	{
		if (m_tab == null)
		{
			m_tab = dockPaneStrip.CreateTab(Content);
		}
		return m_tab;
	}

	protected virtual void OnDockStateChanged(EventArgs e)
	{
		((EventHandler)Events[DockStateChangedEvent])?.Invoke(this, e);
	}

	private void Form_Disposed(object sender, EventArgs e)
	{
		Dispose();
	}

	private void Form_TextChanged(object sender, EventArgs e)
	{
		if (DockHelper.IsDockStateAutoHide(DockState))
		{
			DockPanel.RefreshAutoHideStrip();
		}
		else if (Pane != null)
		{
			if (Pane.FloatWindow != null)
			{
				Pane.FloatWindow.SetText();
			}
			Pane.RefreshChanges();
		}
	}

	public void ApplyTheme()
	{
		if (m_tabPageContextMenuStrip != null && DockPanel != null)
		{
			DockPanel.Theme.ApplyTo(m_tabPageContextMenuStrip);
		}
	}

	bool IDockDragSource.CanDockTo(DockPane pane)
	{
		if (!IsDockStateValid(pane.DockState))
		{
			return false;
		}
		if (Pane == pane && pane.DisplayingContents.Count == 1)
		{
			return false;
		}
		return true;
	}

	Rectangle IDockDragSource.BeginDrag(Point ptMouse)
	{
		DockPane floatPane = FloatPane;
		Size size = ((DockState != DockState.Float && floatPane != null && floatPane.FloatWindow.NestedPanes.Count == 1) ? floatPane.FloatWindow.Size : DockPanel.DefaultFloatWindowSize);
		Rectangle clientRectangle = Pane.ClientRectangle;
		Point p;
		if (DockState == DockState.Document)
		{
			p = ((Pane.DockPanel.DocumentTabStripLocation != DocumentTabStripLocation.Bottom) ? new Point(clientRectangle.Left, clientRectangle.Top) : new Point(clientRectangle.Left, clientRectangle.Bottom - size.Height));
		}
		else
		{
			p = new Point(clientRectangle.Left, clientRectangle.Bottom);
			p.Y -= size.Height;
		}
		p = Pane.PointToScreen(p);
		if (ptMouse.X > p.X + size.Width)
		{
			p.X += ptMouse.X - (p.X + size.Width) + DockPanel.Theme.Measures.SplitterSize;
		}
		return new Rectangle(p, size);
	}

	void IDockDragSource.EndDrag()
	{
	}

	public void FloatAt(Rectangle floatWindowBounds)
	{
		DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, floatWindowBounds, show: true);
	}

	public void DockTo(DockPane pane, DockStyle dockStyle, int contentIndex)
	{
		if (dockStyle == DockStyle.Fill)
		{
			bool flag = Pane == pane;
			if (!flag)
			{
				Pane = pane;
			}
			int num = 0;
			int num2 = 0;
			while (num <= contentIndex && num2 < Pane.Contents.Count)
			{
				if (Pane.Contents[num2] is DockContent { IsHidden: false })
				{
					num++;
				}
				num2++;
			}
			contentIndex = Math.Min(Math.Max(0, num2 - 1), Pane.Contents.Count - 1);
			if (contentIndex == -1 || !flag)
			{
				pane.SetContentIndex(Content, contentIndex);
				return;
			}
			DockContentCollection contents = pane.Contents;
			int num3 = contents.IndexOf(Content);
			int num4 = contentIndex;
			if (num3 < num4)
			{
				num4++;
				if (num4 > contents.Count - 1)
				{
					num4 = -1;
				}
			}
			pane.SetContentIndex(Content, num4);
		}
		else
		{
			DockPane dockPane = DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, pane.DockState, show: true);
			INestedPanesContainer nestedPanesContainer = pane.NestedPanesContainer;
			switch (dockStyle)
			{
			case DockStyle.Left:
				dockPane.DockTo(nestedPanesContainer, pane, DockAlignment.Left, 0.5);
				break;
			case DockStyle.Right:
				dockPane.DockTo(nestedPanesContainer, pane, DockAlignment.Right, 0.5);
				break;
			case DockStyle.Top:
				dockPane.DockTo(nestedPanesContainer, pane, DockAlignment.Top, 0.5);
				break;
			case DockStyle.Bottom:
				dockPane.DockTo(nestedPanesContainer, pane, DockAlignment.Bottom, 0.5);
				break;
			}
			dockPane.DockState = pane.DockState;
		}
	}

	public void DockTo(DockPanel panel, DockStyle dockStyle)
	{
		if (panel != DockPanel)
		{
			throw new ArgumentException(Strings.IDockDragSource_DockTo_InvalidPanel, "panel");
		}
		switch (dockStyle)
		{
		case DockStyle.Top:
			DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, DockState.DockTop, show: true);
			break;
		case DockStyle.Bottom:
			DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, DockState.DockBottom, show: true);
			break;
		case DockStyle.Left:
			DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, DockState.DockLeft, show: true);
			break;
		case DockStyle.Right:
			DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, DockState.DockRight, show: true);
			break;
		case DockStyle.Fill:
			DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, DockState.Document, show: true);
			break;
		}
	}

	static DockContentHandler()
	{
		DockStateChangedEvent = new object();
	}
}
