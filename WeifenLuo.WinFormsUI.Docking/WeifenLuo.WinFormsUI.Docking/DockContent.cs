using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public class DockContent : Form, IDockContent, IContextMenuStripHost
{
	private DockContentHandler m_dockHandler;

	private bool m_showTabText = true;

	private string m_tabText;

	private bool m_layoutPending;

	private static readonly object DockStateChangedEvent;

	[Browsable(false)]
	public DockContentHandler DockHandler => m_dockHandler;

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_AllowEndUserDocking_Description")]
	[DefaultValue(true)]
	public bool AllowEndUserDocking
	{
		get
		{
			return DockHandler.AllowEndUserDocking;
		}
		set
		{
			DockHandler.AllowEndUserDocking = value;
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_DockAreas_Description")]
	[DefaultValue(DockAreas.Float | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom | DockAreas.Document)]
	public DockAreas DockAreas
	{
		get
		{
			return DockHandler.DockAreas;
		}
		set
		{
			DockHandler.DockAreas = value;
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_AutoHidePortion_Description")]
	[DefaultValue(0.25)]
	public double AutoHidePortion
	{
		get
		{
			return DockHandler.AutoHidePortion;
		}
		set
		{
			DockHandler.AutoHidePortion = value;
		}
	}

	[DefaultValue(true)]
	public bool ShowTabText
	{
		get
		{
			return m_showTabText;
		}
		set
		{
			DockHandler.ShowTabText = (m_showTabText = value);
		}
	}

	[Localizable(true)]
	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_TabText_Description")]
	[DefaultValue(null)]
	public string TabText
	{
		get
		{
			return m_tabText;
		}
		set
		{
			DockHandler.TabText = (m_tabText = value);
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_CloseButton_Description")]
	[DefaultValue(true)]
	public bool CloseButton
	{
		get
		{
			return DockHandler.CloseButton;
		}
		set
		{
			DockHandler.CloseButton = value;
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_CloseButtonVisible_Description")]
	[DefaultValue(true)]
	public bool CloseButtonVisible
	{
		get
		{
			return DockHandler.CloseButtonVisible;
		}
		set
		{
			DockHandler.CloseButtonVisible = value;
		}
	}

	[DefaultValue(true)]
	public bool ShowText { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DockPanel DockPanel
	{
		get
		{
			return DockHandler.DockPanel;
		}
		set
		{
			DockHandler.DockPanel = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DockState DockState
	{
		get
		{
			return DockHandler.DockState;
		}
		set
		{
			DockHandler.DockState = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DockPane Pane
	{
		get
		{
			return DockHandler.Pane;
		}
		set
		{
			DockHandler.Pane = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsHidden
	{
		get
		{
			return DockHandler.IsHidden;
		}
		set
		{
			DockHandler.IsHidden = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DockState VisibleState
	{
		get
		{
			return DockHandler.VisibleState;
		}
		set
		{
			DockHandler.VisibleState = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsFloat
	{
		get
		{
			return DockHandler.IsFloat;
		}
		set
		{
			DockHandler.IsFloat = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DockPane PanelPane
	{
		get
		{
			return DockHandler.PanelPane;
		}
		set
		{
			DockHandler.PanelPane = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DockPane FloatPane
	{
		get
		{
			return DockHandler.FloatPane;
		}
		set
		{
			DockHandler.FloatPane = value;
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_HideOnClose_Description")]
	[DefaultValue(false)]
	public bool HideOnClose
	{
		get
		{
			return DockHandler.HideOnClose;
		}
		set
		{
			DockHandler.HideOnClose = value;
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_ShowHint_Description")]
	[DefaultValue(DockState.Unknown)]
	public DockState ShowHint
	{
		get
		{
			return DockHandler.ShowHint;
		}
		set
		{
			DockHandler.ShowHint = value;
		}
	}

	[Browsable(false)]
	public bool IsActivated => DockHandler.IsActivated;

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_TabPageContextMenu_Description")]
	[DefaultValue(null)]
	public ContextMenu TabPageContextMenu
	{
		get
		{
			return DockHandler.TabPageContextMenu;
		}
		set
		{
			DockHandler.TabPageContextMenu = value;
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockContent_TabPageContextMenuStrip_Description")]
	[DefaultValue(null)]
	public ContextMenuStrip TabPageContextMenuStrip
	{
		get
		{
			return DockHandler.TabPageContextMenuStrip;
		}
		set
		{
			DockHandler.TabPageContextMenuStrip = value;
		}
	}

	[Localizable(true)]
	[Category("Appearance")]
	[LocalizedDescription("DockContent_ToolTipText_Description")]
	[DefaultValue(null)]
	public string ToolTipText
	{
		get
		{
			return DockHandler.ToolTipText;
		}
		set
		{
			DockHandler.ToolTipText = value;
		}
	}

	[LocalizedCategory("Category_PropertyChanged")]
	[LocalizedDescription("Pane_DockStateChanged_Description")]
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

	public DockContent()
	{
		m_dockHandler = new DockContentHandler(this, GetPersistString);
		m_dockHandler.DockStateChanged += DockHandler_DockStateChanged;
		if (PatchController.EnableFontInheritanceFix != true)
		{
			base.ParentChanged += DockContent_ParentChanged;
		}
	}

	private void DockContent_ParentChanged(object Sender, EventArgs e)
	{
		if (base.Parent != null)
		{
			Font = base.Parent.Font;
		}
	}

	private bool ShouldSerializeTabText()
	{
		return m_tabText != null;
	}

	protected virtual string GetPersistString()
	{
		return GetType().ToString();
	}

	public bool IsDockStateValid(DockState dockState)
	{
		return DockHandler.IsDockStateValid(dockState);
	}

	public void ApplyTheme()
	{
		DockHandler.ApplyTheme();
		if (DockPanel != null)
		{
			if (base.MainMenuStrip != null)
			{
				DockPanel.Theme.ApplyTo(base.MainMenuStrip);
			}
			if (ContextMenuStrip != null)
			{
				DockPanel.Theme.ApplyTo(ContextMenuStrip);
			}
		}
	}

	public new void Activate()
	{
		DockHandler.Activate();
	}

	public new void Hide()
	{
		DockHandler.Hide();
	}

	public new void Show()
	{
		DockHandler.Show();
	}

	public void Show(DockPanel dockPanel)
	{
		DockHandler.Show(dockPanel);
	}

	public void Show(DockPanel dockPanel, DockState dockState)
	{
		DockHandler.Show(dockPanel, dockState);
	}

	public void Show(DockPanel dockPanel, DockState dockState, bool activate)
	{
		DockHandler.Show(dockPanel, dockState, activate);
	}

	public void Show(DockPanel dockPanel, Rectangle floatWindowBounds)
	{
		DockHandler.Show(dockPanel, floatWindowBounds);
	}

	public void Show(DockPane pane, IDockContent beforeContent)
	{
		DockHandler.Show(pane, beforeContent);
	}

	public void Show(DockPane previousPane, DockAlignment alignment, double proportion)
	{
		DockHandler.Show(previousPane, alignment, proportion);
	}

	public void FloatAt(Rectangle floatWindowBounds)
	{
		DockHandler.FloatAt(floatWindowBounds);
	}

	public void DockTo(DockPane paneTo, DockStyle dockStyle, int contentIndex)
	{
		DockHandler.DockTo(paneTo, dockStyle, contentIndex);
	}

	public void DockTo(DockPanel panel, DockStyle dockStyle)
	{
		DockHandler.DockTo(panel, dockStyle);
	}

	void IDockContent.OnActivated(EventArgs e)
	{
		OnActivated(e);
	}

	void IDockContent.OnDeactivate(EventArgs e)
	{
		OnDeactivate(e);
	}

	private void DockHandler_DockStateChanged(object sender, EventArgs e)
	{
		OnDockStateChanged(e);
	}

	protected virtual void OnDockStateChanged(EventArgs e)
	{
		((EventHandler)base.Events[DockStateChangedEvent])?.Invoke(this, e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		if (DockPanel != null && DockPanel.SupportDeeplyNestedContent && base.IsHandleCreated)
		{
			if (m_layoutPending)
			{
				System.Diagnostics.Debug.WriteLine("[Resize] DockContent.OnSizeChanged (debounce skip)");
				return;
			}
			m_layoutPending = true;
			System.Diagnostics.Debug.WriteLine("[Resize] DockContent.OnSizeChanged (queue layout)");
			BeginInvoke((MethodInvoker)delegate
			{
				m_layoutPending = false;
				base.OnSizeChanged(e);
			});
		}
		else
		{
			base.OnSizeChanged(e);
		}
	}

	static DockContent()
	{
		DockStateChangedEvent = new object();
	}
}
