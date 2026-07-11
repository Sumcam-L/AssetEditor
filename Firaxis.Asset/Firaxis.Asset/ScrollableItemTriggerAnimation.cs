using System;
using System.ComponentModel;
using System.Drawing;
using Firaxis.Asset.Properties;
using Firaxis.Asset.Trigger;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Controls;
using Firaxis.Controls.Scrollables;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class ScrollableItemTriggerAnimation : ScrollableItemAnimationBase, IScrollableItemTrack
{
	private IServiceProvider serviceProvider;

	private StateTransitionInfo _stateTransition;

	[Description("The duration of the timeline (this value cannot be modified if the timeline is bound to an animation)")]
	public float Duration
	{
		get
		{
			return StateTransition.Duration;
		}
		set
		{
			if (StateTransition.IsReadOnly || !(value > 0f) || serviceProvider == null)
			{
				return;
			}
			StateTransition.Duration = value;
			IStateTransitionNameProvider stateTransitionNameProvider = (IStateTransitionNameProvider)serviceProvider.GetService(typeof(IStateTransitionNameProvider));
			if (stateTransitionNameProvider == null || !stateTransitionNameProvider.TimelineStateTransitions.TryGetValue(TimelineName, out var value2))
			{
				return;
			}
			foreach (StateTransitionInfo item in value2)
			{
				item.Duration = value;
			}
		}
	}

	private bool ActiveAnimation => false;

	[Browsable(false)]
	public TriggerTrack Track { get; private set; }

	[Browsable(false)]
	public string TimelineID
	{
		get
		{
			return Track.ID;
		}
		private set
		{
			Track.ID = value;
			UpdateText();
		}
	}

	[Browsable(false)]
	public string AnimationID
	{
		get
		{
			return Track.Binding;
		}
		set
		{
			Track.Binding = value;
			UpdateText();
		}
	}

	[Category("Behavior")]
	[Description("Name of the timeline")]
	public string TimelineName => TimelineID;

	[Category("Behavior")]
	[Description("State transition to apply for animation preview")]
	[TypeConverter(typeof(StateTransitionNameTypeConverter))]
	public StateTransitionInfo StateTransition
	{
		get
		{
			return _stateTransition;
		}
		set
		{
			_stateTransition = value;
		}
	}

	[Category("Behavior")]
	[Description("Indicates whether the timeline is bound to an animation")]
	public bool BoundToAnimation => StateTransition.IsReadOnly;

	public ScrollableItemTriggerAnimation(Font font, Image image, TriggerTrack trk)
		: base(font, image)
	{
		Track = trk;
		base.DurationEndColor = Color.LightGray;
		base.ActiveFillColor = Color.FromArgb(96, Color.Orange);
		base.InactiveFillColor = Color.FromArgb(128, Color.Gray);
		base.ActiveTextColor = Color.Black;
		base.InactiveTextColor = Color.LightGray;
		StateTransition = new StateTransitionInfo();
		UpdateText();
	}

	public ScrollableItemTriggerAnimation(IServiceProvider serviceProvider, Font font, Image image, TriggerTrack trk)
		: this(font, image, trk)
	{
		this.serviceProvider = serviceProvider;
	}

	private void UpdateText()
	{
		if (string.IsNullOrEmpty(Track.Binding))
		{
			Text = Track.ID;
		}
		else
		{
			Text = $"{Track.ID} ({Track.Binding})";
		}
	}

	public override void PaintItem(object sender, ScrollableItemPaintEventArgs e)
	{
		base.PaintItem(sender, e);
		if (ActiveAnimation)
		{
			DrawingHelper.DrawImageCentered(e.Graphics, Resources.flagged, e.Bounds.Right - 10, e.Bounds.Y + e.Bounds.Height / 2);
		}
	}

	public virtual void PaintTrack(object sender, ScrollableItemPaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		TimeLineControl timeLineControl = (TimeLineControl)sender;
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
		bool activeAnimation = ActiveAnimation;
		Color color = (activeAnimation ? base.ActiveFillColor : base.InactiveFillColor);
		Color color2 = (activeAnimation ? base.ActiveTextColor : base.InactiveTextColor);
		using (Brush brush = new SolidBrush(color))
		{
			graphics.FillRectangle(brush, bounds);
		}
		using (StringFormat stringFormat = new StringFormat())
		{
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Center;
			using Brush brush2 = new SolidBrush(color2);
			graphics.DrawString(TimeCode.ToString(duration, TimeCodeFormat.Seconds), base.Font, brush2, bounds, stringFormat);
		}
		if (base.Visible)
		{
			using (Pen pen = new Pen(base.DurationEndColor))
			{
				graphics.DrawLine(pen, num2, e.Bounds.Y, num2, e.Bounds.Bottom);
			}
		}
	}
}
