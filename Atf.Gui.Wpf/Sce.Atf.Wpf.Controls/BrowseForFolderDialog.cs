using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Sce.Atf.Wpf.Controls;

public class BrowseForFolderDialog
{
	public delegate int BrowseCallbackProc(IntPtr hwnd, MessageFromBrowser uMsg, IntPtr lParam, IntPtr lpData);

	[Flags]
	public enum BrowseInfoFlags : uint
	{
		BIF_None = 0u,
		BIF_RETURNONLYFSDIRS = 1u,
		BIF_DONTGOBELOWDOMAIN = 2u,
		BIF_STATUSTEXT = 4u,
		BIF_RETURNFSANCESTORS = 8u,
		BIF_EDITBOX = 0x10u,
		BIF_VALIDATE = 0x20u,
		BIF_NEWDIALOGSTYLE = 0x40u,
		BIF_USENEWUI = 0x50u,
		BIF_BROWSEINCLUDEURLS = 0x80u,
		BIF_UAHINT = 0x100u,
		BIF_NONEWFOLDERBUTTON = 0x200u,
		BIF_NOTRANSLATETARGETS = 0x400u,
		BIF_BROWSEFORCOMPUTER = 0x1000u,
		BIF_BROWSEFORPRINTER = 0x2000u,
		BIF_BROWSEINCLUDEFILES = 0x4000u,
		BIF_SHAREABLE = 0x8000u
	}

	public enum MessageFromBrowser : uint
	{
		BFFM_INITIALIZED = 1u,
		BFFM_SELCHANGED,
		BFFM_VALIDATEFAILEDA,
		BFFM_VALIDATEFAILEDW,
		BFFM_IUNKNOWN
	}

	public enum MessageToBrowser : uint
	{
		WM_USER = 1024u,
		BFFM_SETSTATUSTEXTA = 1124u,
		BFFM_ENABLEOK = 1125u,
		BFFM_SETSELECTIONA = 1126u,
		BFFM_SETSELECTIONW = 1127u,
		BFFM_SETSTATUSTEXTW = 1128u,
		BFFM_SETOKTEXT = 1129u,
		BFFM_SETEXPANDED = 1130u
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class BROWSEINFOW
	{
		public IntPtr hwndOwner;

		public IntPtr pidlRoot;

		public string pszDisplayName;

		public string lpszTitle;

		public BrowseInfoFlags ulFlags;

		public BrowseCallbackProc lpfn;

		public IntPtr lParam;

		public int iImage;
	}

	private BROWSEINFOW browseInfo;

	public string SelectedFolder { get; protected set; }

	public string Title
	{
		get
		{
			return BrowseInfo.lpszTitle;
		}
		set
		{
			BrowseInfo.lpszTitle = value;
		}
	}

	public string InitialFolder { get; set; }

	public string InitialExpandedFolder { get; set; }

	public string OKButtonText { get; set; }

	public BROWSEINFOW BrowseInfo
	{
		get
		{
			return browseInfo;
		}
		protected set
		{
			browseInfo = value;
		}
	}

	public BrowseInfoFlags BrowserDialogFlags
	{
		get
		{
			return BrowseInfo.ulFlags;
		}
		set
		{
			BrowseInfo.ulFlags = value;
		}
	}

	public BrowseForFolderDialog()
	{
		BrowseInfo = new BROWSEINFOW();
		BrowseInfo.hwndOwner = IntPtr.Zero;
		BrowseInfo.pidlRoot = IntPtr.Zero;
		BrowseInfo.pszDisplayName = new string(' ', 260);
		BrowseInfo.lpszTitle = "Select a folder:";
		BrowseInfo.ulFlags = BrowseInfoFlags.BIF_NEWDIALOGSTYLE;
		BrowseInfo.lpfn = BrowseEventHandler;
		BrowseInfo.lParam = IntPtr.Zero;
		BrowseInfo.iImage = -1;
	}

	public bool? ShowDialog()
	{
		return PInvokeSHBrowseForFolder(null);
	}

	public bool? ShowDialog(Window owner)
	{
		return PInvokeSHBrowseForFolder(owner);
	}

	[DllImport("user32.dll")]
	public static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern IntPtr SendMessageW(IntPtr hWnd, MessageToBrowser msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string str);

	private bool? PInvokeSHBrowseForFolder(Window owner)
	{
		if (owner != null)
		{
			WindowInteropHelper windowInteropHelper = new WindowInteropHelper(owner);
			BrowseInfo.hwndOwner = windowInteropHelper.Handle;
		}
		IntPtr intPtr = SHBrowseForFolderW(browseInfo);
		if (IntPtr.Zero != intPtr)
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			if (SHGetPathFromIDList(intPtr, stringBuilder))
			{
				SelectedFolder = stringBuilder.ToString();
				Marshal.FreeCoTaskMem(intPtr);
				return true;
			}
		}
		return false;
	}

	private int BrowseEventHandler(IntPtr hwnd, MessageFromBrowser uMsg, IntPtr lParam, IntPtr lpData)
	{
		switch (uMsg)
		{
		case MessageFromBrowser.BFFM_INITIALIZED:
			if (!string.IsNullOrEmpty(InitialExpandedFolder))
			{
				SendMessageW(hwnd, MessageToBrowser.BFFM_SETEXPANDED, new IntPtr(1), InitialExpandedFolder);
			}
			else if (!string.IsNullOrEmpty(InitialFolder))
			{
				SendMessageW(hwnd, MessageToBrowser.BFFM_SETSELECTIONW, new IntPtr(1), InitialFolder);
			}
			if (!string.IsNullOrEmpty(OKButtonText))
			{
				SendMessageW(hwnd, MessageToBrowser.BFFM_SETOKTEXT, new IntPtr(1), OKButtonText);
			}
			break;
		case MessageFromBrowser.BFFM_SELCHANGED:
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			if (SHGetPathFromIDList(lParam, stringBuilder))
			{
				SelectedFolder = stringBuilder.ToString();
			}
			break;
		}
		}
		return 0;
	}

	[DllImport("shell32.dll")]
	private static extern IntPtr SHBrowseForFolderW([In][Out][MarshalAs(UnmanagedType.LPStruct)] BROWSEINFOW bi);

	[DllImport("shell32.dll")]
	private static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder path);
}
