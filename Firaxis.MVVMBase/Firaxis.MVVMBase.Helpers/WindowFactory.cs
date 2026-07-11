using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Firaxis.MVVMBase.Views;

namespace Firaxis.MVVMBase.Helpers;

public static class WindowFactory
{
	private const int GWL_STYLE = -16;

	private const int WS_MINIMIZEBOX = 131072;

	private const int WS_MAXIMIZEBOX = 65536;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	public static bool? CreateDialogFromContent(object content, Window owner = null)
	{
		WindowFactoryRegistration windowFactoryRegistration = (owner?.TryFindResource(content.GetType()) ?? System.Windows.Application.Current?.TryFindResource(content.GetType())) as WindowFactoryRegistration;
		Window window = ((!(windowFactoryRegistration?.ViewType != null)) ? new Window() : ((Window)Activator.CreateInstance(windowFactoryRegistration.ViewType)));
		window.Owner = owner;
		window.DataContext = content;
		window.SourceInitialized += DialogWindow_SourceInitialized;
		return window.ShowDialog();
	}

	public static bool? CreateDialogFromContent(object content, System.Windows.Forms.IWin32Window owner)
	{
		WindowFactoryRegistration windowFactoryRegistration = System.Windows.Application.Current?.TryFindResource(content.GetType()) as WindowFactoryRegistration;
		Window window = ((!(windowFactoryRegistration?.ViewType != null)) ? new Window() : ((Window)Activator.CreateInstance(windowFactoryRegistration.ViewType)));
		WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
		windowInteropHelper.Owner = owner.Handle;
		window.DataContext = content;
		window.SourceInitialized += DialogWindow_SourceInitialized;
		return window.ShowDialog();
	}

	public static string PromptForString(string dialogMessage, string dialogTitle = "")
	{
		PromptForStringDialog promptForStringDialog = new PromptForStringDialog();
		promptForStringDialog.Title = dialogTitle;
		promptForStringDialog.MessagePrompt = dialogMessage;
		if (promptForStringDialog.ShowDialog() != true)
		{
			return null;
		}
		return promptForStringDialog.userString;
	}

	private static void DialogWindow_SourceInitialized(object sender, EventArgs e)
	{
		if (sender is Window window)
		{
			IntPtr handle = new WindowInteropHelper(window).Handle;
			int windowLong = GetWindowLong(handle, -16);
			SetWindowLong(handle, -16, windowLong & -196609);
		}
	}
}
