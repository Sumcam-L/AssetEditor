using System;
using System.ComponentModel;

namespace Firaxis.MVVMBase;

[Serializable]
public abstract class Notifier : INotifyPropertyChanged
{
	[field: NonSerialized]
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
