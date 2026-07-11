using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Sce.Atf.Wpf;

public static class DialogUtils
{
	private class WindowWrapper : System.Windows.Forms.IWin32Window
	{
		public IntPtr Handle { get; private set; }

		public WindowWrapper(IntPtr handle)
		{
			Handle = handle;
		}
	}

	public static bool? ShowDialogWithViewModel<TDialog, TViewModel>() where TDialog : Window, new()
	{
		return new TDialog
		{
			DataContext = Activator.CreateInstance<TViewModel>()
		}.ShowParentedDialog();
	}

	public static bool? ShowDialogWithViewModel<TDialog>(object viewModel) where TDialog : Window, new()
	{
		return new TDialog
		{
			DataContext = viewModel
		}.ShowParentedDialog();
	}

	public static void ShowWithViewModel<TDialog>(object viewModel) where TDialog : Window, new()
	{
		new TDialog
		{
			DataContext = viewModel
		}.ShowParented();
	}

	public static Window GetActiveWindow()
	{
		Window result = System.Windows.Application.Current.MainWindow;
		if (!System.Windows.Application.Current.MainWindow.IsActive)
		{
			Window window = System.Windows.Application.Current.Windows.Cast<Window>().FirstOrDefault((Window x) => x.IsActive);
			if (window != null)
			{
				result = window;
			}
		}
		return result;
	}

	public static DialogResult ShowParentedDialog(CommonDialog dialog)
	{
		Window activeWindow = GetActiveWindow();
		WindowInteropHelper windowInteropHelper = new WindowInteropHelper(activeWindow);
		return dialog.ShowDialog(new WindowWrapper(windowInteropHelper.Handle));
	}
}
