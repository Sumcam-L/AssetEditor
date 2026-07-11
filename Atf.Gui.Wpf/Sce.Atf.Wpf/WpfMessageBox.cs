using System.Windows;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf;

public static class WpfMessageBox
{
	private static ShowMessageBoxDelegate s_showDelegate = ShowDefault;

	public static MessageBoxResult Show(string messageBoxText)
	{
		return Show(null, messageBoxText, null, MessageBoxButton.OK, MessageBoxImage.None);
	}

	public static MessageBoxResult Show(string messageBoxText, string caption)
	{
		return Show(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
	}

	public static MessageBoxResult Show(Window owner, string messageBoxText)
	{
		return Show(owner, messageBoxText, null, MessageBoxButton.OK, MessageBoxImage.None);
	}

	public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
	{
		return Show(null, messageBoxText, caption, button, MessageBoxImage.None);
	}

	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
	{
		return Show(owner, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
	}

	public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
	{
		return Show(null, messageBoxText, caption, button, icon);
	}

	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button)
	{
		return Show(owner, messageBoxText, caption, button, MessageBoxImage.None);
	}

	public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
	{
		if (owner == null && Application.Current != null)
		{
			owner = Application.Current.MainWindow;
		}
		return s_showDelegate(owner, messageBoxText, caption, button, icon);
	}

	public static void SetProvider(ShowMessageBoxDelegate del)
	{
		Requires.NotNull(del, "del");
		s_showDelegate = del;
	}

	private static MessageBoxResult ShowDefault(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
	{
		MessageBoxDialog messageBoxDialog = new MessageBoxDialog(caption, messageBoxText, button, icon);
		messageBoxDialog.Owner = owner;
		messageBoxDialog.ShowDialog();
		return messageBoxDialog.MessageBoxResult;
	}
}
