using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Controls.Resources;
using Firaxis.Utility;

namespace Firaxis.Controls;

[ToolboxBitmap(typeof(ResourceTag), "tooltip.bmp")]
[Description("Displays information when the user hovers the mouse.  Supports multiple regions on a single control")]
public class TooltipComponent : Component, ITooltipHandler
{
	private enum TipState
	{
		Hidden,
		Pending,
		Shown
	}

	private class TipDescriptor
	{
		public ITooltipHandler Handler;

		public string Text;

		public TipDescriptor(ITooltipHandler handler, string text)
		{
			Handler = handler;
			Text = text;
		}
	}

	private Control parent;

	private TipDescriptor tip;

	private TipState state = TipState.Hidden;

	private IToolTipView tipView;

	private int autoPop;

	private int initial;

	private int reshow;

	private int trackid;

	private long poptime;

	private IContainer components = null;

	private Timer timer;

	public TooltipComponent()
	{
		InitializeComponent();
		GetTimingInfo();
	}

	public TooltipComponent(IContainer container)
	{
		container.Add(this);
		InitializeComponent();
		GetTimingInfo();
	}

	public void AddTip(Control control, string text)
	{
		AddTip(control, this, text);
	}

	public void AddTip(Control control, ITooltipHandler handler)
	{
		AddTip(control, handler, null);
	}

	private void AddTip(Control control, ITooltipHandler handler, string text)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		if (parent != null || tip != null)
		{
			throw new ArgumentException("Tip handler already attached");
		}
		parent = control;
		tip = new TipDescriptor(handler, text);
		SubscribeEvents(control, subscribe: true);
	}

	protected virtual IToolTipView CreateTipView()
	{
		return new ToolTipView();
	}

	public void RemoveTip()
	{
		if (parent != null && tip != null)
		{
			HideTip();
			SubscribeEvents(parent, subscribe: false);
			parent = null;
			tip = null;
		}
	}

	private void SubscribeEvents(Control control, bool subscribe)
	{
		if (subscribe)
		{
			if (control is Form)
			{
				((Form)control).FormClosing += control_FormClosing;
			}
			control.MouseMove += control_MouseMove;
			control.KeyDown += control_KeyDown;
			control.MouseDown += control_MouseDown;
			control.MouseUp += control_MouseUp;
			control.MouseEnter += control_MouseEnter;
		}
		else
		{
			if (control is Form)
			{
				((Form)control).FormClosing -= control_FormClosing;
			}
			control.MouseMove -= control_MouseMove;
			control.KeyDown -= control_KeyDown;
			control.MouseDown -= control_MouseDown;
			control.MouseUp -= control_MouseUp;
			control.MouseEnter -= control_MouseEnter;
		}
	}

	private void control_MouseEnter(object sender, EventArgs e)
	{
		EnterTip();
	}

	private void EnterTip()
	{
		if (tip == null)
		{
			throw new ArgumentNullException("tip");
		}
		poptime = 0L;
		timer.Interval = 80;
		timer.Enabled = true;
		state = TipState.Hidden;
	}

	private void HideTip()
	{
		if (tipView != null)
		{
			tipView.ShowTip(parent, show: false);
		}
		timer.Enabled = false;
		state = TipState.Hidden;
		trackid = 0;
	}

	private void control_MouseUp(object sender, MouseEventArgs e)
	{
		HideTip();
	}

	private void control_MouseDown(object sender, MouseEventArgs e)
	{
		HideTip();
	}

	private void control_KeyDown(object sender, KeyEventArgs e)
	{
		HideTip();
	}

	private void control_MouseMove(object sender, MouseEventArgs e)
	{
		MouseMove(new Point(e.X, e.Y));
	}

	private void control_FormClosing(object sender, FormClosingEventArgs e)
	{
		HideTip();
	}

	private bool IsOverTip(Point point)
	{
		return point.X >= 0 && point.Y >= 0 && point.X < parent.Size.Width && point.Y < parent.Size.Height;
	}

	private void timer_Tick(object sender, EventArgs e)
	{
		Point position = Cursor.Position;
		Point point = parent.PointToClient(position);
		poptime += timer.Interval;
		switch (state)
		{
		case TipState.Shown:
			if (!IsOverTip(point) || tip.Handler.GetTipID(parent, point) != trackid)
			{
				HideTip();
			}
			break;
		case TipState.Pending:
			if (IsOverTip(point) && tip.Handler.GetTipID(parent, point) == trackid)
			{
				if (poptime < initial)
				{
					break;
				}
				string text = null;
				Image image = null;
				string description = null;
				if (!tip.Handler.GetTipInfo(parent, trackid, ref text, ref image, ref description) || (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(description) && image == null))
				{
					HideTip();
					break;
				}
				if (text == null)
				{
					text = string.Empty;
				}
				if (description == null)
				{
					description = string.Empty;
				}
				state = TipState.Shown;
				poptime = 0L;
				position.Y += SystemInformation.CursorSize.Height;
				if (tipView == null)
				{
					tipView = CreateTipView();
					if (tipView == null)
					{
						throw new ArgumentNullException("tipView");
					}
				}
				tipView.Image = image;
				tipView.Caption = text;
				tipView.Description = description;
				tipView.Position = position;
				tipView.ShowTip(parent, show: true);
			}
			else
			{
				HideTip();
			}
			break;
		}
	}

	private void MouseMove(Point point)
	{
		int tipID = tip.Handler.GetTipID(parent, point);
		switch (state)
		{
		case TipState.Hidden:
			if (tipID != 0)
			{
				state = TipState.Pending;
				trackid = tipID;
				poptime = 0L;
			}
			else if (tipID != trackid)
			{
				trackid = 0;
			}
			break;
		case TipState.Pending:
		case TipState.Shown:
			poptime = 0L;
			if (tipID != trackid)
			{
				HideTip();
			}
			break;
		}
	}

	private void GetTimingInfo()
	{
		using (Firaxis.Utility.NativeWindow nativeWindow = new Firaxis.Utility.NativeWindow())
		{
			nativeWindow.CreateWindow(0, "tooltips_class32", "", 0, 0, 0, 0, 0, null);
			autoPop = NativeMethods.SendMessage(nativeWindow.WindowHandle, 1045, new IntPtr(2), IntPtr.Zero).ToInt32();
			initial = NativeMethods.SendMessage(nativeWindow.WindowHandle, 1045, new IntPtr(3), IntPtr.Zero).ToInt32();
			reshow = NativeMethods.SendMessage(nativeWindow.WindowHandle, 1045, new IntPtr(1), IntPtr.Zero).ToInt32();
		}
		timer.Tick += timer_Tick;
	}

	public int GetTipID(Control control, Point point)
	{
		return (tip != null) ? 1 : 0;
	}

	public bool GetTipInfo(Control control, int id, ref string text, ref Image image, ref string description)
	{
		if (tip != null)
		{
			text = tip.Text ?? "";
			return true;
		}
		return false;
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
		components = new Container();
		timer = new Timer(components);
	}
}
