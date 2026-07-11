using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

public interface IFilterViewModel : IViewModel, IDisposable, INotifyPropertyChanged, IComparable<IViewModel>, System.Runtime.Serialization.ISerializable
{
	string FilterName { get; }

	string FilterText { get; set; }

	event EventHandler FilterChanged;

	IEntityFilterSet GetFilterSet();
}
