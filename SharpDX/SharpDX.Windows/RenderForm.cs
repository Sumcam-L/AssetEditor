using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SharpDX.Properties;

namespace SharpDX.Windows;

public class RenderForm : Form
{
	private const int WM_SIZE = 5;

	private const int SIZE_RESTORED = 0;

	private const int SIZE_MINIMIZED = 1;

	private const int SIZE_MAXIMIZED = 2;

	private const int SIZE_MAXSHOW = 3;

	private const int SIZE_MAXHIDE = 4;

	private const int WM_ACTIVATEAPP = 28;

	private const int WM_POWERBROADCAST = 536;

	private const int WM_MENUCHAR = 288;

	private const int WM_SYSCOMMAND = 274;

	private const uint PBT_APMRESUMESUSPEND = 7u;

	private const uint PBT_APMQUERYSUSPEND = 0u;

	private const int SC_MONITORPOWER = 61808;

	private const int SC_SCREENSAVE = 61760;

	private const int MNC_CLOSE = 1;

	private Size cachedSize;

	private FormWindowState previousWindowState;

	private bool isUserResizing;

	private bool allowUserResizing;

	private bool isBackgroundFirstDraw;

	private bool isSizeChangedWithoutResizeBegin;

	public bool AllowUserResizing
	{
		get
		{
			return allowUserResizing;
		}
		set
		{
			if (allowUserResizing != value)
			{
				allowUserResizing = value;
				base.MaximizeBox = allowUserResizing;
				base.FormBorderStyle = ((!IsFullscreen) ? ((!allowUserResizing) ? FormBorderStyle.FixedSingle : FormBorderStyle.Sizable) : FormBorderStyle.None);
			}
		}
	}

	public bool IsFullscreen { get; set; }

	public event EventHandler<EventArgs> AppActivated;

	public event EventHandler<EventArgs> AppDeactivated;

	public event EventHandler<EventArgs> MonitorChanged;

	public event EventHandler<EventArgs> PauseRendering;

	public event EventHandler<EventArgs> ResumeRendering;

	public event EventHandler<CancelEventArgs> Screensaver;

	public event EventHandler<EventArgs> SystemResume;

	public event EventHandler<EventArgs> SystemSuspend;

	public event EventHandler<EventArgs> UserResized;

	public RenderForm()
		: this("SharpDX")
	{
	}

	public RenderForm(string text)
	{
		Text = text;
		base.ClientSize = new Size(800, 600);
		base.ResizeRedraw = true;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, value: true);
		base.Icon = Resources.logo;
		previousWindowState = FormWindowState.Normal;
		AllowUserResizing = true;
	}

	protected override void OnResizeBegin(EventArgs e)
	{
		isUserResizing = true;
		base.OnResizeBegin(e);
		cachedSize = base.Size;
		OnPauseRendering(e);
	}

	protected override void OnResizeEnd(EventArgs e)
	{
		base.OnResizeEnd(e);
		if (isUserResizing && cachedSize != base.Size)
		{
			OnUserResized(e);
		}
		isUserResizing = false;
		OnResumeRendering(e);
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		if (!isBackgroundFirstDraw)
		{
			base.OnPaintBackground(e);
			isBackgroundFirstDraw = true;
		}
	}

	private void OnPauseRendering(EventArgs e)
	{
		if (this.PauseRendering != null)
		{
			this.PauseRendering(this, e);
		}
	}

	private void OnResumeRendering(EventArgs e)
	{
		if (this.ResumeRendering != null)
		{
			this.ResumeRendering(this, e);
		}
	}

	private void OnUserResized(EventArgs e)
	{
		if (this.UserResized != null)
		{
			this.UserResized(this, e);
		}
	}

	private void OnMonitorChanged(EventArgs e)
	{
		if (this.MonitorChanged != null)
		{
			this.MonitorChanged(this, e);
		}
	}

	private void OnAppActivated(EventArgs e)
	{
		if (this.AppActivated != null)
		{
			this.AppActivated(this, e);
		}
	}

	private void OnAppDeactivated(EventArgs e)
	{
		if (this.AppDeactivated != null)
		{
			this.AppDeactivated(this, e);
		}
	}

	private void OnSystemSuspend(EventArgs e)
	{
		if (this.SystemSuspend != null)
		{
			this.SystemSuspend(this, e);
		}
	}

	private void OnSystemResume(EventArgs e)
	{
		if (this.SystemResume != null)
		{
			this.SystemResume(this, e);
		}
	}

	private void OnScreensaver(CancelEventArgs e)
	{
		if (this.Screensaver != null)
		{
			this.Screensaver(this, e);
		}
	}

	protected override void OnClientSizeChanged(EventArgs e)
	{
		base.OnClientSizeChanged(e);
		if (!isUserResizing && (isSizeChangedWithoutResizeBegin || cachedSize != base.Size))
		{
			isSizeChangedWithoutResizeBegin = false;
			cachedSize = base.Size;
			OnUserResized(EventArgs.Empty);
		}
	}

	protected override void WndProc(ref Message m)
	{
		long num = m.WParam.ToInt64();
		switch (m.Msg)
		{
		case 5:
		{
			if (num == 1)
			{
				previousWindowState = FormWindowState.Minimized;
				OnPauseRendering(EventArgs.Empty);
				break;
			}
			Win32Native.GetClientRect(m.HWnd, out var lpRect);
			if (lpRect.Bottom - lpRect.Top == 0)
			{
				break;
			}
			switch (num)
			{
			case 2L:
				if (previousWindowState == FormWindowState.Minimized)
				{
					OnResumeRendering(EventArgs.Empty);
				}
				previousWindowState = FormWindowState.Maximized;
				OnUserResized(EventArgs.Empty);
				cachedSize = base.Size;
				break;
			case 0L:
				if (previousWindowState == FormWindowState.Minimized)
				{
					OnResumeRendering(EventArgs.Empty);
				}
				if (!isUserResizing && (base.Size != cachedSize || previousWindowState == FormWindowState.Maximized))
				{
					previousWindowState = FormWindowState.Normal;
					if (cachedSize != Size.Empty)
					{
						isSizeChangedWithoutResizeBegin = true;
					}
				}
				previousWindowState = FormWindowState.Normal;
				break;
			}
			break;
		}
		case 28:
			if (num != 0)
			{
				OnAppActivated(EventArgs.Empty);
			}
			else
			{
				OnAppDeactivated(EventArgs.Empty);
			}
			break;
		case 536:
			switch (num)
			{
			case 0L:
				OnSystemSuspend(EventArgs.Empty);
				m.Result = new IntPtr(1);
				return;
			case 7L:
				OnSystemResume(EventArgs.Empty);
				m.Result = new IntPtr(1);
				return;
			}
			break;
		case 288:
			m.Result = new IntPtr(65536);
			return;
		case 274:
			num &= 0xFFF0;
			if (num == 61808 || num == 61760)
			{
				CancelEventArgs e = new CancelEventArgs();
				OnScreensaver(e);
				if (e.Cancel)
				{
					m.Result = IntPtr.Zero;
					return;
				}
			}
			break;
		}
		base.WndProc(ref m);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (keyData == (Keys.Menu | Keys.Alt) || keyData == Keys.F10)
		{
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}
}
