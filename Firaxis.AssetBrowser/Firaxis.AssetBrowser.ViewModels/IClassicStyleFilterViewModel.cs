using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

public interface IClassicStyleFilterViewModel : IFilterViewModel, IViewModel, IDisposable, INotifyPropertyChanged, IComparable<IViewModel>, ISerializable
{
	List<string> FilterByChoices { get; }

	string FilterByChoice { get; set; }

	List<AssetTypeViewModel> AssetTypes { get; }

	bool IsClassFilteringEnabled { get; }

	string ClassFilterDisplay { get; }

	List<ProjectFilterViewModel> Projects { get; }
}
