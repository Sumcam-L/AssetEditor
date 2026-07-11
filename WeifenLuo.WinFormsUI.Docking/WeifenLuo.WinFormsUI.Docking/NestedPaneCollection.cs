using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

public sealed class NestedPaneCollection : ReadOnlyCollection<DockPane>
{
	private INestedPanesContainer m_container;

	private VisibleNestedPaneCollection m_visibleNestedPanes;

	public INestedPanesContainer Container => m_container;

	public VisibleNestedPaneCollection VisibleNestedPanes => m_visibleNestedPanes;

	public DockState DockState => Container.DockState;

	public bool IsFloat => DockState == DockState.Float;

	internal NestedPaneCollection(INestedPanesContainer container)
		: base((IList<DockPane>)new List<DockPane>())
	{
		m_container = container;
		m_visibleNestedPanes = new VisibleNestedPaneCollection(this);
	}

	internal void Add(DockPane pane)
	{
		if (pane != null)
		{
			NestedPaneCollection nestedPaneCollection = ((pane.NestedPanesContainer == null) ? null : pane.NestedPanesContainer.NestedPanes);
			nestedPaneCollection?.InternalRemove(pane);
			base.Items.Add(pane);
			nestedPaneCollection?.CheckFloatWindowDispose();
		}
	}

	private void CheckFloatWindowDispose()
	{
		if (base.Count == 0 && Container.DockState == DockState.Float)
		{
			FloatWindow floatWindow = (FloatWindow)Container;
			if (!floatWindow.Disposing && !floatWindow.IsDisposed && !Win32Helper.IsRunningOnMono)
			{
				NativeMethods.PostMessage(((FloatWindow)Container).Handle, 1025, 0u, 0u);
			}
		}
	}

	public void SwitchPaneWithFirstChild(DockPane pane)
	{
		if (!Contains(pane))
		{
			return;
		}
		NestedDockingStatus nestedDockingStatus = pane.NestedDockingStatus;
		DockPane dockPane = null;
		for (int num = base.Count - 1; num > IndexOf(pane); num--)
		{
			if (base[num].NestedDockingStatus.PreviousPane == pane)
			{
				dockPane = base[num];
				break;
			}
		}
		if (dockPane == null)
		{
			return;
		}
		int num2 = IndexOf(dockPane);
		base.Items[IndexOf(pane)] = dockPane;
		base.Items[num2] = pane;
		NestedDockingStatus nestedDockingStatus2 = dockPane.NestedDockingStatus;
		DockAlignment alignment = ((nestedDockingStatus2.Alignment == DockAlignment.Left) ? DockAlignment.Right : ((nestedDockingStatus2.Alignment != DockAlignment.Right) ? ((nestedDockingStatus2.Alignment != DockAlignment.Top) ? DockAlignment.Top : DockAlignment.Bottom) : DockAlignment.Left));
		double proportion = 1.0 - nestedDockingStatus2.Proportion;
		nestedDockingStatus2.SetStatus(this, nestedDockingStatus.PreviousPane, nestedDockingStatus.Alignment, nestedDockingStatus.Proportion);
		for (int num3 = num2 - 1; num3 > IndexOf(dockPane); num3--)
		{
			NestedDockingStatus nestedDockingStatus3 = base[num3].NestedDockingStatus;
			if (nestedDockingStatus3.PreviousPane == pane)
			{
				nestedDockingStatus3.SetStatus(this, dockPane, nestedDockingStatus3.Alignment, nestedDockingStatus3.Proportion);
			}
		}
		nestedDockingStatus.SetStatus(this, dockPane, alignment, proportion);
	}

	internal void Remove(DockPane pane)
	{
		InternalRemove(pane);
		CheckFloatWindowDispose();
	}

	private void InternalRemove(DockPane pane)
	{
		if (!Contains(pane))
		{
			return;
		}
		NestedDockingStatus nestedDockingStatus = pane.NestedDockingStatus;
		DockPane dockPane = null;
		for (int num = base.Count - 1; num > IndexOf(pane); num--)
		{
			if (base[num].NestedDockingStatus.PreviousPane == pane)
			{
				dockPane = base[num];
				break;
			}
		}
		if (dockPane != null)
		{
			int num2 = IndexOf(dockPane);
			base.Items.Remove(dockPane);
			base.Items[IndexOf(pane)] = dockPane;
			dockPane.NestedDockingStatus.SetStatus(this, nestedDockingStatus.PreviousPane, nestedDockingStatus.Alignment, nestedDockingStatus.Proportion);
			for (int num3 = num2 - 1; num3 > IndexOf(dockPane); num3--)
			{
				NestedDockingStatus nestedDockingStatus2 = base[num3].NestedDockingStatus;
				if (nestedDockingStatus2.PreviousPane == pane)
				{
					nestedDockingStatus2.SetStatus(this, dockPane, nestedDockingStatus2.Alignment, nestedDockingStatus2.Proportion);
				}
			}
		}
		else
		{
			base.Items.Remove(pane);
		}
		nestedDockingStatus.SetStatus(null, null, DockAlignment.Left, 0.5);
		nestedDockingStatus.SetDisplayingStatus(isDisplaying: false, null, DockAlignment.Left, 0.5);
		nestedDockingStatus.SetDisplayingBounds(Rectangle.Empty, Rectangle.Empty, Rectangle.Empty);
	}

	public DockPane GetDefaultPreviousPane(DockPane pane)
	{
		for (int num = base.Count - 1; num >= 0; num--)
		{
			if (base[num] != pane)
			{
				return base[num];
			}
		}
		return null;
	}
}
