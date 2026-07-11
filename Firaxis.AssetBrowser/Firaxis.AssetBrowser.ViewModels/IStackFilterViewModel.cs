using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

internal interface IStackFilterViewModel : IFilterViewModel, IViewModel, IDisposable, INotifyPropertyChanged, IComparable<IViewModel>, ISerializable
{
	ObservableCollection<FilterBuilderViewModel> FilterStack { get; }

	ObservableCollection<string> AvailableFilters { get; }

	string SelectedFilterName { get; set; }
}
