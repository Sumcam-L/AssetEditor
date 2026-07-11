using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Firaxis.MVVMBase.Controls;
using Firaxis.MVVMBase.Extensions;

namespace Firaxis.MVVMBase.Attached;

public class ListViewHelper
{
	public static readonly DependencyProperty SortOnColumnHeaderClickProperty = DependencyProperty.RegisterAttached("SortOnColumnHeaderClick", typeof(bool), typeof(ListViewHelper), new PropertyMetadata(false, SortOnColumnHeaderClickChanged));

	private static readonly DependencyPropertyKey SortOnColumnHeaderClickHandlerPropertyKey = DependencyProperty.RegisterAttachedReadOnly("SortOnColumnHeaderClickHandler", typeof(RoutedEventHandler), typeof(ListViewHelper), new PropertyMetadata(null));

	private static readonly DependencyProperty SortOnColumnHeaderClickHandlerProperty = SortOnColumnHeaderClickHandlerPropertyKey.DependencyProperty;

	public static readonly DependencyProperty SortPropertyNameProperty = DependencyProperty.RegisterAttached("SortPropertyName", typeof(string), typeof(ListViewHelper), new PropertyMetadata(null, SortPropertyNameChanged));

	public static readonly DependencyProperty InitialSortProperty = DependencyProperty.RegisterAttached("InitialSort", typeof(bool), typeof(ListViewHelper), new PropertyMetadata(false, InitialSortChanged));

	public static bool GetSortOnColumnHeaderClick(ListView target)
	{
		return (bool)target.GetValue(SortOnColumnHeaderClickProperty);
	}

	public static void SetSortOnColumnHeaderClick(ListView target, bool value)
	{
		target.SetValue(SortOnColumnHeaderClickProperty, value);
	}

