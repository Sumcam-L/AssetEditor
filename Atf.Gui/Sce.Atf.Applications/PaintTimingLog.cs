using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Sce.Atf.Applications;

public static class PaintTimingLog
{
#if DEBUG
	private static readonly object s_lock = new object();
	private static readonly object s_flushLock = new object();
	private static readonly string s_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log");
	private static readonly StringBuilder s_buffer = new StringBuilder();
	private static readonly Timer s_flushTimer = new Timer(_ => Flush(), null, 2000, 2000);

	static PaintTimingLog()
	{
		AppDomain.CurrentDomain.ProcessExit += (_, __) =>
		{
			s_flushTimer.Dispose();
			Flush();
		};
	}
#endif

	public static void Clear()
	{
#if DEBUG
		try
		{
			lock (s_flushLock)
			{
				lock (s_lock)
				{
					s_buffer.Clear();
				}
				if (File.Exists(s_path))
				{
					File.Delete(s_path);
				}
			}
		}
		catch
		{
			// Timing diagnostics should never affect editor behavior.
		}
#endif
	}

	public static void Write(string format, params object[] args)
	{
#if DEBUG
		Write(args == null || args.Length == 0 ? format : string.Format(format, args));
#endif
	}

	public static void Write(string message)
	{
#if DEBUG
		try
		{
			string line = string.Format("{0:O} {1}{2}", DateTime.Now, message, Environment.NewLine);
			lock (s_lock)
			{
				s_buffer.Append(line);
			}
		}
		catch
		{
			// Timing diagnostics should never affect editor behavior.
		}
#endif
	}

	public static void Flush()
	{
#if DEBUG
		try
		{
			lock (s_flushLock)
			{
				string pending;
				lock (s_lock)
				{
					if (s_buffer.Length == 0)
					{
						return;
					}
					pending = s_buffer.ToString();
					s_buffer.Clear();
				}
				File.AppendAllText(s_path, pending);
			}
		}
		catch
		{
			// Timing diagnostics should never affect editor behavior.
		}
#endif
	}
}
