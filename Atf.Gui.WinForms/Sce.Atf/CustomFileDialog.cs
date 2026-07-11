using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sce.Atf;

public abstract class CustomFileDialog
{
	protected delegate int WndProcDelegate(IntPtr hWnd, uint msg, int wParam, int lParam);

	public struct WINDOWINFO
	{
		public uint cbSize;

		public User32.RECT rcWindow;

		public User32.RECT rcClient;

		public uint dwStyle;

		public uint dwExStyle;

		public uint dwWindowStatus;

		public uint cxWindowBorders;

		public uint cyWindowBorders;

		public ushort atomWindowType;

		public ushort wCreatorVersion;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	protected struct OPENFILENAME
	{
		public int lStructSize;

		public IntPtr hwndOwner;

		public int hInstance;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpstrFilter;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpstrCustomFilter;

		public int nMaxCustFilter;

		public int nFilterIndex;

		public IntPtr lpstrFile;

		public int nMaxFile;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpstrFileTitle;

		public int nMaxFileTitle;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpstrInitialDir;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpstrTitle;

		public int Flags;

		public short nFileOffset;

		public short nFileExtension;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpstrDefExt;

		public int lCustData;

		public WndProcDelegate lpfnHook;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpTemplateName;

		public int pvReserved;

		public int dwReserved;

		public int FlagsEx;
	}

	protected const int OFN_ENABLEHOOK = 32;

	protected const int OFN_EXPLORER = 524288;

	protected const int OFN_FILEMUSTEXIST = 4096;

	protected const int OFN_HIDEREADONLY = 4;

	protected const int OFN_CREATEPROMPT = 8192;

	protected const int OFN_NOTESTFILECREATE = 65536;

	protected const int OFN_OVERWRITEPROMPT = 2;

	protected const int OFN_PATHMUSTEXIST = 2048;

	protected const int OFN_NODEREFERENCELINKS = 1048576;

	protected const int OFN_NOCHANGEDIR = 8;

	protected const int OFN_NOVALIDATE = 256;

	protected const int OFN_ENABLESIZING = 8388608;

	protected const int OFN_READONLY = 1;

	protected const int OFN_ALLOWMULTISELECT = 512;

	private const int GWL_WNDPROC = -4;

	private const int SWP_NOMOVE = 2;

	private const int SWP_NOZORDER = 4;

	private static Dictionary<string, string> s_filterToLastDir = new Dictionary<string, string>();

	private string m_filter = string.Empty;

	private int m_filterIndex;

	private string m_fileName = string.Empty;

	private string[] m_fileNames = new string[0];

	private string m_forcedInitialDir = string.Empty;

	private string m_title = string.Empty;

	private string m_defaultExt = string.Empty;

	private bool m_addExt = true;

	private int m_flags = 8914982;

	public bool AddExtension
	{
		get
		{
			return m_addExt;
		}
		set
		{
			m_addExt = value;
		}
	}

	public bool CheckFileExists
	{
		get
		{
			return GetFlag(4096);
		}
		set
		{
			SetFlag(4096, value);
		}
	}

	public bool CheckPathExists
	{
		get
		{
			return GetFlag(2048);
		}
		set
		{
			SetFlag(2048, value);
		}
	}

	public string DefaultExt
	{
		get
		{
			return m_defaultExt;
		}
		set
		{
			if (value != null)
			{
				m_defaultExt = value;
			}
			else
			{
				m_defaultExt = string.Empty;
			}
		}
	}

	public bool DereferenceLinks
	{
		get
		{
			return !GetFlag(1048576);
		}
		set
		{
			SetFlag(1048576, !value);
		}
	}

	public string FileName
	{
		get
		{
			return m_fileName;
		}
		set
		{
			if (value != null)
			{
				m_fileName = value;
			}
			else
			{
				m_fileName = string.Empty;
			}
		}
	}

	public string[] FileNames => m_fileNames;

	public string Filter
	{
		get
		{
			return m_filter;
		}
		set
		{
			if (value != null)
			{
				m_filter = value;
			}
			else
			{
				m_filter = string.Empty;
			}
		}
	}

	public int FilterIndex
	{
		get
		{
			return m_filterIndex;
		}
		set
		{
			m_filterIndex = value;
		}
	}

	public string ForcedInitialDirectory
	{
		get
		{
			return m_forcedInitialDir;
		}
		set
		{
			if (value != null)
			{
				m_forcedInitialDir = value;
			}
			else
			{
				m_forcedInitialDir = string.Empty;
			}
		}
	}

	public bool RestoreDirectory
	{
		get
		{
			return GetFlag(8);
		}
		set
		{
			SetFlag(8, value);
		}
	}

	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			if (value != null)
			{
				m_title = value;
			}
			else
			{
				m_title = string.Empty;
			}
		}
	}

	public bool ValidateNames
	{
		get
		{
			return !GetFlag(256);
		}
		set
		{
			SetFlag(256, !value);
		}
	}

	internal static IDictionary<string, string> FilterToLastUsedDirectory => s_filterToLastDir;

	public DialogResult ShowDialog()
	{
		return ShowDialog(null);
	}

	public DialogResult ShowDialog(IWin32Window owner)
	{
		return ShowNonCustomDialog(owner);
	}

	protected internal virtual DialogResult ShowNonCustomDialogInternal(FileDialog dialog, IWin32Window owner)
	{
		dialog.Filter = m_filter;
		dialog.FilterIndex = m_filterIndex;
		if (!string.IsNullOrEmpty(m_forcedInitialDir))
		{
			dialog.InitialDirectory = m_forcedInitialDir;
		}
		else
		{
			dialog.InitialDirectory = GetLastDirForFilter(m_filter);
		}
		dialog.FileName = m_fileName;
		dialog.Title = m_title;
		dialog.CheckFileExists = CheckFileExists;
		dialog.CheckPathExists = CheckPathExists;
		dialog.DereferenceLinks = DereferenceLinks;
		dialog.RestoreDirectory = RestoreDirectory;
		dialog.ValidateNames = ValidateNames;
		if (m_addExt)
		{
			dialog.DefaultExt = m_defaultExt;
		}
		DialogResult dialogResult = dialog.ShowDialog(owner);
		if (dialogResult == DialogResult.OK)
		{
			m_filterIndex = dialog.FilterIndex;
			if (dialog.FileNames.Length > 1)
			{
				m_fileNames = new string[dialog.FileNames.Length];
				for (int i = 0; i < dialog.FileNames.Length; i++)
				{
					m_fileNames[i] = dialog.FileNames[i];
				}
				m_fileName = m_fileNames[0];
			}
			else
			{
				m_fileName = dialog.FileName;
				m_fileNames = new string[1] { m_fileName };
			}
			SetLastDirForFilter(m_filter, m_fileName);
		}
		return dialogResult;
	}

	protected void SetFlag(int mask, bool value)
	{
		if (value)
		{
			m_flags |= mask;
		}
		else
		{
			m_flags &= ~mask;
		}
	}

	protected bool GetFlag(int mask)
	{
		return (m_flags & mask) != 0;
	}

	protected internal virtual DialogResult ShowNonCustomDialog(IWin32Window owner)
	{
		return DialogResult.Cancel;
	}

	private static void SetLastDirForFilter(string filter, string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				s_filterToLastDir[filter] = directoryName;
			}
		}
	}

	private static string GetLastDirForFilter(string filter)
	{
		s_filterToLastDir.TryGetValue(filter, out var value);
		return value;
	}
}
