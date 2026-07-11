using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IListView
{
	string[] ColumnNames { get; }

	IEnumerable<object> Items { get; }
}
