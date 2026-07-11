using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Timelines;

public class HitEventArgs : EventArgs
{
	public bool Handled;

	public readonly RectangleF PickRectangle;

	public readonly MouseEventArgs MouseEvent;

	private HitRecord m_hitRecord;

	public HitRecord HitRecord
	{
		get
		{
			return m_hitRecord;
		}
		set
		{
			m_hitRecord = value;
			Handled = true;
		}
	}

	public HitEventArgs(HitRecord hitRecord, RectangleF pickRectangle, MouseEventArgs mouseEvent)
	{
		m_hitRecord = hitRecord;
		PickRectangle = pickRectangle;
		MouseEvent = mouseEvent;
	}
}
