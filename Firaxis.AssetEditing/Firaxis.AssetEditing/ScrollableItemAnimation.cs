using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Firaxis.Asset;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Controls;
using Firaxis.Controls.Scrollables;
using Firaxis.Utility;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ScrollableItemAnimation : ScrollableItemAnimationBase, IScrollableItemTrack, ITimelineBoundItem, IDomNodeTreeItem
{
	public virtual DomNode DomNode => AnimationBinding?.DomNode;

	private ISelectionContext SelectionContext { get; set; }

	public TimelineAdapter Timeline { get; private set; }

	private AnimationBindingAdapter AnimationBinding { get; set; }

	private IAnimationKnobService AnimationKnobService { get; set; }

	[Browsable(false)]
	public override string Text
	{
		get
		{
			return Timeline.Name + "(" + AnimationBinding.AnimationName + ")";
		}
		set
		{
			BugSubmitter.SilentReport("Why are we setting a readonly value??? @assign bwhitman");
		}
	}

	private float Duration
	{
		get
		{
			if (!AnimationKnobService.TimelineStateTransitions.ContainsKey(Timeline.Name))
			{
				return 0f;
			}
			IList<StateTransitionInfo> list = AnimationKnobService.TimelineStateTransitions[Timeline.Name];
			if (list.Count == 0)
			{
				return 0f;
			}
			return list[0].Duration;
		}
	}

	public ScrollableItemAnimation(ISelectionContext selection, IAnimationKnobService aks, TimelineAdapter timeline, AnimationBindingAdapter anim, Font font, Image image)
		: base(font, image)
	{
		SelectionContext = selection;
		AnimationKnobService = aks;
		Timeline = timeline;
		AnimationBinding = anim;
	}

	private bool IsAnimationSelected()
	{
		return SelectionContext.SelectionContains(AnimationBinding);
	}

	public virtual void PaintTrack(object sender, ScrollableItemPaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		TimeLineControl timeLineControl = (TimeLineControl)sender;
		if (IsAnimationSelected())
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
