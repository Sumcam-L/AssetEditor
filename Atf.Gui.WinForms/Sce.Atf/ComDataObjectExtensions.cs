using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Sce.Atf;

internal static class ComDataObjectExtensions
{
	public static void SetData<T>(this IDataObject dataObject, string format, T data) where T : struct
	{
		FORMATETC formatIn = OleConverter.CreateFormat(format);
		formatIn.tymed = TYMED.TYMED_HGLOBAL;
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
		try
		{
			Marshal.StructureToPtr((object)data, intPtr, false);
			STGMEDIUM medium = default(STGMEDIUM);
			medium.pUnkForRelease = null;
			medium.tymed = TYMED.TYMED_HGLOBAL;
			medium.unionmember = intPtr;
			dataObject.SetData(ref formatIn, ref medium, release: true);
		}
		catch
		{
			Marshal.FreeHGlobal(intPtr);
			throw;
		}
	}

	public static bool TryGetData<T>(this IDataObject dataObject, string format, out T data) where T : struct
	{
		FORMATETC format2 = OleConverter.CreateFormat(format);
		format2.tymed = TYMED.TYMED_HGLOBAL;
		if (dataObject.QueryGetData(ref format2) == 0)
		{
			dataObject.GetData(ref format2, out var medium);
			try
			{
				data = (T)Marshal.PtrToStructure(medium.unionmember, typeof(T));
				return true;
			}
			finally
			{
				ReleaseStgMedium(ref medium);
			}
		}
		data = default(T);
		return false;
	}

	[DllImport("ole32.dll")]
	private static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);
}
