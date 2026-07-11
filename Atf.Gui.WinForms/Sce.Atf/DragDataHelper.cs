using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace Sce.Atf;

public static class DragDataHelper
{
	public static void DragEnter(Control control, DragEventArgs e)
	{
		Point pt = new Point(e.X, e.Y);
		DragDropHelper.DragEnter(control.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref pt, (int)e.Effect);
	}

	public static void DragLeave()
	{
		DragDropHelper.DragLeave();
	}

	public static void DragOver(DragEventArgs e)
	{
		Point pt = new Point(e.X, e.Y);
		DragDropHelper.DragOver(ref pt, (int)e.Effect);
	}

	public static void Drop(DragEventArgs e)
	{
		Point pt = new Point(e.X, e.Y);
		DragDropHelper.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref pt, (int)e.Effect);
	}

	public static void SetDescription(this DragEventArgs e, string message, string insert)
	{
		DropDescriptionHelper.SetDropDescription((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, (DropImageType)e.Effect, message, insert);
	}

	public static void ClearDescription(this DragEventArgs e)
	{
		DropDescriptionHelper.SetDropDescription((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, DropImageType.Invalid, null, null);
	}

	public static void SetDescriptionIsDefault(this DragEventArgs e, bool value)
	{
		DropDescriptionHelper.SetDropDescriptionIsDefault((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, value);
	}

	public static void SetData<T>(this System.Windows.Forms.IDataObject dataObject, string format, T data) where T : struct
	{
		((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).SetData(format, data);
	}

	public static bool TryGetData<T>(this System.Windows.Forms.IDataObject dataObject, string format, out T data) where T : struct
	{
		return ((System.Runtime.InteropServices.ComTypes.IDataObject)dataObject).TryGetData<T>(format, out data);
	}
}
