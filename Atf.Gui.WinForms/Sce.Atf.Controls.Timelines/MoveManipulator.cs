using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Timelines;

public class MoveManipulator
{
	private static readonly TimelineControl.SnapOptions s_snapOptions = new TimelineControl.SnapOptions();

	private readonly TimelineControl m_owner;

	private GhostInfo[] m_ghosts;

	private HitRecord m_mouseMoveHitRecord;

	protected TimelineControl Owner => m_owner;

	protected GhostInfo[] Ghosts => m_ghosts;

	protected bool IsMovingSelection => m_mouseMoveHitRecord != null && m_owner.DragOverThreshold && !m_owner.IsUsingMouse;

	public MoveManipulator(TimelineControl owner)
	{
		m_owner = owner;
		m_owner.MouseDownPicked += owner_MouseDownPicked;
		m_owner.MouseMovePicked += owner_MouseMovePicked;
		m_owner.MouseMove += owner_MouseMove;
		m_owner.MouseUp += owner_MouseUp;
		m_owner.Paint += owner_Paint;
		m_owner.KeyDown += owner_KeyDown;
	}

	protected virtual void MoveSelection()
	{
		PointF dragOffset = m_owner.GetDragOffset();
		if (dragOffset.X == 0f && dragOffset.Y == 0f)
		{
			return;
		}
		if (dragOffset.Y < 0f)
		{
			GhostInfo[] ghosts = m_ghosts;
			foreach (GhostInfo ghostInfo in ghosts)
			{
				if (!ghostInfo.Valid)
				{
					return;
				}
			}
		}
		bool tryToCopy = Control.ModifierKeys == Keys.Control;
		m_owner.TransactionContext.DoTransaction(delegate
		{
			ITimeline timeline = m_owner.TimelineDocument.Timeline;
			Dictionary<ITrack, ITrack> newTrackMap = new Dictionary<ITrack, ITrack>();
			List<Pair<ITrack, IEvent>> list = new List<Pair<ITrack, IEvent>>();
			for (int j = 0; j < m_ghosts.Length; j++)
			{
				GhostInfo ghostInfo2 = m_ghosts[j];
				ITimelineObject timelineObject = null;
				if (tryToCopy && ghostInfo2.Object is ICloneable cloneable)
				{
					timelineObject = cloneable.Clone() as ITimelineObject;
				}
				ITimelineReference timelineReference = ghostInfo2.Object as ITimelineReference;
				if (timelineReference != null)
				{
					if (timelineObject != null)
					{
						timelineReference = (ITimelineReference)timelineObject;
					}
					timelineReference.Start = ghostInfo2.Start;
					if (timelineObject != null && timeline is IHierarchicalTimelineList)
					{
						((IHierarchicalTimelineList)timeline).References.Add(timelineReference);
					}
				}
				else
				{
					IInterval interval = ghostInfo2.Object as IInterval;
					if (interval != null)
					{
						if (timelineObject != null)
						{
							interval = (IInterval)timelineObject;
						}
						interval.Start = ghostInfo2.Start;
						interval.Length = ghostInfo2.Length;
						ITrack track = (ITrack)ghostInfo2.Target;
						if (track != interval.Track)
						{
							if (track == null)
							{
								track = CreateTargetTrack(interval.Track, newTrackMap);
							}
							if (timelineObject == null)
							{
								interval.Track.Intervals.Remove(interval);
							}
							list.Add(new Pair<ITrack, IEvent>(track, interval));
						}
					}
					else
					{
						IKey key = ghostInfo2.Object as IKey;
						if (key != null)
						{
							if (timelineObject != null)
							{
								key = (IKey)timelineObject;
							}
							key.Start = ghostInfo2.Start;
							ITrack track2 = (ITrack)ghostInfo2.Target;
							if (track2 != key.Track)
							{
								if (track2 == null)
								{
									track2 = CreateTargetTrack(key.Track, newTrackMap);
								}
								if (timelineObject == null)
								{
									key.Track.Keys.Remove(key);
								}
								list.Add(new Pair<ITrack, IEvent>(track2, key));
							}
						}
						else if (ghostInfo2.Object is IMarker marker)
						{
							if (ghostInfo2.Valid && marker.Start != ghostInfo2.Start)
							{
								if (timelineObject != null)
								{
									IMarker marker2 = (IMarker)timelineObject;
									marker2.Start = ghostInfo2.Start;
									marker.Timeline.Markers.Add(marker2);
								}
								else
								{
									marker.Start = ghostInfo2.Start;
								}
							}
						}
						else
						{
							ITrack track3 = ghostInfo2.Object as ITrack;
							if (track3 != null)
							{
								ITrack track4 = (ITrack)ghostInfo2.Target;
								if (track4 != null && track4 != track3)
								{
									if (timelineObject != null)
									{
										track3 = (ITrack)timelineObject;
									}
									int index = track4.Group.Tracks.IndexOf(track4);
									if (timelineObject == null)
									{
										track3.Group.Tracks.Remove(track3);
									}
									track4.Group.Tracks.Insert(index, track3);
								}
							}
							else
							{
								IGroup obj = ghostInfo2.Object as IGroup;
								if (obj != null)
								{
									IGroup obj2 = (IGroup)ghostInfo2.Target;
									if (obj2 != null && obj2 != obj)
									{
										if (timelineObject != null)
										{
											obj = (IGroup)timelineObject;
										}
										int index2 = m_owner.TimelineDocument.Timeline.Groups.IndexOf(obj2);
										if (timelineObject == null)
										{
											m_owner.TimelineDocument.Timeline.Groups.Remove(obj);
										}
										m_owner.TimelineDocument.Timeline.Groups.Insert(index2, obj);
									}
								}
							}
						}
					}
				}
			}
			foreach (Pair<ITrack, IEvent> item in list)
			{
				if (item.Second is IInterval)
				{
					item.First.Intervals.Add((IInterval)item.Second);
				}
				else
				{
					item.First.Keys.Add((IKey)item.Second);
				}
			}
		}, "Move Events".Localize("Move Manipulator's undo / redo description for moving timeline events"));
	}

