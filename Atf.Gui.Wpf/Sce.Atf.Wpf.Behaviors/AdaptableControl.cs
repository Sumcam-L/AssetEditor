using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sce.Atf.Adaptation;
using Sce.Atf.Wpf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Behaviors;

public static class AdaptableControl
{
	private class AdaptableControlAdapter : IAdaptableControl, IAdaptable
	{
		private readonly List<DependencyObject> m_adapters = new List<DependencyObject>();

		private readonly Dictionary<Type, object> m_adapterCache = new Dictionary<Type, object>();

		private FrameworkElement Control { get; set; }

		public AdaptableControlAdapter(FrameworkElement control)
		{
			Requires.NotNull(control, "control");
			Control = control;
		}

		public void Attach(DependencyObject dependencyObject)
		{
			Requires.NotNull(dependencyObject, "dependencyObject");
			m_adapters.Add(dependencyObject);
		}

		public void Detach(DependencyObject dependencyObject)
		{
			Requires.NotNull(dependencyObject, "dependencyObject");
			m_adapters.Remove(dependencyObject);
		}

		public object GetAdapter(Type type)
		{
			if (type.IsAssignableFrom(Control.GetType()))
			{
				return Control;
			}
			return As(type);
		}

		public T As<T>() where T : class
		{
			return As(typeof(T)) as T;
		}

		private object As(Type type)
		{
			if (m_adapterCache.TryGetValue(type, out var value))
			{
				return value;
			}
			for (int num = m_adapters.Count - 1; num >= 0; num--)
			{
				value = m_adapters[num];
				if (type.IsAssignableFrom(value.GetType()))
				{
					m_adapterCache.Add(type, value);
					return value;
				}
			}
			if (type.IsAssignableFrom(Control.GetType()))
			{
				m_adapterCache.Add(type, Control);
				return Control;
			}
			return null;
		}

		public IEnumerable<T> AsAll<T>() where T : class
		{
			return from adapter in m_adapters
				select adapter.As<T>() into t
				where t != null
				select t;
		}
	}

	private static readonly DependencyProperty ControlAdapterProperty = DependencyProperty.RegisterAttached("ControlAdapter", typeof(AdaptableControlAdapter), typeof(AdaptableControl), new PropertyMetadata((object)null));

	public static IAdaptableControl AsAdaptableControl(this FrameworkElement control)
	{
		AdaptableControlAdapter adaptableControlAdapter = GetControlAdapter(control);
		if (adaptableControlAdapter == null)
		{
			adaptableControlAdapter = new AdaptableControlAdapter(control);
			SetControlAdapter(control, adaptableControlAdapter);
		}
		return adaptableControlAdapter;
	}

	public static T ControlAs<T>(this FrameworkElement control) where T : class
	{
		return control.AsAdaptableControl().As<T>();
	}

	public static IEnumerable<T> ControlAsAll<T>(this FrameworkElement control) where T : class
	{
		return control.AsAdaptableControl().AsAll<T>();
	}

	private static void SetControlAdapter(FrameworkElement element, AdaptableControlAdapter value)
	{
		element.SetValue(ControlAdapterProperty, value);
	}

	private static AdaptableControlAdapter GetControlAdapter(FrameworkElement element)
	{
		return (AdaptableControlAdapter)element.GetValue(ControlAdapterProperty);
	}
}
