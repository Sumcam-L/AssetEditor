using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;

internal static class Program
{
    private static readonly string s_logPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log");

    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        PaintTimingLog.Clear();

        using (var form = new Form())
        using (var panel = new RecreatingPanel())
        {
            form.Controls.Add(panel);
            if (panel.IsHandleCreated)
                return Fail("observer setup precondition created the target handle");

            IDisposable observer = DocumentSwitchTrace.Observe(
                panel,
                "logical",
                () => "doc=Test",
                () => true);

            try
            {
                if (panel.IsHandleCreated)
                    return Fail("observer creation created the target handle");

                form.Show();
                Application.DoEvents();
                PaintTimingLog.Flush();
                if (NativeEntries(ReadLog()).Any())
                    return Fail("native trace entries were written outside the gate");

                PaintTimingLog.Clear();
                long generation = DocumentSwitchTrace.Begin("repro");
                if (!DocumentSwitchTrace.IsActive)
                    return Fail("trace gate was not active after Begin");

                panel.Hide();
                panel.Show();
                panel.Invalidate();
                panel.Update();
                Application.DoEvents();
                DocumentSwitchTrace.End(generation);
                if (DocumentSwitchTrace.IsActive)
                    return Fail("trace gate remained active after End");

                PaintTimingLog.Flush();
                string[] entries = NativeEntries(ReadLog()).ToArray();
                int result = AssertSelectedMessagesAndMetadata(entries, panel.Handle);
                if (result != 0)
                    return result;

                IntPtr firstHandle = panel.Handle;
                panel.RecreateHandleForTest();
                Application.DoEvents();
                if (panel.Handle == firstHandle)
                    return Fail("target handle was not recreated");

                PaintTimingLog.Clear();
                generation = DocumentSwitchTrace.Begin("recreated");
                panel.Hide();
                panel.Show();
                Application.DoEvents();
                DocumentSwitchTrace.End(generation);
                PaintTimingLog.Flush();

                entries = NativeEntries(ReadLog()).ToArray();
                if (!entries.Any(line => ContainsHandle(line, panel.Handle)))
                    return Fail("trace did not use the recreated current HWND");
                if (entries.Any(line => ContainsHandle(line, firstHandle)))
                    return Fail("trace retained the stale pre-recreation HWND");

                observer.Dispose();
                observer = null;
                PaintTimingLog.Clear();
                generation = DocumentSwitchTrace.Begin("disposed");
                panel.Hide();
                panel.Show();
                Application.DoEvents();
                DocumentSwitchTrace.End(generation);
                PaintTimingLog.Flush();

                if (NativeEntries(ReadLog()).Any(line => line.Contains("role=logical")))
                    return Fail("disposed observer continued to write logical-control entries");
            }
            finally
            {
                observer?.Dispose();
            }
        }

        Console.WriteLine("PASS: native document-switch tracing honors its gate, message contract, handle lifecycle, and disposal.");
        return 0;
    }

    private static int AssertSelectedMessagesAndMetadata(string[] entries, IntPtr handle)
    {
        string[] selectedMessages =
        {
            "WM_ERASEBKGND",
            "WM_PAINT",
            "WM_SHOWWINDOW",
            "WM_WINDOWPOSCHANGING",
            "WM_WINDOWPOSCHANGED",
        };

        foreach (string message in selectedMessages)
        {
            if (!entries.Any(line => line.Contains("message=" + message)))
                return Fail("trace did not contain selected message " + message);
        }

        if (entries.Any(line => !selectedMessages.Any(message => line.Contains("message=" + message))))
            return Fail("trace contained a native message outside the selected set");

        string metadataEntry = entries.FirstOrDefault(line => ContainsHandle(line, handle));
        string[] requiredMetadata =
        {
            "role=logical",
            "type=",
            "hwnd=",
            "parentHwnd=",
            "visible=",
            "isHandleCreated=",
            "backColor=",
            "bounds=",
            "clientRectangle=",
            "doc=Test",
            "active=True",
        };

        if (metadataEntry == null)
            return Fail("trace did not contain the target HWND");
        foreach (string metadata in requiredMetadata)
        {
            if (!metadataEntry.Contains(metadata))
                return Fail("trace entry omitted metadata " + metadata);
        }

        return 0;
    }

    private static string ReadLog()
    {
        return File.Exists(s_logPath) ? File.ReadAllText(s_logPath) : string.Empty;
    }

    private static string[] NativeEntries(string contents)
    {
        return contents
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains("NativeSwitchTrace") && line.Contains("message="))
            .ToArray();
    }

    private static bool ContainsHandle(string line, IntPtr handle)
    {
        string hexadecimal = handle.ToInt64().ToString("X", CultureInfo.InvariantCulture);
        return line.IndexOf("hwnd=0x" + hexadecimal, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine("FAIL: " + message);
        return 1;
    }

    private sealed class RecreatingPanel : Panel
    {
        public void RecreateHandleForTest()
        {
            RecreateHandle();
        }
    }
}