	private ITrack CreateTargetTrack(ITrack original, Dictionary<ITrack, ITrack> newTrackMap)
	{
		if (!newTrackMap.TryGetValue(original, out var value))
		{
			ITimeline timeline = m_owner.TimelineDocument.Timeline;
			IGroup obj = m_owner.Create(original.Group);
			timeline.Groups.Add(obj);
			value = m_owner.Create(original);
			obj.Tracks.Add(value);
			newTrackMap.Add(original, value);
		}
		return value;
	}

	private void owner_MouseDownPicked(object sender, HitEventArgs e)
	{
		if (e.MouseEvent.Button != MouseButtons.Left || m_owner.IsUsingMouse)
		{
			return;
		}
		m_mouseMoveHitRecord = null;
		HitRecord hitRecord = e.HitRecord;
		if (m_owner.IsEditable(hitRecord.HitPath))
		{
			switch (e.HitRecord.Type)
			{
			case HitType.Key:
			case HitType.Marker:
			case HitType.Interval:
			case HitType.GroupMove:
			case HitType.TrackMove:
				m_mouseMoveHitRecord = hitRecord;
				break;
			}
		}
		m_owner.IsMovingSelection = m_mouseMoveHitRecord != null;
	}

	private void owner_MouseMovePicked(object sender, HitEventArgs e)
	{
		if (e.MouseEvent.Button != MouseButtons.None || m_owner.IsUsingMouse)
		{
			return;
		}
		HitRecord hitRecord = e.HitRecord;
		if (m_owner.IsEditable(hitRecord.HitPath))
		{
			switch (hitRecord.Type)
			{
			case HitType.Key:
			case HitType.Marker:
			case HitType.Interval:
				m_owner.Cursor = Cursors.SizeAll;
				break;
			case HitType.IntervalResizeLeft:
			case HitType.IntervalResizeRight:
				break;
			}
		}
	}

	private void owner_MouseMove(object sender, MouseEventArgs e)
	{
		if (IsMovingSelection)
		{
			m_ghosts = GetMoveGhostInfo(m_owner.Transform, m_owner.GetLayout());
			if (Control.ModifierKeys == Keys.Control)
			{
				m_owner.Cursor = Cursors.PanNW;
			}
			else
			{
				m_owner.Cursor = Cursors.SizeAll;
			}
		}
	}

	private void owner_MouseUp(object sender, MouseEventArgs e)
	{
		if (IsMovingSelection)
		{
			MoveSelection();
			m_owner.Invalidate();
		}
		CancelDrag();
	}

	private void owner_Paint(object sender, PaintEventArgs e)
	{
		Rectangle clientRectangle = m_owner.ClientRectangle;
		Matrix transform = m_owner.Transform;
		if (!IsMovingSelection || m_ghosts == null)
		{
			return;
		}
		m_owner.Renderer.DrawGhosts(m_ghosts, GhostType.Move, transform, clientRectangle, e.Graphics);
		GhostInfo[] ghosts = m_ghosts;
		foreach (GhostInfo ghostInfo in ghosts)
		{
			if (ghostInfo.Object is IEvent obj)
			{
				m_owner.MoveEvent(new EventMovedEventArgs(obj, ghostInfo.Start, ghostInfo.Target as ITrack));
			}
		}
	}

