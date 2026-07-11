using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.MVVMBase;
using Firaxis.Reflection;
using Firaxis.Utility;
using Sce.Atf;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels;

public class MiniImporterViewModel : DialogViewModel, IDisposable
{
	private DelegateCommand m_assignClassToChildrenCommand;

	private DelegateCommand m_openFileCommand;

	private DelegateCommand m_overwriteAllCommand;

	private DelegateCommand m_selectAllCommand;

	private DelegateCommand m_sortSourcesCommand;

	private ObservableCollection<string> m_availableClassNames;

	private string m_selectedAssignableClass;

	private bool m_singleSelectOnly = false;

	private ObservableCollection<MiniSourceObjectViewModel> m_sourceObjects;

	private static string m_lastUsedDirectory = string.Empty;

	private readonly Predicate<IImportedEntity> m_exportPredicate;

	private readonly List<EntityID> m_importedEntities = new List<EntityID>();

	public ObservableCollection<string> AvailableClassNames
	{
		get
		{
			return m_availableClassNames;
		}
		set
		{
			if (m_availableClassNames != value)
			{
				m_availableClassNames = value;
				OnPropertyChanged("AvailableClassNames");
			}
		}
	}

	public bool Busy { get; private set; }

	public IEnumerable<EntityID> ImportedEntities => m_importedEntities;

	public InstanceType NewEntityType { get; private set; }

	public string NewEntityTypeName => ReflectionHelper.GetDisplayName(NewEntityType);

	public string SelectedAssignableClass
	{
		get
		{
			return m_selectedAssignableClass;
		}
		set
		{
			if (m_selectedAssignableClass != value)
			{
				m_selectedAssignableClass = value;
				OnPropertyChanged("SelectedAssignableClass");
			}
		}
	}

	public bool SingleSelectOnly
	{
		get
		{
			return m_singleSelectOnly;
		}
		set
		{
			if (m_singleSelectOnly != value)
			{
				m_singleSelectOnly = value;
				OnPropertyChanged("SingleSelectOnly");
			}
		}
	}

	public ObservableCollection<MiniSourceObjectViewModel> SourceObjects
	{
		get
		{
			return m_sourceObjects;
		}
		set
		{
			if (m_sourceObjects != value)
			{
				m_sourceObjects = value;
				OnPropertyChanged("SourceObjects");
			}
		}
	}

	public bool SourcesExist => SourceObjects.Count > 0;

	protected ICivTechService CivTechService { get; private set; }

	private Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> ImportFunction { get; set; }

	private IEntityCacheService CacheService { get; }

	private IInstanceSet InstanceSet { get; }

