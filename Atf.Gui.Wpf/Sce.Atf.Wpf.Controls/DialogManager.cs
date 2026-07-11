using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls;

public static class DialogManager
{
	public static SizeChangedEventHandler SetupAndOpenDialog(IDialogSite window, Control dialog)
	{
		dialog.MinHeight = window.Site.ActualHeight / 4.0;
		dialog.MaxHeight = window.Site.ActualHeight;
		SizeChangedEventHandler sizeChangedEventHandler = null;
		sizeChangedEventHandler = delegate
		{
			dialog.MinHeight = window.Site.ActualHeight / 4.0;
			dialog.MaxHeight = window.Site.ActualHeight;
		};
		window.Site.SizeChanged += sizeChangedEventHandler;
		window.Site.Children.Add(dialog);
		return sizeChangedEventHandler;
	}
}
