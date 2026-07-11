using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf.Applications;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (Form form = new Form())
        using (HookControl control = new HookControl { Visible = false })
        {
            ControlHostService service = new ControlHostService(form);
            ((IPartImportsSatisfiedNotification)service).OnImportsSatisfied();

            ControlInfo info = new ControlInfo("Hook", "Hook", StandardControlGroup.Center)
            {
                IsDocument = true
            };

            service.RegisterControl(control, info, new TestClient());
            Application.DoEvents();

            if (!control.BeforeShowCalled)
            {
                Console.Error.WriteLine("FAIL: hosted control did not receive pre-show callback.");
                return 1;
            }

            if (control.WasVisibleDuringBeforeShow)
            {
                Console.Error.WriteLine("FAIL: pre-show callback ran after the control became visible.");
                return 1;
            }

            Console.WriteLine("PASS: hosted control receives pre-show callback before visibility.");
            return 0;
        }
    }

    private sealed class HookControl : UserControl, IControlHostPreShowClient
    {
        public bool BeforeShowCalled { get; private set; }

        public bool WasVisibleDuringBeforeShow { get; private set; }

        public void BeforeControlHostShow()
        {
            BeforeShowCalled = true;
            WasVisibleDuringBeforeShow = Visible;
        }
    }

    private sealed class TestClient : IControlHostClient
    {
        public void Activate(Control control)
        {
        }

        public void Deactivate(Control control)
        {
        }

        public bool Close(Control control)
        {
            return true;
        }
    }
}
