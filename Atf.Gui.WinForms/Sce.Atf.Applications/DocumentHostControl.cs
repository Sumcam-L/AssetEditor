using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

internal sealed class DocumentHostControl : UserControl
{
	private readonly IDisposable m_logicalControlObserver;

    public DocumentHostControl(Control logicalControl)
    {
        LogicalControl = logicalControl ?? throw new ArgumentNullException(nameof(logicalControl));
        Dock = DockStyle.Fill;
        Tag = logicalControl.Tag;
		BackColor = logicalControl.BackColor;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		m_logicalControlObserver = DocumentSwitchTrace.Observe(
			LogicalControl,
			"logical-control",
			() => "document=" + GetTraceIdentity(LogicalControl),
			() => HasAttachedLogicalControl && Parent != null && Parent.Visible);
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
        if (LogicalControl.Parent != null && LogicalControl.Parent != this)
        {
            LogicalControl.Parent.Controls.Remove(LogicalControl);
        }
        long remove = phase.ElapsedMilliseconds;

        phase.Restart();
        LogicalControl.Dock = DockStyle.Fill;
        long dock = phase.ElapsedMilliseconds;

        phase.Restart();
        SuspendLayout();
        try
        {
            if (!Controls.Contains(LogicalControl))
            {
                Controls.Add(LogicalControl);
            }
            LogicalControl.Visible = true;
            LogicalControl.BringToFront();
        }
        finally
        {
            ResumeLayout(true);
        }
        long show = phase.ElapsedMilliseconds;
        total.Stop();
        PaintTimingLog.Write(
            "DocumentHostAttach: control={0}, total={1}ms, remove={2}ms, dock={3}ms, show={4}ms, handle={5}->{6}",
            LogicalControl.GetType().Name, total.ElapsedMilliseconds, remove, dock, show, hadHandle, LogicalControl.IsHandleCreated);
        return true;
    }

    public void DetachLogicalControl()
    {
        if (!HasAttachedLogicalControl)
        {
            return;
        }

        SuspendLayout();
        try
        {
            LogicalControl.Visible = false;
            Controls.Remove(LogicalControl);
        }
        finally
        {
            ResumeLayout(false);
        }
    }

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		if (HasAttachedLogicalControl && LogicalControl.Visible && LogicalControl.Bounds.Contains(ClientRectangle))
		{
			return;
		}
		base.OnPaintBackground(e);
	}

	protected override void WndProc(ref Message message)
	{
		DocumentSwitchTrace.Trace(this, "inner-host", "before", ref message,
			() => "document=" + GetTraceIdentity(LogicalControl), () => Visible && Parent != null && Parent.Visible);
		base.WndProc(ref message);
		DocumentSwitchTrace.Trace(this, "inner-host", "after", ref message,
			() => "document=" + GetTraceIdentity(LogicalControl), () => Visible && Parent != null && Parent.Visible);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			m_logicalControlObserver.Dispose();
		base.Dispose(disposing);
	}

	private static string GetTraceIdentity(Control control)
	{
		return control.GetType().FullName + "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(control).ToString("X");
	}
}
