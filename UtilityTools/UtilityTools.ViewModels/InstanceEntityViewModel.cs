using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.MVVMBase;
using Firaxis.Utility;
using UtilityTools.Helpers;
using UtilityTools.ViewModels.ParameterViewModels;

namespace UtilityTools.ViewModels;

public class InstanceEntityViewModel : IInstanceEntityViewModel, INotifyPropertyChanged, IDisposable
{
	private DelegateCommand m_addTagCommand;

	private DelegateCommand m_cancelCommand;

	private DelegateCommand m_newFromFileCommand;

	private DelegateCommand m_reimportCommand;

	private DelegateCommand m_saveCommand;

	private DelegateCommand m_saveAsCommand;

	private IEnumerable<string> m_availableClasses;

	private IClassEntity m_class;

	private IInstanceEntity m_entity;

	private bool m_newInstanceEntity;

	private IParameterSetViewModel m_parametersViewModel;

	private Cursor m_previousCursor;

	private string m_tagToAdd;

	private bool m_userSavedEntity;

	public IEnumerable<string> AvailableClasses
	{
		get
		{
			return m_availableClasses;
		}
		private set
		{
			if (m_availableClasses != value)
			{
				m_availableClasses = value;
				RaisePropertyChanged("AvailableClasses");
			}
		}
	}

	public string CachedEntityName { get; private set; }

	public string ClassName
	{
		get
		{
			return InstanceEntity.ClassName;
		}
		set
		{
			if (!(InstanceEntity.ClassName != value))
			{
				return;
			}
			IClassEntity classEntity = global::DatabaseWrapper.DatabaseWrapper.GetClass(CivTechRegistry.CivTechService.PrimaryProject.Name, InstanceEntity.Type, value);
			if (classEntity != null)
			{
				InstanceEntity.ClassName = value;
				StaticMethods.RepopulateInstanceCookParameters(InstanceEntity, classEntity);
				SetupMaterialViewModels(classEntity);
				EntityClassInstance = classEntity;
				if (m_newFromFileCommand != null)
				{
					m_newFromFileCommand.RaiseCanExecuteChanged();
				}
			}
		}
	}

	public string Description
	{
		get
		{
			return InstanceEntity.Description;
		}
		set
		{
			if (InstanceEntity.Description != value)
			{
				InstanceEntity.Description = value;
				RaisePropertyChanged("Description");
			}
		}
	}

	public IClassEntity EntityClassInstance
	{
		get
		{
			return m_class;
		}
		set
		{
			if (m_class != value)
			{
				m_class = value;
				RaisePropertyChanged("EntityClassInstance");
			}
		}
	}

	public bool HasClass => !string.IsNullOrWhiteSpace(InstanceEntity.ClassName);

	public IInstanceEntity InstanceEntity
	{
		get
		{
			return m_entity;
		}
		private set
		{
			if (m_entity != value)
			{
				m_entity = value;
			}
		}
	}

	public IInstanceSet InstanceSet { get; set; }

	public bool IsImporting { get; private set; }

	public bool IsMaterial => InstanceEntity.Type == InstanceType.IT_MATERIAL;

	public string Name
	{
		get
		{
			return InstanceEntity.Name;
		}
		set
		{
			if (InstanceEntity.Name != value)
			{
				InstanceEntity.Name = value.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				SetSourceEntity();
				if (SourceInstanceEntity != null && string.Equals(InstanceEntity.Name, SourceInstanceEntity.Name, StringComparison.CurrentCultureIgnoreCase))
				{
					InstanceEntity.Name = SourceInstanceEntity.Name;
				}
				RaisePropertyChanged("Name");
			}
		}
	}

	public bool NewInstanceEntity
	{
		get
		{
			return m_newInstanceEntity;
		}
		private set
		{
			if (m_newInstanceEntity != value)
			{
				m_newInstanceEntity = value;
				RaisePropertyChanged("NewInstanceEntity");
			}
		}
	}

	public IParameterSetViewModel ParametersViewModel
	{
		get
		{
			return m_parametersViewModel;
		}
		set
		{
			m_parametersViewModel = value;
			RaisePropertyChanged("ParametersViewModel");
		}
	}

	public bool ReimportVisible
	{
		get
		{
			bool flag = false;
			switch (InstanceEntity.Type)
			{
			case InstanceType.IT_MATERIAL:
			case InstanceType.IT_GEOMETRY:
			case InstanceType.IT_TEXTURE:
			case InstanceType.IT_ANIMATION:
				return !NewInstanceEntity;
			default:
				return false;
			}
		}
	}

	public IEnumerable<string> Tags => InstanceEntity.Tags;

	public string TagsAsString => InstanceEntity.FlattenTagsToString();

