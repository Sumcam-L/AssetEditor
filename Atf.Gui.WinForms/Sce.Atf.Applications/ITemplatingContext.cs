using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public interface ITemplatingContext : ITreeView, IItemView
{
	void SetActiveItem(object item);

	IDataObject GetInstances(IEnumerable<object> items);

	bool CanReference(object item);

	object CreateReference(object item);
}
