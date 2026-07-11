using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Timelines;

public class TimelineControl : CanvasControl
{
	public delegate bool SnapFilter(IEvent testEvent, SnapOptions options);

	public class SnapOptions
	{
		public bool IncludeSelected;

		public bool IncludeScrubber = true;

		public bool CheckModifierKeys = true;

		public SnapFilter Filter;

		public object FilterContext;
	}

	public delegate float SnapOffsetFinder(IEnumerable<float> movingPoints, SnapOptions options);

	public class BoundingRectEventArgs : EventArgs
	{
		public readonly RectangleF CurrentClientRect;

		public RectangleF NewClientRect;

		public BoundingRectEventArgs(RectangleF currentClientRect)
		{
			CurrentClientRect = currentClientRect;
			NewClientRect = currentClientRect;
		}
	}

	private readonly TimelineRenderer m_timelineRenderer;

	private readonly TimelineConstraints m_timelineConstraints;

	private ITimelineDocument m_timelineDocument;

	private ITimeline m_timeline;

	private ISelectionContext m_selection;

	private ITransactionContext m_transactionContext;

	private TimelinePath m_activeTrack;

	private TimelinePath m_activeGroup;

	private MouseButtons m_panButton = MouseButtons.Left;

	private Keys m_panModifierKeys = Keys.Alt;

	private MouseButtons m_zoomButton = MouseButtons.Right;

	private Keys m_zoomModifierKeys = Keys.Alt;

	private Keys m_constrainModifierKeys = Keys.Shift;

	private Keys m_cancelDragKey = Keys.Escape;

	private bool m_moveGroupEventsTogether;

	private bool m_movingSelection;

	private bool m_resizingHeader;

	private bool m_isResizingSelection;

	private TimelineLayout m_layout;

	private SnapOffsetFinder m_snapper;

	private bool m_allowHeaderResize;

	private const int MinHeaderWidth = 32;

	private const int MaxHeaderWidth = 256;

	public override Matrix Transform
	{
		get
		{
			Matrix transform = base.Transform;
			transform.Translate(m_timelineRenderer.HeaderWidth, 0f, MatrixOrder.Append);
			return transform;
		}
	}

	public bool AllowHeaderResize
	{
		get
		{
			return m_allowHeaderResize;
		}
		set
		{
			m_allowHeaderResize = value;
		}
	}

	public SnapOffsetFinder GetSnapOffset
	{
		get
		{
			return m_snapper;
		}
		set
		{
			if (value != null)
			{
				m_snapper = value;
			}
			else
			{
				m_snapper = NoSnapOffset;
			}
		}
	}

	public override Rectangle VisibleClientRectangle
	{
		get
		{
			Rectangle visibleClientRectangle = base.VisibleClientRectangle;
			visibleClientRectangle.X += m_timelineRenderer.HeaderWidth;
			visibleClientRectangle.Width -= m_timelineRenderer.HeaderWidth;
			return visibleClientRectangle;
		}
	}

	public ITimelineDocument TimelineDocument
	{
		get
		{
			return m_timelineDocument;
		}
		set
		{
			if (m_timelineDocument != null)
			{
				m_selection.SelectionChanged -= selection_SelectionChanged;
				m_timelineDocument.ItemInserted -= timelineDocument_ItemInserted;
				m_timelineDocument.ItemRemoved -= timelineDocument_ItemRemoved;
				m_timelineDocument.ItemChanged -= timelineDocument_ItemChanged;
				m_timelineDocument.Reloaded -= timelineDocument_Reloaded;
			}
			if (value == null)
			{
				AllowDrop = false;
			}
			else
			{
				AllowDrop = true;
			}
			m_timelineDocument = value;
			if (m_timelineDocument != null)
			{
				m_timeline = m_timelineDocument.Timeline;
				m_timelineDocument.ItemInserted += timelineDocument_ItemInserted;
				m_timelineDocument.ItemRemoved += timelineDocument_ItemRemoved;
				m_timelineDocument.ItemChanged += timelineDocument_ItemChanged;
				m_timelineDocument.Reloaded += timelineDocument_Reloaded;
				m_selection = m_timelineDocument.Cast<ISelectionContext>();
				m_selection.SelectionChanged += selection_SelectionChanged;
				m_transactionContext = m_timelineDocument.As<ITransactionContext>();
			}
			else
			{
				m_timeline = null;
			}
			Invalidate();
		}
	}

