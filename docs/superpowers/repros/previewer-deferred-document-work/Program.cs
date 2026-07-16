using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Firaxis.AssetPreviewing;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        using (var form = new Form())
        {
            form.Show();
            Application.DoEvents();

            var queue = new DeferredPreviewDocumentQueue<string>();
            var executed = new List<string>();

            if (queue.Enqueue(null, "null-dispatcher", executed.Add))
                return Fail("null dispatcher accepted work");
            Application.DoEvents();
            if (executed.Count != 0 || queue.PendingItems.Any())
                return Fail("null dispatcher queued or executed work");

            using (var disposedDispatcher = new Form())
            {
                disposedDispatcher.Show();
                Application.DoEvents();
                disposedDispatcher.Dispose();
                if (queue.Enqueue(disposedDispatcher, "disposed-dispatcher", executed.Add))
                    return Fail("disposed dispatcher accepted work");
            }
            Application.DoEvents();
            if (executed.Count != 0 || queue.PendingItems.Any())
                return Fail("disposed dispatcher queued or executed work");

            queue.Enqueue(form, "first", executed.Add);
            queue.Enqueue(form, "first", executed.Add);
            if (executed.Count != 0 || queue.PendingItems.Count() != 1)
                return Fail("work did not remain deferred or duplicate work was queued");

            if (!queue.Cancel("first"))
                return Fail("pending work could not be canceled");
            Application.DoEvents();
            if (executed.Count != 0)
                return Fail("canceled work executed");

            queue.Enqueue(form, "second", executed.Add);
            Application.DoEvents();
            if (!executed.SequenceEqual(new[] { "second" }))
                return Fail("deferred work did not execute exactly once");

            queue.Enqueue(form, "third", executed.Add);
            queue.Clear();
            Application.DoEvents();
            if (executed.Count != 1 || queue.PendingItems.Any())
                return Fail("cleared work executed or remained pending");
        }

        Console.WriteLine("PASS: preview document work is deferred, deduplicated, and cancellable.");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine("FAIL: " + message);
        return 1;
    }
}
