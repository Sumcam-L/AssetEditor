using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.MVVMBase;
using UtilityTools.Helpers;
using UtilityTools.ViewModels.ParameterViewModels;

namespace UtilityTools.ViewModels;

public class ParameterSetViewModel : Notifier, IParameterSetViewModel, IDisposable
{
	private ObservableCollection<IParameterViewModel> m_parameterList;

	private IParameterSet m_parameters;

	private IInstanceEntity m_entity;

	private ObservableCollection<string> m_materialNames;

	private Cursor m_previousCursor;

	private IValueSet m_values;

	public IInstanceEntity Entity
	{
		get
		{
			return m_entity;
		}
		set
		{
			if (m_entity != value)
			{
				m_entity = value;
				OnPropertyChanged("Entity");
			}
		}
	}

	public ObservableCollection<string> MaterialNames
	{
		get
		{
			return m_materialNames;
		}
		set
		{
			if (m_materialNames != value)
			{
				m_materialNames = value;
				OnPropertyChanged("MaterialNames");
			}
		}
	}

	protected ICivTechService CivTechService { get; private set; }

	private IInstanceSet InstanceSet { get; set; }

	private bool ParameterSetImporting { get; set; }

	public IParameterSet Parameters
	{
		get
		{
			return m_parameters;
		}
		set
		{
			m_parameters = value;
		}
	}

	public IValueSet Values
	{
		get
		{
			return m_values;
		}
		set
		{
			m_values = value;
		}
	}

	public IEnumerable<IParameterViewModel> ParameterList => m_parameterList;

	public bool IsImporting
	{
		get
		{
			if (ParameterSetImporting)
			{
				return true;
			}
			bool result = false;
			foreach (IParameterViewModel parameter in m_parameterList)
			{
				if (parameter.IsImporting)
				{
					result = true;
				}
			}
			return result;
		}
	}

	public event EventHandler<EntityReimportedEventArgs> EntityReimportedEvent;

	public event EventHandler<ParameterChangedEventArgs> ParameterChangedEvent;

	public ParameterSetViewModel(ICivTechService civTechSvc, IInstanceEntity entity, IParameterSet parameters, IValueSet values, IInstanceSet instanceSet)
		: this(civTechSvc, entity, parameters, values, instanceSet, null)
	{
	}

	public ParameterSetViewModel(ICivTechService civTechSvc, IInstanceEntity entity, IParameterSet parameters, IValueSet values, IInstanceSet instanceSet, IEnumerable<string> materialList)
	{
		CivTechService = civTechSvc;
		ParameterSetImporting = false;
		m_parameters = parameters;
		m_values = values;
		InstanceSet = instanceSet;
		Entity = entity;
		if (materialList != null)
		{
			MaterialNames = new ObservableCollection<string>(materialList);
		}
		else
		{
			MaterialNames = new ObservableCollection<string>();
		}
		PopulateParameterListFromSet();
	}

	public void Dispose()
	{
		if (ParameterList == null)
		{
			return;
		}
		foreach (IParameterViewModel parameter in ParameterList)
		{
			if (!(parameter is BaseParameterViewModel baseParameterViewModel))
			{
				continue;
			}
			baseParameterViewModel.ParameterValueChangedEvent -= HandleParameterValueChangedEvent;
			if (parameter is ObjectParameterViewModel objectParameterViewModel)
			{
				objectParameterViewModel.EntityReimportedEvent -= HandleEntityReimportedEvent;
			}
			if (!(parameter is CollectionParameterViewModel collectionParameterViewModel))
			{
				continue;
			}
			foreach (BaseParameterViewModel item in collectionParameterViewModel.Items)
			{
				item.ParameterValueChangedEvent -= HandleParameterValueChangedEvent;
				if (item is ObjectParameterViewModel)
				{
					((ObjectParameterViewModel)item).EntityReimportedEvent -= HandleEntityReimportedEvent;
				}
			}
		}
		m_parameterList.Clear();
		m_parameterList = null;
	}

	public void RevertTemporaryHiding()
	{
		foreach (IParameterViewModel parameter in ParameterList)
		{
			if (parameter is ObjectParameterViewModel objectParameterViewModel)
			{
				objectParameterViewModel.UseDefaultParameter = false;
			}
		}
	}

	protected virtual void HandleEntityReimportedEvent(object sender, EntityReimportedEventArgs e)
	{
		OnEntityReimportedEvent(e);
	}

	protected virtual void HandleParameterValueChangedEvent(object sender, ParameterValueChangedEventArgs e)
	{
		OnParameterChangedEvent(Entity.Name, e.ParameterName, e.NewValue);
	}