	public ITimeline Timeline => (TimelineDocument != null) ? TimelineDocument.Timeline : null;

	public ISelectionContext Selection => m_selection;

	public ITransactionContext TransactionContext => m_transactionContext;

	public IEnumerable<TimelinePath> EditableSelection
	{
		get
		{
			foreach (TimelinePath selected in m_selection.GetSelection<TimelinePath>())
			{
				if (IsEditable(selected))
				{
					yield return selected;
				}
			}
		}
	}

	public TimelineRenderer Renderer => m_timelineRenderer;

	public TimelinePath TargetTrack => m_activeTrack;

	public TimelinePath TargetGroup => m_activeGroup;

	public MouseButtons PanButton
	{
		get
		{
			return m_panButton;
		}
		set
		{
			m_panButton = value;
		}
	}

	public Keys PanModifierKeys
	{
		get
		{
			return m_panModifierKeys;
		}
		set
		{
			m_panModifierKeys = value;
		}
	}

	public MouseButtons ZoomButton
	{
		get
		{
			return m_zoomButton;
		}
		set
		{
			m_zoomButton = value;
		}
	}

	public Keys ZoomModifierKeys
	{
		get
		{
			return m_zoomModifierKeys;
		}
		set
		{
			m_zoomModifierKeys = value;
		}
	}

	public Keys ConstrainModifierKeys
	{
		get
		{
			return m_constrainModifierKeys;
		}
		set
		{
			m_constrainModifierKeys = value;
		}
	}

	public Keys CancelDragKey
	{
		get
		{
			return m_cancelDragKey;
		}
		set
		{
			m_cancelDragKey = value;
		}
	}

	public bool MoveGroupEventsTogether
	{
		get
		{
			return m_moveGroupEventsTogether;
		}
		set
		{
			m_moveGroupEventsTogether = value;
		}
	}

	public TimelineConstraints Constraints => m_timelineConstraints;

	public IEnumerable<TimelinePath> AllEvents
	{
		get
		{
			foreach (TimelinePath @object in GetObjects<IEvent>(m_timeline))
			{
				yield return @object;
			}
		}
	}

	public IEnumerable<TimelinePath> AllMarkers
	{
		get
		{
			foreach (TimelinePath @object in GetObjects<IMarker>(m_timeline))
			{
				yield return @object;
			}
		}
	}

	public IEnumerable<TimelinePath> AllTracks
	{
		get
		{
			foreach (TimelinePath @object in GetObjects<ITrack>(m_timeline))
			{
				yield return @object;
			}
		}
	}

	public IEnumerable<TimelinePath> AllGroups
	{
		get
		{
			foreach (TimelinePath @object in GetObjects<IGroup>(m_timeline))
			{
				yield return @object;
			}
		}
	}

	public bool IsUsingMouse => base.IsDragging;

	public bool IsResizingSelection
	{
		get
		{
			return m_isResizingSelection;
		}
		set
		{
			m_isResizingSelection = value;
		}
	}

	public bool IsMovingSelection
	{
		get
		{
			return base.DragOverThreshold && m_movingSelection;
		}
		set
		{
			m_movingSelection = value;
		}
	}

	public event EventHandler<EventMovedEventArgs> EventMoved;

	public event EventHandler<HitEventArgs> Picking;

	public event EventHandler<HitEventArgs> MouseDownPicked;

	public event EventHandler<HitEventArgs> MouseMovePicked;

	public event EventHandler<BoundingRectEventArgs> BoundingRectUpdating;

	public TimelineControl(ITimelineDocument timelineDocument)
		: this(timelineDocument, new DefaultTimelineRenderer())
	{
	}

