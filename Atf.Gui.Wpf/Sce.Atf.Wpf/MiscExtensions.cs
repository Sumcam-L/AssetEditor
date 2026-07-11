using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf;

public static class MiscExtensions
{
	private static bool _isDialogOpen;

	public static bool GuidTryParse(string s, out Guid result)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		Regex regex = new Regex("^[A-Fa-f0-9]{32}$|^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
		Match match = regex.Match(s);
		if (match.Success)
		{
			result = new Guid(s);
			return true;
		}
		result = Guid.Empty;
		return false;
	}

	public static int BinarySearchIndexOf<T>(this IList<T> list, T value, IComparer<T> comparer = null)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		comparer = comparer ?? Comparer<T>.Default;
		int num = 0;
		int num2 = list.Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num) / 2;
			int num4 = comparer.Compare(value, list[num3]);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 < 0)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return ~num;
	}

	public static bool? ShowChildDialog(this Window owner, Window child)
	{
		Requires.NotNull(owner, "owner");
		Requires.NotNull(child, "child");
		child.DataContext = owner.DataContext;
		child.Owner = owner;
		return child.ShowDialog();
	}

	public static void CreateAndShowChildDialog<T>(this Window owner, ShowDialogEventArgs e) where T : Window, new()
    {
		Requires.NotNull(e, "e");
		e.DialogResult = new T
		{
			DataContext = e.ViewModel,
			Owner = owner
		}.ShowDialog();
	}

	public static bool? ShowParentedDialog(this Window dialog)
	{
		dialog.Owner = DialogUtils.GetActiveWindow();
		return dialog.ShowDialog();
	}

	public static void ShowParented(this Window dialog)
	{
		dialog.Owner = DialogUtils.GetActiveWindow();
		dialog.Show();
	}

	public static bool? ShowDialogWithViewModel(this Window dialog, object viewModel)
	{
		dialog.DataContext = viewModel;
		return dialog.ShowParentedDialog();
	}

	public static bool? ShowDialogWithViewModel<TViewModel>(this Window dialog)
	{
		dialog.DataContext = Activator.CreateInstance<TViewModel>();
		return dialog.ShowParentedDialog();
	}

	public static bool? ShowCommonDialogWorkaround(this CommonDialog dialog)
	{
		Window mainWindow = Application.Current.MainWindow;
		Window activeWindow = DialogUtils.GetActiveWindow();
		try
		{
			Application.Current.MainWindow = activeWindow;
			HookWindowActivatedEvents();
			_isDialogOpen = true;
			return dialog.ShowDialog();
		}
		finally
		{
			_isDialogOpen = false;
			UnhookWindowsActivatedEvents();
			Application.Current.MainWindow = mainWindow;
		}
	}

	private static void UnhookWindowsActivatedEvents()
	{
		foreach (object window2 in Application.Current.Windows)
		{
			if (window2 is Window window)
			{
				window.IsHitTestVisible = true;
				window.Activated -= Window_Activated;
			}
		}
	}

	private static void HookWindowActivatedEvents()
	{
		foreach (object window2 in Application.Current.Windows)
		{
			if (window2 is Window window)
			{
				window.IsHitTestVisible = false;
				window.Activated += Window_Activated;
			}
		}
	}

	private static void Window_Activated(object sender, EventArgs e)
	{
		if (_isDialogOpen)
		{
			Application.Current.MainWindow.Activate();
		}
	}
}
