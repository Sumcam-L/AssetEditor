using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase.Helpers;

namespace Firaxis.AssetBrowser;

public static class AssetBrowserDialog
{
	public static bool? CreateDialog(out AssetBrowserDialogViewModel dialogViewModel, IEntityFilteringContext filteringContext, Window owner = null, bool allowMultipleSelection = false)
	{
		dialogViewModel = new AssetBrowserDialogViewModel(filteringContext, allowMultipleSelection);
		return WindowFactory.CreateDialogFromContent(dialogViewModel, owner);
	}

	public static bool? CreateDialog(out AssetBrowserDialogViewModel dialogViewModel, IEntityFilteringContext filteringContext, IWin32Window owner, bool allowMultipleSelection = false)
	{
		dialogViewModel = new AssetBrowserDialogViewModel(filteringContext, allowMultipleSelection);
		return WindowFactory.CreateDialogFromContent(dialogViewModel, owner);
	}

	public static bool? CreateDialog(out AssetBrowserDialogViewModel dialogViewModel, IEnumerable<InstanceType> instanceTypes, IEnumerable<string> allowedClasses, Window owner = null, bool allowMultipleSelection = false)
	{
		dialogViewModel = new AssetBrowserDialogViewModel(instanceTypes, allowedClasses, allowMultipleSelection);
		return WindowFactory.CreateDialogFromContent(dialogViewModel, owner);
	}

	public static bool? CreateDialog(out AssetBrowserDialogViewModel dialogViewModel, IEnumerable<InstanceType> instanceTypes, IEnumerable<string> allowedClasses, IWin32Window owner, bool allowMultipleSelection = false)
	{
		dialogViewModel = new AssetBrowserDialogViewModel(instanceTypes, allowedClasses, allowMultipleSelection);
		return WindowFactory.CreateDialogFromContent(dialogViewModel, owner);
	}
}
