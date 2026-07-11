using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Firaxis.AssetBrowser;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.MVVMBase;
using Microsoft.Win32;
using Sce.Atf;
using UtilityTools.ViewModels;
using UtilityTools.Views;

namespace UtilityTools.Helpers;

public class DialogHelper
{
	private static string m_lastSourceFileDirectory = string.Empty;

	public static void DisplayError(string messageToUser)
	{
		DisplayError(messageToUser, "Error");
	}

	public static void DisplayError(string messageToUser, string caption)
	{
		MessageBoxViewModel viewModel = new MessageBoxViewModel(caption, messageToUser);
		MessageBoxView messageBoxView = new MessageBoxView(viewModel);
		messageBoxView.ShowDialog();
	}

	public static bool LaunchAssetCopier(IAssetInstance sourceAsset, IEnumerable<IAssetInstance> targetAssets)
	{
		AssetCopierViewModel assetCopierViewModel = new AssetCopierViewModel(sourceAsset, targetAssets);
		AssetCopierView assetCopierView = new AssetCopierView(assetCopierViewModel);
		assetCopierView.ShowDialog();
		return assetCopierViewModel.UserPressedOK;
	}

	public static bool LaunchEntityNamer(IEnumerable<IInstanceEntity> entities)
	{
		EntityNamerViewModel entityNamerViewModel = new EntityNamerViewModel(entities);
		EntityNamerView entityNamerView = new EntityNamerView();
		entityNamerView.DataContext = entityNamerViewModel;
		entityNamerViewModel.RegisterWindow(entityNamerView);
		entityNamerView.ShowDialog();
		return entityNamerViewModel.UserPressedOK;
	}

	public static void LaunchInstanceEditor(Window owner, UtilityTools.ViewModels.InstanceEntityViewModel viewModel)
	{
		InstanceEntityWindowViewModel viewModel2 = new InstanceEntityWindowViewModel(viewModel);
		InstanceEntityWindowView instanceEntityWindowView = new InstanceEntityWindowView(viewModel2);
		instanceEntityWindowView.Owner = owner;
		instanceEntityWindowView.ShowDialog();
	}

	public static IInstanceEntity LaunchInstanceEditor(ICivTechService civTechSvc, IInstanceEntity instanceEntity, IInstanceSet instances)
	{
		UtilityTools.ViewModels.InstanceEntityViewModel viewModel = UtilityTools.ViewModels.InstanceEntityViewModel.Open(civTechSvc, instanceEntity, instances);
		IInstanceEntity entity = null;
		EventHandler closedHandler = null;
		Action<object, EventArgs> action = delegate(object sender, EventArgs e)
		{
			if (sender is InstanceEntityWindowViewModel instanceEntityWindowViewModel2)
			{
				if (instanceEntityWindowViewModel2.InstanceEntityViewModel.UserSavedEntity)
				{
					entity = instanceEntityWindowViewModel2.InstanceEntityViewModel.InstanceEntity;
				}
				if (closedHandler != null)
				{
					instanceEntityWindowViewModel2.CloseEvent -= closedHandler;
				}
			}
		};
		closedHandler = action.Invoke;
		InstanceEntityWindowViewModel instanceEntityWindowViewModel = new InstanceEntityWindowViewModel(viewModel);
		InstanceEntityWindowView instanceEntityWindowView = new InstanceEntityWindowView(instanceEntityWindowViewModel);
		instanceEntityWindowViewModel.CloseEvent += closedHandler;
		instanceEntityWindowView.ShowDialog();
		return entity;
	}

