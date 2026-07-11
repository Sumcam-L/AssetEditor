using System.Linq;
using System.Windows;

namespace Sce.Atf.Wpf;

public static class DialogUtil
{
	public static Window GetActiveWindow()
	{
		Window result = Application.Current.MainWindow;
		if (!Application.Current.MainWindow.IsActive)
		{
			Window window = Application.Current.Windows.Cast<Window>().FirstOrDefault((Window x) => x.IsActive);
			if (window != null)
			{
				result = window;
			}
		}
		return result;
	}
}
