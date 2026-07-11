using System.Windows;
using System.Windows.Markup;
using UtilityTools.ViewModels;

namespace UtilityTools.Views;

public partial class NameSelectionView : Window, IComponentConnector
{
	public NameSelectionView(NameSelectionViewModel vm)
	{
		InitializeComponent();
		base.DataContext = vm;
		vm.RegisterWindow(this);
	}
}
