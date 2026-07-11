using System.Windows;
using System.Windows.Markup;

namespace Firaxis.MVVMBase;

public partial class StringAssignerView : Window, IComponentConnector
{
	private StringAssignerViewModel ViewModel => base.DataContext as StringAssignerViewModel;

	public StringAssignerView()
	{
		InitializeComponent();
	}
}
