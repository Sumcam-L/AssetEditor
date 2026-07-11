using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Timelines;

public abstract class TimelineRenderer : IDisposable
{
	[Flags]
	protected enum DrawMode
	{
		Normal = 1,
		Collapsed = 2,
		Ghost = 4,
		Invalid = 8,
		States = 0xF,
		ResizeLeft = 0x10,
		ResizeRight = 0x20,
		Selected = 0x40,
		Modifiers = 0x70,
		Printing = 0x80
	}

	protected class Context
	{
		public readonly Graphics Graphics;

		public readonly RectangleF ClientRectangle;

		public RectangleF Bounds;

		public readonly Font Font;

		public readonly float FontHeight;

		public readonly SizeF PixelSize;

		private Matrix m_transform;

		private Matrix m_inverseTransform;

		private readonly Stack<Matrix> m_stack = new Stack<Matrix>(1);

		private readonly HashSet<ITimeline> m_timelines = new HashSet<ITimeline>();

		public Matrix Transform
		{
			get
			{
				return m_transform;
			}
			set
			{
				m_transform = value.Clone();
				m_inverseTransform = value.Clone();
				m_inverseTransform.Invert();
			}
		}

		public Matrix InverseTransform => m_inverseTransform;

		public Context(TimelineRenderer renderer, Matrix transform, RectangleF clientRectangle, Graphics g)
		{
			Graphics = g;
			Transform = transform;
			ClientRectangle = clientRectangle;
			Bounds = GdiUtil.InverseTransform(transform, clientRectangle);
			Font = renderer.m_font;
			FontHeight = Font.Height;
			PixelSize = new SizeF(1f / transform.Elements[0], 1f / transform.Elements[3]);
		}

		public Matrix PushTransform(Matrix m, MatrixOrder matrixOrder)
		{
			m_stack.Push(m_transform);
			m_transform = m_transform.Clone();
			m_transform.Multiply(m, matrixOrder);
			m_inverseTransform = m_transform.Clone();
			m_inverseTransform.Invert();
			return m_transform;
		}

		public Matrix PopTransform()
		{
			m_transform = m_stack.Pop();
			m_inverseTransform = m_transform.Clone();
			m_inverseTransform.Invert();
			return m_transform;
		}

		public void ClearRecursionData()
		{
			m_timelines.Clear();
		}
	}

	private readonly Font m_font;

	private int m_margin = 8;

	private int m_headerWidth = 128;

	private int m_timeScaleHeight = 24;

	private int m_majorTickSpacing = 64;

	private float m_minimumGraphStep = 1f;

	private int m_pickTolerance = 3;

	private readonly Pen m_gridPen;

	private bool m_disposed;

	private float m_OffsetX;

	private float m_minimumTrackSize = 0.025f;

	private int m_expanderRectSize = 8;

	private Padding m_expanderRectMargin = new Padding(3, 3, 3, 3);

	private int m_trackIndent = 16;

	private bool m_printing;

	private Rectangle m_marginBounds;

	private Pair<ITimeline, TimelineLayout> m_cachedLayout;

	private static readonly StringFormat s_groupLabelFormat;

	private static readonly StringFormat s_trackLabelFormat;

	protected Padding ExpanderRectMargin
	{
		get
		{
			return m_expanderRectMargin;
		}
		set
		{
			m_expanderRectMargin = value;
		}
	}

	protected int ExpanderRectSize
	{
		get
		{
			return m_expanderRectSize;
		}
		set
		{
			m_expanderRectSize = value;
		}
	}

	protected int TrackIndent
	{
		get
		{
			return m_trackIndent;
		}
		set
		{
			m_trackIndent = value;
		}
	}

	protected float MinimumTrackSize
	{
		get
		{
			return m_minimumTrackSize;
		}
		set
		{
			m_minimumTrackSize = value;
		}
	}

