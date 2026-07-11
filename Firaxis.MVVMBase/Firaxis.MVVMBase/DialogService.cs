using System;
using System.Collections.Generic;
using System.Windows;

namespace Firaxis.MVVMBase;

public static class DialogService
{
	private static readonly IDictionary<Type, Type> RegisteredWindows;

	static DialogService()
	{
		RegisteredWindows = new Dictionary<Type, Type>();
		RegisterWindow<MessageBoxViewModel, MessageBoxView>();
		RegisterWindow<StringAssignerViewModel, StringAssignerView>();
	}

	public static void RegisterWindow<VMType, WindowType>() where VMType : DialogViewModel where WindowType : Window
	{
		Type typeFromHandle = typeof(VMType);
		Type typeFromHandle2 = typeof(WindowType);
		RegisteredWindows[typeFromHandle] = typeFromHandle2;
	}

	public static bool ShowMessageBox(string title, string message)
	{
		MessageBoxViewModel viewModel = new MessageBoxViewModel(title, message);
		return ShowDialog(viewModel);
	}

	public static bool ShowDialog<T>(T viewModel) where T : DialogViewModel
	{
		if (viewModel == null)
		{
			return false;
		}
		bool result = false;
		Type type = viewModel.GetType();
		Type value = null;
		if (RegisteredWindows.TryGetValue(type, out value) && Activator.CreateInstance(value) is Window window)
		{
			viewModel.RegisterWindow(window);
			window.ShowDialog();
			result = viewModel.UserPressedOK;
		}
		return result;
	}
}
