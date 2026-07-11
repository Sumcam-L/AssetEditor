using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Sce.Atf;

internal static class DragDropHelper
{
	[ComImport]
	[Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
	private class ComHelper
	{
	}

	private static readonly ComHelper s_helper = new ComHelper();

	public static void InitializeFromBitmap(IDataObject dataObject, Bitmap image, Point offset)
	{
		IntPtr hbitmap = image.GetHbitmap();
		ShDragImage dragImage = new ShDragImage
		{
			sizeDragImage = image.Size,
			ptOffset = offset,
			crColorKey = Color.Magenta.ToArgb(),
			hbmpDragImage = hbitmap
		};
		try
		{
			IDragSourceHelper2 dragSourceHelper = (IDragSourceHelper2)s_helper;
			dragSourceHelper.InitializeFromBitmap(ref dragImage, dataObject);
		}
		catch
		{
			DeleteObject(hbitmap);
		}
	}

	public static void InitializeFromWindow(IntPtr hwnd, ref Point pt, IDataObject dataObject)
	{
		IDragSourceHelper2 dragSourceHelper = (IDragSourceHelper2)s_helper;
		dragSourceHelper.InitializeFromWindow(hwnd, ref pt, dataObject);
	}

	public static void SetFlags(int dwFlags)
	{
		IDragSourceHelper2 dragSourceHelper = (IDragSourceHelper2)s_helper;
		dragSourceHelper.SetFlags(dwFlags);
	}

	[DllImport("gdiplus.dll")]
	private static extern bool DeleteObject(IntPtr hgdi);

	public static void DragEnter(IntPtr hwndTarget, IDataObject dataObject, ref Point pt, int effect)
	{
		IDropTargetHelper dropTargetHelper = (IDropTargetHelper)s_helper;
		dropTargetHelper.DragEnter(hwndTarget, dataObject, ref pt, effect);
	}

	public static void DragLeave()
	{
		IDropTargetHelper dropTargetHelper = (IDropTargetHelper)s_helper;
		dropTargetHelper.DragLeave();
	}

	public static void DragOver(ref Point pt, int effect)
	{
		IDropTargetHelper dropTargetHelper = (IDropTargetHelper)s_helper;
		dropTargetHelper.DragOver(ref pt, effect);
	}

	public static void Drop(IDataObject dataObject, ref Point pt, int effect)
	{
		IDropTargetHelper dropTargetHelper = (IDropTargetHelper)s_helper;
		dropTargetHelper.Drop(dataObject, ref pt, effect);
	}

	public static void Show(bool show)
	{
		IDropTargetHelper dropTargetHelper = (IDropTargetHelper)s_helper;
		dropTargetHelper.Show(show);
	}
}