	protected virtual void ImportCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		Mouse.OverrideCursor = m_previousCursor;
		if (e.Result is IEnumerable<ImportOperationResult> allResults)
		{
			IEnumerable<ImportOperationResult> validResults = allResults.GetValidResults();
			IEnumerable<ImportOperationResult> failedResults = allResults.GetFailedResults();
			foreach (ImportOperationResult item in validResults)
			{
				OnEntityReimportedEvent(item.Entity.Name, item.Entity.Type);
			}
			if (failedResults.Any())
			{
				DialogHelper.DisplayError(failedResults.GetCombinedFailureMessages());
			}
		}
		ParameterSetImporting = false;
	}

	protected virtual void OnEntityReimportedEvent(string entityName, InstanceType entityType)
	{
		this.EntityReimportedEvent?.Invoke(this, new EntityReimportedEventArgs(entityName, entityType));
	}

	protected virtual void OnEntityReimportedEvent(EntityReimportedEventArgs args)
	{
		this.EntityReimportedEvent?.Invoke(this, args);
	}

	protected virtual void OnParameterChangedEvent(string entityName, string paramName, IValue newValue)
	{
		this.ParameterChangedEvent?.Invoke(this, new ParameterChangedEventArgs(entityName, paramName, newValue));
	}

	protected virtual void StartImport(object sender, DoWorkEventArgs e)
	{
		IList<ImportOperationResult> list = new List<ImportOperationResult>();
		foreach (IParameterViewModel parameter in m_parameterList)
		{
			if (parameter is ObjectParameterViewModel objectParameterViewModel)
			{
				list.Add(objectParameterViewModel.ReimportParameterObject());
			}
		}
		e.Result = list;
	}

	private BaseParameterViewModel GetCollectionParameterViewModel(ICollectionParameter collectionParam, ICollectionValue collectionValue, IInstanceSet instanceSet, IEnumerable<string> materialNames)
	{
		List<BaseParameterViewModel> list = new List<BaseParameterViewModel>();
		IParameter entryParameter = collectionParam.EntryParameter;
		foreach (IValue item in collectionValue.Items)
		{
			list.Add(GetParameterViewModel(entryParameter, item, instanceSet, materialNames));
		}
		return new CollectionParameterViewModel(collectionParam, collectionValue, list);
	}

	private BaseParameterViewModel GetParameterViewModel(IParameter param, IValue val, IInstanceSet instanceSet, IEnumerable<string> materialNames)
	{
		BaseParameterViewModel baseParameterViewModel = null;
		switch (param.ParameterType)
		{
		case ParameterType.PT_BOOLEAN:
			baseParameterViewModel = new BoolParameterViewModel(param as IBoolParameter, val as IBoolValue);
			break;
		case ParameterType.PT_INT:
			baseParameterViewModel = new IntParameterViewModel(param as IIntParameter, val as IIntValue);
			break;
		case ParameterType.PT_FLOAT:
			baseParameterViewModel = new FloatParameterViewModel(param as IFloatParameter, val as IFloatValue);
			break;
		case ParameterType.PT_STRING:
			baseParameterViewModel = new StringParameterViewModel(param as IStringParameter, val as IStringValue);
			break;
		case ParameterType.PT_ENUM:
			baseParameterViewModel = new EnumParameterViewModel(param as IEnumParameter, val as IStringValue);
			break;
		case ParameterType.PT_RGB:
			baseParameterViewModel = new RGBParameterViewModel(param as IRGBParameter, val as IRGBValue);
			break;
		case ParameterType.PT_OBJECT:
		{
			IObjectParameter objectParameter = param as IObjectParameter;
			baseParameterViewModel = ((objectParameter.ObjectType != InstanceType.IT_MATERIAL) ? new ObjectParameterViewModel(CivTechService, objectParameter, val as IObjectValue, instanceSet) : new MaterialObjectParameterViewModel(CivTechService, objectParameter, val as IObjectValue, instanceSet, materialNames));
			(baseParameterViewModel as ObjectParameterViewModel).EntityReimportedEvent += HandleEntityReimportedEvent;
			break;
		}
		case ParameterType.PT_COLLECTION:
			baseParameterViewModel = GetCollectionParameterViewModel((ICollectionParameter)param, (ICollectionValue)val, instanceSet, materialNames);
			break;
		default:
			baseParameterViewModel = new BaseParameterViewModel(param, val);
			break;
		}
		baseParameterViewModel.ParameterValueChangedEvent += HandleParameterValueChangedEvent;
		return baseParameterViewModel;
	}

	private void PopulateParameterListFromSet()
	{
		m_parameterList = new ObservableCollection<IParameterViewModel>();
		Values.AddDefaultValuesAsNecessary(Parameters);
		Values.RemoveUnusedValues(Parameters);
		foreach (IParameter parameter in Parameters.GetParameters())
		{
			IValue val = Values.FindValue(parameter.Name);
			BaseParameterViewModel parameterViewModel = GetParameterViewModel(parameter, val, InstanceSet, MaterialNames);
			m_parameterList.Add(parameterViewModel);
		}
	}

	private void ReimportAllImportableEntities(object context)
	{
		ParameterSetImporting = true;
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		m_previousCursor = Mouse.OverrideCursor;
		Mouse.OverrideCursor = Cursors.Wait;
		backgroundWorker.DoWork += StartImport;
		backgroundWorker.RunWorkerCompleted += ImportCompleted;
		backgroundWorker.RunWorkerAsync(context);
	}
}
