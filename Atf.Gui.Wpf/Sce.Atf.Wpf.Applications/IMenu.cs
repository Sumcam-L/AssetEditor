using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Applications;

public interface IMenu : IMenuItem, INotifyPropertyChanged
{
	IEnumerable<IMenuItem> Children { get; }

	IEnumerable<ICommandItem> CommandsInSubtree { get; }

	IMenu Parent { get; }
}
