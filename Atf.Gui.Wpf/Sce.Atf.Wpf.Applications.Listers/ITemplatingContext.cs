using System.Collections.Generic;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications.Listers;

public interface ITemplatingContext : ITreeView, IItemView
{
	void SetActiveItem(object item);

	IDataObject GetInstances(IEnumerable<object> items);

	bool CanReference(object item);

	object CreateReference(object item);
}
