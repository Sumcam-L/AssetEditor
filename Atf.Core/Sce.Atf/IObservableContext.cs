using System;

namespace Sce.Atf;

public interface IObservableContext
{
	event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	event EventHandler Reloaded;
}
