using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Timelines;

public class SplitManipulator
{
	private readonly TimelineControl m_owner;

	private bool m_active;

	private bool m_splitCursor;

	private static readonly TimelineControl.SnapOptions s_snapOptions;

	public bool Active
	{
		get
		{
			return m_active;
		}
		set
		{
			if (m_active != value)
			{
				m_active = value;
				if (value)
				{
					SetCursor();
				}
				else
				{
					ClearCursor();
				}
			}
		}
	}

	public SplitManipulator(TimelineControl owner)
	{
		m_owner = owner;
		m_owner.MouseDownPicked += owner_MouseDownPicked;
		m_owner.MouseMovePicked += owner_MouseMovePicked;
		m_owner.KeyDown += owner_KeyDown;
	}

	private void owner_MouseMovePicked(object sender, HitEventArgs e)
	{
		string text = null;
		if (!m_active)
		{
			return;
		}
		e.Handled = true;
		if (e.HitRecord.Type == HitType.Interval && e.MouseEvent.Button == MouseButtons.None && !m_owner.IsUsingMouse && m_owner.IsEditable(e.HitRecord.HitPath))
		{
			SetCursor();
			TimelinePath hitPath = e.HitRecord.HitPath;
			IInterval interval = (IInterval)e.HitRecord.HitTimelineObject;
			float num = GdiUtil.InverseTransform(m_owner.Transform, e.MouseEvent.Location.X);
			float num2 = m_owner.GetSnapOffset(new float[1] { num }, s_snapOptions);
			num += num2;
			num = m_owner.ConstrainFrameOffset(num);
			Matrix matrix = TimelineControl.CalculateLocalToWorld(hitPath);
			if (num <= GdiUtil.Transform(matrix, interval.Start) || num >= GdiUtil.Transform(matrix, interval.Start + interval.Length))
			{
				m_owner.GetSnapOffset(new float[0], s_snapOptions);
			}
			else
			{
				text = num.ToString();
			}
		}
	}

	private void owner_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			Active = false;
		}
	}

	private void owner_MouseDownPicked(object sender, HitEventArgs e)
	{
		TimelinePath hitPath = e.HitRecord.HitPath;
		IInterval interval = e.HitRecord.HitTimelineObject as IInterval;
		if (!m_active || e.MouseEvent.Button != MouseButtons.Left || m_owner.IsUsingMouse || !m_owner.IsEditable(hitPath))
		{
			return;
		}
		if (interval != null && e.HitRecord.Type == HitType.Interval)
		{
			float num = GdiUtil.InverseTransform(m_owner.Transform, e.MouseEvent.Location.X);
			num += m_owner.GetSnapOffset(new float[1] { num }, s_snapOptions);
			Matrix matrix = TimelineControl.CalculateLocalToWorld(hitPath);
			float fraction = (num - GdiUtil.Transform(matrix, interval.Start)) / GdiUtil.TransformVector(matrix, interval.Length);
			if (m_owner.Selection.SelectionContains(interval))
			{
				SplitSelectedIntervals(fraction);
			}
			else
			{
				SplitUnselectedInterval(interval, fraction);
			}
		}
		Active = false;
		e.Handled = true;
	}

	private void SplitUnselectedInterval(IInterval interval, float fraction)
	{
		m_owner.TransactionContext.DoTransaction(delegate
		{
			DoSplit(interval, fraction);
		}, "Split Interval");
	}

	private void SplitSelectedIntervals(float fraction)
	{
		m_owner.TransactionContext.DoTransaction(delegate
		{
			List<IInterval> list = new List<IInterval>(m_owner.Selection.SelectionCount * 2);
			list.AddRange(m_owner.Selection.GetSelection<IInterval>());
			foreach (IInterval item in m_owner.Selection.GetSelection<IInterval>())
			{
				IInterval interval = DoSplit(item, fraction);
				if (interval != null)
				{
					list.Add(interval);
				}
			}
			m_owner.Selection.SetRange(list);
		}, "Split Interval");
	}

	private IInterval DoSplit(IInterval interval, float fraction)
	{
		IInterval interval2 = null;
		float start = interval.Start;
		float length = interval.Length;
		float offset = start + length * fraction;
		offset = m_owner.ConstrainFrameOffset(offset);
		if (offset > start && offset < start + length)
		{
			interval2 = m_owner.Create(interval);
			interval2.Start = offset;
			interval2.Length = length - (offset - start);
			interval.Length = offset - start;
			interval.Track.Intervals.Add(interval2);
		}
		return interval2;
	}

	private void SetCursor()
	{
		m_splitCursor = true;
		m_owner.Cursor = Cursors.VSplit;
	}

	private void ClearCursor()
	{
		if (m_splitCursor)
		{
			m_owner.GetSnapOffset(new float[0], null);
			m_splitCursor = false;
			m_owner.Cursor = Cursors.Default;
		}
	}

	static SplitManipulator()
	{
		s_snapOptions = new TimelineControl.SnapOptions();
		s_snapOptions.IncludeSelected = true;
	}
}
