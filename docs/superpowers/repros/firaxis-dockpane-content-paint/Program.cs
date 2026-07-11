using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Firaxis.Theme;
using WeifenLuo.WinFormsUI.Docking;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (Form host = new Form())
        using (DockPanel dockPanel = new DockPanel())
        using (DockContent content = new DockContent())
        using (DockContent secondContent = new DockContent())
        {
            host.StartPosition = FormStartPosition.Manual;
            host.Bounds = new Rectangle(100, 100, 800, 600);
            dockPanel.Theme = new FiraxisTheme();
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
            host.Controls.Add(dockPanel);
            host.Show();

            content.Name = "Geometries";
            content.Text = "Geometries";
            content.Show(dockPanel, DockState.Document);
            secondContent.Name = "Properties";
            secondContent.Text = "Properties";
            secondContent.Show(dockPanel, DockState.Document);
            Application.DoEvents();

            DockPane pane = content.DockHandler.Pane;
			pane.ActiveContent = content;
			Application.DoEvents();
			int immediateTabPaints = 0;
			pane.TabStripControl.Paint += delegate { immediateTabPaints++; };
			pane.ActiveContent = secondContent;
			if (immediateTabPaints == 0)
			{
				Console.Error.WriteLine("FAIL: document tab activation returned before repainting the old and new tab states.");
				return 1;
			}
            Color expectedBackground = dockPanel.Theme.ColorPalette.MainWindowActive.Background;
            if (pane.BackColor.ToArgb() != expectedBackground.ToArgb())
            {
                Console.Error.WriteLine("FAIL: FiraxisDockPane erase background does not match the theme environment background.");
                Console.Error.WriteLine("Expected: {0}", expectedBackground);
                Console.Error.WriteLine("Actual:   {0}", pane.BackColor);
                return 1;
            }
            Rectangle contentRect = GetContentRectangle(pane);
            if (contentRect.Width <= 2 || contentRect.Height <= 2)
            {
                Console.Error.WriteLine("FAIL: content rectangle is unexpectedly small: {0}", contentRect);
                return 1;
            }

            Rectangle clip = new Rectangle(contentRect.Left + 1, contentRect.Top + 1, 4, 4);
            Color sentinel = Color.FromArgb(17, 23, 31);

            using (Bitmap bitmap = new Bitmap(pane.ClientSize.Width, pane.ClientSize.Height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(sentinel);
                graphics.SetClip(clip);
                InvokeOnPaint(pane, graphics, clip);

                Color actual = bitmap.GetPixel(clip.Left, clip.Top);
                if (actual.ToArgb() != sentinel.ToArgb())
                {
                    Console.Error.WriteLine("FAIL: FiraxisDockPane painted inside ContentRectangle.");
                    Console.Error.WriteLine("ContentRectangle: {0}", contentRect);
                    Console.Error.WriteLine("Clip:             {0}", clip);
                    Console.Error.WriteLine("Expected pixel:   {0}", sentinel);
                    Console.Error.WriteLine("Actual pixel:     {0}", actual);
                    return 1;
                }
            }

            Console.WriteLine("PASS: FiraxisDockPane leaves child content area untouched during pane paint.");
            return 0;
        }
    }

    private static Rectangle GetContentRectangle(DockPane pane)
    {
        PropertyInfo property = typeof(DockPane).GetProperty("ContentRectangle", BindingFlags.Instance | BindingFlags.NonPublic);
        return (Rectangle)property.GetValue(pane, null);
    }

    private static void InvokeOnPaint(DockPane pane, Graphics graphics, Rectangle clip)
    {
        MethodInfo method = typeof(Control).GetMethod("OnPaint", BindingFlags.Instance | BindingFlags.NonPublic);
        using (PaintEventArgs args = new PaintEventArgs(graphics, clip))
        {
            method.Invoke(pane, new object[] { args });
        }
    }
}
