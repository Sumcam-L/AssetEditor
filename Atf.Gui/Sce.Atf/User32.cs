using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Sce.Atf;

public static class User32
{
	public struct HDITEM
	{
		public uint mask;

		public int cxy;

		public string pszText;

		public IntPtr hbm;

		public int cchTextMax;

		public int fmt;

		public IntPtr lParam;

		public int iImage;

		public int iOrder;

		public uint type;

		public IntPtr pvFilter;

		public uint state;
	}

	public struct POINT
	{
		public int X;

		public int Y;
	}

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public struct MINMAXINFO
	{
		public POINT ptReserved;

		public POINT ptMaxSize;

		public POINT ptMaxPosition;

		public POINT ptMinTrackSize;

		public POINT ptMaxTrackSize;
	}

	public struct MSG
	{
		public int hWnd;

		public int msg;

		public int wParam;

		public int lParam;

		public uint time;

		public POINT p;
	}

	public struct TRACKMOUSEEVENT
	{
		public int cbSize;

		public int dwFlags;

		public IntPtr hwndTrack;

		public int dwHoverTime;

		public TRACKMOUSEEVENT(IntPtr hWnd)
		{
			cbSize = Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
			hwndTrack = hWnd;
			dwHoverTime = -1;
			dwFlags = 1;
		}
	}

	public struct NMHDR
	{
		public IntPtr hwndFrom;

		public IntPtr idFrom;

		public int code;
	}

	public class StopDrawingHelper : IDisposable
	{
		private readonly IntPtr m_hwnd;

		public StopDrawingHelper(IntPtr hwnd)
		{
			m_hwnd = hwnd;
			StopDrawing(m_hwnd);
		}

		void IDisposable.Dispose()
		{
			StartDrawing(m_hwnd);
		}
	}

	public delegate int WindowsHookCallback(int code, int wParam, int lParam);

	public enum HookType
	{
		WH_JOURNALRECORD,
		WH_JOURNALPLAYBACK,
		WH_KEYBOARD,
		WH_GETMESSAGE,
		WH_CALLWNDPROC,
		WH_CBT,
		WH_SYSMSGFILTER,
		WH_MOUSE,
		WH_HARDWARE,
		WH_DEBUG,
		WH_SHELL,
		WH_FOREGROUNDIDLE,
		WH_CALLWNDPROCRET,
		WH_KEYBOARD_LL,
		WH_MOUSE_LL
	}

	public enum ShellEvents
	{
		HSHELL_WINDOWCREATED = 1,
		HSHELL_WINDOWDESTROYED = 2,
		HSHELL_ACTIVATESHELLWINDOW = 3,
		HSHELL_WINDOWACTIVATED = 4,
		HSHELL_GETMINRECT = 5,
		HSHELL_REDRAW = 6,
		HSHELL_TASKMAN = 7,
		HSHELL_LANGUAGE = 8,
		HSHELL_ACCESSIBILITYSTATE = 11
	}

	public enum CbtEvents
	{
		HCBT_MOVESIZE,
		HCBT_MINMAX,
		HCBT_QS,
		HCBT_CREATEWND,
		HCBT_DESTROYWND,
		HCBT_ACTIVATE,
		HCBT_CLICKSKIPPED,
		HCBT_KEYSKIPPED,
		HCBT_SYSCOMMAND,
		HCBT_SETFOCUS
	}

	public const int WM_NULL = 0;

	public const int WM_CREATE = 1;

	public const int WM_DESTROY = 2;

	public const int WM_MOVE = 3;

	public const int WM_SIZE = 5;

	public const int WM_ACTIVATE = 6;

	public const int WA_INACTIVE = 0;

	public const int WA_ACTIVE = 1;

	public const int WA_CLICKACTIVE = 2;

	public const int WM_SETFOCUS = 7;

	public const int WM_KILLFOCUS = 8;

	public const int WM_ENABLE = 10;

	public const int WM_SETREDRAW = 11;

	public const int WM_SETTEXT = 12;

	public const int WM_GETTEXT = 13;

	public const int WM_GETTEXTLENGTH = 14;

