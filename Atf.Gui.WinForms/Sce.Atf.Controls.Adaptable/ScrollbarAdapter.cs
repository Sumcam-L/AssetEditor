using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class ScrollbarAdapter : ControlAdapter
{
	private readonly ITransformAdapter m_transformAdapter;

	private readonly ICanvasAdapter m_canvasAdapter;

	private readonly VScrollBar m_vScrollBar;

	private readonly HScrollBar m_hScrollBar;

	private bool m_updatingScrollbars;

	public ScrollbarAdapter(ITransformAdapter transformAdapter, ICanvasAdapter canvasAdapter)
	{
		m_transformAdapter = transformAdapter;
		m_transformAdapter.TransformChanged += transformAdapter_TransformChanged;
		m_canvasAdapter = canvasAdapter;
		m_canvasAdapter.BoundsChanged += canvasAdapter_BoundsChanged;
		m_canvasAdapter.WindowBoundsChanged += canvasAdapter_WindowBoundsChanged;
		m_vScrollBar = new VScrollBar();
		m_vScrollBar.Dock = DockStyle.Right;
		m_vScrollBar.ValueChanged += vScrollBar_ValueChanged;
		m_hScrollBar = new HScrollBar();
		m_hScrollBar.Dock = DockStyle.Bottom;
		m_hScrollBar.ValueChanged += hScrollBar_ValueChanged;
	}

	protected override void Bind(AdaptableControl control)
	{
		control.Painting += control_BeforePaint;
		control.Controls.Add(m_vScrollBar);
		control.Controls.Add(m_hScrollBar);
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.Painting -= control_BeforePaint;
		control.Controls.Remove(m_vScrollBar);
		control.Controls.Remove(m_hScrollBar);
	}

	private void transformAdapter_TransformChanged(object sender, EventArgs e)
	{
		UpdateScrollbars();
	}

	private void canvasAdapter_WindowBoundsChanged(object sender, EventArgs e)
	{
		UpdateScrollbars();
	}

	private void canvasAdapter_BoundsChanged(object sender, EventArgs e)
	{
		UpdateScrollbars();
	}

	private void control_BeforePaint(object sender, EventArgs e)
	{
		Rectangle clientRectangle = base.AdaptedControl.ClientRectangle;
		Rectangle windowBounds = m_canvasAdapter.WindowBounds;
		if (m_hScrollBar.Enabled)
		{
			clientRectangle.Height = Math.Min(windowBounds.Height, clientRectangle.Height - m_hScrollBar.Height);
		}
		if (m_vScrollBar.Enabled)
		{
			clientRectangle.Width = Math.Min(windowBounds.Width, clientRectangle.Width - m_vScrollBar.Width);
		}
		m_canvasAdapter.WindowBounds = clientRectangle;
	}

	private void vScrollBar_ValueChanged(object sender, EventArgs e)
	{
		if (!m_updatingScrollbars)
		{
			m_updatingScrollbars = true;
			try
			{
				m_transformAdapter.Translation = new PointF(m_transformAdapter.Translation.X, -m_vScrollBar.Value);
			}
			finally
			{
				m_updatingScrollbars = false;
			}
		}
		OnScroll(EventArgs.Empty);
		base.AdaptedControl.Refresh();
	}

	private void hScrollBar_ValueChanged(object sender, EventArgs e)
	{
		if (!m_updatingScrollbars)
		{
			m_updatingScrollbars = true;
			try
			{
				m_transformAdapter.Translation = new PointF(-m_hScrollBar.Value, m_transformAdapter.Translation.Y);
			}
			finally
			{
				m_updatingScrollbars = false;
			}
		}
		OnScroll(EventArgs.Empty);
		base.AdaptedControl.Refresh();
	}

	protected virtual void OnScroll(EventArgs e)
	{
	}

	private void UpdateScrollbars()
	{
		if (m_updatingScrollbars)
		{
			return;
		}
		try
		{
			m_updatingScrollbars = true;
			Rectangle clientRectangle = base.AdaptedControl.ClientRectangle;
			Rectangle bounds = m_canvasAdapter.Bounds;
			PointF scale = m_transformAdapter.Scale;
			int num = (int)((float)bounds.X * scale.X - (float)clientRectangle.Width * 0.5f);
			int num2 = (int)((float)bounds.Y * scale.Y - (float)clientRectangle.Height * 0.5f);
			int num3 = (int)((float)bounds.Right * scale.X - (float)clientRectangle.Width * 0.5f);
			int num4 = (int)((float)bounds.Bottom * scale.Y - (float)clientRectangle.Height * 0.5f);
			WinFormsUtil.UpdateScrollbars(contentArea: new Rectangle(num, num2, num3 - num, num4 - num2), vScrollBar: m_vScrollBar, hScrollBar: m_hScrollBar, visibleArea: clientRectangle);
			if (!m_hScrollBar.Capture && !m_vScrollBar.Capture)
			{
				PointF translation = m_transformAdapter.Translation;
				m_hScrollBar.Value = Math.Min(Math.Max(m_hScrollBar.Minimum, -(int)translation.X), m_hScrollBar.Maximum);
				m_vScrollBar.Value = Math.Min(Math.Max(m_vScrollBar.Minimum, -(int)translation.Y), m_vScrollBar.Maximum);
			}
		}
		finally
		{
			m_updatingScrollbars = false;
		}
	}
}
