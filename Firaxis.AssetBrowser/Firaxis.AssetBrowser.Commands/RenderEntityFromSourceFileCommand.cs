using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
public class RenderEntityFromSourceFileCommand : ICommand, IAssetBrowserCommandDefinition
{
	public string Name => "Render from Source";

	public ImageSource Content { get; }

	public ICommand Command => this;

	private ICivTechService CivTechService { get; }

	private ISplashScreenService SplashScreenService { get; }

	private IMessageBoxService MessageBoxService { get; }

	public event EventHandler CanExecuteChanged;

	[ImportingConstructor]
	public RenderEntityFromSourceFileCommand(ICivTechService civTechService, ISplashScreenService splashScreenService, IMessageBoxService messageBoxService)
	{
		CivTechService = civTechService;
		SplashScreenService = splashScreenService;
		MessageBoxService = messageBoxService;
		BitmapSource bitmapSource = ImageHelper.CreateBitmapSourceFromBitmap(Resources.RenderFromSource);
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
		return viewModels.Any() && viewModels.All((InstanceEntityViewModel vm) => CanRenderFromSource(vm));
	}

	private bool CanRenderFromSource(InstanceEntityViewModel vm)
	{
		if (vm.InstanceType != InstanceType.IT_ANIMATION)
		{
			return false;
		}
		if (!(vm.Entity is IAnimationInstance animationInstance))
		{
			return false;
		}
		string text = CivTechService.PrimaryProject.VersionControl?.GetLocalPath(animationInstance.SourceFilePath);
		if (string.IsNullOrEmpty(text) || !File.Exists(text))
		{
			return false;
		}
		if (string.IsNullOrEmpty(animationInstance.SourceObjectName))
		{
			return false;
		}
		string extension = Path.GetExtension(text);
		return extension.Equals(".ma", StringComparison.CurrentCultureIgnoreCase) || extension.Equals(".mb", StringComparison.CurrentCultureIgnoreCase);
	}

	public void Execute(object parameter)
	{
		FolderBrowserDialog outputFolderDialog = new FolderBrowserDialog();
		outputFolderDialog.Description = "Pick the folder to put the Renders";
		outputFolderDialog.ShowDialog();
		if (!string.IsNullOrWhiteSpace(outputFolderDialog.SelectedPath))
		{
			IEnumerable<IImportedEntity> entities = (from vm in CommandHelpers.GetViewModels(parameter)
				select vm.Entity).OfType<IImportedEntity>().ToArray();
			IEnumerable<ImportOperationResult> results = Enumerable.Empty<ImportOperationResult>();
			Action action = delegate
			{
				results = global::DatabaseWrapper.DatabaseWrapper.RenderEntityFromSource(entities, outputFolderDialog.SelectedPath);
			};
			SplashScreenService.ShowSplashScreen(action, "Rendering Entities...", "Rendering Entities...");
			IEnumerable<ImportOperationResult> failedResults = results.GetFailedResults();
			if (failedResults.Any())
			{
				string combinedFailureMessages = failedResults.GetCombinedFailureMessages();
				MessageBoxService.Show(combinedFailureMessages, "Asset Cloud", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