	public string TagToAdd
	{
		get
		{
			return m_tagToAdd;
		}
		set
		{
			if (m_tagToAdd != value)
			{
				m_tagToAdd = value;
				RaisePropertyChanged("TagToAdd");
			}
		}
	}

	public bool UserSavedEntity
	{
		get
		{
			return m_userSavedEntity;
		}
		private set
		{
			if (m_userSavedEntity != value)
			{
				m_userSavedEntity = value;
			}
		}
	}

	protected ICivTechService CivTechService { get; private set; }

	private IInstanceEntity SourceInstanceEntity { get; set; }

	private IInstanceSet SourceInstanceSet { get; set; }

	public ICommand AddTagCommand
	{
		get
		{
			if (m_addTagCommand == null)
			{
				m_addTagCommand = new DelegateCommand(AddTag);
			}
			return m_addTagCommand;
		}
	}

	public ICommand CancelCommand
	{
		get
		{
			if (m_cancelCommand == null)
			{
				m_cancelCommand = new DelegateCommand(Cancel);
			}
			return m_cancelCommand;
		}
	}

	public ICommand NewFromFileCommand
	{
		get
		{
			if (m_newFromFileCommand == null)
			{
				m_newFromFileCommand = new DelegateCommand(ExecuteNewFromFileCommand, CanExecuteNewFromFileCommand);
			}
			return m_newFromFileCommand;
		}
	}

	public ICommand ReimportCommand
	{
		get
		{
			if (m_reimportCommand == null)
			{
				m_reimportCommand = new DelegateCommand(ReimportEntity);
			}
			return m_reimportCommand;
		}
	}

	public ICommand SaveAsCommand
	{
		get
		{
			if (m_saveAsCommand == null)
			{
				m_saveAsCommand = new DelegateCommand(SaveAsEntity);
			}
			return m_saveAsCommand;
		}
	}

	public ICommand SaveCommand
	{
		get
		{
			if (m_saveCommand == null)
			{
				m_saveCommand = new DelegateCommand(SaveEntity);
			}
			return m_saveCommand;
		}
	}

	public virtual event PropertyChangedEventHandler PropertyChanged;

	public event EventHandler CloseEvent;

	public event EventHandler<ParameterChangedEventArgs> ParameterChangedEvent;

	public event EventHandler<EntityReimportedEventArgs> EntityReimportedEvent;

	public InstanceEntityViewModel(ICivTechService civTechSvc, IInstanceEntity instanceEntity, IEnumerable<string> availableClassList, IInstanceSet instances)
	{
		CivTechService = civTechSvc;
		InstanceSet = instances;
		AvailableClasses = new ObservableCollection<string>(availableClassList);
		SourceInstanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>();
		InstanceEntity = instanceEntity;
		if (!string.IsNullOrWhiteSpace(instanceEntity.ClassName))
		{
			ClassName = instanceEntity.ClassName;
		}
		NewInstanceEntity = !global::DatabaseWrapper.DatabaseWrapper.DoesEntityExist(instanceEntity);
		SetSourceEntity();
		CachedEntityName = instanceEntity.Name;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public static InstanceEntityViewModel Open(ICivTechService civTechSvc, IInstanceEntity inst, IInstanceSet instances)
	{
		IEnumerable<string> classNames = global::DatabaseWrapper.DatabaseWrapper.GetClassNames(civTechSvc.PrimaryProject.Name, inst.Type);
		InstanceEntityViewModel instanceEntityViewModel = new InstanceEntityViewModel(civTechSvc, inst, classNames, instances);
		instanceEntityViewModel.SetupMaterialViewModels(global::DatabaseWrapper.DatabaseWrapper.GetClass(civTechSvc.PrimaryProject.Name, inst.Type, inst.ClassName));
		return instanceEntityViewModel;
	}

	public void ResetInstance()
	{
		InstanceSet.Remove(InstanceEntity);
	}

	public void RevertTemporaryHiding()
	{
		ParametersViewModel.RevertTemporaryHiding();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (ParametersViewModel is IDisposable disposable)
			{
				disposable.Dispose();
				ParametersViewModel.ParameterChangedEvent -= HandleParameterChangedEvent;
				ParametersViewModel.EntityReimportedEvent -= HandleEntityReimportedEvent;
				IDisposable disposable2 = null;
			}
			if (SourceInstanceSet != null)
			{
				SourceInstanceSet.Dispose();
				SourceInstanceSet = null;
			}
		}
	}

	protected virtual void HandleEntityReimportedEvent(object sender, EntityReimportedEventArgs e)
	{
		OnEntityReimportedEvent(e);
	}

	protected virtual void HandleParameterChangedEvent(object sender, ParameterChangedEventArgs e)
	{
		OnParameterChangedEvent(e);
	}