	public TimelineControl(ITimelineDocument timelineDocument, TimelineRenderer timelineRenderer)
		: this(timelineDocument, timelineRenderer, new DefaultTimelineConstraints())
	{
	}

	public TimelineControl(ITimelineDocument timelineDocument, TimelineRenderer timelineRenderer, TimelineConstraints timelineConstraints)
		: this(timelineDocument, timelineRenderer, timelineConstraints, createDefaultManipulators: true)
	{
	}

	public TimelineControl(ITimelineDocument timelineDocument, TimelineRenderer timelineRenderer, TimelineConstraints timelineConstraints, bool createDefaultManipulators)
	{
		SetStyle(ControlStyles.DoubleBuffer, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		SetZoomRange(0.1f, 50f, 10f, 100f);
		base.XZoom = 40f;
		base.YZoom = 40f;
		base.AllowDrop = timelineDocument != null;
		base.AutoScroll = true;
		m_timelineRenderer = timelineRenderer;
		timelineRenderer.Invalidated += timelineRenderer_Invalidated;
		m_timelineConstraints = timelineConstraints;
		TimelineDocument = timelineDocument;
		m_allowHeaderResize = true;
		GetSnapOffset = null;
		if (createDefaultManipulators)
		{
			new SelectionManipulator(this);
			new MoveManipulator(this);
			new ScaleManipulator(this);
			new SplitManipulator(this);
			SnapManipulator snapManipulator = new SnapManipulator(this);
			ScrubberManipulator scrubber = new ScrubberManipulator(this);
			snapManipulator.Scrubber = scrubber;
		}
	}

	public PointF GetDragOffset()
	{
		PointF result = GdiUtil.InverseTransformVector(Transform, base.DragDelta);
		result.X = ConstrainFrameOffset(result.X);
		return result;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			TimelineDocument = null;
			m_timelineRenderer.Invalidated -= timelineRenderer_Invalidated;
		}
		base.Dispose(disposing);
	}

	public TimelineLayout GetLayout()
	{
		if (m_layout != null)
		{
			return m_layout;
		}
		TimelineLayout layout;
		using (Graphics g = CreateGraphics())
		{
			layout = m_timelineRenderer.GetLayout(m_timeline, Transform, base.ClientRectangle, g);
		}
		return layout;
	}

	public RectangleF GetBoundingRect()
	{
		return GetBoundingRect(selectionOnly: false);
	}

	public RectangleF GetSelectionBoundingRect()
	{
		return GetBoundingRect(selectionOnly: true);
	}

	public void Frame()
	{
		Frame(GetBoundingRect());
	}

	public void FrameSelection()
	{
		Frame(GetSelectionBoundingRect());
	}

	public HitRecord Pick(Point clientPoint)
	{
		Matrix transform = Transform;
		Rectangle clientRectangle = base.ClientRectangle;
		RectangleF pickingRectangle = GetPickingRectangle(clientPoint);
		HitEventArgs e = new HitEventArgs(null, pickingRectangle, null);
		OnPicking(e);
		if (e.HitRecord != null)
		{
			return e.HitRecord;
		}
		IList<HitRecord> list;
		using (Graphics g = CreateGraphics())
		{
			list = m_timelineRenderer.Pick(m_timeline, pickingRectangle, transform, clientRectangle, g);
		}
		if (list.Count > 0)
		{
			HitRecord hitRecord = list[0];
			if (hitRecord.HitObject == null || m_selection.SelectionContains(hitRecord.HitPath))
			{
				return hitRecord;
			}
			Type type = hitRecord.HitObject.GetType();
			for (int i = 1; i < list.Count; i++)
			{
				HitRecord hitRecord2 = list[i];
				Type c = null;
				if (hitRecord2.HitObject != null)
				{
					c = hitRecord2.HitObject.GetType();
				}
				if (type.IsAssignableFrom(c) && m_selection.SelectionContains(hitRecord2.HitPath))
				{
					return hitRecord2;
				}
			}
			return hitRecord;
		}
		return new HitRecord(HitType.None, null);
	}

