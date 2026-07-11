using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf;

internal static class OleConverter
{
	private static readonly byte[] s_serializedObjectId = new Guid("FD9EA796-3B13-4370-A679-56106BB288FB").ToByteArray();

	public const TYMED SupportedTymed = TYMED.TYMED_HGLOBAL | TYMED.TYMED_ISTREAM | TYMED.TYMED_GDI;

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GlobalLock(HandleRef handle);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool GlobalUnlock(HandleRef handle);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int GlobalSize(HandleRef handle);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GlobalFree(HandleRef handle);

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	public static extern int DragQueryFile(HandleRef hDrop, int iFile, StringBuilder lpszFile, int cch);

	public static object Convert(string format, ref STGMEDIUM medium)
	{
		if (medium.unionmember != IntPtr.Zero)
		{
			if (medium.tymed == TYMED.TYMED_HGLOBAL)
			{
				return ConvertHandle(format, new HandleRef(format, medium.unionmember));
			}
			if (medium.tymed == TYMED.TYMED_ISTREAM)
			{
				return ConvertStream(format, new HandleRef(format, medium.unionmember));
			}
			if (medium.tymed == TYMED.TYMED_GDI)
			{
				return ConvertBitmap(new HandleRef(format, medium.unionmember));
			}
		}
		return null;
	}

	public static FORMATETC CreateFormat(string format)
	{
		return new FORMATETC
		{
			tymed = (TYMED.TYMED_HGLOBAL | TYMED.TYMED_ISTREAM | TYMED.TYMED_GDI),
			cfFormat = (short)(ushort)DataFormats.GetFormat(format).Id,
			dwAspect = DVASPECT.DVASPECT_CONTENT,
			lindex = -1,
			ptd = IntPtr.Zero
		};
	}

	private static object ConvertHandle(string format, HandleRef data)
	{
		IntPtr source = GlobalLock(data);
		try
		{
			if (format.Equals(DataFormats.FileDrop))
			{
				return ReadFileListFromHandle(data);
			}
			int num = GlobalSize(data);
			byte[] array = new byte[num];
			Marshal.Copy(source, array, 0, num);
			if (IsSerializedObject(array))
			{
				int num2 = s_serializedObjectId.Length;
				using MemoryStream serializationStream = new MemoryStream(array, num2, array.Length - num2);
				BinaryFormatter binaryFormatter = new BinaryFormatter
				{
					AssemblyFormat = FormatterAssemblyStyle.Simple
				};
				return binaryFormatter.Deserialize(serializationStream);
			}
			if (format.Equals(DataFormats.Text) || format.Equals(DataFormats.Rtf) || format.Equals(DataFormats.OemText) || format.Equals("FileName"))
			{
				return Encoding.ASCII.GetString(array);
			}
			if (format.Equals(DataFormats.UnicodeText) || format.Equals("FileNameW"))
			{
				return Encoding.Unicode.GetString(array);
			}
			if (format.Equals(DataFormats.Html))
			{
				return Encoding.UTF8.GetString(array);
			}
			return new MemoryStream(array);
		}
		finally
		{
			GlobalUnlock(data);
			GlobalFree(data);
		}
	}

	private static string[] ReadFileListFromHandle(HandleRef handle)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = DragQueryFile(handle, -1, null, 0);
		if (num <= 0)
		{
			return null;
		}
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			int num2 = DragQueryFile(handle, i, stringBuilder, stringBuilder.Capacity);
			string text = stringBuilder.ToString();
			if (text.Length > num2)
			{
				text = text.Substring(0, num2);
			}
			array[i] = text;
		}
		return array;
	}

	private static object ConvertStream(string format, HandleRef data)
	{
		IStream stream = (IStream)Marshal.GetObjectForIUnknown(data.Handle);
		Marshal.Release(data.Handle);
		stream.Stat(out var pStatstg, 0);
		int num = (int)pStatstg.cbSize;
		IntPtr handle = GlobalAlloc(8258, num);
		HandleRef handleRef = new HandleRef(format, handle);
		IntPtr buf = GlobalLock(handleRef);
		stream.Read(buf, num);
		GlobalUnlock(handleRef);
		return ConvertHandle(format, handleRef);
	}

	private static object ConvertBitmap(HandleRef data)
	{
		using Image image = Image.FromHbitmap(data.Handle);
		return image.Clone();
	}

	private static bool IsSerializedObject(byte[] bytes)
	{
		if (bytes.Length <= s_serializedObjectId.Length)
		{
			return false;
		}
		for (int i = 0; i < s_serializedObjectId.Length; i++)
		{
			if (s_serializedObjectId[i] != bytes[i])
			{
				return false;
			}
		}
		return true;
	}
}
