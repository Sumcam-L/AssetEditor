using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public interface IMainWindow
{
	string Text { get; set; }

	IWin32Window DialogOwner { get; }

	event EventHandler Loading;

	event EventHandler Loaded;

	event CancelEventHandler Closing;

	event EventHandler Closed;

	void Close();
}
