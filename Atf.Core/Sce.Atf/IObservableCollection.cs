using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sce.Atf;

public interface IObservableCollection : IList, ICollection, IEnumerable, INotifyPropertyChanged, INotifyCollectionChanged
{
}
public interface IObservableCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, INotifyPropertyChanged, INotifyCollectionChanged
{
}
