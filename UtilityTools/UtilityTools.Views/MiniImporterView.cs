using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using UtilityTools.ViewModels;

namespace UtilityTools.Views;

public partial class MiniImporterView : Window, IComponentConnector
{
	private MiniImporterViewModel ViewModel => (MiniImporterViewModel)base.DataContext;

	public MiniImporterView()
	{
		InitializeComponent();
		base.Closing += MiniImporterView_Closing;
	}

	private void MiniImporterView_Closing(object sender, CancelEventArgs e)
	{
		if (!ViewModel.CloseRequested)
		{
			if (ViewModel.Busy)
			{
				e.Cancel = true;
				MessageBox.Show("Cannot close the window when an import is in progress.");
				return;
			}
			ViewModel.Dispose();
		}
		base.Closing -= MiniImporterView_Closing;
	}
}
