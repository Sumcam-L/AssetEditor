using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Firaxis.Utility;

public sealed class AppBarHelper
{
	public class FormWatcher
	{
		private AppBarHelper helper;

		private AppDockStyle style;

		private int callbackMessage;

		private bool autoHide;

		private bool saveTopMost;

		private FormBorderStyle savedBorder;

		private bool saveInTaskBar;

		private int monitorIndex;

		private Rectangle notDockedBounds;

		public Form Form { get; private set; }

		public Size Size { get; set; }

		public bool Registered { get; private set; }

		public int MonitorIndex
		{
			get
			{
				return monitorIndex;
			}
			set
			{
				monitorIndex = value;
				UpdatePosition();
			}
		}

		public AppDockStyle Style
		{
			get
			{
				return style;
			}
			set
			{
				style = value;
				switch (style)
				{
				case AppDockStyle.Left:
					Edge = ABEdge.Left;
					break;
				case AppDockStyle.Top:
					Edge = ABEdge.Top;
					break;
				case AppDockStyle.Right:
					Edge = ABEdge.Right;
					break;
				case AppDockStyle.Bottom:
					Edge = ABEdge.Bottom;
					break;
				}
				UpdatePosition();
			}
		}

		public ABEdge Edge { get; private set; }

		public bool AutoHide
		{
			get
			{
				return autoHide;
			}
			set
			{
				autoHide = value;
				APPBARDATA data = default(APPBARDATA);
				data.cbsize = Marshal.SizeOf((object)data);
				data.hwnd = Form.Handle;
				data.uedge = (int)Edge;
				data.lparam = new IntPtr(autoHide ? 1 : 0);
				SHAppBarMessage(ABMessage.ABM_SETAUTOHIDEBAR, ref data);
			}
		}

		internal FormWatcher(AppBarHelper helper, Form form, AppDockStyle style, Size idealSize, int monitorIndex)
		{
			this.helper = helper;
			this.monitorIndex = monitorIndex;
			notDockedBounds = form.Bounds;
			Form = form;
			Size = idealSize;
			Style = style;
			form.Load += form_Load;
			form.FormClosed += form_FormClosed;
			form.LocationChanged += form_LocationChanged;
			Register();
		}

		public void UpdatePosition()
		{
			if (!Registered)
			{
				return;
			}
			Screen[] allScreens = Screen.AllScreens;
			int num = Math.Max(0, Math.Min(monitorIndex, allScreens.Length - 1));
			Screen screen = allScreens[num];
			APPBARDATA data = default(APPBARDATA);
			data.cbsize = Marshal.SizeOf((object)data);
			data.hwnd = Form.Handle;
			data.uedge = (int)Edge;
			data.rect.SetRect(screen.Bounds);
			if (Style == AppDockStyle.Left || Style == AppDockStyle.Right)
			{
				if (Style == AppDockStyle.Left)
				{
					data.rect.right = Size.Width;
				}
				else
				{
					data.rect.left = data.rect.right - Size.Width;
				}
			}
			else if (Style == AppDockStyle.Top)
			{
				data.rect.bottom = Size.Height;
			}
			else
			{
				data.rect.top = data.rect.bottom - Size.Height;
			}
			SHAppBarMessage(ABMessage.ABM_QUERYPOS, ref data);
			switch ((ABEdge)data.uedge)
			{
			case ABEdge.Left:
				data.rect.right = data.rect.left + Size.Width;
				break;
			case ABEdge.Right:
				data.rect.left = data.rect.right - Size.Width;
				break;
			case ABEdge.Top:
				data.rect.bottom = data.rect.top + Size.Height;
				break;
			case ABEdge.Bottom:
				data.rect.top = data.rect.bottom - Size.Height;
				break;
			}
			SHAppBarMessage(ABMessage.ABM_SETPOS, ref data);
			NativeMethods.MoveWindow(Form.Handle, data.rect.left, data.rect.top, data.rect.right - data.rect.left, data.rect.bottom - data.rect.top, redraw: true);
		}

		private void form_FormClosed(object sender, FormClosedEventArgs e)
		{
			Unregister();
		}

