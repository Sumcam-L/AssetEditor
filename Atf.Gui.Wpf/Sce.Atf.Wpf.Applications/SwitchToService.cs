using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SwitchToService : IInitializable
{
	[Import(AllowDefault = true)]
	private MainWindowAdapter m_mainWindow = null;

	[Import]
	private IControlHostService m_controlHostService = null;

	public void Initialize()
	{
		if (m_mainWindow != null)
		{
			m_mainWindow.Loaded += MainWindowLoaded;
		}
	}

	private void MainWindowLoaded(object sender, EventArgs e)
	{
		if (m_mainWindow != null)
		{
			m_mainWindow.MainWindow.PreviewKeyDown += MainWindowOnPreviewKeyDown;
		}
	}

	private void MainWindowOnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (!e.Handled && Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Tab)
		{
			if (!SwitchToDialog.IsInUse)
			{
				new SwitchToDialog(m_controlHostService).ShowParentedDialog();
			}
			else
			{
				SwitchToDialog.FocusCurrentInstance();
			}
			e.Handled = true;
		}
	}
}
