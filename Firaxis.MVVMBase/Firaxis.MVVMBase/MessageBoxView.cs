using System.Windows;
using System.Windows.Markup;

namespace Firaxis.MVVMBase;

public partial class MessageBoxView : Window, IComponentConnector
{
	public MessageBoxViewModel ViewModel => (MessageBoxViewModel)base.DataContext;

	public MessageBoxView(MessageBoxViewModel viewModel)
	{
		InitializeComponent();
		base.DataContext = viewModel;
		ViewModel.RegisterWindow(this);
	}

	public MessageBoxView()
	{
		InitializeComponent();
	}
}
