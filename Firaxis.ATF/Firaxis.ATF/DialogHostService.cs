using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IDialogHostService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DialogHostService : IDialogHostService, IInitializable
{
	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow;

	private bool m_windowOpen;

	public bool? ShowDialog(Window window)
	{
		if (m_mainWindow != null && m_windowOpen)
		{
			System.Windows.Forms.IWin32Window dialogOwner = m_mainWindow.DialogOwner;
			new WindowInteropHelper(window).Owner = dialogOwner.Handle;
		}
		return window.ShowDialog();
	}

	public DialogResult ShowDialog(Form window)
	{
		System.Windows.Forms.IWin32Window owner = ((m_mainWindow != null && m_windowOpen) ? m_mainWindow.DialogOwner : null);
		return window.ShowDialog(owner);
	}

	public void Initialize()
	{
		if (m_mainWindow != null)
		{
			m_windowOpen = true;
			m_mainWindow.Closing += _mainWindow_Closing;
		}
	}

	private void _mainWindow_Closing(object sender, CancelEventArgs e)
	{
		m_windowOpen = false;
		m_mainWindow.Closing -= _mainWindow_Closing;
	}
}
