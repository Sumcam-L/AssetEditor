using System;
using System.Collections.Generic;
using Sce.Atf;

namespace Firaxis.ATF;

public interface IFieldContainerAdapter
{
	IList<IFieldValueAdapter> Fields { get; }

	event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;
}
