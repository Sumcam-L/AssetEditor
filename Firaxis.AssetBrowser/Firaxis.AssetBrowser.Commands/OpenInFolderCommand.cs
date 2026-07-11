using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Firaxis.AssetBrowser.Properties;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech;
using Firaxis.Utility;

namespace Firaxis.AssetBrowser.Commands;

[Export(typeof(IAssetBrowserCommandDefinition))]
public class OpenInFolderCommand : ICommand, IAssetBrowserCommandDefinition
{
	public string Name => "Show in Folder";

	public ImageSource Content { get; }

	public ICommand Command => this;

	private ICivTechService CivTechService { get; }

	public event EventHandler CanExecuteChanged;

	[ImportingConstructor]
	public OpenInFolderCommand(ICivTechService civTechService)
	{
		CivTechService = civTechService;
		BitmapSource bitmapSource = ImageHelper.CreateBitmapSourceFromBitmap(Resources.OpenSource.ToBitmap());
		bitmapSource.Freeze();
		Content = bitmapSource;
	}

	public bool CanExecute(object parameter)
	{
		if (CivTechService == null)
		{
			return false;
		}
		IEnumerable<InstanceEntityViewModel> viewModels = CommandHelpers.GetViewModels(parameter);
		int num = 0;
		foreach (InstanceEntityViewModel item in viewModels)
		{
			if (++num > 1)
			{
				return false;
			}
		}
		return num == 1;
	}

	public void Execute(object parameter)
	{
		IEnumerable<InstanceEntityViewModel> viewModels = CommandHelpers.GetViewModels(parameter);
		string xMLPath = viewModels.First().Entity.GetXMLPath();
		xMLPath = xMLPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		string arguments = $"/select, \"{xMLPath}\"";
		Process.Start("explorer.exe", arguments);
	}
}
