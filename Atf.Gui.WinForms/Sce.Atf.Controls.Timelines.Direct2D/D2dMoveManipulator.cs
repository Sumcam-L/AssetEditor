using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Timelines.Direct2D;

public class D2dMoveManipulator
{
	private static readonly D2dTimelineControl.SnapOptions s_snapOptions = new D2dTimelineControl.SnapOptions();

	private readonly D2dTimelineControl m_owner;

	private GhostInfo[] m_ghosts;

	private HitRecord m_mouseMoveHitRecord;

	private Point m_firstPoint;

	private Point m_currentPoint;

	private int m_dragThreshold = 3;

	private bool m_dragOverThreshold;

	private bool m_isDragging;

	private bool m_isDragDropping;

	protected D2dTimelineControl Owner => m_owner;

	protected GhostInfo[] Ghosts => m_ghosts;

	protected bool IsMovingSelection
	{
		get
		{
			if (m_isDragDropping)
			{
				return true;
			}
			return m_mouseMoveHitRecord != null && DragOverThreshold && !m_owner.IsUsingMouse;
		}
	}

	private Point DragDelta
	{
		get
		{
			if (!m_isDragDropping)
			{
				return m_owner.DragDelta;
			}
			Point dragPoint = DragPoint;
			return new Point(dragPoint.X - m_firstPoint.X, dragPoint.Y - m_firstPoint.Y);
		}
	}

	private bool DragOverThreshold
	{
		get
		{
			if (!m_isDragDropping)
			{
				return m_owner.DragOverThreshold;
			}
			return m_isDragging && m_dragOverThreshold;
		}
	}

	private Point DragPoint => m_dragOverThreshold ? m_currentPoint : m_firstPoint;

	public D2dMoveManipulator(D2dTimelineControl owner)
	{
		m_owner = owner;
		m_owner.MouseDownPicked += owner_MouseDownPicked;
		m_owner.MouseMovePicked += owner_MouseMovePicked;
		m_owner.MouseMove += owner_MouseMove;
		m_owner.MouseDown += owner_MouseDown;
		m_owner.MouseUp += owner_MouseUp;
		m_owner.DrawingD2d += owner_DrawingD2d;
		m_owner.KeyDown += owner_KeyDown;
		m_owner.DragOver += owner_DragOver;
		m_owner.DragEnter += owner_DragEnter;
		m_owner.DragLeave += owner_DragLeave;
		m_owner.DragDrop += owner_DragDrop;
	}

	protected virtual void MoveSelection()
	{
		PointF dragOffset = GetDragOffset();
		if (!m_isDragDropping && dragOffset.X == 0f && dragOffset.Y == 0f)
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
		if (m_isDragDropping)
		{
			_MoveSelection(tryToCopy: false, createTracksAndGroups: false);
			return;
		}
		bool tryToCopy = Control.ModifierKeys == Keys.Control;
		m_owner.TransactionContext.DoTransaction(delegate
		{
			_MoveSelection(tryToCopy, createTracksAndGroups: true);
		}, "Move Events".Localize("Move Manipulator's undo / redo description for moving timeline events"));
	}

