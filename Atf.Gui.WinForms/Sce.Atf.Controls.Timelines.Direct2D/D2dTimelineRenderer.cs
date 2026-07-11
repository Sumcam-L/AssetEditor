using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D;

public abstract class D2dTimelineRenderer : IDisposable
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

	public class Context
	{
		public readonly D2dGraphics Graphics;

		public readonly RectangleF ClientRectangle;

		public RectangleF Bounds;

		public readonly D2dTextFormat TextFormat;

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

		public Context(D2dTimelineRenderer renderer, Matrix transform, RectangleF clientRectangle, D2dGraphics g)
		{
			Graphics = g;
			Transform = transform;
			ClientRectangle = clientRectangle;
			Bounds = GdiUtil.InverseTransform(transform, clientRectangle);
			TextFormat = renderer.m_textFormat;
			FontHeight = renderer.m_textFormat.FontHeight;
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

	public readonly Font Font;

	private static int s_pickTolerance = 3;

	private static int s_headerWidth = 128;

	private static int s_keySize = 12;

	private static int s_majorTickSpacing = 64;

	private static float s_trackHeight = 1f;

	private D2dGraphics m_graphics;

	private int m_margin = 8;

	private int m_timeScaleHeight = 24;

	private float m_minimumGraphStep = 1f;

	private bool m_disposed;

	private float m_offsetX;

	private float m_minimumTrackSize = 0.025f;

	private int m_expanderRectSize = 8;

	private Padding m_expanderRectMargin = new Padding(3, 3, 3, 3);

	private int m_trackIndent = 16;

	private bool m_printing;

	private Rectangle m_marginBounds;

	private D2dBrush m_gridPen;

	private D2dBrush m_headerBrush;

	private D2dBrush m_headerLineBrush;

	private D2dBrush m_scaleLineBrush;

	private D2dBrush m_expanderBrush;

	private D2dBrush m_nameBrush;

	private D2dBrush m_scaleTextBrush;

	private D2dSolidColorBrush m_generalSolidColorBrush;

	private D2dTextFormat m_trackTextFormat;

	private readonly D2dTextFormat m_textFormat;

	private Pair<ITimeline, TimelineLayout> m_cachedLayout;

	public D2dGraphics Graphics => m_graphics;

	public D2dBrush GridBrush
	{
		get
		{
			return m_gridPen;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_gridPen.Dispose();
			m_gridPen = value;
		}
	}

	public D2dBrush HeaderBrush
	{
		get
		{
			return m_headerBrush;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_headerBrush.Dispose();
			m_headerBrush = value;
		}
	}

	public D2dBrush HeaderLineBrush
	{
		get
		{
			return m_headerLineBrush;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_headerLineBrush.Dispose();
			m_headerLineBrush = value;
		}
	}

	public D2dBrush ScaleLineBrush
	{
		get
		{
			return m_scaleLineBrush;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_scaleLineBrush.Dispose();
			m_scaleLineBrush = value;
		}
	}

	public D2dBrush ScaleTextBrush
	{
		get
		{
			return m_scaleTextBrush;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_scaleTextBrush.Dispose();
			m_scaleTextBrush = value;
		}
	}

	public D2dBrush ExpanderBrush
	{
		get
		{
			return m_expanderBrush;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_expanderBrush.Dispose();
			m_expanderBrush = value;
		}
	}

	public D2dBrush NameBrush
	{
		get
		{
			return m_nameBrush;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_nameBrush.Dispose();
			m_nameBrush = value;
		}
	}

	public D2dTextFormat TextFormat => m_textFormat;

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

	public float OffsetX => m_offsetX;

	[DefaultValue(128)]
	public virtual int HeaderWidth
	{
		get
		{
			return GlobalHeaderWidth;
		}
		set
		{
			GlobalHeaderWidth = value;
			OnInvalidated(EventArgs.Empty);
		}
	}

	[DefaultValue(128)]
	public static int GlobalHeaderWidth
	{
		get
		{
			return s_headerWidth;
		}
		set
		{
			s_headerWidth = value;
		}
	}

	[DefaultValue(12)]
	public static int GlobalKeySize
	{
		get
		{
			return s_keySize;
		}
		set
		{
			s_keySize = value;
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
			return GlobalMajorTickSpacing;
		}
		set
		{
			if (GlobalMajorTickSpacing != value)
			{
				GlobalMajorTickSpacing = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(64)]
	public static int GlobalMajorTickSpacing
	{
		get
		{
			return s_majorTickSpacing;
		}
		set
		{
			s_majorTickSpacing = value;
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
	public virtual int PickTolerance
	{
		get
		{
			return GlobalPickTolerance;
		}
		set
		{
			GlobalPickTolerance = value;
		}
	}

	[DefaultValue(3)]
	public static int GlobalPickTolerance
	{
		get
		{
			return s_pickTolerance;
		}
		set
		{
			s_pickTolerance = value;
		}
	}

	[DefaultValue(1)]
	public static float GlobalTrackHeight
	{
		get
		{
			return s_trackHeight;
		}
		set
		{
			s_trackHeight = value;
		}
	}

	public event EventHandler Invalidated;

	protected D2dTimelineRenderer()
		: this(SystemFonts.StatusFont)
	{
	}

	protected D2dTimelineRenderer(Font font)
	{
		Font = font;
		m_textFormat = D2dFactory.CreateTextFormat(font);
	}

	public virtual void Init(D2dGraphics graphics)
	{
		if (m_graphics != null)
		{
			throw new InvalidOperationException("Only call Init() once");
		}
		m_graphics = graphics;
		m_gridPen = m_graphics.CreateSolidBrush(Color.FromArgb(128, 128, 128, 128));
		m_headerBrush = m_graphics.CreateSolidBrush(SystemColors.Control);
		m_headerLineBrush = m_graphics.CreateSolidBrush(SystemColors.ControlDark);
		m_scaleLineBrush = m_graphics.CreateSolidBrush(SystemColors.ControlDark);
		m_expanderBrush = m_graphics.CreateSolidBrush(Color.DimGray);
		m_nameBrush = m_graphics.CreateSolidBrush(SystemColors.WindowText);
		m_scaleTextBrush = m_graphics.CreateSolidBrush(SystemColors.WindowText);
		m_generalSolidColorBrush = m_graphics.CreateSolidBrush(Color.Empty);
		m_trackTextFormat = D2dFactory.CreateTextFormat(Font);
		m_trackTextFormat.ParagraphAlignment = D2dParagraphAlignment.Far;
		m_trackTextFormat.TextAlignment = D2dTextAlignment.Trailing;
	}

	~D2dTimelineRenderer()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_disposed)
		{
			if (disposing)
			{
				m_gridPen.Dispose();
				m_headerBrush.Dispose();
				m_headerLineBrush.Dispose();
				m_scaleLineBrush.Dispose();
				m_expanderBrush.Dispose();
				m_nameBrush.Dispose();
				m_scaleTextBrush.Dispose();
				m_generalSolidColorBrush.Dispose();
				m_trackTextFormat.Dispose();
				m_textFormat.Dispose();
			}
			m_disposed = true;
		}
	}

	protected virtual void OnInvalidated(EventArgs e)
	{
		this.Invalidated?.Invoke(this, e);
	}

	public TimelineLayout GetLayout(ITimeline timeline, Matrix transform, Rectangle clientRectangle)
	{
		Context c = new Context(this, transform, clientRectangle, m_graphics);
		return Layout(timeline, c);
	}

	public void Print(ITimeline timeline, ISelectionContext selection, TimelinePath activeGroup, TimelinePath activeTrack, Matrix transform, RectangleF clientRectangle, Rectangle marginBounds)
	{
		m_marginBounds = marginBounds;
		try
		{
			m_printing = true;
			Draw(timeline, selection, activeGroup, activeTrack, transform, clientRectangle);
		}
		finally
		{
			m_printing = false;
		}
	}

	public virtual TimelineLayout Draw(ITimeline timeline, ISelectionContext selection, TimelinePath activeGroup, TimelinePath activeTrack, Matrix transform, RectangleF clientRectangle)
	{
		Context context = new Context(this, transform, clientRectangle, m_graphics);
		TimelineLayout timelineLayout = Layout(timeline, context);
		context.ClearRecursionData();
		try
		{
			if (m_printing)
			{
				transform.Translate(m_marginBounds.Left, m_marginBounds.Top, MatrixOrder.Append);
				m_graphics.PushAxisAlignedClip(m_marginBounds);
			}
			m_graphics.FillRectangle(new RectangleF(0f, 0f, HeaderWidth, context.ClientRectangle.Height), m_headerBrush);
			DrawSubTimeline(null, timeline, subTimeline: false, expandedTimeline: true, selection, activeGroup, activeTrack, timelineLayout, context);
			foreach (TimelinePath item in D2dTimelineControl.GetHierarchy(timeline))
			{
				DrawSubTimeline(item, selection, activeGroup, activeTrack, timelineLayout, context);
			}
			m_graphics.DrawLine(new PointF(TrackIndent, m_timeScaleHeight), new PointF(TrackIndent, context.ClientRectangle.Height), m_headerLineBrush);
			m_graphics.DrawLine(new PointF(HeaderWidth, m_timeScaleHeight), new PointF(HeaderWidth, context.ClientRectangle.Height), m_headerLineBrush);
			if (m_printing)
			{
				context.Graphics.TranslateTransform(0f, m_marginBounds.Top);
			}
			DrawEventOverlay(context);
			m_graphics.DrawLine(new PointF(0f, m_timeScaleHeight), new PointF(HeaderWidth, m_timeScaleHeight), m_scaleLineBrush);
		}
		finally
		{
			if (m_printing)
			{
				m_graphics.PopAxisAlignedClip();
			}
		}
		RectangleF clipBounds = m_graphics.ClipBounds;
		clipBounds.X = HeaderWidth;
		DrawMarkers(null, timeline, selection, context, timelineLayout, clipBounds);
		return timelineLayout;
	}

	private void DrawSubTimeline(TimelinePath path, ISelectionContext selection, TimelinePath activeGroup, TimelinePath activeTrack, TimelineLayout layout, Context c)
	{
		Matrix m = D2dTimelineControl.CalculateLocalToWorld(path);
		c.PushTransform(m, MatrixOrder.Prepend);
		ITimelineReference timelineReference = (ITimelineReference)path.Last;
		RectangleF clipBounds = m_graphics.ClipBounds;
		RectangleF bounds = layout.GetBounds(path);
		IHierarchicalTimeline target = timelineReference.Target;
		if (bounds.IntersectsWith(clipBounds))
		{
			DrawMode drawMode = DrawMode.Normal;
			if (selection.SelectionContains(path))
			{
				drawMode |= DrawMode.Selected;
			}
			DrawTimelineReference(timelineReference, bounds, drawMode, c);
		}
		if (target != null)
		{
			DrawSubTimeline(path, target, subTimeline: true, timelineReference.Options.Expanded, selection, activeGroup, activeTrack, layout, c);
		}
		c.PopTransform();
	}

	private void DrawSubTimeline(TimelinePath path, ITimeline timeline, bool subTimeline, bool expandedTimeline, ISelectionContext selection, TimelinePath activeGroup, TimelinePath activeTrack, TimelineLayout layout, Context c)
	{
		if (!subTimeline)
		{
			m_offsetX = c.Transform.OffsetX;
		}
		RectangleF clipBounds = m_graphics.ClipBounds;
		DrawGroupsAndTracks(path, timeline, expandedTimeline, selection, c, layout, clipBounds);
		if (subTimeline)
		{
			clipBounds.X = HeaderWidth;
			clipBounds.Width -= HeaderWidth;
			DrawMarkers(path, timeline, selection, c, layout, clipBounds);
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
			RectangleF layoutRect = new RectangleF(bounds.X, bounds.Y, HeaderWidth, bounds.Height);
			DrawMoveHandle(bounds, selection.SelectionContains(timelinePath), timelinePath == activeGroup);
			if (tracks.Count > 1)
			{
				RectangleF expanderRect = GetExpanderRect(bounds);
				m_graphics.DrawExpander(expanderRect.X, expanderRect.Y, expanderRect.Width, m_expanderBrush, group.Expanded);
				layoutRect.X += TrackIndent;
				layoutRect.Width -= TrackIndent;
			}
			if (group.Expanded || tracks.Count == 1)
			{
				foreach (ITrack item in tracks)
				{
					TimelinePath timelinePath2 = path + item;
					bounds = layout.GetBounds(timelinePath2);
					bounds = GetTrackHandleRect(bounds);
					DrawMoveHandle(bounds, selection.SelectionContains(timelinePath2), timelinePath2 == activeTrack);
					if (bounds.Width > 0f)
					{
						m_graphics.DrawText(item.Name, m_trackTextFormat, bounds, m_nameBrush);
					}
				}
			}
			if (layoutRect.Width > 0f)
			{
				m_graphics.DrawText(group.Name, c.TextFormat, layoutRect, m_nameBrush);
			}
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
		rect.X = HeaderWidth;
		rect.Width -= HeaderWidth;
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

	public void DrawGhosts(ICollection<GhostInfo> ghosts, GhostType type, Matrix transform, Rectangle clientRectangle)
	{
		Context context = new Context(this, transform, clientRectangle, m_graphics);
		RectangleF clipBounds = context.Graphics.ClipBounds;
		RectangleF clipRect = new RectangleF(clipBounds.X + (float)HeaderWidth, clipBounds.Y, clipBounds.Width - (float)HeaderWidth, clipBounds.Height);
		try
		{
			m_graphics.PushAxisAlignedClip(clipRect);
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
		}
		finally
		{
			m_graphics.PopAxisAlignedClip();
		}
	}

	public virtual IList<HitRecord> Pick(ITimeline timeline, RectangleF pickRect, Matrix transform, Rectangle clientRectangle)
	{
		List<HitRecord> list = new List<HitRecord>();
		Context c = new Context(this, transform, clientRectangle, m_graphics);
		TimelineLayout cachedLayout = GetCachedLayout(timeline, c);
		PickSubTimeline(null, timeline, pickRect, c, cachedLayout, list);
		if (!(pickRect.Left > (float)HeaderWidth) || !(pickRect.Right < (float)clientRectangle.Width) || !(pickRect.Bottom > 0f) || !(pickRect.Bottom < (float)TimeScaleHeight))
		{
			foreach (TimelinePath item in D2dTimelineControl.GetHierarchy(timeline))
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
		if (timeline == null)
		{
			return;
		}
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
		DrawReferenceBackground(reference, bounds, drawMode, c);
		RectangleF labelBounds = new RectangleF(bounds.X, bounds.Y, HeaderWidth, bounds.Height);
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
			labelBounds.X += TrackIndent;
			labelBounds.Width -= TrackIndent;
		}
		DrawReferenceLabel(reference, labelBounds, drawMode, c);
		RectangleF originRect = bounds;
		float num = GdiUtil.Transform(c.Transform, 0f) + bounds.X;
		originRect.X = num - 4f;
		originRect.Width = 8f;
		originRect.Height = 16f;
		if (originRect.X + originRect.Width >= (float)HeaderWidth && originRect.X <= c.ClientRectangle.Width)
		{
			DrawReferenceOrigin(reference, originRect, drawMode, c);
		}
		if (target != null)
		{
			bounds = GetGroupHandleRect(bounds, collapsed);
			if (flag2)
			{
				RectangleF expanderRect = GetExpanderRect(bounds);
				m_graphics.DrawExpander(expanderRect.X, expanderRect.Y, expanderRect.Width, m_expanderBrush, flag);
			}
		}
	}

	protected virtual void DrawReferenceBackground(ITimelineReference reference, RectangleF bounds, DrawMode drawMode, Context c)
	{
		if (drawMode == DrawMode.Normal)
		{
			m_generalSolidColorBrush.Color = reference.Color;
		}
		else
		{
			m_generalSolidColorBrush.Color = Color.LightCoral;
		}
		c.Graphics.FillRectangle(bounds, m_generalSolidColorBrush);
	}

	protected virtual void DrawReferenceLabel(ITimelineReference reference, RectangleF labelBounds, DrawMode drawMode, Context c)
	{
		c.Graphics.DrawText(reference.Name, c.TextFormat, labelBounds, m_nameBrush);
	}

	protected virtual void DrawReferenceOrigin(ITimelineReference reference, RectangleF originRect, DrawMode drawMode, Context c)
	{
		Color color = ((drawMode != DrawMode.Normal) ? Color.Gray : Color.Black);
		c.Graphics.DrawLines(new PointF[5]
		{
			new PointF(originRect.X + originRect.Width * 0.5f, originRect.Y),
			new PointF(originRect.X, originRect.Y + originRect.Height * 0.5f),
			new PointF(originRect.X + originRect.Width * 0.5f, originRect.Bottom),
			new PointF(originRect.Right, originRect.Y + originRect.Height * 0.5f),
			new PointF(originRect.X + originRect.Width * 0.5f, originRect.Y)
		}, color);
	}

	protected virtual void DrawEventOverlay(Context c)
	{
		m_generalSolidColorBrush.Color = Color.White;
		m_graphics.FillRectangle(new RectangleF(HeaderWidth, 0f, c.ClientRectangle.Width, m_timeScaleHeight), m_generalSolidColorBrush);
		RectangleF clientRectangle = c.ClientRectangle;
		clientRectangle.X += HeaderWidth;
		clientRectangle.Width -= HeaderWidth;
		clientRectangle = GdiUtil.InverseTransform(c.Transform, clientRectangle);
		c.Graphics.DrawVerticalScaleGrid(c.Transform, clientRectangle, MajorTickSpacing, m_gridPen);
		c.Graphics.DrawHorizontalScale(c.Transform, clientRectangle, top: true, MajorTickSpacing, m_minimumGraphStep, m_gridPen, m_textFormat, m_scaleTextBrush);
	}

	protected virtual void DrawMoveHandle(RectangleF r, bool selected, bool active)
	{
		Color color = Color.LightGray;
		if (active)
		{
			color = Color.DarkGray;
		}
		if (selected)
		{
			color = Color.Tomato;
		}
		m_graphics.FillRectangle(r, color);
		m_graphics.DrawLine(r.Left, r.Bottom, r.Right, r.Bottom, Color.DarkGray);
		m_graphics.DrawLine(r.Right, r.Bottom, r.Right, r.Top, Color.DarkGray);
		m_graphics.DrawLine(r.Left, r.Bottom, r.Left, r.Top, Color.WhiteSmoke);
		m_graphics.DrawLine(r.Left, r.Top, r.Right, r.Top, Color.WhiteSmoke);
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

	public RectangleF GetBounds(IInterval interval, float trackTop, Matrix transform, RectangleF clientRectangle)
	{
		Context c = new Context(this, transform, clientRectangle, m_graphics);
		return GetBounds(interval, trackTop, c);
	}

	public RectangleF GetBounds(IKey key, float trackTop, Matrix transform, RectangleF clientRectangle)
	{
		Context c = new Context(this, transform, clientRectangle, m_graphics);
		return GetBounds(key, trackTop, c);
	}

	public RectangleF GetBounds(IMarker marker, Matrix transform, RectangleF clientRectangle)
	{
		Context c = new Context(this, transform, clientRectangle, m_graphics);
		return GetBounds(marker, c);
	}

	protected TimelineLayout Layout(ITimeline timeline, Context c)
	{
		TimelineLayout timelineLayout = new TimelineLayout();
		float documentTop = c.PixelSize.Height * (float)m_timeScaleHeight;
		LayoutSubTimeline(null, timeline, ref documentTop, expandedTimeline: true, c, timelineLayout);
		foreach (TimelinePath item in D2dTimelineControl.GetHierarchy(timeline))
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
			Matrix m = D2dTimelineControl.CalculateLocalToWorld(path);
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
					result.Add(path + interval, bounds);
				}
				foreach (IKey key in track.Keys)
				{
					bounds = GetBounds(key, num5, c);
					num6 = Math.Max(num6, bounds.Bottom);
					bounds = GdiUtil.Transform(c.Transform, bounds);
					result.Add(path + key, bounds);
				}
				num6 += num;
				num6 = Math.Max(num6, num4 + MinimumTrackSize);
				bounds = new RectangleF(c.Bounds.X, num4, c.Bounds.Width, num6 - num4);
				bounds = GdiUtil.Transform(c.Transform, bounds);
				bounds.X = c.ClientRectangle.X;
				result.Add(path + track, bounds);
				if (flag)
				{
					num4 = num6;
				}
				val = Math.Max(val, num6);
			}
			val = Math.Max(val, num2 + Math.Max(num * 2f, MinimumTrackSize));
			float height = val - num2;
			bounds = new RectangleF(0f, num2, c.Bounds.Width, height);
			bounds = GdiUtil.Transform(c.Transform, bounds);
			bounds.X = c.ClientRectangle.X;
			result.Add(path + group, bounds);
			if (expandedTimeline)
			{
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
		int num = (collapsed ? HeaderWidth : TrackIndent);
		return new RectangleF(0f, bounds.Y, num, bounds.Height);
	}

	private RectangleF GetTrackHandleRect(RectangleF bounds)
	{
		return new RectangleF(TrackIndent, bounds.Y, HeaderWidth - TrackIndent, bounds.Height);
	}

	private RectangleF GetExpanderRect(RectangleF bounds)
	{
		return new RectangleF(bounds.X + (float)ExpanderRectMargin.Left, bounds.Y + (float)ExpanderRectMargin.Top, ExpanderRectSize, ExpanderRectSize);
	}

	private int CompareHits(HitRecord x, HitRecord y)
	{
		return x.Type.CompareTo(y.Type);
	}
}
