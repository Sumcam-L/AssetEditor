using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ItemToCollectionConverter : ConverterMarkupExtension<ItemToCollectionConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		ObservableCollection<object> observableCollection = new ObservableCollection<object>();
		observableCollection.Add(value);
		return observableCollection;
	}
}