	private void _MoveSelection(bool tryToCopy, bool createTracksAndGroups)
	{
		ITimeline timeline = m_owner.TimelineDocument.Timeline;
		Dictionary<ITrack, ITrack> newTrackMap = new Dictionary<ITrack, ITrack>();
		List<Pair<ITrack, IEvent>> list = new List<Pair<ITrack, IEvent>>();
		for (int i = 0; i < m_ghosts.Length; i++)
		{
			GhostInfo ghostInfo = m_ghosts[i];
			ITimelineObject timelineObject = null;
			if (tryToCopy && ghostInfo.Object is ICloneable cloneable)
			{
				timelineObject = cloneable.Clone() as ITimelineObject;
			}
			ITimelineReference timelineReference = ghostInfo.Object as ITimelineReference;
			if (timelineReference != null)
			{
				if (timelineObject != null)
				{
					timelineReference = (ITimelineReference)timelineObject;
				}
				timelineReference.Start = ghostInfo.Start;
				if (timelineObject != null && timeline is IHierarchicalTimelineList)
				{
					((IHierarchicalTimelineList)timeline).References.Add(timelineReference);
				}
				continue;
			}
			IInterval interval = ghostInfo.Object as IInterval;
			if (interval != null)
			{
				if (timelineObject != null)
				{
					interval = (IInterval)timelineObject;
				}
				interval.Start = ghostInfo.Start;
				interval.Length = ghostInfo.Length;
				ITrack track = (ITrack)ghostInfo.Target;
				if (track != interval.Track && createTracksAndGroups)
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
				continue;
			}
			IKey key = ghostInfo.Object as IKey;
			if (key != null)
			{
				if (timelineObject != null)
				{
					key = (IKey)timelineObject;
				}
				key.Start = ghostInfo.Start;
				ITrack track2 = (ITrack)ghostInfo.Target;
				if (track2 != key.Track && createTracksAndGroups)
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
				continue;
			}
			if (ghostInfo.Object is IMarker marker)
			{
				if (ghostInfo.Valid && marker.Start != ghostInfo.Start)
				{
					if (timelineObject != null)
					{
						IMarker marker2 = (IMarker)timelineObject;
						marker2.Start = ghostInfo.Start;
						marker.Timeline.Markers.Add(marker2);
					}
					else
					{
						marker.Start = ghostInfo.Start;
					}
				}
				continue;
			}
			ITrack track3 = ghostInfo.Object as ITrack;
			if (track3 != null)
			{
				if (ghostInfo.Target is ITrack track4 && track4 != track3)
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
				else if (ghostInfo.Target is IGroup obj)
				{
					if (timelineObject != null)
					{
						track3 = (ITrack)timelineObject;
					}
					else
					{
						track3.Group.Tracks.Remove(track3);
					}
					obj.Tracks.Insert(0, track3);
				}
				continue;
			}
			IGroup obj2 = ghostInfo.Object as IGroup;
			if (obj2 == null)
			{
				continue;
			}
			IGroup obj3 = (IGroup)ghostInfo.Target;
			if (obj3 != null && obj3 != obj2)
			{
				if (timelineObject != null)
				{
					obj2 = (IGroup)timelineObject;
				}
				int index2 = m_owner.TimelineDocument.Timeline.Groups.IndexOf(obj3);
				if (timelineObject == null)
				{
					m_owner.TimelineDocument.Timeline.Groups.Remove(obj2);
				}
				m_owner.TimelineDocument.Timeline.Groups.Insert(index2, obj2);
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
	}

	private PointF GetDragOffset()
	{
		if (!m_isDragDropping)
		{
			return m_owner.GetDragOffset();
		}
		PointF result = GdiUtil.InverseTransformVector(m_owner.Transform, DragDelta);
		result.X = m_owner.ConstrainFrameOffset(result.X);
		return result;
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

	private void owner_DragOver(object sender, DragEventArgs e)
	{
		IEnumerable<ITimelineObject> dragDropObjects = m_owner.DragDropObjects;
		if (dragDropObjects == null)
		{
			return;
		}
		ref Point firstPoint = ref m_firstPoint;
		int x = (m_currentPoint.X = e.X);
		firstPoint.X = x;
		ref Point firstPoint2 = ref m_firstPoint;
		x = (m_currentPoint.Y = e.Y);
		firstPoint2.Y = x;
		float x3 = m_owner.PointToClient(new Point(e.X, e.Y)).X;
		float start = GdiUtil.InverseTransform(m_owner.Transform, x3);
		foreach (ITimelineObject item in dragDropObjects)
		{
			if (item is IEvent obj)
			{
				obj.Start = start;
			}
		}
		MouseMove(e.X, e.Y);
	}

	private void owner_MouseMove(object sender, MouseEventArgs e)
	{
		MouseMove(e.X, e.Y);
	}

	private void MouseMove(int x, int y)
	{
		m_currentPoint = new Point(x, y);
		int num = Math.Abs(m_currentPoint.X - m_firstPoint.X);
		int num2 = Math.Abs(m_currentPoint.Y - m_firstPoint.Y);
		if (m_isDragging && !m_dragOverThreshold && (num > m_dragThreshold || num2 > m_dragThreshold))
		{
			m_dragOverThreshold = true;
		}
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

	private void owner_DragEnter(object sender, DragEventArgs e)
	{
		MouseDown(e.X, e.Y);
	}

	private void owner_MouseDown(object sender, MouseEventArgs e)
	{
		MouseDown(e.X, e.Y);
	}

	private void MouseDown(int x, int y)
	{
		m_firstPoint = (m_currentPoint = new Point(x, y));
		m_isDragging = true;
		m_isDragDropping = m_owner.DragDropObjects != null;
	}

	private void owner_DragLeave(object sender, EventArgs e)
	{
		CancelDrag();
	}

	private void owner_DragDrop(object sender, DragEventArgs e)
	{
		owner_MouseUp(sender, null);
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

	private void owner_DrawingD2d(object sender, EventArgs e)
	{
		Rectangle clientRectangle = m_owner.ClientRectangle;
		Matrix transform = m_owner.Transform;
		if (!IsMovingSelection || m_ghosts == null)
		{
			return;
		}
		m_owner.Renderer.DrawGhosts(m_ghosts, GhostType.Move, transform, clientRectangle);
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
		m_isDragging = false;
		m_dragOverThreshold = false;
		m_isDragDropping = false;
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
		PointF dragOffset = GetDragOffset();
		List<float> list = new List<float>(2);
		TimelinePath timelinePath = ((m_mouseMoveHitRecord == null) ? (m_owner.Selection.LastSelected as TimelinePath) : m_mouseMoveHitRecord.HitPath);
		IEvent obj = ((timelinePath != null) ? (timelinePath.Last as IEvent) : null);
		if (obj != null)
		{
			Matrix matrix = D2dTimelineControl.CalculateLocalToWorld(timelinePath);
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
		if (m_owner.DragDropObjects != null)
		{
			foreach (ITimelineObject dragDropObject in m_owner.DragDropObjects)
			{
				layout[dragDropObject] = GetDragDropBounds(dragDropObject);
			}
		}
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
				flag = track2 != null;
				flag &= m_owner.Constraints.IsStartValid(interval, ref start);
				flag &= m_owner.Constraints.IsLengthValid(interval, ref length);
				if (flag)
				{
					ITrack track3 = interval.Track;
					y = ((track3 == null) ? (layout[timelineObject].Y - bounds.Y) : (layout[timelineObject].Y - layout[interval.Track].Y));
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
				ITrack track4 = timelineObject as ITrack;
				flag = track4 != null;
				flag &= m_owner.Constraints.IsStartValid(key, ref start);
				if (flag)
				{
					ITrack track5 = key.Track;
					y = ((track5 == null) ? (layout[track4].Y - bounds.Y) : (layout[track4].Y - layout[key.Track].Y));
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
					timelineObject = ((DragDelta.Y < 0) ? GetLastTrack() : GetFirstTrack());
				}
			}
			else if (obj2 != null)
			{
				x = 0f;
				if (timelineObject == null)
				{
					IList<IGroup> groups = m_owner.TimelineDocument.Timeline.Groups;
					timelineObject = ((DragDelta.Y < 0) ? groups[0] : groups[groups.Count - 1]);
				}
			}
			bounds.Offset(x, y);
			array[num2] = new GhostInfo(last, timelineObject, start, length, bounds, flag);
		}
		return array;
	}

	private RectangleF GetDragDropBounds(ITimelineObject dragDrop)
	{
		RectangleF r = default(RectangleF);
		if (dragDrop is IInterval interval)
		{
			r = m_owner.Renderer.GetBounds(interval, 0f, m_owner.Transform, m_owner.ClientRectangle);
		}
		if (dragDrop is IKey key)
		{
			r = m_owner.Renderer.GetBounds(key, 0f, m_owner.Transform, m_owner.ClientRectangle);
		}
		return GdiUtil.Transform(m_owner.Transform, r);
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
					Point dragPoint = DragPoint;
					timelinePath.Last = item3;
					RectangleF rectangleF = layout[timelinePath];
					if (rectangleF.Top <= (float)dragPoint.Y && rectangleF.Bottom >= (float)dragPoint.Y)
					{
						array[num] = new TimelinePath(timelinePath);
						break;
					}
				}
				continue;
			}
			if (item2.Last is ITrack)
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
				if (!(array[num] == null))
				{
					continue;
				}
				foreach (IGroup item5 in list)
				{
					Point dragPoint3 = DragPoint;
					timelinePath.Last = item5;
					RectangleF rectangleF3 = layout[timelinePath];
					if (rectangleF3.Top <= (float)dragPoint3.Y && rectangleF3.Bottom >= (float)dragPoint3.Y)
					{
						array[num] = new TimelinePath(timelinePath);
						break;
					}
				}
				continue;
			}
			if (item2.Last is IInterval { Track: var track2 })
			{
				if (track2 != null)
				{
					timelinePath.Last = track2;
					RectangleF rectangleF4 = layout[timelinePath];
					float y = rectangleF4.Top + rectangleF4.Height * 0.5f + (float)DragDelta.Y;
					array[num] = GetTargetTrack(y, layout, list2);
					continue;
				}
				float y2 = m_owner.RectangleToClient(new Rectangle(Cursor.Position, default(Size))).Y;
				array[num] = GetTargetTrack(y2, layout, list2);
			}
			if (item2.Last is IKey { Track: var track3 })
			{
				if (track3 != null)
				{
					timelinePath.Last = track3;
					RectangleF rectangleF4 = layout[timelinePath];
					float y3 = rectangleF4.Top + rectangleF4.Height * 0.5f + (float)DragDelta.Y;
					array[num] = GetTargetTrack(y3, layout, list2);
				}
				else
				{
					float y4 = m_owner.RectangleToClient(new Rectangle(Cursor.Position, default(Size))).Y;
					array[num] = GetTargetTrack(y4, layout, list2);
				}
			}
		}
		return array;
	}

	private bool MoveSnapFilter(IEvent testEvent, D2dTimelineControl.SnapOptions options)
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
