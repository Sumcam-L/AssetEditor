using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

internal sealed class DocumentHostControl : UserControl
{
    public DocumentHostControl(Control logicalControl)
    {
        LogicalControl = logicalControl ?? throw new ArgumentNullException(nameof(logicalControl));
        Dock = DockStyle.Fill;
        Tag = logicalControl.Tag;
    }

    public Control LogicalControl { get; }

    public bool HasAttachedLogicalControl => !LogicalControl.IsDisposed && LogicalControl.Parent == this;

    public bool AttachLogicalControl()
    {
        if (LogicalControl.IsDisposed || HasAttachedLogicalControl)
        {
            return false;
        }

        bool hadHandle = LogicalControl.IsHandleCreated;
        var total = Stopwatch.StartNew();
        var phase = Stopwatch.StartNew();
        LogicalControl.Visible = false;
        long hide = phase.ElapsedMilliseconds;

        phase.Restart();
        if (LogicalControl.Parent != null)
        {
            LogicalControl.Parent.Controls.Remove(LogicalControl);
        }
        long remove = phase.ElapsedMilliseconds;

        phase.Restart();
        LogicalControl.Dock = DockStyle.Fill;
        long dock = phase.ElapsedMilliseconds;

        phase.Restart();
        Controls.Add(LogicalControl);
        long add = phase.ElapsedMilliseconds;

        phase.Restart();
        LogicalControl.Visible = true;
        long show = phase.ElapsedMilliseconds;

        phase.Restart();
        LogicalControl.BringToFront();
        long front = phase.ElapsedMilliseconds;
        total.Stop();
        PaintTimingLog.Write(
            "DocumentHostAttach: control={0}, total={1}ms, hide={2}ms, remove={3}ms, dock={4}ms, add={5}ms, show={6}ms, front={7}ms, handle={8}->{9}",
            LogicalControl.GetType().Name, total.ElapsedMilliseconds, hide, remove, dock, add, show, front, hadHandle, LogicalControl.IsHandleCreated);
        return true;
    }

    public void DetachLogicalControl()
    {
        if (!HasAttachedLogicalControl)
        {
            return;
        }

        LogicalControl.Visible = false;
        Controls.Remove(LogicalControl);
    }
}