	private string LastUsedDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_lastUsedDirectory))
			{
				m_lastUsedDirectory = CivTechService.PrimaryProject.Paths.ArtDev;
			}
			return m_lastUsedDirectory;
		}
		set
		{
			m_lastUsedDirectory = value;
		}
	}

	public ICommand AssignClassToChildrenCommand
	{
		get
		{
			if (m_assignClassToChildrenCommand == null)
			{
				m_assignClassToChildrenCommand = new DelegateCommand(ExecuteAssignClassToChildrenCommand);
			}
			return m_assignClassToChildrenCommand;
		}
	}

	public ICommand OpenFileCommand
	{
		get
		{
			if (m_openFileCommand == null)
			{
				m_openFileCommand = new DelegateCommand(ExecuteOpenFileCommand, CanExecuteOpenFileCommand);
			}
			return m_openFileCommand;
		}
	}

	public ICommand OverwriteAllCommand
	{
		get
		{
			if (m_overwriteAllCommand == null)
			{
				m_overwriteAllCommand = new DelegateCommand(ExecuteOverwriteAllCommand);
			}
			return m_overwriteAllCommand;
		}
	}

	public ICommand SelectAllCommand
	{
		get
		{
			if (m_selectAllCommand == null)
			{
				m_selectAllCommand = new DelegateCommand(ExecuteSelectAllCommand);
			}
			return m_selectAllCommand;
		}
	}

	public ICommand SortSourcesCommand
	{
		get
		{
			if (m_sortSourcesCommand == null)
			{
				m_sortSourcesCommand = new DelegateCommand(ExecuteSortSourcesCommand);
			}
			return m_sortSourcesCommand;
		}
	}

	public MiniImporterViewModel(ICivTechService civTechSvc, IFileWatcherService fileWatchSvc, IEnumerable<string> allowedEntityClasses, InstanceType newEntityType, IEntityCacheService entityCacheService, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> exportPredicate, bool singleSelectOnly, IEnumerable<IImportedEntity> newEntities)
	{
		CivTechService = civTechSvc;
		NewEntityType = newEntityType;
		CacheService = entityCacheService;
		SourceObjects = new ObservableCollection<MiniSourceObjectViewModel>();
		SingleSelectOnly = singleSelectOnly;
		ImportFunction = importFunction;
		m_exportPredicate = exportPredicate;
		if (fileWatchSvc.UseSinglePantryMode)
		{
			IEnumerable<string> enumerable = new string[1] { civTechSvc.GetBaseGamePantryPath() };
			InstanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { enumerable });
		}
		else
		{
			InstanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { civTechSvc.GetActivePantryPaths() });
		}
		Busy = false;
		AvailableClassNames = new ObservableCollection<string>(allowedEntityClasses);
		SelectedAssignableClass = AvailableClassNames.FirstOrDefault();
		if (newEntities == null)
		{
			return;
		}
		foreach (IImportedEntity newEntity in newEntities)
		{
			MiniSourceObjectViewModel miniSourceObjectViewModel = new MiniSourceObjectViewModel(CivTechService, newEntity, AvailableClassNames, entityCacheService);
			miniSourceObjectViewModel.IsSelected = !singleSelectOnly;
			miniSourceObjectViewModel.PropertyChanged += sourceObjectVM_PropertyChanged;
			SourceObjects.Add(miniSourceObjectViewModel);
		}
	}

	public MiniImporterViewModel(ICivTechService civTechSvc, IFileWatcherService fileWatchSvc, IEntityContainerClass parentClass, InstanceType newEntityType, IEntityCacheService entityCacheService, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> exportPredicate)
		: this(civTechSvc, fileWatchSvc, parentClass, newEntityType, entityCacheService, importFunction, exportPredicate, singleSelectOnly: false, null)
	{
	}

	public MiniImporterViewModel(ICivTechService civTechSvc, IFileWatcherService fileWatchSvc, IEntityContainerClass parentEntity, InstanceType newEntityType, IEntityCacheService entityCacheService, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> exportPredicate, bool singleSelectOnly, IEnumerable<IImportedEntity> newEntities)
		: this(civTechSvc, fileWatchSvc, parentEntity.GetAllowedClasses(newEntityType), newEntityType, entityCacheService, importFunction, exportPredicate, singleSelectOnly, newEntities)
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected override bool CanExecuteCancelCommand(object context)
	{
		return !Busy;
	}

	protected override bool CanExecuteOKCommand(object context)
	{
		return !Busy;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing || SourceObjects == null)
		{
			return;
		}
		foreach (MiniSourceObjectViewModel sourceObject in SourceObjects)
		{
			sourceObject.PropertyChanged -= sourceObjectVM_PropertyChanged;
		}
		SourceObjects.Clear();
		SourceObjects = null;
		m_importedEntities.Clear();
	}

	protected override void ExecuteCancelCommand(object context)
	{
		if (!Busy)
		{
			base.ExecuteCancelCommand(context);
		}
	}

	protected override void ExecuteOKCommand(object context)
	{
		RefreshCommands();
		IEnumerable<MiniSourceObjectViewModel> source = SourceObjects.Where((MiniSourceObjectViewModel miniSourceObjectViewModel) => miniSourceObjectViewModel.IsSelected && miniSourceObjectViewModel.SelectedEntityClass != null).ToArray();
		IEnumerable<string> enumerable = (from ent in SourceObjects
			where ent.IsSelected && ent.SelectedEntityClass == null
			select ent.Name).ToArray();
		if (enumerable.Count() > 0)
		{
			string messageToUser = string.Format("Some of the objects to import don't have a class assigned to them. \nPlease assign a class and try again:\n\t{0}", string.Join("\n\t", enumerable));
			DialogHelper.DisplayError(messageToUser, "Entity Class not assigned");
			return;
		}
		IEnumerable<string> enumerable2 = (from ent in source
			where ent.EntityAlreadyExists && !ent.OverwriteSelected
			select ent.Name into x
			orderby x
			select x).ToArray();
		if (enumerable2.Any())
		{
			string messageToUser2 = string.Format("Cannot continue with the import.  The following names are taken:\n\t{0}", string.Join("\n\t", enumerable2));
			DialogHelper.DisplayError(messageToUser2, "Name Collision");
			return;
		}
		IEnumerable<IImportedEntity> enumerable3 = source.Select((MiniSourceObjectViewModel ent) => ent.Entity).ToArray();
		IEnumerable<IImportedEntity> enumerable4 = enumerable3.Where((IImportedEntity ent) => !m_exportPredicate(ent)).ToArray();
		if (enumerable4.Any())
		{
			string text = string.Join("\n", enumerable4.Select((IImportedEntity ent) => ent.Name));
			MessageBoxResult messageBoxResult = MessageBox.Show("The following entities cannot be exported because they are read-only (and not open in Perforce).  Continue anyway?\n\n" + text, "Continue with Export?", MessageBoxButton.YesNo);
			if (messageBoxResult != MessageBoxResult.Yes)
			{
				return;
			}
			enumerable3 = enumerable3.Except(enumerable4).ToArray();
		}
		string failureMessage;
		bool flag = PerformExport(CivTechService.PrimaryProject.Name, enumerable3, out failureMessage);
		RefreshCommands();
		if (flag)
		{
			base.ExecuteOKCommand(context);
		}
		else
		{
			DialogHelper.DisplayError(failureMessage);
		}
	}

	protected override void RefreshCommands()
	{
		base.RefreshCommands();
		m_openFileCommand.RaiseCanExecuteChanged();
	}

	private bool CanExecuteOpenFileCommand(object context)
	{
		return AvailableClassNames != null && AvailableClassNames.Any();
	}

	private void ExecuteAssignClassToChildrenCommand(object context)
	{
		foreach (MiniSourceObjectViewModel sourceObject in SourceObjects)
		{
			if (sourceObject.IsSelected && !sourceObject.EntityAlreadyExists)
			{
				sourceObject.SelectedEntityClass = SelectedAssignableClass;
			}
		}
	}

	private void ExecuteOpenFileCommand(object context)
	{
		if (Busy)
		{
			return;
		}
		IEnumerable<IContentExporter> contentExporters = ExporterService.GetContentExporters(NewEntityType);
		List<string> list = new List<string>();
		foreach (IContentExporter item in contentExporters)
		{
			list.AddRange(item.SupportedFileTypes);
		}
		Busy = true;
		RefreshCommands();
		IEnumerable<string> source = DialogHelper.SelectFiles("Source Files", list, multiSelect: true, "Pick the source file to add", LastUsedDirectory);
		foreach (string item2 in source.OrderBy((string x) => x))
		{
			SourceFileModel sourceFileModel = null;
			try
			{
				LastUsedDirectory = Path.GetDirectoryName(item2);
				Uri sourceFilePath = new Uri(item2);
				sourceFileModel = new SourceFileModel(sourceFilePath, NewEntityType);
			}
			catch (Exception ex)
			{
				string messageBoxText = $"There was an error getting file information for file {item2}.  This file will be skipped.\n\nError message: {ex.Message}.";
				MessageBox.Show(messageBoxText, "Error loading file.");
				continue;
			}
			if (!sourceFileModel.SourceObjects.Any() && (sourceFileModel.Extension.Contains("ma") || sourceFileModel.Extension.Contains("mb")))
			{
				MessageBox.Show(string.Concat("There were not object found for export in the file:\n\n", sourceFileModel.SourceFilePath, "\n\nPlease make sure that the objects you want to export are assigned in the model manager in Max/Maya"));
			}
			foreach (SourceObjectModel sourceObject in sourceFileModel.SourceObjects)
			{
				string smartName = sourceObject.SmartName;
				IImportedEntity importedEntity = InstanceSet.FindByNameAndType(smartName, NewEntityType) as IImportedEntity;
				if (importedEntity != null)
				{
					foreach (MiniSourceObjectViewModel sourceObject2 in SourceObjects)
					{
						if (sourceObject2.Entity.Name == importedEntity.Name && sourceObject2.Entity.Type == NewEntityType)
						{
							string name = GenerateUniqueName(InstanceSet, smartName, NewEntityType);
							importedEntity = InstanceSet.CreateEntityByName(name, NewEntityType) as IImportedEntity;
							importedEntity.ClassName = string.Empty;
							importedEntity.SourceFilePath = CivTechRegistry.CivTechService.PrimaryProject.VersionControl.GetDepotPath(item2);
							importedEntity.SourceObjectName = sourceObject.SourceObjectName;
							break;
						}
					}
				}
				if (importedEntity == null)
				{
					importedEntity = InstanceSet.CreateEntityByName(smartName, NewEntityType) as IImportedEntity;
					importedEntity.ClassName = string.Empty;
					importedEntity.SourceFilePath = CivTechRegistry.CivTechService.PrimaryProject.VersionControl.GetDepotPath(item2);
					importedEntity.SourceObjectName = sourceObject.SourceObjectName;
				}
				MiniSourceObjectViewModel miniSourceObjectViewModel = new MiniSourceObjectViewModel(CivTechService, importedEntity, AvailableClassNames, CacheService);
				miniSourceObjectViewModel.IsSelected = !SingleSelectOnly;
				miniSourceObjectViewModel.PropertyChanged += sourceObjectVM_PropertyChanged;
				SourceObjects.Add(miniSourceObjectViewModel);
			}
		}
		SortSourcesCommand.Execute(null);
		Busy = false;
		RefreshCommands();
		OnPropertyChanged("SourcesExist");
	}

	private string GenerateUniqueName(IInstanceSet instanceSet, string baseName, InstanceType type)
	{
		int num = 0;
		string text = baseName;
		while (instanceSet.FindByNameAndType(text, type) != null)
		{
			int num2 = num + 1;
			num = num2;
			text = baseName + num2.ToString("D3");
		}
		return text;
	}

	private void ExecuteOverwriteAllCommand(object context)
	{
		RefreshCommands();
		if (SingleSelectOnly)
		{
			return;
		}
		foreach (MiniSourceObjectViewModel sourceObject in SourceObjects)
		{
			if (sourceObject.OverwriteEnabled)
			{
				sourceObject.OverwriteSelected = (bool)context;
			}
		}
	}

	private void ExecuteSelectAllCommand(object context)
	{
		RefreshCommands();
		if (SingleSelectOnly)
		{
			return;
		}
		foreach (MiniSourceObjectViewModel sourceObject in SourceObjects)
		{
			sourceObject.IsSelected = (bool)context;
		}
	}

	private void ExecuteSortSourcesCommand(object context)
	{
		List<MiniSourceObjectViewModel> list = SourceObjects.ToList();
		list.Sort();
		SourceObjects = new ObservableCollection<MiniSourceObjectViewModel>(list);
	}

	private bool PerformExport(string projectName, IEnumerable<IImportedEntity> entitiesToImport, out string failureMessage)
	{
		failureMessage = string.Empty;
		bool flag = false;
		PopulateEntityDataFiles(entitiesToImport);
		using (new WaitCursor())
		{
			Busy = true;
			IEnumerable<ImportOperationResult> allResults = ImportFunction(CivTechService, projectName, entitiesToImport);
			IEnumerable<ImportOperationResult> validResults = allResults.GetValidResults();
			IEnumerable<ImportOperationResult> failedResults = allResults.GetFailedResults();
			flag = !failedResults.Any();
			foreach (ImportOperationResult item2 in validResults)
			{
				EntityID item = new EntityID(item2.Entity);
				m_importedEntities.Add(item);
			}
			Busy = false;
			if (!flag)
			{
				failureMessage = failedResults.GetCombinedFailureMessages();
			}
		}
		return flag;
	}

	private void PopulateEntityDataFiles(IEnumerable<IInstanceEntity> entities)
	{
		IClassSet classes = CivTechService.PrimaryProject.Config.Classes;
		foreach (IInstanceEntity entity in entities)
		{
			IClassEntity classEntity = classes.FindForInstance(entity);
			if (classEntity != null)
			{
				entity.PopulateDataFiles(classEntity);
			}
		}
	}

	private void sourceObjectVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (!SingleSelectOnly || !(e.PropertyName == "IsSelected"))
		{
			return;
		}
		MiniSourceObjectViewModel miniSourceObjectViewModel = sender as MiniSourceObjectViewModel;
		if (miniSourceObjectViewModel.IsSelected)
		{
			foreach (MiniSourceObjectViewModel sourceObject in SourceObjects)
			{
				if (sourceObject != miniSourceObjectViewModel)
				{
					sourceObject.SelectionEnabled = false;
				}
			}
			return;
		}
		foreach (MiniSourceObjectViewModel sourceObject2 in SourceObjects)
		{
			sourceObject2.SelectionEnabled = true;
		}
	}
}
