using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Utility;

namespace Firaxis.Controls;

public class TimeRulerControl : UserControl
{
	public enum HitLocation
	{
		Nothing,
		Shuttle,
		RangeBegin,
		RangeEnd,
		Range
	}

	private enum DragMode
	{
		Idle,
		Pan,
		Shuttle,
		RangeLeft,
		RangeRight
	}

	public class TimeRulerEventArgs : EventArgs
	{
		public float Time { get; private set; }

		public TimeRulerEventArgs(float time)
		{
			Time = time;
		}
	}

	public delegate void TimeRuleEventHandler(object sender, TimeRulerEventArgs e);

	public const int PadX = 10;

	public const int PadY = 1;

	private DragMode drag;

	private Point drag_pt;

	private float current_time;

	private float major_scale;

	private float origin;

	private float range_begin;

	private float range_duration;

	private float max_time;

	private bool trackRangeVisible;

	private IAsyncResult m_inFlightInvoke = null;

	private IContainer components = null;

	public Color TickColor { get; set; }

	public Color ShuttleColor { get; set; }

	public Color RangeColor { get; set; }

	public bool ShuttleVisible { get; set; }

	public float RangeStart
	{
		get
		{
			return range_begin;
		}
		set
		{
			range_begin = Math.Min(Math.Max(0f, value), 1000f);
			OnRangeChanged(EventArgs.Empty);
			SafeRefresh();
		}
	}

	public float RangeDuration
	{
		get
		{
			return range_duration;
		}
		set
		{
			range_duration = Math.Min(Math.Max(0f, value), 1000f);
			OnRangeChanged(EventArgs.Empty);
			SafeRefresh();
		}
	}

	public bool TrackRangeVisible
	{
		get
		{
			return trackRangeVisible;
		}
		set
		{
			trackRangeVisible = value;
			SafeRefresh();
		}
	}

	public float CurrentTime
	{
		get
		{
			return current_time;
		}
		set
		{
			current_time = value;
			ThrottledRefresh();
			this.CurrentTimeChanged?.Invoke(this, new TimeRulerEventArgs(current_time));
		}
	}

	public float MajorScale
	{
		get
		{
			return major_scale;
		}
		set
		{
			if (!(value < 2f))
			{
				major_scale = value;
				this.ScaleChanged?.Invoke(this, EventArgs.Empty);
				SafeRefresh();
			}
		}
	}

