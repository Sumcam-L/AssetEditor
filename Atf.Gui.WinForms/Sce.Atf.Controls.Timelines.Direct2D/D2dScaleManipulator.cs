using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D;

public class D2dScaleManipulator
{
	protected enum ScaleMode
	{
		InPlace,
		TimePeriod
	}

	protected enum Side
	{
		Left,
		Right
	}

	protected class Resizer
	{
		public readonly ScaleMode Mode;

		public float DragOffsetWithSnap;

		private readonly D2dTimelineControl m_owner;

		private readonly HashSet<TimelinePath> m_selection;

		private readonly Side m_intervalSide;

		private readonly float m_originalBoundary;

		private float m_initialMin;

		private float m_initialMax;

		private float m_worldGhostMin;

		private float m_worldGhostMax;

		private GhostInfo[] m_ghosts;

		public Side DraggedSide => m_intervalSide;

		public float WorldGhostMin => m_worldGhostMin;

		public float WorldGhostMax => m_worldGhostMax;

		internal Resizer(Side side, ScaleMode mode, float boundary, D2dTimelineControl owner)
		{
			m_intervalSide = side;
			Mode = mode;
			m_originalBoundary = boundary;
			m_owner = owner;
			if (Mode == ScaleMode.InPlace)
			{
				m_selection = new HashSet<TimelinePath>(GetEditableEvents<IInterval>());
			}
			else
			{
				m_selection = new HashSet<TimelinePath>(GetEditableEvents<IEvent>());
			}
			if (m_selection.Count == 0)
			{
				throw new InvalidTransactionException("only IEvents can be resized");
			}
			m_initialMin = float.MaxValue;
			m_initialMax = float.MinValue;
			if (Mode == ScaleMode.TimePeriod)
			{
				foreach (TimelinePath item in m_selection)
				{
					IEvent obj = (IEvent)item.Last;
					if (obj.Start < m_initialMin)
					{
						m_initialMin = obj.Start;
					}
					if (m_initialMax < obj.Start + obj.Length)
					{
						m_initialMax = obj.Start + obj.Length;
					}
				}
			}
			DragOffsetWithSnap = m_owner.GetDragOffset().X;
		}

		private IEnumerable<TimelinePath> GetEditableEvents<T>() where T : class, IEvent
		{
			foreach (TimelinePath path in m_owner.Selection.GetSelection<TimelinePath>())
			{
				if (path.Last is T && m_owner.IsEditable(path))
				{
					yield return path;
				}
			}
		}

		internal GhostInfo[] CreateGhostInfo(TimelineLayout layout, float worldDrag, Matrix worldToView)
		{
			float[] movingPoints = new float[1] { worldDrag + m_originalBoundary };
			float num = m_owner.GetSnapOffset(movingPoints, null);
			worldDrag += num;
			DragOffsetWithSnap = worldDrag;
			GhostInfo[] array = new GhostInfo[m_selection.Count];
			IEnumerator<TimelinePath> enumerator = m_selection.GetEnumerator();
			m_worldGhostMin = float.MaxValue;
			m_worldGhostMax = float.MinValue;
			for (int i = 0; i < array.Length; i++)
			{
				enumerator.MoveNext();
				IEvent obj = (IEvent)enumerator.Current.Last;
				RectangleF rectangleF = layout[enumerator.Current];
				float viewStart = rectangleF.Left;
				float viewEnd = viewStart + rectangleF.Width;
				float worldStart = obj.Start;
				float worldEnd = worldStart + obj.Length;
				Resize(worldDrag, worldToView, ref viewStart, ref viewEnd, ref worldStart, ref worldEnd);
				float length = worldEnd - worldStart;
				rectangleF = new RectangleF(viewStart, rectangleF.Y, viewEnd - viewStart, rectangleF.Height);
				if (m_worldGhostMin > worldStart)
				{
					m_worldGhostMin = worldStart;
				}
				if (m_worldGhostMax < worldEnd)
				{
					m_worldGhostMax = worldEnd;
				}
				bool valid = true;
				if (obj is IInterval interval)
				{
					TimelinePath timelinePath = new TimelinePath(enumerator.Current);
					foreach (IInterval interval3 in interval.Track.Intervals)
					{
						IInterval interval2 = (IInterval)(timelinePath.Last = interval3);
						if ((Mode == ScaleMode.TimePeriod && m_selection.Contains(timelinePath)) || interval2 == interval || m_owner.Constraints.IsIntervalValid(interval, ref worldStart, ref length, interval2))
						{
							continue;
						}
						valid = false;
						break;
					}
				}
				array[i] = new GhostInfo(obj, null, worldStart, length, rectangleF, valid);
			}
			m_ghosts = array;
			return array;
		}

