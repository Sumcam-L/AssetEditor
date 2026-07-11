using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Firaxis.Asset;
using Firaxis.Asset.Properties;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Controls;
using Firaxis.Controls.Scrollables;
using Firaxis.MathEx;
using Firaxis.Reflection;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ScrollableItemTrack : ScrollableItemTrackBase, IScrollableItemTrack, IScrollableItemKeys, ITrackAdapterTreeItem, IDomNodeTreeItem
{
	private class TriggerAdapterKey : ITriggerAdapterKey, IKey, IEquatable<IKey>
	{
		public virtual TriggerAdapter Adapter { get; private set; }

		public float Time
		{
			get
			{
				return Adapter.StartTime;
			}
			set
			{
				if (Adapter.StartTime != value)
				{
					Adapter.StartTime = value;
				}
			}
		}

		public float Value { get; set; }

		public TriggerAdapterKey(TriggerAdapter adapter)
		{
			Adapter = adapter;
		}

		public virtual bool Equals(IKey other)
		{
			if (!(other is TriggerAdapterKey triggerAdapterKey))
			{
				return false;
			}
			if (triggerAdapterKey.Adapter.DomNode.Equals(Adapter.DomNode))
			{
				return triggerAdapterKey.Adapter.Name == Adapter.Name;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Adapter.Name.GetHashCode();
		}
	}

	private TimelineAdapter m_timeline;

	private TrackAdapter m_track;

	private ISelectionContext m_selectionContext;

	private static IDictionary<TriggerType, Color> s_triggerColors;

	public float MarkTime { get; set; }

	public bool ShowMarkTime { get; set; }

	private IEnumerable<TriggerAdapter> Keys => m_timeline.Triggers.Where((TriggerAdapter trg) => trg.TrackIndex == m_track.Index && trg.TriggerType == m_track.TriggerType);

	[Browsable(false)]
	public override string Text
	{
		get
		{
			return m_track.Name;
		}
		set
		{
			BugSubmitter.SilentReport("Why are we setting a readonly value??? @assign bwhitman");
		}
	}

	public DomNode DomNode => m_track.DomNode;

	public TrackAdapter Adapter => m_track;

	static ScrollableItemTrack()
	{
		s_triggerColors = new Dictionary<TriggerType, Color>();
		s_triggerColors[TriggerType.TT_ASSET_VFX] = Color.FromArgb(0, 0, 255);
		s_triggerColors[TriggerType.TT_ARTDEF_VFX] = Color.FromArgb(64, 128, 128);
		s_triggerColors[TriggerType.TT_LIGHT] = Color.FromArgb(255, 255, 0);
		s_triggerColors[TriggerType.TT_SOUND] = Color.FromArgb(32, 255, 0);
		s_triggerColors[TriggerType.TT_TRANSFER] = Color.FromArgb(255, 0, 0);
		s_triggerColors[TriggerType.TT_ACTION] = Color.FromArgb(255, 128, 0);
	}

	public ScrollableItemTrack(ISelectionContext selectionContext, TimelineAdapter timeline, TrackAdapter track, Font font, Image img)
		: base(font, img)
	{
		m_timeline = timeline;
		m_selectionContext = selectionContext;
		m_track = track;
	}

	private bool IsTrackSelected()
	{
		return m_selectionContext.SelectionContains(Adapter);
	}

	private bool AreTrackTriggersSelected()
	{
		return (from selObj in m_selectionContext.Selection
			where selObj.Is<TriggerAdapter>()
			select selObj.As<TriggerAdapter>()).Intersect(Adapter.Triggers).Any();
	}

	public IEnumerable<IKey> FindKeys(int x, int y, TimeLineHitArgs e)
	{
		TimeRulerControl timeRuler = e.TimeLine.TimeRuler;
		foreach (TriggerAdapter key in Keys)
		{
			if (Math.Abs(timeRuler.TimeToX(key.StartTime) - x) < 8)
			{
				yield return new TriggerAdapterKey(key);
			}
		}
	}

	public void MoveKey(object sender, IKey key, int delta)
	{
		TimeLineControl timeLineControl = (TimeLineControl)sender;
		float val = key.Time + (float)delta / timeLineControl.TimeRuler.MajorScale;
		key.Time = Math.Max(0f, val);
	}

	public IKey AddKey(object sender, int x, int y)
	{
		BugSubmitter.SilentReport("How do keys get added? What renders them?? @assign bwhitman");
		return null;
	}

	public void RemoveKey(object sender, IKey key)
	{
		BugSubmitter.SilentReport("How do keys get removed? @assign bwhitman");
	}

	public void PaintTrack(object sender, ScrollableItemPaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		TimeLineControl timeLineControl = (TimeLineControl)sender;
		using StringFormat stringFormat = new StringFormat();
		stringFormat.LineAlignment = StringAlignment.Center;
		int num = e.Bounds.Y + ItemHeight / 2;
		int num2 = 0;
		float duration = m_timeline.Duration;
		if (duration > float.Epsilon && duration < 1000f)
		{
			num2 = timeLineControl.TimeRuler.TimeToX(duration);
			using Pen pen = new Pen(base.DurationEndColor);
			graphics.DrawLine(pen, num2, e.Bounds.Y, num2, e.Bounds.Bottom);
		}
		if (IsTrackSelected() || AreTrackTriggersSelected())
		{
			using Brush brush = new SolidBrush(base.HighlightedBackColor);
			graphics.FillRectangle(brush, e.Bounds);
		}
		foreach (TriggerAdapter key in Keys)
		{
			num2 = timeLineControl.TimeRuler.TimeToX(key.StartTime);
			Color color = (key.As<IColorProvider>()?.Color ?? s_triggerColors[key.TriggerType]).Desaturate(timeLineControl.AllowEdit ? 1f : 0f);
			if (m_selectionContext.SelectionContains(key))
			{
				DrawingHelper.DrawImageCentered(graphics, Firaxis.Asset.Properties.Resources.time_keysel, num2, num);
				if (e.Interacting)
				{
					int num3 = num2 + Firaxis.Asset.Properties.Resources.time_keysel.Width / 2;
					string text = TimeCode.ToString(key.StartTime, TimeCodeFormat.Frame);
					SizeF sizeF = graphics.MeasureString(text, base.Font, new PointF(num3, num), stringFormat);
					using (Brush brush2 = new SolidBrush(base.ChangingBackColor))
					{
						graphics.FillRectangle(brush2, num3, (float)num - sizeF.Height / 2f, sizeF.Width, sizeF.Height);
					}
					using Brush brush3 = new SolidBrush(base.ChangingTextColor);
					graphics.DrawString(text, base.Font, brush3, num3, num, stringFormat);
				}
			}
			IDurableTriggerAdapter durableTriggerAdapter = key.As<IDurableTriggerAdapter>();
			if (durableTriggerAdapter != null)
			{
				float duration2 = durableTriggerAdapter.Duration;
				if (duration2 < 0f || duration2 > float.Epsilon)
				{
					using Pen pen2 = new Pen(color);
					int num4 = ((!(durableTriggerAdapter.Duration > float.Epsilon)) ? ((int)(timeLineControl.TimeRuler.MajorScale * timeLineControl.TimeRuler.MaxTime)) : ((int)(timeLineControl.TimeRuler.MajorScale * duration2)));
					graphics.DrawLine(pen2, num2, num, num2 + num4, num);
				}
			}
			DrawingHelper.DrawImageCentered(graphics, Firaxis.Asset.Properties.Resources.time_key0, num2, num, color);
		}
		if (ShowMarkTime)
		{
			num2 = timeLineControl.TimeRuler.TimeToX(MarkTime);
			using Pen pen3 = new Pen(base.MarkColor);
			graphics.DrawLine(pen3, num2, e.Bounds.Y, num2, e.Bounds.Bottom);
			return;
		}
	}
}
