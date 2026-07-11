using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public interface IPrototypingContext : ITreeView, IItemView
{
	void SetActiveItem(object item);

	IDataObject GetInstances(IEnumerable<object> items);
}
