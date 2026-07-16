using System;
using System.IO;

internal static class Program
{
    private static int Main()
    {
        string sourcePath = Path.GetFullPath(Path.Combine(
            Environment.CurrentDirectory, "AssetEditor", "AssetEditor", "Program.cs"));
        string source = File.ReadAllText(sourcePath);

        if (!source.Contains("splash.ShowInTaskbar = true"))
        {
            Console.Error.WriteLine("FAIL: startup has no taskbar button while initialization is running.");
            return 1;
        }

        int shownIndex = source.IndexOf("assetEditorForm.Shown +=", StringComparison.Ordinal);
        int closeIndex = shownIndex >= 0 ? source.IndexOf("splash.Close();", shownIndex, StringComparison.Ordinal) : -1;
        int beginInvokeIndex = closeIndex >= 0 ? source.IndexOf("assetEditorForm.BeginInvoke", closeIndex, StringComparison.Ordinal) : -1;
        int runIndex = source.IndexOf("Application.Run(assetEditorForm)", StringComparison.Ordinal);

        if (shownIndex < 0 || closeIndex < shownIndex || beginInvokeIndex < closeIndex ||
            runIndex < beginInvokeIndex)
        {
            Console.Error.WriteLine("FAIL: splash does not hand off to a deferred taskbar registration after close.");
            return 1;
        }

        string shownHandler = source.Substring(shownIndex, runIndex - shownIndex);
        int registrationIndex = shownHandler.IndexOf("RefreshMainWindowTaskbarRegistration", StringComparison.Ordinal);
        if (registrationIndex < 0 ||
            shownHandler.IndexOf("RefreshMainWindowTaskbarRegistration", registrationIndex + 1, StringComparison.Ordinal) >= 0)
        {
            Console.Error.WriteLine("FAIL: Shown handler must perform exactly one taskbar registration.");
            return 1;
        }

        if (source.Contains("forceReregister") ||
            !source.Contains("WS_EX_APPWINDOW") ||
            !source.Contains("WS_EX_TOOLWINDOW") ||
            !source.Contains("SWP_FRAMECHANGED") ||
            source.Contains("form.ShowInTaskbar = false") ||
            source.Contains("form.ShowInTaskbar = true") ||
            source.Contains("SetForegroundWindow(") ||
            source.Contains("BringToFront()") ||
            source.Contains("assetEditorForm.Activate()") ||
            source.Contains("assetEditorForm.Focus()"))
        {
            Console.Error.WriteLine("FAIL: taskbar repair toggles ShowInTaskbar (causes flash) or uses focus stealing.");
            return 1;
        }

        Console.WriteLine("PASS: splash closes before deferred, non-flashing taskbar re-registration.");
        return 0;
    }
}
