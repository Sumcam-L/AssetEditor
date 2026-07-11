using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WeifenLuo.WinFormsUI.Docking;

public class DockWindowCollection : ReadOnlyCollection<DockWindow>
{
	public DockWindow this[DockState dockState]
	{
		get
		{
			switch (dockState)
			{
			case DockState.Document:
				return base.Items[0];
			case DockState.DockLeftAutoHide:
			case DockState.DockLeft:
				return base.Items[1];
			case DockState.DockRightAutoHide:
			case DockState.DockRight:
				return base.Items[2];
			case DockState.DockTopAutoHide:
			case DockState.DockTop:
				return base.Items[3];
			case DockState.DockBottomAutoHide:
			case DockState.DockBottom:
				return base.Items[4];
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	internal DockWindowCollection(DockPanel dockPanel)
		: base((IList<DockWindow>)new List<DockWindow>())
	{
		base.Items.Add(dockPanel.Theme.Extender.DockWindowFactory.CreateDockWindow(dockPanel, DockState.Document));
		base.Items.Add(dockPanel.Theme.Extender.DockWindowFactory.CreateDockWindow(dockPanel, DockState.DockLeft));
		base.Items.Add(dockPanel.Theme.Extender.DockWindowFactory.CreateDockWindow(dockPanel, DockState.DockRight));
		base.Items.Add(dockPanel.Theme.Extender.DockWindowFactory.CreateDockWindow(dockPanel, DockState.DockTop));
		base.Items.Add(dockPanel.Theme.Extender.DockWindowFactory.CreateDockWindow(dockPanel, DockState.DockBottom));
	}
}
