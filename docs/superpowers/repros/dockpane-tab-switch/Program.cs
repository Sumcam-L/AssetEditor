using System;
using System.Drawing;
using System.Windows.Forms;
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
        using (ObservedDockContent first = new ObservedDockContent("First"))
        using (ObservedDockContent second = new ObservedDockContent("Second"))
        {
            host.StartPosition = FormStartPosition.Manual;
            host.Bounds = new Rectangle(100, 100, 800, 600);
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
            host.Controls.Add(dockPanel);
            host.Show();

            first.Show(dockPanel, DockState.Document);
            second.Show(dockPanel, DockState.Document);
            Application.DoEvents();

            first.Activate();
            Application.DoEvents();

			second.ResetObservations();
			second.Activate();
			Application.DoEvents();

			if (second.LastBoundsBeforeVisible == Rectangle.Empty)
			{
				Console.Error.WriteLine("FAIL: second document did not record bounds before becoming visible.");
				return 1;
			}

			if (second.LastBoundsBeforeVisible == second.Bounds)
			{
				Console.Error.WriteLine("FAIL: hidden inactive document was moved into the visible content rectangle before being shown.");
				Console.Error.WriteLine("Bounds before visible: {0}", second.LastBoundsBeforeVisible);
				Console.Error.WriteLine("Final bounds:          {0}", second.Bounds);
				return 1;
			}

			Console.WriteLine("PASS: hidden inactive document was not pre-moved into the visible content rectangle.");
			return 0;
        }
    }

    private sealed class ObservedDockContent : DockContent
    {
        public ObservedDockContent(string text)
        {
            Text = text;
        }

		public Rectangle LastBoundsBeforeVisible { get; private set; }

		public void ResetObservations()
		{
			LastBoundsBeforeVisible = Rectangle.Empty;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible && LastBoundsBeforeVisible == Rectangle.Empty)
			{
				LastBoundsBeforeVisible = Bounds;
			}
		}
    }
}