		internal void ResizeSelection()
		{
			if (m_ghosts == null || m_ghosts.Length == 0)
			{
				return;
			}
			GhostInfo[] ghosts = m_ghosts;
			foreach (GhostInfo ghostInfo in ghosts)
			{
				if (ghostInfo != null && !ghostInfo.Valid)
				{
					return;
				}
			}
			m_owner.TransactionContext.DoTransaction(delegate
			{
				GhostInfo[] ghosts2 = m_ghosts;
				foreach (GhostInfo ghostInfo2 in ghosts2)
				{
					if (ghostInfo2.Object is IEvent obj)
					{
						obj.Start = ghostInfo2.Start;
						obj.Length = ghostInfo2.Length;
					}
				}
			}, "Resize Events".Localize("scale manipulator's undo / redo description for resizing timeline events"));
		}

		private void Resize(float worldDrag, Matrix worldToView, ref float viewStart, ref float viewEnd, ref float worldStart, ref float worldEnd)
		{
			if (Mode == ScaleMode.InPlace)
			{
				m_initialMin = worldStart;
				m_initialMax = worldEnd;
			}
			float num;
			float num2;
			if (m_intervalSide == Side.Left)
			{
				num = m_owner.ConstrainFrameOffset(m_initialMin + worldDrag);
				if (num < 0f)
				{
					num = 0f;
				}
				else if (num >= m_initialMax)
				{
					num = m_initialMin;
				}
				num2 = m_initialMax;
			}
			else
			{
				num2 = m_owner.ConstrainFrameOffset(m_initialMax + worldDrag);
				if (num2 <= m_initialMin)
				{
					num2 = m_initialMax;
				}
				num = m_initialMin;
			}
			ScalePoint(ref worldStart, m_initialMin, m_initialMax, num, num2);
			worldStart = m_owner.ConstrainFrameOffset(worldStart);
			ScalePoint(ref worldEnd, m_initialMin, m_initialMax, num, num2);
			worldEnd = m_owner.ConstrainFrameOffset(worldEnd);
			viewStart = GdiUtil.Transform(worldToView, worldStart);
			viewEnd = GdiUtil.Transform(worldToView, worldEnd);
		}

		private static void ScalePoint(ref float x, float initialMin, float initialMax, float finalMin, float finalMax)
		{
			float num = initialMax - initialMin;
			if (!(num < 1E-05f))
			{
				x = (x - initialMin) * ((finalMax - finalMin) / num) + finalMin;
			}
		}
	}

	private class HitRecordObject
	{
		public readonly Side Side;

		public HitRecordObject(Side side)
		{
			Side = side;
		}

		public override string ToString()
		{
			return "drag left or right handles to scale current selection";
		}
	}

	public readonly D2dTimelineControl Owner;

	private bool m_visibleManipulator;

	private float m_worldMin;

	private float m_worldMax;

	private readonly HitRecordObject m_leftHitObject = new HitRecordObject(Side.Left);

	private readonly HitRecordObject m_rightHitObject = new HitRecordObject(Side.Right);

	private Resizer m_resizer;

	private RectangleF m_leftHandle;

	private RectangleF m_rightHandle;

	private static int s_handleHeight = 24;

	private static Color s_color = Color.Black;

	[DefaultValue(24)]
	public static int HandleHeight
	{
		get
		{
			return s_handleHeight;
		}
		set
		{
			s_handleHeight = value;
		}
	}

	[DefaultValue(typeof(Color), "Black")]
	public static Color Color
	{
		get
		{
			return s_color;
		}
		set
		{
			s_color = value;
		}
	}

	protected float WorldMin => m_worldMin;

	protected float WorldMax => m_worldMax;

	protected bool IsScaling => m_resizer != null && Owner.DragOverThreshold && !Owner.IsUsingMouse;

	protected Resizer ScaleHelper => m_resizer;

	protected GhostType DraggedSide => (m_resizer.DraggedSide == Side.Left) ? GhostType.ResizeLeft : GhostType.ResizeRight;

	public D2dScaleManipulator(D2dTimelineControl owner)
	{
		Owner = owner;
		Owner.MouseDownPicked += owner_MouseDownPicked;
		Owner.MouseMovePicked += owner_MouseMovePicked;
		Owner.Picking += owner_Picking;
		Owner.MouseUp += owner_MouseUp;
		Owner.DrawingD2d += owner_DrawingD2d;
	}

	protected virtual void DrawManipulator(D2dGraphics g, out RectangleF leftHandle, out RectangleF rightHandle)
	{
		Matrix transform = Owner.Transform;
		float num = GdiUtil.Transform(transform, WorldMin);
		float num2 = GdiUtil.Transform(transform, WorldMax);
		leftHandle = new RectangleF(num - 1.5f, 0f, 3f, HandleHeight);
		rightHandle = new RectangleF(num2 - 1.5f, 0f, 3f, HandleHeight);
		if (IsScaling && ScaleHelper.Mode == ScaleMode.TimePeriod)
		{
			num = GdiUtil.Transform(transform, ScaleHelper.WorldGhostMin);
			num2 = GdiUtil.Transform(transform, ScaleHelper.WorldGhostMax);
			g.DrawLine(num, 0f, num2, 0f, Color.Red, 3f);
			g.DrawLine(num, 0f, num, HandleHeight, Color.Red, 3f);
			g.DrawLine(num2, 0f, num2, HandleHeight, Color.Red, 3f);
		}
		else
		{
			g.DrawLine(num, 0f, num2, 0f, Color, 3f);
			g.DrawLine(num, 0f, num, HandleHeight, Color, 3f);
			g.DrawLine(num2, 0f, num2, HandleHeight, Color, 3f);
		}
	}

