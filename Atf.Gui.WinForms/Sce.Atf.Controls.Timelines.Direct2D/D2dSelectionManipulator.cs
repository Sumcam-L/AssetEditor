using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Timelines.Direct2D;

public class D2dSelectionManipulator
{
	protected readonly D2dTimelineControl Owner;

	protected TimelinePath Anchor;

	private HitRecord m_mouseDownHitRecord;

	private Point m_mouseDownPos;

	private ISelectionContext SelectionContext
	{
		get
		{
			if (Owner != null && Owner.TimelineDocument != null)
			{
				return Owner.TimelineDocument.As<ISelectionContext>();
			}
			return null;
		}
	}

	public D2dSelectionManipulator(D2dTimelineControl owner)
	{
		Owner = owner;
		Owner.MouseDownPicked += Owner_MouseDownPicked;
		Owner.KeyDown += Owner_KeyDown;
		Owner.MouseUp += Owner_MouseUp;
	}

	protected virtual void Owner_MouseDownPicked(object sender, HitEventArgs e)
	{
		HitRecord hitRecord = e.HitRecord;
		TimelinePath hitPath = hitRecord.HitPath;
		if (!(hitPath != null))
		{
			return;
		}
		Keys modifierKeys = Control.ModifierKeys;
		if (e.MouseEvent.Button == MouseButtons.Left)
		{
			if (modifierKeys == Keys.None && SelectionContext != null && SelectionContext.SelectionContains(hitPath))
			{
				m_mouseDownHitRecord = hitRecord;
				m_mouseDownPos = e.MouseEvent.Location;
			}
			else if (modifierKeys == Keys.Control)
			{
				m_mouseDownHitRecord = hitRecord;
				m_mouseDownPos = e.MouseEvent.Location;
			}
			else if ((modifierKeys & Keys.Alt) == 0)
			{
				UpdateSelection(hitRecord, modifierKeys);
			}
		}
		else if (e.MouseEvent.Button == MouseButtons.Right && modifierKeys == Keys.None && SelectionContext != null && !SelectionContext.SelectionContains(hitPath))
		{
			UpdateSelection(hitRecord, modifierKeys);
		}
	}

	protected virtual void Owner_MouseUp(object sender, MouseEventArgs e)
	{
		if (m_mouseDownHitRecord == null)
		{
			return;
		}
		try
		{
			int pickTolerance = Owner.Renderer.PickTolerance;
			if (Math.Abs(e.Location.X - m_mouseDownPos.X) <= pickTolerance && Math.Abs(e.Location.Y - m_mouseDownPos.Y) <= pickTolerance)
			{
				UpdateSelection(m_mouseDownHitRecord, Control.ModifierKeys);
			}
		}
		finally
		{
			m_mouseDownHitRecord = null;
		}
	}

	private void UpdateSelection(HitRecord hitRecord, Keys modifiers)
	{
		TimelinePath hitPath = hitRecord.HitPath;
		bool flag = true;
		switch (hitRecord.Type)
		{
		case HitType.GroupMove:
			SelectGroups(hitPath);
			break;
		case HitType.TrackMove:
			SelectTracks(hitPath);
			break;
		case HitType.Key:
		case HitType.Marker:
		case HitType.Interval:
			SelectEvents(hitPath);
			Owner.Constrain = (modifiers & Owner.ConstrainModifierKeys) != 0;
			break;
		default:
			Anchor = null;
			flag = false;
			break;
		}
		if (flag && ((modifiers & Keys.Shift) == 0 || Anchor == null || (Anchor.Last is IGroup && hitPath.Last is ITrack) || (Anchor.Last is ITrack && hitPath.Last is IGroup) || (Anchor.Last is IEvent && hitPath.Last is IEvent)))
		{
			Anchor = hitPath;
		}
	}

	protected virtual void Owner_KeyDown(object sender, KeyEventArgs e)
	{
		if ((e.Modifiers & Keys.Shift) == 0)
		{
			return;
		}
		if (e.KeyCode == Keys.End)
		{
			if (Anchor != null && Anchor.Last is IGroup)
			{
				SelectGroups(GetLastGroup());
			}
			else if (Anchor != null && Anchor.Last is ITrack)
			{
				SelectTracks(GetLastTrack());
			}
		}
		else if (e.KeyCode == Keys.Home)
		{
			if (Anchor != null && Anchor.Last is IGroup)
			{
				SelectGroups(GetFirstGroup());
			}
			else if (Anchor != null && Anchor.Last is ITrack)
			{
				SelectTracks(GetFirstTrack());
			}
		}
	}

	protected virtual void SelectEvents(TimelinePath target)
	{
		if ((Control.ModifierKeys & Keys.Shift) != Keys.None && Anchor != null && Anchor.Last is IEvent && SelectionContext != null)
		{
			SelectionContext.AddRange(GetRangeOfEvents(Anchor, target));
		}
		else
		{
			Owner.Select<IEvent>(target);
		}
	}

