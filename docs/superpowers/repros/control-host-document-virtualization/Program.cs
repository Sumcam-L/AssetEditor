using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.Theme;
using Sce.Atf.Applications;
using WeifenLuo.WinFormsUI.Docking;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (Form form = new Form())
        using (Panel first = NewDocumentPanel("first"))
        using (Panel second = NewDocumentPanel("second"))
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Bounds = new Rectangle(100, 100, 800, 600);

            ControlHostService service = new ControlHostService(form);
            ((IPartImportsSatisfiedNotification)service).OnImportsSatisfied();
            ThemeService themeService = new ThemeService(service);
            themeService.ActiveTheme = new FiraxisTheme();

            TestClient client = new TestClient();
            ControlInfo firstInfo = NewDocumentInfo("First");
            ControlInfo secondInfo = NewDocumentInfo("Second");

            service.RegisterControl(first, firstInfo, client);
            service.RegisterControl(second, secondInfo, client);
            form.Show();
            Application.DoEvents();

            int secondActivation = client.ActivationHistory.IndexOf(second);
            if (secondActivation >= 0 && client.ActivationHistory.Skip(secondActivation + 1).Contains(first))
            {
                Console.Error.WriteLine("FAIL: registering the second document reactivated the previous document after the new document was active: " +
                    string.Join(" -> ", client.ActivationHistory.Select(control => control.Name)));
                return 1;
            }

            if (firstInfo.Control != first || secondInfo.Control != second)
            {
                Console.Error.WriteLine("FAIL: ControlInfo.Control stopped tracking the logical real control.");
                return 1;
            }

            if (firstInfo.HostControl == null || secondInfo.HostControl == null)
            {
                Console.Error.WriteLine("FAIL: document controls did not receive DockContent host controls.");
                return 1;
            }

            WeifenLuo.WinFormsUI.Docking.DockContent firstContent = firstInfo.HostControl as WeifenLuo.WinFormsUI.Docking.DockContent;
            WeifenLuo.WinFormsUI.Docking.DockContent secondContent = secondInfo.HostControl as WeifenLuo.WinFormsUI.Docking.DockContent;
            if (firstContent == null || secondContent == null)
            {
                Console.Error.WriteLine("FAIL: document host controls are not DockContent instances.");
                return 1;
            }

            if (!firstContent.DockHandler.CloseButtonVisible || !secondContent.DockHandler.CloseButtonVisible)
            {
                Console.Error.WriteLine("FAIL: document tab close buttons are not visible.");
                return 1;
            }

            DockPane documentPane = firstContent.Pane;
            if (documentPane == null || documentPane.DockState != DockState.Document)
            {
                Console.Error.WriteLine("FAIL: document content was not shown in a document DockPane.");
                return 1;
            }

            if (documentPane.TabStripControl.GetType().FullName != "Firaxis.Theme.FiraxisDockPaneStrip")
            {
                Console.Error.WriteLine("FAIL: document pane is not using the Firaxis themed tab strip: " + documentPane.TabStripControl.GetType().FullName);
                return 1;
            }

            Control firstHosted = firstInfo.HostControl.Controls.Cast<Control>().SingleOrDefault();
            Control secondHosted = secondInfo.HostControl.Controls.Cast<Control>().SingleOrDefault();
            if (firstHosted == first || secondHosted == second)
            {
                Console.Error.WriteLine("FAIL: real document controls were hosted directly instead of through lightweight hosts.");
                return 1;
            }

            service.Show(first);
            Application.DoEvents();
            if (client.LastActivated != first)
            {
                Console.Error.WriteLine("FAIL: Activate did not receive first logical real control.");
                return 1;
            }

            service.Show(second);
            Application.DoEvents();
            if (client.LastDeactivated != first || client.LastActivated != second)
            {
                Console.Error.WriteLine("FAIL: activation routing did not use logical real controls when switching.");
                return 1;
            }

            if (first.Parent != null && first.Visible)
            {
                Console.Error.WriteLine("FAIL: inactive first real control is still visible in a host.");
                return 1;
            }

            if (second.Parent == null || !second.Visible)
            {
                Console.Error.WriteLine("FAIL: active second real control is not attached and visible.");
                return 1;
            }

            service.Hide(first);
            Application.DoEvents();
            service.Show(second);
            Application.DoEvents();
            if (first.Parent != null && first.Visible)
            {
                Console.Error.WriteLine("FAIL: hidden first real control is still visible in a host after activating second.");
                return 1;
            }

            service.Show(first);
            if (first.Parent == null || !first.Visible)
            {
                Console.Error.WriteLine("FAIL: showing hidden first document did not reattach and show its real control.");
                return 1;
            }

            Application.DoEvents();

            typeof(ControlHostService)
                .GetMethod("AttachVisibleDocumentHosts", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(service, null);

            if (second.Parent != null && second.Visible)
            {
                Console.Error.WriteLine("FAIL: visible-host sweep attached an inactive document after the active document.");
                return 1;
            }

            typeof(ControlHostService)
                .GetField("m_pendingRegisteredDocumentControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(service, first);
            typeof(ControlHostService)
                .GetField("m_activeDockContent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(service, secondContent);
            typeof(ControlHostService)
                .GetMethod("ActivateClientIfStillActive", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(service, new object[] { secondContent, 0L });
            if (second.Parent != null || first.Parent == null)
            {
                Console.Error.WriteLine("FAIL: stale activation attached the old document while a new document registration was pending.");
                return 1;
            }
            typeof(ControlHostService)
                .GetField("m_pendingRegisteredDocumentControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(service, null);

            using (var tool = new WeifenLuo.WinFormsUI.Docking.DockContent())
            {
                firstContent.Controls[0].GetType()
                    .GetMethod("DetachLogicalControl")
                    .Invoke(firstContent.Controls[0], null);
                tool.Show(firstContent.DockPanel, DockState.DockRight);
                tool.Activate();
                Application.DoEvents();
                typeof(ControlHostService)
                    .GetField("m_activeDockContent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(service, firstContent);
                typeof(ControlHostService)
                    .GetMethod("ActivateClientIfStillActive", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(service, new object[] { firstContent, 0L });
                if (first.Parent == null || !first.Visible)
                {
                    Console.Error.WriteLine("FAIL: active document was not attached while a tool window held active content.");
                    return 1;
                }
            }

            typeof(ControlHostService)
                .GetField("m_activeDockContent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(service, secondContent);
            typeof(ControlHostService)
                .GetMethod("ActivateClientIfStillActive", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(service, new object[] { secondContent, 0L });
            if (second.Parent != null || first.Parent == null)
            {
                Console.Error.WriteLine("FAIL: stale activation attached the old document after pending registration completed.");
                return 1;
            }

            service.Show(second);
            Application.DoEvents();
            if (second.Parent == null || !second.Visible)
            {
                Console.Error.WriteLine("FAIL: second document was not visible before active close test.");
                return 1;
            }

            secondContent.Close();
            Application.DoEvents();
            if (first.Parent == null || !first.Visible)
            {
                Console.Error.WriteLine("FAIL: closing the active document did not attach and show the next active real control.");
                return 1;
            }

            service.UnregisterControl(first);
            Application.DoEvents();

            PaintTrackingPanel baselineChild;
            PaintTrackingPanel targetChild;
            using (PaintTrackingPanel baseline = NewPaintDocumentPanel("visibleBaseline", out baselineChild))
            using (PaintTrackingPanel target = NewPaintDocumentPanel("visibleTarget", out targetChild))
            {
                service.RegisterControl(baseline, NewDocumentInfo("Visible Baseline"), client);
                Application.DoEvents();

                bool targetPaintedBeforeActivation = false;
                client.Activating = control =>
                {
                    if (control == target)
                    {
                        targetPaintedBeforeActivation = target.PaintCount > 0 && targetChild.PaintCount > 0;
                        Thread.Sleep(250);
                    }
                };

                service.RegisterControl(target, NewDocumentInfo("Visible Target"), client);
                client.Activating = null;

                if (!targetPaintedBeforeActivation)
                {
                    Console.Error.WriteLine("FAIL: visible registered document reached slow activation before its first complete paint.");
                    return 1;
                }

                service.UnregisterControl(target);
                service.UnregisterControl(baseline);
                Application.DoEvents();
            }

            Console.WriteLine("PASS: document virtualization routes lifecycle to logical controls and attaches only the active real control.");
            return 0;
        }
    }

    private static Panel NewDocumentPanel(string name)
    {
        return new Panel
        {
            Name = name,
            Tag = name,
            BackColor = Color.FromArgb(20, 40, 60)
        };
    }

    private sealed class PaintTrackingPanel : Panel
    {
        public int PaintCount { get; private set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            PaintCount++;
            base.OnPaint(e);
        }
    }

    private static PaintTrackingPanel NewPaintDocumentPanel(string name, out PaintTrackingPanel child)
    {
        var root = new PaintTrackingPanel
        {
            Name = name,
            Tag = name,
            BackColor = Color.FromArgb(24, 48, 72)
        };
        child = new PaintTrackingPanel
        {
            Name = name + "Child",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(72, 48, 24)
        };
        root.Controls.Add(child);
        return root;
    }

    private static ControlInfo NewDocumentInfo(string name)
    {
        return new ControlInfo(name, name, StandardControlGroup.Center)
        {
            IsDocument = true
        };
    }

    private sealed class TestClient : IControlHostClient
    {
        public Control LastActivated { get; private set; }

        public Control LastDeactivated { get; private set; }

        public List<Control> ActivationHistory { get; } = new List<Control>();

        public Action<Control> Activating { get; set; }

        public void Activate(Control control)
        {
            Activating?.Invoke(control);
            LastActivated = control;
            ActivationHistory.Add(control);
        }

        public void Deactivate(Control control)
        {
            LastDeactivated = control;
        }

        public bool Close(Control control)
        {
            return true;
        }
    }
}
