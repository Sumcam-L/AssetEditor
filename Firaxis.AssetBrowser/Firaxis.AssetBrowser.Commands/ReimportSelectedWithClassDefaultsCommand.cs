using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DatabaseWrapper;
using Firaxis.AssetBrowser.Properties;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.Utility;
using Sce.Atf.Applications;

namespace Firaxis.AssetBrowser.Commands;

[Export(typeof(IAssetBrowserCommandDefinition))]
public class ReimportSelectedWithClassDefaultsCommand : ICommand, IAssetBrowserCommandDefinition
{
	public string Name => "Reimport with Class Defaults";

	public ImageSource Content { get; }

	public ICommand Command => this;

	private ICivTechService CivTechService { get; }

	private ISplashScreenService SplashScreenService { get; }

	private IMessageBoxService MessageBoxService { get; }

	public event EventHandler CanExecuteChanged;

	[ImportingConstructor]
	public ReimportSelectedWithClassDefaultsCommand(ICivTechService civTechService, ISplashScreenService splashScreenService, IMessageBoxService messageBoxService)
	{
		CivTechService = civTechService;
		SplashScreenService = splashScreenService;
		MessageBoxService = messageBoxService;
		BitmapSource bitmapSource = ImageHelper.CreateBitmapSourceFromBitmap(Resources.reimport_texture);
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
		return viewModels.Any() && viewModels.All((InstanceEntityViewModel vm) => vm.InstanceType == InstanceType.IT_TEXTURE);
	}

	public void Execute(object parameter)
	{
		IProjectConfig config = CivTechService.PrimaryProject.Config;
		IEnumerable<ITextureInstance> importingEntities = (from vm in CommandHelpers.GetViewModels(parameter)
			select vm.Entity).OfType<ITextureInstance>().ToArray();
		foreach (ITextureInstance item in importingEntities)
		{
			if (config.Classes.FindForInstance(item) is ITextureClass textureClass)
			{
				item.ExportSettings.AssignFromTextureClass(textureClass);
				item.ExportSettings.SupportScale = textureClass.ExportOptions.DefaultMipSupportScale;
				item.ExportSettings.FilterType = textureClass.ExportOptions.DefaultMipFilter;
			}
		}
		IEnumerable<ImportOperationResult> results = Enumerable.Empty<ImportOperationResult>();
		Action action = delegate
		{
			results = global::DatabaseWrapper.DatabaseWrapper.ImportEntities(CivTechService, CivTechService.PrimaryProject.Name, importingEntities);
		};
		SplashScreenService.ShowSplashScreen(action, "Importing Entities...", "Importing Entities...");
		IEnumerable<ImportOperationResult> failedResults = results.GetFailedResults();
		if (failedResults.Any())
		{
			string combinedFailureMessages = failedResults.GetCombinedFailureMessages();
			MessageBoxService.Show(combinedFailureMessages, "Asset Cloud", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