	public bool Select<T>(TimelinePath item) where T : class, ITimelineObject
	{
		Keys modifierKeys = Control.ModifierKeys;
		List<TimelinePath> list;
		if (KeysUtil.ClearsSelection(modifierKeys))
		{
			list = new List<TimelinePath>(1);
			list.Add(item);
		}
		else
		{
			list = new List<TimelinePath>();
			foreach (TimelinePath item2 in m_selection.GetSelection<TimelinePath>())
			{
				if (item2.Last is T)
				{
					list.Add(item2);
				}
			}
			if (KeysUtil.TogglesSelection(modifierKeys))
			{
				if (!list.Remove(item))
				{
					list.Add(item);
				}
			}
			else
			{
				list.Remove(item);
				list.Add(item);
			}
		}
		m_selection.SetRange(list);
		return m_selection.LastSelected != null;
	}

	public virtual float ConstrainFrameOffset(float offset)
	{
		return (float)Math.Round(offset);
	}

	public IGroup Create(IGroup original)
	{
		if (original is ITimelineObjectCreator timelineObjectCreator)
		{
			return (IGroup)timelineObjectCreator.Create();
		}
		return TimelineDocument.Timeline.CreateGroup();
	}

	public IMarker Create(IMarker original)
	{
		if (original is ITimelineObjectCreator timelineObjectCreator)
		{
			return (IMarker)timelineObjectCreator.Create();
		}
		return TimelineDocument.Timeline.CreateMarker();
	}

	public ITrack Create(ITrack original)
	{
		if (original is ITimelineObjectCreator timelineObjectCreator)
		{
			return (ITrack)timelineObjectCreator.Create();
		}
		return original.Group.CreateTrack();
	}

	public IInterval Create(IInterval original)
	{
		if (original is ITimelineObjectCreator timelineObjectCreator)
		{
			return (IInterval)timelineObjectCreator.Create();
		}
		return original.Track.CreateInterval();
	}

	public IKey Create(IKey original)
	{
		if (original is ITimelineObjectCreator timelineObjectCreator)
		{
			return (IKey)timelineObjectCreator.Create();
		}
		return original.Track.CreateKey();
	}

	public virtual bool IsEditable(TimelinePath path)
	{
		if (path == null)
		{
			return false;
		}
		ITimeline owningTimeline = GetOwningTimeline(path.Last);
		return owningTimeline == Timeline;
	}

	public virtual ITimeline GetOwningTimeline(ITimelineObject obj)
	{
		if (obj is IInterval)
		{
			return ((IInterval)obj).Track.Group.Timeline;
		}
		if (obj is ITimelineReference)
		{
			return ((ITimelineReference)obj).Parent;
		}
		if (obj is IKey)
		{
			return ((IKey)obj).Track.Group.Timeline;
		}
		if (obj is IMarker)
		{
			return ((IMarker)obj).Timeline;
		}
		if (obj is ITrack)
		{
			return ((ITrack)obj).Group.Timeline;
		}
		if (obj is IGroup)
		{
			return ((IGroup)obj).Timeline;
		}
		return null;
	}

	public static IEnumerable<T> GetObjectsInOneDocument<T>(ITimeline timeline) where T : ITimelineObject
	{
		if (typeof(T).IsAssignableFrom(typeof(IMarker)))
		{
			foreach (T marker in timeline.Markers)
			{
				yield return marker;
			}
		}
		foreach (IGroup group in timeline.Groups)
		{
			if (typeof(T).IsAssignableFrom(typeof(IGroup)))
			{
				yield return (T)group;
			}
			foreach (ITrack track in group.Tracks)
			{
				if (typeof(T).IsAssignableFrom(typeof(ITrack)))
				{
					yield return (T)track;
				}
				if (typeof(T).IsAssignableFrom(typeof(IInterval)))
				{
					foreach (IInterval interval in track.Intervals)
					{
						yield return (T)interval;
					}
				}
				if (!typeof(T).IsAssignableFrom(typeof(IKey)))
				{
					continue;
				}
				foreach (IKey key in track.Keys)
				{
					yield return (T)key;
				}
			}
		}
	}