	public const int WM_PAINT = 15;

	public const int WM_CLOSE = 16;

	public const int WM_ERASEBKGND = 20;

	public const int WM_SETCURSOR = 32;

	public const int WM_GETMINMAXINFO = 36;

	public const int WM_HELP = 83;

	public const int WM_NCHITTEST = 132;

	public const int WM_NCPAINT = 133;

	public const int WM_NCLBUTTONDOWN = 161;

	public const int WM_NCLBUTTONUP = 162;

	public const int WM_KEYDOWN = 256;

	public const int WM_SYSKEYDOWN = 260;

	public const int WM_INITDIALOG = 272;

	public const int WM_COMMAND = 273;

	public const int WM_SYSCOMMAND = 274;

	public const int WM_HSCROLL = 276;

	public const int WM_VSCROLL = 277;

	public const int WM_UPDATEUISTATE = 296;

	public const int WM_SIZING = 532;

	public const int WM_LBUTTONDOWN = 513;

	public const int WM_LBUTTONUP = 514;

	public const int WM_LBUTTONDBLCLK = 515;

	public const int WM_MOUSEMOVE = 512;

	public const int WM_MOUSEWHEEL = 522;

	public const int WM_CUT = 768;

	public const int WM_COPY = 769;

	public const int WM_PASTE = 770;

	public const int WM_CLEAR = 771;

	public const int WM_UNDO = 772;

	public const int WM_USER = 1024;

	public const int WM_REFLECT = 8192;

	public const int WM_NOTIFY = 78;

	public const int CB_OKAY = 0;

	public const int CB_ERR = -1;

	public const int CB_ERRSPACE = -2;

	public const int CBN_ERRSPACE = -1;

	public const int CBN_SELCHANGE = 1;

	public const int CBN_DBLCLK = 2;

	public const int CBN_SETFOCUS = 3;

	public const int CBN_KILLFOCUS = 4;

	public const int CBN_EDITCHANGE = 5;

	public const int CBN_EDITUPDATE = 6;

	public const int CBN_DROPDOWN = 7;

	public const int CBN_CLOSEUP = 8;

	public const int CBN_SELENDOK = 9;

	public const int CBN_SELENDCANCEL = 10;

	public const int CBS_SIMPLE = 1;

	public const int CBS_DROPDOWN = 2;

	public const int CBS_DROPDOWNLIST = 3;

	public const int CBS_OWNERDRAWFIXED = 16;

	public const int CBS_OWNERDRAWVARIABLE = 32;

	public const int CBS_AUTOHSCROLL = 64;

	public const int CBS_OEMCONVERT = 128;

	public const int CBS_SORT = 256;

	public const int CBS_HASSTRINGS = 512;

	public const int CBS_NOINTEGRALHEIGHT = 1024;

	public const int CBS_DISABLENOSCROLL = 2048;

	public const int CBS_UPPERCASE = 8192;

	public const int CBS_LOWERCASE = 16384;

	public const int CB_GETEDITSEL = 320;

	public const int CB_LIMITTEXT = 321;

	public const int CB_SETEDITSEL = 322;

	public const int CB_ADDSTRING = 323;

	public const int CB_DELETESTRING = 324;

	public const int CB_DIR = 325;

	public const int CB_GETCOUNT = 326;

	public const int CB_GETCURSEL = 327;

	public const int CB_GETLBTEXT = 328;

	public const int CB_GETLBTEXTLEN = 329;

	public const int CB_INSERTSTRING = 330;

	public const int CB_RESETCONTENT = 331;

	public const int CB_FINDSTRING = 332;

	public const int CB_SELECTSTRING = 333;

	public const int CB_SETCURSEL = 334;

	public const int CB_SHOWDROPDOWN = 335;

	public const int CB_GETITEMDATA = 336;

	public const int CB_SETITEMDATA = 337;

	public const int CB_GETDROPPEDCONTROLRECT = 338;

	public const int CB_SETITEMHEIGHT = 339;

	public const int CB_GETITEMHEIGHT = 340;

	public const int CB_SETEXTENDEDUI = 341;

	public const int CB_GETEXTENDEDUI = 342;

