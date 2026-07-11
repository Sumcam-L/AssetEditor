using System.ComponentModel;

namespace Sce.Atf.Wpf.Controls;

public class HostClosingEventArgs : CancelEventArgs
{
	public bool? DialogResult { get; set; }
}
