using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Applications;

[InheritedExport(typeof(IComposer))]
public abstract class AtfApp : Application, IComposer
{
	[ImportMany]
	private List<Lazy<IInitializable>> m_initializables = null;

	[ImportMany]
	private List<Lazy<IDisposable>> m_disposables = null;

	[Import]
	private MainWindowAdapter m_mainWindow = null;

	public CompositionContainer Container { get; private set; }

	internal bool IsShuttingDown { get; private set; }

	public AtfApp()
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
		Thread.CurrentThread.Name = "Main";
		FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
		m_initializables = new List<Lazy<IInitializable>>();
		m_disposables = new List<Lazy<IDisposable>>();
		base.Exit += AtfAppExit;
	}

	protected abstract AggregateCatalog GetCatalog();

	protected virtual void OnCompositionBeginning()
	{
	}

	protected virtual void OnCompositionComplete()
	{
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		if (Compose())
		{
			OnCompositionBeginning();
			foreach (Lazy<IInitializable> initializable in m_initializables)
			{
				initializable.Value.Initialize();
			}
			OnCompositionComplete();
			m_mainWindow.ShowMainWindow();
		}
		else
		{
			Shutdown();
		}
	}

	protected override void OnExit(ExitEventArgs e)
	{
		base.OnExit(e);
		foreach (Lazy<IDisposable> disposable in m_disposables)
		{
			disposable.Value.Dispose();
		}
		if (Container != null)
		{
			Container.Dispose();
		}
	}

	private bool Compose()
	{
		AggregateCatalog catalog = GetCatalog();
		Container = new CompositionContainer(catalog);
		try
		{
			Container.ComposeParts(this);
		}
		catch (CompositionException ex)
		{
			MessageBox.Show(ex.ToString());
			return false;
		}
		return true;
	}

	private void AtfAppExit(object sender, ExitEventArgs e)
	{
		IsShuttingDown = true;
	}
}