	public const int CB_GETDROPPEDSTATE = 343;

	public const int CB_FINDSTRINGEXACT = 344;

	public const int CB_SETLOCALE = 345;

	public const int CB_GETLOCALE = 346;

	public const int CB_GETTOPINDEX = 347;

	public const int CB_SETTOPINDEX = 348;

	public const int CB_GETHORIZONTALEXTENT = 349;

	public const int CB_SETHORIZONTALEXTENT = 350;

	public const int CB_GETDROPPEDWIDTH = 351;

	public const int CB_SETDROPPEDWIDTH = 352;

	public const int CB_INITSTORAGE = 353;

	public const int CB_MULTIPLEADDSTRING = 355;

	public const int CB_GETCOMBOBOXINFO = 356;

	public const int CB_MSGMAX = 357;

	public const int LVM_FIRST = 4096;

	public const int HDM_FIRST = 4608;

	public const int HDM_SETFOCUSEDITEM = 4636;

	public const int HDI_FORMAT = 4;

	public const int HDF_LEFT = 0;

	public const int HDF_STRING = 16384;

	public const int HDF_SORTUP = 1024;

	public const int HDF_SORTDOWN = 512;

	public const int LVM_GETHEADER = 4127;

	public const int HDM_GETITEM = 4619;

	public const int HDM_SETITEM = 4620;

	public const int BS_PUSHBUTTON = 0;

	public const int BS_DEFPUSHBUTTON = 1;

	public const int BS_CHECKBOX = 2;

	public const int BS_AUTOCHECKBOX = 3;

	public const int BS_RADIOBUTTON = 4;

	public const int BS_3STATE = 5;

	public const int BS_AUTO3STATE = 6;

	public const int BS_GROUPBOX = 7;

	public const int BS_AUTORADIOBUTTON = 9;

	public const int BS_OWNERDRAW = 11;

	public const int BS_LEFTTEXT = 32;

	public const int BS_TEXT = 0;

	public const int BS_LEFT = 256;

	public const int BS_RIGHT = 512;

	public const int BS_CENTER = 768;

	public const int BS_TOP = 1024;

	public const int BS_BOTTOM = 2048;

	public const int BS_VCENTER = 3072;

	public const int BS_PUSHLIKE = 4096;

	public const int BS_MULTILINE = 8192;

	public const int BS_NOTIFY = 16384;

	public const int BS_RIGHTBUTTON = 32;

	public const int BN_CLICKED = 0;

	public const int BN_PAINT = 1;

	public const int BN_DBLCLK = 5;

	public const int BN_SETFOCUS = 6;

	public const int BN_KILLFOCUS = 7;

	public const int QS_MOUSEMOVE = 2;

	public const int QS_MOUSEBUTTON = 4;

	public const int QS_MOUSE = 6;

	public const int QS_KEY = 1;

	public const int QS_RAWINPUT = 1024;

	public const int PM_QS_INPUT = 67567616;

	public const int PM_NOREMOVE = 0;

	public const int PM_REMOVE = 1;

	public const int PM_NOYIELD = 2;

	public const int TME_HOVER = 1;

	public const int HOVER_DEFAULT = -1;

	public const int HTCAPTION = 2;

	public const int CS_NOCLOSE = 512;

	public const int EM_GETEVENTMASK = 1083;

	public const int EM_SETEVENTMASK = 1093;

	public const int NM_FIRST = 0;

	public const int HDN_FIRST = -300;

	public const int HDN_ITEMCHANGINGA = -300;

	public const int HDN_ITEMCHANGINGW = -320;

	public const int HDN_ITEMCHANGEDA = -301;

	public const int HDN_ITEMCHANGEDW = -321;

	public const int HDN_ITEMCLICKA = -302;

	public const int HDN_ITEMCLICKW = -322;

	public const int HDN_ITEMDBLCLICKA = -303;

	public const int HDN_ITEMDBLCLICKW = -323;

	public const int HDN_DIVIDERDBLCLICKA = -305;

	public const int HDN_DIVIDERDBLCLICKW = -325;

	public const int HDN_BEGINTRACKA = -306;

