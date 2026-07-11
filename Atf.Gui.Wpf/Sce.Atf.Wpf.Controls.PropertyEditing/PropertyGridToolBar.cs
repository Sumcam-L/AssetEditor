using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class PropertyGridToolBar : Control
{
	public static readonly ICommand ShowCategorized;

	public static readonly ICommand ShowAlphaSorted;

	public static readonly DependencyProperty IsCategorizedProperty;

	public static readonly DependencyProperty PropertiesProperty;

	private static SortDescription s_alphaSort;

	public bool IsCategorized
	{
		get
		{
			return (bool)GetValue(IsCategorizedProperty);
		}
		set
		{
			SetValue(IsCategorizedProperty, value);
		}
	}

	public IEnumerable Properties
	{
		get
		{
			return (IEnumerable)GetValue(PropertiesProperty);
		}
		set
		{
			SetValue(PropertiesProperty, value);
		}
	}

	private static void PropertiesProperty_Changed(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (!(o is PropertyGridToolBar { Properties: not null } propertyGridToolBar))
		{
			return;
		}
		if (propertyGridToolBar.IsCategorized)
		{
			if (propertyGridToolBar.GetCollectionView().CanSort)
			{
				propertyGridToolBar.ExecuteSort(null, null);
			}
		}
		else if (propertyGridToolBar.GetCollectionView().CanGroup)
		{
			propertyGridToolBar.ExecuteGroup(null, null);
		}
	}

	static PropertyGridToolBar()
	{
		ShowCategorized = new RoutedCommand("ShowCategorized", typeof(PropertyGridToolBar));
		ShowAlphaSorted = new RoutedCommand("ShowAlphaSorted", typeof(PropertyGridToolBar));
		IsCategorizedProperty = DependencyProperty.Register("IsCategorized", typeof(bool), typeof(PropertyGridToolBar));
		PropertiesProperty = DependencyProperty.Register("Properties", typeof(IEnumerable), typeof(PropertyGridToolBar), new FrameworkPropertyMetadata(PropertiesProperty_Changed));
		s_alphaSort = new SortDescription("Descriptor.DisplayName", ListSortDirection.Ascending);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridToolBar), new FrameworkPropertyMetadata(typeof(PropertyGridToolBar)));
	}

	public PropertyGridToolBar()
	{
		base.CommandBindings.Add(new CommandBinding(ShowAlphaSorted, ExecuteSort, CanExecuteSort));
		base.CommandBindings.Add(new CommandBinding(ShowCategorized, ExecuteGroup, CanExecuteGroup));
	}

	private void CanExecuteSort(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = Properties != null && GetCollectionView().CanSort;
	}

	private void ExecuteSort(object sender, ExecutedRoutedEventArgs e)
	{
		Clear();
		ICollectionView collectionView = GetCollectionView();
		collectionView.SortDescriptions.Add(s_alphaSort);
		IsCategorized = true;
	}

	private void CanExecuteGroup(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = Properties != null && GetCollectionView().CanGroup;
	}

	private void ExecuteGroup(object sender, ExecutedRoutedEventArgs e)
	{
		Clear();
		ICollectionView collectionView = GetCollectionView();
		collectionView.GroupDescriptions.Add(DefaultPropertyGrouping.ByCategory);
		IsCategorized = false;
	}

	private void Clear()
	{
		if (Properties != null)
		{
			ICollectionView collectionView = GetCollectionView();
			collectionView.SortDescriptions.Clear();
			collectionView.GroupDescriptions.Clear();
		}
	}

	private ICollectionView GetCollectionView()
	{
		return CollectionViewSource.GetDefaultView(Properties);
	}
}
