using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Behaviors;

public class AtfTreeViewBehavior : Behavior<TreeView>
{
	public static readonly DependencyProperty IsMultiSelectedProperty = DependencyProperty.RegisterAttached("IsMultiSelected", typeof(bool), typeof(AtfTreeViewBehavior), new PropertyMetadata(false, IsMultiSelected_PropertyChanged));

	public static readonly DependencyProperty EnsureVisiblePathProperty = DependencyProperty.RegisterAttached("EnsureVisiblePath", typeof(Path<Node>), typeof(AtfTreeViewBehavior), new PropertyMetadata(EnsureVisiblePath_PropertyChanged));

	public static readonly DependencyProperty SynchronisingSelectionProperty = DependencyProperty.Register("SynchronisingSelection", typeof(bool), typeof(AtfTreeViewBehavior), new UIPropertyMetadata(false));

	public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached("IsChecked", typeof(bool?), typeof(AtfTreeViewBehavior), new PropertyMetadata(false));

	private static MethodInfo s_bringIntoViewMethod = typeof(VirtualizingStackPanel).GetMethod("BringIndexIntoView", BindingFlags.Instance | BindingFlags.NonPublic);

	private bool m_isMouseDown;

	private bool m_selecting;

	private Node m_leftClickedSelectedNode;

	private Node m_extendSelectionBaseNode;

	public bool SynchronisingSelection
	{
		get
		{
			return (bool)GetValue(SynchronisingSelectionProperty);
		}
		set
		{
			SetValue(SynchronisingSelectionProperty, value);
		}
	}

	private IEnumerable<Node> VisibleNodes
	{
		get
		{
			Stack<Node> nodes = new Stack<Node>();
			foreach (Node node in Roots.Reverse())
			{
				nodes.Push(node);
			}
			while (nodes.Count > 0)
			{
				Node node2 = nodes.Pop();
				yield return node2;
				if (node2.Expanded && node2.ChildrenInternal != null)
				{
					for (int i = node2.ChildrenInternal.Count - 1; i >= 0; i--)
					{
						nodes.Push(node2.ChildrenInternal[i]);
					}
				}
			}
		}
	}

	private IEnumerable<Node> SelectedNodes
	{
		get
		{
			Stack<Node> nodes = new Stack<Node>();
			foreach (Node node in Roots)
			{
				nodes.Push(node);
			}
			while (nodes.Count > 0)
			{
				Node node2 = nodes.Pop();
				if (node2.IsSelected)
				{
					yield return node2;
				}
				if (node2.ChildrenInternal != null)
				{
					for (int i = node2.ChildrenInternal.Count - 1; i >= 0; i--)
					{
						nodes.Push(node2.ChildrenInternal[i]);
					}
				}
			}
		}
	}

	private IEnumerable<Node> Roots
	{
		get
		{
			foreach (object item in AssociatedObject.ItemsSource)
			{
				yield return item as Node;
			}
		}
	}

	private static bool IsCtrlPressed => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

	private static bool IsAltPressed => Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

	private static bool IsShiftPressed => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

	public static bool GetIsMultiSelected(DependencyObject obj)
	{
		return (bool)obj.GetValue(IsMultiSelectedProperty);
	}

	public static void SetIsMultiSelected(DependencyObject obj, bool value)
	{
		obj.SetValue(IsMultiSelectedProperty, value);
	}