	public static IEnumerable<EntityID> LaunchMiniImporter(ICivTechService civTechSvc, IFileWatcherService fileWatchSvc, IEnumerable<string> allowedClasses, InstanceType newEntityType, IEntityCacheService entityCacheService, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> canExport, bool singleSelect = false, IEnumerable<IImportedEntity> newEntities = null)
	{
		IEnumerable<EntityID> result = Enumerable.Empty<EntityID>();
		using (MiniImporterViewModel miniImporterViewModel = new MiniImporterViewModel(civTechSvc, fileWatchSvc, allowedClasses, newEntityType, entityCacheService, importFunction, canExport, singleSelect, newEntities))
		{
			MiniImporterView miniImporterView = new MiniImporterView();
			miniImporterView.DataContext = miniImporterViewModel;
			miniImporterViewModel.RegisterWindow(miniImporterView);
			miniImporterView.ShowDialog();
			result = miniImporterViewModel.ImportedEntities.ToArray();
		}
		return result;
	}

	public static IEnumerable<EntityID> LaunchMiniImporter(ICivTechService civTechSvc, IFileWatcherService fileWatchSvc, IEntityContainerEntity entity, InstanceType newEntityType, IEntityCacheService cacheService, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> canExport, bool singleSelect = false, IEnumerable<IImportedEntity> newEntities = null)
	{
		IEnumerable<EntityID> result = Enumerable.Empty<EntityID>();
		IEntityContainerClass parentEntity = civTechSvc.PrimaryProject.Config.Classes.FindForInstance(entity) as IEntityContainerClass;
		using (MiniImporterViewModel miniImporterViewModel = new MiniImporterViewModel(civTechSvc, fileWatchSvc, parentEntity, newEntityType, cacheService, importFunction, canExport, singleSelect, newEntities))
		{
			MiniImporterView miniImporterView = new MiniImporterView();
			miniImporterView.DataContext = miniImporterViewModel;
			miniImporterViewModel.RegisterWindow(miniImporterView);
			miniImporterView.ShowDialog();
			result = miniImporterViewModel.ImportedEntities.ToArray();
		}
		return result;
	}

	public static IEnumerable<IImportedEntity> LaunchSourceClassAssociationView(ICivTechService civTechSvc, IEnumerable<SourceFileModel> sourceFiles, IParameterSet parameters, IValueSet values, InstanceType entityType, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> canExport, IInstanceSet instances = null)
	{
		DateTime sentinelTime = DateTime.Now;
		SourceClassAssociationViewModel sourceClassAssociationViewModel = new SourceClassAssociationViewModel(civTechSvc, parameters, values, sourceFiles, entityType, importFunction, canExport, instances);
		SourceClassAssociationView sourceClassAssociationView = new SourceClassAssociationView();
		sourceClassAssociationView.DataContext = sourceClassAssociationViewModel;
		sourceClassAssociationViewModel.RegisterWindow(sourceClassAssociationView);
		sourceClassAssociationView.ShowDialog();
		List<IImportedEntity> list = new List<IImportedEntity>();
		if (sourceClassAssociationViewModel.UserPressedOK && sourceClassAssociationViewModel.InstancesToImport != null)
		{
			list.AddRange(from ent in sourceClassAssociationViewModel.InstancesToImport.Items.OfType<IImportedEntity>()
				where ent.ExportedTime > sentinelTime
				select ent);
		}
		sourceClassAssociationViewModel.Dispose();
		return list;
	}

	public static IEnumerable<IImportedEntity> LaunchSourceClassAssociationView(ICivTechService civTechSvc, IEnumerable<SourceFileModel> sourceFiles, IObjectParameter parameter, IObjectValue value, InstanceType entityType, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> canExport, IInstanceSet instances = null)
	{
		DateTime sentinelTime = DateTime.Now;
		SourceClassAssociationViewModel sourceClassAssociationViewModel = new SourceClassAssociationViewModel(civTechSvc, parameter, value, sourceFiles, entityType, importFunction, canExport, instances);
		SourceClassAssociationView sourceClassAssociationView = new SourceClassAssociationView();
		sourceClassAssociationView.DataContext = sourceClassAssociationViewModel;
		sourceClassAssociationViewModel.RegisterWindow(sourceClassAssociationView);
		sourceClassAssociationView.ShowDialog();
		List<IImportedEntity> list = new List<IImportedEntity>();
		if (sourceClassAssociationViewModel.UserPressedOK && sourceClassAssociationViewModel.InstancesToImport != null)
		{
			list.AddRange(from ent in sourceClassAssociationViewModel.InstancesToImport.Items.OfType<IImportedEntity>()
				where ent.ExportedTime > sentinelTime
				select ent);
		}
		sourceClassAssociationViewModel.Dispose();
		return list;
	}

