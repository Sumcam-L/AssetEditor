using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable;

public class ContextMenuAdapter : ControlAdapter
{
	private readonly ICommandService m_commandService;

	private readonly IEnumerable<Lazy<IContextMenuCommandProvider>> m_providers;

	public Point? TriggeringLocation { get; set; }

	public ContextMenuAdapter(ICommandService commandService, IEnumerable<Lazy<IContextMenuCommandProvider>> providers)
	{
		m_commandService = commandService;
		m_providers = providers;
	}

	protected override void BindReverse(AdaptableControl control)
	{
		control.MouseUp += control_MouseUp;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.MouseUp -= control_MouseUp;
	}

	private void control_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right || (Control.ModifierKeys & Keys.Alt) != Keys.None)
		{
			return;
		}
		Point point = new Point(e.X, e.Y);
		object obj = null;
		foreach (IPickingAdapter item in base.AdaptedControl.AsAll<IPickingAdapter>())
		{
			DiagramHitRecord diagramHitRecord = item.Pick(point);
			if (diagramHitRecord.Item != null)
			{
				obj = diagramHitRecord.Item;
				break;
			}
		}
		if (obj == null)
		{
			foreach (IPickingAdapter2 item2 in base.AdaptedControl.AsAll<IPickingAdapter2>())
			{
				DiagramHitRecord diagramHitRecord2 = item2.Pick(point);
				if (diagramHitRecord2.Item != null)
				{
					obj = diagramHitRecord2.Item;
					break;
				}
			}
		}
		object context = base.AdaptedControl.Context;
		TriggeringLocation = point;
		List<object> list = new List<object>(m_providers.GetCommands(context, obj));
		OnContextMenuOpening(list);
		Point screenPoint = base.AdaptedControl.PointToScreen(point);
		m_commandService.RunContextMenu(list, screenPoint);
	}

	protected virtual void OnContextMenuOpening(IList<object> commands)
	{
	}
}
