using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace Sce.Atf;

internal static class DropDescriptionHelper
{
	[Flags]
	private enum DropDescriptionFlags
	{
		None = 0,
		IsDefault = 1,
		InvalidateRequired = 2
	}

	public static bool IsDropDescriptionDefault(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		DropDescriptionFlags dropDescriptionFlag = GetDropDescriptionFlag(dataObject);
		return (dropDescriptionFlag & DropDescriptionFlags.IsDefault) == DropDescriptionFlags.IsDefault;
	}

	public static bool InvalidateRequired(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		DropDescriptionFlags dropDescriptionFlag = GetDropDescriptionFlag(dataObject);
		return (dropDescriptionFlag & DropDescriptionFlags.InvalidateRequired) == DropDescriptionFlags.InvalidateRequired;
	}

	public static void SetDropDescriptionIsDefault(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, bool isDefault)
	{
		SetDropDescriptionFlag(dataObject, DropDescriptionFlags.IsDefault, isDefault);
	}

	public static void SetInvalidateRequired(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, bool required)
	{
		SetDropDescriptionFlag(dataObject, DropDescriptionFlags.InvalidateRequired, required);
	}

	private static void SetDropDescriptionFlag(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, DropDescriptionFlags flag, bool enabled)
	{
		DropDescriptionFlags dropDescriptionFlag = GetDropDescriptionFlag(dataObject);
		DropDescriptionFlags dropDescriptionFlags = (enabled ? (dropDescriptionFlag | flag) : ((dropDescriptionFlag | flag) ^ flag));
		if (dropDescriptionFlag != dropDescriptionFlags)
		{
			dataObject.SetData("DropDescriptionFlags", (int)dropDescriptionFlags);
		}
	}

	private static DropDescriptionFlags GetDropDescriptionFlag(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		if (dataObject.TryGetData<int>("DropDescriptionFlags", out var data))
		{
			return (DropDescriptionFlags)data;
		}
		return DropDescriptionFlags.None;
	}

	public static void SetDropDescription(System.Runtime.InteropServices.ComTypes.IDataObject data, DropImageType dragDropEffects, string format, string insert)
	{
		if (format != null && format.Length > 259)
		{
			throw new ArgumentException("Format string exceeds the maximum allowed length of 259.", "format");
		}
		if (insert != null && insert.Length > 259)
		{
			throw new ArgumentException("Insert string exceeds the maximum allowed length of 259.", "insert");
		}
		DropDescription data2 = default(DropDescription);
		data2.type = dragDropEffects;
		data2.szMessage = format;
		data2.szInsert = insert;
		data.SetData("DropDescription", data2);
	}

	public static object GetDropDescription(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		if (dataObject.TryGetData<DropDescription>("DropDescription", out var data))
		{
			return data;
		}
		return null;
	}

	public static DropImageType GetDropImageType(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		object dropDescription = GetDropDescription(dataObject);
		return (dropDescription is DropDescription) ? ((DropDescription)dropDescription).type : DropImageType.Invalid;
	}

	public static bool IsDropDescriptionValid(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		return GetDropImageType(dataObject) != DropImageType.Invalid;
	}

	public static void DefaultGiveFeedback(System.Runtime.InteropServices.ComTypes.IDataObject data, GiveFeedbackEventArgs e)
	{
		bool flag = false;
		bool flag2 = IsDropDescriptionDefault(data);
		DropImageType dropImageType = DropImageType.Invalid;
		if (!IsDropDescriptionValid(data) || flag2)
		{
			dropImageType = GetDropImageType(data);
			flag = true;
		}
		if (IsShowingLayered(data))
		{
			e.UseDefaultCursors = false;
			Cursor.Current = Cursors.Default;
		}
		else
		{
			e.UseDefaultCursors = true;
		}
		if (InvalidateRequired(data) || !flag2 || dropImageType != DropImageType.None)
		{
			if (data.TryGetData<IntPtr>("DragWindow", out var data2))
			{
				PostMessage(data2, 1027u, IntPtr.Zero, IntPtr.Zero);
			}
			SetInvalidateRequired(data, required: false);
		}
		if (flag && e.Effect != (DragDropEffects)dropImageType)
		{
			if (e.Effect != DragDropEffects.None)
			{
				SetDropDescription(data, (DropImageType)e.Effect, e.Effect.ToString(), null);
			}
			else
			{
				SetDropDescription(data, (DropImageType)e.Effect, e.Effect.ToString(), null);
			}
			SetDropDescriptionIsDefault(data, isDefault: true);
			SetInvalidateRequired(data, required: true);
		}
	}

	private static bool IsShowingLayered(System.Runtime.InteropServices.ComTypes.IDataObject dataObject)
	{
		if (dataObject.TryGetData<int>("IsShowingLayered", out var data))
		{
			return data != 0;
		}
		return false;
	}

	[DllImport("user32.dll")]
	private static extern void PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
