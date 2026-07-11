using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IIndexSelectionContext
{
	IEnumerable<int> SelectedIndices { get; set; }

	IEnumerable<object> SelectedObjects { get; }
}
