using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using AtfPropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        try
        {
            return Run();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }
    }

    private static int Run()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (AtfPropertyGrid grid = new AtfPropertyGrid(PropertyGridMode.DisableSearchControls))
        {
            grid.Visible = false;
            grid.BuildPropertiesWhenHidden = true;
            grid.Bind(new SampleContext(new Sample { First = 1, Second = 2 }));

            object[] initialRows = GetPropertyRows(grid.PropertyGridView);
            if (initialRows.Length != 2)
            {
                return Fail("hidden eager binding did not construct both property controls");
            }

            using (Form host = new Form())
            {
                host.Controls.Add(grid);
                grid.Dock = DockStyle.Fill;
				grid.Visible = true;
                host.Show();
                Application.DoEvents();
                AssertSameRows(initialRows, GetPropertyRows(grid.PropertyGridView), "first display rebuilt property rows");

                grid.Visible = false;
                Application.DoEvents();
                grid.Visible = true;
                Application.DoEvents();
                AssertSameRows(initialRows, GetPropertyRows(grid.PropertyGridView), "hide/show rebuilt property rows");
				host.Controls.Remove(grid);
            }

            grid.Bind(new SampleContext(new AlternateSample { Third = 3 }));
            object[] replacementRows = GetPropertyRows(grid.PropertyGridView);
            if (replacementRows.Length != 1 || initialRows.Contains(replacementRows[0]))
            {
                return Fail("a structurally different context did not rebuild property controls");
            }
        }

        using (AtfPropertyGrid lazyGrid = new AtfPropertyGrid(PropertyGridMode.DisableSearchControls))
        {
            lazyGrid.Visible = false;
            lazyGrid.Bind(new SampleContext(new Sample { First = 1, Second = 2 }));
            if (GetPropertyRows(lazyGrid.PropertyGridView).Length != 0)
            {
                return Fail("ordinary hidden grid no longer preserves lazy construction");
            }

            using (Form host = new Form())
            {
                host.Controls.Add(lazyGrid);
                lazyGrid.Visible = true;
                host.Show();
                Application.DoEvents();
                object[] firstDisplayRows = GetPropertyRows(lazyGrid.PropertyGridView);
                if (firstDisplayRows.Length != 2)
                {
                    return Fail("ordinary grid did not construct rows on first display");
                }
                lazyGrid.Visible = false;
                lazyGrid.Visible = true;
                Application.DoEvents();
                AssertSameRows(firstDisplayRows, GetPropertyRows(lazyGrid.PropertyGridView), "ordinary grid rebuilt rows after first display");
                host.Controls.Remove(lazyGrid);
            }
        }

        AssertSourceOmits("Firaxis.ATF/Firaxis.ATF/CommandControl.cs", "VisibleChanged += CommandControl_VisibleChanged", "CommandControl still subscribes to repeated visibility-driven skinning");

        Console.WriteLine("PASS: hidden binding prebuilds property rows and visibility transitions reuse them.");
        return 0;
    }

    private static object[] GetPropertyRows(PropertyView view)
    {
        FieldInfo field = typeof(PropertyView).GetField("m_activeProperties", BindingFlags.Instance | BindingFlags.NonPublic);
        IEnumerable properties = (IEnumerable)field.GetValue(view);
        var rows = new List<object>();
        foreach (object property in properties)
        {
            rows.Add(property);
        }
        return rows.ToArray();
    }

    private static void AssertSameRows(object[] expected, object[] actual, string message)
    {
        if (expected.Length != actual.Length || expected.Where((control, index) => !ReferenceEquals(control, actual[index])).Any())
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertVisibilityHandlerOmits(string relativePath, string handlerName, string forbiddenCall, string message)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", ".."));
        string source = File.ReadAllText(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        int handler = source.IndexOf(handlerName, StringComparison.Ordinal);
        if (handler < 0)
        {
            throw new InvalidOperationException("visibility handler was not found in " + relativePath);
        }
        string body = source.Substring(handler, Math.Min(500, source.Length - handler));
        if (body.IndexOf(forbiddenCall, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertSourceOmits(string relativePath, string forbiddenText, string message)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", ".."));
        string source = File.ReadAllText(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (source.IndexOf(forbiddenText, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine("FAIL: " + message);
        return 1;
    }

    private sealed class Sample
    {
        public int First { get; set; }
        public int Second { get; set; }
    }

    private sealed class AlternateSample
    {
        public int Third { get; set; }
    }

    private sealed class SampleContext : IPropertyEditingContext
    {
        private readonly object m_item;

        public SampleContext(object item)
        {
            m_item = item;
        }

        public IEnumerable<object> Items
        {
            get { yield return m_item; }
        }

        public IEnumerable<PropertyDescriptor> PropertyDescriptors
        {
            get { return TypeDescriptor.GetProperties(m_item).Cast<PropertyDescriptor>(); }
        }
    }
}
