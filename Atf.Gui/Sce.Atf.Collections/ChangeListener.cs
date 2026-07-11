using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sce.Atf.Collections;

public abstract class ChangeListener : INotifyPropertyChanged, IDisposable
{
	protected string PropertyName;

	public event PropertyChangedEventHandler PropertyChanged;

	protected abstract void Unsubscribe();

	protected virtual void RaisePropertyChanged(object sender, string propertyName)
	{
		OnPropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(sender, e);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Unsubscribe();
		}
	}

	~ChangeListener()
	{
		Dispose(disposing: false);
	}

	public static ChangeListener Create(INotifyPropertyChanged value)
	{
		return Create(value, null);
	}

	public static ChangeListener Create(INotifyPropertyChanged value, string propertyName)
	{
		if (value is INotifyCollectionChanged)
		{
			return new CollectionChangeListener(value as INotifyCollectionChanged, propertyName);
		}
		return new ChildChangeListener(value, propertyName);
	}
}
