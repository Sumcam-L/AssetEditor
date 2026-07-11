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

        int addTabIndex = source.IndexOf("RegisterMainWindowTaskbarButton(assetEditorForm)", StringComparison.Ordinal);
        int closeIndex = source.LastIndexOf("splash.Close();", StringComparison.Ordinal);
        int runIndex = source.IndexOf("Application.Run(assetEditorForm)", StringComparison.Ordinal);
        if (addTabIndex < 0)
        {
            Console.Error.WriteLine("FAIL: main window is not explicitly registered with the Windows taskbar.");
            return 1;
        }

        if (closeIndex < addTabIndex || runIndex < closeIndex)
        {
            Console.Error.WriteLine("FAIL: splash closes before the main taskbar button is registered.");
            return 1;
        }

        if (source.Contains("assetEditorForm.ShowInTaskbar = false") ||
            source.Contains("RefreshMainWindowTaskbarRegistration(assetEditorForm"))
        {
            Console.Error.WriteLine("FAIL: startup repairs taskbar state after the main form is visible.");
            return 1;
        }

        Console.WriteLine("PASS: main taskbar button is registered before the splash taskbar button closes.");
        return 0;
    }
}
