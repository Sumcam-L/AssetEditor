using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Sce.Atf.Wpf.Behaviors;

namespace Sce.Atf.Wpf;

public static class VisualTreeExtensions
{
	private static readonly object s_disconnectedItem = typeof(BindingExpressionBase).GetField("DisconnectedItem", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

	public static object SafeGetDataContext(this FrameworkElement e)
	{
		object dataContext = e.DataContext;
		if (s_disconnectedItem != null && dataContext == s_disconnectedItem)
		{
			return null;
		}
		return dataContext;
	}

	public static T GetFrameworkElementByName<T>(this FrameworkElement referenceElement, string name) where T : FrameworkElement
	{
		if (referenceElement == null)
		{
			return null;
		}
		T val = null;
		int childrenCount = VisualTreeHelper.GetChildrenCount(referenceElement);
		for (int i = 0; i < childrenCount; i++)
		{
			FrameworkElement frameworkElement = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
			if (!(frameworkElement is T))
			{
				val = frameworkElement.GetFrameworkElementByName<T>(name);
				if (val != null)
				{
					break;
				}
				continue;
			}
			if (!string.IsNullOrEmpty(name))
			{
				if (frameworkElement.Name == name)
				{
					val = (T)frameworkElement;
					break;
				}
				continue;
			}
			val = (T)frameworkElement;
			break;
		}
		return val;
	}

	public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
	{
		T val = null;
		if (parent == null)
		{
			return val;
		}
		int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
		for (int i = 0; i < childrenCount; i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			val = (child as T) ?? FindVisualChild<T>(child);
			if (val != null)
			{
				break;
			}
		}
		return val;
	}

	public static T GetFrameworkElementByType<T>(this FrameworkElement referenceElement) where T : FrameworkElement
	{
		T val = null;
		if (referenceElement == null)
		{
			return val;
		}
		int childrenCount = VisualTreeHelper.GetChildrenCount(referenceElement);
		for (int i = 0; i < childrenCount; i++)
		{
			FrameworkElement frameworkElement = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
			val = (frameworkElement as T) ?? frameworkElement.GetFrameworkElementByType<T>();
			if (val != null)
			{
				break;
			}
		}
		return val;
	}

	public static IEnumerable<T> GetFrameworkElementsByType<T>(this FrameworkElement referenceElement) where T : FrameworkElement
	{
		FrameworkElement child = null;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement); i++)
		{
			if (child != null)
			{
				break;
			}
			child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
			if (child != null && child.GetType() == typeof(T))
			{
				yield return child as T;
			}
			foreach (T item in child.GetFrameworkElementsByType<T>())
			{
				yield return item;
			}
		}
	}

	public static T FindParent<T>(this DependencyObject obj) where T : DependencyObject
	{
		return (obj == null) ? null : obj.GetAncestors().OfType<T>().FirstOrDefault();
	}

	public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject element)
	{
		Requires.NotNull(element, "element");
		do
		{
			yield return element;
			element = VisualTreeHelper.GetParent(element);
		}
		while (element != null);
	}

	public static T FindAncestor<T>(this DependencyObject dep) where T : class
	{
		return dep.GetLineage().FirstOrDefault((DependencyObject x) => x is T) as T;
	}

	public static IEnumerable<DependencyObject> GetLineage(this DependencyObject dep)
	{
		for (DependencyObject current = dep; current != null; current = current.GetVisualOrLogicalParent())
		{
			yield return current;
		}
	}

	public static IEnumerable<DependencyObject> GetSubtree(this DependencyObject dep)
	{
		Queue<DependencyObject> nodes = new Queue<DependencyObject>();
		nodes.Enqueue(dep);
		while (nodes.Count > 0)
		{
			DependencyObject node = nodes.Dequeue();
			yield return node;
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(node, i);
				if (child != null)
				{
					nodes.Enqueue(child);
				}
			}
		}
	}

	public static DependencyObject GetVisualOrLogicalParent(this DependencyObject dep)
	{
		if (dep == null)
		{
			return null;
		}
		if (dep is ContentElement contentElement)
		{
			DependencyObject parent = ContentOperations.GetParent(contentElement);
			if (parent != null)
			{
				return parent;
			}
			return (contentElement is FrameworkContentElement frameworkContentElement) ? frameworkContentElement.Parent : null;
		}
		if (dep is Visual || dep is Visual3D)
		{
			return VisualTreeHelper.GetParent(dep);
		}
		return LogicalTreeHelper.GetParent(dep);
	}

	public static UIElement GetItemContainerFromChildElement(ItemsControl itemsControl, UIElement child)
	{
		Requires.NotNull(itemsControl, "itemsControl");
		Requires.NotNull(child, "child");
		if (itemsControl.Items.Count > 0 && VisualTreeHelper.GetParent(itemsControl.ItemContainerGenerator.ContainerFromIndex(0)) is Panel panel)
		{
			UIElement uIElement;
			do
			{
				uIElement = VisualTreeHelper.GetParent(child) as UIElement;
				if (uIElement == panel)
				{
					return child;
				}
				child = uIElement;
			}
			while (uIElement != null);
		}
		return null;
	}

	public static object GetItemAtMousePoint(this ItemsControl parent)
	{
		PresentationSource presentationSource = PresentationSource.FromVisual(parent);
		if (presentationSource != null)
		{
			return parent.GetItemAtPoint(MouseUtilities.CorrectGetPosition(parent));
		}
		return null;
	}

	public static DependencyObject GetItemContainerAtPoint(this ItemsControl itemsControl, Point p)
	{
		if (itemsControl is TreeView)
		{
			return ((TreeView)itemsControl).GetItemContainerAtPoint(p);
		}
		object itemAtPoint = itemsControl.GetItemAtPoint(p);
		if (itemAtPoint != null)
		{
			return itemsControl.ItemContainerGenerator.ContainerFromItem(itemAtPoint);
		}
		return null;
	}

	public static object GetItemAtPoint(this ItemsControl itemsControl, Point p)
	{
		if (itemsControl is TreeView)
		{
			return ((TreeView)itemsControl).GetItemAtPoint(p);
		}
		HitTestResult hitTestResult = VisualTreeHelper.HitTest(itemsControl, p);
		if (hitTestResult != null)
		{
			DependencyObject dependencyObject = hitTestResult.VisualHit;
			if (dependencyObject != null)
			{
				while (dependencyObject != null && dependencyObject != itemsControl)
				{
					object obj = itemsControl.ItemContainerGenerator.ItemFromContainer(dependencyObject);
					if (obj != DependencyProperty.UnsetValue)
					{
						return obj;
					}
					dependencyObject = LogicalTreeHelper.GetParent(dependencyObject) ?? VisualTreeHelper.GetParent(dependencyObject);
				}
			}
		}
		return null;
	}

	public static object GetItemAtPoint(this TreeView treeView, Point p)
	{
		TreeViewItem itemContainerAtPoint = treeView.GetItemContainerAtPoint(p);
		ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(itemContainerAtPoint);
		if (itemsControl != null)
		{
			object obj = itemsControl.ItemContainerGenerator.ItemFromContainer(itemContainerAtPoint);
			if (obj != DependencyProperty.UnsetValue)
			{
				return obj;
			}
		}
		return null;
	}

	public static TreeViewItem GetItemContainerAtPoint(this TreeView treeView, Point p)
	{
		if (treeView.InputHitTest(p) is DependencyObject dep)
		{
			return dep.FindAncestor<TreeViewItem>();
		}
		return null;
	}
}
