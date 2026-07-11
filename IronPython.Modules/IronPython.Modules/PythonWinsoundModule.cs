using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonWinsoundModule
{
	public const int SND_SYNC = 0;

	public const int SND_ASYNC = 1;

	public const int SND_NODEFAULT = 2;

	public const int SND_MEMORY = 4;

	public const int SND_LOOP = 8;

	public const int SND_NOSTOP = 16;

	public const int SND_NOWAIT = 8192;

	public const int SND_ALIAS = 65536;

	public const int SND_ALIAS_ID = 1114112;

	public const int SND_FILENAME = 131072;

	public const int SND_RESOURCE = 262148;

	public const int SND_PURGE = 64;

	public const int SND_APPLICATION = 128;

	public const int MB_OK = 0;

	public const int MB_ICONASTERISK = 64;

	public const int MB_ICONEXCLAMATION = 48;

	public const int MB_ICONHAND = 16;

	public const int MB_ICONQUESTION = 32;

	public static readonly string __doc__ = "PlaySound(sound, flags) - play a sound\r\nSND_FILENAME - sound is a wav file name\r\nSND_ALIAS - sound is a registry sound association name\r\nSND_LOOP - Play the sound repeatedly; must also specify SND_ASYNC\r\nSND_MEMORY - sound is a memory image of a wav file\r\nSND_PURGE - stop all instances of the specified sound\r\nSND_ASYNC - PlaySound returns immediately\r\nSND_NODEFAULT - Do not play a default beep if the sound can not be found\r\nSND_NOSTOP - Do not interrupt any sounds currently playing\r\nSND_NOWAIT - Return immediately if the sound driver is busy\r\n\r\nBeep(frequency, duration) - Make a beep through the PC speaker.";

	[DllImport("winmm.dll")]
	private static extern bool PlaySound(string fileName, IntPtr hMod, int flags);

	[DllImport("winmm.dll")]
	private static extern bool PlaySound(byte[] bytes, IntPtr hMod, int flags);

	[DllImport("winmm.dll")]
	private static extern bool PlaySound(IntPtr input, IntPtr hMod, int flags);

	[DllImport("kernel32.dll")]
	private static extern bool Beep(int dwFreq, int dwDuration);

	[DllImport("user32.dll")]
	private static extern bool MessageBeep(int uType);

	[Documentation("PlaySound(sound, flags) - a wrapper around the Windows PlaySound API\r\n\r\nThe sound argument can be a filename, data, or None.\r\nFor flag values, ored together, see module documentation.")]
	public static void PlaySound(CodeContext context, [NotNull] string sound, int flags)
	{
		if ((flags & 1) == 1 && (flags & 4) == 4)
		{
			throw PythonOps.RuntimeError("Cannot play asynchronously from memory");
		}
		if (!PlaySound(sound, IntPtr.Zero, flags))
		{
			throw PythonOps.RuntimeError("Failed to play sound");
		}
	}

	[Documentation("PlaySound(sound, flags) - a wrapper around the Windows PlaySound API\r\n\r\nThe sound argument can be a filename, data, or None.\r\nFor flag values, ored together, see module documentation.")]
	public static void PlaySound(CodeContext context, [NotNull] IList<byte> sound, int flags)
	{
		if ((flags & 1) == 1 && (flags & 4) == 4)
		{
			throw PythonOps.RuntimeError("Cannot play asynchronously from memory");
		}
		if (!PlaySound(sound.ToArray(), IntPtr.Zero, flags))
		{
			throw PythonOps.RuntimeError("Failed to play sound");
		}
	}

	[Documentation("PlaySound(sound, flags) - a wrapper around the Windows PlaySound API\r\n\r\nThe sound argument can be a filename, data, or None.\r\nFor flag values, ored together, see module documentation.")]
	public static void PlaySound(CodeContext context, object sound, int flags)
	{
		bool flag = false;
		if ((flags & 1) == 1 && (flags & 4) == 4)
		{
			throw PythonOps.RuntimeError("Cannot play asynchronously from memory");
		}
		if (sound == null)
		{
			flag = PlaySound(IntPtr.Zero, IntPtr.Zero, flags);
		}
		else if (sound is string)
		{
			flag = PlaySound((string)sound, IntPtr.Zero, flags);
		}
		else
		{
			if (!(sound is IList<byte>))
			{
				throw PythonOps.RuntimeError("Failed to play sound");
			}
			flag = PlaySound(((IList<byte>)sound).ToArray(), IntPtr.Zero, flags);
		}
		if (!flag)
		{
			throw PythonOps.RuntimeError("Failed to play sound");
		}
	}

	[Documentation("Beep(frequency, duration) - a wrapper around the Windows Beep API\r\n\r\nThe frequency argument specifies frequency, in hertz, of the sound.\r\nThis parameter must be in the range 37 through 32,767.\r\nThe duration argument specifies the number of milliseconds.\r\n")]
	public static void Beep(CodeContext context, int freq, int dur)
	{
		if (freq < 37 || freq > 32767)
		{
			throw PythonOps.ValueError("frequency must be in 37 thru 32767");
		}
		if (!Beep(freq, dur))
		{
			throw PythonOps.RuntimeError("Failed to beep");
		}
	}

	[Documentation("MessageBeep(x) - call Windows MessageBeep(x). x defaults to MB_OK.")]
	public static void MessageBeep(CodeContext context, [DefaultParameterValue(0)] int x)
	{
		MessageBeep(x);
	}
}
