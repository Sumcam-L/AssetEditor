using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class HoverAdapter : ControlAdapter, IDisposable
{
	private readonly Timer m_hoverTimer;

	private object m_hoverItem;

	private object m_hoverPart;

	private object m_hoverSubItem;

	private object m_hoverSubPart;

	private bool m_hovering;

	public bool Hovering => m_hovering;

	public event EventHandler<HoverEventArgs<object, object>> HoverStarted;

	public event EventHandler HoverStopped;

	public HoverAdapter()
	{
		m_hoverTimer = new Timer();
		m_hoverTimer.Interval = 10;
		m_hoverTimer.Tick += hoverTimer_Tick;
	}

	public void Dispose()
	{
		StopHover();
		m_hoverTimer.Dispose();
	}

	protected virtual void OnHoverStarted(HoverEventArgs<object, object> e)
	{
	}

	protected virtual void OnHoverStopped(EventArgs e)
	{
	}

	protected override void Bind(AdaptableControl control)
	{
		control.ContextChanged += control_ContextChanged;
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		m_hoverItem = null;
		m_hoverPart = null;
		StopHover();
	}

	protected override void BindReverse(AdaptableControl control)
	{
		control.MouseDown += control_MouseDown;
		control.MouseMove += control_MouseMove;
		control.MouseLeave += control_MouseLeave;
	}

	protected override void Unbind(AdaptableControl control)
	{
		StopHover();
		control.MouseDown -= control_MouseDown;
		control.MouseMove -= control_MouseMove;
		control.MouseLeave -= control_MouseLeave;
		control.ContextChanged -= control_ContextChanged;
	}

	private void control_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.None || !base.AdaptedControl.Focused)
		{
			return;
		}
		object obj = null;
		DiagramHitRecord diagramHitRecord = null;
		foreach (IPickingAdapter item in base.AdaptedControl.AsAll<IPickingAdapter>())
		{
			diagramHitRecord = item.Pick(new Point(e.X, e.Y));
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
				diagramHitRecord = item2.Pick(new Point(e.X, e.Y));
				if (diagramHitRecord.Item != null)
				{
					break;
				}
			}
		}
		if (diagramHitRecord.Item != m_hoverItem || diagramHitRecord.Part != m_hoverPart || diagramHitRecord.SubItem != m_hoverSubItem || diagramHitRecord.SubPart != m_hoverSubPart)
		{
			StartHover(diagramHitRecord);
		}
	}

	private void control_MouseDown(object sender, MouseEventArgs e)
	{
		StopHover();
	}

	private void control_MouseLeave(object sender, EventArgs e)
	{
		StopHover();
	}

	private void hoverTimer_Tick(object sender, EventArgs e)
	{
		StopHover();
		if (m_hoverItem != null)
		{
			m_hovering = true;
			HoverEventArgs<object, object> e2 = new HoverEventArgs<object, object>(m_hoverItem, m_hoverPart, m_hoverSubItem, m_hoverSubPart, base.AdaptedControl);
			OnHoverStarted(e2);
			this.HoverStarted.Raise(this, e2);
		}
	}

	private void StartHover(DiagramHitRecord hitRecord)
	{
		m_hoverItem = hitRecord.Item;
		m_hoverPart = hitRecord.Part;
		m_hoverSubItem = hitRecord.SubItem;
		m_hoverSubPart = hitRecord.SubPart;
		StopHover();
		m_hoverTimer.Start();
	}

	private void StopHover()
	{
		if (m_hovering)
		{
			m_hovering = false;
			OnHoverStopped(EventArgs.Empty);
			this.HoverStopped.Raise(this, EventArgs.Empty);
		}
		m_hoverTimer.Stop();
	}
}