	private static void IsMultiSelected_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		TreeViewItem treeViewItem = (TreeViewItem)sender;
		bool flag = (bool)e.NewValue;
	}

	public static Path<Node> GetEnsureVisiblePath(DependencyObject obj)
	{
		return (Path<Node>)obj.GetValue(EnsureVisiblePathProperty);
	}

	public static void SetEnsureVisiblePath(DependencyObject obj, Path<Node> value)
	{
		obj.SetValue(EnsureVisiblePathProperty, value);
	}

	private static void EnsureVisiblePath_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		ItemsControl itemsControl = ((AtfTreeViewBehavior)sender).AssociatedObject;
		if (itemsControl == null)
		{
			return;
		}
		Path<Node> path = e.NewValue as Path<Node>;
		if (!(path != null))
		{
			return;
		}
		Dispatcher.CurrentDispatcher.WaitForPriority(DispatcherPriority.ContextIdle);
		foreach (Node item in path)
		{
			VirtualizingStackPanel frameworkElementByType = itemsControl.GetFrameworkElementByType<VirtualizingStackPanel>();
			TreeViewItem treeViewItem;
			if (frameworkElementByType == null)
			{
				treeViewItem = (TreeViewItem)itemsControl.ItemContainerGenerator.ContainerFromItem(item);
			}
			else
			{
				treeViewItem = (TreeViewItem)itemsControl.ItemContainerGenerator.ContainerFromItem(item);
				if (treeViewItem == null)
				{
					frameworkElementByType.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action<VirtualizingStackPanel, int>(InvokeBringIndexIntoView), frameworkElementByType, item.Index);
					treeViewItem = (TreeViewItem)itemsControl.ItemContainerGenerator.ContainerFromItem(item);
				}
			}
			if (treeViewItem == null)
			{
				break;
			}
			if (item != path.Last && !treeViewItem.IsExpanded)
			{
				treeViewItem.IsExpanded = true;
				Dispatcher.CurrentDispatcher.WaitForPriority(DispatcherPriority.ContextIdle);
			}
			itemsControl = treeViewItem;
			if (item == path.Last)
			{
				itemsControl.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(itemsControl.BringIntoView));
			}
		}
	}

	public static void SetIsChecked(UIElement element, bool? value)
	{
		element.SetValue(IsCheckedProperty, value);
	}

	public static bool? GetIsChecked(UIElement element)
	{
		return (bool?)element.GetValue(IsCheckedProperty);
	}

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.SelectedItemChanged += AssociatedObject_SelectedItemChanged;
		base.AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
		base.AssociatedObject.PreviewMouseUp += AssociatedObject_PreviewMouseUp;
		base.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
		base.AssociatedObject.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.SelectedItemChanged -= AssociatedObject_SelectedItemChanged;
		base.AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
		base.AssociatedObject.PreviewMouseUp -= AssociatedObject_PreviewMouseUp;
		base.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
		base.AssociatedObject.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
	}

	private void AssociatedObject_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (SynchronisingSelection || !(base.AssociatedObject.SelectedItem is Node node))
		{
			return;
		}
		try
		{
			SynchronisingSelection = true;
			if (IsShiftPressed)
			{
				ExtendSelection(node);
			}
			else if (!IsCtrlPressed && !m_isMouseDown)
			{
				SetSelection(node);
			}
			else if (!node.IsSelected)
			{
				node.IsSelected = true;
			}
		}
		finally
		{
			SynchronisingSelection = false;
		}
	}

	private void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		m_isMouseDown = true;
		if (IsAltPressed)
		{
			return;
		}
		bool isExpanderHit;
		TreeViewItem treeViewItemAtPoint = GetTreeViewItemAtPoint(e.GetPosition(base.AssociatedObject), out isExpanderHit);
		if (treeViewItemAtPoint == null || isExpanderHit)
		{
			return;
		}
		Node node = (Node)treeViewItemAtPoint.DataContext;
		try
		{
			SynchronisingSelection = true;
			if (e.ChangedButton == MouseButton.Left)
			{
				m_selecting = true;
				if (IsCtrlPressed)
				{
					e.Handled = true;
				}
				else if (IsShiftPressed)
				{
					ExtendSelection(node);
					treeViewItemAtPoint.Focus();
					e.Handled = true;
				}
				else if (node.IsSelected)
				{
					m_leftClickedSelectedNode = node;
					treeViewItemAtPoint.Focus();
				}
				else
				{
					SetSelection(node);
					treeViewItemAtPoint.Focus();
					e.Handled = true;
				}
			}
			else if (e.ChangedButton == MouseButton.Right && !node.IsSelected)
			{
				m_selecting = true;
				SetSelection(node);
				treeViewItemAtPoint.Focus();
				e.Handled = true;
			}
		}
		finally
		{
			SynchronisingSelection = false;
		}
	}

	private void AssociatedObject_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		m_isMouseDown = false;
		if (m_selecting)
		{
			m_selecting = false;
			bool isExpanderHit;
			TreeViewItem treeViewItemAtPoint = GetTreeViewItemAtPoint(e.GetPosition(base.AssociatedObject), out isExpanderHit);
			if (treeViewItemAtPoint != null && !isExpanderHit)
			{
				Node node = (Node)treeViewItemAtPoint.DataContext;
				if (IsCtrlPressed)
				{
					SynchronisingSelection = true;
					node.IsSelected = !node.IsSelected;
					SynchronisingSelection = false;
					if (node.IsSelected)
					{
						treeViewItemAtPoint.Focus();
					}
				}
				if (node == m_leftClickedSelectedNode && e.ChangedButton == MouseButton.Left)
				{
					try
					{
						SynchronisingSelection = true;
						SetSelection(m_leftClickedSelectedNode);
					}
					finally
					{
						SynchronisingSelection = false;
					}
				}
			}
		}
		m_leftClickedSelectedNode = null;
	}

	private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
	{
		bool flag = false;
		if (e.OriginalSource is TreeViewItem)
		{
			if (e.Key == Key.Space)
			{
				if ((Keyboard.Modifiers & ModifierKeys.Alt) != ModifierKeys.Alt)
				{
					flag = true;
				}
			}
			else if (e.Key == Key.Return && (bool)(sender as DependencyObject).GetValue(KeyboardNavigation.AcceptsReturnProperty))
			{
				flag = true;
			}
		}
		if (flag)
		{
			UpdateIsChecked(e.OriginalSource as UIElement);
			e.Handled = true;
		}
	}

	private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
	{
		if (base.AssociatedObject.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
		{
			Binding binding = new Binding("EnsureVisiblePath");
			BindingOperations.SetBinding(this, EnsureVisiblePathProperty, binding);
			Binding binding2 = new Binding("SynchronisingSelection");
			binding2.Mode = BindingMode.OneWayToSource;
			BindingOperations.SetBinding(this, SynchronisingSelectionProperty, binding2);
		}
	}

	private void UpdateIsChecked(UIElement d)
	{
		bool? checkState = GetIsChecked(d) != true;
		foreach (Node selectedNode in SelectedNodes)
		{
			selectedNode.CheckState = checkState;
		}
	}

	private void SetSelection(Node selected)
	{
		foreach (Node selectedNode in SelectedNodes)
		{
			selectedNode.IsSelected = false;
		}
		if (selected != null)
		{
			selected.IsSelected = true;
		}
		m_extendSelectionBaseNode = selected;
	}

	private void ExtendSelection(Node clickedNode)
	{
		if (m_extendSelectionBaseNode == null)
		{
			return;
		}
		bool flag = false;
		bool? flag2 = null;
		List<Node> list = new List<Node>();
		List<Node> list2 = new List<Node>();
		foreach (Node visibleNode in VisibleNodes)
		{
			if (!flag && visibleNode.IsSelected)
			{
				list2.Add(visibleNode);
			}
			else if (flag && !visibleNode.IsSelected)
			{
				list.Add(visibleNode);
			}
			if (visibleNode == m_extendSelectionBaseNode || visibleNode == clickedNode)
			{
				if (m_extendSelectionBaseNode != clickedNode)
				{
					flag = !flag;
				}
				if (!flag2.HasValue)
				{
					flag2 = visibleNode == clickedNode;
				}
				list.Add(visibleNode);
			}
		}
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].IsSelected = false;
		}
		if (flag2 == true)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].IsSelected = true;
			}
		}
		else
		{
			for (int j = 0; j < list.Count; j++)
			{
				list[j].IsSelected = true;
			}
		}
	}

	private TreeViewItem GetTreeViewItemAtPoint(Point p, out bool isExpanderHit)
	{
		isExpanderHit = false;
		if (base.AssociatedObject.InputHitTest(p) is DependencyObject dep)
		{
			return FindTreeViewItem(dep, out isExpanderHit);
		}
		return null;
	}

	private static TreeViewItem FindTreeViewItem(DependencyObject dep, out bool isExpanderHit)
	{
		isExpanderHit = false;
		for (DependencyObject dependencyObject = dep; dependencyObject != null; dependencyObject = ((!(dependencyObject is Visual) && !(dependencyObject is Visual3D)) ? LogicalTreeHelper.GetParent(dependencyObject) : VisualTreeHelper.GetParent(dependencyObject)))
		{
			if (dependencyObject is ToggleButton)
			{
				isExpanderHit = true;
			}
			if (dependencyObject is TreeViewItem result)
			{
				return result;
			}
		}
		return null;
	}

	private static void InvokeBringIndexIntoView(VirtualizingStackPanel panel, int index)
	{
		ItemsControl itemsOwner = ItemsControl.GetItemsOwner(panel);
		if (itemsOwner != null && index >= 0 && index < itemsOwner.Items.Count)
		{
			s_bringIntoViewMethod.Invoke(panel, new object[1] { index });
		}
	}
}
