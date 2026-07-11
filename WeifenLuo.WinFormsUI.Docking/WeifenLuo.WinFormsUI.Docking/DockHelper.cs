using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public static class DockHelper
{
	public static bool IsDockStateAutoHide(DockState dockState)
	{
		if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide || dockState == DockState.DockTopAutoHide || dockState == DockState.DockBottomAutoHide)
		{
			return true;
		}
		return false;
	}

	public static bool IsDockStateValid(DockState dockState, DockAreas dockableAreas)
	{
		if ((dockableAreas & DockAreas.Float) == 0 && dockState == DockState.Float)
		{
			return false;
		}
		if ((dockableAreas & DockAreas.Document) == 0 && dockState == DockState.Document)
		{
			return false;
		}
		if ((dockableAreas & DockAreas.DockLeft) == 0 && (dockState == DockState.DockLeft || dockState == DockState.DockLeftAutoHide))
		{
			return false;
		}
		if ((dockableAreas & DockAreas.DockRight) == 0 && (dockState == DockState.DockRight || dockState == DockState.DockRightAutoHide))
		{
			return false;
		}
		if ((dockableAreas & DockAreas.DockTop) == 0 && (dockState == DockState.DockTop || dockState == DockState.DockTopAutoHide))
		{
			return false;
		}
		if ((dockableAreas & DockAreas.DockBottom) == 0 && (dockState == DockState.DockBottom || dockState == DockState.DockBottomAutoHide))
		{
			return false;
		}
		return true;
	}

	public static bool IsDockWindowState(DockState state)
	{
		if (state == DockState.DockTop || state == DockState.DockBottom || state == DockState.DockLeft || state == DockState.DockRight || state == DockState.Document)
		{
			return true;
		}
		return false;
	}

	public static DockState ToggleAutoHideState(DockState state)
	{
		return state switch
		{
			DockState.DockLeft => DockState.DockLeftAutoHide, 
			DockState.DockRight => DockState.DockRightAutoHide, 
			DockState.DockTop => DockState.DockTopAutoHide, 
			DockState.DockBottom => DockState.DockBottomAutoHide, 
			DockState.DockLeftAutoHide => DockState.DockLeft, 
			DockState.DockRightAutoHide => DockState.DockRight, 
			DockState.DockTopAutoHide => DockState.DockTop, 
			DockState.DockBottomAutoHide => DockState.DockBottom, 
			_ => state, 
		};
	}

	public static DockPane PaneAtPoint(Point pt, DockPanel dockPanel)
	{
		if (!Win32Helper.IsRunningOnMono)
		{
			for (Control control = Win32Helper.ControlAtPoint(pt); control != null; control = control.Parent)
			{
				if (control is IDockContent dockContent && dockContent.DockHandler.DockPanel == dockPanel)
				{
					return dockContent.DockHandler.Pane;
				}
				if (control is DockPane dockPane && dockPane.DockPanel == dockPanel)
				{
					return dockPane;
				}
			}
		}
		return null;
	}

	public static FloatWindow FloatWindowAtPoint(Point pt, DockPanel dockPanel)
	{
		if (!Win32Helper.IsRunningOnMono)
		{
			for (Control control = Win32Helper.ControlAtPoint(pt); control != null; control = control.Parent)
			{
				if (control is FloatWindow floatWindow && floatWindow.DockPanel == dockPanel)
				{
					return floatWindow;
				}
			}
		}
		return null;
	}
}
