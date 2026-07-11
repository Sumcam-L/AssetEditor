using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Firaxis.MVVMBase.Helpers;

public static class ApplicationHelper
{
	private static readonly List<DispatcherTimer> _Timers = new List<DispatcherTimer>();

	private static readonly HashSet<string> _ImportedResourceDictionaries = new HashSet<string>();

	public static void ImportResourceDictionary(Type representativeType, string relativePath)
	{
		ImportResourceDictionary(representativeType.Assembly, relativePath);
	}

	public static void ImportResourceDictionary(Assembly assembly, string relativePath)
	{
		string text = $"/{assembly.GetName()};component/{relativePath}";
		if (_ImportedResourceDictionaries.Add(text))
		{
			Application.Current?.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(text, UriKind.Relative)) as ResourceDictionary);
		}
	}

	public static void InvokeIfNeeded(Action action)
	{
		if (Application.Current == null || Application.Current.Dispatcher.Thread == Thread.CurrentThread)
		{
			action();
		}
		else
		{
			Application.Current.Dispatcher.Invoke(action);
		}
	}

	public static void BeginInvokeIfNeeded(Action action)
	{
		if (Application.Current == null || Application.Current.Dispatcher.Thread == Thread.CurrentThread)
		{
			action();
		}
		else
		{
			Application.Current.Dispatcher.BeginInvoke(action);
		}
	}

	public static void InvokeIfNeeded(Dispatcher dispatcher, Action action)
	{
		if (dispatcher == null || dispatcher.Thread == Thread.CurrentThread)
		{
			action();
		}
		else
		{
			dispatcher.Invoke(action);
		}
	}

	public static void BeginInvokeIfNeeded(Dispatcher dispatcher, Action action)
	{
		if (dispatcher == null || dispatcher.Thread == Thread.CurrentThread)
		{
			action();
		}
		else
		{
			dispatcher.BeginInvoke(action);
		}
	}

	public static void ExecuteOnPriority(Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Background)
	{
		DispatcherTimer timer = null;
		timer = new DispatcherTimer(TimeSpan.Zero, dispatcherPriority, delegate
		{
			timer.Stop();
			action();
			_Timers.Remove(timer);
		}, Dispatcher.CurrentDispatcher);
		_Timers.Add(timer);
	}
}
