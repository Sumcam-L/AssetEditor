using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using Firaxis.ATF;
using Firaxis.AssetEditing;
using Sce.Atf.Applications;
using WeifenLuo.WinFormsUI.Docking;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (Form host = new Form())
        {
            PaintTimingLog.Clear();
            using (AssetEditorControl control = new AssetEditorControl(string.Empty, new TestThemeService()))
            {
                PaintTimingLog.Flush();
                string constructionLog = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log"));
                string[] transientPages = { "Cook Params", "Geometries", "Attachments", "Animations", "Particles", "Behaviors", "Splines" };
                foreach (string page in transientPages)
                {
                    if (constructionLog.IndexOf("content=" + page + " Activate begin", StringComparison.Ordinal) >= 0)
                    {
                        Console.Error.WriteLine("FAIL: constructor explicitly activated inner page {0}.", page);
                        return 1;
                    }
                }

                host.Controls.Add(control);
                control.Dock = DockStyle.Fill;

                HideAllInnerContent(control);
                host.Show();

                DockPanel innerDockPanel = GetInnerDockPanel(control);
                if (innerDockPanel.ActiveContent == null)
                {
                    Console.Error.WriteLine("FAIL: active inner content was not restored synchronously when the editor became visible.");
                    return 1;
                }

                SetPaneActiveContentToNull(innerDockPanel.ActiveContent.DockHandler.Pane);
                typeof(AssetEditorControl).GetMethod("EnsureActiveInnerContent", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, null);
                if (innerDockPanel.ActiveContent == null)
                {
                    Console.Error.WriteLine("FAIL: active inner content was not restored synchronously without deferred activation.");
                    return 1;
                }

                string currentLayout = control.EditorLayoutState;
                bool sawNullActiveContentWhileVisible = false;
                innerDockPanel.ActiveContentChanged += delegate
                {
                    if (control.Visible && innerDockPanel.ActiveContent == null)
                    {
                        sawNullActiveContentWhileVisible = true;
                    }
                };

                control.EditorLayoutState = currentLayout;

                if (sawNullActiveContentWhileVisible)
                {
                    Console.Error.WriteLine("FAIL: applying editor layout while visible temporarily cleared active inner content.");
                    return 1;
                }

                Firaxis.ATF.DockContent attachments = FindInnerDockContent(control, "Attachments");
                if (attachments == null)
                {
                    Console.Error.WriteLine("FAIL: could not find Attachments inner content.");
                    return 1;
                }
                attachments.Activate();
                Application.DoEvents();
                SetPaneActiveContentToNull(attachments.DockHandler.Pane);
                typeof(AssetEditorControl).GetMethod("EnsureActiveInnerContent", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, null);
                if ((attachments.DockHandler.Pane?.ActiveContent as Firaxis.ATF.DockContent)?.Name != "Attachments")
                {
                    Console.Error.WriteLine("FAIL: fallback activation did not preserve the last active Attachments tab.");
                    Console.Error.WriteLine("Actual active: {0}", (attachments.DockHandler.Pane?.ActiveContent as Firaxis.ATF.DockContent)?.Name ?? "null");
                    return 1;
                }
            }
        }

        using (AssetEditorControl source = new AssetEditorControl(string.Empty, new TestThemeService()))
        {
            string sourceLayout = source.EditorLayoutState;
            string savedClassLayout = GetDockPanelLayout(sourceLayout);
            PaintTimingLog.Clear();
            string savedLayouts = WrapClassLayout("Unit", sourceLayout);
            using (AssetEditorControl target = new AssetEditorControl(savedLayouts, new TestThemeService(), "Unit"))
            {
                if (target.IsEditorLayoutStateApplied(savedClassLayout))
                {
                    Console.Error.WriteLine("FAIL: saved class layout was applied before the editor had a visible host size.");
                    return 1;
                }

                using (Form host = new Form())
                {
                    host.Bounds = new System.Drawing.Rectangle(100, 100, 800, 600);
                    host.Controls.Add(target);
                    target.Dock = DockStyle.Fill;
                    host.Show();
                    Application.DoEvents();

                    if (target.IsEditorLayoutStateApplied(savedClassLayout))
                    {
                        Console.Error.WriteLine("FAIL: saved class layout was applied during initial AST display.");
                        return 1;
                    }
                    host.Controls.Remove(target);
                    host.Close();
                    Application.DoEvents();
                }
            }
            string log = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log"));
            int initialApplyIndex = log.IndexOf("AssetEditorControl: initial apply editor layout class=Unit", StringComparison.Ordinal);
            int visibleIndex = log.IndexOf("AssetEditorControl InnerDockContentVisible", StringComparison.Ordinal);
            if (visibleIndex >= 0 && (initialApplyIndex < 0 || visibleIndex < initialApplyIndex))
            {
                Console.Error.WriteLine("FAIL: saved-layout construction showed default inner content before applying the saved layout.");
                return 1;
            }
        }

        Console.WriteLine("PASS: active inner content was restored synchronously and saved layout is skipped during initial AST display.");
        return 0;
    }

    private static string WrapClassLayout(string className, string layout)
    {
        return "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><ActiveAssetEditorLayouts><layout entityclass=\"" + className + "\">" + GetDockPanelLayout(layout) + "</layout></ActiveAssetEditorLayouts>";
    }

    private static string GetDockPanelLayout(string layout)
    {
        XmlDocument source = new XmlDocument();
        source.LoadXml(layout);
        XmlNode dockPanel = source.SelectSingleNode("DockPanel");
        return dockPanel.OuterXml;
    }

    private static void HideAllInnerContent(AssetEditorControl control)
    {
        FieldInfo field = typeof(AssetEditorControl).GetField("m_dockContent", BindingFlags.Instance | BindingFlags.NonPublic);
        IDictionary dockContent = (IDictionary)field.GetValue(control);
        foreach (object value in dockContent.Values)
        {
            ((Firaxis.ATF.DockContent)value).Hide();
        }
        Application.DoEvents();
    }

	private static Firaxis.ATF.DockContent FindInnerDockContent(AssetEditorControl control, string name)
	{
		FieldInfo field = typeof(AssetEditorControl).GetField("m_dockContent", BindingFlags.Instance | BindingFlags.NonPublic);
		IDictionary dockContent = (IDictionary)field.GetValue(control);
		foreach (object value in dockContent.Values)
		{
			Firaxis.ATF.DockContent content = (Firaxis.ATF.DockContent)value;
			if (content.Name == name)
			{
				return content;
			}
		}
		return null;
	}

    private static DockPanel GetInnerDockPanel(AssetEditorControl control)
    {
        FieldInfo field = typeof(AssetEditorControl).GetField("m_dockPanel", BindingFlags.Instance | BindingFlags.NonPublic);
        return (DockPanel)field.GetValue(control);
    }

    private static void SetPaneActiveContentToNull(DockPane pane)
    {
        FieldInfo field = typeof(DockPane).GetField("m_activeContent", BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(pane, null);
    }

    private sealed class TestThemeService : IThemeService
    {
        public ThemeBase ActiveTheme { get; set; } = new VS2005Theme();

        public event EventHandler ThemeChanged;
    }
}