	public int Margin
	{
		get
		{
			return m_margin;
		}
		set
		{
			if (m_margin != value)
			{
				m_margin = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	public float OffsetX => m_OffsetX;

	[DefaultValue(128)]
	public int HeaderWidth
	{
		get
		{
			return m_headerWidth;
		}
		set
		{
			m_headerWidth = value;
			OnInvalidated(EventArgs.Empty);
		}
	}

	[DefaultValue(24)]
	public int TimeScaleHeight
	{
		get
		{
			return m_timeScaleHeight;
		}
		set
		{
			if (m_timeScaleHeight != value)
			{
				m_timeScaleHeight = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(64)]
	public int MajorTickSpacing
	{
		get
		{
			return m_majorTickSpacing;
		}
		set
		{
			if (m_majorTickSpacing != value)
			{
				m_majorTickSpacing = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(1f)]
	public float MinimumGraphStep
	{
		get
		{
			return m_minimumGraphStep;
		}
		set
		{
			if (m_minimumGraphStep != value)
			{
				m_minimumGraphStep = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(3)]
	public int PickTolerance
	{
		get
		{
			return m_pickTolerance;
		}
		set
		{
			m_pickTolerance = value;
		}
	}

	public event EventHandler Invalidated;

	public TimelineRenderer()
		: this(SystemFonts.StatusFont)
	{
	}

	public TimelineRenderer(Font font)
	{
		m_font = font;
		m_gridPen = new Pen(Color.FromArgb(128, 128, 128, 128));
	}

	~TimelineRenderer()
	{
		if (!m_disposed)
		{
			Dispose(disposing: false);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposed)
		{
			return;
		}
		if (disposing)
		{
			if (!m_font.IsSystemFont)
			{
				m_font.Dispose();
			}
			m_gridPen.Dispose();
		}
		m_disposed = true;
	}

	protected virtual void OnInvalidated(EventArgs e)
	{
		this.Invalidated?.Invoke(this, e);
	}

	public TimelineLayout GetLayout(ITimeline timeline, Matrix transform, Rectangle clientRectangle, Graphics g)
	{
		Context c = new Context(this, transform, clientRectangle, g);
		return Layout(timeline, c);
	}

	public void Print(ITimeline timeline, ISelectionContext selection, TimelinePath activeGroup, TimelinePath activeTrack, Matrix transform, RectangleF clientRectangle, Rectangle marginBounds, Graphics g)
	{
		m_marginBounds = marginBounds;
		try
		{
			m_printing = true;
			Draw(timeline, selection, activeGroup, activeTrack, transform, clientRectangle, g);
		}
		finally
		{
			m_printing = false;
		}
	}

	public virtual TimelineLayout Draw(ITimeline timeline, ISelectionContext selection, TimelinePath activeGroup, TimelinePath activeTrack, Matrix transform, RectangleF clientRectangle, Graphics g)
	{
		if (m_printing)
		{
			transform.Translate(m_marginBounds.Left, m_marginBounds.Top, MatrixOrder.Append);
			g.SetClip(m_marginBounds);
		}
		Context context = new Context(this, transform, clientRectangle, g);
		TimelineLayout timelineLayout = Layout(timeline, context);
		context.ClearRecursionData();
		g.FillRectangle(SystemBrushes.Control, 0f, 0f, m_headerWidth, context.ClientRectangle.Height);
		DrawSubTimeline(null, timeline, subTimeline: false, expandedTimeline: true, selection, activeGroup, activeTrack, timelineLayout, context);
		foreach (TimelinePath item in TimelineControl.GetHierarchy(timeline))
		{
			DrawSubTimeline(item, selection, activeGroup, activeTrack, timelineLayout, context);
		}
		g.DrawLine(SystemPens.ControlDark, TrackIndent, m_timeScaleHeight, TrackIndent, context.ClientRectangle.Height);
		g.DrawLine(SystemPens.ControlDark, m_headerWidth, m_timeScaleHeight, m_headerWidth, context.ClientRectangle.Height);
		if (m_printing)
		{
			context.Graphics.TranslateTransform(0f, m_marginBounds.Top);
		}
		DrawEventOverlay(context);
		g.DrawLine(SystemPens.ControlDark, 0, m_timeScaleHeight, m_headerWidth, m_timeScaleHeight);
		RectangleF clipBounds = g.ClipBounds;
		clipBounds.X = m_headerWidth;
		DrawMarkers(null, timeline, selection, context, timelineLayout, clipBounds);
		return timelineLayout;
	}

	private void DrawSubTimeline(TimelinePath path, ISelectionContext selection, TimelinePath activeGroup, TimelinePath activeTrack, TimelineLayout layout, Context c)
	{
		ITimelineReference timelineReference = (ITimelineReference)path.Last;
		Graphics graphics = c.Graphics;
		RectangleF clipBounds = graphics.ClipBounds;
		RectangleF bounds = layout.GetBounds(path);
		IHierarchicalTimeline target = timelineReference.Target;
		if (bounds.IntersectsWith(clipBounds))
		{
			DrawTimelineReference(timelineReference, bounds, DrawMode.Normal, c);
		}
		if (target != null)
		{
			Matrix m = TimelineControl.CalculateLocalToWorld(path);
			c.PushTransform(m, MatrixOrder.Prepend);
			DrawSubTimeline(path, target, subTimeline: true, timelineReference.Options.Expanded, selection, activeGroup, activeTrack, layout, c);
			c.PopTransform();
		}
	}

	private void DrawSubTimeline(TimelinePath path, ITimeline timeline, bool subTimeline, bool expandedTimeline, ISelectionContext selection, TimelinePath activeGroup, TimelinePath activeTrack, TimelineLayout layout, Context c)
	{
		if (!subTimeline)
		{
			m_OffsetX = c.Transform.OffsetX;
		}
		Graphics graphics = c.Graphics;
		RectangleF clipBounds = graphics.ClipBounds;
		DrawGroupsAndTracks(path, timeline, expandedTimeline, selection, c, layout, clipBounds);
		if (subTimeline)
		{
			clipBounds.X = m_headerWidth;
			clipBounds.Width -= m_headerWidth;
			DrawMarkers(path, timeline, selection, c, layout, clipBounds);
			clipBounds = graphics.ClipBounds;
		}
		if (!expandedTimeline)
		{
			return;
		}
		if (m_printing)
		{
			c.Graphics.TranslateTransform(m_marginBounds.Left, 0f);
		}
		foreach (IGroup group in timeline.Groups)
		{
			IList<ITrack> tracks = group.Tracks;
			TimelinePath timelinePath = path + group;
			RectangleF bounds = layout.GetBounds(timelinePath);
			bounds = GetGroupHandleRect(bounds, !group.Expanded);
			RectangleF layoutRectangle = new RectangleF(bounds.X, bounds.Y, m_headerWidth, bounds.Height);
			DrawMoveHandle(bounds, selection.SelectionContains(timelinePath), timelinePath == activeGroup, graphics);
			if (tracks.Count > 1)
			{
				RectangleF expanderRect = GetExpanderRect(bounds);
				GdiUtil.DrawExpander((int)expanderRect.X, (int)expanderRect.Y, (int)expanderRect.Width, Brushes.DimGray, group.Expanded, graphics);
				layoutRectangle.X += TrackIndent;
				layoutRectangle.Width -= TrackIndent;
			}
			if (group.Expanded || tracks.Count == 1)
			{
				foreach (ITrack item in tracks)
				{
					TimelinePath timelinePath2 = path + item;
					bounds = layout.GetBounds(timelinePath2);
					bounds = GetTrackHandleRect(bounds);
					DrawMoveHandle(bounds, selection.SelectionContains(timelinePath2), timelinePath2 == activeTrack, graphics);
					graphics.DrawString(item.Name, c.Font, SystemBrushes.WindowText, bounds, s_trackLabelFormat);
				}
			}
			graphics.DrawString(group.Name, c.Font, SystemBrushes.WindowText, layoutRectangle, s_groupLabelFormat);
		}
		if (m_printing)
		{
			c.Graphics.TranslateTransform(-m_marginBounds.Left, 0f);
		}
	}

	private void DrawMarkers(TimelinePath path, ITimeline timeline, ISelectionContext selection, Context c, TimelineLayout layout, RectangleF clipBounds)
	{
		foreach (IMarker marker in timeline.Markers)
		{
			TimelinePath timelinePath = path + marker;
			if (layout.TryGetBounds(timelinePath, out var bounds) && bounds.IntersectsWith(clipBounds))
			{
				DrawMode drawMode = DrawMode.Normal;
				if (selection.SelectionContains(timelinePath))
				{
					drawMode |= DrawMode.Selected;
				}
				Draw(marker, bounds, drawMode, c);
			}
		}
	}

	private void DrawGroupsAndTracks(TimelinePath path, ITimeline timeline, bool expandedTimeline, ISelectionContext selection, Context c, TimelineLayout layout, RectangleF clipBounds)
	{
		RectangleF rect = clipBounds;
		rect.X = m_headerWidth;
		rect.Width -= m_headerWidth;
		foreach (IGroup group in timeline.Groups)
		{
			TimelinePath timelinePath = path + group;
			if (!layout.TryGetBounds(timelinePath, out var bounds) || !bounds.IntersectsWith(clipBounds))
			{
				continue;
			}
			DrawMode drawMode = DrawMode.Normal;
			if (selection.SelectionContains(timelinePath))
			{
				drawMode |= DrawMode.Selected;
			}
			if (expandedTimeline)
			{
				Draw(group, bounds, drawMode, c);
			}
			IList<ITrack> tracks = group.Tracks;
			bool flag = !expandedTimeline || (!group.Expanded && tracks.Count > 1);
			foreach (ITrack item in tracks)
			{
				TimelinePath timelinePath2 = path + item;
				bounds = layout.GetBounds(timelinePath2);
				if (!bounds.IntersectsWith(clipBounds))
				{
					continue;
				}
				drawMode = DrawMode.Normal;
				if (selection.SelectionContains(timelinePath2))
				{
					drawMode |= DrawMode.Selected;
				}
				if (flag)
				{
					drawMode = DrawMode.Collapsed;
				}
				Draw(item, bounds, drawMode, c);
				foreach (IInterval interval in item.Intervals)
				{
					TimelinePath timelinePath3 = path + interval;
					bounds = layout.GetBounds(timelinePath3);
					if (bounds.IntersectsWith(rect))
					{
						drawMode = DrawMode.Normal;
						if (selection.SelectionContains(timelinePath3))
						{
							drawMode |= DrawMode.Selected;
						}
						if (flag)
						{
							drawMode = DrawMode.Collapsed;
						}
						Draw(interval, bounds, drawMode, c);
					}
				}
				foreach (IKey key in item.Keys)
				{
					TimelinePath timelinePath4 = path + key;
					bounds = layout.GetBounds(timelinePath4);
					if (bounds.IntersectsWith(rect))
					{
						drawMode = DrawMode.Normal;
						if (selection.SelectionContains(timelinePath4))
						{
							drawMode |= DrawMode.Selected;
						}
						if (flag)
						{
							drawMode = DrawMode.Collapsed;
						}
						Draw(key, bounds, drawMode, c);
					}
				}
			}
		}
	}

	public void DrawGhosts(ICollection<GhostInfo> ghosts, GhostType type, Matrix transform, Rectangle clientRectangle, Graphics g)
	{
		Context context = new Context(this, transform, clientRectangle, g);
		RectangleF clipBounds = context.Graphics.ClipBounds;
		RectangleF clip = new RectangleF(clipBounds.X + (float)m_headerWidth, clipBounds.Y, clipBounds.Width - (float)m_headerWidth, clipBounds.Height);
		context.Graphics.SetClip(clip);
		DrawMode drawMode = (DrawMode)0;
		if (type == GhostType.ResizeLeft)
		{
			drawMode |= DrawMode.ResizeLeft;
		}
		if (type == GhostType.ResizeRight)
		{
			drawMode |= DrawMode.ResizeRight;
		}
		foreach (GhostInfo ghost in ghosts)
		{
			DrawMode drawMode2 = (ghost.Valid ? DrawMode.Ghost : DrawMode.Invalid);
			drawMode2 |= drawMode;
			if (ghost.Object is IInterval interval)
			{
				Draw(interval, ghost.Bounds, drawMode2, context);
			}
			else if (ghost.Object is ITimelineReference reference)
			{
				DrawTimelineReference(reference, ghost.Bounds, drawMode2, context);
			}
			else if (ghost.Object is IKey key)
			{
				Draw(key, ghost.Bounds, drawMode2, context);
			}
			else if (ghost.Object is IMarker marker)
			{
				Draw(marker, ghost.Bounds, drawMode2, context);
			}
			else if (ghost.Object is ITrack track)
			{
				Draw(track, ghost.Bounds, drawMode2, context);
			}
			else if (ghost.Object is IGroup obj)
			{
				Draw(obj, ghost.Bounds, drawMode2, context);
			}
		}
		context.Graphics.SetClip(clipBounds);
	}

	public virtual IList<HitRecord> Pick(ITimeline timeline, RectangleF pickRect, Matrix transform, Rectangle clientRectangle, Graphics g)
	{
		List<HitRecord> list = new List<HitRecord>();
		Context c = new Context(this, transform, clientRectangle, g);
		TimelineLayout cachedLayout = GetCachedLayout(timeline, c);
		PickSubTimeline(null, timeline, pickRect, c, cachedLayout, list);
		if (!(pickRect.Left > (float)HeaderWidth) || !(pickRect.Right < (float)clientRectangle.Width) || !(pickRect.Bottom > 0f) || !(pickRect.Bottom < (float)TimeScaleHeight))
		{
			foreach (TimelinePath item in TimelineControl.GetHierarchy(timeline))
			{
				PickSubTimeline(item, pickRect, c, cachedLayout, list);
			}
		}
		PrioritizeHits(list);
		return list;
	}

	private void PickSubTimeline(TimelinePath path, RectangleF pickRect, Context c, TimelineLayout layout, List<HitRecord> result)
	{
		RectangleF clipBounds = c.Graphics.ClipBounds;
		ITimelineReference timelineReference = (ITimelineReference)path.Last;
		if (!layout.TryGetBounds(path, out var bounds))
		{
			return;
		}
		IHierarchicalTimeline target = timelineReference.Target;
		if (target != null && timelineReference.Options.Expanded)
		{
			PickSubTimeline(path, target, pickRect, c, layout, result);
		}
		if (target != null && bounds.IntersectsWith(clipBounds))
		{
			IList<IGroup> groups = target.Groups;
			bool flag = groups.Count > 0;
			bool expanded = timelineReference.Options.Expanded;
			bool collapsed = flag && !expanded;
			RectangleF groupHandleRect = GetGroupHandleRect(bounds, collapsed);
			RectangleF expanderRect = GetExpanderRect(groupHandleRect);
			if (flag && expanderRect.IntersectsWith(pickRect))
			{
				result.Add(new HitRecord(HitType.GroupExpand, path));
			}
			else if (bounds.IntersectsWith(pickRect))
			{
				result.Add(new HitRecord(HitType.Key, path));
			}
		}
		else if (bounds.IntersectsWith(pickRect))
		{
			result.Add(new HitRecord(HitType.Key, path));
		}
	}

	private void PickSubTimeline(TimelinePath root, ITimeline timeline, RectangleF pickRect, Context c, TimelineLayout layout, List<HitRecord> result)
	{
		RectangleF clipBounds = c.Graphics.ClipBounds;
		RectangleF clientRectangle = c.ClientRectangle;
		RectangleF bounds;
		foreach (IMarker marker in timeline.Markers)
		{
			if (layout.TryGetBounds(root + marker, out bounds) && bounds.IntersectsWith(clipBounds) && pickRect.Right >= (float)HeaderWidth)
			{
				HitType hitType = Pick(marker, bounds, pickRect, c);
				if (hitType != HitType.None)
				{
					result.Add(new HitRecord(hitType, root + marker));
				}
			}
		}
		if (pickRect.Left > (float)HeaderWidth && pickRect.Right < clientRectangle.Width && pickRect.Bottom > 0f && pickRect.Bottom < (float)TimeScaleHeight)
		{
			if (result.Count == 0 && pickRect.Height <= (float)(2 * PickTolerance) && pickRect.Width <= (float)(2 * PickTolerance))
			{
				result.Add(new HitRecord(HitType.TimeScale, null));
			}
			return;
		}
		IList<IGroup> groups = timeline.Groups;
		for (int num = groups.Count - 1; num >= 0; num--)
		{
			IGroup obj = groups[num];
			if (layout.ContainsPath(root + obj))
			{
				IList<ITrack> tracks = obj.Tracks;
				bool expanded = obj.Expanded;
				bool flag = tracks.Count > 1;
				bool flag2 = flag && !expanded;
				if (!flag2)
				{
					for (int num2 = tracks.Count - 1; num2 >= 0; num2--)
					{
						ITrack track = tracks[num2];
						IList<IKey> keys = track.Keys;
						for (int num3 = keys.Count - 1; num3 >= 0; num3--)
						{
							IKey key = keys[num3];
							if (layout.TryGetBounds(root + key, out bounds) && bounds.IntersectsWith(clipBounds) && pickRect.Right >= (float)HeaderWidth)
							{
								HitType hitType = Pick(key, bounds, pickRect, c);
								if (hitType != HitType.None)
								{
									result.Add(new HitRecord(hitType, root + key));
								}
							}
						}
						IList<IInterval> intervals = track.Intervals;
						for (int num4 = intervals.Count - 1; num4 >= 0; num4--)
						{
							IInterval interval = intervals[num4];
							if (layout.TryGetBounds(root + interval, out bounds) && bounds.IntersectsWith(clipBounds) && pickRect.Right >= (float)HeaderWidth)
							{
								HitType hitType = Pick(interval, bounds, pickRect, c);
								if (hitType != HitType.None)
								{
									result.Add(new HitRecord(hitType, root + interval));
								}
							}
						}
						if (layout.TryGetBounds(root + track, out bounds) && bounds.IntersectsWith(clipBounds))
						{
							if (GetTrackHandleRect(bounds).IntersectsWith(pickRect))
							{
								result.Add(new HitRecord(HitType.TrackMove, root + track));
							}
							else if (bounds.IntersectsWith(pickRect))
							{
								result.Add(new HitRecord(HitType.Track, root + track));
							}
						}
					}
				}
				if (layout.TryGetBounds(root + obj, out bounds) && bounds.IntersectsWith(clipBounds))
				{
					RectangleF groupHandleRect = GetGroupHandleRect(bounds, flag2);
					RectangleF expanderRect = GetExpanderRect(groupHandleRect);
					if (flag && expanderRect.IntersectsWith(pickRect))
					{
						result.Add(new HitRecord(HitType.GroupExpand, root + obj));
					}
					else if (groupHandleRect.IntersectsWith(pickRect))
					{
						result.Add(new HitRecord(HitType.GroupMove, root + obj));
					}
					else if (bounds.IntersectsWith(pickRect))
					{
						result.Add(new HitRecord(HitType.Group, root + obj));
					}
				}
			}
		}
		if (pickRect.Left < (float)HeaderWidth && (float)HeaderWidth < pickRect.Right)
		{
			result.Add(new HitRecord(HitType.HeaderResize, null));
		}
	}

	protected virtual void PrioritizeHits(List<HitRecord> hits)
	{
		if (hits.Count > 1)
		{
			hits.Sort(CompareHits);
		}
	}

	protected abstract void Draw(IGroup group, RectangleF bounds, DrawMode drawMode, Context c);

	protected abstract void Draw(ITrack track, RectangleF bounds, DrawMode drawMode, Context c);

	protected abstract void Draw(IInterval interval, RectangleF bounds, DrawMode drawMode, Context c);

	protected abstract void Draw(IKey key, RectangleF bounds, DrawMode drawMode, Context c);

	protected abstract void Draw(IMarker marker, RectangleF bounds, DrawMode drawMode, Context c);

	protected virtual void DrawTimelineReference(ITimelineReference reference, RectangleF bounds, DrawMode drawMode, Context c)
	{
		Graphics graphics = c.Graphics;
		Color color;
		if (drawMode == DrawMode.Normal)
		{
			using (Brush brush = new SolidBrush(reference.Color))
			{
				graphics.FillRectangle(brush, bounds);
			}
			color = Color.Black;
		}
		else
		{
			color = Color.Gray;
		}
		RectangleF layoutRectangle = new RectangleF(bounds.X, bounds.Y, m_headerWidth, bounds.Height);
		IHierarchicalTimeline target = reference.Target;
		bool flag = false;
		bool flag2 = false;
		bool collapsed = false;
		if (target != null)
		{
			IList<IGroup> groups = target.Groups;
			flag = reference.Options.Expanded;
			flag2 = groups.Count > 0;
			collapsed = flag2 && !flag;
		}
		if (flag2)
		{
			layoutRectangle.X += TrackIndent;
			layoutRectangle.Width -= TrackIndent;
		}
		graphics.DrawString(reference.Name, c.Font, SystemBrushes.WindowText, layoutRectangle, s_groupLabelFormat);
		RectangleF rectangleF = bounds;
		float num = GdiUtil.Transform(c.Transform, reference.Start) + bounds.X;
		rectangleF.X = num - 4f;
		rectangleF.Width = 8f;
		rectangleF.Height = 16f;
		if (rectangleF.X + rectangleF.Width >= (float)m_headerWidth && rectangleF.X <= c.ClientRectangle.Width)
		{
			using Pen pen = new Pen(color);
			graphics.DrawLines(pen, new PointF[5]
			{
				new PointF(rectangleF.X + rectangleF.Width * 0.5f, rectangleF.Y),
				new PointF(rectangleF.X, rectangleF.Y + rectangleF.Height * 0.5f),
				new PointF(rectangleF.X + rectangleF.Width * 0.5f, rectangleF.Bottom),
				new PointF(rectangleF.Right, rectangleF.Y + rectangleF.Height * 0.5f),
				new PointF(rectangleF.X + rectangleF.Width * 0.5f, rectangleF.Y)
			});
		}
		if (target != null)
		{
			bounds = GetGroupHandleRect(bounds, collapsed);
			if (flag2)
			{
				RectangleF expanderRect = GetExpanderRect(bounds);
				GdiUtil.DrawExpander((int)expanderRect.X, (int)expanderRect.Y, (int)expanderRect.Width, Brushes.DimGray, flag, graphics);
			}
		}
	}

	protected virtual void DrawEventOverlay(Context c)
	{
		c.Graphics.FillRectangle(Brushes.White, m_headerWidth, 0f, c.ClientRectangle.Width, m_timeScaleHeight);
		RectangleF clientRectangle = c.ClientRectangle;
		clientRectangle.X += m_headerWidth;
		clientRectangle.Width -= m_headerWidth;
		clientRectangle = GdiUtil.InverseTransform(c.Transform, clientRectangle);
		ChartUtil.DrawVerticalScaleGrid(c.Transform, clientRectangle, m_majorTickSpacing, m_gridPen, c.Graphics);
		ChartUtil.DrawHorizontalScale(c.Transform, clientRectangle, top: true, m_majorTickSpacing, m_minimumGraphStep, m_gridPen, m_font, Brushes.Gray, c.Graphics);
	}

	protected virtual void DrawMoveHandle(RectangleF r, bool selected, bool active, Graphics g)
	{
		Brush brush = Brushes.LightGray;
		if (active)
		{
			brush = Brushes.DarkGray;
		}
		if (selected)
		{
			brush = Brushes.Tomato;
		}
		g.FillRectangle(brush, r);
		g.DrawLine(Pens.DarkGray, r.Left, r.Bottom, r.Right, r.Bottom);
		g.DrawLine(Pens.DarkGray, r.Right, r.Bottom, r.Right, r.Top);
		g.DrawLine(Pens.WhiteSmoke, r.Left, r.Bottom, r.Left, r.Top);
		g.DrawLine(Pens.WhiteSmoke, r.Left, r.Top, r.Right, r.Top);
	}

	protected virtual HitType Pick(IInterval interval, RectangleF bounds, RectangleF pickRect, Context c)
	{
		if (!bounds.IntersectsWith(pickRect))
		{
			return HitType.None;
		}
		if (pickRect.Left <= bounds.Left && pickRect.Right >= bounds.Left)
		{
			return HitType.IntervalResizeLeft;
		}
		if (pickRect.Left <= bounds.Right && pickRect.Right >= bounds.Right)
		{
			return HitType.IntervalResizeRight;
		}
		return HitType.Interval;
	}

	protected virtual HitType Pick(IKey key, RectangleF bounds, RectangleF pickRect, Context c)
	{
		return (!bounds.IntersectsWith(pickRect)) ? HitType.None : HitType.Key;
	}

	protected virtual HitType Pick(IMarker marker, RectangleF bounds, RectangleF pickRect, Context c)
	{
		return bounds.IntersectsWith(pickRect) ? HitType.Marker : HitType.None;
	}

	protected abstract RectangleF GetBounds(IInterval interval, float trackTop, Context c);

	protected abstract RectangleF GetBounds(IKey key, float trackTop, Context c);

	protected abstract RectangleF GetBounds(IMarker marker, Context c);

	protected TimelineLayout Layout(ITimeline timeline, Context c)
	{
		TimelineLayout timelineLayout = new TimelineLayout();
		float documentTop = c.PixelSize.Height * (float)m_timeScaleHeight;
		LayoutSubTimeline(null, timeline, ref documentTop, expandedTimeline: true, c, timelineLayout);
		foreach (TimelinePath item in TimelineControl.GetHierarchy(timeline))
		{
			LayoutSubTimeline(item, ref documentTop, c, timelineLayout);
		}
		m_cachedLayout.First = timeline;
		m_cachedLayout.Second = timelineLayout;
		return timelineLayout;
	}

	protected TimelineLayout GetCachedLayout(ITimeline timeline, Context c)
	{
		if (m_cachedLayout.First == timeline)
		{
			return m_cachedLayout.Second;
		}
		return Layout(timeline, c);
	}

	private void LayoutSubTimeline(TimelinePath path, ref float documentTop, Context c, TimelineLayout result)
	{
		float num = c.PixelSize.Height * (float)m_margin;
		RectangleF bounds = c.Bounds;
		c.Bounds.Y = documentTop;
		float num2 = Math.Max(num * 2f, MinimumTrackSize);
		RectangleF r = new RectangleF(c.Bounds.X, documentTop, c.Bounds.Width, num2);
		documentTop += num2;
		ITimelineReference timelineReference = (ITimelineReference)path.Last;
		TimelineReferenceOptions options = timelineReference.Options;
		IHierarchicalTimeline target = timelineReference.Target;
		if (target != null)
		{
			Matrix m = TimelineControl.CalculateLocalToWorld(path);
			c.PushTransform(m, MatrixOrder.Prepend);
			LayoutSubTimeline(path, target, ref documentTop, options.Expanded, c, result);
			c.PopTransform();
		}
		if (!options.Expanded)
		{
			num2 = Math.Max(num2, documentTop - r.Y);
			r.Height = num2;
		}
		r = GdiUtil.Transform(c.Transform, r);
		result.Add(path, r);
		c.Bounds = bounds;
	}

	private void LayoutSubTimeline(TimelinePath path, ITimeline timeline, ref float documentTop, bool expandedTimeline, Context c, TimelineLayout result)
	{
		float num = c.PixelSize.Height * (float)m_margin;
		float num2 = documentTop;
		float num3 = num2;
		foreach (IGroup group in timeline.Groups)
		{
			bool flag = expandedTimeline && group.Expanded;
			float val = num2;
			float num4 = num2;
			RectangleF bounds;
			foreach (ITrack track in group.Tracks)
			{
				float num5 = num4 + num;
				float num6 = num5;
				foreach (IInterval interval in track.Intervals)
				{
					bounds = GetBounds(interval, num5, c);
					num6 = Math.Max(num6, bounds.Bottom);
					bounds = GdiUtil.Transform(c.Transform, bounds);
					if (expandedTimeline)
					{
						result.Add(path + interval, bounds);
					}
				}
				foreach (IKey key in track.Keys)
				{
					bounds = GetBounds(key, num5, c);
					num6 = Math.Max(num6, bounds.Bottom);
					bounds = GdiUtil.Transform(c.Transform, bounds);
					if (expandedTimeline)
					{
						result.Add(path + key, bounds);
					}
				}
				num6 += num;
				num6 = Math.Max(num6, num4 + MinimumTrackSize);
				bounds = new RectangleF(c.Bounds.X, num4, c.Bounds.Width, num6 - num4);
				bounds = GdiUtil.Transform(c.Transform, bounds);
				bounds.X = c.ClientRectangle.X;
				if (expandedTimeline)
				{
					result.Add(path + track, bounds);
				}
				if (flag)
				{
					num4 = num6;
				}
				val = Math.Max(val, num6);
			}
			val = Math.Max(val, num2 + c.PixelSize.Height);
			float height = val - num2;
			bounds = new RectangleF(0f, num2, c.Bounds.Width, height);
			bounds = GdiUtil.Transform(c.Transform, bounds);
			bounds.X = c.ClientRectangle.X;
			if (expandedTimeline)
			{
				result.Add(path + group, bounds);
				num2 = val;
			}
			num3 = Math.Max(num3, val);
		}
		if (expandedTimeline)
		{
			RectangleF bounds2 = c.Bounds;
			c.Bounds.Height = num3 - c.Bounds.Y;
			foreach (IMarker marker in timeline.Markers)
			{
				RectangleF bounds = GetBounds(marker, c);
				bounds = GdiUtil.Transform(c.Transform, bounds);
				result.Add(path + marker, bounds);
			}
			c.Bounds = bounds2;
		}
		documentTop = num3;
	}

	private RectangleF GetGroupHandleRect(RectangleF bounds, bool collapsed)
	{
		int num = (collapsed ? m_headerWidth : TrackIndent);
		return new RectangleF(0f, bounds.Y, num, bounds.Height);
	}

	private RectangleF GetTrackHandleRect(RectangleF bounds)
	{
		return new RectangleF(TrackIndent, bounds.Y, m_headerWidth - TrackIndent, bounds.Height);
	}

	private RectangleF GetExpanderRect(RectangleF bounds)
	{
		return new RectangleF(bounds.X + (float)ExpanderRectMargin.Left, bounds.Y + (float)ExpanderRectMargin.Top, ExpanderRectSize, ExpanderRectSize);
	}

	private int CompareHits(HitRecord x, HitRecord y)
	{
		return x.Type.CompareTo(y.Type);
	}

	static TimelineRenderer()
	{
		s_groupLabelFormat = new StringFormat(StringFormatFlags.NoWrap);
		s_groupLabelFormat.Trimming = StringTrimming.EllipsisCharacter;
		s_trackLabelFormat = new StringFormat(StringFormatFlags.NoWrap);
		s_trackLabelFormat.Alignment = StringAlignment.Far;
		s_trackLabelFormat.LineAlignment = StringAlignment.Far;
		s_trackLabelFormat.Trimming = StringTrimming.EllipsisCharacter;
	}
}
