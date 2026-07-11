using System;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Docking;

public interface IDockContent : INotifyPropertyChanged
{
	string Header { get; set; }

	object Icon { get; set; }

	string UID { get; }

	bool IsVisible { get; }

	bool IsFocused { get; }

	object Content { get; }

	event EventHandler<BooleanArgs> IsVisibleChanged;

	event EventHandler<BooleanArgs> IsFocusedChanged;

	event ContentClosedEvent Closing;
}
