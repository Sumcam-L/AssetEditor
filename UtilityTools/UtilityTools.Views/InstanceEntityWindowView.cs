using System;
using System.Windows;
using System.Windows.Markup;
using UtilityTools.ViewModels;

namespace UtilityTools.Views;

public partial class InstanceEntityWindowView : Window, IComponentConnector
{
	public InstanceEntityWindowViewModel ViewModel => (InstanceEntityWindowViewModel)base.DataContext;

	public InstanceEntityWindowView(InstanceEntityWindowViewModel viewModel)
	{
		InitializeComponent();
		base.DataContext = viewModel;
		ViewModel.CloseEvent += ViewModel_CloseEvent;
		InstanceEntityView element = new InstanceEntityView(viewModel.InstanceEntityViewModel);
		baseGrid.Children.Add(element);
	}

	protected override void OnClosed(EventArgs e)
	{
		ViewModel.InstanceEntityViewModel.RevertTemporaryHiding();
		if (!ViewModel.InstanceEntityViewModel.UserSavedEntity)
		{
			ViewModel.InstanceEntityViewModel.ResetInstance();
		}
		ViewModel.InstanceEntityViewModel.Dispose();
		base.OnClosed(e);
	}

	protected virtual void ViewModel_CloseEvent(object sender, EventArgs e)
	{
		ViewModel.CloseEvent -= ViewModel_CloseEvent;
		base.Dispatcher.BeginInvoke(new Action(base.Close));
	}
}