	protected virtual IEnumerable<TimelinePath> GetRangeOfEvents(TimelinePath begin, TimelinePath end)
	{
		if (begin.Equals(end))
		{
			return new TimelinePath[1] { begin };
		}
		bool flag = false;
		IEnumerable enumerable;
		if (begin.Last is IMarker || end.Last is IMarker)
		{
			enumerable = Owner.AllTracks;
			flag = true;
		}
		else
		{
			TimelinePath owningTrack = GetOwningTrack(begin);
			TimelinePath owningTrack2 = GetOwningTrack(end);
			enumerable = GetRangeOfTracks(owningTrack, owningTrack2);
		}
		D2dTimelineControl.CalculateRange(begin, out var worldStart, out var worldEnd);
		D2dTimelineControl.CalculateRange(end, out var worldStart2, out var worldEnd2);
		float beginTime = Math.Min(worldStart, worldStart2);
		float endTime = Math.Max(worldEnd, worldEnd2);
		List<TimelinePath> list = new List<TimelinePath>();
		foreach (TimelinePath item in enumerable)
		{
			ITrack track = (ITrack)item.Last;
			foreach (IKey key in track.Keys)
			{
				item.Last = key;
				if (Overlaps(item, beginTime, endTime))
				{
					list.Add(new TimelinePath(item));
				}
			}
			foreach (IInterval interval in track.Intervals)
			{
				item.Last = interval;
				if (Overlaps(item, beginTime, endTime))
				{
					list.Add(new TimelinePath(item));
				}
			}
		}
		if (flag)
		{
			foreach (TimelinePath allMarker in Owner.AllMarkers)
			{
				if (Overlaps(allMarker, beginTime, endTime))
				{
					list.Add(allMarker);
				}
			}
		}
		return list;
	}

	protected virtual void SelectGroups(TimelinePath target)
	{
		if ((Control.ModifierKeys & Keys.Shift) != Keys.None && Anchor != null && Anchor.Last is IGroup && SelectionContext != null)
		{
			SelectionContext.SetRange(GetRangeOfGroups(Anchor, target));
		}
		else
		{
			Owner.Select<IGroup>(target);
		}
	}

	protected virtual IEnumerable<TimelinePath> GetRangeOfGroups(TimelinePath begin, TimelinePath end)
	{
		TimelinePath timelinePath = null;
		List<TimelinePath> list = new List<TimelinePath>();
		foreach (TimelinePath allGroup in Owner.AllGroups)
		{
			if (timelinePath == null)
			{
				if (allGroup == begin)
				{
					timelinePath = end;
				}
				else if (allGroup == end)
				{
					timelinePath = begin;
				}
			}
			if (timelinePath != null)
			{
				list.Add(allGroup);
			}
			if (allGroup == timelinePath)
			{
				break;
			}
		}
		if (timelinePath == begin)
		{
			list.Reverse();
		}
		return list;
	}

	protected virtual void SelectTracks(TimelinePath target)
	{
		if ((Control.ModifierKeys & Keys.Shift) != Keys.None && Anchor != null && Anchor.Last is ITrack && SelectionContext != null)
		{
			SelectionContext.SetRange(GetRangeOfTracks(Anchor, target));
		}
		else
		{
			Owner.Select<ITrack>(target);
		}
	}

	protected virtual IEnumerable<TimelinePath> GetRangeOfTracks(TimelinePath begin, TimelinePath end)
	{
		TimelinePath timelinePath = null;
		List<TimelinePath> list = new List<TimelinePath>();
		foreach (TimelinePath allTrack in Owner.AllTracks)
		{
			if (timelinePath == null)
			{
				if (allTrack == begin)
				{
					timelinePath = end;
				}
				else if (allTrack == end)
				{
					timelinePath = begin;
				}
			}
			if (timelinePath != null)
			{
				list.Add(allTrack);
			}
			if (allTrack == timelinePath)
			{
				break;
			}
		}
		if (timelinePath == begin)
		{
			list.Reverse();
		}
		return list;
	}

	protected TimelinePath GetFirstGroup()
	{
		using (IEnumerator<TimelinePath> enumerator = Owner.AllGroups.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	protected TimelinePath GetLastGroup()
	{
		TimelinePath result = null;
		foreach (TimelinePath allGroup in Owner.AllGroups)
		{
			result = allGroup;
		}
		return result;
	}

	protected TimelinePath GetFirstTrack()
	{
		using (IEnumerator<TimelinePath> enumerator = Owner.AllTracks.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	protected TimelinePath GetLastTrack()
	{
		TimelinePath result = null;
		foreach (TimelinePath allTrack in Owner.AllTracks)
		{
			result = allTrack;
		}
		return result;
	}

	private bool Overlaps(TimelinePath path, float beginTime, float endTime)
	{
		IEvent obj = (IEvent)path.Last;
		Matrix matrix = D2dTimelineControl.CalculateLocalToWorld(path);
		float num = GdiUtil.Transform(matrix, obj.Start);
		float num2 = GdiUtil.TransformVector(matrix, obj.Length);
		if (num2 == 0f)
		{
			return !(num > endTime) && !(num + num2 < beginTime);
		}
		return !(num >= endTime) && !(num + num2 <= beginTime);
	}

	private TimelinePath GetOwningTrack(TimelinePath begin)
	{
		if (begin.Last is IKey)
		{
			TimelinePath timelinePath = new TimelinePath(begin);
			timelinePath.Last = ((IKey)begin.Last).Track;
			return timelinePath;
		}
		if (begin.Last is IInterval)
		{
			TimelinePath timelinePath2 = new TimelinePath(begin);
			timelinePath2.Last = ((IInterval)begin.Last).Track;
			return timelinePath2;
		}
		if (begin.Last is ITrack)
		{
			return begin;
		}
		return null;
	}
}
