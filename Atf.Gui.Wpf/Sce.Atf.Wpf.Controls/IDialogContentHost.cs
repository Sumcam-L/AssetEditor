using System;
using System.Windows;

namespace Sce.Atf.Wpf.Controls;

public interface IDialogContentHost
{
	Window Owner { get; set; }

	event EventHandler<HostClosingEventArgs> DialogClosing;

	bool? ShowDialog();

	void RequestClose(bool? dialogResult);
}
