using System;
using System.IO;
using Sce.Atf.Applications;

internal static class Program
{
    private static int Main()
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log");

        PaintTimingLog.Write("old startup data");
        PaintTimingLog.Clear();
        PaintTimingLog.Write("fresh startup data");

        if (File.Exists(path) && File.ReadAllText(path).Contains("fresh startup data"))
        {
            Console.Error.WriteLine("FAIL: Write() synchronously persisted timing data.");
            return 1;
        }

        PaintTimingLog.Flush();

        string contents = File.ReadAllText(path);
        if (contents.Contains("old startup data"))
        {
            Console.Error.WriteLine("FAIL: old log contents remained after Clear().");
            return 1;
        }

        if (!contents.Contains("fresh startup data"))
        {
            Console.Error.WriteLine("FAIL: new log contents were not written after Clear().");
            return 1;
        }

        Console.WriteLine("PASS: PaintTimingLog buffers writes, flushes explicitly, and clears old data.");
        return 0;
    }
}
