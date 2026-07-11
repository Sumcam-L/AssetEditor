using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

[ToolboxItem(false)]
public class DockPane : UserControl, IDockDragSource, IDragSource
{
	public enum AppearanceStyle
	{
		ToolWindow,
		Document
	}

	private enum HitTestArea
	{
		Caption,
		TabStrip,
		Content,
		None
	}

	private struct HitTestResult
	{
		public HitTestArea HitArea;

		public int Index;

		public HitTestResult(HitTestArea hitTestArea, int index)
		{
			HitArea = hitTestArea;
			Index = index;
		}
	}

	internal class DefaultSplitterControl : SplitterControlBase
	{
		public DefaultSplitterControl(DockPane pane)
			: base(pane)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (base.DockPane.DockState == DockState.Document)
			{
				Graphics graphics = e.Graphics;
				Rectangle clientRectangle = base.ClientRectangle;
				if (base.Alignment == DockAlignment.Top || base.Alignment == DockAlignment.Bottom)
				{
					graphics.DrawLine(SystemPens.ControlDark, clientRectangle.Left, clientRectangle.Bottom - 1, clientRectangle.Right, clientRectangle.Bottom - 1);
				}
				else if (base.Alignment == DockAlignment.Left || base.Alignment == DockAlignment.Right)
				{
					graphics.DrawLine(SystemPens.ControlDarkDark, clientRectangle.Right - 1, clientRectangle.Top, clientRectangle.Right - 1, clientRectangle.Bottom);
				}
			}
		}
	}

	public class SplitterControlBase : Control, ISplitterDragSource, IDragSource
	{
		private DockPane m_pane;

		private DockAlignment m_alignment;

		public DockPane DockPane => m_pane;

		public DockAlignment Alignment
		{
			get
			{
				return m_alignment;
			}
			set
			{
				m_alignment = value;
				if (m_alignment == DockAlignment.Left || m_alignment == DockAlignment.Right)
				{
					Cursor = Cursors.VSplit;
				}
				else if (m_alignment == DockAlignment.Top || m_alignment == DockAlignment.Bottom)
				{
					Cursor = Cursors.HSplit;
				}
				else
				{
					Cursor = Cursors.Default;
				}
				if (DockPane.DockState == DockState.Document)
				{
					Invalidate();
				}
			}
		}

		bool ISplitterDragSource.IsVertical
		{
			get
			{
				NestedDockingStatus nestedDockingStatus = DockPane.NestedDockingStatus;
				if (nestedDockingStatus.DisplayingAlignment != DockAlignment.Left)
				{
					return nestedDockingStatus.DisplayingAlignment == DockAlignment.Right;
				}
				return true;
			}
		}

		Rectangle ISplitterDragSource.DragLimitBounds
		{
			get
			{
				NestedDockingStatus nestedDockingStatus = DockPane.NestedDockingStatus;
				Rectangle result = base.Parent.RectangleToScreen(nestedDockingStatus.LogicalBounds);
				if (((ISplitterDragSource)this).IsVertical)
				{
					result.X += 24;
					result.Width -= 48;
				}
				else
				{
					result.Y += 24;
					result.Height -= 48;
				}
				return result;
			}
		}

		Control IDragSource.DragControl => this;

		public SplitterControlBase(DockPane pane)
		{
			SetStyle(ControlStyles.Selectable, value: false);
			m_pane = pane;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left)
			{
				DockPane.DockPanel.BeginDrag(this, base.Parent.RectangleToScreen(base.Bounds));
			}
		}

		void ISplitterDragSource.BeginDrag(Rectangle rectSplitter)
		{
		}

		void ISplitterDragSource.EndDrag()
		{
		}

		void ISplitterDragSource.MoveSplitter(int offset)
		{
			NestedDockingStatus nestedDockingStatus = DockPane.NestedDockingStatus;
			double proportion = nestedDockingStatus.Proportion;
			if (nestedDockingStatus.LogicalBounds.Width > 0 && nestedDockingStatus.LogicalBounds.Height > 0)
			{
				proportion = ((nestedDockingStatus.DisplayingAlignment == DockAlignment.Left) ? (proportion + (double)offset / (double)nestedDockingStatus.LogicalBounds.Width) : ((nestedDockingStatus.DisplayingAlignment == DockAlignment.Right) ? (proportion - (double)offset / (double)nestedDockingStatus.LogicalBounds.Width) : ((nestedDockingStatus.DisplayingAlignment != DockAlignment.Top) ? (proportion - (double)offset / (double)nestedDockingStatus.LogicalBounds.Height) : (proportion + (double)offset / (double)nestedDockingStatus.LogicalBounds.Height))));
				DockPane.SetNestedDockingProportion(proportion);
			}
		}
	}

	private DockPaneCaptionBase m_captionControl;

	private DockPaneStripBase m_tabStripControl;

	private bool m_isDisposing;

	private IDockContent m_activeContent;

	private bool m_allowDockDragAndDrop = true;

	private IDisposable m_autoHidePane;

	private object m_autoHideTabs;

	private DockContentCollection m_contents;

	private DockContentCollection m_displayingContents;

	private DockPanel m_dockPanel;

	private bool m_isActivated;

	private bool m_isActiveDocumentPane;

	private bool m_isHidden = true;

	private bool m_layoutPending;

	private static readonly object DockStateChangedEvent;

	private static readonly object IsActivatedChangedEvent;

	private static readonly object IsActiveDocumentPaneChangedEvent;

	private NestedDockingStatus m_nestedDockingStatus;

	private bool m_isFloat;

	private DockState m_dockState;

	private int m_countRefreshStateChange;

	private DockWindow _lastParentWindow;

	private SplitterControlBase m_splitter;

	private DockPaneCaptionBase CaptionControl => m_captionControl;

	public DockPaneStripBase TabStripControl => m_tabStripControl;

	public virtual IDockContent ActiveContent
	{
		get
		{
			return m_activeContent;
		}
		set
		{
			if (ActiveContent == value)
			{
				return;
			}
			DockTrace.WritePane(this, "ActiveContent set begin {0}->{1}, displaying={2}, contents={3}", ActiveContent?.DockHandler?.Form?.Name ?? "null", value?.DockHandler?.Form?.Name ?? "null", DisplayingContents.Count, Contents.Count);
			if (value != null)
			{
				if (!DisplayingContents.Contains(value))
				{
					throw new InvalidOperationException(Strings.DockPane_ActiveContent_InvalidValue);
				}
			}
			else if (DisplayingContents.Count != 0)
			{
				throw new InvalidOperationException(Strings.DockPane_ActiveContent_InvalidValue);
			}
			IDockContent activeContent = m_activeContent;
			if (DockPanel.ActiveAutoHideContent == activeContent)
			{
				DockPanel.ActiveAutoHideContent = null;
			}
			m_activeContent = value;
			if (DockPanel.DocumentStyle == DocumentStyle.DockingMdi && DockState == DockState.Document)
			{
				if (m_activeContent != null)
				{
					m_activeContent.DockHandler.Form.BringToFront();
				}
			}
		else
		{
			if (m_activeContent != null)
			{
				m_activeContent.DockHandler.SetVisible();
			}
			if (activeContent != null && DisplayingContents.Contains(activeContent))
			{
				activeContent.DockHandler.SetVisible();
			}
				if (IsActivated && m_activeContent != null)
				{
					m_activeContent.DockHandler.Activate();
				}
			}
			if (FloatWindow != null)
			{
				FloatWindow.SetText();
			}
			if (DockPanel.DocumentStyle == DocumentStyle.DockingMdi && DockState == DockState.Document)
			{
				RefreshChanges(performLayout: false);
			}
			else
			{
				RefreshChanges();
			}
			if (m_activeContent != null)
			{
				TabStripControl.EnsureTabVisible(m_activeContent);
			}
		DockTrace.WritePane(this, "ActiveContent set end active={0}, displaying={1}, contents={2}", ActiveContent?.DockHandler?.Form?.Name ?? "null", DisplayingContents.Count, Contents.Count);
		}
	}

	public virtual bool AllowDockDragAndDrop
	{
		get
		{
			return m_allowDockDragAndDrop;
		}
		set
		{
			m_allowDockDragAndDrop = value;
		}
	}

	internal IDisposable AutoHidePane
	{
		get
		{
			return m_autoHidePane;
		}
		set
		{
			m_autoHidePane = value;
		}
	}

	internal object AutoHideTabs
	{
		get
		{
			return m_autoHideTabs;
		}
		set
		{
			m_autoHideTabs = value;
		}
	}

	private object TabPageContextMenu
	{
		get
		{
			IDockContent activeContent = ActiveContent;
			if (activeContent == null)
			{
				return null;
			}
			if (activeContent.DockHandler.TabPageContextMenuStrip != null)
			{
				return activeContent.DockHandler.TabPageContextMenuStrip;
			}
			if (activeContent.DockHandler.TabPageContextMenu != null)
			{
				return activeContent.DockHandler.TabPageContextMenu;
			}
			return null;
		}
	}

	internal bool HasTabPageContextMenu => TabPageContextMenu != null;

	private Rectangle CaptionRectangle
	{
		get
		{
			if (!HasCaption)
			{
				return Rectangle.Empty;
			}
			Rectangle displayingRectangle = DisplayingRectangle;
			int num = displayingRectangle.X;
			int num2 = displayingRectangle.Y;
			int num3 = displayingRectangle.Width;
			int num4 = CaptionControl.MeasureHeight();
			return new Rectangle(num, num2, num3, num4);
		}
	}

	protected internal virtual Rectangle ContentRectangle
	{
		get
		{
			Rectangle displayingRectangle = DisplayingRectangle;
			Rectangle captionRectangle = CaptionRectangle;
			Rectangle tabStripRectangle = TabStripRectangle;
			int num = displayingRectangle.X;
			int num2 = displayingRectangle.Y + ((!captionRectangle.IsEmpty) ? captionRectangle.Height : 0);
			if (DockState == DockState.Document && DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Top)
			{
				num2 += tabStripRectangle.Height;
			}
			int num3 = displayingRectangle.Width;
			int num4 = displayingRectangle.Height - captionRectangle.Height - tabStripRectangle.Height;
			return new Rectangle(num, num2, num3, num4);
		}
	}

	internal Rectangle TabStripRectangle
	{
		get
		{
			if (Appearance == AppearanceStyle.ToolWindow)
			{
				return TabStripRectangle_ToolWindow;
			}
			return TabStripRectangle_Document;
		}
	}

	private Rectangle TabStripRectangle_ToolWindow
	{
		get
		{
			if (DisplayingContents.Count <= 1 || IsAutoHide)
			{
				return Rectangle.Empty;
			}
			Rectangle displayingRectangle = DisplayingRectangle;
			int num = displayingRectangle.Width;
			int num2 = TabStripControl.MeasureHeight();
			int num3 = displayingRectangle.X;
			int num4 = displayingRectangle.Bottom - num2;
			Rectangle captionRectangle = CaptionRectangle;
			if (captionRectangle.Contains(num3, num4))
			{
				num4 = captionRectangle.Y + captionRectangle.Height;
			}
			return new Rectangle(num3, num4, num, num2);
		}
	}

	private Rectangle TabStripRectangle_Document
	{
		get
		{
			if (DisplayingContents.Count == 0)
			{
				return Rectangle.Empty;
			}
			if (DisplayingContents.Count == 1 && DockPanel.DocumentStyle == DocumentStyle.DockingSdi)
			{
				return Rectangle.Empty;
			}
			Rectangle displayingRectangle = DisplayingRectangle;
			int num = displayingRectangle.X;
			int num2 = displayingRectangle.Width;
			int num3 = TabStripControl.MeasureHeight();
			int num4 = 0;
			num4 = ((DockPanel.DocumentTabStripLocation != DocumentTabStripLocation.Bottom) ? displayingRectangle.Y : (displayingRectangle.Height - num3));
			return new Rectangle(num, num4, num2, num3);
		}
	}

	public virtual string CaptionText
	{
		get
		{
			if (ActiveContent != null)
			{
				return ActiveContent.DockHandler.TabText;
			}
			return string.Empty;
		}
	}

	public DockContentCollection Contents => m_contents;

	public DockContentCollection DisplayingContents => m_displayingContents;

	public DockPanel DockPanel => m_dockPanel;

	private bool HasCaption
	{
		get
		{
			if (DockState == DockState.Document || DockState == DockState.Hidden || DockState == DockState.Unknown || (DockState == DockState.Float && FloatWindow.VisibleNestedPanes.Count <= 1))
			{
				return false;
			}
			return true;
		}
	}

	public bool IsActivated => m_isActivated;

	public bool IsActiveDocumentPane => m_isActiveDocumentPane;

	public bool IsActivePane => this == DockPanel.ActivePane;

	public bool IsAutoHide => DockHelper.IsDockStateAutoHide(DockState);

	public AppearanceStyle Appearance
	{
		get
		{
			if (DockState != DockState.Document)
			{
				return AppearanceStyle.ToolWindow;
			}
			return AppearanceStyle.Document;
		}
	}

	public Rectangle DisplayingRectangle => base.ClientRectangle;

	public bool IsHidden => m_isHidden;

	public DockWindow DockWindow
	{
		get
		{
			if (m_nestedDockingStatus.NestedPanes != null)
			{
				return m_nestedDockingStatus.NestedPanes.Container as DockWindow;
			}
			return null;
		}
		set
		{
			if (DockWindow != value)
			{
				DockTo(value);
			}
		}
	}

	public FloatWindow FloatWindow
	{
		get
		{
			if (m_nestedDockingStatus.NestedPanes != null)
			{
				return m_nestedDockingStatus.NestedPanes.Container as FloatWindow;
			}
			return null;
		}
		set
		{
			if (FloatWindow != value)
			{
				DockTo(value);
			}
		}
	}

	public NestedDockingStatus NestedDockingStatus => m_nestedDockingStatus;

	public bool IsFloat => m_isFloat;

	public INestedPanesContainer NestedPanesContainer
	{
		get
		{
			if (NestedDockingStatus.NestedPanes == null)
			{
				return null;
			}
			return NestedDockingStatus.NestedPanes.Container;
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
			SetDockState(value);
		}
	}

	private bool IsRefreshStateChangeSuspended => m_countRefreshStateChange != 0;

	Control IDragSource.DragControl => this;

	public IDockContent MouseOverTab { get; set; }

	private SplitterControlBase Splitter => m_splitter;

	internal Rectangle SplitterBounds
	{
		set
		{
			Splitter.Bounds = value;
		}
	}

	internal DockAlignment SplitterAlignment
	{
		set
		{
			Splitter.Alignment = value;
		}
	}

	public event EventHandler DockStateChanged
	{
		add
		{
			base.Events.AddHandler(DockStateChangedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(DockStateChangedEvent, value);
		}
	}

	public event EventHandler IsActivatedChanged
	{
		add
		{
			base.Events.AddHandler(IsActivatedChangedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(IsActivatedChangedEvent, value);
		}
	}

	public event EventHandler IsActiveDocumentPaneChanged
	{
		add
		{
			base.Events.AddHandler(IsActiveDocumentPaneChangedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(IsActiveDocumentPaneChangedEvent, value);
		}
	}

	protected internal DockPane(IDockContent content, DockState visibleState, bool show)
	{
		InternalConstruct(content, visibleState, flagBounds: false, Rectangle.Empty, null, DockAlignment.Right, 0.5, show);
	}

	protected internal DockPane(IDockContent content, FloatWindow floatWindow, bool show)
	{
		if (floatWindow == null)
		{
			throw new ArgumentNullException("floatWindow");
		}
		InternalConstruct(content, DockState.Float, flagBounds: false, Rectangle.Empty, floatWindow.NestedPanes.GetDefaultPreviousPane(this), DockAlignment.Right, 0.5, show);
	}

	protected internal DockPane(IDockContent content, DockPane previousPane, DockAlignment alignment, double proportion, bool show)
	{
		if (previousPane == null)
		{
			throw new ArgumentNullException("previousPane");
		}
		InternalConstruct(content, previousPane.DockState, flagBounds: false, Rectangle.Empty, previousPane, alignment, proportion, show);
	}

	protected internal DockPane(IDockContent content, Rectangle floatWindowBounds, bool show)
	{
		InternalConstruct(content, DockState.Float, flagBounds: true, floatWindowBounds, null, DockAlignment.Right, 0.5, show);
	}

	private void InternalConstruct(IDockContent content, DockState dockState, bool flagBounds, Rectangle floatWindowBounds, DockPane prevPane, DockAlignment alignment, double proportion, bool show)
	{
		if (dockState == DockState.Hidden || dockState == DockState.Unknown)
		{
			throw new ArgumentException(Strings.DockPane_SetDockState_InvalidState);
		}
		if (content == null)
		{
			throw new ArgumentNullException(Strings.DockPane_Constructor_NullContent);
		}
		if (content.DockHandler.DockPanel == null)
		{
			throw new ArgumentException(Strings.DockPane_Constructor_NullDockPanel);
		}
		SuspendLayout();
		SetStyle(ControlStyles.Selectable, value: false);
		m_isFloat = dockState == DockState.Float;
		m_contents = new DockContentCollection();
		m_displayingContents = new DockContentCollection(this);
		m_dockPanel = content.DockHandler.DockPanel;
		m_dockPanel.AddPane(this);
		m_splitter = content.DockHandler.DockPanel.Theme.Extender.DockPaneSplitterControlFactory.CreateSplitterControl(this);
		m_nestedDockingStatus = new NestedDockingStatus(this);
		m_captionControl = DockPanel.Theme.Extender.DockPaneCaptionFactory.CreateDockPaneCaption(this);
		m_tabStripControl = DockPanel.Theme.Extender.DockPaneStripFactory.CreateDockPaneStrip(this);
		base.Controls.AddRange(new Control[2] { m_captionControl, m_tabStripControl });
		DockPanel.SuspendLayout(allWindows: true);
		if (flagBounds)
		{
			FloatWindow = DockPanel.Theme.Extender.FloatWindowFactory.CreateFloatWindow(DockPanel, this, floatWindowBounds);
		}
		else if (prevPane != null)
		{
			DockTo(prevPane.NestedPanesContainer, prevPane, alignment, proportion);
		}
		SetDockState(dockState);
		if (show)
		{
			content.DockHandler.Pane = this;
		}
		else if (IsFloat)
		{
			content.DockHandler.FloatPane = this;
		}
		else
		{
			content.DockHandler.PanelPane = this;
		}
		ResumeLayout();
		DockPanel.ResumeLayout(performLayout: true, allWindows: true);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Win32Helper.IsRunningOnMono)
			{
				if (m_isDisposing)
				{
					return;
				}
				m_isDisposing = true;
			}
			m_dockState = DockState.Unknown;
			if (NestedPanesContainer != null)
			{
				NestedPanesContainer.NestedPanes.Remove(this);
			}
			if (DockPanel != null)
			{
				DockPanel.RemovePane(this);
				m_dockPanel = null;
			}
			Splitter.Dispose();
			if (m_autoHidePane != null)
			{
				m_autoHidePane.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	internal void ClearLastActiveContent()
	{
		m_activeContent = null;
	}

	internal void ShowTabPageContextMenu(Control control, Point position)
	{
		object tabPageContextMenu = TabPageContextMenu;
		if (tabPageContextMenu != null)
		{
			if (tabPageContextMenu is ContextMenuStrip contextMenuStrip)
			{
				contextMenuStrip.Show(control, position);
			}
			else if (tabPageContextMenu is ContextMenu contextMenu)
			{
				contextMenu.Show(this, position);
			}
		}
	}

	internal void SetIsActivated(bool value)
	{
		if (m_isActivated != value)
		{
			m_isActivated = value;
			if (DockState != DockState.Document)
			{
				RefreshChanges(performLayout: false);
			}
			OnIsActivatedChanged(EventArgs.Empty);
		}
	}

	internal void SetIsActiveDocumentPane(bool value)
	{
		if (m_isActiveDocumentPane != value)
		{
			m_isActiveDocumentPane = value;
			if (DockState == DockState.Document)
			{
				RefreshChanges();
			}
			OnIsActiveDocumentPaneChanged(EventArgs.Empty);
		}
	}

	public bool IsDockStateValid(DockState dockState)
	{
		foreach (IDockContent content in Contents)
		{
			if (!content.DockHandler.IsDockStateValid(dockState))
			{
				return false;
			}
		}
		return true;
	}

	public void Activate()
	{
		if (DockHelper.IsDockStateAutoHide(DockState) && DockPanel.ActiveAutoHideContent != ActiveContent)
		{
			DockPanel.ActiveAutoHideContent = ActiveContent;
		}
		else if (!IsActivated && ActiveContent != null)
		{
			ActiveContent.DockHandler.Activate();
		}
	}

	internal void AddContent(IDockContent content)
	{
		if (!Contents.Contains(content))
		{
			Contents.Add(content);
		}
	}

	internal void Close()
	{
		Dispose();
	}

	public void CloseActiveContent()
	{
		CloseContent(ActiveContent);
	}

	internal void CloseContent(IDockContent content)
	{
		if (content == null || !content.DockHandler.CloseButton)
		{
			return;
		}
		DockPanel dockPanel = DockPanel;
		dockPanel.SuspendLayout(allWindows: true);
		try
		{
			if (content.DockHandler.HideOnClose)
			{
				content.DockHandler.Hide();
				NestedDockingStatus.NestedPanes.SwitchPaneWithFirstChild(this);
			}
			else
			{
				content.DockHandler.Close();
			}
		}
		finally
		{
			dockPanel.ResumeLayout(performLayout: true, allWindows: true);
		}
	}

	private HitTestResult GetHitTest(Point ptMouse)
	{
		Point pt = PointToClient(ptMouse);
		if (CaptionRectangle.Contains(pt))
		{
			return new HitTestResult(HitTestArea.Caption, -1);
		}
		if (ContentRectangle.Contains(pt))
		{
			return new HitTestResult(HitTestArea.Content, -1);
		}
		if (TabStripRectangle.Contains(pt))
		{
			return new HitTestResult(HitTestArea.TabStrip, TabStripControl.HitTest(TabStripControl.PointToClient(ptMouse)));
		}
		return new HitTestResult(HitTestArea.None, -1);
	}

	private void SetIsHidden(bool value)
	{
		if (m_isHidden != value)
		{
			m_isHidden = value;
			if (DockHelper.IsDockStateAutoHide(DockState))
			{
				DockPanel.RefreshAutoHideStrip();
				DockPanel.PerformLayout();
			}
			else if (NestedPanesContainer != null)
			{
				((Control)NestedPanesContainer).PerformLayout();
			}
		}
	}

	protected override void OnLayout(LayoutEventArgs e)
	{
		DockTrace.WritePane(this, "OnLayout begin hidden={0}, active={1}, displaying={2}, contents={3}", IsHidden, ActiveContent?.DockHandler?.Form?.Name ?? "null", DisplayingContents.Count, Contents.Count);
		SetIsHidden(DisplayingContents.Count == 0);
		if (!IsHidden)
		{
			CaptionControl.Bounds = CaptionRectangle;
			TabStripControl.Bounds = TabStripRectangle;
			SetContentBounds();
			foreach (IDockContent content in Contents)
			{
				if (DisplayingContents.Contains(content) && content.DockHandler.FlagClipWindow && content.DockHandler.Form.Visible)
				{
					content.DockHandler.FlagClipWindow = false;
				}
			}
		}
		base.OnLayout(e);
		DockTrace.WritePane(this, "OnLayout end hidden={0}, active={1}, displaying={2}, contents={3}", IsHidden, ActiveContent?.DockHandler?.Form?.Name ?? "null", DisplayingContents.Count, Contents.Count);
	}

	internal void SetContentBounds()
	{
		Rectangle rectangle = ContentRectangle;
		if (DockState == DockState.Document && DockPanel.DocumentStyle == DocumentStyle.DockingMdi)
		{
			rectangle = DockPanel.RectangleToMdiClient(RectangleToScreen(rectangle));
		}
		Rectangle bounds = new Rectangle(-rectangle.Width, rectangle.Y, rectangle.Width, rectangle.Height);
		foreach (IDockContent content in Contents)
		{
			if (content.DockHandler.Pane == this)
			{
				if (content == ActiveContent)
				{
					content.DockHandler.Form.Bounds = rectangle;
				}
				else
				{
					content.DockHandler.Form.Bounds = bounds;
				}
			}
		}
	}

	internal void RefreshChanges()
	{
		RefreshChanges(performLayout: true);
	}

	private void RefreshChanges(bool performLayout)
	{
		if (!base.IsDisposed)
		{
			CaptionControl.RefreshChanges();
			TabStripControl.RefreshChanges();
			if (DockState == DockState.Float && FloatWindow != null)
			{
				FloatWindow.RefreshChanges();
			}
			if (DockHelper.IsDockStateAutoHide(DockState) && DockPanel != null)
			{
				DockPanel.RefreshAutoHideStrip();
				DockPanel.PerformLayout();
			}
			if (performLayout)
			{
				PerformLayout();
			}
		}
	}

	internal void RemoveContent(IDockContent content)
	{
		if (Contents.Contains(content))
		{
			Contents.Remove(content);
		}
	}

	public void SetContentIndex(IDockContent content, int index)
	{
		int num = Contents.IndexOf(content);
		if (num == -1)
		{
			throw new ArgumentException(Strings.DockPane_SetContentIndex_InvalidContent);
		}
		if ((index < 0 || index > Contents.Count - 1) && index != -1)
		{
			throw new ArgumentOutOfRangeException(Strings.DockPane_SetContentIndex_InvalidIndex);
		}
		if (num != index && (num != Contents.Count - 1 || index != -1))
		{
			Contents.Remove(content);
			if (index == -1)
			{
				Contents.Add(content);
			}
			else if (num < index)
			{
				Contents.AddAt(content, index - 1);
			}
			else
			{
				Contents.AddAt(content, index);
			}
			RefreshChanges();
		}
	}

	private void SetParent()
	{
		if (DockState == DockState.Unknown || DockState == DockState.Hidden)
		{
			SetParent(null);
			Splitter.Parent = null;
		}
		else if (DockState == DockState.Float)
		{
			SetParent(FloatWindow);
			Splitter.Parent = FloatWindow;
		}
		else if (DockHelper.IsDockStateAutoHide(DockState))
		{
			SetParent(DockPanel.AutoHideControl);
			Splitter.Parent = null;
		}
		else
		{
			SetParent(DockPanel.DockWindows[DockState]);
			Splitter.Parent = base.Parent;
		}
	}

	private void SetParent(Control value)
	{
		if (base.Parent != value)
		{
			IDockContent focusedContent = GetFocusedContent();
			if (focusedContent != null)
			{
				DockPanel.SaveFocus();
			}
			base.Parent = value;
			focusedContent?.DockHandler.Activate();
		}
	}

	public new void Show()
	{
		Activate();
	}

	internal void TestDrop(IDockDragSource dragSource, DockOutlineBase dockOutline)
	{
		if (dragSource.CanDockTo(this))
		{
			Point mousePosition = Control.MousePosition;
			HitTestResult hitTest = GetHitTest(mousePosition);
			if (hitTest.HitArea == HitTestArea.Caption)
			{
				dockOutline.Show(this, -1);
			}
			else if (hitTest.HitArea == HitTestArea.TabStrip && hitTest.Index != -1)
			{
				dockOutline.Show(this, hitTest.Index);
			}
		}
	}

	internal void ValidateActiveContent()
	{
		DockTrace.WritePane(this, "ValidateActiveContent begin active={0}, displaying={1}, contents={2}", ActiveContent?.DockHandler?.Form?.Name ?? "null", DisplayingContents.Count, Contents.Count);
		if (ActiveContent == null)
		{
			if (DisplayingContents.Count != 0)
			{
				ActiveContent = DisplayingContents[0];
			}
		}
		else
		{
			if (DisplayingContents.IndexOf(ActiveContent) >= 0)
			{
				return;
			}
			IDockContent dockContent = null;
			for (int num = Contents.IndexOf(ActiveContent) - 1; num >= 0; num--)
			{
				if (Contents[num].DockHandler.DockState == DockState)
				{
					dockContent = Contents[num];
					break;
				}
			}
			IDockContent dockContent2 = null;
			for (int i = Contents.IndexOf(ActiveContent) + 1; i < Contents.Count; i++)
			{
				if (Contents[i].DockHandler.DockState == DockState)
				{
					dockContent2 = Contents[i];
					break;
				}
			}
			if (dockContent != null)
			{
				ActiveContent = dockContent;
			}
			else if (dockContent2 != null)
			{
				ActiveContent = dockContent2;
			}
			else
			{
				ActiveContent = null;
			}
		}
		DockTrace.WritePane(this, "ValidateActiveContent end active={0}, displaying={1}, contents={2}", ActiveContent?.DockHandler?.Form?.Name ?? "null", DisplayingContents.Count, Contents.Count);
	}

	protected virtual void OnDockStateChanged(EventArgs e)
	{
		((EventHandler)base.Events[DockStateChangedEvent])?.Invoke(this, e);
	}

	protected virtual void OnIsActivatedChanged(EventArgs e)
	{
		((EventHandler)base.Events[IsActivatedChangedEvent])?.Invoke(this, e);
	}

	protected virtual void OnIsActiveDocumentPaneChanged(EventArgs e)
	{
		((EventHandler)base.Events[IsActiveDocumentPaneChangedEvent])?.Invoke(this, e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		if (base.IsHandleCreated)
		{
			if (m_layoutPending)
			{
				System.Diagnostics.Debug.WriteLine("[Resize] DockPane.OnSizeChanged (debounce skip)");
				return;
			}
			m_layoutPending = true;
			System.Diagnostics.Debug.WriteLine("[Resize] DockPane.OnSizeChanged (queue layout)");
			BeginInvoke((MethodInvoker)delegate
			{
				m_layoutPending = false;
				base.OnSizeChanged(e);
			});
		}
	}

	public DockPane SetDockState(DockState value)
	{
		if (value == DockState.Unknown || value == DockState.Hidden)
		{
			throw new InvalidOperationException(Strings.DockPane_SetDockState_InvalidState);
		}
		if (value == DockState.Float == IsFloat)
		{
			InternalSetDockState(value);
			return this;
		}
		if (DisplayingContents.Count == 0)
		{
			return null;
		}
		IDockContent dockContent = null;
		for (int i = 0; i < DisplayingContents.Count; i++)
		{
			IDockContent dockContent2 = DisplayingContents[i];
			if (dockContent2.DockHandler.IsDockStateValid(value))
			{
				dockContent = dockContent2;
				break;
			}
		}
		if (dockContent == null)
		{
			return null;
		}
		dockContent.DockHandler.DockState = value;
		DockPane pane = dockContent.DockHandler.Pane;
		DockPanel.SuspendLayout(allWindows: true);
		for (int j = 0; j < DisplayingContents.Count; j++)
		{
			IDockContent dockContent3 = DisplayingContents[j];
			if (dockContent3.DockHandler.IsDockStateValid(value))
			{
				dockContent3.DockHandler.Pane = pane;
			}
		}
		DockPanel.ResumeLayout(performLayout: true, allWindows: true);
		return pane;
	}

	private void InternalSetDockState(DockState value)
	{
		if (m_dockState != value)
		{
			DockState dockState = m_dockState;
			INestedPanesContainer nestedPanesContainer = NestedPanesContainer;
			m_dockState = value;
			SuspendRefreshStateChange();
			IDockContent focusedContent = GetFocusedContent();
			if (focusedContent != null)
			{
				DockPanel.SaveFocus();
			}
			if (!IsFloat)
			{
				DockWindow = DockPanel.DockWindows[DockState];
			}
			else if (FloatWindow == null)
			{
				FloatWindow = DockPanel.Theme.Extender.FloatWindowFactory.CreateFloatWindow(DockPanel, this);
			}
			if (focusedContent != null && !Win32Helper.IsRunningOnMono)
			{
				DockPanel.ContentFocusManager.Activate(focusedContent);
			}
			ResumeRefreshStateChange(nestedPanesContainer, dockState);
		}
	}

	private void SuspendRefreshStateChange()
	{
		m_countRefreshStateChange++;
		DockPanel.SuspendLayout(allWindows: true);
	}

	private void ResumeRefreshStateChange()
	{
		m_countRefreshStateChange--;
		DockPanel.ResumeLayout(performLayout: true, allWindows: true);
	}

	private void ResumeRefreshStateChange(INestedPanesContainer oldContainer, DockState oldDockState)
	{
		ResumeRefreshStateChange();
		RefreshStateChange(oldContainer, oldDockState);
	}

	private void RefreshStateChange(INestedPanesContainer oldContainer, DockState oldDockState)
	{
		if (IsRefreshStateChangeSuspended)
		{
			return;
		}
		SuspendRefreshStateChange();
		DockPanel.SuspendLayout(allWindows: true);
		IDockContent focusedContent = GetFocusedContent();
		if (focusedContent != null)
		{
			DockPanel.SaveFocus();
		}
		SetParent();
		if (ActiveContent != null)
		{
			ActiveContent.DockHandler.SetDockState(ActiveContent.DockHandler.IsHidden, DockState, ActiveContent.DockHandler.Pane);
		}
		foreach (IDockContent content in Contents)
		{
			if (content.DockHandler.Pane == this)
			{
				content.DockHandler.SetDockState(content.DockHandler.IsHidden, DockState, content.DockHandler.Pane);
			}
		}
		if (oldContainer != null)
		{
			Control control = (Control)oldContainer;
			if (oldContainer.DockState == oldDockState && !control.IsDisposed)
			{
				control.PerformLayout();
			}
		}
		if (DockHelper.IsDockStateAutoHide(oldDockState))
		{
			DockPanel.RefreshActiveAutoHideContent();
		}
		if (NestedPanesContainer.DockState == DockState)
		{
			((Control)NestedPanesContainer).PerformLayout();
		}
		if (DockHelper.IsDockStateAutoHide(DockState))
		{
			DockPanel.RefreshActiveAutoHideContent();
		}
		if (DockHelper.IsDockStateAutoHide(oldDockState) || DockHelper.IsDockStateAutoHide(DockState))
		{
			DockPanel.RefreshAutoHideStrip();
			DockPanel.PerformLayout();
		}
		ResumeRefreshStateChange();
		focusedContent?.DockHandler.Activate();
		DockPanel.ResumeLayout(performLayout: true, allWindows: true);
		if (oldDockState != DockState)
		{
			OnDockStateChanged(EventArgs.Empty);
		}
	}

	private IDockContent GetFocusedContent()
	{
		IDockContent result = null;
		foreach (IDockContent content in Contents)
		{
			if (content.DockHandler.Form.ContainsFocus)
			{
				result = content;
				break;
			}
		}
		return result;
	}

	public DockPane DockTo(INestedPanesContainer container)
	{
		if (container == null)
		{
			throw new InvalidOperationException(Strings.DockPane_DockTo_NullContainer);
		}
		return DockTo(alignment: (container.DockState != DockState.DockLeft && container.DockState != DockState.DockRight) ? DockAlignment.Right : DockAlignment.Bottom, container: container, previousPane: container.NestedPanes.GetDefaultPreviousPane(this), proportion: 0.5);
	}

	public DockPane DockTo(INestedPanesContainer container, DockPane previousPane, DockAlignment alignment, double proportion)
	{
		if (container == null)
		{
			throw new InvalidOperationException(Strings.DockPane_DockTo_NullContainer);
		}
		if (container.IsFloat == IsFloat)
		{
			InternalAddToDockList(container, previousPane, alignment, proportion);
			return this;
		}
		if (GetFirstContent(container.DockState) == null)
		{
			return null;
		}
		DockPanel.DummyContent.DockPanel = DockPanel;
		DockPane dockPane = ((!container.IsFloat) ? DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(DockPanel.DummyContent, container.DockState, show: true) : DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(DockPanel.DummyContent, (FloatWindow)container, show: true));
		dockPane.DockTo(container, previousPane, alignment, proportion);
		SetVisibleContentsToPane(dockPane);
		DockPanel.DummyContent.DockPanel = null;
		return dockPane;
	}

	private void SetVisibleContentsToPane(DockPane pane)
	{
		SetVisibleContentsToPane(pane, ActiveContent);
	}

	private void SetVisibleContentsToPane(DockPane pane, IDockContent activeContent)
	{
		for (int i = 0; i < DisplayingContents.Count; i++)
		{
			IDockContent dockContent = DisplayingContents[i];
			if (dockContent.DockHandler.IsDockStateValid(pane.DockState))
			{
				dockContent.DockHandler.Pane = pane;
				i--;
			}
		}
		if (activeContent.DockHandler.Pane == pane)
		{
			pane.ActiveContent = activeContent;
		}
	}

	private void InternalAddToDockList(INestedPanesContainer container, DockPane prevPane, DockAlignment alignment, double proportion)
	{
		if (container.DockState == DockState.Float != IsFloat)
		{
			throw new InvalidOperationException(Strings.DockPane_DockTo_InvalidContainer);
		}
		int num = container.NestedPanes.Count;
		if (container.NestedPanes.Contains(this))
		{
			num--;
		}
		if (prevPane == null && num > 0)
		{
			throw new InvalidOperationException(Strings.DockPane_DockTo_NullPrevPane);
		}
		if (prevPane != null && !container.NestedPanes.Contains(prevPane))
		{
			throw new InvalidOperationException(Strings.DockPane_DockTo_NoPrevPane);
		}
		if (prevPane == this)
		{
			throw new InvalidOperationException(Strings.DockPane_DockTo_SelfPrevPane);
		}
		INestedPanesContainer nestedPanesContainer = NestedPanesContainer;
		DockState dockState = DockState;
		container.NestedPanes.Add(this);
		NestedDockingStatus.SetStatus(container.NestedPanes, prevPane, alignment, proportion);
		if (DockHelper.IsDockWindowState(DockState))
		{
			m_dockState = container.DockState;
		}
		RefreshStateChange(nestedPanesContainer, dockState);
	}

	public void SetNestedDockingProportion(double proportion)
	{
		NestedDockingStatus.SetStatus(NestedDockingStatus.NestedPanes, NestedDockingStatus.PreviousPane, NestedDockingStatus.Alignment, proportion);
		if (NestedPanesContainer != null)
		{
			((Control)NestedPanesContainer).PerformLayout();
		}
	}

	public DockPane Float()
	{
		DockPanel.SuspendLayout(allWindows: true);
		IDockContent activeContent = ActiveContent;
		DockPane dockPane = GetFloatPaneFromContents();
		if (dockPane == null)
		{
			IDockContent firstContent = GetFirstContent(DockState.Float);
			if (firstContent == null)
			{
				DockPanel.ResumeLayout(performLayout: true, allWindows: true);
				return null;
			}
			dockPane = DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(firstContent, DockState.Float, show: true);
		}
		SetVisibleContentsToPane(dockPane, activeContent);
		DockPanel.ResumeLayout(performLayout: true, allWindows: true);
		return dockPane;
	}

	private DockPane GetFloatPaneFromContents()
	{
		DockPane dockPane = null;
		for (int i = 0; i < DisplayingContents.Count; i++)
		{
			IDockContent dockContent = DisplayingContents[i];
			if (dockContent.DockHandler.IsDockStateValid(DockState.Float))
			{
				if (dockPane != null && dockContent.DockHandler.FloatPane != dockPane)
				{
					return null;
				}
				dockPane = dockContent.DockHandler.FloatPane;
			}
		}
		return dockPane;
	}

	private IDockContent GetFirstContent(DockState dockState)
	{
		for (int i = 0; i < DisplayingContents.Count; i++)
		{
			IDockContent dockContent = DisplayingContents[i];
			if (dockContent.DockHandler.IsDockStateValid(dockState))
			{
				return dockContent;
			}
		}
		return null;
	}

	public void RestoreToPanel()
	{
		DockPanel.SuspendLayout(allWindows: true);
		_ = DockPanel.ActiveContent;
		for (int num = DisplayingContents.Count - 1; num >= 0; num--)
		{
			IDockContent dockContent = DisplayingContents[num];
			if (dockContent.DockHandler.CheckDockState(isFloat: false) != DockState.Unknown)
			{
				dockContent.DockHandler.IsFloat = false;
			}
		}
		DockPanel.ResumeLayout(performLayout: true, allWindows: true);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 33)
		{
			Activate();
		}
		base.WndProc(ref m);
	}

	bool IDockDragSource.IsDockStateValid(DockState dockState)
	{
		return IsDockStateValid(dockState);
	}

	bool IDockDragSource.CanDockTo(DockPane pane)
	{
		if (!IsDockStateValid(pane.DockState))
		{
			return false;
		}
		if (pane == this)
		{
			return false;
		}
		return true;
	}

	Rectangle IDockDragSource.BeginDrag(Point ptMouse)
	{
		Point location = PointToScreen(new Point(0, 0));
		DockPane floatPane = ActiveContent.DockHandler.FloatPane;
		Size size = ((DockState != DockState.Float && floatPane != null && floatPane.FloatWindow.NestedPanes.Count == 1) ? floatPane.FloatWindow.Size : DockPanel.DefaultFloatWindowSize);
		if (ptMouse.X > location.X + size.Width)
		{
			location.X += ptMouse.X - (location.X + size.Width) + DockPanel.Theme.Measures.SplitterSize;
		}
		return new Rectangle(location, size);
	}

	void IDockDragSource.EndDrag()
	{
	}

	public void FloatAt(Rectangle floatWindowBounds)
	{
		if (FloatWindow == null || FloatWindow.NestedPanes.Count != 1)
		{
			FloatWindow = DockPanel.Theme.Extender.FloatWindowFactory.CreateFloatWindow(DockPanel, this, floatWindowBounds);
		}
		else
		{
			FloatWindow.Bounds = floatWindowBounds;
		}
		DockState = DockState.Float;
		NestedDockingStatus.NestedPanes.SwitchPaneWithFirstChild(this);
	}

	public void DockTo(DockPane pane, DockStyle dockStyle, int contentIndex)
	{
		switch (dockStyle)
		{
		case DockStyle.Fill:
		{
			IDockContent activeContent = ActiveContent;
			for (int num = Contents.Count - 1; num >= 0; num--)
			{
				IDockContent dockContent = Contents[num];
				if (dockContent.DockHandler.DockState == DockState)
				{
					dockContent.DockHandler.Pane = pane;
					if (contentIndex != -1)
					{
						pane.SetContentIndex(dockContent, contentIndex);
					}
				}
			}
			pane.ActiveContent = activeContent;
			return;
		}
		case DockStyle.Left:
			DockTo(pane.NestedPanesContainer, pane, DockAlignment.Left, 0.5);
			break;
		case DockStyle.Right:
			DockTo(pane.NestedPanesContainer, pane, DockAlignment.Right, 0.5);
			break;
		case DockStyle.Top:
			DockTo(pane.NestedPanesContainer, pane, DockAlignment.Top, 0.5);
			break;
		case DockStyle.Bottom:
			DockTo(pane.NestedPanesContainer, pane, DockAlignment.Bottom, 0.5);
			break;
		}
		DockState = pane.DockState;
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
			DockState = DockState.DockTop;
			break;
		case DockStyle.Bottom:
			DockState = DockState.DockBottom;
			break;
		case DockStyle.Left:
			DockState = DockState.DockLeft;
			break;
		case DockStyle.Right:
			DockState = DockState.DockRight;
			break;
		case DockStyle.Fill:
			DockState = DockState.Document;
			break;
		}
	}

	protected override void OnParentChanged(EventArgs e)
	{
		base.OnParentChanged(e);
		DockWindow dockWindow = base.Parent as DockWindow;
		if (dockWindow != _lastParentWindow)
		{
			if (_lastParentWindow != null)
			{
				_lastParentWindow.PerformLayout();
			}
			_lastParentWindow = dockWindow;
		}
	}

	static DockPane()
	{
		DockStateChangedEvent = new object();
		IsActivatedChangedEvent = new object();
		IsActiveDocumentPaneChangedEvent = new object();
	}
}
