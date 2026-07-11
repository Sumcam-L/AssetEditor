using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Firaxis.MVVMBase;

public interface IViewModel : IDisposable, INotifyPropertyChanged, IComparable<IViewModel>
{
	string DisplayName { get; }

	void OnPropertyChanged(string propertyName);

	void OnPropertyChanged(string propertyName, HashSet<string> alreadyNotified);
}
