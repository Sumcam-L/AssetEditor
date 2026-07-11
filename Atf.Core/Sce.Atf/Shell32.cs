using System;
using System.Runtime.InteropServices;

namespace Sce.Atf;

public static class Shell32
{
	public struct SHITEMID
	{
		public ushort cb;

		[MarshalAs(UnmanagedType.LPArray)]
		public byte[] abID;
	}

	public struct ITEMIDLIST
	{
		public SHITEMID mkid;
	}

	public struct BROWSEINFO
	{
		public IntPtr hwndOwner;

		public IntPtr pidlRoot;

		public IntPtr pszDisplayName;

		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpszTitle;

		public uint ulFlags;

		public IntPtr lpfn;

		public int lParam;

		public IntPtr iImage;
	}

	public struct SHFILEINFO
	{
		public const int NAMESIZE = 80;

		public IntPtr hIcon;

		public int iIcon;

		public uint dwAttributes;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDisplayName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	}

	public const int MAX_PATH = 260;

	public const uint BIF_RETURNONLYFSDIRS = 1u;

	public const uint BIF_DONTGOBELOWDOMAIN = 2u;

	public const uint BIF_STATUSTEXT = 4u;

	public const uint BIF_RETURNFSANCESTORS = 8u;

	public const uint BIF_EDITBOX = 16u;

	public const uint BIF_VALIDATE = 32u;

	public const uint BIF_NEWDIALOGSTYLE = 64u;

	public const uint BIF_USENEWUI = 80u;

	public const uint BIF_BROWSEINCLUDEURLS = 128u;

	public const uint BIF_BROWSEFORCOMPUTER = 4096u;

	public const uint BIF_BROWSEFORPRINTER = 8192u;

	public const uint BIF_BROWSEINCLUDEFILES = 16384u;

	public const uint BIF_SHAREABLE = 32768u;

	public const uint SHGFI_ICON = 256u;

	public const uint SHGFI_DISPLAYNAME = 512u;

	public const uint SHGFI_TYPENAME = 1024u;

	public const uint SHGFI_ATTRIBUTES = 2048u;

	public const uint SHGFI_ICONLOCATION = 4096u;

	public const uint SHGFI_EXETYPE = 8192u;

	public const uint SHGFI_SYSICONINDEX = 16384u;

	public const uint SHGFI_LINKOVERLAY = 32768u;

	public const uint SHGFI_SELECTED = 65536u;

	public const uint SHGFI_ATTR_SPECIFIED = 131072u;

	public const uint SHGFI_LARGEICON = 0u;

	public const uint SHGFI_SMALLICON = 1u;

	public const uint SHGFI_OPENICON = 2u;

	public const uint SHGFI_SHELLICONSIZE = 4u;

	public const uint SHGFI_PIDL = 8u;

	public const uint SHGFI_USEFILEATTRIBUTES = 16u;

	public const uint SHGFI_ADDOVERLAYS = 32u;

	public const uint SHGFI_OVERLAYINDEX = 64u;

	public const uint FILE_ATTRIBUTE_DIRECTORY = 16u;

	public const uint FILE_ATTRIBUTE_NORMAL = 128u;

	[DllImport("Shell32.dll", CharSet = CharSet.Ansi)]
	public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);
}
