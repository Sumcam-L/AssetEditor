using System;
using System.Diagnostics;

namespace Sce.Atf.Applications;

internal static class ResizeTrace
{
	public static bool IsResizing { get; set; }

	[Conditional("DEBUG")]
	public static void Log(string label)
	{
		Debug.WriteLine($"[Resize] {label}");
	}
}
