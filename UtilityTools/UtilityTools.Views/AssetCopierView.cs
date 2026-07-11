using System.Windows;
using System.Windows.Markup;
using UtilityTools.ViewModels;

namespace UtilityTools.Views;

public partial class AssetCopierView : Window, IComponentConnector
{
	private AssetCopierViewModel ViewModel => base.DataContext as AssetCopierViewModel;

	public AssetCopierView(AssetCopierViewModel vm)
	{
		InitializeComponent();
		base.DataContext = vm;
		vm.RegisterWindow(this);
	}
}
