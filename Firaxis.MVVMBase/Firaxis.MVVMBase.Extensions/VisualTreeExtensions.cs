using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Firaxis.MVVMBase.Extensions;

public static class VisualTreeExtensions
{
	public static DependencyObject GetVisualTreeAncestorByType(this DependencyObject original, Type type)
	{
		if (!typeof(DependencyObject).IsAssignableFrom(type))
		{
			throw new ArgumentException("Must be a type descended from DependencyObject", "type");
		}
		DependencyObject parent = VisualTreeHelper.GetParent(original);
		while (parent != null && !type.IsInstanceOfType(parent))
		{
			parent = VisualTreeHelper.GetParent(parent);
		}
		return parent;
	}

	public static T GetVisualTreeAncestorByType<T>(this DependencyObject original) where T : DependencyObject
	{
		DependencyObject parent = VisualTreeHelper.GetParent(original);
		while (parent != null && !(parent is T))
		{
			parent = VisualTreeHelper.GetParent(parent);
		}
		return parent as T;
	}

	public static IEnumerable<T> GetVisualChildrenByType<T>(this DependencyObject original) where T : DependencyObject
	{
		if (original == null)
		{
			yield break;
		}
		int count = VisualTreeHelper.GetChildrenCount(original);
		int c = 0;
		while (c < count)
		{
			DependencyObject child = VisualTreeHelper.GetChild(original, c);
			if (child is T tChild)
			{
				yield return tChild;
			}
			int num = c + 1;
			c = num;
		}
	}

	public static IEnumerable<T> GetVisualDescendantsByType<T>(this DependencyObject original) where T : DependencyObject
	{
		if (original == null)
		{
			yield break;
		}
		int count = VisualTreeHelper.GetChildrenCount(original);
		int c = 0;
		while (c < count)
		{
			DependencyObject child = VisualTreeHelper.GetChild(original, c);
			if (child is T tChild)
			{
				yield return tChild;
			}
			foreach (T item in child.GetVisualDescendantsByType<T>())
			{
				yield return item;
			}
			int num = c + 1;
			c = num;
		}
	}
}
