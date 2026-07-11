using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using Firaxis.Utility;

namespace Firaxis.ATF;

public class ScopedSplashScreen : Window, IComponentConnector
{
	private readonly TaskScheduler _uiTaskScheduler;

	private readonly Action _operation;

	internal TextBlock Caption;

	private bool _contentLoaded;

	public ScopedSplashScreen(Action operation, string message)
		: this(operation, message, TaskScheduler.FromCurrentSynchronizationContext())
	{
	}

	public ScopedSplashScreen(Action operation, string message, TaskScheduler uiScheduler)
	{
		InitializeComponent();
		_operation = operation;
		_uiTaskScheduler = uiScheduler;
		Caption.Text = message;
		base.Title = message;
		base.Loaded += ScopedSplashScreen_Loaded;
	}

	public void Connect(int connectionId, object target)
	{
		throw new NotImplementedException();
	}

	public void InitializeComponent()
	{
		throw new NotImplementedException();
	}

	private void HideCloseButton()
	{
		IntPtr handle = new WindowInteropHelper(this).Handle;
		int dwNewLong = NativeMethods.GetWindowLong(handle, -16) & -524289;
		NativeMethods.SetWindowLong(handle, -16, dwNewLong);
	}

	private void ScopedSplashScreen_Loaded(object sender, RoutedEventArgs e)
	{
		HideCloseButton();
		Task.Factory.StartNew(_operation).ContinueWith(delegate
		{
			Close();
		}, _uiTaskScheduler);
	}
}
