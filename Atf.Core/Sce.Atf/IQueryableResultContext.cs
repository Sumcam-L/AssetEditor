using System;
using System.Collections.Generic;

namespace Sce.Atf;

public interface IQueryableResultContext
{
	IEnumerable<object> Results { get; }

	event EventHandler ResultsChanged;
}
