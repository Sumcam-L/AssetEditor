using System;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

[PythonType("msvcrt")]
public class PythonMsvcrt
{
	public const string __doc__ = "Functions from the Microsoft Visual C Runtime.";

	private static int EOF = -1;

	private static ushort WEOF = ushort.MaxValue;

	[Documentation("heapmin() -> None\r\n\r\nForce the malloc() heap to clean itself up and return unused blocks\r\nto the operating system. On failure, this raises IOError.")]
	public static void heapmin()
	{
		if (_heapmin() != 0)
		{
			throw PythonOps.IOError(new Win32Exception());
		}
	}

	[Documentation("open_osfhandle(handle, flags) -> file descriptor\r\n\r\nCreate a C runtime file descriptor from the file handle handle. The\r\nflags parameter should be a bitwise OR of os.O_APPEND, os.O_RDONLY,\r\nand os.O_TEXT. The returned file descriptor may be used as a parameter\r\nto os.fdopen() to create a file object.\r\n")]
	public static int open_osfhandle(CodeContext context, BigInteger os_handle, int arg1)
	{
		FileStream o = new FileStream(new IntPtr((long)os_handle), FileAccess.ReadWrite, ownsHandle: true);
		return context.LanguageContext.FileManager.AddToStrongMapping(o);
	}

	[Documentation("get_osfhandle(fd) -> file handle\r\n\r\nReturn the file handle for the file descriptor fd. Raises IOError\r\nif fd is not recognized.")]
	public static object get_osfhandle(CodeContext context, int fd)
	{
		PythonFile fileFromId = context.LanguageContext.FileManager.GetFileFromId(context.LanguageContext, fd);
		if (fileFromId._stream is FileStream fileStream)
		{
			return fileStream.Handle.ToPython();
		}
		return -1;
	}

	[Documentation("setmode(fd, mode) -> Previous mode\r\n\r\nSet the line-end translation mode for the file descriptor fd. To set\r\nit to text mode, flags should be os.O_TEXT; for binary, it should be\r\nos.O_BINARY.")]
	public static int setmode(CodeContext context, int fd, int flags)
	{
		PythonFile fileFromId = context.LanguageContext.FileManager.GetFileFromId(context.LanguageContext, fd);
		return flags switch
		{
			16384 => fileFromId.SetMode(context, text: true) ? 16384 : 32768, 
			32768 => fileFromId.SetMode(context, text: false) ? 16384 : 32768, 
			_ => throw PythonOps.ValueError("unknown mode: {0}", flags), 
		};
	}

	[Documentation("kbhit() -> bool\r\n\r\nReturn true if a keypress is waiting to be read.")]
	public static bool kbhit()
	{
		if (_kbhit() != 0)
		{
			return true;
		}
		return false;
	}

	[Documentation("getch() -> key character\r\n\r\nRead a keypress and return the resulting character. Nothing is echoed to\r\nthe console. This call will block if a keypress is not already\r\navailable, but will not wait for Enter to be pressed. If the pressed key\r\nwas a special function key, this will return '\\\\000' or '\\\\xe0'; the next\r\ncall will return the keycode. The Control-C keypress cannot be read with\r\nthis function.")]
	public static string getch()
	{
		return new string((char)_getch(), 1);
	}

	[Documentation("getwch() -> Unicode key character\r\n\r\nWide char variant of getch(), returning a Unicode value.")]
	public static string getwch()
	{
		return new string((char)_getwch(), 1);
	}

	[Documentation("getche() -> key character\r\n\r\nSimilar to getch(), but the keypress will be echoed if it represents\r\na printable character.")]
	public static string getche()
	{
		return new string((char)_getche(), 1);
	}

	[Documentation("getwche() -> Unicode key character\r\n\r\nWide char variant of getche(), returning a Unicode value.")]
	public static string getwche()
	{
		return new string((char)_getwche(), 1);
	}

	[Documentation("putch(char) -> None\r\n\r\nPrint the character char to the console without buffering.")]
	public static void putch(char @char)
	{
		_putch(@char);
	}

	[Documentation("putwch(unicode_char) -> None\r\n\r\nWide char variant of putch(), accepting a Unicode value.")]
	public static void putwch(char @char)
	{
		_putwch(@char);
	}

	[Documentation("ungetch(char) -> None\r\n\r\nCause the character char to be \"pushed back\" into the console buffer;\r\nit will be the next character read by getch() or getche().")]
	public static void ungetch(char @char)
	{
		if (_ungetch(@char) == EOF)
		{
			throw PythonOps.IOError(new Win32Exception());
		}
	}

	[Documentation("ungetwch(unicode_char) -> None\r\n\r\nWide char variant of ungetch(), accepting a Unicode value.")]
	public static void ungetwch(char @char)
	{
		if (_ungetwch(@char) == WEOF)
		{
			throw PythonOps.IOError(new Win32Exception());
		}
	}

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	private static extern int _heapmin();

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl)]
	private static extern int _kbhit();

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl)]
	private static extern int _getch();

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl)]
	private static extern int _getche();

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl)]
	private static extern int _putch(int c);

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	private static extern int _ungetch(int c);

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl)]
	private static extern ushort _getwch();

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl)]
	private static extern ushort _getwche();

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl)]
	private static extern ushort _putwch(char c);

	[DllImport("msvcr100", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	private static extern ushort _ungetwch(ushort c);
}
