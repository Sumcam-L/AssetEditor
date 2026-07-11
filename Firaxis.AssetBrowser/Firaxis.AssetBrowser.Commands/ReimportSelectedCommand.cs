using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
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
public class ReimportSelectedCommand : ICommand, IAssetBrowserCommandDefinition
{
	public string Name => "Reimport";

	public ImageSource Content { get; }

	public ICommand Command => this;

	private ICivTechService CivTechService { get; }

	private ISplashScreenService SplashScreenService { get; }

	private IMessageBoxService MessageBoxService { get; }

	public event EventHandler CanExecuteChanged;

	[ImportingConstructor]
	public ReimportSelectedCommand(ICivTechService civTechService, ISplashScreenService splashScreenService, IMessageBoxService messageBoxService)
	{
		CivTechService = civTechService;
		SplashScreenService = splashScreenService;
		MessageBoxService = messageBoxService;
		BitmapSource bitmapSource = ImageHelper.CreateBitmapSourceFromBitmap(Resources.Reimport.ToBitmap());
		bitmapSource.Freeze();
		Content = bitmapSource;
	}

	public bool CanExecute(object parameter)
	{
		IEnumerable<InstanceEntityViewModel> viewModels = CommandHelpers.GetViewModels(parameter);
		return viewModels.Any() && viewModels.All((InstanceEntityViewModel vm) => StaticMethods.IsImportableType(vm.InstanceType));
	}

	public void Execute(object parameter)
	{
		IEnumerable<IImportedEntity> importingEntities = (from vm in CommandHelpers.GetViewModels(parameter)
			select vm.Entity).OfType<IImportedEntity>().ToArray();
		IEnumerable<ImportOperationResult> results = Enumerable.Empty<ImportOperationResult>();
		Action action = delegate
		{
			results = global::DatabaseWrapper.DatabaseWrapper.ImportEntities(CivTechService, CivTechService.PrimaryProject.Name, importingEntities);
		};
		string name = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Name;
		SplashScreenService.ShowSplashScreen(action, name, "Importing Entities...");
		IEnumerable<ImportOperationResult> failedResults = results.GetFailedResults();
		if (failedResults.Any())
		{
			string combinedFailureMessages = failedResults.GetCombinedFailureMessages();
			MessageBoxService.Show(combinedFailureMessages, name, MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
