using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Firaxis.Collections;

namespace Firaxis.Error;

public static class ExceptionLogger
{
	public class ExceptionEntry
	{
		public static string sSeparator = "--------------------------------------------------------------------------------";

		public Exception Exception { get; private set; }

		public DateTime Time { get; private set; }

		public string Caption { get; private set; }

		public string Note { get; private set; }

		public MethodBase Source { get; private set; }

		public OperationResultLevel Level { get; private set; }

		public ExceptionEntry(Exception e, string caption, string note, MethodBase source, OperationResultLevel level)
		{
			Exception = e;
			Time = DateTime.Now;
			Note = note ?? string.Empty;
			Level = level;
			Caption = caption ?? string.Empty;
			Source = source;
		}

		public override string ToString()
		{
			string caption = Caption;
			string text = $"[{Time.Year:D4}-{Time.Month:D2}-{Time.Day:D2} {Time.Hour:D2}:{Time.Minute:D2}:{Time.Second:D2}]";
			string text2 = string.Empty;
			if (Source != null && Source.DeclaringType != null && Source.DeclaringType.Name != null && Source.Name != null)
			{
				text2 = Source.DeclaringType.Name.Remove(0, Source.DeclaringType.Name.LastIndexOf(".") + 1) + (Source.Name.StartsWith(".") ? Source.Name : ("." + Source.Name)) + "()";
			}
			string text3 = ((Level == OperationResultLevel.None || Level == OperationResultLevel.Always) ? $"\r\n{text} {text2}\r\n{caption}" : $"\r\n{sSeparator}\r\n{text} {text2}\r\n{Level.ToString().ToUpperInvariant()}: {caption}\r\n{sSeparator}");
			if (!string.IsNullOrEmpty(Note))
			{
				text3 = text3 + "\r\nNote: " + Note;
			}
			if (Exception != null)
			{
				text3 = text3 + "\r\n" + ErrorHandling.GetExceptionReportText(Exception).Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
			}
			return text3;
		}
	}

	public static string LogFile { get; set; }

	public static OperationResultLevel LogToConsole { get; set; }

	public static ListEvent<ExceptionEntry> ExceptionLog { get; private set; }

	static ExceptionLogger()
	{
		ExceptionLog = new ListEvent<ExceptionEntry>();
		Application.ApplicationExit += OnApplicationExit;
	}

	public static void Log(string caption, string note)
	{
		Log(null, caption, note, OperationResultLevel.None);
	}

	public static void Log(string caption, string note, OperationResultLevel level)
	{
		Log(null, caption, note, level);
	}

	public static void Log(bool condition, string caption, string note, bool throwException)
	{
		if (!condition)
		{
			Log(null, caption, note, throwException ? OperationResultLevel.Error : OperationResultLevel.Warning);
			if (throwException)
			{
				throw new Exception(caption);
			}
		}
	}

	public static void Log(Exception e)
	{
		Log(e, e.GetType().ToString(), "", OperationResultLevel.Error);
	}

	public static void Log(Exception e, string note)
	{
		Log(e, e.GetType().ToString(), note, OperationResultLevel.Error);
	}

	public static void Log(Exception e, string caption, string note, OperationResultLevel level)
	{
		MethodBase methodBase = null;
		if (e != null)
		{
			methodBase = e.TargetSite;
		}
		else
		{
			StackFrame stackFrame = new StackFrame(1);
			methodBase = stackFrame.GetMethod();
			if (methodBase.DeclaringType == typeof(ExceptionLogger))
			{
				stackFrame = new StackFrame(2);
				methodBase = stackFrame.GetMethod();
				if (methodBase.DeclaringType == typeof(ExceptionLogger))
				{
					methodBase = null;
				}
			}
		}
		ExceptionEntry exceptionEntry = new ExceptionEntry(e, caption, note, methodBase, level);
		ExceptionLog.Add(exceptionEntry);
		if (level >= LogToConsole)
		{
			Console.WriteLine(exceptionEntry.ToString());
		}
		if (e != null && e.InnerException != null)
		{
			Log(e.InnerException, caption + ": Inner Exception", note, level);
		}
	}

	public static bool ToFile()
	{
		if (!string.IsNullOrEmpty(LogFile))
		{
			return ToFile(LogFile);
		}
		return false;
	}

	public static bool ToFile(string sFilename)
	{
		try
		{
			using (StreamWriter streamWriter = new StreamWriter(sFilename))
			{
				streamWriter.Write(ErrorHandling.GetErrorReportHeader().Replace("\n", "\r\n") + "\r\n");
				foreach (ExceptionEntry item in ExceptionLog)
				{
					streamWriter.Write(item.ToString());
				}
				if (ExceptionLog.Count > 0)
				{
					streamWriter.Write("\r\n" + ExceptionEntry.sSeparator);
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void OnApplicationExit(object sender, EventArgs e)
	{
		ToFile();
	}

	public static void FatalExit(int exitCode)
	{
		ToFile();
		Environment.Exit(exitCode);
	}

	public static void FatalExit(int exitCode, string note, string caption)
	{
		Log(caption ?? string.Empty, note ?? string.Empty);
		FatalExit(exitCode);
	}
}
