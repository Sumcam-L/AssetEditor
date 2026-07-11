using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IContextRegistry
{
	object ActiveContext { get; set; }

	IEnumerable<object> Contexts { get; }

	event EventHandler ActiveContextChanging;

	event EventHandler ActiveContextChanged;

	event EventHandler<ItemInsertedEventArgs<object>> ContextAdded;

	event EventHandler<ItemRemovedEventArgs<object>> ContextRemoved;

	T GetActiveContext<T>() where T : class;

	T GetMostRecentContext<T>() where T : class;

	bool RemoveContext(object context);
}