		private void form_Load(object sender, EventArgs e)
		{
			UpdatePosition();
		}

		private void form_LocationChanged(object sender, EventArgs e)
		{
			UpdatePosition();
		}

		public void Register()
		{
			if (!Registered)
			{
				Form.Hide();
				savedBorder = Form.FormBorderStyle;
				saveInTaskBar = Form.ShowInTaskbar;
				saveTopMost = Form.TopMost;
				Form.FormBorderStyle = FormBorderStyle.None;
				Form.ShowInTaskbar = false;
				Form.TopMost = true;
				callbackMessage = NativeMethods.RegisterWindowMessage("AppBarMessage");
				APPBARDATA data = default(APPBARDATA);
				data.cbsize = Marshal.SizeOf((object)data);
				data.hwnd = Form.Handle;
				data.ucallbackmessage = callbackMessage;
				SHAppBarMessage(ABMessage.ABM_NEW, ref data);
				UpdatePosition();
				Registered = true;
				Form.Show();
			}
		}

		public void Unregister()
		{
			if (Registered)
			{
				Form.Hide();
				Form.FormBorderStyle = savedBorder;
				Form.ShowInTaskbar = saveInTaskBar;
				Form.TopMost = saveTopMost;
				APPBARDATA data = default(APPBARDATA);
				data.cbsize = Marshal.SizeOf((object)data);
				data.hwnd = Form.Handle;
				SHAppBarMessage(ABMessage.ABM_REMOVE, ref data);
				Registered = false;
				Rectangle bounds = notDockedBounds;
				NativeMethods.MoveWindow(Form.Handle, bounds.Left, bounds.Top, bounds.Width, bounds.Height, redraw: true);
				Form.Bounds = bounds;
				Form.Show();
			}
		}
	}

	private struct APPBARDATA
	{
		public int cbsize;

		public IntPtr hwnd;

		public int ucallbackmessage;

		public int uedge;

		public NativeMethods.RECT rect;

		public IntPtr lparam;
	}

	private struct TaskbarState
	{
		public ABEdge edge;

		public ABState state;

		public NativeMethods.RECT size;
	}

	private enum ABMessage
	{
		ABM_NEW,
		ABM_REMOVE,
		ABM_QUERYPOS,
		ABM_SETPOS,
		ABM_GETSTATE,
		ABM_GETTASKBARPOS,
		ABM_ACTIVATE,
		ABM_GETAUTOHIDEBAR,
		ABM_SETAUTOHIDEBAR,
		ABM_WINDOWPOSCHANGED,
		ABM_SETSTATE
	}

	private enum ABNotify
	{
		ABN_STATECHANGE,
		ABN_POSCHANGED,
		ABN_FULLSCREENAPP,
		ABN_WINDOWARRANGE
	}

	[Flags]
	private enum ABState
	{
		ABS_MANUAL = 0,
		ABS_AUTOHIDE = 1,
		ABS_ALWAYSONTOP = 2,
		ABS_AUTOHIDEANDONTOP = 3
	}

	public enum ABEdge
	{
		Left,
		Top,
		Right,
		Bottom
	}

	public enum AppDockStyle
	{
		Left,
		Top,
		Right,
		Bottom
	}

	private List<FormWatcher> forms = new List<FormWatcher>();

	public void Add(Form form)
	{
		Add(form, AppDockStyle.Top, new Size(32, 32), 0);
	}

	public void Add(Form form, AppDockStyle style, Size idealSize)
	{
		Add(form, style, idealSize, 0);
	}

	public void Add(Form form, AppDockStyle style, Size idealSize, int monitorIndex)
	{
		forms.Add(new FormWatcher(this, form, style, idealSize, monitorIndex));
	}

	public FormWatcher Find(Form form)
	{
		return forms.Find((FormWatcher w) => w.Form == form);
	}

	public void Remove(Form form)
	{
		FormWatcher formWatcher = Find(form);
		if (formWatcher != null)
		{
			formWatcher.Unregister();
			forms.Remove(formWatcher);
		}
	}

	[DllImport("shell32.dll")]
	private static extern IntPtr SHAppBarMessage(ABMessage message, ref APPBARDATA data);
}