	private void owner_DrawingD2d(object sender, EventArgs e)
	{
		m_visibleManipulator = D2dTimelineControl.CalculateRange(Owner.EditableSelection, out m_worldMin, out m_worldMax);
		if (!m_visibleManipulator)
		{
			return;
		}
		D2dGraphics d2dGraphics = Owner.D2dGraphics;
		Matrix transform = Owner.Transform;
		if (IsScaling)
		{
			TimelineLayout layout = Owner.GetLayout();
			GhostInfo[] ghosts = m_resizer.CreateGhostInfo(layout, Owner.GetDragOffset().X, transform);
			Owner.Renderer.DrawGhosts(ghosts, DraggedSide, transform, Owner.ClientRectangle);
		}
		try
		{
			d2dGraphics.PushAxisAlignedClip(Owner.VisibleClientRectangle);
			DrawManipulator(d2dGraphics, out m_leftHandle, out m_rightHandle);
		}
		finally
		{
			d2dGraphics.PopAxisAlignedClip();
		}
	}

	private void owner_MouseDownPicked(object sender, HitEventArgs e)
	{
		if (e.MouseEvent.Button != MouseButtons.Left)
		{
			return;
		}
		TimelinePath hitPath = e.HitRecord.HitPath;
		bool isResizingSelection = false;
		switch (e.HitRecord.Type)
		{
		case HitType.IntervalResizeLeft:
			if (Owner.IsEditable(hitPath) && (Owner.Selection.SelectionContains(hitPath) || Owner.Select<IEvent>(hitPath)))
			{
				IInterval interval = e.HitRecord.HitTimelineObject as IInterval;
				m_resizer = new Resizer(Side.Left, ScaleMode.InPlace, interval.Start, Owner);
				isResizingSelection = true;
			}
			break;
		case HitType.IntervalResizeRight:
			if (Owner.IsEditable(hitPath) && (Owner.Selection.SelectionContains(hitPath) || Owner.Select<IEvent>(hitPath)))
			{
				IInterval interval2 = e.HitRecord.HitTimelineObject as IInterval;
				m_resizer = new Resizer(Side.Right, ScaleMode.InPlace, interval2.Start + interval2.Length, Owner);
				isResizingSelection = true;
			}
			break;
		case HitType.Custom:
			if (e.HitRecord.HitObject is HitRecordObject hitRecordObject)
			{
				if (hitRecordObject.Side == Side.Left)
				{
					m_resizer = new Resizer(Side.Left, ScaleMode.TimePeriod, m_worldMin, Owner);
				}
				else
				{
					m_resizer = new Resizer(Side.Right, ScaleMode.TimePeriod, m_worldMax, Owner);
				}
				isResizingSelection = true;
			}
			break;
		default:
			m_resizer = null;
			break;
		}
		Owner.IsResizingSelection = isResizingSelection;
	}

	private void owner_MouseMovePicked(object sender, HitEventArgs e)
	{
		if (e.MouseEvent.Button != MouseButtons.None)
		{
			return;
		}
		HitRecord hitRecord = e.HitRecord;
		TimelinePath hitPath = hitRecord.HitPath;
		switch (hitRecord.Type)
		{
		case HitType.IntervalResizeLeft:
		case HitType.IntervalResizeRight:
			if (Owner.IsEditable(hitPath))
			{
				Owner.Cursor = Cursors.SizeWE;
			}
			break;
		case HitType.Custom:
			if (hitRecord.HitObject == m_leftHitObject || hitRecord.HitObject == m_rightHitObject)
			{
				Owner.Cursor = Cursors.SizeWE;
			}
			break;
		}
	}

	private void owner_Picking(object sender, HitEventArgs e)
	{
		if (m_visibleManipulator)
		{
			Rectangle visibleClientRectangle = Owner.VisibleClientRectangle;
			if (m_leftHandle.IntersectsWith(visibleClientRectangle) && e.PickRectangle.IntersectsWith(m_leftHandle))
			{
				e.HitRecord = new HitRecord(HitType.Custom, m_leftHitObject);
			}
			else if (m_rightHandle.IntersectsWith(visibleClientRectangle) && e.PickRectangle.IntersectsWith(m_rightHandle))
			{
				e.HitRecord = new HitRecord(HitType.Custom, m_rightHitObject);
			}
		}
	}

	private void owner_MouseUp(object sender, MouseEventArgs e)
	{
		if (IsScaling)
		{
			m_resizer.ResizeSelection();
			m_resizer = null;
			Owner.Invalidate();
		}
		m_resizer = null;
	}
}
