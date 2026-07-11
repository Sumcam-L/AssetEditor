using System.Drawing;
using System.Runtime.InteropServices;

namespace Sce.Atf;

public static class FileIconUtil
{
	public enum IconSize
	{
		Large,
		Small
	}

	public enum FolderType
	{
		Open,
		Closed
	}

	public static Icon GetFileIcon(string name, IconSize size, bool linkOverlay)
	{
		Shell32.SHFILEINFO psfi = default(Shell32.SHFILEINFO);
		uint num = 272u;
		if (linkOverlay)
		{
			num += 32768;
		}
		num = ((IconSize.Small != size) ? num : (num + 1));
		Shell32.SHGetFileInfo(name, 128u, ref psfi, (uint)Marshal.SizeOf((object)psfi), num);
		Icon result = (Icon)Icon.FromHandle(psfi.hIcon).Clone();
		User32.DestroyIcon(psfi.hIcon);
		return result;
	}

	public static Icon GetFolderIcon(IconSize size, FolderType folderType)
	{
		uint num = 272u;
		if (folderType == FolderType.Open)
		{
			num += 2;
		}
		num = ((IconSize.Small != size) ? num : (num + 1));
		Shell32.SHFILEINFO psfi = default(Shell32.SHFILEINFO);
		Shell32.SHGetFileInfo(null, 16u, ref psfi, (uint)Marshal.SizeOf((object)psfi), num);
		Icon.FromHandle(psfi.hIcon);
		Icon result = (Icon)Icon.FromHandle(psfi.hIcon).Clone();
		User32.DestroyIcon(psfi.hIcon);
		return result;
	}
}
