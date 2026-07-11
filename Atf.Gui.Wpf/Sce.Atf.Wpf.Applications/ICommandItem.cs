using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Applications;

public interface ICommandItem : IMenuItem, INotifyPropertyChanged, ICommand
{
	object CommandTag { get; }

	bool IsChecked { get; set; }

	object ImageSourceKey { get; set; }

	IEnumerable<Keys> Shortcuts { get; set; }

	CommandVisibility Visibility { get; }

	int Index { get; }
}
