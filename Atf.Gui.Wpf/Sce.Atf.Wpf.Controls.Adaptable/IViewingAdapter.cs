using System.Collections.Generic;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public interface IViewingAdapter
{
	void Invalidate();

	bool CanFrame(IEnumerable<object> items);

	void Frame(IEnumerable<object> items);

	bool CanEnsureVisible(IEnumerable<object> items);

	void EnsureVisible(IEnumerable<object> items);
}
