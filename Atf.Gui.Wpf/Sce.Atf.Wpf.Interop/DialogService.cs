using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Interop;

[Export(typeof(IDialogService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DialogService : IDialogService, IInitializable
{
	private class WindowWrapper : System.Windows.Forms.IWin32Window
	{
		public IntPtr Handle { get; private set; }

		public WindowWrapper(IntPtr handle)
		{
			Handle = handle;
		}
	}

	private Window m_mainWindow;

	[ImportingConstructor]
	public DialogService(Window mainWindow)
	{
		m_mainWindow = mainWindow;
	}

	public DialogResult ShowParentedDialog(Form form)
	{
		WindowInteropHelper windowInteropHelper = new WindowInteropHelper(m_mainWindow);
		return form.ShowDialog(new WindowWrapper(windowInteropHelper.Handle));
	}

	public void Initialize()
	{
	}
}
