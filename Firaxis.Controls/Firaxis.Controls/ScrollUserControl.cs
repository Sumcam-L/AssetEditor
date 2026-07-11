using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Controls.Resources;
using Firaxis.Utility;

namespace Firaxis.Controls;

[ToolboxBitmap(typeof(ResourceTag), "userscroll.bmp")]
[Description("A user control with manual scroll bars")]
public class ScrollUserControl : UserControl
{
	public sealed class ScrollInfo
	{
		private int minValue;

		private int maxValue;

		private int smallInc;

		private int currentValue;

		private NativeMethods.ScrollBarDirection direction;

		private Control owner;

		public int MinValue
		{
			get
			{
				return minValue;
			}
			set
			{
				minValue = value;
				Update();
			}
		}

		public int MaxValue
		{
			get
			{
				return maxValue;
			}
			set
			{
				maxValue = value;
				Update();
			}
		}

		public int SmallInc
		{
			get
			{
				return smallInc;
			}
			set
			{
				smallInc = value;
			}
		}

		public int Value
		{
			get
			{
				return currentValue;
			}
			set
			{
				currentValue = value;
				Update();
				this.ValueChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler ValueChanged;

		internal ScrollInfo(Control owner, NativeMethods.ScrollBarDirection direction)
		{
			this.owner = owner;
			this.direction = direction;
			smallInc = 16;
		}

		private void Update()
		{
			NativeMethods.SCROLLINFO lpsi = NativeMethods.SCROLLINFO.Empty;
			lpsi.fMask = 5;
			lpsi.nMin = minValue;
			lpsi.nMax = maxValue;
			lpsi.nPos = currentValue;
			NativeMethods.SetScrollInfo(owner.Handle, (int)direction, ref lpsi, fRedraw: true);
		}
	}

	private ScrollInfo vert;

	private ScrollInfo horz;

	private IContainer components = null;

	[Browsable(false)]
	public new ScrollInfo VerticalScroll => vert;

	[Browsable(false)]
	public new ScrollInfo HorizontalScroll => horz;

	[Browsable(false)]
	public Point GetScrollPosition => new Point(horz.Value, vert.Value);

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.Style |= 3145728;
			return createParams;
		}
	}

	public ScrollUserControl()
	{
		vert = new ScrollInfo(this, NativeMethods.ScrollBarDirection.SB_VERT);
		horz = new ScrollInfo(this, NativeMethods.ScrollBarDirection.SB_HORZ);
		InitializeComponent();
		base.MouseWheel += ScrollUserControl_MouseWheel;
	}

	private void ScrollUserControl_MouseWheel(object sender, MouseEventArgs e)
	{
		OnVScroll((e.Delta < 0) ? ScrollEventType.SmallIncrement : ScrollEventType.SmallDecrement, 0);
	}

	public void SetScrollPosition(int x, int y)
	{
		horz.Value = x;
		vert.Value = y;
	}

	protected override void WndProc(ref Message m)
	{
		switch (m.Msg)
		{
		case 276:
			OnHScroll((ScrollEventType)NativeMethods.LOWORD(m.WParam), NativeMethods.HIWORD(m.WParam));
			break;
		case 277:
			OnVScroll((ScrollEventType)NativeMethods.LOWORD(m.WParam), NativeMethods.HIWORD(m.WParam));
			break;
		default:
			base.WndProc(ref m);
			break;
		}
	}

	protected virtual void OnHScroll(ScrollEventType type, int newPos)
	{
		NativeMethods.SCROLLINFO lpsi = NativeMethods.SCROLLINFO.Empty;
		lpsi.fMask = 7;
		NativeMethods.GetScrollInfo(base.Handle, 0, ref lpsi);
		int num = lpsi.nPos;
		switch (type)
		{
		case ScrollEventType.First:
			num = lpsi.nMin;
			break;
		case ScrollEventType.Last:
			num = lpsi.nMax - lpsi.nPage;
			break;
		case ScrollEventType.SmallDecrement:
			num -= vert.SmallInc;
			break;
		case ScrollEventType.SmallIncrement:
			num += vert.SmallInc;
			break;
		case ScrollEventType.LargeDecrement:
			num -= base.ClientSize.Width;
			break;
		case ScrollEventType.LargeIncrement:
			num += base.ClientSize.Width;
			break;
		case ScrollEventType.ThumbPosition:
		case ScrollEventType.ThumbTrack:
			num = newPos;
			break;
		}
		horz.Value = Math.Max(Math.Min(num, lpsi.nMax), lpsi.nMin);
		Invalidate();
	}

	public void PerformVScroll(ScrollEventType type, int newPos)
	{
		OnVScroll(type, newPos);
	}

	protected virtual void OnVScroll(ScrollEventType type, int newPos)
	{
		NativeMethods.SCROLLINFO lpsi = NativeMethods.SCROLLINFO.Empty;
		lpsi.fMask = 7;
		NativeMethods.GetScrollInfo(base.Handle, 1, ref lpsi);
		int num = lpsi.nPos;
		switch (type)
		{
		case ScrollEventType.First:
			num = lpsi.nMin;
			break;
		case ScrollEventType.Last:
			num = lpsi.nMax - lpsi.nPage;
			break;
		case ScrollEventType.SmallDecrement:
			num -= vert.SmallInc;
			break;
		case ScrollEventType.SmallIncrement:
			num += vert.SmallInc;
			break;
		case ScrollEventType.LargeDecrement:
			num -= base.ClientSize.Height;
			break;
		case ScrollEventType.LargeIncrement:
			num += base.ClientSize.Height;
			break;
		case ScrollEventType.ThumbPosition:
		case ScrollEventType.ThumbTrack:
			num = newPos;
			break;
		}
		vert.Value = Math.Max(Math.Min(num, lpsi.nMax - base.ClientSize.Height), lpsi.nMin);
		Invalidate();
		Update();
	}

	public virtual void UpdateScrollSizes()
	{
		ScrollUserControl_SizeChanged(this, EventArgs.Empty);
	}

	private void ScrollUserControl_SizeChanged(object sender, EventArgs e)
	{
		NativeMethods.SCROLLINFO lpsi = NativeMethods.SCROLLINFO.Empty;
		lpsi.fMask = 2;
		lpsi.nPage = base.ClientSize.Height;
		NativeMethods.SetScrollInfo(base.Handle, 1, ref lpsi, fRedraw: true);
		lpsi.nPage = base.ClientSize.Width;
		NativeMethods.SetScrollInfo(base.Handle, 0, ref lpsi, fRedraw: true);
		NativeMethods.ShowScrollBar(base.Handle, 1, vert.MaxValue > base.ClientSize.Height);
		NativeMethods.ShowScrollBar(base.Handle, 0, horz.MaxValue > base.ClientSize.Width);
		lpsi.fMask = 4;
		NativeMethods.GetScrollInfo(base.Handle, 1, ref lpsi);
		vert.Value = lpsi.nPos;
		NativeMethods.GetScrollInfo(base.Handle, 0, ref lpsi);
		horz.Value = lpsi.nPos;
		Invalidate();
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
		base.Name = "ScrollUserControl";
		base.Size = new System.Drawing.Size(239, 184);
		base.SizeChanged += new System.EventHandler(ScrollUserControl_SizeChanged);
		base.ResumeLayout(false);
	}
}
