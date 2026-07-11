using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;

namespace Firaxis.MVVMBase.Attached;

public class GlobalPropertyCache
{
	private static readonly ConcurrentDictionary<object, ConcurrentDictionary<DependencyProperty, object>> _cache = new ConcurrentDictionary<object, ConcurrentDictionary<DependencyProperty, object>>();

	private static bool _updating = false;

	public static readonly DependencyProperty TokenProperty = DependencyProperty.RegisterAttached("Token", typeof(object), typeof(GlobalPropertyCache), new PropertyMetadata(null, TokenChanged));

	public static readonly DependencyProperty SizeProperty = DependencyProperty.RegisterAttached("Size", typeof(bool), typeof(GlobalPropertyCache), new PropertyMetadata(false, SizeChanged));

	public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached("Location", typeof(bool), typeof(GlobalPropertyCache), new PropertyMetadata(false, LocationChanged));

	public static object GetToken(FrameworkElement target)
	{
		return target.GetValue(TokenProperty);
	}

	public static void SetToken(FrameworkElement target, object value)
	{
		target.SetValue(TokenProperty, value);
	}

	private static void TokenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(sender is FrameworkElement frameworkElement))
		{
			return;
		}
		object tokenOrDefault = GetTokenOrDefault(frameworkElement, e.OldValue);
		object tokenOrDefault2 = GetTokenOrDefault(frameworkElement, e.NewValue);
		if (tokenOrDefault != tokenOrDefault2)
		{
			if (!_cache.TryRemove(tokenOrDefault, out var value))
			{
				value = new ConcurrentDictionary<DependencyProperty, object>();
			}
			_cache.TryAdd(tokenOrDefault2, value);
		}
	}

	public static bool GetSize(FrameworkElement target)
	{
		return (bool)target.GetValue(SizeProperty);
	}

	public static void SetSize(FrameworkElement target, bool value)
	{
		target.SetValue(SizeProperty, value);
	}

	private static void SizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(sender is FrameworkElement frameworkElement))
		{
			return;
		}
		frameworkElement.SizeChanged -= FrameworkElement_SizeChanged;
		frameworkElement.Loaded -= FrameworkElement_Loaded;
		if (!GetSize(frameworkElement))
		{
			return;
		}
		frameworkElement.SizeChanged += FrameworkElement_SizeChanged;
		if (frameworkElement.IsLoaded)
		{
			_updating = true;
			if (GetCacheValue(frameworkElement, FrameworkElement.WidthProperty, out double value))
			{
				frameworkElement.Width = value;
			}
			if (GetCacheValue(frameworkElement, FrameworkElement.HeightProperty, out double value2))
			{
				frameworkElement.Height = value2;
			}
			_updating = false;
		}
		else
		{
			frameworkElement.Loaded += FrameworkElement_Loaded;
		}
	}

	private static void FrameworkElement_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (sender is FrameworkElement { IsLoaded: not false } frameworkElement && !_updating)
		{
			object defaultToken = GetDefaultToken(frameworkElement);
			ConcurrentDictionary<DependencyProperty, object> orAdd = _cache.GetOrAdd(defaultToken, (object o) => new ConcurrentDictionary<DependencyProperty, object>());
			orAdd.AddOrUpdate(FrameworkElement.WidthProperty, e.NewSize.Width, (DependencyProperty dp, object o) => e.NewSize.Width);
			orAdd.AddOrUpdate(FrameworkElement.HeightProperty, e.NewSize.Height, (DependencyProperty dp, object o) => e.NewSize.Height);
		}
	}

	public static bool GetLocation(Window target)
	{
		return (bool)target.GetValue(LocationProperty);
	}

	public static void SetLocation(Window target, bool value)
	{
		target.SetValue(LocationProperty, value);
	}

	private static void LocationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(sender is Window window))
		{
			return;
		}
		window.LocationChanged -= Window_LocationChanged;
		window.Loaded -= FrameworkElement_Loaded;
		if (!GetLocation(window))
		{
			return;
		}
		window.LocationChanged += Window_LocationChanged;
		if (window.IsLoaded)
		{
			_updating = true;
			if (GetCacheValue((FrameworkElement)window, Window.LeftProperty, out double value))
			{
				window.Left = value;
			}
			if (GetCacheValue((FrameworkElement)window, Window.TopProperty, out double value2))
			{
				window.Top = value2;
			}
			_updating = false;
		}
		else
		{
			window.Loaded += FrameworkElement_Loaded;
		}
	}

	private static void Window_LocationChanged(object sender, EventArgs e)
	{
		Window window = sender as Window;
		if (window != null && window.IsLoaded && !_updating)
		{
			object defaultToken = GetDefaultToken(window);
			ConcurrentDictionary<DependencyProperty, object> orAdd = _cache.GetOrAdd(defaultToken, (object o) => new ConcurrentDictionary<DependencyProperty, object>());
			orAdd.AddOrUpdate(Window.LeftProperty, window.Left, (DependencyProperty dp, object o) => window.Left);
			orAdd.AddOrUpdate(Window.TopProperty, window.Top, (DependencyProperty dp, object o) => window.Top);
		}
	}

	private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			frameworkElement.Loaded -= FrameworkElement_Loaded;
			UpdateOnLoad(frameworkElement);
		}
	}

	private static void UpdateOnLoad(FrameworkElement frameworkElement)
	{
		if (!_cache.TryGetValue(GetDefaultToken(frameworkElement), out var value))
		{
			return;
		}
		_updating = true;
		foreach (KeyValuePair<DependencyProperty, object> item in value)
		{
			frameworkElement.SetValue(item.Key, item.Value);
		}
		_updating = false;
	}

	private static object GetTokenOrDefault(FrameworkElement frameworkElement, object potentialToken)
	{
		return potentialToken ?? frameworkElement.GetType();
	}

	private static object GetDefaultToken(FrameworkElement frameworkElement)
	{
		return GetToken(frameworkElement) ?? frameworkElement.GetType();
	}

	private static bool GetCacheValue(object token, DependencyProperty property, out object value)
	{
		if (_cache.TryGetValue(token, out var value2))
		{
			return value2.TryGetValue(property, out value);
		}
		value = null;
		return false;
	}

	private static bool GetCacheValue<T>(object token, DependencyProperty property, out T value)
	{
		if (GetCacheValue(token, property, out var value2) && value2 is T)
		{
			value = (T)value2;
			return true;
		}
		value = default(T);
		return false;
	}

	private static bool GetCacheValue(FrameworkElement frameworkElement, DependencyProperty property, out object value)
	{
		return GetCacheValue(GetDefaultToken(frameworkElement), property, out value);
	}

	private static bool GetCacheValue<T>(FrameworkElement frameworkElement, DependencyProperty property, out T value)
	{
		return GetCacheValue(GetDefaultToken(frameworkElement), property, out value);
	}

	private static void SetCacheValue(object token, DependencyProperty property, object value)
	{
		ConcurrentDictionary<DependencyProperty, object> orAdd = _cache.GetOrAdd(token, (object o) => new ConcurrentDictionary<DependencyProperty, object>());
		orAdd.AddOrUpdate(property, value, (DependencyProperty dp, object o) => value);
	}
}
