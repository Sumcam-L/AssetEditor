using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface ISelectionContext
{
	IEnumerable<object> Selection { get; set; }

	object LastSelected { get; }

	int SelectionCount { get; }

	event EventHandler SelectionChanging;

	event EventHandler SelectionChanged;

	IEnumerable<T> GetSelection<T>() where T : class;

	T GetLastSelected<T>() where T : class;

	bool SelectionContains(object item);
}