	public static IInstanceEntity PickAndLoadAssetObject(Window owner, ICivTechService civTechSvc, InstanceType eType, IEnumerable<string> allowedClasses, IInstanceSet instances)
	{
		return PickAndLoadAssetObject(owner, civTechSvc, new InstanceType[1] { eType }, allowedClasses, instances);
	}

	public static IInstanceEntity PickAndLoadAssetObject(Window owner, ICivTechService civTechSvc, IEnumerable<InstanceType> eTypes, IEnumerable<string> allowedClasses, IInstanceSet instances)
	{
		allowedClasses = allowedClasses?.ToArray();
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, eTypes, allowedClasses, owner);
		if (!flag.HasValue || !flag.Value || !dialogViewModel.HasSingleSelection)
		{
			return null;
		}
		IInstanceEntity instanceByNameAndType = EngineContextWrapper.GetInstanceByNameAndType(dialogViewModel.SelectedType, dialogViewModel.SelectedName, instances);
		if (allowedClasses == null || allowedClasses.Contains(instanceByNameAndType.ClassName))
		{
			return instanceByNameAndType;
		}
		DisplayError("This entity: " + instanceByNameAndType.Name + " has class: " + instanceByNameAndType.ClassName + " which is not allowed here");
		return null;
	}

	public static IEnumerable<IInstanceEntity> PickAndLoadAssetObjects(Window owner, ICivTechService civTechSvc, IEnumerable<InstanceType> eTypes, IEnumerable<string> allowedClasses, IInstanceSet instances)
	{
		allowedClasses = allowedClasses?.ToArray();
		List<IInstanceEntity> list = new List<IInstanceEntity>();
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, eTypes, allowedClasses, owner, allowMultipleSelection: true);
		if (!flag.HasValue || !flag.Value || !dialogViewModel.HasSelection)
		{
			return list;
		}
		foreach (KeyValuePair<string, InstanceType> selectedEntity in dialogViewModel.SelectedEntities)
		{
			IInstanceEntity instanceByNameAndType = EngineContextWrapper.GetInstanceByNameAndType(selectedEntity.Value, selectedEntity.Key, instances);
			if (instanceByNameAndType != null)
			{
				if (allowedClasses == null || !allowedClasses.Any())
				{
					list.Add(instanceByNameAndType);
				}
				else if (allowedClasses.Contains(instanceByNameAndType.ClassName))
				{
					list.Add(instanceByNameAndType);
				}
			}
		}
		return list;
	}

	public static string PickAssetObject(UserControl owner, ICivTechService civTechSvc, InstanceType eType, IEnumerable<string> allowedClasses)
	{
		return PickAssetObject(owner, civTechSvc, new InstanceType[1] { eType }, allowedClasses);
	}

	public static string PickAssetObject(UserControl owner, ICivTechService civTechSvc, IEnumerable<InstanceType> eTypes, IEnumerable<string> allowedClasses)
	{
		allowedClasses = allowedClasses?.ToArray();
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, eTypes, allowedClasses, owner);
		if (flag.HasValue && flag.Value && dialogViewModel.HasSingleSelection)
		{
			return dialogViewModel.SelectedName;
		}
		return "";
	}

	public static bool PromptForBool(string dialogMessage)
	{
		return PromptForBool(dialogMessage, "Alert!");
	}

	public static bool PromptForBool(string dialogMessage, string dialogCaption, MessageBoxImage mbImg = MessageBoxImage.None)
	{
		MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(dialogMessage, dialogCaption, MessageBoxButton.YesNo, mbImg);
		return messageBoxResult == MessageBoxResult.Yes;
	}

	public static string PromptForString(string dialogMessage, string dialogTitle = "")
	{
		PromptForStringDialog promptForStringDialog = new PromptForStringDialog();
		promptForStringDialog.Title = dialogTitle;
		promptForStringDialog.MessagePrompt = dialogMessage;
		if (promptForStringDialog.ShowDialog() != true)
		{
			return null;
		}
		return promptForStringDialog.userString;
	}

	public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int num2;
		for (num2 = str.IndexOf(oldValue, comparison); num2 != -1; num2 = str.IndexOf(oldValue, num2, comparison))
		{
			stringBuilder.Append(str.Substring(num, num2 - num));
			stringBuilder.Append(newValue);
			num2 += oldValue.Length;
			num = num2;
		}
		stringBuilder.Append(str.Substring(num));
		return stringBuilder.ToString();
	}

	public static bool SelectColor(ref Color outColor)
	{
		ColorDialog colorDialog = new ColorDialog();
		colorDialog.SolidColorOnly = true;
		if (colorDialog.ShowDialog() == DialogResult.OK)
		{
			outColor = colorDialog.Color;
			return true;
		}
		return false;
	}

	public static IEnumerable<string> SelectFiles(string filterName, IEnumerable<string> supportedExtensions, bool multiSelect, string dialogTitle = "Open", string initialDirectory = "")
	{
		List<string> list = new List<string>();
		Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
		if (!string.IsNullOrEmpty(initialDirectory))
		{
			openFileDialog.InitialDirectory = initialDirectory;
		}
		IEnumerable<string> enumerable = ((supportedExtensions != null && supportedExtensions.Any()) ? supportedExtensions : new List<string>(new string[1] { ".*" }));
		StringBuilder stringBuilder = new StringBuilder(filterName + " (");
		foreach (string item in enumerable)
		{
			stringBuilder.Append($"*{item};");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		stringBuilder.Append(")|");
		foreach (string item2 in enumerable)
		{
			stringBuilder.Append($"*{item2};");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		openFileDialog.Filter = stringBuilder.ToString();
		openFileDialog.Multiselect = multiSelect;
		openFileDialog.Title = dialogTitle;
		Outputs.WriteLine(OutputMessageType.Info, "Opening file dialog: \"{0}\" \"{1}\"", stringBuilder.ToString(), initialDirectory);
		if (openFileDialog.ShowDialog() == true)
		{
			list.AddRange(openFileDialog.FileNames);
		}
		return list;
	}

	public static string SelectFolder(string description)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.ShowNewFolderButton = false;
		folderBrowserDialog.Description = description;
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			return folderBrowserDialog.SelectedPath;
		}
		return string.Empty;
	}

	public static IEnumerable<string> SelectSourceFiles(ICivTechService civTechSvc, bool multiSelect, InstanceType entityType)
	{
		IEnumerable<IContentExporter> contentExporters = ExporterService.GetContentExporters(entityType);
		List<string> list = new List<string>();
		foreach (IContentExporter item in contentExporters)
		{
			list.AddRange(item.SupportedFileTypes);
		}
		if (string.IsNullOrEmpty(m_lastSourceFileDirectory))
		{
			m_lastSourceFileDirectory = civTechSvc.PrimaryProject.Paths.ArtDev;
		}
		IEnumerable<string> enumerable = SelectFiles("Source Files", list, multiSelect, "Select Source Files", m_lastSourceFileDirectory);
		if (enumerable.Any())
		{
			m_lastSourceFileDirectory = Path.GetDirectoryName(enumerable.First());
		}
		return enumerable;
	}
}
