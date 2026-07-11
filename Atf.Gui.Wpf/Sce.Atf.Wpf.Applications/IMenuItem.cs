using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Applications;

public interface IMenuItem : INotifyPropertyChanged
{
	string Text { get; set; }

	string Description { get; set; }

	bool IsVisible { get; }

	object MenuTag { get; }

	object GroupTag { get; }

	IEnumerable<string> MenuPath { get; }
}