	public float Origin
	{
		get
		{
			return origin;
		}
		set
		{
			origin = value;
			SafeRefresh();
			this.OriginChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public float MaxTime
	{
		get
		{
			return max_time;
		}
		set
		{
			max_time = value;
			SafeRefresh();
			this.MaxTimeChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public float TimeSpan => (float)base.ClientSize.Width / MajorScale;

	public event TimeRuleEventHandler CurrentTimeChanged;

	public event EventHandler OriginChanged;

	public event EventHandler MaxTimeChanged;

	public event EventHandler ScaleChanged;

	public event EventHandler RangeChanged;

	public event PaintEventHandler PreShuttlePaint;

	public event TimeRuleEventHandler UserTimeChanged;

	public TimeRulerControl()
	{
		InitializeComponent();
		ShuttleVisible = true;
		drag = DragMode.Idle;
		origin = 0f;
		max_time = 3600f;
		current_time = 0f;
		major_scale = 540f;
		range_begin = 0f;
		range_duration = 3f;
		TickColor = Color.FromArgb(186, 182, 169);
		ShuttleColor = Color.FromArgb(216, 30, 0);
		RangeColor = Color.FromArgb(245, 164, 83);
	}

	private void SafeRefresh()
	{
		if (base.IsHandleCreated && base.InvokeRequired)
		{
			BeginInvoke((Action)delegate
			{
				Invalidate();
				Update();
			});
		}
		else
		{
			Invalidate();
			Update();
		}
	}

	private void ThrottledRefresh()
	{
		if (m_inFlightInvoke != null && !m_inFlightInvoke.IsCompleted)
		{
			return;
		}
		if (base.IsHandleCreated && base.InvokeRequired)
		{
			if (m_inFlightInvoke != null)
			{
				EndInvoke(m_inFlightInvoke);
			}
			m_inFlightInvoke = BeginInvoke((Action)delegate
			{
				Invalidate();
				Update();
			});
		}
		else
		{
			Invalidate();
			Update();
		}
	}

	public void SetTime(float time)
	{
		current_time = time;
		ThrottledRefresh();
	}

	protected virtual void OnRangeChanged(EventArgs e)
	{
		this.RangeChanged?.Invoke(this, e);
	}

	public int TimeToX(float time)
	{
		float num = time - Origin;
		return (int)(num * MajorScale) + 10;
	}

	public float XToTime(int x)
	{
		x -= 10;
		return (float)x / MajorScale + Origin;
	}

	public void EnsureVisible()
	{
		EnsureVisible(CurrentTime);
	}

	public void EnsureVisible(float time)
	{
		if (time < Origin)
		{
			Origin = Math.Max(0f, time - 64f / MajorScale);
		}
		else if (time > Origin + TimeSpan)
		{
			Origin = Math.Max(0f, time - TimeSpan / 2f);
		}
	}

	private void TimeRulerControl_Paint(object sender, PaintEventArgs e)
	{
		PaintRuler(sender, e);
	}

	protected virtual void PaintRuler(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		float num = (float)Math.Truncate(Origin);
		float num2 = Origin + TimeSpan;
		float num3 = 0.125f / (MajorScale / 100f);
		int num4 = 0;
		using Pen pen = new Pen(TickColor);
		int num6;
		for (float index = num; index < num2; index += num3)
		{
			num6 = TimeToX(index);
			graphics.DrawLine(pen, num6, base.ClientSize.Height - 4, num6, base.ClientSize.Height - 2);
		}
		SizeF sizeF = graphics.MeasureString(TimeCode.ToString(num2, TimeCodeFormat.Seconds), Font);
		using Brush brush = new SolidBrush(ForeColor);
		using StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Center;
		float num5 = num;
		num4 = TimeToX(num - num3);
		for (; num5 < num2; num5 += 1f)
		{
			num6 = TimeToX(num5);
			graphics.DrawLine(pen, num6, base.ClientSize.Height - 8, num6, base.ClientSize.Height - 2);
			if ((float)num4 + sizeF.Width < (float)num6)
			{
				graphics.DrawString(TimeCode.ToString(num5, TimeCodeFormat.Seconds), Font, brush, num6, 1f, stringFormat);
				num4 = num6;
			}
		}
		this.PreShuttlePaint?.Invoke(sender, e);
		if (TrackRangeVisible)
		{
			num6 = TimeToX(RangeStart);
			int num7 = (int)(RangeDuration * MajorScale);
			if (num7 > 0)
			{
				Rectangle rect = new Rectangle(num6, base.ClientSize.Height / 2, num7, base.ClientSize.Height / 2);
				using (Brush brush2 = new SolidBrush(Color.FromArgb(128, RangeColor)))
				{
					graphics.FillRectangle(brush2, rect);
				}
				using Pen pen2 = new Pen(RangeColor);
				graphics.DrawRectangle(pen2, rect);
			}
		}
		if (!ShuttleVisible)
		{
			return;
		}
		using (Pen pen3 = new Pen(ShuttleColor))
		{
			using Brush brush3 = new SolidBrush(Color.FromArgb(128, ShuttleColor));
			num6 = TimeToX(CurrentTime);
			Rectangle rect2 = new Rectangle(num6 - 3, base.ClientSize.Height - 22, 7, 16);
			graphics.FillRectangle(brush3, rect2);
			graphics.DrawRectangle(pen3, rect2);
			graphics.DrawLine(pen3, num6, rect2.Bottom, num6, base.ClientSize.Height);
		}
		if (drag == DragMode.Shuttle)
		{
			using (Brush brush4 = new SolidBrush(ShuttleColor))
			{
				stringFormat.LineAlignment = StringAlignment.Center;
				stringFormat.Alignment = StringAlignment.Near;
				graphics.DrawString(TimeCode.ToString(CurrentTime, TimeCodeFormat.Frame), Font, brush4, new PointF(num6 + 5, base.ClientSize.Height / 2), stringFormat);
				return;
			}
		}
	}

	private HitLocation GetHitLocation(int x, int y)
	{
		int num = TimeToX(CurrentTime);
		if (Math.Abs(num - x) < 8)
		{
			return HitLocation.Shuttle;
		}
		num = TimeToX(RangeStart + RangeDuration);
		if (Math.Abs(num - x) < 8)
		{
			return HitLocation.RangeEnd;
		}
		num = TimeToX(RangeStart);
		if (Math.Abs(num - x) < 8)
		{
			return HitLocation.RangeBegin;
		}
		float num2 = XToTime(x);
		if (num2 >= RangeStart && num2 < RangeStart + RangeDuration)
		{
			return HitLocation.Range;
		}
		return HitLocation.Nothing;
	}

	private bool HitShuttle(int x, int y)
	{
		return GetHitLocation(x, y) == HitLocation.Shuttle;
	}

	private void TimeRulerControl_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Middle)
		{
			drag = DragMode.Pan;
			drag_pt.X = e.X;
			base.Capture = true;
			Cursor = CustomCursors.Get(CustomCursor.HandDrag);
		}
		if (e.Button == MouseButtons.Left)
		{
			switch (GetHitLocation(e.X, e.Y))
			{
			case HitLocation.Shuttle:
				drag = DragMode.Shuttle;
				base.Capture = true;
				break;
			case HitLocation.RangeBegin:
				drag = DragMode.RangeLeft;
				drag_pt.X = e.X;
				base.Capture = true;
				break;
			case HitLocation.RangeEnd:
				drag = DragMode.RangeRight;
				drag_pt.X = e.X;
				base.Capture = true;
				break;
			}
		}
	}

	private void NotifyUserTimeChanged(float time)
	{
		this.UserTimeChanged?.Invoke(this, new TimeRulerEventArgs(time));
	}

	private void TimeRulerControl_MouseMove(object sender, MouseEventArgs e)
	{
		switch (drag)
		{
		case DragMode.Pan:
		{
			float val = Origin - (float)(e.X - drag_pt.X) / MajorScale;
			Origin = Math.Min(Math.Max(0f, val), Math.Max(0f, MaxTime - TimeSpan + 0.5f));
			drag_pt.X = e.X;
			break;
		}
		case DragMode.Shuttle:
		{
			float num3 = Math.Max(0f, XToTime(e.X));
			NotifyUserTimeChanged(num3);
			CurrentTime = num3;
			Update();
			break;
		}
		case DragMode.RangeLeft:
		{
			float num4 = range_begin + range_duration;
			float num5 = (float)(e.X - drag_pt.X) / MajorScale;
			range_begin += num5;
			range_duration -= num5;
			if (range_begin < 0f)
			{
				range_duration = num4;
				range_begin = 0f;
			}
			if (range_duration < 0f)
			{
				float num6 = range_begin;
				range_begin += range_duration;
				range_duration = num6 - range_begin;
				drag = DragMode.RangeRight;
				Cursor = CustomCursors.Get(CustomCursor.RightExtend);
			}
			OnRangeChanged(EventArgs.Empty);
			drag_pt.X = e.X;
			Invalidate();
			break;
		}
		case DragMode.RangeRight:
		{
			float num = (float)(e.X - drag_pt.X) / MajorScale;
			range_duration += num;
			if (range_duration < 0f)
			{
				float num2 = range_begin;
				range_begin += range_duration;
				range_duration = num2 - range_begin;
				drag = DragMode.RangeLeft;
				Cursor = CustomCursors.Get(CustomCursor.LeftExtend);
			}
			OnRangeChanged(EventArgs.Empty);
			drag_pt.X = e.X;
			Invalidate();
			break;
		}
		case DragMode.Idle:
			switch (GetHitLocation(e.X, e.Y))
			{
			case HitLocation.Shuttle:
				Cursor = CustomCursors.Get(CustomCursor.FingerPoint);
				break;
			case HitLocation.RangeBegin:
				Cursor = CustomCursors.Get(CustomCursor.LeftExtend);
				break;
			case HitLocation.RangeEnd:
				Cursor = CustomCursors.Get(CustomCursor.RightExtend);
				break;
			case HitLocation.Range:
				Cursor = CustomCursors.Get(CustomCursor.HandDrag);
				break;
			default:
				Cursor = Cursors.Default;
				break;
			}
			break;
		}
	}

	private void TimeRulerControl_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			switch (drag)
			{
			case DragMode.Shuttle:
				EnsureVisible();
				drag = DragMode.Idle;
				base.Capture = false;
				Cursor = Cursors.Default;
				Invalidate();
				break;
			case DragMode.RangeLeft:
			case DragMode.RangeRight:
				drag = DragMode.Idle;
				base.Capture = false;
				Cursor = Cursors.Default;
				break;
			}
		}
		else if (e.Button == MouseButtons.Middle && drag == DragMode.Pan)
		{
			drag = DragMode.Idle;
			base.Capture = false;
			Cursor = Cursors.Default;
		}
	}

	private void TimeRulerControl_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			float num = Math.Max(0f, XToTime(e.X));
			NotifyUserTimeChanged(num);
			CurrentTime = num;
			EnsureVisible();
		}
	}

	private void TimeRulerControl_MouseWheel(object sender, MouseEventArgs e)
	{
		int num = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
		MajorScale += num;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.DoubleBuffered = true;
		base.Name = "TimeRulerControl";
		this.ForeColor = System.Drawing.Color.Black;
		this.BackColor = System.Drawing.Color.FromArgb(232, 232, 232);
		base.Size = new System.Drawing.Size(412, 55);
		base.Paint += new System.Windows.Forms.PaintEventHandler(TimeRulerControl_Paint);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(TimeRulerControl_MouseMove);
		base.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(TimeRulerControl_MouseDoubleClick);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(TimeRulerControl_MouseDown);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(TimeRulerControl_MouseUp);
		base.MouseWheel += new System.Windows.Forms.MouseEventHandler(TimeRulerControl_MouseWheel);
		base.ResumeLayout(false);
	}
}
