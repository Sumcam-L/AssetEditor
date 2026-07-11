using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Behaviors;

public static class GridViewSortBehavior
{
	public static readonly DependencyProperty CanUserSortColumnsProperty = DependencyProperty.RegisterAttached("CanUserSortColumns", typeof(bool), typeof(GridViewSortBehavior), new FrameworkPropertyMetadata(OnCanUserSortColumnsChanged));

	public static readonly DependencyProperty CanUseSortProperty = DependencyProperty.RegisterAttached("CanUseSort", typeof(bool), typeof(GridViewSortBehavior), new FrameworkPropertyMetadata(true));

	public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached("SortDirection", typeof(ListSortDirection?), typeof(GridViewSortBehavior));

	public static readonly DependencyProperty SortExpressionProperty = DependencyProperty.RegisterAttached("SortExpression", typeof(string), typeof(GridViewSortBehavior));

	[AttachedPropertyBrowsableForType(typeof(ListView))]
	public static bool GetCanUserSortColumns(ListView element)
	{
		return (bool)element.GetValue(CanUserSortColumnsProperty);
	}

	[AttachedPropertyBrowsableForType(typeof(ListView))]
	public static void SetCanUserSortColumns(ListView element, bool value)
	{
		element.SetValue(CanUserSortColumnsProperty, value);
	}

	[AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
	public static bool GetCanUseSort(GridViewColumn element)
	{
		return (bool)element.GetValue(CanUseSortProperty);
	}

	[AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
	public static void SetCanUseSort(GridViewColumn element, bool value)
	{
		element.SetValue(CanUseSortProperty, value);
	}

	[AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
	public static ListSortDirection? GetSortDirection(GridViewColumn element)
	{
		return (ListSortDirection?)element.GetValue(SortDirectionProperty);
	}

	[AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
	public static void SetSortDirection(GridViewColumn element, ListSortDirection? value)
	{
		element.SetValue(SortDirectionProperty, value);
	}

	[AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
	public static string GetSortExpression(GridViewColumn element)
	{
		return (string)element.GetValue(SortExpressionProperty);
	}

	[AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
	public static void SetSortExpression(GridViewColumn element, string value)
	{
		element.SetValue(SortExpressionProperty, value);
	}

	private static void OnCanUserSortColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ListView listView = (ListView)d;
		if ((bool)e.NewValue)
		{
			listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClick));
			if (listView.IsLoaded)
			{
				DoInitialSort(listView);
			}
			else
			{
				listView.Loaded += OnLoaded;
			}
		}
		else
		{
			listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnColumnHeaderClick));
		}
	}

	private static void OnLoaded(object sender, RoutedEventArgs e)
	{
		ListView listView = (ListView)e.Source;
		listView.Loaded -= OnLoaded;
		DoInitialSort(listView);
	}

	private static void DoInitialSort(ListView listView)
	{
		GridView gridView = (GridView)listView.View;
		GridViewColumn gridViewColumn = gridView.Columns.FirstOrDefault((GridViewColumn c) => GetSortDirection(c).HasValue);
		if (gridViewColumn != null)
		{
			DoSort(listView, gridViewColumn);
		}
	}

	private static void OnColumnHeaderClick(object sender, RoutedEventArgs e)
	{
		if (e.OriginalSource is GridViewColumnHeader { Column: not null } gridViewColumnHeader && GetCanUseSort(gridViewColumnHeader.Column))
		{
			DoSort((ListView)e.Source, gridViewColumnHeader.Column);
		}
	}

	private static void DoSort(ListView listView, GridViewColumn newColumn)
	{
		SortDescriptionCollection sortDescriptions = listView.Items.SortDescriptions;
		ListSortDirection listSortDirection = ListSortDirection.Ascending;
		string text = ResolveSortExpression(newColumn);
		if (text == null)
		{
			return;
		}
		if (sortDescriptions.Count > 0)
		{
			if (sortDescriptions[0].PropertyName == text)
			{
				listSortDirection = ((GetSortDirection(newColumn) == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
			}
			else
			{
				GridView gridView = (GridView)listView.View;
				foreach (GridViewColumn item in gridView.Columns.Where((GridViewColumn c) => GetSortDirection(c).HasValue))
				{
					SetSortDirection(item, null);
				}
			}
			sortDescriptions.Clear();
		}
		sortDescriptions.Add(new SortDescription(text, listSortDirection));
		SetSortDirection(newColumn, listSortDirection);
	}

	private static string ResolveSortExpression(GridViewColumn column)
	{
		string sortExpression = GetSortExpression(column);
		if (sortExpression == null)
		{
			return (column.DisplayMemberBinding is Binding binding) ? binding.Path.Path : null;
		}
		return sortExpression;
	}
}