	public static IEnumerable<TimelinePath> GetObjects<T>(ITimeline timeline) where T : ITimelineObject
	{
		foreach (T item3 in GetObjectsInOneDocument<T>(timeline))
		{
			ITimelineObject item = item3;
			yield return new TimelinePath(item);
		}
		foreach (TimelinePath path in GetHierarchy(timeline))
		{
			IHierarchicalTimeline hierarchical = ((ITimelineReference)path.Last).Target;
			if (hierarchical == null)
			{
				continue;
			}
			foreach (T item4 in GetObjectsInOneDocument<T>(hierarchical))
			{
				ITimelineObject item2 = item4;
				yield return new TimelinePath(path) + new TimelinePath(item2);
			}
		}
	}

	public static IEnumerable<TimelinePath> GetHierarchy(ITimeline timeline)
	{
		if (!(timeline is IHierarchicalTimeline root))
		{
			return EmptyEnumerable<TimelinePath>.Instance;
		}
		List<ITimelineObject> lineage = new List<ITimelineObject>();
		HashSet<IHierarchicalTimeline> all = new HashSet<IHierarchicalTimeline>();
		return GetHierarchy(root, lineage, all);
	}

	private static IEnumerable<TimelinePath> GetHierarchy(IHierarchicalTimeline root, List<ITimelineObject> lineage, HashSet<IHierarchicalTimeline> all)
	{
		all.Add(root);
		foreach (ITimelineReference reference in root.References)
		{
			lineage.Add(reference);
			yield return new TimelinePath(lineage);
			IHierarchicalTimeline child = reference.Target;
			if (child != null && !all.Contains(child))
			{
				foreach (TimelinePath item in GetHierarchy(child, lineage, all))
				{
					yield return item;
				}
			}
			lineage.RemoveAt(lineage.Count - 1);
		}
	}

	public static Matrix CalculateLocalToWorld(TimelinePath path)
	{
		Matrix matrix = new Matrix();
		for (int i = 0; i < path.Count; i++)
		{
			if (path[i] is ITimelineReference timelineReference)
			{
				Matrix matrix2 = new Matrix(1f, 0f, 0f, 1f, timelineReference.Start, 0f);
				matrix.Multiply(matrix2, MatrixOrder.Prepend);
			}
		}
		return matrix;
	}

	public static void CalculateRange(TimelinePath path, out float worldStart, out float worldEnd)
	{
		IEvent obj = (IEvent)path.Last;
		Matrix matrix = CalculateLocalToWorld(path);
		worldStart = GdiUtil.Transform(matrix, obj.Start);
		worldEnd = GdiUtil.Transform(matrix, obj.Start + obj.Length);
	}

	public static bool CalculateRange(IEnumerable<TimelinePath> objects, out float worldMin, out float worldMax)
	{
		worldMin = float.MaxValue;
		worldMax = float.MinValue;
		foreach (TimelinePath @object in objects)
		{
			if (@object.Last is IEvent)
			{
				CalculateRange(@object, out var worldStart, out var worldEnd);
				if (worldStart < worldMin)
				{
					worldMin = worldStart;
				}
				if (worldMax < worldEnd)
				{
					worldMax = worldEnd;
				}
			}
		}
		return worldMax > worldMin + float.Epsilon;
	}

	public void MoveEvent(EventMovedEventArgs e)
	{
		OnEventMoved(e);
	}

	public void CancelDrag()
	{
		m_movingSelection = false;
		Cursor = Cursors.Arrow;
		Invalidate();
	}

	protected virtual void OnEventMoved(EventMovedEventArgs e)
	{
		if (this.EventMoved != null)
		{
			this.EventMoved(this, e);
		}
	}

	protected virtual string GetEventTooltipText(IEvent _event)
	{
		return _event.ToString();
	}

	protected virtual string GetGroupTooltipText(IGroup _group)
	{
		return _group.ToString();
	}

	protected virtual string GetTrackTooltipText(ITrack _track)
	{
		return _track.ToString();
	}

