using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Sce.Atf.Wpf;

public class DataContextSpy : Freezable
{
	public static readonly DependencyProperty DataContextProperty = FrameworkElement.DataContextProperty.AddOwner(typeof(DataContextSpy), new PropertyMetadata(null, null, OnCoerceDataContext));

	public bool IsSynchronizedWithCurrentItem { get; set; }

	public object DataContext
	{
		get
		{
			return GetValue(DataContextProperty);
		}
		set
		{
			SetValue(DataContextProperty, value);
		}
	}

	public DataContextSpy()
	{
		BindingOperations.SetBinding(this, DataContextProperty, new Binding());
		IsSynchronizedWithCurrentItem = true;
	}

	protected override Freezable CreateInstanceCore()
	{
		throw new NotImplementedException();
	}

	private static object OnCoerceDataContext(DependencyObject depObj, object value)
	{
		if (!(depObj is DataContextSpy dataContextSpy))
		{
			return value;
		}
		if (dataContextSpy.IsSynchronizedWithCurrentItem)
		{
			ICollectionView defaultView = CollectionViewSource.GetDefaultView(value);
			if (defaultView != null)
			{
				return defaultView.CurrentItem;
			}
		}
		return value;
	}
}