	private void owner_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == m_owner.CancelDragKey)
		{
			CancelDrag();
		}
	}

	private void CancelDrag()
	{
		if (m_mouseMoveHitRecord != null)
		{
			m_mouseMoveHitRecord = null;
			m_ghosts = null;
			m_owner.IsMovingSelection = false;
			m_owner.Cursor = Cursors.Arrow;
			m_owner.Invalidate();
		}
	}

	private GhostInfo[] GetMoveGhostInfo(Matrix worldToView, TimelineLayout layout)
	{
		PointF dragOffset = m_owner.GetDragOffset();
		List<float> list = new List<float>(2);
		TimelinePath timelinePath = ((m_mouseMoveHitRecord == null) ? (m_owner.Selection.LastSelected as TimelinePath) : m_mouseMoveHitRecord.HitPath);
		IEvent obj = ((timelinePath != null) ? (timelinePath.Last as IEvent) : null);
		if (obj != null)
		{
			Matrix matrix = TimelineControl.CalculateLocalToWorld(timelinePath);
			float item = GdiUtil.Transform(matrix, obj.Start + dragOffset.X);
			list.Add(item);
			if (obj.Length > 0f)
			{
				list.Add(GdiUtil.Transform(matrix, obj.Start + dragOffset.X + obj.Length));
			}
		}
		float num;
		try
		{
			s_snapOptions.FilterContext = obj;
			s_snapOptions.Filter = MoveSnapFilter;
			num = m_owner.GetSnapOffset(list, s_snapOptions);
		}
		finally
		{
			s_snapOptions.FilterContext = null;
			s_snapOptions.Filter = null;
		}
		dragOffset.X += num;
		float x = dragOffset.X * worldToView.Elements[0];
		float y = dragOffset.Y * worldToView.Elements[3];
		TimelinePath[] moveTargets = GetMoveTargets(layout);
		GhostInfo[] array = new GhostInfo[moveTargets.Length];
		int num2 = -1;
		foreach (TimelinePath item2 in m_owner.Selection.Selection)
		{
			num2++;
			ITimelineObject last = item2.Last;
			RectangleF bounds = layout[item2];
			TimelinePath timelinePath3 = moveTargets[num2];
			ITimelineObject timelineObject = ((timelinePath3 != null) ? timelinePath3.Last : null);
			float start = 0f;
			float length = 0f;
			bool flag = true;
			IInterval interval = last as IInterval;
			IKey key = last as IKey;
			IMarker marker = last as IMarker;
			ITrack track = last as ITrack;
			IGroup obj2 = last as IGroup;
			ITimelineReference timelineReference = last as ITimelineReference;
			if (interval != null)
			{
				ITrack track2 = timelineObject as ITrack;
				start = interval.Start + dragOffset.X;
				length = interval.Length;
				flag = track2 != null && m_owner.Constraints.IsStartValid(interval, ref start) && m_owner.Constraints.IsLengthValid(interval, ref length);
				if (flag)
				{
					y = layout[timelineObject].Y - layout[interval.Track].Y;
					TimelinePath timelinePath4 = new TimelinePath(timelinePath3);
					foreach (IInterval interval2 in track2.Intervals)
					{
						IInterval other = (IInterval)(timelinePath4.Last = interval2);
						if (m_owner.Selection.SelectionContains(timelinePath4) || m_owner.Constraints.IsIntervalValid(interval, ref start, ref length, other))
						{
							continue;
						}
						flag = false;
						break;
					}
				}
			}
			else if (timelineReference != null)
			{
				start = timelineReference.Start + dragOffset.X;
				flag = true;
			}
			else if (key != null)
			{
				start = key.Start + dragOffset.X;
				ITrack track3 = timelineObject as ITrack;
				flag = track3 != null && m_owner.Constraints.IsStartValid(key, ref start);
				if (flag)
				{
					y = layout[track3].Y - layout[key.Track].Y;
				}
			}
			else if (marker != null)
			{
				start = marker.Start + dragOffset.X;
				y = 0f;
				flag = m_owner.Constraints.IsStartValid(marker, ref start);
			}
			else if (track != null)
			{
				x = 0f;
				if (timelineObject == null)
				{
					timelineObject = ((m_owner.DragDelta.Y < 0) ? GetLastTrack() : GetFirstTrack());
				}
			}
			else if (obj2 != null)
			{
				x = 0f;
				if (timelineObject == null)
				{
					IList<IGroup> groups = m_owner.TimelineDocument.Timeline.Groups;
					timelineObject = ((m_owner.DragDelta.Y < 0) ? groups[0] : groups[groups.Count - 1]);
				}
			}
			bounds.Offset(x, y);
			array[num2] = new GhostInfo(last, timelineObject, start, length, bounds, flag);
		}
		return array;
	}

	private ITrack GetFirstTrack()
	{
		foreach (IGroup group in m_owner.TimelineDocument.Timeline.Groups)
		{
			using IEnumerator<ITrack> enumerator2 = group.Tracks.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				return enumerator2.Current;
			}
		}
		return null;
	}

	private ITrack GetLastTrack()
	{
		IList<IGroup> groups = m_owner.TimelineDocument.Timeline.Groups;
		for (int num = groups.Count - 1; num >= 0; num--)
		{
			IGroup obj = groups[num];
			IList<ITrack> tracks = obj.Tracks;
			int num2 = tracks.Count - 1;
			if (num2 >= 0)
			{
				return tracks[num2];
			}
		}
		return null;
	}

	private TimelinePath GetTargetTrack(float y, TimelineLayout layout, IList<ITrack> tracks)
	{
		TimelinePath timelinePath = new TimelinePath((ITimelineObject)null);
		foreach (ITrack track in tracks)
		{
			timelinePath.Last = track;
			RectangleF rectangleF = layout[timelinePath];
			if (rectangleF.Top <= y && rectangleF.Bottom >= y)
			{
				return timelinePath;
			}
		}
		return null;
	}

	private TimelinePath[] GetMoveTargets(TimelineLayout layout)
	{
		List<IGroup> list = new List<IGroup>();
		List<ITrack> list2 = new List<ITrack>();
		foreach (IGroup group in m_owner.TimelineDocument.Timeline.Groups)
		{
			list.Add(group);
			IList<ITrack> tracks = group.Tracks;
			bool expanded = group.Expanded;
			if (tracks.Count > 1 && !expanded)
			{
				continue;
			}
			foreach (ITrack item in tracks)
			{
				list2.Add(item);
			}
		}
		TimelinePath[] array = new TimelinePath[m_owner.Selection.SelectionCount];
		TimelinePath timelinePath = new TimelinePath((ITimelineObject)null);
		int num = -1;
		foreach (TimelinePath item2 in m_owner.Selection.Selection)
		{
			num++;
			if (item2.Last is IGroup)
			{
				foreach (IGroup item3 in list)
				{
					Point dragPoint = m_owner.DragPoint;
					timelinePath.Last = item3;
					RectangleF rectangleF = layout[timelinePath];
					if (rectangleF.Top <= (float)dragPoint.Y && rectangleF.Bottom >= (float)dragPoint.Y)
					{
						array[num] = new TimelinePath(timelinePath);
						break;
					}
				}
			}
			else if (item2.Last is ITrack)
			{
				foreach (ITrack item4 in list2)
				{
					Point dragPoint2 = m_owner.DragPoint;
					timelinePath.Last = item4;
					RectangleF rectangleF2 = layout[timelinePath];
					if (rectangleF2.Top <= (float)dragPoint2.Y && rectangleF2.Bottom >= (float)dragPoint2.Y)
					{
						array[num] = new TimelinePath(timelinePath);
						break;
					}
				}
			}
			else if (item2.Last is IInterval { Track: { } track2 })
			{
				timelinePath.Last = track2;
				RectangleF rectangleF3 = layout[timelinePath];
				float y = rectangleF3.Top + rectangleF3.Height * 0.5f + (float)m_owner.DragDelta.Y;
				array[num] = GetTargetTrack(y, layout, list2);
			}
			else if (item2.Last is IKey { Track: { } track3 })
			{
				timelinePath.Last = track3;
				RectangleF rectangleF3 = layout[timelinePath];
				float y2 = rectangleF3.Top + rectangleF3.Height * 0.5f + (float)m_owner.DragDelta.Y;
				array[num] = GetTargetTrack(y2, layout, list2);
			}
		}
		return array;
	}

	private bool MoveSnapFilter(IEvent testEvent, TimelineControl.SnapOptions options)
	{
		if (!(options.FilterContext is ITimelineReference { Target: var target } timelineReference))
		{
			return true;
		}
		if (target == null)
		{
			return true;
		}
		ITimeline timeline = null;
		if (testEvent is ITimelineReference)
		{
			timeline = ((ITimelineReference)testEvent).Parent;
		}
		else if (testEvent is IInterval)
		{
			timeline = ((IInterval)testEvent).Track.Group.Timeline;
		}
		else if (testEvent is IKey)
		{
			timeline = ((IKey)testEvent).Track.Group.Timeline;
		}
		else if (testEvent is IMarker)
		{
			timeline = ((IMarker)testEvent).Timeline;
		}
		if (timeline == target)
		{
			return false;
		}
		if (timeline == timelineReference.Parent)
		{
			return true;
		}
		return true;
	}
}
