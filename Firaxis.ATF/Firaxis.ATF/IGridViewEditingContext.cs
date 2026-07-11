using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public abstract class IGridViewEditingContext : IATFEditingContext, IListView, IPropertyEditingContext, IObservableContext, IIndexSelectionContext
{
	public abstract string[] ColumnNames { get; }

	public abstract IEnumerable<object> Items { get; }

	public abstract IEnumerable<PropertyDescriptor> PropertyDescriptors { get; }

	public abstract IEnumerable<int> SelectedIndices { get; set; }

	public abstract IEnumerable<object> SelectedObjects { get; }

	public abstract event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public abstract event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public abstract event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public abstract event EventHandler Reloaded;
}