	protected virtual void OnMouseDownPicked(HitEventArgs e)
	{
		if (this.MouseDownPicked != null)
		{
			Delegate[] invocationList = this.MouseDownPicked.GetInvocationList();
			int num = invocationList.Length;
			while (--num >= 0 && !e.Handled)
			{
				invocationList[num].DynamicInvoke(this, e);
			}
		}
	}

	protected virtual void OnMouseMovePicked(HitEventArgs e)
	{
		if (this.MouseMovePicked != null)
		{
			Delegate[] invocationList = this.MouseMovePicked.GetInvocationList();
			int num = invocationList.Length;
			while (--num >= 0 && !e.Handled)
			{
				invocationList[num].DynamicInvoke(this, e);
			}
		}
	}

	protected virtual void OnPicking(HitEventArgs e)
	{
		if (this.Picking != null)
		{
			Delegate[] invocationList = this.Picking.GetInvocationList();
			int num = invocationList.Length;
			while (--num >= 0 && !e.Handled)
			{
				invocationList[num].DynamicInvoke(this, e);
			}
		}
	}

	protected override Size GetCanvasSize()
	{
		RectangleF boundingRect = GetBoundingRect();
		Point scrollPosition = base.ScrollPosition;
		int num = (int)Math.Ceiling(boundingRect.Right) - scrollPosition.X + base.Width / 2;
		int num2 = (int)Math.Ceiling(boundingRect.Bottom) - scrollPosition.Y + base.Height / 2;
		return new Size(num, num2);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.HighQuality;
		graphics.SetClip(e.ClipRectangle);
		Matrix transform = Transform;
		Rectangle clientRectangle = base.ClientRectangle;
		if (m_timeline != null)
		{
			m_layout = m_timelineRenderer.Draw(m_timeline, m_selection, m_activeGroup, m_activeTrack, transform, clientRectangle, graphics);
		}
		base.OnPaint(e);
		if (m_timeline != null)
		{
			UpdateScrollBars(base.VerticalScrollBar, base.HorizontalScrollBar);
		}
		m_layout = null;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();
		HitRecord hitRecord = Pick(e.Location);
		Keys modifierKeys = Control.ModifierKeys;
		bool constrain = (modifierKeys & m_constrainModifierKeys) != 0;
		if ((e.Button == m_panButton || e.Button == MouseButtons.Middle) && (modifierKeys & m_panModifierKeys) != Keys.None)
		{
			base.IsScrolling = true;
			base.Constrain = constrain;
		}
		else if (e.Button == m_zoomButton && (modifierKeys & m_zoomModifierKeys) != Keys.None)
		{
			base.IsZooming = true;
			base.Constrain = constrain;
		}
		else if (e.Button == MouseButtons.Left)
		{
			ITimelineObject hitTimelineObject = hitRecord.HitTimelineObject;
			ITrack track = hitTimelineObject as ITrack;
			IGroup obj = hitTimelineObject as IGroup;
			IInterval interval = hitTimelineObject as IInterval;
			ITimelineReference timelineReference = hitTimelineObject as ITimelineReference;
			if (interval != null)
			{
				track = interval.Track;
			}
			if (hitRecord.HitObject is IKey key)
			{
				track = key.Track;
			}
			if (track != null)
			{
				obj = track.Group;
			}
			TimelinePath activeGroup = null;
			TimelinePath activeTrack = null;
			switch (hitRecord.Type)
			{
			case HitType.GroupExpand:
				if (obj != null)
				{
					ToggleGroupExpansion(obj);
				}
				else if (timelineReference != null)
				{
					ToggleReferenceExpansion(timelineReference);
				}
				Invalidate();
				break;
			case HitType.HeaderResize:
				if (m_allowHeaderResize)
				{
					m_resizingHeader = true;
					activeGroup = m_activeGroup;
					activeTrack = m_activeTrack;
					base.AutoScroll = false;
				}
				break;
			case HitType.Track:
			case HitType.Group:
			case HitType.None:
				if (base.ClientRectangle.Contains(e.X, e.Y))
				{
					base.IsMultiSelecting = true;
				}
				break;
			}
			m_activeGroup = activeGroup;
			m_activeTrack = activeTrack;
		}
		RectangleF pickingRectangle = GetPickingRectangle(e.Location);
		OnMouseDownPicked(new HitEventArgs(hitRecord, pickingRectangle, e));
	}

