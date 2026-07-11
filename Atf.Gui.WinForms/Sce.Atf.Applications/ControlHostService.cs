using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.PropertyEditing;
using WeifenLuo.WinFormsUI.Docking;

namespace Sce.Atf.Applications;

[Export(typeof(IControlHostService))]
[Export(typeof(IControlRegistry))]
[Export(typeof(IInitializable))]
[Export(typeof(IDockStateProvider))]
[Export(typeof(ControlHostService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ControlHostService : IControlHostService, IControlRegistry, ICommandClient, IPartImportsSatisfiedNotification, IInitializable, IDockStateProvider, IDisposable
{
	[Flags]
	public enum CommandRegister
	{
		None = 0,
		WindowTileHorizontal = 1,
		WindowTileVertical = 2,
		WindowTileTabbed = 4,
		UILock = 8,
		Default = 0xF
	}

	private class DockPanel : WeifenLuo.WinFormsUI.Docking.DockPanel
	{
		public DockColors DockColors
		{
			get
			{
				return new DockColors
				{
					AutoHideDockStripGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.AutoHideStripSkin.DockStripGradient),
					AutoHideTabGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.AutoHideStripSkin.TabGradient),
					DocumentActiveTabGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient),
					DocumentInactiveTabGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient),
					DocumentDockStripGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.DockPaneStripSkin.DocumentGradient.DockStripGradient),
					ToolWindowActiveTabGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient),
					ToolWindowInactiveTabGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient),
					ToolWindowActiveCaptionGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient),
					ToolWindowInactiveCaptionGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient),
					ToolWindowDockStripGradient = GetControlGradientFromDockPanelGradient(base.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient)
				};
			}
			set
			{
				base.Theme.Skin.AutoHideStripSkin = new AutoHideStripSkin
				{
					DockStripGradient = GetTabGradientFromControlGradient(value.AutoHideDockStripGradient),
					TabGradient = GetTabGradientFromControlGradient(value.AutoHideTabGradient)
				};
				base.Theme.Skin.DockPaneStripSkin = new DockPaneStripSkin
				{
					DocumentGradient = new DockPaneStripGradient
					{
						ActiveTabGradient = GetTabGradientFromControlGradient(value.DocumentActiveTabGradient),
						InactiveTabGradient = GetTabGradientFromControlGradient(value.DocumentInactiveTabGradient),
						DockStripGradient = GetTabGradientFromControlGradient(value.DocumentDockStripGradient)
					},
					ToolWindowGradient = new DockPaneStripToolWindowGradient
					{
						ActiveTabGradient = GetTabGradientFromControlGradient(value.ToolWindowActiveTabGradient),
						InactiveTabGradient = GetTabGradientFromControlGradient(value.ToolWindowInactiveTabGradient),
						ActiveCaptionGradient = GetTabGradientFromControlGradient(value.ToolWindowActiveCaptionGradient),
						InactiveCaptionGradient = GetTabGradientFromControlGradient(value.ToolWindowInactiveCaptionGradient),
						DockStripGradient = GetTabGradientFromControlGradient(value.ToolWindowDockStripGradient)
					}
				};
			}
		}
	}

	private class DockContent : WeifenLuo.WinFormsUI.Docking.DockContent
	{
		private const int WM_PAINT = 15;

		private const int WM_ERASEBKGND = 20;

		private readonly ControlHostService m_controlHostService;

		public DockContent(ControlHostService controlHostService)
		{
			m_controlHostService = controlHostService;
		}

		protected override string GetPersistString()
		{
			return base.Name;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool flag = false;
			Control control = WinFormsUtil.GetFocusedControl();
			if (control is AdaptableControl)
			{
				AdaptableControl adaptableControl = control as AdaptableControl;
				if (adaptableControl.HasKeyboardFocus)
				{
					return false;
				}
			}
			if ((!(control is TextBoxBase) && !(control is ComboBox)) || !KeysUtil.IsTextBoxInput(control, keyData))
			{
				ICommandService commandService = m_controlHostService.m_commandService;
				if (commandService != null)
				{
					flag = commandService.ProcessKey(keyData);
				}
				if (!flag)
				{
					flag = base.ProcessCmdKey(ref msg, keyData);
				}
			}
			return flag;
		}

		protected override void WndProc(ref Message m)
		{
			DocumentSwitchTrace.Trace(this, "outer-host", "before", ref m,
				m_controlHostService.GetDocumentTraceContext, () => m_controlHostService.IsActiveDocumentSurface(this));
			if (ShouldTracePaint && (m.Msg == WM_PAINT || m.Msg == WM_ERASEBKGND))
			{
				PaintTimingLog.Write("ControlHostDockContent: name={0}, msg={1}, visible={2}, child={3}, childVisible={4}, childBounds={5}, containsFocus={6}", TraceName, m.Msg, Visible, TraceChildName, TraceChildVisible, TraceChildBounds, ContainsFocus);
			}
			base.WndProc(ref m);
			DocumentSwitchTrace.Trace(this, "outer-host", "after", ref m,
				m_controlHostService.GetDocumentTraceContext, () => m_controlHostService.IsActiveDocumentSurface(this));
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (ShouldTracePaint)
			{
				PaintTimingLog.Write("ControlHostDockContent: name={0}, VisibleChanged visible={1}, child={2}, childVisible={3}, childBounds={4}, containsFocus={5}", TraceName, Visible, TraceChildName, TraceChildVisible, TraceChildBounds, ContainsFocus);
			}
			base.OnVisibleChanged(e);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (ShouldTracePaint)
			{
				PaintTimingLog.Write("ControlHostDockContent: name={0}, PaintBackground clip={1}, visible={2}, child={3}, childVisible={4}, childBounds={5}, containsFocus={6}", TraceName, e.ClipRectangle, Visible, TraceChildName, TraceChildVisible, TraceChildBounds, ContainsFocus);
			}
			using (Region paintRegion = new Region(e.ClipRectangle))
			{
				foreach (Control child in Controls)
				{
					if (child.Visible)
					{
						paintRegion.Exclude(child.Bounds);
					}
				}
				if (!paintRegion.IsEmpty(e.Graphics))
				{
					Region previousClip = e.Graphics.Clip;
					try
					{
						e.Graphics.Clip = paintRegion;
						base.OnPaintBackground(e);
					}
					finally
					{
						e.Graphics.Clip = previousClip;
					}
				}
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (ShouldTracePaint)
			{
				PaintTimingLog.Write("ControlHostDockContent: name={0}, Paint clip={1}, visible={2}, child={3}, childVisible={4}, childBounds={5}, containsFocus={6}", TraceName, e.ClipRectangle, Visible, TraceChildName, TraceChildVisible, TraceChildBounds, ContainsFocus);
			}
			base.OnPaint(e);
		}

		private string TraceName => (Name ?? "null") + "#" + RuntimeHelpers.GetHashCode(this).ToString("X");

		private bool ShouldTracePaint => Name?.StartsWith(DocumentContentPrefix) == true || Controls.Count > 0 && Controls[0].GetType().Name == "AssetEditorControl";

		private string TraceChildName => Controls.Count > 0 ? Controls[0].GetType().Name : "null";

		private bool TraceChildVisible => Controls.Count > 0 && Controls[0].Visible;

		private Rectangle TraceChildBounds => Controls.Count > 0 ? Controls[0].Bounds : Rectangle.Empty;
	}

	private class DefaultClient : IControlHostClient
	{
		public void Activate(Control control)
		{
		}

		public void Deactivate(Control control)
		{
		}

		public bool Close(Control control)
		{
			return true;
		}
	}

	private const string DocumentContentPrefix = "Sce.Atf.DockPanel.DocumentContent,";

	private readonly Form m_mainForm;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[Import(AllowDefault = true)]
	private ICommandService m_commandService;

	[ImportMany]
	private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

	private readonly Dictionary<ControlInfo, DockContent> m_dockContent = new Dictionary<ControlInfo, DockContent>();

	private readonly Dictionary<DockPane, DocumentHostControl> m_activeDocumentHostsByPane = new Dictionary<DockPane, DocumentHostControl>();

	private readonly Dictionary<DockPane, IDisposable> m_documentPaneTraceObservers = new Dictionary<DockPane, IDisposable>();

	private readonly List<DockContent> m_unregisteredContents = new List<DockContent>();

	private readonly ActiveCollection<ControlInfo> m_controls;

	private readonly UniqueNamer m_uniqueNamer = new UniqueNamer('(');

	private readonly UniqueNamer m_idNamer = new UniqueNamer();

	private readonly DockPanel m_dockPanel;

	private DockContent m_activeDockContent;

	private long m_documentSwitchTraceGeneration;

	private bool m_activateCurrentDockContentPending;

	private Control m_activeClientControl;

	private Control m_pendingRegisteredDocumentControl;

	private ToolStripContainer m_toolStripContainer;

	private string m_dockPanelState;

	private bool m_formLoaded;

	private bool m_canFireDockStateChanged;

	private Image m_uiLockImage;

	private Image m_uiUnlockImage;

	private CommandRegister m_registeredCommands = CommandRegister.Default;

	private static readonly char[] s_pathDelimiters = new char[2] { '/', '\\' };

	private bool disposedValue = false;

	public virtual ThemeBase Theme
	{
		get
		{
			return m_dockPanel.Theme;
		}
		set
		{
			if (m_dockPanel.Theme != value)
			{
				IEnumerable<ControlInfo> infos = DetachControlInfos();
				m_dockPanel.Theme = value;
				ReattachControlInfos(infos);
			}
		}
	}

	public CommandRegister RegisteredCommands
	{
		get
		{
			return m_registeredCommands;
		}
		set
		{
			m_registeredCommands = value;
		}
	}

	private bool UILocked
	{
		get
		{
			return !m_dockPanel.AllowEndUserDocking;
		}
		set
		{
			m_dockPanel.AllowEndUserDocking = !value;
			if (m_commandService is CommandServiceBase commandServiceBase)
			{
				if (commandServiceBase.UserSelectedImageSize == CommandServiceBase.ImageSizes.Size16x16)
				{
					m_uiLockImage = ResourceUtil.GetImage16(Resources.LockUIImage);
					m_uiUnlockImage = ResourceUtil.GetImage16(Resources.UnlockUIImage);
				}
				else if (commandServiceBase.UserSelectedImageSize == CommandServiceBase.ImageSizes.Size32x32)
				{
					m_uiLockImage = ResourceUtil.GetImage32(Resources.LockUIImage);
					m_uiUnlockImage = ResourceUtil.GetImage32(Resources.UnlockUIImage);
				}
			}
			if ((RegisteredCommands & CommandRegister.UILock) == CommandRegister.UILock)
			{
				CommandInfo.UILock.GetButton().Image = (value ? m_uiLockImage : m_uiUnlockImage);
				CommandInfo.UILock.GetButton().ToolTipText = (value ? "Unlock UI Layout".Localize() : "Lock UI Layout".Localize());
			}
			if (m_toolStripContainer == null)
			{
				return;
			}
			IEnumerable<ToolStrip> enumerable = m_toolStripContainer.TopToolStripPanel.Controls.AsIEnumerable<ToolStrip>().Concat(m_toolStripContainer.BottomToolStripPanel.Controls.AsIEnumerable<ToolStrip>()).Concat(m_toolStripContainer.LeftToolStripPanel.Controls.AsIEnumerable<ToolStrip>())
				.Concat(m_toolStripContainer.RightToolStripPanel.Controls.AsIEnumerable<ToolStrip>());
			foreach (ToolStrip item in enumerable)
			{
				item.GripStyle = ((!value) ? ToolStripGripStyle.Visible : ToolStripGripStyle.Hidden);
				item.AllowItemReorder = !value;
			}
		}
	}

	public ControlInfo ActiveControl
	{
		get
		{
			return m_controls.ActiveItem;
		}
		set
		{
			m_controls.ActiveItem = value;
		}
	}

	public IEnumerable<ControlInfo> Controls => m_controls;

	object IDockStateProvider.DockState
	{
		get
		{
			return DockPanelState;
		}
		set
		{
			using (new User32.StopDrawingHelper(m_mainForm.Handle))
			{
				DockPanelState = (string)value;
			}
			m_mainForm.Invalidate(invalidateChildren: true);
		}
	}

	public static bool DoubleClickFloatWindowTitleMaximizes
	{
		get
		{
			return !FloatWindow.DoubleClickTitleBarToDockDefault;
		}
		set
		{
			FloatWindow.DoubleClickTitleBarToDockDefault = !value;
		}
	}

	public string DockPanelState
	{
		get
		{
			if (!m_formLoaded && !string.IsNullOrEmpty(m_dockPanelState))
			{
				return m_dockPanelState;
			}
			return GetDockPanelState();
		}
		set
		{
			m_dockPanelState = value;
			if (m_formLoaded)
			{
				SetDockPanelState(value);
			}
		}
	}

	public event EventHandler ActiveControlChanging;

	public event EventHandler ActiveControlChanged;

	public event EventHandler ControlVisibilityChanged;

	public event EventHandler<ItemInsertedEventArgs<ControlInfo>> ControlAdded;

	public event EventHandler<ItemRemovedEventArgs<ControlInfo>> ControlRemoved;

	public event EventHandler DockStateChanged;

	[ImportingConstructor]
	public ControlHostService(Form mainForm)
	{
		m_controls = new ActiveCollection<ControlInfo>();
		m_controls.ActiveItemChanged += controls_ActiveItemChanged;
		m_controls.ActiveItemChanging += controls_ActiveItemChanging;
		m_controls.ItemAdded += controls_ItemAdded;
		m_controls.ItemRemoved += controls_ItemRemoved;
		m_mainForm = mainForm;
		m_dockPanel = new DockPanel();
		m_dockPanel.Dock = DockStyle.Fill;
		m_dockPanel.DockBackColor = mainForm.BackColor;
		m_dockPanel.ShowDocumentIcon = true;
		m_dockPanel.ActiveContentChanged += dockPanel_ActiveContentChanged;
		m_dockPanel.ContentAdded += DockPanelContentAdded;
		m_dockPanel.ContentRemoved += DockPanelContentRemoved;
		DoubleClickFloatWindowTitleMaximizes = true;
		m_uiLockImage = ResourceUtil.GetImage24(Resources.LockUIImage);
		m_uiUnlockImage = ResourceUtil.GetImage24(Resources.UnlockUIImage);
	}

	private IEnumerable<ControlInfo> DetachControlInfos()
	{
		IList<ControlInfo> list = new List<ControlInfo>();
		ControlInfo[] array = m_dockContent.Keys.ToArray();
		foreach (ControlInfo controlInfo in array)
		{
			UnregisterControl(controlInfo.Control);
			list.Add(controlInfo);
		}
		return list;
	}

	private void ReattachControlInfos(IEnumerable<ControlInfo> infos)
	{
		foreach (ControlInfo info in infos)
		{
			RegisterControl(info.Control, info, info.Client);
		}
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		foreach (Control control in m_mainForm.Controls)
		{
			m_toolStripContainer = control as ToolStripContainer;
			if (m_toolStripContainer != null)
			{
				break;
			}
		}
		if (m_toolStripContainer != null)
		{
			m_dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
			m_toolStripContainer.ContentPanel.Controls.Add(m_dockPanel);
		}
		else
		{
			m_mainForm.IsMdiContainer = true;
			m_mainForm.Controls.Add(m_dockPanel);
		}
	}

	void IInitializable.Initialize()
	{
		m_mainForm.Load += mainForm_Load;
		m_mainForm.Shown += MainFormShown;
		m_mainForm.Closing += mainForm_Closing;
		m_dockPanel.SuspendLayout();
		ShowDefaultControls();
		m_dockPanel.ResumeLayout(performLayout: false);
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => DockPanelState, "DockPanelState", null, null));
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => UILocked, "UILocked", null, null));
		}
		CommandInfo.UILock.EnableCheckCanDoEvent(this);
		CommandInfo.WindowTileHorizontal.EnableCheckCanDoEvent(this);
		CommandInfo.WindowTileVertical.EnableCheckCanDoEvent(this);
		CommandInfo.WindowTileTabbed.EnableCheckCanDoEvent(this);
		if (m_commandService != null)
		{
			if ((RegisteredCommands & CommandRegister.UILock) == CommandRegister.UILock)
			{
				m_commandService.RegisterCommand(CommandInfo.UILock, this);
			}
			if ((RegisteredCommands & CommandRegister.WindowTileHorizontal) == CommandRegister.WindowTileHorizontal)
			{
				m_commandService.RegisterCommand(CommandInfo.WindowTileHorizontal, this);
			}
			if ((RegisteredCommands & CommandRegister.WindowTileVertical) == CommandRegister.WindowTileVertical)
			{
				m_commandService.RegisterCommand(CommandInfo.WindowTileVertical, this);
			}
			if ((RegisteredCommands & CommandRegister.WindowTileTabbed) == CommandRegister.WindowTileTabbed)
			{
				m_commandService.RegisterCommand(CommandInfo.WindowTileTabbed, this);
			}
		}
	}

	public bool RemoveControl(ControlInfo control)
	{
		return m_controls.Remove(control);
	}

	public void RegisterControl(Control control, ControlInfo info, IControlHostClient client)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		if (FindControlInfo(control) != null)
		{
			throw new ArgumentException("Control already registered");
		}
		if (client == null)
		{
			client = Global<DefaultClient>.Instance;
		}
		info.Client = client;
		info.Control = control;
		info.Changed += info_Changed;
		DockContent dockContent = null;
		bool flag = true;
		if (info.IsDocument.HasValue && info.IsDocument.Value)
		{
			foreach (DockContent unregisteredContent in m_unregisteredContents)
			{
				if (unregisteredContent.Name.StartsWith("Sce.Atf.DockPanel.DocumentContent,"))
				{
					dockContent = unregisteredContent;
					m_unregisteredContents.Remove(unregisteredContent);
					flag = false;
					break;
				}
			}
		}
		if (dockContent == null)
		{
			dockContent = new DockContent(this);
			dockContent.Name = GetPersistenceId(info);
		}
		dockContent.DockHandler.IsDirtyDocument = info.IsDirtyDocument;
		dockContent.DockHandler.IsReadOnlyDocument = info.IsReadOnlyDocument;
		if (!string.IsNullOrEmpty(info.HelpUrl))
		{
			dockContent.AddHelp(info.HelpUrl);
		}
		var t = Stopwatch.StartNew();
		UpdateDockContent(dockContent, info);
		t.Stop();
		PaintTimingLog.Write("RegisterControl.UpdateDockContent: {0}ms", t.ElapsedMilliseconds);
		m_dockContent.Add(info, dockContent);
		m_controls.ActiveItem = info;
		if (info.IsDocument == true)
		{
			m_pendingRegisteredDocumentControl = control;
		}
		info.HostControl = dockContent;
		Control hostedControl = GetHostedRegistrationControl(control, info);
		info.OriginalDock = control.Dock;
		hostedControl.Dock = DockStyle.Fill;
		var tAdd = Stopwatch.StartNew();
		dockContent.Controls.Add(hostedControl);
		tAdd.Stop();
		dockContent.FormClosing += dockContent_FormClosing;
		if (control is IControlHostPreShowClient preShowClient)
		{
			preShowClient.BeforeControlHostShow();
		}
		var tShow = Stopwatch.StartNew();
		ShowDockContent(dockContent, flag ? info : null);
		tShow.Stop();
		PaintTimingLog.Write("RegisterControl: add={0}ms, show={1}ms", tAdd.ElapsedMilliseconds, tShow.ElapsedMilliseconds);
		if (info.ShowInMenu)
		{
			if (info.IsDocument.HasValue && info.IsDocument.Value)
			{
				info.MenuText = "@" + info.Description;
			}
			RegisterMenuCommand(info, info.MenuText);
		}
		if (info.IsDocument.HasValue && info.IsDocument.Value)
		{
			ShowAndAttachDocumentHost(dockContent);
			ScheduleActivateCurrentDockContent("register document");
		}
		else
		{
			BringClientToFront(client);
		}
		ActivateClient(control);
	}

	public void UnregisterControl(Control control)
	{
		ControlInfo controlInfo = FindControlInfo(control);
		if (controlInfo != null)
		{
			DockContent dockContent = FindContent(controlInfo);
			if (controlInfo.ShowInMenu)
			{
				UnregisterMenuCommand(control);
			}
			dockContent.FormClosing -= dockContent_FormClosing;
			NotifyControlHostUnregister(control);
			DetachDocumentHostForUnregister(dockContent);
			if (dockContent.Controls.Count > 0)
			{
				dockContent.Controls.RemoveAt(0);
			}
			control.Dock = controlInfo.OriginalDock;
			m_dockContent.Remove(controlInfo);
			m_controls.Remove(controlInfo);
			controlInfo.Changed -= info_Changed;
			m_uniqueNamer.Retire(dockContent.Text);
			m_idNamer.Retire(dockContent.Name);
			dockContent.Hide();
			dockContent.Dispose();
			controlInfo.HostControl = null;
		}
	}

	public void UnregisterControl(ControlInfo info)
	{
		if (info != null)
		{
			DockContent dockContent = FindContent(info);
			if (info.ShowInMenu)
			{
				UnregisterMenuCommand(info.Control);
			}
			dockContent.FormClosing -= dockContent_FormClosing;
			NotifyControlHostUnregister(info.Control);
			DetachDocumentHostForUnregister(dockContent);
			if (dockContent.Controls.Count > 0)
			{
				dockContent.Controls.RemoveAt(0);
			}
			info.Control.Dock = info.OriginalDock;
			m_dockContent.Remove(info);
			m_controls.Remove(info);
			info.Changed -= info_Changed;
			m_uniqueNamer.Retire(dockContent.Text);
			m_idNamer.Retire(dockContent.Name);
			dockContent.Hide();
			dockContent.Dispose();
			info.HostControl = null;
		}
	}

	public void Show(Control control)
	{
		if (control != null)
		{
			ControlInfo controlInfo = FindControlInfo(control);
			if (controlInfo != null)
			{
				DockContent dockContent = FindContent(controlInfo);
				if (dockContent.DockPanel == null && dockContent.FloatPane == null)
				{
					dockContent.Show(m_dockPanel);
				}
				else
				{
					dockContent.Show();
				}
				if (dockContent.Visible && (dockContent == m_activeDockContent || m_dockPanel.ActiveContent == dockContent) && dockContent.Controls.Count > 0 && GetDocumentHost(dockContent.Controls[0]) != null)
				{
					AttachDocumentHostIfNeeded(dockContent);
				}
				this.ControlVisibilityChanged.Raise(control, EventArgs.Empty);
			}
		}
	}

	public void Hide(Control control)
	{
		ControlInfo controlInfo = FindControlInfo(control);
		DockContent dockContent = controlInfo != null ? FindContent(controlInfo) : FindContent(control);
		if (dockContent != null && dockContent.Visible)
		{
			dockContent.Hide();
			this.ControlVisibilityChanged.Raise(control, EventArgs.Empty);
		}
	}

	public object GetTabGroup(Control control)
	{
		return FindContent(control)?.Pane;
	}

	public IEnumerable<object> GetTabGroups()
	{
		foreach (DockPane pane in m_dockPanel.Panes)
		{
			yield return pane;
		}
	}

	public IEnumerable<Control> GetControlsInTabGroup(object tabGroupID)
	{
		if (!(tabGroupID is DockPane))
		{
			yield break;
		}
		foreach (KeyValuePair<ControlInfo, DockContent> pair in m_dockContent)
		{
			if (pair.Value.Pane == tabGroupID)
			{
				yield return pair.Key.Control;
			}
		}
	}

	public object GetPanel(Control control)
	{
		return (GetTabGroup(control) is DockPane dockPane) ? dockPane.DockPanel : null;
	}

	public void SetPanelPortion(object panel, StandardControlGroup group, float portion)
	{
		DockPanel dockPanel = ((panel != null) ? ((DockPanel)panel) : m_dockPanel);
		if (portion <= 0f)
		{
			throw new ArgumentOutOfRangeException("portion", "Must be greater than 0");
		}
		switch (group)
		{
		case StandardControlGroup.Left:
			dockPanel.DockLeftPortion = portion;
			break;
		case StandardControlGroup.Right:
			dockPanel.DockRightPortion = portion;
			break;
		case StandardControlGroup.Top:
			dockPanel.DockTopPortion = portion;
			break;
		case StandardControlGroup.Bottom:
			dockPanel.DockBottomPortion = portion;
			break;
		default:
			throw new ArgumentOutOfRangeException("group", "Can only be the left, top, bottom, or right portions");
		}
	}

	public bool CanDoCommand(object commandTag)
	{
		if (commandTag is Control)
		{
			return true;
		}
		bool result = false;
		if (commandTag is StandardCommand)
		{
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.WindowTileHorizontal:
				result = true;
				break;
			case StandardCommand.WindowTileVertical:
				result = true;
				break;
			case StandardCommand.WindowTileTabbed:
				result = true;
				break;
			case StandardCommand.UILock:
				result = true;
				break;
			}
		}
		return result;
	}

	public void DoCommand(object commandTag)
	{
		if (commandTag is Control control)
		{
			DockContent dockContent = FindContent(control);
			if (dockContent.DockState == DockState.Document)
			{
				if (!dockContent.Visible || dockContent.IsHidden)
				{
					dockContent.Show();
				}
			}
			else if (dockContent.Visible && !dockContent.IsHidden)
			{
				dockContent.Hide();
			}
			else
			{
				dockContent.Show();
			}
			this.ControlVisibilityChanged.Raise(control, EventArgs.Empty);
		}
		if (commandTag is StandardCommand)
		{
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.WindowTileHorizontal:
				TileDocumentContent(DockStyle.Right);
				break;
			case StandardCommand.WindowTileVertical:
				TileDocumentContent(DockStyle.Bottom);
				break;
			case StandardCommand.WindowTileTabbed:
				TileDocumentContent(DockStyle.Fill);
				break;
			case StandardCommand.UILock:
				UILocked = m_dockPanel.AllowEndUserDocking;
				break;
			case StandardCommand.HelpAbout:
				break;
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
		if (commandTag is Control control)
		{
			if (string.IsNullOrEmpty(state.Text))
			{
				string controlMenuText = GetControlMenuText(control);
				state.Text = controlMenuText;
			}
			DockContent dockContent = FindContent(control);
			state.Check = dockContent.Visible && !dockContent.IsHidden;
		}
		else if (commandTag is StandardCommand && (StandardCommand)commandTag == StandardCommand.UILock)
		{
			state.Text = (UILocked ? "Unlock UI Layout".Localize() : "Lock UI Layout".Localize());
		}
	}

	void IDockStateProvider.ResetDockState()
	{
		ControlInfo[] array = m_controls.ToArray();
		ControlInfo[] array2 = array;
		foreach (ControlInfo controlInfo in array2)
		{
			UnregisterControl(controlInfo.Control);
		}
		ControlInfo[] array3 = array;
		foreach (ControlInfo controlInfo2 in array3)
		{
			RegisterControl(controlInfo2.Control, controlInfo2, controlInfo2.Client);
		}
	}

	public void TileDocumentContent(DockStyle dockStyle)
	{
		DockWindow dockWindow = m_dockPanel.DockWindows[DockState.Document];
		if (dockWindow.NestedPanes.Count <= 0)
		{
			return;
		}
		DockPane dockPane = null;
		int num = m_dockPanel.DocumentsCount;
		foreach (IDockContent document in m_dockPanel.Documents)
		{
			if (document != null)
			{
				DockContentHandler dockHandler = document.DockHandler;
				DockPane pane = dockHandler.Pane;
				if (dockPane == null)
				{
					dockPane = pane;
					continue;
				}
				dockHandler.DockTo(dockPane, dockStyle, -1);
				num--;
			}
		}
	}

	public void ShowDefaultControls()
	{
		BringClientToFront(null);
	}

	public bool Close()
	{
		List<ControlInfo> list = new List<ControlInfo>(m_controls);
		foreach (ControlInfo item in list)
		{
			if (!m_controls.Contains(item) || !IsCenterGroup(item.Group) || item.Client.Close(item.Control))
			{
				continue;
			}
			return false;
		}
		foreach (ControlInfo item2 in list)
		{
			if (!m_controls.Contains(item2) || IsCenterGroup(item2.Group) || item2.Client.Close(item2.Control))
			{
				continue;
			}
			return false;
		}
		return true;
	}

	private void mainForm_Load(object sender, EventArgs e)
	{
		m_formLoaded = true;
		if (!string.IsNullOrEmpty(m_dockPanelState))
		{
			SetDockPanelState(m_dockPanelState);
		}
	}

	private void MainFormShown(object sender, EventArgs e)
	{
		m_canFireDockStateChanged = true;
		ScheduleActivateCurrentDockContent("main form shown");
	}

	private void mainForm_Closing(object sender, CancelEventArgs e)
	{
		if (e.Cancel)
		{
			return;
		}
		foreach (DockContent unregisteredContent in m_unregisteredContents)
		{
			unregisteredContent.DockHandler.Form.Close();
		}
		m_formLoaded = false;
		m_dockPanelState = GetDockPanelState();
		m_canFireDockStateChanged = false;
		e.Cancel = !Close();
		m_canFireDockStateChanged = e.Cancel;
	}

	private void controls_ActiveItemChanging(object sender, EventArgs e)
	{
		this.ActiveControlChanging.Raise(this, EventArgs.Empty);
	}

	private void controls_ActiveItemChanged(object sender, EventArgs e)
	{
		this.ActiveControlChanged.Raise(this, EventArgs.Empty);
	}

	private void controls_ItemAdded(object sender, ItemInsertedEventArgs<ControlInfo> e)
	{
		this.ControlAdded.Raise(this, e);
	}

	private void controls_ItemRemoved(object sender, ItemRemovedEventArgs<ControlInfo> e)
	{
		this.ControlRemoved.Raise(this, e);
	}

	private void dockPanel_ActiveContentChanged(object sender, EventArgs e)
	{
		DockContent dockContent = m_dockPanel.ActiveContent as DockContent;
		if (dockContent == m_activeDockContent)
			return;
		DockContent previousDockContent = m_activeDockContent;
		if (dockContent != null && dockContent.DockState == DockState.Document)
		{
			m_documentSwitchTraceGeneration = DocumentSwitchTrace.Begin(string.Format(
				"old={0}, new={1}", GetDocumentTraceIdentity(previousDockContent), GetDocumentTraceIdentity(dockContent)));
			ObserveDocumentPane(dockContent.Pane);
		}
		long tDeact = 0, tAct = 0, tOther = 0;
		var swTotal = Stopwatch.StartNew();

		if (m_activeDockContent != null && m_activeDockContent.Controls.Count > 0)
		{
			var sw = Stopwatch.StartNew();
			DockPaneStripBase dockPaneStripBase = GetDockPaneStripBase(m_activeDockContent);
			if (dockPaneStripBase != null)
				dockPaneStripBase.MouseUp -= dockPaneStrip_MouseUp;
			DeactivateClient(GetLogicalControl(m_activeDockContent.Controls[0]));
			tDeact = sw.ElapsedMilliseconds;
		}
		m_activeDockContent = dockContent;
		if (dockContent == null || dockContent.Controls.Count <= 0)
			return;

		bool deferDocumentActivate = dockContent.DockState == DockState.Document;
		{
			var sw = Stopwatch.StartNew();
			if (deferDocumentActivate)
			{
				long traceGeneration = m_documentSwitchTraceGeneration;
				m_dockPanel.BeginInvoke((Action)(() => ActivateClientIfStillActive(dockContent, traceGeneration)));
			}
			else
				ActivateClient(AttachDocumentHostIfNeeded(dockContent));
			tAct = sw.ElapsedMilliseconds;
		}
		{
			var sw = Stopwatch.StartNew();
			ControlInfo controlInfo = FindControlInfo(GetLogicalControl(dockContent.Controls[0]));
			if (controlInfo != null)
				m_controls.ActiveItem = controlInfo;
			DockPane pane = dockContent.Pane;
			foreach (ControlInfo control in m_controls)
				control.InActiveGroup = pane.Contains(control.HostControl);
			DockPaneStripBase dockPaneStripBase2 = GetDockPaneStripBase(dockContent);
			if (dockPaneStripBase2 != null)
				dockPaneStripBase2.MouseUp += dockPaneStrip_MouseUp;
			tOther = sw.ElapsedMilliseconds;
		}
		swTotal.Stop();
		if (swTotal.ElapsedMilliseconds > 0)
			PaintTimingLog.Write("TabSwitch: {0}ms (deact={1}ms, act={2}ms, other={3}ms)",
				swTotal.ElapsedMilliseconds, tDeact, tAct, tOther);
	}

	private void ActivateClientIfStillActive(DockContent dockContent, long traceGeneration = 0)
	{
		if (!m_dockPanel.IsDisposed && dockContent == m_activeDockContent && dockContent.Controls.Count > 0)
		{
			var sw = Stopwatch.StartNew();
			bool attachedLogicalControl;
			Control logicalControl = AttachDocumentHostIfNeeded(dockContent, out attachedLogicalControl);
			if (attachedLogicalControl)
			{
				FlushDocumentPaint(dockContent);
			}
			ActivateClient(logicalControl);
			if (sw.ElapsedMilliseconds > 0)
				PaintTimingLog.Write("DeferredActivateClient: {0}ms", sw.ElapsedMilliseconds);
			if (traceGeneration != 0 && !m_dockPanel.IsDisposed && m_dockPanel.IsHandleCreated)
				m_dockPanel.BeginInvoke((Action)(() => DocumentSwitchTrace.End(traceGeneration)));
		}
	}

	private void ObserveDocumentPane(DockPane pane)
	{
		if (pane == null || !pane.IsHandleCreated || m_documentPaneTraceObservers.ContainsKey(pane))
			return;
		m_documentPaneTraceObservers.Add(pane, DocumentSwitchTrace.Observe(
			pane, "dock-pane", GetDocumentTraceContext,
			() => m_activeDockContent != null && m_activeDockContent.Pane == pane));
	}

	private string GetDocumentTraceContext()
	{
		return "document=" + GetDocumentTraceIdentity(m_activeDockContent);
	}

	private bool IsActiveDocumentSurface(DockContent dockContent)
	{
		return dockContent != null && dockContent == m_activeDockContent;
	}

	private static string GetDocumentTraceIdentity(DockContent dockContent)
	{
		if (dockContent == null)
			return "null";
		Control logicalControl = dockContent.Controls.Count > 0 ? GetLogicalControl(dockContent.Controls[0]) : null;
		return (logicalControl?.GetType().FullName ?? dockContent.GetType().FullName) + "#" +
			RuntimeHelpers.GetHashCode(logicalControl ?? (object)dockContent).ToString("X");
	}

	private void ActivateCurrentDockContent()
	{
		DockContent dockContent = m_dockPanel.ActiveContent as DockContent;
		if (dockContent != null && dockContent == m_activeDockContent)
		{
			ActivateClientIfStillActive(dockContent);
			return;
		}
		dockPanel_ActiveContentChanged(m_dockPanel, EventArgs.Empty);
	}

	private void AttachVisibleDocumentHost(DockContent dockContent)
	{
		if (dockContent == null || dockContent.DockState != DockState.Document || !dockContent.Visible || dockContent.Controls.Count <= 0)
		{
			return;
		}

		bool attachedLogicalControl;
		Control logicalControl = AttachDocumentHostIfNeeded(dockContent, out attachedLogicalControl);
		if (attachedLogicalControl)
		{
			FlushDocumentPaint(dockContent);
		}
		ActivateClient(logicalControl);
	}

	private void ShowAndAttachDocumentHost(DockContent dockContent)
	{
		if (dockContent == null || dockContent.DockState != DockState.Document)
		{
			return;
		}
		if (!dockContent.Visible || dockContent.IsHidden)
		{
			dockContent.Show();
		}
		AttachVisibleDocumentHost(dockContent);
	}

	private void ScheduleActivateCurrentDockContent(string reason)
	{
		if (m_activateCurrentDockContentPending || m_mainForm.IsDisposed || !m_mainForm.IsHandleCreated)
		{
			return;
		}
		m_activateCurrentDockContentPending = true;
		m_mainForm.BeginInvoke((Action)delegate
		{
			m_activateCurrentDockContentPending = false;
			PaintTimingLog.Write("ActivateCurrentDockContent: " + reason);
			ActivateCurrentDockContent();
			AttachVisibleDocumentHosts();
			m_mainForm.BeginInvoke((Action)delegate
			{
				AttachVisibleDocumentHosts();
				m_pendingRegisteredDocumentControl = null;
			});
		});
	}

	private void AttachVisibleDocumentHosts()
	{
		foreach (DockContent dockContent in m_dockContent.Values.ToArray())
		{
			if (dockContent.DockState == DockState.Document && dockContent.Visible && !dockContent.IsHidden && dockContent.Controls.Count > 0 && GetDocumentHost(dockContent.Controls[0]) != null)
			{
				PaintTimingLog.Write("AttachVisibleDocumentHosts: " + dockContent.Name);
				AttachVisibleDocumentHost(dockContent);
			}
		}
	}

	private Control AttachDocumentHostIfNeeded(DockContent dockContent)
	{
		bool attachedLogicalControl;
		return AttachDocumentHostIfNeeded(dockContent, out attachedLogicalControl);
	}

	private Control AttachDocumentHostIfNeeded(DockContent dockContent, out bool attachedLogicalControl)
	{
		attachedLogicalControl = false;
		Control hostedControl = dockContent.Controls.Count > 0 ? dockContent.Controls[0] : null;
		DocumentHostControl documentHost = GetDocumentHost(hostedControl);
		if (documentHost == null)
		{
			return hostedControl;
		}

		DockPane pane = dockContent.Pane;
		long tDetach = 0;
		long tAttach = 0;
		DocumentHostControl activeDocumentHost;
		if (pane != null && m_activeDocumentHostsByPane.TryGetValue(pane, out activeDocumentHost) && activeDocumentHost != documentHost)
		{
			var swDetach = Stopwatch.StartNew();
			activeDocumentHost.DetachLogicalControl();
			tDetach = swDetach.ElapsedMilliseconds;
		}

		var swAttach = Stopwatch.StartNew();
		attachedLogicalControl = documentHost.AttachLogicalControl();
		tAttach = swAttach.ElapsedMilliseconds;
		if (pane != null)
		{
			m_activeDocumentHostsByPane[pane] = documentHost;
		}
		if (tDetach + tAttach > 0)
		{
			PaintTimingLog.Write("DocumentHostSwitch: detach={0}ms, attach={1}ms", tDetach, tAttach);
		}
		return documentHost.LogicalControl;
	}

	private void FlushDocumentPaint(DockContent dockContent)
	{
		if (dockContent.DockState != DockState.Document || !dockContent.Visible)
		{
			return;
		}
		var sw = Stopwatch.StartNew();
		var stats = new VisibleTreeUpdateStats();
		UpdateVisibleTree(dockContent, 0, stats);
		PaintTimingLog.Write(
			"FlushDocumentPaint: total={0}ms, controls={1}, depth={2}, update={3}ms, slowest={4}:{5}ms",
			sw.ElapsedMilliseconds, stats.ControlCount, stats.MaxDepth, stats.UpdateMilliseconds,
			stats.SlowestControlType ?? "null", stats.SlowestUpdateMilliseconds);
	}

	private sealed class VisibleTreeUpdateStats
	{
		public int ControlCount;
		public int MaxDepth;
		public long UpdateMilliseconds;
		public long SlowestUpdateMilliseconds;
		public string SlowestControlType;
	}

	private void UpdateVisibleTree(Control control, int depth, VisibleTreeUpdateStats stats)
	{
		if (control == null || !control.Visible)
		{
			return;
		}
		stats.ControlCount++;
		stats.MaxDepth = Math.Max(stats.MaxDepth, depth);
		control.Invalidate(invalidateChildren: true);
		var sw = Stopwatch.StartNew();
		control.Update();
		long updateMilliseconds = sw.ElapsedMilliseconds;
		stats.UpdateMilliseconds += updateMilliseconds;
		if (updateMilliseconds > stats.SlowestUpdateMilliseconds)
		{
			stats.SlowestUpdateMilliseconds = updateMilliseconds;
			stats.SlowestControlType = control.GetType().Name;
		}
		foreach (Control child in control.Controls)
		{
			UpdateVisibleTree(child, depth + 1, stats);
		}
	}

	private DockPaneStripBase GetDockPaneStripBase(DockContent dockContent)
	{
		DockPane pane = dockContent.Pane;
		if (pane != null)
		{
			foreach (Control control in pane.Controls)
			{
				if (control is DockPaneStripBase result)
				{
					return result;
				}
			}
		}
		return null;
	}

	private void dockPaneStrip_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && m_activeDockContent != null)
		{
			DockPaneStripBase dockPaneStripBase = sender as DockPaneStripBase;
			ControlInfo target = FindControlInfo(GetLogicalControl(m_activeDockContent.Controls[0]));
			IEnumerable<object> commands = m_contextMenuCommandProviders.GetCommands(null, target);
			Point screenPoint = dockPaneStripBase.PointToScreen(new Point(e.X, e.Y));
			m_commandService.RunContextMenu(commands, screenPoint);
		}
	}

	private void dockContent_FormClosing(object sender, FormClosingEventArgs e)
	{
		DockContent dockContent = (DockContent)sender;
		Control control = GetLogicalControl(dockContent.Controls[0]);
		ControlInfo controlInfo = FindControlInfo(control);
		if (controlInfo.Client.Close(control))
		{
			if (IsCenterGroup(controlInfo.Group))
			{
				UnregisterControl(control);
				if (m_activeDockContent == dockContent)
				{
					m_activeDockContent = null;
				}
				ScheduleActivateCurrentDockContent("close document");
			}
			else
			{
				dockContent.Hide();
				e.Cancel = true;
			}
		}
		else
		{
			e.Cancel = true;
		}
	}

	private void info_Changed(object sender, EventArgs e)
	{
		ControlInfo controlInfo = (ControlInfo)sender;
		DockContent dockContent = FindContent(controlInfo);
		UpdateDockContent(dockContent, controlInfo);
	}

	private void ActivateClient(Control control)
	{
		if (m_pendingRegisteredDocumentControl != null && control != m_pendingRegisteredDocumentControl)
		{
			ControlInfo controlInfo = FindControlInfo(control);
			if (controlInfo != null && controlInfo.IsDocument == true)
			{
				return;
			}
		}
		if (control != null && m_activeClientControl != control)
		{
			m_activeClientControl = control;
			FindControlInfo(control)?.Client.Activate(control);
		}
	}

	private void DeactivateClient(Control control)
	{
		if (control != null && m_activeClientControl == control)
		{
			m_activeClientControl = null;
			FindControlInfo(control)?.Client.Deactivate(control);
		}
	}

	private void DetachDocumentHostForUnregister(DockContent dockContent)
	{
		DocumentHostControl documentHost = dockContent.Controls.Count > 0 ? GetDocumentHost(dockContent.Controls[0]) : null;
		if (documentHost != null)
		{
			if (m_activeClientControl == documentHost.LogicalControl)
			{
				m_activeClientControl = null;
			}
			documentHost.DetachLogicalControl();
			DockPane pane = dockContent.Pane;
			DocumentHostControl activeDocumentHost;
			if (pane != null && m_activeDocumentHostsByPane.TryGetValue(pane, out activeDocumentHost) && activeDocumentHost == documentHost)
			{
				m_activeDocumentHostsByPane.Remove(pane);
			}
		}
	}

	private void NotifyControlHostUnregister(Control control)
	{
		if (control is IControlHostUnregisteringClient unregisteringClient)
		{
			unregisteringClient.BeforeControlHostUnregister();
		}
	}

	private void BringClientToFront(IControlHostClient client)
	{
		foreach (ControlInfo control in m_controls)
		{
			if (client == control.Client)
			{
				DockContent dockContent = FindContent(control);
				dockContent.BringToFront();
			}
		}
	}

	private ControlInfo FindControlInfo(Control control)
	{
		Control logicalControl = GetLogicalControl(control);
		foreach (KeyValuePair<ControlInfo, DockContent> pair in m_dockContent)
		{
			if (pair.Key.Control == logicalControl)
			{
				return pair.Key;
			}
		}
		return null;
	}

	private static bool UseVirtualDocumentHost(ControlInfo info)
	{
		return info != null && info.IsDocument.HasValue && info.IsDocument.Value;
	}

	private static Control GetHostedRegistrationControl(Control control, ControlInfo info)
	{
		return UseVirtualDocumentHost(info) ? new DocumentHostControl(control) : control;
	}

	private static Control GetLogicalControl(Control control)
	{
		return control is DocumentHostControl documentHost ? documentHost.LogicalControl : control;
	}

	private static DocumentHostControl GetDocumentHost(Control control)
	{
		return control as DocumentHostControl;
	}

	private DockContent FindContent(Control control)
	{
		ControlInfo controlInfo = FindControlInfo(control);
		return FindContent(controlInfo);
	}

	private DockContent FindContent(ControlInfo controlInfo)
	{
		m_dockContent.TryGetValue(controlInfo, out var value);
		return value;
	}

	private void UpdateDockContent(DockContent dockContent, ControlInfo info)
	{
		if (!string.IsNullOrEmpty(dockContent.Text))
		{
			m_uniqueNamer.Retire(dockContent.Text);
		}
		string name = info.Name;
		if (info.IsDocument.HasValue && info.IsDocument.Value)
		{
			dockContent.Text = name;
			dockContent.Name = "Sce.Atf.DockPanel.DocumentContent,";
		}
		else
		{
			string text = m_uniqueNamer.Name(name);
			dockContent.Text = text;
		}
		dockContent.ToolTipText = info.Description;
		if (info.Icon != null)
		{
			dockContent.ShowIcon = true;
			dockContent.Icon = info.Icon;
		}
		else if (info.Image != null)
		{
			dockContent.ShowIcon = true;
			dockContent.Icon = GdiUtil.CreateIcon(info.Image, 16, keepAspectRatio: true);
		}
		else
		{
			dockContent.ShowIcon = false;
			dockContent.Icon = null;
		}
		bool closeButtonVisible = (dockContent.CloseButton = !info.Group.Equals(StandardControlGroup.CenterPermanent));
		dockContent.CloseButtonVisible = closeButtonVisible;
		if (info.Docking != null)
		{
			dockContent.DockAreas = (DockAreas)info.Docking.DockAreas;
			dockContent.Tag = info.Docking.GroupTag;
		}
		if (dockContent.DockState == DockState.Document && dockContent.Pane != null)
		{
			dockContent.Pane.Invalidate(invalidateChildren: true);
		}
	}

	private void ShowDockContent(DockContent dockContent, ControlInfo info)
	{
		if (info == null)
		{
			dockContent.Show(m_dockPanel);
			return;
		}
		DockState dockState = (IsCenterGroup(info.Group) ? (dockContent.DockHandler.IsFloat ? DockState.Float : DockState.Document) : (info.Group switch
		{
			StandardControlGroup.Left => DockState.DockLeft, 
			StandardControlGroup.Right => DockState.DockRight, 
			StandardControlGroup.Top => DockState.DockTop, 
			StandardControlGroup.Bottom => DockState.DockBottom, 
			StandardControlGroup.Floating => DockState.Float, 
			_ => DockState.DockLeftAutoHide, 
		}));
		if (dockContent.Tag != null && dockState != DockState.Float)
		{
			foreach (FloatWindow floatWindow in m_dockPanel.FloatWindows)
			{
				foreach (DockPane visibleNestedPane in floatWindow.VisibleNestedPanes)
				{
					if (visibleNestedPane.Tag == dockContent.Tag)
					{
						dockState = DockState.Float;
					}
				}
			}
		}
		if (dockContent.DockPanel == null && dockContent.FloatPane == null)
		{
			if (dockState == DockState.Document)
			{
				// Fast path: defer layout+activate to next frame so Open() returns faster
				m_dockPanel.SuspendLayout(allWindows: true);
				dockContent.DockPanel = m_dockPanel;
				DockPane docPane = null;
				foreach (DockPane p in m_dockPanel.Panes)
				{
					if (p.DockState == DockState.Document && p.IsActivated)
					{
						docPane = p;
						break;
					}
					if (p.DockState == DockState.Document && docPane == null)
						docPane = p;
				}
				if (docPane == null)
					docPane = m_dockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(dockContent, DockState.Document, show: false);
				dockContent.Pane = docPane;
				dockContent.DockState = DockState.Document;
				m_dockPanel.BeginInvoke(new Action(() =>
				{
					m_dockPanel.ResumeLayout(performLayout: true, allWindows: true);
					dockContent.Activate();
				}));
			}
			else
			{
				dockContent.Show(m_dockPanel, dockState);
			}
		}
		else if (info.ControlVisibility == ControlInitialVisibility.AlwaysVisible)
		{
			dockContent.Show();
		}
		else if (info.ControlVisibility == ControlInitialVisibility.AlwaysHidden)
		{
			dockContent.Hide();
		}
	}

	private string GetPersistenceId(ControlInfo info)
	{
		if (info.IsDocument.HasValue && info.IsDocument.Value)
		{
			return info.Name;
		}
		string text = info.Name;
		if (string.IsNullOrEmpty(text) || text.Length > 64 || text.IndexOfAny(s_pathDelimiters) > 0 || text.Contains("."))
		{
			text = "document_panel";
		}
		string desired = info.Client.GetType().Name + "_" + text;
		return m_idNamer.Name(desired);
	}

	private string GetControlMenuText(Control control)
	{
		ControlInfo controlInfo = FindControlInfo(control);
		string name = controlInfo.Name;
		string text = name;
		if (name.StartsWith("@"))
		{
			text = text.TrimStart('@');
		}
		else if (!IsCenterGroup(controlInfo.Group))
		{
			int num = text.LastIndexOfAny(s_pathDelimiters);
			if (num >= 0)
			{
				text = text.Substring(num + 1);
			}
		}
		return text;
	}

	private void RegisterMenuCommand(ControlInfo info, string text)
	{
		if (m_commandService != null)
		{
			bool flag = ((!info.IsDocument.HasValue) ? (info.Client is IDocumentClient) : info.IsDocument.Value);
			CommandInfo info2 = new CommandInfo(info.Control, StandardMenu.Window, info.MenuGroupOverride ?? ((object)(flag ? StandardCommandGroup.WindowDocuments : StandardCommandGroup.WindowGeneral)), text, "Activate Window".Localize());
			m_commandService.RegisterCommand(info2, this);
		}
	}

	private void UnregisterMenuCommand(Control control)
	{
		if (m_commandService != null)
		{
			m_commandService.UnregisterCommand(control, this);
		}
	}

	private static bool IsCenterGroup(StandardControlGroup groupTag)
	{
		return groupTag == StandardControlGroup.Center || groupTag == StandardControlGroup.CenterPermanent;
	}

	private void ConvertToAbsolutePanelPortions()
	{
		DockPanel dockPanel = m_dockPanel;
		int num = dockPanel.ClientRectangle.Height - dockPanel.DockPadding.Bottom - dockPanel.DockPadding.Top;
		int num2 = dockPanel.ClientRectangle.Width - dockPanel.DockPadding.Left - dockPanel.DockPadding.Right;
		if (dockPanel.DockBottomPortion < 1.0)
		{
			dockPanel.DockBottomPortion *= num;
		}
		if (dockPanel.DockLeftPortion < 1.0)
		{
			dockPanel.DockLeftPortion *= num2;
		}
		if (dockPanel.DockRightPortion < 1.0)
		{
			dockPanel.DockRightPortion *= num2;
		}
		if (dockPanel.DockTopPortion < 1.0)
		{
			dockPanel.DockTopPortion *= num;
		}
	}

	private void SetDockPanelState(string value)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using StreamWriter streamWriter = new StreamWriter(memoryStream);
			int length = Math.Min(value.Length, 20);
			string text = StringUtil.RemoveAllWhiteSpace(value.Substring(0, length));
			if (!text.StartsWith("<?xmlversion=", StringComparison.OrdinalIgnoreCase))
			{
				streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
			}
			streamWriter.Write(value);
			streamWriter.Flush();
			memoryStream.Seek(0L, SeekOrigin.Begin);
			m_dockPanel.SuspendLayout();
			foreach (ControlInfo control in m_controls)
			{
				DockContent dockContent = FindContent(control);
				dockContent.DockState = DockState.Unknown;
				dockContent.DockPanel = null;
				dockContent.FloatPane = null;
				dockContent.Pane = null;
			}
			m_dockPanel.ActiveContentChanged -= dockPanel_ActiveContentChanged;
			m_dockPanel.ContentAdded -= DockPanelContentAdded;
			m_dockPanel.ContentRemoved -= DockPanelContentRemoved;
			DeserializeDockContent deserializeContent = StringToDockContent;
			try
			{
				m_dockPanel.LoadFromXml(memoryStream, deserializeContent, closeStream: true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("ControlHostService saved Layout failed to load from Xml: {0}", ex.Message);
				m_dockPanel.ResumeLayout();
				m_dockPanel.ActiveContentChanged += dockPanel_ActiveContentChanged;
				m_dockPanel.ContentAdded += DockPanelContentAdded;
				m_dockPanel.ContentRemoved += DockPanelContentRemoved;
				IEnumerable<ControlInfo> infos = m_dockContent.Keys.ToArray();
				m_controls.Clear();
				m_dockContent.Clear();
				ReattachControlInfos(infos);
				return;
			}
			m_dockPanel.ResumeLayout();
			m_dockPanel.ActiveContentChanged += dockPanel_ActiveContentChanged;
			m_dockPanel.ContentAdded += DockPanelContentAdded;
			m_dockPanel.ContentRemoved += DockPanelContentRemoved;
			ActivateCurrentDockContent();
			foreach (KeyValuePair<ControlInfo, DockContent> item in m_dockContent)
			{
				DockContent value2 = item.Value;
				ControlInfo key = item.Key;
				if ((value2.DockPanel == null && value2.FloatPane == null) || key.ControlVisibility == ControlInitialVisibility.AlwaysHidden || key.ControlVisibility == ControlInitialVisibility.AlwaysVisible)
				{
					UpdateDockContent(value2, key);
					ShowDockContent(value2, key);
				}
			}
			foreach (DockContent unregisteredContent in m_unregisteredContents)
			{
				unregisteredContent.Hide();
			}
		}
		ConvertToAbsolutePanelPortions();
	}

	private string GetDockPanelState()
	{
		using MemoryStream memoryStream = new MemoryStream();
		m_dockPanel.SaveAsXml(memoryStream, Encoding.UTF8);
		using MemoryStream stream = new MemoryStream(memoryStream.GetBuffer());
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}

	private IDockContent StringToDockContent(string id)
	{
		foreach (DockContent value in m_dockContent.Values)
		{
			if (value.Name == id)
			{
				return value;
			}
		}
		if (id.StartsWith("Sce.Atf.DockPanel.DocumentContent,"))
		{
			DockContent dockContent = new DockContent(this);
			dockContent.Name = id;
			m_unregisteredContents.Add(dockContent);
			return dockContent;
		}
		return null;
	}

	private void DockPanelContentAdded(object sender, DockContentEventArgs e)
	{
		DockHandlerSubscribe(e.Content);
		OnDockStateChanged();
	}

	private void DockPanelContentRemoved(object sender, DockContentEventArgs e)
	{
		DockHandlerUnsubscribe(e.Content);
		OnDockStateChanged();
	}

	private void DockHandlerSubscribe(IDockContent content)
	{
		if (content == null)
		{
			return;
		}
		DockContentHandler dockHandler = content.DockHandler;
		if (dockHandler != null)
		{
			dockHandler.DockStateChanged += DockPanelSomethingChanged;
			if (dockHandler.DockPanel != null)
			{
				dockHandler.DockPanel.SizeChanged += DockPanelSomethingChanged;
			}
			if (dockHandler.Form != null)
			{
				dockHandler.Form.SizeChanged += DockPanelSomethingChanged;
			}
			if (dockHandler.FloatPane != null)
			{
				dockHandler.FloatPane.SizeChanged += DockPanelSomethingChanged;
			}
			if (dockHandler.Pane != null)
			{
				dockHandler.Pane.SizeChanged += DockPanelSomethingChanged;
			}
			if (dockHandler.PanelPane != null)
			{
				dockHandler.PanelPane.SizeChanged += DockPanelSomethingChanged;
			}
		}
	}

	private void DockHandlerUnsubscribe(IDockContent content)
	{
		if (content == null)
		{
			return;
		}
		DockContentHandler dockHandler = content.DockHandler;
		if (dockHandler != null)
		{
			dockHandler.DockStateChanged -= DockPanelSomethingChanged;
			if (dockHandler.DockPanel != null)
			{
				dockHandler.DockPanel.SizeChanged -= DockPanelSomethingChanged;
			}
			if (dockHandler.Form != null)
			{
				dockHandler.Form.SizeChanged -= DockPanelSomethingChanged;
			}
			if (dockHandler.FloatPane != null)
			{
				dockHandler.FloatPane.SizeChanged -= DockPanelSomethingChanged;
			}
			if (dockHandler.Pane != null)
			{
				dockHandler.Pane.SizeChanged -= DockPanelSomethingChanged;
			}
			if (dockHandler.PanelPane != null)
			{
				dockHandler.PanelPane.SizeChanged -= DockPanelSomethingChanged;
			}
		}
	}

	private bool m_dockStateThrottled;

	private void DockPanelSomethingChanged(object sender, EventArgs e)
	{
		if (m_dockStateThrottled)
			return;
		m_dockStateThrottled = true;
		var sw = Stopwatch.StartNew();
		OnDockStateChanged();
		sw.Stop();
		if (sw.ElapsedMilliseconds > 1)
			PaintTimingLog.Write("DockStateChanged event: {0}ms", sw.ElapsedMilliseconds);
		System.Windows.Forms.Timer t = new System.Windows.Forms.Timer { Interval = 50 };
		t.Tick += (_, __) => { t.Stop(); t.Dispose(); m_dockStateThrottled = false; };
		t.Start();
	}

	private void OnDockStateChanged()
	{
		if (m_canFireDockStateChanged)
		{
			this.DockStateChanged.Raise(this, EventArgs.Empty);
		}
	}

	private static TabGradient GetTabGradientFromControlGradient(ControlGradient controlGradient)
	{
		return new TabGradient
		{
			StartColor = controlGradient.StartColor,
			EndColor = controlGradient.EndColor,
			LinearGradientMode = controlGradient.LinearGradientMode,
			TextColor = controlGradient.TextColor
		};
	}

	private static ControlGradient GetControlGradientFromDockPanelGradient(DockPanelGradient dockPanelGradient)
	{
		ControlGradient result = new ControlGradient
		{
			StartColor = dockPanelGradient.StartColor,
			EndColor = dockPanelGradient.EndColor,
			LinearGradientMode = dockPanelGradient.LinearGradientMode
		};
		if (dockPanelGradient is TabGradient tabGradient)
		{
			result.TextColor = tabGradient.TextColor;
		}
		return result;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				DocumentSwitchTrace.End(m_documentSwitchTraceGeneration);
				m_documentSwitchTraceGeneration = 0;
				foreach (IDisposable observer in m_documentPaneTraceObservers.Values)
					observer.Dispose();
				m_documentPaneTraceObservers.Clear();
				m_commandService.UnregisterCommand(StandardCommand.UILock, this);
				m_commandService.UnregisterCommand(StandardCommand.WindowTileVertical, this);
				m_commandService.UnregisterCommand(StandardCommand.WindowTileHorizontal, this);
				m_commandService.UnregisterCommand(StandardCommand.WindowTileTabbed, this);
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