	protected virtual void ImportCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		Mouse.OverrideCursor = m_previousCursor;
		ImportOperationResult importOperationResult = e.Result as ImportOperationResult;
		IEnumerable<ImportOperationResult> enumerable = e.Result as IEnumerable<ImportOperationResult>;
		if (importOperationResult != null)
		{
			if ((bool)importOperationResult.Result)
			{
				OnEntityReimportedEvent(importOperationResult.Entity.Name, importOperationResult.Entity.Type);
			}
			else
			{
				DialogHelper.DisplayError(importOperationResult.Result.Message);
			}
		}
		else if (enumerable != null)
		{
			IEnumerable<ImportOperationResult> validResults = enumerable.GetValidResults();
			IEnumerable<ImportOperationResult> failedResults = enumerable.GetFailedResults();
			foreach (ImportOperationResult item in validResults)
			{
				OnEntityReimportedEvent(item.Entity.Name, item.Entity.Type);
			}
			if (failedResults.Any())
			{
				string messageToUser = "The following entities were not exported.\n\n" + failedResults.GetCombinedFailureMessages();
				DialogHelper.DisplayError(messageToUser);
			}
		}
		else
		{
			DialogHelper.DisplayError("No export occurred.");
		}
		IsImporting = false;
	}

	protected virtual void OnCloseEvent()
	{
		this.CloseEvent?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnEntityReimportedEvent(string entityName, InstanceType entityType)
	{
		this.EntityReimportedEvent?.Invoke(this, new EntityReimportedEventArgs(entityName, entityType));
	}

	protected virtual void OnEntityReimportedEvent(EntityReimportedEventArgs args)
	{
		this.EntityReimportedEvent?.Invoke(this, args);
	}

	protected virtual void OnParameterChangedEvent(string entityName, string parameterName, IValue value)
	{
		this.ParameterChangedEvent?.Invoke(this, new ParameterChangedEventArgs(entityName, parameterName, value));
	}

	protected virtual void OnParameterChangedEvent(ParameterChangedEventArgs args)
	{
		this.ParameterChangedEvent?.Invoke(this, args);
	}

	protected virtual void StartImport(object sender, DoWorkEventArgs e)
	{
		IInstanceEntity instanceEntity = ((InstanceEntityViewModel)e.Argument).InstanceEntity;
		if (instanceEntity != null)
		{
			if (instanceEntity is IMaterialInstance)
			{
				e.Result = global::DatabaseWrapper.DatabaseWrapper.ImportEntities(CivTechRegistry.CivTechService, CivTechRegistry.CivTechService.PrimaryProject.Name, global::DatabaseWrapper.DatabaseWrapper.GetImportableEntities(CivTechRegistry.CivTechService, instanceEntity, InstanceSet, recursive: false));
			}
			else
			{
				e.Result = global::DatabaseWrapper.DatabaseWrapper.ImportEntity(CivTechRegistry.CivTechService, CivTechRegistry.CivTechService.PrimaryProject.Name, instanceEntity as IImportedEntity);
			}
		}
	}

	private void AddTag(object context)
	{
		if (!m_entity.Tags.Contains(TagToAdd))
		{
			m_entity.AddTag(TagToAdd);
			RaisePropertyChanged("Tags");
			RaisePropertyChanged("TagsAsString");
			TagToAdd = string.Empty;
		}
	}

	private void Cancel(object context)
	{
		InstanceSet.Remove(InstanceEntity);
		UserSavedEntity = false;
		OnCloseEvent();
	}

	private bool CanExecuteNewFromFileCommand(object context)
	{
		if (!IsMaterial || ParametersViewModel == null)
		{
			return false;
		}
		bool result = true;
		List<InstanceType> list = new List<InstanceType>();
		foreach (IParameterViewModel parameter in ParametersViewModel.ParameterList)
		{
			if (parameter.Parameter is IObjectParameter objectParameter)
			{
				if (list.Count == 0)
				{
					list.Add(objectParameter.ObjectType);
				}
				else if (!list.Contains(objectParameter.ObjectType))
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private void ExecuteNewFromFileCommand(object context)
	{
		IParameterViewModel parameterViewModel = ParametersViewModel.ParameterList.First((IParameterViewModel vm) => vm.Parameter is IObjectParameter);
		InstanceType objectType = (parameterViewModel.Parameter as IObjectParameter).ObjectType;
		List<string> list = DialogHelper.SelectSourceFiles(CivTechService, multiSelect: true, objectType).ToList();
		List<SourceFileModel> list2 = new List<SourceFileModel>();
		foreach (string item in list)
		{
			list2.Add(new SourceFileModel(item, objectType));
		}
		if (list.Count == 0)
		{
			return;
		}
		IParameterSet parameters = ParametersViewModel.Parameters;
		IValueSet values = ParametersViewModel.Values;
		DialogHelper.LaunchSourceClassAssociationView(CivTechService, list2, parameters, values, objectType, global::DatabaseWrapper.DatabaseWrapper.ImportEntities, (IImportedEntity ent) => true);
		foreach (IObjectValue item2 in values.Items.OfType<IObjectValue>())
		{
			foreach (IParameterViewModel parameter in ParametersViewModel.ParameterList)
			{
				if (parameter.Parameter.Name == item2.ParameterName && parameter is ObjectParameterViewModel objectParameterViewModel)
				{
					objectParameterViewModel.ObjectValue = item2.GetBoundObjectName();
				}
			}
		}
	}

	private void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void ReimportEntity(object context)
	{
		if (context != null)
		{
			IsImporting = true;
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			m_previousCursor = Mouse.OverrideCursor;
			Mouse.OverrideCursor = Cursors.Wait;
			backgroundWorker.DoWork += StartImport;
			backgroundWorker.RunWorkerCompleted += ImportCompleted;
			backgroundWorker.RunWorkerAsync(context);
		}
	}

	private void SaveAsEntity(object context)
	{
		bool flag = false;
		flag = SaveAsEntity();
		if (flag)
		{
			UserSavedEntity = flag;
			OnCloseEvent();
		}
	}

	private bool SaveAsEntity()
	{
		string text = DialogHelper.PromptForString("Choose a new name");
		if (text != null)
		{
			string name = Name;
			Name = text;
			if (global::DatabaseWrapper.DatabaseWrapper.DoesEntityExist(InstanceEntity) && !DialogHelper.PromptForBool("There is already an entity named: " + InstanceEntity.Name + ". Would you like to overwrite?", "Entity already exists"))
			{
				Name = name;
				return false;
			}
			if (!SaveEntity())
			{
				Name = name;
				return false;
			}
			return true;
		}
		return false;
	}

	private void SaveEntity(object context)
	{
		bool flag = false;
		flag = ((!NewInstanceEntity) ? SaveEntity() : SaveAsEntity());
		if (flag)
		{
			UserSavedEntity = flag;
			OnCloseEvent();
		}
	}

	private bool SaveEntity()
	{
		RevertTemporaryHiding();
		CachedEntityName = InstanceEntity.Name;
		if (InstanceEntity.Type == InstanceType.IT_MATERIAL && !ValidateMaterial())
		{
			DialogHelper.DisplayError($"Could not save the material: {InstanceEntity.Name}.\n\tIts class requires all textures to be of a uniform size.");
			return false;
		}
		bool result = false;
		if (ClassName != "")
		{
			if (global::DatabaseWrapper.DatabaseWrapper.UploadEntity(CivTechRegistry.CivTechService, InstanceEntity, out var errMsg))
			{
				NewInstanceEntity = false;
				result = true;
			}
			else
			{
				DialogHelper.DisplayError($"Could not save the entity.\n{errMsg}");
			}
		}
		else
		{
			DialogHelper.DisplayError("Please assign a Class to your entity before saving.");
		}
		return result;
	}

	private void SetSourceEntity()
	{
		SourceInstanceSet.Clear();
		SourceInstanceEntity = SourceInstanceSet.LoadEntityByName(InstanceEntity.Name, InstanceEntity.Type);
	}

	private void SetupMaterialViewModels(IClassEntity classInstance)
	{
		ParametersViewModel = new ParameterSetViewModel(CivTechService, InstanceEntity, classInstance.CookParameters, InstanceEntity.CookParameters, InstanceSet);
		ParametersViewModel.ParameterChangedEvent += HandleParameterChangedEvent;
		ParametersViewModel.EntityReimportedEvent += HandleEntityReimportedEvent;
		RaisePropertyChanged("ClassName");
		RaisePropertyChanged("HasClass");
	}

	private bool ValidateMaterial()
	{
		IMaterialClass materialClass = global::DatabaseWrapper.DatabaseWrapper.GetMaterialClass(CivTechRegistry.CivTechService.PrimaryProject.Name, InstanceEntity.ClassName);
		if (materialClass == null)
		{
			return false;
		}
		if (!materialClass.ValidationOptions.RequireUniformTextureSize)
		{
			return true;
		}
		List<ITextureInstance> list = new List<ITextureInstance>();
		IEnumerable<IImportedEntity> importableEntities = global::DatabaseWrapper.DatabaseWrapper.GetImportableEntities(CivTechRegistry.CivTechService, InstanceEntity, InstanceSet, recursive: true);
		foreach (IImportedEntity item2 in importableEntities)
		{
			if (item2 is ITextureInstance item)
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			return true;
		}
		uint height = list[0].Height;
		uint width = list[0].Width;
		bool result = true;
		foreach (ITextureInstance item3 in list)
		{
			if (item3.Height != height || item3.Width != width)
			{
				result = false;
			}
		}
		return result;
	}
}