	private void InvalidateControl()
	{
		if (base.InvokeRequired)
		{
			BeginInvoke(new Action(InvalidateControl));
		}
		else
		{
			Invalidate();
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		User32.TRACKMOUSEEVENT lpEventTrack = new User32.TRACKMOUSEEVENT(base.Handle);
		User32.TrackMouseEvent(ref lpEventTrack);
		HitRecord hitRecord = Pick(e.Location);
		if (e.Button == MouseButtons.None)
		{
			switch (hitRecord.Type)
			{
			case HitType.GroupMove:
			case HitType.TrackMove:
				Cursor = Cursors.SizeAll;
				break;
			case HitType.HeaderResize:
				if (m_allowHeaderResize)
				{
					Cursor = Cursors.VSplit;
				}
				else
				{
					Cursor = Cursors.Default;
				}
				break;
			default:
				Cursor = Cursors.Default;
				break;
			}
		}
		else
		{
			if (m_resizingHeader)
			{
				m_timelineRenderer.HeaderWidth = Math.Max(e.X, 32);
			}
			if (!base.IsMultiSelecting)
			{
				Invalidate();
			}
		}
		RectangleF pickingRectangle = GetPickingRectangle(e.Location);
		OnMouseMovePicked(new HitEventArgs(hitRecord, pickingRectangle, e));
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (base.IsMultiSelecting)
		{
			Keys modifierKeys = Control.ModifierKeys;
			List<TimelinePath> list = ((!KeysUtil.ClearsSelection(modifierKeys)) ? new List<TimelinePath>(m_selection.GetSelection<TimelinePath>()) : new List<TimelinePath>());
			using (Graphics g = CreateGraphics())
			{
				IList<HitRecord> hits = m_timelineRenderer.Pick(m_timeline, base.SelectionRect, Transform, base.ClientRectangle, g);
				KeysUtil.Select(list, HitRecordsToEvents(hits), modifierKeys);
			}
			m_selection.Selection = list.Cast<object>();
			Invalidate();
		}
		Cursor = Cursors.Arrow;
		m_resizingHeader = false;
		base.AutoScroll = true;
		base.OnMouseUp(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		Invalidate();
		base.OnMouseWheel(e);
	}

	private void ToggleGroupExpansion(IGroup group)
	{
		bool expanded = group.Expanded;
		string transactionName = (expanded ? "Collapse Group" : "Expand Group");
		TransactionContext.DoTransaction(delegate
		{
			group.Expanded = !expanded;
		}, transactionName);
	}

	private void ToggleReferenceExpansion(ITimelineReference reference)
	{
		bool expanded = reference.Options.Expanded;
		string transactionName = (expanded ? "Collapse Reference" : "Expand Reference");
		TransactionContext.DoTransaction(delegate
		{
			reference.Options.Expanded = !expanded;
		}, transactionName);
	}

	private RectangleF GetBoundingRect(bool selectionOnly)
	{
		TimelineLayout layout = GetLayout();
		RectangleF rectangleF = default(RectangleF);
		IEnumerator<KeyValuePair<TimelinePath, RectangleF>> enumerator = layout.GetEnumerator();
		if (enumerator.MoveNext())
		{
			rectangleF = enumerator.Current.Value;
		}
		while (enumerator.MoveNext())
		{
			KeyValuePair<TimelinePath, RectangleF> current = enumerator.Current;
			if (!selectionOnly || m_selection.SelectionContains(current.Key))
			{
				if (current.Key.Last is IInterval || current.Key.Last is IKey)
				{
					rectangleF = RectangleF.Union(rectangleF, current.Value);
				}
				else if (current.Key.Last is IGroup || current.Key.Last is ITrack || current.Key.Last is ITimelineReference)
				{
					RectangleF value = current.Value;
					rectangleF.Y = Math.Min(rectangleF.Y, value.Y);
					float num = Math.Max(rectangleF.Bottom, value.Bottom);
					rectangleF.Height = num - rectangleF.Y;
				}
				else if (current.Key.Last is IMarker)
				{
					RectangleF value2 = current.Value;
					rectangleF.X = Math.Min(rectangleF.X, value2.X);
					float num2 = Math.Max(rectangleF.Right, value2.Right);
					rectangleF.Width = num2 - rectangleF.Left;
				}
			}
		}
		BoundingRectEventArgs e = new BoundingRectEventArgs(rectangleF);
		if (this.BoundingRectUpdating != null)
		{
			Delegate[] invocationList = this.BoundingRectUpdating.GetInvocationList();
			foreach (Delegate obj in invocationList)
			{
				obj.DynamicInvoke(this, e);
				rectangleF = RectangleF.Union(rectangleF, e.NewClientRect);
			}
		}
		return rectangleF;
	}

	private RectangleF GetPickingRectangle(Point clientPoint)
	{
		int pickTolerance = m_timelineRenderer.PickTolerance;
		return new RectangleF(clientPoint.X - pickTolerance, clientPoint.Y - pickTolerance, 2 * pickTolerance, 2 * pickTolerance);
	}

	private string GetToolTipText(object hitObject)
	{
		if (hitObject is IEvent)
		{
			return GetEventTooltipText((IEvent)hitObject);
		}
		if (hitObject is IGroup)
		{
			return GetGroupTooltipText((IGroup)hitObject);
		}
		if (hitObject is ITrack)
		{
			return GetTrackTooltipText((ITrack)hitObject);
		}
		string text = hitObject.ToString();
		if (text == hitObject.GetType().ToString())
		{
			return null;
		}
		return text;
	}

	private float NoSnapOffset(IEnumerable<float> movingPoints, SnapOptions options)
	{
		return 0f;
	}

	private float GetSliderPosition(Point inOffsetPosition)
	{
		float offsetX = Renderer.OffsetX;
		Point point = inOffsetPosition;
		point.X -= (int)offsetX;
		PointF pointF = GdiUtil.InverseTransformVector(Transform, point);
		if (pointF.X < 0f)
		{
			pointF.X = 0f;
		}
		return (float)Math.Round(pointF.X);
	}

	private IEnumerable<TimelinePath> HitRecordsToEvents(IEnumerable<HitRecord> hits)
	{
		foreach (HitRecord hit in hits)
		{
			if (hit.HitTimelineObject is IEvent)
			{
				yield return hit.HitPath;
			}
		}
	}

	private void timelineDocument_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		if (e.Item is IEvent obj)
		{
			obj.Start = ConstrainFrameOffset(obj.Start);
			obj.Length = ConstrainFrameOffset(obj.Length);
			if (obj is IInterval { Track: not null } interval)
			{
				foreach (IInterval interval2 in interval.Track.Intervals)
				{
					if (interval2 != interval)
					{
						float start = interval.Start;
						float length = interval.Length;
						if (!m_timelineConstraints.IsIntervalValid(interval, ref start, ref length, interval2))
						{
							throw new InvalidTransactionException("Intervals must not overlap");
						}
						if (interval.Start != start)
						{
							interval.Start = start;
						}
						if (interval.Length != length)
						{
							interval.Length = length;
						}
					}
				}
			}
		}
		Invalidate();
	}

	private void timelineDocument_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		if (m_activeGroup != null && m_activeGroup.Last == e.Item)
		{
			m_activeGroup = null;
		}
		else if (m_activeTrack != null && m_activeTrack.Last == e.Item)
		{
			m_activeTrack = null;
		}
		Invalidate();
	}

	private void timelineDocument_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		Invalidate();
	}

	private void timelineDocument_Reloaded(object sender, EventArgs e)
	{
		m_timeline = m_timelineDocument.Timeline;
		Invalidate();
	}

	private void selection_SelectionChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void timelineRenderer_Invalidated(object sender, EventArgs e)
	{
		Invalidate();
	}
}
