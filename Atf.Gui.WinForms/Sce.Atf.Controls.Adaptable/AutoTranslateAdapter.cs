using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class AutoTranslateAdapter : ControlAdapter, IAutoTranslateAdapter, IDisposable
{
	private readonly ITransformAdapter m_transformAdapter;

	private readonly Timer m_timer;

	private Point m_mousePosition;

	private bool m_enabled;

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
		}
	}

	public AutoTranslateAdapter(ITransformAdapter transformAdapter)
	{
		m_transformAdapter = transformAdapter;
		m_timer = new Timer();
		m_timer.Interval = 10;
		m_timer.Tick += timer_Tick;
	}

	public void Dispose()
	{
		m_timer.Dispose();
	}

	protected override void BindReverse(AdaptableControl control)
	{
		control.MouseMove += control_MouseMove;
		control.MouseUp += control_MouseUp;
		control.MouseLeave += control_MouseLeave;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.MouseMove -= control_MouseMove;
		control.MouseUp -= control_MouseUp;
		control.MouseLeave -= control_MouseLeave;
	}

	private void control_MouseMove(object sender, MouseEventArgs e)
	{
		if (m_enabled)
		{
			m_mousePosition = new Point(e.X, e.Y);
			if (!base.AdaptedControl.ClientRectangle.Contains(m_mousePosition) && !m_timer.Enabled)
			{
				m_timer.Start();
			}
		}
	}

	private void control_MouseUp(object sender, MouseEventArgs e)
	{
		m_timer.Stop();
	}

	private void control_MouseLeave(object sender, EventArgs e)
	{
		m_timer.Stop();
	}

	private void timer_Tick(object sender, EventArgs e)
	{
		m_timer.Stop();
		if (!base.AdaptedControl.ClientRectangle.Contains(m_mousePosition))
		{
			int num = 0;
			if (m_mousePosition.X < 0)
			{
				num = 10;
			}
			else if (m_mousePosition.X > base.AdaptedControl.Width)
			{
				num = -10;
			}
			int num2 = 0;
			if (m_mousePosition.Y < 0)
			{
				num2 = 10;
			}
			else if (m_mousePosition.Y > base.AdaptedControl.Height)
			{
				num2 = -10;
			}
			PointF translation = m_transformAdapter.Translation;
			m_transformAdapter.Translation = new PointF(translation.X + (float)num, translation.Y + (float)num2);
			m_timer.Start();
		}
	}
}
