using System;
using System.Diagnostics;
using System.IO;

namespace WeifenLuo.WinFormsUI.Docking;

internal static class DockTrace
{
	private static readonly object s_lock = new object();

	private static int s_count;

	private const int MaxEntries = 10000;

	public static bool ShouldTrace(IDockContent content)
	{
		if (s_count >= MaxEntries)
		{
			return false;
		}
		string name = GetName(content);
		return name == "Entity"
			|| name == "Global"
			|| name == "Properties"
			|| name == "Cook Params"
			|| name == "Geometries"
			|| name == "Attachments"
			|| name == "Animations"
			|| name == "Particles"
			|| name == "Behaviors"
			|| name == "Splines";
	}

	public static void Write(IDockContent content, string message, params object[] args)
	{
		if (!ShouldTrace(content))
		{
			return;
		}
		try
		{
			s_count++;
			object[] values = new object[(args?.Length ?? 0) + 1];
			values[0] = GetName(content);
			if (args != null)
			{
				Array.Copy(args, 0, values, 1, args.Length);
			}
			WriteLine(string.Format("DockTrace: content={0} " + message, values));
		}
		catch
		{
		}
	}

	public static void WriteUnknownTransition(IDockContent content, DockState oldVisibleState, DockState newVisibleState, bool oldHidden, bool newHidden)
	{
		string name = GetName(content);
		if (!ShouldTrace(content) || newVisibleState != DockState.Unknown)
		{
			return;
		}
		try
		{
			s_count++;
			WriteLine(string.Format("DockTraceStack: content={0} hidden={1}->{2}, visible={3}->{4}, stack={5}", name, oldHidden, newHidden, oldVisibleState, newVisibleState, CompactStackTrace()));
		}
		catch
		{
		}
	}

	public static void WritePane(DockPane pane, string message, params object[] args)
	{
		if (!ShouldTrace(pane?.ActiveContent) && !HasTraceContent(pane))
		{
			return;
		}
		try
		{
			s_count++;
			WriteLine(string.Format("DockTrace: pane=" + GetPaneLabel(pane) + " " + message, args ?? Array.Empty<object>()));
		}
		catch
		{
		}
	}

	private static bool HasTraceContent(DockPane pane)
	{
		if (pane == null)
		{
			return false;
		}
		foreach (IDockContent content in pane.Contents)
		{
			if (ShouldTrace(content))
			{
				return true;
			}
		}
		return false;
	}

	private static string GetName(IDockContent content)
	{
		return content?.DockHandler?.Form?.Name ?? content?.DockHandler?.Form?.Text ?? "null";
	}

	private static string GetPaneLabel(DockPane pane)
	{
		if (pane == null)
		{
			return "null";
		}
		return string.Format("{0:X}:{1}:active={2}:displaying={3}:contents={4}", pane.GetHashCode(), pane.DockState, GetName(pane.ActiveContent), pane.DisplayingContents.Count, pane.Contents.Count);
	}

	private static string CompactStackTrace()
	{
		StackTrace stackTrace = new StackTrace(skipFrames: 2, fNeedFileInfo: false);
		var frames = stackTrace.GetFrames();
		if (frames == null)
		{
			return "null";
		}
		int count = Math.Min(frames.Length, 12);
		string[] parts = new string[count];
		for (int i = 0; i < count; i++)
		{
			var method = frames[i].GetMethod();
			parts[i] = string.Format("{0}.{1}", method?.DeclaringType?.Name ?? "null", method?.Name ?? "null");
		}
		return string.Join(" <- ", parts);
	}

	private static void WriteLine(string message)
	{
		try
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log");
			string line = string.Format("{0:O} {1}{2}", DateTime.Now, message, Environment.NewLine);
			lock (s_lock)
			{
				File.AppendAllText(path, line);
			}
		}
		catch
		{
		}
	}
}
