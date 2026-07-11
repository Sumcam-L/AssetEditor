using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DatabaseWrapper;
using Firaxis.AssetBrowser;
using Firaxis.AssetBrowser.Commands;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

[Export(typeof(IAssetBrowserCommandDefinition))]
public class ReimportSelectedCommand : ICommand, IAssetBrowserCommandDefinition
{
	public string Name => "Reimport";

	public ImageSource Content { get; }

	public ICommand Command => this;

	private ICivTechService CivTechService { get; }

	private ISplashScreenService SplashScreenService { get; }

	private IMessageBoxService MessageBoxService { get; }

	private IDocumentRegistry DocumentRegistry { get; }

	[Import(AllowDefault = true)]
	private ISynchronizeInvoke UIInvoker { get; set; }

	[Import(AllowDefault = true)]
	private IAssetPreviewerService PreviewerService { get; set; }

	public event EventHandler CanExecuteChanged;

	[ImportingConstructor]
	public ReimportSelectedCommand(ICivTechService civTechService, ISplashScreenService splashScreenService, IMessageBoxService messageBoxService, IDocumentRegistry documentRegistry)
	{
		CivTechService = civTechService;
		SplashScreenService = splashScreenService;
		MessageBoxService = messageBoxService;
		DocumentRegistry = documentRegistry;
		Image image = ResourceUtil.GetImage16(Resources.ReimportFileIcon);
		using MemoryStream memoryStream = new MemoryStream();
		image.Save(memoryStream, ImageFormat.Bmp);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		BitmapImage bitmapImage = new BitmapImage();
		bitmapImage.BeginInit();
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.StreamSource = memoryStream;
		bitmapImage.EndInit();
		bitmapImage.Freeze();
		Content = bitmapImage;
	}

	public bool CanExecute(object parameter)
	{
		IEnumerable<InstanceEntityViewModel> viewModels = CommandHelpers.GetViewModels(parameter);
		if (viewModels.Any())
		{
			return viewModels.All((InstanceEntityViewModel vm) => StaticMethods.IsImportableType(vm.InstanceType));
		}
		return false;
	}

	private bool AnyAssetDocumentDependsOnGeometry(IGeometryInstance geo, IEnumerable<AssetDocument> assetDocs)
	{
		return assetDocs.Any((AssetDocument a) => AssetDocumentDependsOnGeometry(geo, a));
	}

	private bool AssetDocumentDependsOnGeometry(IGeometryInstance geo, AssetDocument assetDoc)
	{
		IWorkspaceDependencyRegistry workspaceDependencyRegistry = CivTechService.GetWorkspaceDependencyRegistry(assetDoc.Uri);
		if (workspaceDependencyRegistry == null)
		{
			return false;
		}
		Uri entityThatIsDependedOn = new Uri(geo.GetXMLPath());
		return workspaceDependencyRegistry.DependsOn(assetDoc.Uri, entityThatIsDependedOn);
	}

	private ResultCode PerformCheckout(IImportedEntity entity)
	{
		string entityPath = CivTechService.GetEntityPath(entity.Name, entity.Type);
		string projectName = CivTechService.GetProjectName(entity);
		IVersionControlService versionControl = CivTechService.ActiveProjectMap[projectName].VersionControl;
		bool flag = versionControl.IsVersionControlled(entityPath);
		bool flag2 = versionControl.IsMarkedForDelete(entityPath);
		if (!flag || flag2)
		{
			if (flag2)
			{
				Outputs.WriteLine(OutputMessageType.Info, "Readding marked for delete file {0} to version control", entityPath);
			}
			else if (!flag)
			{
				Outputs.WriteLine(OutputMessageType.Info, "Adding new file {0} to version control. Workspace Root: {1}", entityPath, versionControl.WorkspaceRoot);
			}
			if (!versionControl.AddFile(entityPath, out var errMsg))
			{
				return new ResultCode("Failed to open " + entityPath + " for add in version control.\nYou will be unable to save this file!\n\n" + errMsg);
			}
		}
		else if (!versionControl.IsEditible(entityPath))
		{
			if (!versionControl.GetLatest(entityPath, out var errMsg2))
			{
				return new ResultCode("Failed to get latest " + entityPath + " from version control.\nModifying this file in this state will remove someone's work!\n\nThe file will not be open for edit!\n\n" + errMsg2);
			}
			if (!versionControl.EditFile(entityPath, out errMsg2))
			{
				return new ResultCode("Failed to open " + entityPath + " for edit in version control.\nYou will be unable to save this file!\n\n" + errMsg2);
			}
		}
		return ResultCode.Success;
	}

