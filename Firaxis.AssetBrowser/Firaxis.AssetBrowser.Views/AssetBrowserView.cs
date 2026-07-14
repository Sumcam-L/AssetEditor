using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Firaxis.MVVMBase;
using Firaxis.Utility;

namespace Firaxis.AssetBrowser.Views;

public partial class AssetBrowserView : UserControl, IComponentConnector
{
	public AssetBrowserView()
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

	//[DebuggerNonUserCode]
	//[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	//internal Delegate _CreateDelegate(Type delegateType, string handler)
	//{
	//	return Delegate.CreateDelegate(delegateType, this, handler);
	//}
}
