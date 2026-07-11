using System;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public class FloatWindow : Form, INestedPanesContainer, IDockDragSource, IDragSource
{
	private NestedPaneCollection m_nestedPanes;

	internal const int WM_CHECKDISPOSE = 1025;

	private bool m_allowEndUserDocking = true;

	private DockPanel m_dockPanel;

	private int m_preDragExStyle;

	public bool AllowEndUserDocking
	{
		get
		{
			return m_allowEndUserDocking;
		}
		set
		{
			m_allowEndUserDocking = value;
		}
	}

	public static bool DoubleClickTitleBarToDockDefault { get; set; }

	public bool DoubleClickTitleBarToDock { get; set; } = DoubleClickTitleBarToDockDefault;

	public NestedPaneCollection NestedPanes => m_nestedPanes;

	public VisibleNestedPaneCollection VisibleNestedPanes => NestedPanes.VisibleNestedPanes;

	public DockPanel DockPanel => m_dockPanel;

	public DockState DockState => DockState.Float;

	public bool IsFloat => DockState == DockState.Float;

	public virtual Rectangle DisplayingRectangle => base.ClientRectangle;

	Control IDragSource.DragControl => this;

	protected internal FloatWindow(DockPanel dockPanel, DockPane pane)
	{
		InternalConstruct(dockPanel, pane, boundsSpecified: false, Rectangle.Empty);
	}

	protected internal FloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
	{
		InternalConstruct(dockPanel, pane, boundsSpecified: true, bounds);
	}

	private void InternalConstruct(DockPanel dockPanel, DockPane pane, bool boundsSpecified, Rectangle bounds)
	{
		if (dockPanel == null)
		{
			throw new ArgumentNullException(Strings.FloatWindow_Constructor_NullDockPanel);
		}
		m_nestedPanes = new NestedPaneCollection(this);
		base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
		base.ShowInTaskbar = false;
		if (dockPanel.RightToLeft != RightToLeft)
		{
			RightToLeft = dockPanel.RightToLeft;
		}
		if (RightToLeftLayout != dockPanel.RightToLeftLayout)
		{
			RightToLeftLayout = dockPanel.RightToLeftLayout;
		}
		SuspendLayout();
		if (boundsSpecified)
		{
			base.Bounds = bounds;
			base.StartPosition = FormStartPosition.Manual;
		}
		else
		{
			base.StartPosition = FormStartPosition.WindowsDefaultLocation;
			base.Size = dockPanel.DefaultFloatWindowSize;
		}
		m_dockPanel = dockPanel;
		base.Owner = DockPanel.FindForm();
		DockPanel.AddFloatWindow(this);
		if (pane != null)
		{
			pane.FloatWindow = this;
		}
		if (PatchController.EnableFontInheritanceFix == true)
		{
			Font = dockPanel.Font;
		}
		ResumeLayout();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (DockPanel != null)
			{
				DockPanel.RemoveFloatWindow(this);
			}
			m_dockPanel = null;
		}
		base.Dispose(disposing);
	}

	internal bool IsDockStateValid(DockState dockState)
	{
		foreach (DockPane nestedPane in NestedPanes)
		{
			foreach (IDockContent content in nestedPane.Contents)
			{
				if (!DockHelper.IsDockStateValid(dockState, content.DockHandler.DockAreas))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected override void OnActivated(EventArgs e)
	{
		DockPanel.FloatWindows.BringWindowToFront(this);
		base.OnActivated(e);
		foreach (DockPane visibleNestedPane in VisibleNestedPanes)
		{
			foreach (IDockContent content in visibleNestedPane.Contents)
			{
				content.OnActivated(e);
			}
		}
	}

	protected override void OnDeactivate(EventArgs e)
	{
		base.OnDeactivate(e);
		foreach (DockPane visibleNestedPane in VisibleNestedPanes)
		{
			foreach (IDockContent content in visibleNestedPane.Contents)
			{
				content.OnDeactivate(e);
			}
		}
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		VisibleNestedPanes.Refresh();
		RefreshChanges();
		base.Visible = VisibleNestedPanes.Count > 0;
		SetText();
		base.OnLayout(levent);
	}

	internal void SetText()
	{
		DockPane dockPane = ((VisibleNestedPanes.Count == 1) ? VisibleNestedPanes[0] : null);
		if (dockPane == null || dockPane.ActiveContent == null)
		{
			Text = " ";
			base.Icon = null;
		}
		else
		{
			Text = dockPane.ActiveContent.DockHandler.TabText;
			base.Icon = dockPane.ActiveContent.DockHandler.Icon;
		}
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		Rectangle virtualScreen = SystemInformation.VirtualScreen;
		if (y + height > virtualScreen.Bottom)
		{
			y -= y + height - virtualScreen.Bottom;
		}
		if (y < virtualScreen.Top)
		{
			y += virtualScreen.Top - y;
		}
		base.SetBoundsCore(x, y, width, height, specified);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		switch (m.Msg)
		{
		case 161:
			if (!base.IsDisposed)
			{
				if (((!Win32Helper.IsRunningOnMono) ? NativeMethods.SendMessage(base.Handle, 132, 0u, (uint)(int)m.LParam) : 0) == 2 && DockPanel.AllowEndUserDocking && AllowEndUserDocking)
				{
					Activate();
					m_dockPanel.BeginDrag(this);
				}
				else
				{
					base.WndProc(ref m);
				}
			}
			break;
		case 164:
			if ((Win32Helper.IsRunningOnMono ? Win32Helper.HitTestCaption(this) : NativeMethods.SendMessage(base.Handle, 132, 0u, (uint)(int)m.LParam)) == 2)
			{
				DockPane dockPane = ((VisibleNestedPanes.Count == 1) ? VisibleNestedPanes[0] : null);
				if (dockPane != null && dockPane.ActiveContent != null)
				{
					dockPane.ShowTabPageContextMenu(this, PointToClient(Control.MousePosition));
					break;
				}
			}
			base.WndProc(ref m);
			break;
		case 16:
		{
			if (NestedPanes.Count == 0)
			{
				base.WndProc(ref m);
				break;
			}
			for (int num = NestedPanes.Count - 1; num >= 0; num--)
			{
				DockContentCollection contents = NestedPanes[num].Contents;
				for (int num2 = contents.Count - 1; num2 >= 0; num2--)
				{
					IDockContent dockContent = contents[num2];
					if (dockContent.DockHandler.DockState == DockState.Float && dockContent.DockHandler.CloseButton)
					{
						if (dockContent.DockHandler.HideOnClose)
						{
							dockContent.DockHandler.Hide();
						}
						else
						{
							dockContent.DockHandler.Close();
						}
					}
				}
			}
			break;
		}
		case 163:
			if (((!DoubleClickTitleBarToDock || Win32Helper.IsRunningOnMono) ? Win32Helper.HitTestCaption(this) : NativeMethods.SendMessage(base.Handle, 132, 0u, (uint)(int)m.LParam)) != 2)
			{
				base.WndProc(ref m);
				break;
			}
			DockPanel.SuspendLayout(allWindows: true);
			foreach (DockPane nestedPane in NestedPanes)
			{
				if (nestedPane.DockState == DockState.Float)
				{
					nestedPane.RestoreToPanel();
				}
			}
			DockPanel.ResumeLayout(performLayout: true, allWindows: true);
			break;
		case 1025:
			if (NestedPanes.Count == 0)
			{
				Dispose();
			}
			break;
		default:
			base.WndProc(ref m);
			break;
		}
	}

	internal void RefreshChanges()
	{
		if (base.IsDisposed)
		{
			return;
		}
		if (VisibleNestedPanes.Count == 0)
		{
			base.ControlBox = true;
			return;
		}
		for (int num = VisibleNestedPanes.Count - 1; num >= 0; num--)
		{
			DockContentCollection contents = VisibleNestedPanes[num].Contents;
			for (int num2 = contents.Count - 1; num2 >= 0; num2--)
			{
				IDockContent dockContent = contents[num2];
				if (dockContent.DockHandler.DockState == DockState.Float && dockContent.DockHandler.CloseButton && dockContent.DockHandler.CloseButtonVisible)
				{
					base.ControlBox = true;
					return;
				}
			}
		}
		if (base.ControlBox)
		{
			base.ControlBox = false;
		}
	}

	internal void TestDrop(IDockDragSource dragSource, DockOutlineBase dockOutline)
	{
		if (VisibleNestedPanes.Count != 1)
		{
			return;
		}
		DockPane pane = VisibleNestedPanes[0];
		if (dragSource.CanDockTo(pane))
		{
			Point mousePosition = Control.MousePosition;
			uint lParam = Win32Helper.MakeLong(mousePosition.X, mousePosition.Y);
			if (!Win32Helper.IsRunningOnMono && NativeMethods.SendMessage(base.Handle, 132, 0u, lParam) == 2)
			{
				dockOutline.Show(VisibleNestedPanes[0], -1);
			}
		}
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
		if (pane.FloatWindow == this)
		{
			return false;
		}
		return true;
	}

	Rectangle IDockDragSource.BeginDrag(Point ptMouse)
	{
		m_preDragExStyle = NativeMethods.GetWindowLong(base.Handle, -20);
		NativeMethods.SetWindowLong(base.Handle, -20, m_preDragExStyle | 0x80020);
		return base.Bounds;
	}

	void IDockDragSource.EndDrag()
	{
		NativeMethods.SetWindowLong(base.Handle, -20, m_preDragExStyle);
		Invalidate(invalidateChildren: true);
		NativeMethods.SendMessage(base.Handle, 133, 1u, 0u);
	}

	public void FloatAt(Rectangle floatWindowBounds)
	{
		base.Bounds = floatWindowBounds;
	}

	public void DockTo(DockPane pane, DockStyle dockStyle, int contentIndex)
	{
		if (dockStyle == DockStyle.Fill)
		{
			for (int num = NestedPanes.Count - 1; num >= 0; num--)
			{
				DockPane dockPane = NestedPanes[num];
				for (int num2 = dockPane.Contents.Count - 1; num2 >= 0; num2--)
				{
					IDockContent dockContent = dockPane.Contents[num2];
					dockContent.DockHandler.Pane = pane;
					if (contentIndex != -1)
					{
						pane.SetContentIndex(dockContent, contentIndex);
					}
					dockContent.DockHandler.Activate();
				}
			}
			return;
		}
		DockAlignment alignment = DockAlignment.Left;
		switch (dockStyle)
		{
		case DockStyle.Left:
			alignment = DockAlignment.Left;
			break;
		case DockStyle.Right:
			alignment = DockAlignment.Right;
			break;
		case DockStyle.Top:
			alignment = DockAlignment.Top;
			break;
		case DockStyle.Bottom:
			alignment = DockAlignment.Bottom;
			break;
		}
		MergeNestedPanes(VisibleNestedPanes, pane.NestedPanesContainer.NestedPanes, pane, alignment, 0.5);
	}

	public void DockTo(DockPanel panel, DockStyle dockStyle)
	{
		if (panel != DockPanel)
		{
			throw new ArgumentException(Strings.IDockDragSource_DockTo_InvalidPanel, "panel");
		}
		NestedPaneCollection nestedPaneCollection = null;
		switch (dockStyle)
		{
		case DockStyle.Top:
			nestedPaneCollection = DockPanel.DockWindows[DockState.DockTop].NestedPanes;
			break;
		case DockStyle.Bottom:
			nestedPaneCollection = DockPanel.DockWindows[DockState.DockBottom].NestedPanes;
			break;
		case DockStyle.Left:
			nestedPaneCollection = DockPanel.DockWindows[DockState.DockLeft].NestedPanes;
			break;
		case DockStyle.Right:
			nestedPaneCollection = DockPanel.DockWindows[DockState.DockRight].NestedPanes;
			break;
		case DockStyle.Fill:
			nestedPaneCollection = DockPanel.DockWindows[DockState.Document].NestedPanes;
			break;
		}
		DockPane prevPane = null;
		for (int num = nestedPaneCollection.Count - 1; num >= 0; num--)
		{
			if (nestedPaneCollection[num] != VisibleNestedPanes[0])
			{
				prevPane = nestedPaneCollection[num];
			}
		}
		MergeNestedPanes(VisibleNestedPanes, nestedPaneCollection, prevPane, DockAlignment.Left, 0.5);
	}

	private static void MergeNestedPanes(VisibleNestedPaneCollection nestedPanesFrom, NestedPaneCollection nestedPanesTo, DockPane prevPane, DockAlignment alignment, double proportion)
	{
		if (nestedPanesFrom.Count == 0)
		{
			return;
		}
		int count = nestedPanesFrom.Count;
		DockPane[] array = new DockPane[count];
		DockPane[] array2 = new DockPane[count];
		DockAlignment[] array3 = new DockAlignment[count];
		double[] array4 = new double[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = nestedPanesFrom[i];
			array2[i] = nestedPanesFrom[i].NestedDockingStatus.PreviousPane;
			array3[i] = nestedPanesFrom[i].NestedDockingStatus.Alignment;
			array4[i] = nestedPanesFrom[i].NestedDockingStatus.Proportion;
		}
		DockPane dockPane = array[0].DockTo(nestedPanesTo.Container, prevPane, alignment, proportion);
		array[0].DockState = nestedPanesTo.DockState;
		for (int j = 1; j < count; j++)
		{
			for (int k = j; k < count; k++)
			{
				if (array2[k] == array[j - 1])
				{
					array2[k] = dockPane;
				}
			}
			dockPane = array[j].DockTo(nestedPanesTo.Container, array2[j], array3[j], array4[j]);
			array[j].DockState = nestedPanesTo.DockState;
		}
	}
}