	public void Execute(object parameter)
	{
		IEnumerable<IImportedEntity> importingEntities = (from vm in CommandHelpers.GetViewModels(parameter)
			select vm.Entity).OfType<IImportedEntity>().ToArray();
		IEnumerable<IImportedEntity> enumerable = importingEntities.Where((IImportedEntity wc) => !CivTechService.IsFromActiveProjectOrDependencies(new EntityID(wc.Name, wc.Type)));
		if (enumerable.Any())
		{
			MessageBoxes.Show("One or more entities being imported is not from the active project or one of its dependencies", "Cannot perform export", MessageBoxButton.OK, MessageBoxImage.Error);
			Outputs.WriteLine(OutputMessageType.Error, "Import failed, the following entities are not from the active project or one of its dependencies:");
			{
				foreach (IImportedEntity item in enumerable)
				{
					Outputs.WriteLine(OutputMessageType.Error, "\t{0}", item.Name);
				}
				return;
			}
		}
		IEnumerable<IImportedEntity> enumerable2 = importingEntities.Where(delegate(IImportedEntity wc)
		{
			string entityPath = CivTechService.GetEntityPath(wc.Name, wc.Type);
			return !File.Exists(entityPath) || File.GetAttributes(entityPath).HasFlag(FileAttributes.ReadOnly);
		});
		if (enumerable2.Any())
		{
			IEnumerable<IImportedEntity> source = enumerable2.Where((IImportedEntity wc) => CivTechRegistry.CivTechService.IsFromActiveProject(new EntityID(wc.Name, wc.Type)));
			if (source.Any())
			{
				IEnumerable<string> values = source.Select((IImportedEntity sc) => CivTechRegistry.CivTechService.GetProjectName(sc)).Distinct();
				if (MessageBoxes.Show(string.Format("Attempting to import entities from the following projects:\n\n{0}\n\nWould you like to continue?", string.Join("\n", values)), "Modifying non-active project entities", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				{
					return;
				}
			}
			IList<Tuple<IImportedEntity, ResultCode>> checkoutResults = new List<Tuple<IImportedEntity, ResultCode>>();
			enumerable2.ForEach(delegate(IImportedEntity fe)
			{
				checkoutResults.Add(new Tuple<IImportedEntity, ResultCode>(fe, PerformCheckout(fe)));
			});
			IEnumerable<string> enumerable3 = from sc in checkoutResults
				where !sc.Item2
				select CivTechService.GetEntityPath(sc.Item1.Name, sc.Item1.Type);
			if (enumerable3.Any() && MessageBoxes.Show(string.Format("The following entities could not be prepared for edit:\n\n{0}\n\nThey will likely fail to export, continue?", string.Join("\n", enumerable3)), "Continue with Export?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				return;
			}
		}
		IEnumerable<ImportOperationResult> results = Enumerable.Empty<ImportOperationResult>();
		AssetDocument[] openAssets = DocumentRegistry.Documents.OfType<AssetDocument>().ToArray();
		IEnumerable<string> ignoreAssetNames = openAssets.Select((AssetDocument doc) => doc.InstanceEntity.Name).ToArray();
		Action action = delegate
		{
			results = global::DatabaseWrapper.DatabaseWrapper.ImportEntities(CivTechService, importingEntities, ignoreAssetNames);
		};
		SplashScreenService.ShowSplashScreen(action, "Importing Entities...", "Importing Entities...");
		IEnumerable<ImportOperationResult> failedResults = results.GetFailedResults();
		if (failedResults.Any())
		{
			MessageBoxes.Show(failedResults.GetCombinedFailureMessages(), "Asset Cloud", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		Action updateAction = delegate
		{
			IEnumerable<IGeometryInstance> enumerable4 = (from result in results.GetValidResults()
				select result.Entity).OfType<IGeometryInstance>();
			if (PreviewerService != null)
			{
				IEntityChangeList changeList = Context.EnsureCreated<CivTechContext>().CreateInstance<IEntityChangeList>();
				changeList.AddGenericEntityChangedEvents(enumerable4.Select((IGeometryInstance x) => new EntityID(x)));
				PreviewerService.SendChanges(changeList);
			}
			foreach (IGeometryInstance geo in enumerable4)
			{
				if (AnyAssetDocumentDependsOnGeometry(geo, openAssets))
				{
					AssetDocument[] array = openAssets;
					foreach (AssetDocument openAssetDocument in array)
					{
						if (AssetDocumentDependsOnGeometry(geo, openAssetDocument))
						{
							ITransactionContext transactionContext = openAssetDocument.As<ITransactionContext>();
							if (transactionContext != null)
							{
								transactionContext.DoTransaction(delegate
								{
									openAssetDocument.UpdateAssetModels(geo);
								}, "Update Asset Models");
							}
							else
							{
								openAssetDocument.UpdateAssetModels(geo);
							}
						}
					}
				}
			}
		};
		if (UIInvoker != null)
		{
			Action action2 = delegate
			{
				UIInvoker.BeginInvoke(updateAction, null).AsyncWaitHandle.WaitOne();
			};
			SplashScreenService.ShowSplashScreen(action2, "Reconciling assets...", "Reconciling assets...");
		}
		else
		{
			SplashScreenService.ShowSplashScreen(updateAction, "Reconciling assets...", "Reconciling assets...");
		}
	}
}