	private static void SortOnColumnHeaderClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is ListView listView)
		{
			RoutedEventHandler sortOnColumnHeaderClickHandler = GetSortOnColumnHeaderClickHandler(listView);
			if (sortOnColumnHeaderClickHandler != null)
			{
				listView.RemoveHandler(ButtonBase.ClickEvent, sortOnColumnHeaderClickHandler);
			}
			if (GetSortOnColumnHeaderClick(listView))
			{
				sortOnColumnHeaderClickHandler = ListView_Click;
				SetSortOnColumnHeaderClickHandler(listView, sortOnColumnHeaderClickHandler);
				listView.AddHandler(ButtonBase.ClickEvent, sortOnColumnHeaderClickHandler);
			}
		}
	}

	private static void ListView_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is ListView listView) || !(e.OriginalSource is GridViewColumnHeader gridViewColumnHeader))
		{
			return;
		}
		string sortPropertyName = GetSortPropertyName(gridViewColumnHeader);
		if (string.IsNullOrWhiteSpace(sortPropertyName))
		{
			return;
		}
		GridViewHeaderRowPresenter visualTreeAncestorByType = gridViewColumnHeader.GetVisualTreeAncestorByType<GridViewHeaderRowPresenter>();
		if (visualTreeAncestorByType == null)
		{
			return;
		}
		int num = -1;
		ListSortDirection listSortDirection = ListSortDirection.Ascending;
		SortDescription[] array = null;
		if (listView.Items.SortDescriptions.Count > 0)
		{
			array = listView.Items.SortDescriptions.ToArray();
			listView.Items.SortDescriptions.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				SortDescription sortDescription = array[i];
				if (!(sortDescription.PropertyName != sortPropertyName))
				{
					if (i == 0 && sortDescription.Direction == ListSortDirection.Ascending)
					{
						listSortDirection = ListSortDirection.Descending;
					}
					num = i;
					break;
				}
			}
		}
		listView.Items.SortDescriptions.Add(new SortDescription(sortPropertyName, listSortDirection));
		if (array != null)
		{
			for (int j = 0; j < array.Length && j != num; j++)
			{
				SortDescription item = array[j];
				listView.Items.SortDescriptions.Add(item);
			}
		}
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(gridViewColumnHeader);
		foreach (GridViewColumnHeader item2 in visualTreeAncestorByType.GetVisualChildrenByType<GridViewColumnHeader>())
		{
			string text = ((item2 == gridViewColumnHeader) ? sortPropertyName : GetSortPropertyName(item2));
			if (text == null)
			{
				continue;
			}
			Adorner[] adorners = adornerLayer.GetAdorners(item2);
			if (adorners == null || adorners.Length == 0)
			{
				if (item2 == gridViewColumnHeader)
				{
					adornerLayer.Add(new SortAdorner(gridViewColumnHeader, listSortDirection));
				}
			}
			else if (adorners[0] is SortAdorner sortAdorner)
			{
				if (item2 == gridViewColumnHeader)
				{
					sortAdorner.Direction = listSortDirection;
					sortAdorner.IsPrimarySort = true;
				}
				else
				{
					sortAdorner.IsPrimarySort = false;
				}
			}
		}
		e.Handled = true;
	}

	private static RoutedEventHandler GetSortOnColumnHeaderClickHandler(ListView target)
	{
		return (RoutedEventHandler)target.GetValue(SortOnColumnHeaderClickHandlerProperty);
	}

	private static void SetSortOnColumnHeaderClickHandler(ListView target, RoutedEventHandler value)
	{
		target.SetValue(SortOnColumnHeaderClickHandlerPropertyKey, value);
	}

	public static string GetSortPropertyName(GridViewColumnHeader target)
	{
		return (string)target.GetValue(SortPropertyNameProperty);
	}

	public static void SetSortPropertyName(GridViewColumnHeader target, string value)
	{
		target.SetValue(SortPropertyNameProperty, value);
	}

	private static void SortPropertyNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is GridViewColumnHeader gridViewColumnHeader && GetInitialSort(gridViewColumnHeader))
		{
			SetUpSort(gridViewColumnHeader.GetVisualTreeAncestorByType<ListView>(), gridViewColumnHeader);
		}
	}

	public static bool GetInitialSort(GridViewColumnHeader target)
	{
		return (bool)target.GetValue(InitialSortProperty);
	}

	public static void SetInitialSort(GridViewColumnHeader target, bool value)
	{
		target.SetValue(InitialSortProperty, value);
	}

	private static void InitialSortChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(sender is GridViewColumnHeader gridViewColumnHeader))
		{
			return;
		}
		ListView visualTreeAncestorByType = gridViewColumnHeader.GetVisualTreeAncestorByType<ListView>();
		if (visualTreeAncestorByType == null)
		{
			gridViewColumnHeader.Loaded -= GridViewColumnHeader_Loaded;
			if (GetInitialSort(gridViewColumnHeader))
			{
				gridViewColumnHeader.Loaded += GridViewColumnHeader_Loaded;
			}
		}
		else
		{
			SetUpSort(visualTreeAncestorByType, gridViewColumnHeader);
		}
	}

	private static void GridViewColumnHeader_Loaded(object sender, RoutedEventArgs e)
	{
		GridViewColumnHeader gridViewColumnHeader = sender as GridViewColumnHeader;
		SetUpSort(gridViewColumnHeader?.GetVisualTreeAncestorByType<ListView>(), gridViewColumnHeader);
	}

	private static void SetUpSort(ListView listView, GridViewColumnHeader gridViewColumnHeader)
	{
		if (listView != null && gridViewColumnHeader != null)
		{
			string sortByName = GetSortPropertyName(gridViewColumnHeader);
			if (!string.IsNullOrWhiteSpace(sortByName) && !listView.Items.SortDescriptions.Any((SortDescription s) => s.PropertyName == sortByName))
			{
				listView.Items.SortDescriptions.Add(new SortDescription(sortByName, ListSortDirection.Ascending));
				AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(gridViewColumnHeader);
				adornerLayer.Add(new SortAdorner(gridViewColumnHeader, ListSortDirection.Ascending));
			}
		}
	}
}
