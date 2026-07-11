using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

[Export(typeof(ISplashScreenService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SplashScreenService : ISplashScreenService, IInitializable, IDisposable
{
	private bool _showSplashDialog = true;

	[Import(AllowDefault = true)]
	private ISettingsService _settingsService;

	private readonly IDialogHostService _dialogService;

	private bool disposedValue;

	public bool ShowSplashDialog
	{
		get
		{
			return _showSplashDialog;
		}
		set
		{
			_showSplashDialog = value;
		}
	}

	[Import(AllowDefault = true)]
	private Lazy<IMainWindow> MainWindow { get; set; }

	[ImportingConstructor]
	public SplashScreenService(IDialogHostService dialogService)
	{
		_dialogService = dialogService;
		Context.Add(this);
	}

	public void Initialize()
	{
		if (_settingsService != null)
		{
			_settingsService.RegisterSettings("Application".Localize(), new BoundPropertyDescriptor(this, () => ShowSplashDialog, "Enable Splash Screen".Localize(), "Splash Screen".Localize(), "If true, the splash screen will be displayed during length operations.".Localize()));
			_settingsService.RegisterUserSettings("Application".Localize(), new BoundPropertyDescriptor(this, () => ShowSplashDialog, "Enable Splash Screen".Localize(), "Splash Screen".Localize(), "If true, the splash screen will be displayed during length operations.".Localize()));
		}
	}

	public void ShowSplashScreen(Action action, string caption, string message)
	{
		if (ShowSplashDialog)
		{
			WaitDialog waitDialog = new WaitDialog(caption, delegate(WaitDialog dlg)
			{
				dlg.SetMessage(message);
				action();
			});
			IWin32Window win32Window = MainWindow?.Value?.DialogOwner;
			if (win32Window != null)
			{
				waitDialog.ShowDialog(win32Window);
			}
			else
			{
				waitDialog.ShowDialog();
			}
		}
		else
		{
			action();
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				Context.Remove(this);
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
