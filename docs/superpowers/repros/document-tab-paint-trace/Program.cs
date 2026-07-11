using System;
using System.IO;
using Sce.Atf.Applications;

internal static class Program
{
    private static readonly string s_logPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log");

    private static int Main()
    {
        PaintTimingLog.Clear();
        DocumentSwitchTrace.TraceTabStrip("refresh", "old=first,new=second");
        PaintTimingLog.Flush();
        if (File.Exists(s_logPath) && File.ReadAllText(s_logPath).Contains("TabStripTrace"))
            return Fail("tab-strip tracing wrote outside an active generation");

        long generation = DocumentSwitchTrace.Begin("tab-strip-repro");
        DocumentSwitchTrace.TraceTabStrip("refresh", "old=first,new=second,oldRect={X=0,Y=0,Width=80,Height=24},newRect={X=80,Y=0,Width=80,Height=24}");
        DocumentSwitchTrace.TraceTabStrip("paint-begin", "clip={X=80,Y=0,Width=80,Height=24}");
        DocumentSwitchTrace.TraceTabStrip("paint-end", "active=second");
        DocumentSwitchTrace.End(generation);
        PaintTimingLog.Flush();

        string log = File.ReadAllText(s_logPath);
        string[] expected = { "TabStripTrace", "phase=refresh", "old=first,new=second", "phase=paint-begin", "phase=paint-end" };
        foreach (string value in expected)
        {
            if (!log.Contains(value))
                return Fail("trace omitted " + value);
        }

        Console.WriteLine("PASS: document tab paint tracing is generation-gated and records refresh and paint boundaries.");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine("FAIL: " + message);
        return 1;
    }
}
