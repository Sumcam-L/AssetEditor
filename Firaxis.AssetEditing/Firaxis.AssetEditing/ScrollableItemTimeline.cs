using System.ComponentModel;
using System.Drawing;
using Firaxis.Asset;
using Firaxis.CivTech;
using Firaxis.Controls;
using Firaxis.Controls.Scrollables;
using Firaxis.Utility;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ScrollableItemTimeline : ScrollableItemAnimationBase, IScrollableItemTrack, ITimelineBoundItem, IDomNodeTreeItem
{
	public virtual DomNode DomNode => TimelineBinding?.DomNode;

	private ISelectionContext SelectionContext { get; set; }

	public TimelineAdapter Timeline { get; private set; }

	private TimelineBindingAdapter TimelineBinding { get; set; }

	[Browsable(false)]
	public override string Text
	{
		get
		{
			return Timeline.Name;
		}
		set
		{
			BugSubmitter.SilentReport("Why are we setting a readonly value??? @assign bwhitman");
		}
	}

	private float Duration => Timeline.Duration;

	public ScrollableItemTimeline(ISelectionContext selection, TimelineAdapter timeline, TimelineBindingAdapter binding, Font font, Image image)
		: base(font, image)
	{
		SelectionContext = selection;
		Timeline = timeline;
		TimelineBinding = binding;
	}

	private bool IsTimelineSelected()
	{
		return SelectionContext.SelectionContains(TimelineBinding);
	}

	public override void PaintItem(object sender, ScrollableItemPaintEventArgs e)
	{
		base.PaintItem(sender, e);
	}

	public virtual void PaintTrack(object sender, ScrollableItemPaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		TimeLineControl timeLineControl = (TimeLineControl)sender;
		if (IsTimelineSelected())
		{
			using Brush brush = new SolidBrush(base.HighlightedBackColor);
			graphics.FillRectangle(brush, e.Bounds);
		}
		float duration = Duration;
		if (!(duration > float.Epsilon) || !(duration < 1000f))
		{
			return;
		}
		int num = timeLineControl.TimeRuler.TimeToX(0f);
		int num2 = timeLineControl.TimeRuler.TimeToX(duration);
		Rectangle bounds = e.Bounds;
		bounds.X = num;
		bounds.Width = num2 - num;
		bounds.Inflate(0, -3);
		Color color = (false ? base.ActiveFillColor : base.InactiveFillColor);
		Color color2 = (false ? base.ActiveTextColor : base.InactiveTextColor);
		using (Brush brush2 = new SolidBrush(color))
		{
			graphics.FillRectangle(brush2, bounds);
		}
		using (StringFormat stringFormat = new StringFormat())
		{
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Center;
			using Brush brush3 = new SolidBrush(color2);
			graphics.DrawString(TimeCode.ToString(duration, TimeCodeFormat.Seconds), base.Font, brush3, bounds, stringFormat);
		}
		if (!base.Visible)
		{
			return;
		}
		using Pen pen = new Pen(base.DurationEndColor);
		graphics.DrawLine(pen, num2, e.Bounds.Y, num2, e.Bounds.Bottom);
	}
}
