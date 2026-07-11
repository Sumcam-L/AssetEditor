using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.MVVMBase;
using Firaxis.Utility;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class ObjectParameterViewModel : BaseParameterViewModel
{
	private DelegateCommand m_clearObjectParameterCommand;

	private DelegateCommand m_newParameterCommand;

	private DelegateCommand m_openSourceFileCommand;

	private DelegateCommand m_reimportEntityCommand;

	private DelegateCommand m_setParameterCommand;

	private string m_currentlySelectedObjectParameter = "";

	private bool m_isImporting = false;

	private Cursor m_previousCursor;

	private bool m_useDefaultParameter;

	public bool ImportableEntitySelected => StaticMethods.IsImportableType(ObjectParam.ObjectType);

	public override bool IsImporting
	{
		get
		{
			return m_isImporting;
		}
		protected set
		{
			m_isImporting = value;
		}
	}

	public string ObjectValue
	{
		get
		{
			return ObjectValueInternal.GetBoundObjectName();
		}
		set
		{
			InstanceType boundObjectType = ObjectValueInternal.GetBoundObjectType();
			if (!string.IsNullOrEmpty(value))
			{
				IInstanceEntity instanceByNameAndType = EngineContextWrapper.GetInstanceByNameAndType(boundObjectType, value, InstanceSet);
				if (instanceByNameAndType != null)
				{
					if (ObjectParam.AllowedClasses.Contains(instanceByNameAndType.ClassName))
					{
						ObjectValueInternal.BindObject(instanceByNameAndType.Name, boundObjectType);
					}
					else
					{
						DialogHelper.DisplayError("This entity: " + instanceByNameAndType.Name + " has class: " + instanceByNameAndType.ClassName + " which is not allowed here");
					}
				}
				else
				{
					ObjectValueInternal.BindObject(string.Empty, boundObjectType);
				}
			}
			else
			{
				ObjectValueInternal.BindObject(string.Empty, boundObjectType);
			}
			OnObjectParameterChanged(value);
			OnPropertyChanged("ObjectValue");
			OnPropertyChanged("Value");
			OnPropertyChanged("ImportableEntitySelected");
			OnParameterValueChangedEvent(base.Name, base.Value);
		}
	}

	public bool UseDefaultParameter
	{
		get
		{
			return m_useDefaultParameter;
		}
		set
		{
			if (m_useDefaultParameter != value)
			{
				m_useDefaultParameter = value;
				if (value)
				{
					m_currentlySelectedObjectParameter = ObjectValue;
					ObjectValue = DefaultValue.GetBoundObjectName();
				}
				else
				{
					ObjectValue = m_currentlySelectedObjectParameter;
				}
				OnPropertyChanged("UseDefaultParameter");
			}
		}
	}

	protected ICivTechService CivTechService { get; private set; }

	protected IInstanceSet InstanceSet { get; set; }

	private IObjectValue DefaultValue => ObjectParam.DefaultValue as IObjectValue;

	private IObjectParameter ObjectParam => (IObjectParameter)base.Parameter;

	private IObjectValue ObjectValueInternal => (IObjectValue)base.Value;

	public ICommand ClearObjectParameterCommand
	{
		get
		{
			if (m_clearObjectParameterCommand == null)
			{
				m_clearObjectParameterCommand = new DelegateCommand(ClearObjectParameter);
			}
			return m_clearObjectParameterCommand;
		}
	}

	public ICommand NewParameterCommand
	{
		get
		{
			if (m_newParameterCommand == null)
			{
				m_newParameterCommand = new DelegateCommand(NewObjectParameter);
			}
			return m_newParameterCommand;
		}
	}

	public ICommand OpenSourceFileCommand
	{
		get
		{
			if (m_openSourceFileCommand == null)
			{
				m_openSourceFileCommand = new DelegateCommand(OpenSourceForSelectedEntity);
			}
			return m_openSourceFileCommand;
		}
	}

	public ICommand ReimportEntityCommand
	{
		get
		{
			if (m_reimportEntityCommand == null)
			{
				m_reimportEntityCommand = new DelegateCommand(ReimportSelectedEntity);
			}
			return m_reimportEntityCommand;
		}
	}

	public ICommand SetParameterCommand
	{
		get
		{
			if (m_setParameterCommand == null)
			{
				m_setParameterCommand = new DelegateCommand(SetObjectParameter);
			}
			return m_setParameterCommand;
		}
	}

	public event EventHandler<EntityReimportedEventArgs> EntityReimportedEvent;

	public ObjectParameterViewModel(ICivTechService civTechSvc, IObjectParameter param, IObjectValue value, IInstanceSet instances)
		: base(param, value)
	{
		CivTechService = civTechSvc;
		InstanceSet = instances;
	}

	public ImportOperationResult ReimportParameterObject()
	{
		if (GetBoundEntity() is IImportedEntity entity)
		{
			return global::DatabaseWrapper.DatabaseWrapper.ImportEntity(CivTechRegistry.CivTechService, CivTechRegistry.CivTechService.PrimaryProject.Name, entity);
		}
		return null;
	}

	protected virtual void ImportCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		Mouse.OverrideCursor = m_previousCursor;
		ImportOperationResult importOperationResult = e.Result as ImportOperationResult;
		if (importOperationResult != null && (bool)importOperationResult.Result)
		{
			OnEntityReimportedEvent(importOperationResult.Entity.Name, importOperationResult.Entity.Type);
		}
		else
		{
			DialogHelper.DisplayError((importOperationResult != null) ? importOperationResult.Result.Message : "No export occurred.");
		}
		IsImporting = false;
	}

	protected virtual void OnEntityReimportedEvent(string entityName, InstanceType entityType)
	{
		this.EntityReimportedEvent?.Invoke(this, new EntityReimportedEventArgs(entityName, entityType));
	}

	protected virtual void OnObjectParameterChanged(string newObjectParameter)
	{
	}

	protected virtual void StartImport(object sender, DoWorkEventArgs e)
	{
		if (GetBoundEntity() is IImportedEntity entity)
		{
			e.Result = global::DatabaseWrapper.DatabaseWrapper.ImportEntity(CivTechRegistry.CivTechService, CivTechRegistry.CivTechService.PrimaryProject.Name, entity);
		}
	}

	private void ClearObjectParameter(object context)
	{
		UseDefaultParameter = false;
		ObjectValue = string.Empty;
	}

	private IInstanceEntity GetBoundEntity()
	{
		return EngineContextWrapper.GetInstanceByNameAndType(ObjectValueInternal.GetBoundObjectType(), ObjectValueInternal.GetBoundObjectName(), InstanceSet);
	}

	private void ImportNewEntity(string sourceObjectName, InstanceType newEntityType, string className, string sourceFilePath)
	{
		string selectedName = global::DatabaseWrapper.DatabaseWrapper.GetImportName(sourceFilePath, sourceObjectName);
		if (!PromptUserForNewName(sourceObjectName, selectedName, className, newEntityType, out selectedName))
		{
			return;
		}
		using (new WaitCursor())
		{
			ImportOperationResult importOperationResult = global::DatabaseWrapper.DatabaseWrapper.FastImportNewEntity(CivTechRegistry.CivTechService, CivTechRegistry.CivTechService.PrimaryProject.Name, sourceObjectName, newEntityType, className, sourceFilePath, InstanceSet, selectedName);
			if ((bool)importOperationResult.Result)
			{
				ObjectValue = selectedName;
			}
			else
			{
				DialogHelper.DisplayError(importOperationResult.Result.Message);
			}
		}
	}

	private void NewObjectParameter(object context)
	{
		UseDefaultParameter = false;
		InstanceType objectType = ObjectParam.ObjectType;
		IEnumerable<string> enumerable = DialogHelper.SelectSourceFiles(CivTechService, multiSelect: true, objectType);
		if (!enumerable.Any())
		{
			return;
		}
		List<SourceFileModel> list = new List<SourceFileModel>();
		foreach (string item in enumerable)
		{
			list.Add(new SourceFileModel(item, objectType));
		}
		string text = null;
		string text2 = null;
		if (list.Count == 1 && list[0].SourceObjects.Count() == 1 && ObjectParam.AllowedClasses.Count() == 1)
		{
			text = list[0].SourceObjects.First().SourceObjectName;
			text2 = ObjectParam.AllowedClasses.First();
			ImportNewEntity(text, objectType, text2, list[0].SourceFilePath.LocalPath);
		}
		else
		{
			DialogHelper.LaunchSourceClassAssociationView(CivTechService, list, ObjectParam, ObjectValueInternal, objectType, global::DatabaseWrapper.DatabaseWrapper.ImportEntities, (IImportedEntity ent) => true);
			ObjectValue = ObjectValueInternal.GetBoundObjectName();
		}
	}

	private void OpenSourceForSelectedEntity(object context)
	{
		if (GetBoundEntity() is IImportedEntity importedEntity && !string.IsNullOrEmpty(importedEntity.SourceFilePath))
		{
			ExporterService.GetContentCreationTool(importedEntity)?.OpenFile(CivTechService.PrimaryProject.VersionControl.GetLocalPath(importedEntity.SourceFilePath));
		}
	}

	private bool PromptUserForNewName(string sourceObjectName, string importName, string className, InstanceType newEntityType, out string selectedName)
	{
		bool flag = false;
		selectedName = importName;
		while (!flag && global::DatabaseWrapper.DatabaseWrapper.DoesEntityExist(newEntityType, selectedName))
		{
			IInstanceEntity instanceByNameAndType = EngineContextWrapper.GetInstanceByNameAndType(newEntityType, selectedName, InstanceSet);
			if (instanceByNameAndType == null)
			{
				Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Error, "ParameterSetView", $"ImportNewEntity failed. Error loading entity of type {newEntityType} with name {selectedName}.");
				return false;
			}
			bool flag2 = instanceByNameAndType.ClassName.Equals(className);
			string dialogTitle = sourceObjectName + " Already Exists in the Cloud";
			string text = "Please choose a new name";
			text = ((!flag2) ? (text + ", " + sourceObjectName + " can't be overwritten because it has an incompatible class") : (text + " or press cancel to overwrite"));
			string text2 = DialogHelper.PromptForString(text, dialogTitle);
			if (string.IsNullOrEmpty(text2))
			{
				flag = true;
				if (!flag2)
				{
					return false;
				}
			}
			else
			{
				selectedName = text2;
			}
		}
		return true;
	}

	private void ReimportSelectedEntity(object context)
	{
		if (!string.IsNullOrWhiteSpace(ObjectValueInternal.GetBoundObjectName()))
		{
			IsImporting = true;
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			m_previousCursor = Mouse.OverrideCursor;
			Mouse.OverrideCursor = Cursors.Wait;
			backgroundWorker.DoWork += StartImport;
			backgroundWorker.RunWorkerCompleted += ImportCompleted;
			backgroundWorker.RunWorkerAsync(null);
		}
	}

	private void SetObjectParameter(object context)
	{
		UseDefaultParameter = false;
		IInstanceEntity instanceEntity = DialogHelper.PickAndLoadAssetObject(context as Window, CivTechService, ObjectParam.ObjectType, ObjectParam.AllowedClasses, InstanceSet);
		if (instanceEntity != null)
		{
			ObjectValue = instanceEntity.Name;
		}
	}
}
