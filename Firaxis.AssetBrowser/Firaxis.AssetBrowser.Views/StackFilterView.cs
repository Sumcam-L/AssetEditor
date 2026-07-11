using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Firaxis.MVVMBase;
using Firaxis.Utility;

namespace Firaxis.AssetBrowser.Views;

public partial class StackFilterView : UserControl, IComponentConnector
{
	public StackFilterView()
	{
		InitializeComponent();
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		Context.GetService<IWpfSkinService>()?.ApplySkin(this);
	}

	private void UserControl_Unloaded(object sender, RoutedEventArgs e)
	{
		Context.GetService<IWpfSkinService>()?.RemoveSkin(this);
	}
}