	public const int HDN_BEGINTRACKW = -326;

	public const int HDN_ENDTRACKA = -307;

	public const int HDN_ENDTRACKW = -327;

	public const int HDN_TRACKA = -308;

	public const int HDN_TRACKW = -328;

	public const int HDN_GETDISPINFOA = -309;

	public const int HDN_GETDISPINFOW = -329;

	public const int HDN_BEGINDRAG = -310;

	public const int HDN_ENDDRAG = -311;

	public const int HDN_FILTERCHANGE = -312;

	public const int HDN_FILTERBTNCLICK = -313;

	public const int SW_HIDE = 0;

	public const int SW_SHOWNORMAL = 1;

	public const int SW_NORMAL = 1;

	public const int SW_SHOWMINIMIZED = 2;

	public const int SW_SHOWMAXIMIZED = 3;

	public const int SW_MAXIMIZE = 3;

	public const int SW_SHOWNOACTIVATE = 4;

	public const int SW_SHOW = 5;

	public const int SW_MINIMIZE = 6;

	public const int SW_SHOWMINNOACTIVE = 7;

	public const int SW_SHOWNA = 8;

	public const int SW_RESTORE = 9;

	public const int SW_SHOWDEFAULT = 10;

	public const int SW_FORCEMINIMIZE = 11;

	public const int SW_MAX = 11;

	private const string DllName = "user32.dll";

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern uint SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", EntryPoint = "SendMessage")]
	public static extern IntPtr SendMessageITEM(IntPtr Handle, int msg, IntPtr wParam, ref HDITEM lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetFocus();

	[DllImport("user32.dll")]
	public static extern int DestroyIcon(IntPtr hIcon);

	[DllImport("user32.dll")]
	public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr iconName);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern int GetScrollPos(IntPtr hWnd, int nBar);

	public static int HIWORD(int n)
	{
		return (n >> 16) & 0xFFFF;
	}

	public static int HIWORD(IntPtr n)
	{
		return HIWORD((int)(long)n);
	}

	public static int LOWORD(int n)
	{
		return n & 0xFFFF;
	}

	public static int LOWORD(IntPtr n)
	{
		return LOWORD((int)(long)n);
	}

	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	public static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern bool IsIconic(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern IntPtr GetDesktopWindow();

	[DllImport("user32.dll")]
	public static extern IntPtr GetDC(IntPtr hwnd);

	[DllImport("user32.dll")]
	public static extern IntPtr GetWindowDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("user32.dll")]
	public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool GetClientRect(IntPtr hWnd, ref Rectangle r);

	[DllImport("user32.dll")]
	public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int GetWindowText(IntPtr hWnd, [Out][MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	public static extern IntPtr WindowFromPoint(Point point);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern bool PeekMessage(out MSG msg, int hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

	[DllImport("user32.dll")]
	public static extern int MsgWaitForMultipleObjects(int nCount, int pHandles, bool bWaitAll, int dwMilliseconds, int dwWakeMask);

	[DllImport("user32.dll")]
	public static extern uint GetClipboardSequenceNumber();

	public static void StartDrawing(IntPtr hwnd)
	{
		SendMessage(hwnd, 11, 1, (int)IntPtr.Zero);
	}

	public static void StopDrawing(IntPtr hwnd)
	{
		SendMessage(hwnd, 11, 0, (int)IntPtr.Zero);
	}

	[DllImport("user32.dll")]
	public static extern IntPtr SetWindowsHookEx(HookType code, WindowsHookCallback func, IntPtr hInstance, int threadID);

	[DllImport("user32.dll")]
	public static extern int CallNextHookEx(IntPtr hhk, int nCode, int wParam, int lParam);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll")]
	public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

	[DllImport("user32.dll")]
	public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

	[DllImport("user32.dll")]
	public static extern bool ShowCaret(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern bool GetCaretPos(out Point lpPoint);

	[DllImport("user32.dll")]
	public static extern bool HideCaret(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern bool SetCaretPos(int x, int y);

	[DllImport("user32.dll")]
	public static extern bool DestroyCaret();
}
