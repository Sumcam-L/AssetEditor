using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface ITreeListView
{
	IEnumerable<object> Roots { get; }

	string[] ColumnNames { get; }

	IEnumerable<object> GetChildren(object parent);
}
