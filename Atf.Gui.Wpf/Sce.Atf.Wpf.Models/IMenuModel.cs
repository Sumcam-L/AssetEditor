using System.Collections.ObjectModel;

namespace Sce.Atf.Wpf.Models;

public interface IMenuModel
{
	string Text { get; }

	string Description { get; }

	ObservableCollection<object> Children { get; }

	bool IsVisible { get; }
}
