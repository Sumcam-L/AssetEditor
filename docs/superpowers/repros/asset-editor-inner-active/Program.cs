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

        using (var geometryEditor = new ModelInstanceStateEditor())
        {
            try
            {
                geometryEditor.Bind(null);
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine("FAIL: ModelInstanceStateEditor.Bind(null) threw NullReferenceException.");
                return 1;
            }
        }

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

                if (constructionLog.IndexOf("AssetPageCreate", StringComparison.Ordinal) >= 0)
                {
                    Console.Error.WriteLine("FAIL: constructor created optional pages during construction; expected lazy creation.");
                    return 1;
                }
                if (FindInnerDockContent(control, "Geometries") != null || FindInnerDockContent(control, "Attachments") != null)
                {
                    Console.Error.WriteLine("FAIL: optional inner pages exist immediately after construction; expected lazy creation.");
                    return 1;
                }

                string sourcePath = Path.GetFullPath(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "..",
                    "Firaxis.AssetEditing", "Firaxis.AssetEditing", "AssetEditorControl.cs"));
                string source = File.ReadAllText(sourcePath);
                if (source.IndexOf("AssetPageBindingCoordinator", StringComparison.Ordinal) < 0 ||
                    source.IndexOf("BindNextIdle", StringComparison.Ordinal) < 0 ||
                    source.IndexOf("BindPendingPageForUser", StringComparison.Ordinal) < 0 ||
                    source.IndexOf("ClearOptionalPageBindings", StringComparison.Ordinal) < 0 ||
                    source.IndexOf("m_firstPaintCompleted = IsHandleCreated && Visible", StringComparison.Ordinal) >= 0)
                {
                    Console.Error.WriteLine("FAIL: AssetEditorControl does not use deferred page binding boundaries.");
                    return 1;
                }

                string configureBody = GetMethodBody(source, "private void ConfigurePageBindings(bool preserveActivePage)");
                string bindBody = GetMethodBody(source, "public override void Bind(IEntityEditorContext context)");
                string reloadBody = GetMethodBody(source, "private void AssetContext_Reloaded(object sender, EventArgs e)");
                int configuringGuard = configureBody.IndexOf("m_configuringPageBindings = true;", StringComparison.Ordinal);
                int guardedClear = configureBody.IndexOf("ClearOptionalPageBindings();", StringComparison.Ordinal);
                if (configuringGuard < 0 || guardedClear <= configuringGuard ||
                    bindBody.IndexOf("ClearOptionalPageBindings();", StringComparison.Ordinal) >= 0 ||
                    reloadBody.IndexOf("ClearOptionalPageBindings();", StringComparison.Ordinal) >= 0 ||
                    source.IndexOf("ResetFailedBinding", StringComparison.Ordinal) < 0 ||
                    source.IndexOf("bind-failed", StringComparison.Ordinal) < 0)
                {
                    Console.Error.WriteLine("FAIL: optional page clearing and failed Bind cleanup are not guarded.");
                    return 1;
                }
                if (reloadBody.IndexOf("catch (System.Exception ex)", StringComparison.Ordinal) < 0 ||
                    reloadBody.IndexOf("ResetFailedBinding();", StringComparison.Ordinal) < 0 ||
                    reloadBody.IndexOf("throw;", StringComparison.Ordinal) < 0)
                {
                    Console.Error.WriteLine("FAIL: failed reload reconfiguration does not reset partial page bindings before rethrowing.");
                    return 1;
                }

                string pagePrewarmIdleBody = GetMethodBody(source, "private void PagePrewarm_Idle(object sender, EventArgs e)");
                if (source.IndexOf("int m_pagePrewarmGeneration", StringComparison.Ordinal) < 0 ||
                    pagePrewarmIdleBody.IndexOf("m_pagePrewarmGeneration != m_pageBindings.Generation", StringComparison.Ordinal) < 0)
                {
                    Console.Error.WriteLine("FAIL: page prewarm callbacks are not guarded by their scheduled generation.");
                    return 1;
                }

                string clearOptionalBody = GetMethodBody(source, "private void ClearOptionalPageBindings()");
                if (source.IndexOf("private void UnsubscribeCookParameterSelection()", StringComparison.Ordinal) < 0 ||
                    clearOptionalBody.IndexOf(".PropertyGridView.SelectedPropertyChanged -=", StringComparison.Ordinal) >= 0)
                {
                    Console.Error.WriteLine("FAIL: cook parameter selection cleanup is not null-safe and centralized.");
                    return 1;
                }

                string geometrySourcePath = Path.GetFullPath(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "..",
                    "Firaxis.AssetEditing", "Firaxis.AssetEditing", "ModelInstanceStateEditor.cs"));
                string geometrySource = File.ReadAllText(geometrySourcePath);
                if (geometrySource.IndexOf("Application.Idle -= Application_Idle", StringComparison.Ordinal) < 0)
                {
                    Console.Error.WriteLine("FAIL: ModelInstanceStateEditor does not unsubscribe its static Idle handler.");
                    return 1;
                }

                EnsurePageCreated(control, "Geometries");
                EnsurePageCreated(control, "Attachments");

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

    private static string GetMethodBody(string source, string signature)
    {
        int signatureIndex = source.IndexOf(signature, StringComparison.Ordinal);
        if (signatureIndex < 0)
            return string.Empty;

        int bodyStart = source.IndexOf('{', signatureIndex);
        int depth = 0;
        for (int i = bodyStart; i < source.Length; i++)
        {
            if (source[i] == '{')
                depth++;
            else if (source[i] == '}' && --depth == 0)
                return source.Substring(bodyStart, i - bodyStart + 1);
        }
        return string.Empty;
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

	private static void EnsurePageCreated(AssetEditorControl control, string pageKindName)
	{
		Type pageKindType = typeof(AssetEditorControl).GetNestedType("PageKind", BindingFlags.NonPublic);
		object kind = Enum.Parse(pageKindType, pageKindName);
		typeof(AssetEditorControl)
			.GetMethod("EnsurePageCreated", BindingFlags.Instance | BindingFlags.NonPublic)
			.Invoke(control, new[] { kind });
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
