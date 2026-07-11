using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.Applications;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (Form mainForm = new Form())
        using (Panel child = new Panel())
        {
            mainForm.StartPosition = FormStartPosition.Manual;
            mainForm.Bounds = new Rectangle(100, 100, 800, 600);

            ControlHostService service = new ControlHostService(mainForm);
            ((IPartImportsSatisfiedNotification)service).OnImportsSatisfied();

            child.BackColor = Color.FromArgb(10, 20, 30);
            ControlInfo info = new ControlInfo("Document", "Document", StandardControlGroup.Center);
            service.RegisterControl(child, info, null);
            mainForm.Show();
            Application.DoEvents();

            Control host = info.HostControl;
            if (host == null || host.Controls.Count == 0 || !host.Controls[0].Visible)
            {
                Console.Error.WriteLine("FAIL: document host or child was not visible.");
                return 1;
            }

            Rectangle childBounds = host.Controls[0].Bounds;
            if (childBounds.Width <= 2 || childBounds.Height <= 2)
            {
                Console.Error.WriteLine("FAIL: child bounds are unexpectedly small: {0}", childBounds);
                return 1;
            }

            Rectangle clip = new Rectangle(childBounds.Left + 1, childBounds.Top + 1, 4, 4);
            Color sentinel = Color.FromArgb(17, 23, 31);

            using (Bitmap bitmap = new Bitmap(host.ClientSize.Width, host.ClientSize.Height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(sentinel);
                graphics.SetClip(clip);
                InvokeOnPaintBackground(host, graphics, clip);

                Color actual = bitmap.GetPixel(clip.Left, clip.Top);
                if (actual.ToArgb() != sentinel.ToArgb())
                {
                    Console.Error.WriteLine("FAIL: document host painted background inside visible child bounds.");
                    Console.Error.WriteLine("Host type:       {0}", host.GetType().FullName);
                    Console.Error.WriteLine("Host client:     {0}", host.ClientSize);
                    Console.Error.WriteLine("Child bounds:    {0}", childBounds);
                    Console.Error.WriteLine("Clip:            {0}", clip);
                    Console.Error.WriteLine("Expected pixel:  {0}", sentinel);
                    Console.Error.WriteLine("Actual pixel:    {0}", actual);
                    return 1;
                }
            }

            Console.WriteLine("PASS: document host leaves visible child bounds untouched during background paint.");
            return 0;
        }
    }

    private static void InvokeOnPaintBackground(Control control, Graphics graphics, Rectangle clip)
    {
        MethodInfo method = typeof(Control).GetMethod("OnPaintBackground", BindingFlags.Instance | BindingFlags.NonPublic);
        using (PaintEventArgs args = new PaintEventArgs(graphics, clip))
        {
            method.Invoke(control, new object[] { args });
        }
    }
}
