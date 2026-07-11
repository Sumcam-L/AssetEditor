using System;
using System.Collections.Generic;
using System.Drawing;
using Firaxis.Asset.Properties;
using Firaxis.Asset.Trigger;
using Firaxis.Collections;
using Firaxis.Controls;
using Firaxis.Controls.Scrollables;
using Firaxis.MathEx;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class ScrollableItemTriggerTrack : ScrollableItemTrackBase, IScrollableItemTrack, IScrollableItemKeys
{
	public class TriggerKey : IKey, IEquatable<IKey>
	{
		public float Time
		{
			get
			{
				return Trigger.Time;
			}
			set
			{
				Trigger.Time = value;
			}
		}

		public ScrollableItemTriggerTrack Track { get; set; }

		public float Value { get; set; }

		public ITrigger Trigger { get; private set; }

		public TriggerKey(ScrollableItemTriggerTrack track, ITrigger trigger)
		{
			Trigger = trigger;
			Track = track;
		}

		public bool Equals(IKey other)
		{
			if (!(other is TriggerKey triggerKey))
			{
				return false;
			}
			if (triggerKey.Trigger.ID != Trigger.ID)
			{
				return false;
			}
			if (triggerKey.Track.ID != Track.ID)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return Trigger.ID.GetHashCode() ^ Track.ID.GetHashCode();
		}
	}

	private ITriggerSystem trigsys;

	private ScrollableItemTriggerAnimation parent;

	public TriggerTrack.SubTrack Track { get; private set; }

	public int ID => Track.ID;

	public string EventName { get; private set; }

	public ListEvent<IKey> Keys { get; private set; }

	public float MarkTime { get; set; }

	public bool ShowMarkTime { get; set; }

	public ScrollableItemTriggerAnimation Animation { get; private set; }

	public ScrollableItemTriggerTrack(Font font, TriggerTrack.SubTrack track, ScrollableItemTriggerAnimation parent, ITriggerSystem trigsys)
		: base(font)
	{
		this.trigsys = trigsys;
		this.parent = parent;
		Animation = parent;
		Track = track;
		Text = track.Name;
		Keys = new ListEvent<IKey>();
		EventName = parent.TimelineID;
		BuildKeys();
	}

	public IKey AddKey(ITrigger trigger)
	{
		if (trigger is ITriggerTrackInfo triggerTrackInfo && triggerTrackInfo.TrackID == EventName && triggerTrackInfo.SubTrackID == ID)
		{
			IKey key = new TriggerKey(this, trigger);
			Keys.Add(key);
			return key;
		}
		return null;
	}

	private void BuildKeys()
	{
		Keys.Clear();
		foreach (ITrigger trigger in trigsys.Triggers)
		{
			AddKey(trigger);
		}
	}

	public void PaintTrack(object sender, ScrollableItemPaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		TimeLineControl timeLineControl = (TimeLineControl)sender;
		using StringFormat stringFormat = new StringFormat();
		stringFormat.LineAlignment = StringAlignment.Center;
		int num = e.Bounds.Y + ItemHeight / 2;
		int num2 = 0;
		float duration = parent.Duration;
		if (duration > float.Epsilon && duration < 1000f)
		{
			num2 = timeLineControl.TimeRuler.TimeToX(duration);
			using Pen pen = new Pen(base.DurationEndColor);
			graphics.DrawLine(pen, num2, e.Bounds.Y, num2, e.Bounds.Bottom);
		}
		foreach (IKey key in Keys)
		{
			TriggerKey triggerKey = (TriggerKey)key;
			num2 = timeLineControl.TimeRuler.TimeToX(key.Time);
			Color color = ReflectionHelper.GetColor(((TriggerKey)key).Trigger).Desaturate(timeLineControl.AllowEdit ? 1f : 0f);
			if (timeLineControl.TimeTrack.SelectedKeys.Contains(key))
			{
				DrawingHelper.DrawImageCentered(graphics, Resources.time_keysel, num2, num);
				if (e.Interacting)
				{
					int num3 = num2 + Resources.time_keysel.Width / 2;
					string text = TimeCode.ToString(key.Time, TimeCodeFormat.Frame);
					SizeF sizeF = graphics.MeasureString(text, base.Font, new PointF(num3, num), stringFormat);
					using (Brush brush = new SolidBrush(base.ChangingBackColor))
					{
						graphics.FillRectangle(brush, num3, (float)num - sizeF.Height / 2f, sizeF.Width, sizeF.Height);
					}
					using Brush brush2 = new SolidBrush(base.ChangingTextColor);
					graphics.DrawString(text, base.Font, brush2, num3, num, stringFormat);
				}
			}
			if (triggerKey.Trigger.Duration < 0f || triggerKey.Trigger.Duration > float.Epsilon)
			{
				using Pen pen2 = new Pen(color);
				int num4 = 0;
				num4 = ((!(triggerKey.Trigger.Duration > float.Epsilon)) ? ((int)(timeLineControl.TimeRuler.MajorScale * timeLineControl.TimeRuler.MaxTime)) : ((int)(timeLineControl.TimeRuler.MajorScale * triggerKey.Trigger.Duration)));
				graphics.DrawLine(pen2, num2, num, num2 + num4, num);
			}
			DrawingHelper.DrawImageCentered(graphics, Resources.time_key0, num2, num, color);
		}
		if (ShowMarkTime)
		{
			num2 = timeLineControl.TimeRuler.TimeToX(MarkTime);
			using Pen pen3 = new Pen(base.MarkColor);
			graphics.DrawLine(pen3, num2, e.Bounds.Y, num2, e.Bounds.Bottom);
			return;
		}
	}

	public IEnumerable<IKey> FindKeys(int x, int y, TimeLineHitArgs e)
	{
		List<IKey> list = new List<IKey>();
		TimeRulerControl timeRuler = e.TimeLine.TimeRuler;
		foreach (IKey key in Keys)
		{
			int num = timeRuler.TimeToX(key.Time);
			if (Math.Abs(num - x) < 8)
			{
				list.Add(key);
			}
		}
		return list;
	}

	public void MoveKey(object sender, IKey key, int delta)
	{
		TimeLineControl timeLineControl = (TimeLineControl)sender;
		float val = key.Time + (float)delta / timeLineControl.TimeRuler.MajorScale;
		key.Time = Math.Max(0f, val);
	}

	public IKey AddKey(object sender, int x, int y)
	{
		TimeLineControl timeLineControl = (TimeLineControl)sender;
		float num = Math.Max(0f, timeLineControl.TimeRuler.XToTime(x));
		return null;
	}

	public void RemoveKey(object sender, IKey key)
	{
	}
}
