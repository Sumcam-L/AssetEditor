using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using Firaxis.MVVMBase;
using UtilityTools.ViewModels;

namespace UtilityTools.Views;

public partial class SourceClassAssociationView : Window, IComponentConnector
{
	public SourceClassAssociationView()
	{
		InitializeComponent();
		base.Closing += SourceClassAssociationView_Closing;
	}

	private void SourceClassAssociationView_Closing(object sender, CancelEventArgs e)
	{
		if (base.DataContext is SourceClassAssociationViewModel { Busy: not false })
		{
			MessageBox.Show("Cannot close the window while an import is in progress.");
			e.Cancel = true;
		}
		else if (base.DataContext is DialogViewModel { CloseRequested: false } dialogViewModel)
		{
			dialogViewModel.RegisterWindow(null);
			dialogViewModel.CancelCommand.Execute(null);
			base.Closing -= SourceClassAssociationView_Closing;
		}
	}
}
