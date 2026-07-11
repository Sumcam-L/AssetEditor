using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IViewingContext
{
	bool CanFrame(IEnumerable<object> items);

	void Frame(IEnumerable<object> items);

	bool CanEnsureVisible(IEnumerable<object> items);

	void EnsureVisible(IEnumerable<object> items);
}
