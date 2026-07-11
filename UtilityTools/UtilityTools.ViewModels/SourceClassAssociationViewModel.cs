using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;
using Firaxis.ContentExporters;
using Firaxis.MVVMBase;
using Firaxis.Utility;
using UtilityTools.Helpers;

namespace UtilityTools.ViewModels;

public class SourceClassAssociationViewModel : DialogViewModel, IDisposable
{
	private DelegateCommand m_overwriteAllCommand;

	private double m_classesDropdownWidth;

	private double m_entityNameEntryWidth;

	private double m_parameterLabelWidth;

	private string m_sourcesMatchedString;

	private double m_sourceObjectDropdownWidth;

	private Dictionary<IObjectValue, string> m_startingValues = new Dictionary<IObjectValue, string>();

	private readonly Predicate<IImportedEntity> m_exportPredicate;

	public bool Busy { get; private set; }

	public double ClassesDropdownWidth
	{
		get
		{
			return m_classesDropdownWidth;
		}
		set
		{
			if (m_classesDropdownWidth != value)
			{
				m_classesDropdownWidth = value;
				OnPropertyChanged("ClassesDropdownWidth");
			}
		}
	}

	public double EntityNameEntryWidth
	{
		get
		{
			return m_entityNameEntryWidth;
		}
		set
		{
			if (m_entityNameEntryWidth != value)
			{
				m_entityNameEntryWidth = value;
				OnPropertyChanged("EntityNameEntryWidth");
			}
		}
	}

	public IInstanceSet InstancesToImport { get; private set; }

	public double ParameterLabelWidth
	{
		get
		{
			return m_parameterLabelWidth;
		}
		set
		{
			if (m_parameterLabelWidth != value)
			{
				m_parameterLabelWidth = value;
				OnPropertyChanged("ParameterLabelWidth");
			}
		}
	}

	public string SourceFilePath { get; private set; }

	public bool SourceFilePathVisible => !string.IsNullOrEmpty(SourceFilePath);

	public double SourceObjectDropdownWidth
	{
		get
		{
			return m_sourceObjectDropdownWidth;
		}
		set
		{
			if (m_sourceObjectDropdownWidth != value)
			{
				m_sourceObjectDropdownWidth = value;
				OnPropertyChanged("SourceObjectDropdownWidth");
			}
		}
	}

	public ObservableCollection<ParameterSourceClassBindingViewModel> SourceObjects { get; set; }

	public string SourcesMatchedString
	{
		get
		{
			return m_sourcesMatchedString;
		}
		set
		{
			if (m_sourcesMatchedString != value)
			{
				m_sourcesMatchedString = value;
				OnPropertyChanged("SourcesMatchedString");
			}
		}
	}

	private ICivTechService CivTechService { get; set; }

	private Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> ImportFunction { get; set; }

	private InstanceType NewEntityType { get; set; }

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

	public SourceClassAssociationViewModel(ICivTechService civTechSvc, IObjectParameter param, IObjectValue value, IEnumerable<SourceFileModel> sourceFiles, InstanceType entityType, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> exportPred, IInstanceSet instances = null)
	{
		CivTechService = civTechSvc;
		SourceObjects = new ObservableCollection<ParameterSourceClassBindingViewModel>();
		if (instances == null)
		{
			InstancesToImport = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() });
		}
		else
		{
			InstancesToImport = instances;
		}
		ImportFunction = importFunction;
		m_exportPredicate = exportPred;
		foreach (SourceFileModel sourceFile in sourceFiles)
		{
			foreach (SourceObjectModel sourceObject in sourceFile.SourceObjects)
			{
				if (InstancesToImport.FindByNameAndType(sourceObject.SmartName, entityType) == null)
				{
					IInstanceEntity instanceEntity = InstancesToImport.CreateEntityByName(sourceObject.SmartName, entityType);
					if (instanceEntity is IImportedEntity importedEntity)
					{
						importedEntity.SourceFilePath = sourceFile.SourceFilePath.LocalPath;
						importedEntity.SourceObjectName = sourceObject.SourceObjectName;
					}
					else
					{
						InstancesToImport.Remove(instanceEntity);
					}
				}
			}
		}
		m_startingValues.Add(value, value.GetBoundObjectName());
		ParameterSourceClassBindingViewModel item = new ParameterSourceClassBindingViewModel(civTechSvc, param, value, sourceFiles, InstancesToImport);
		SourceObjects.Add(item);
		NewEntityType = entityType;
		SetWidths(new string[1] { param.Name }, sourceFiles, new List<IEnumerable<string>> { param.AllowedClasses });
	}

	public SourceClassAssociationViewModel(ICivTechService civTechSvc, IParameterSet parameters, IValueSet values, IEnumerable<SourceFileModel> sourceFiles, InstanceType entityType, Func<ICivTechService, string, IEnumerable<IImportedEntity>, IEnumerable<ImportOperationResult>> importFunction, Predicate<IImportedEntity> exportPred, IInstanceSet instances = null)
	{
		CivTechService = civTechSvc;
		NewEntityType = entityType;
		if (instances == null)
		{
			InstancesToImport = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { civTechSvc.GetActivePantryPaths() });
		}
		else
		{
			InstancesToImport = instances;
		}
		SourceObjects = new ObservableCollection<ParameterSourceClassBindingViewModel>();
		ImportFunction = importFunction;
		m_exportPredicate = exportPred;
		foreach (SourceFileModel sourceFile in sourceFiles)
		{
			foreach (SourceObjectModel sourceObject in sourceFile.SourceObjects)
			{
				if (InstancesToImport.FindByNameAndType(sourceObject.SmartName, entityType) == null)
				{
					IInstanceEntity instanceEntity = InstancesToImport.CreateEntityByName(sourceObject.SmartName, entityType);
					if (instanceEntity is IImportedEntity importedEntity)
					{
						importedEntity.SourceFilePath = sourceFile.SourceFilePath.LocalPath;
						importedEntity.SourceObjectName = sourceObject.SourceObjectName;
					}
					else
					{
						InstancesToImport.Remove(instanceEntity);
					}
				}
			}
		}
		IEnumerable<IObjectValue> enumerable = values.Items.OfType<IObjectValue>();
		foreach (IObjectValue item2 in enumerable)
		{
			IObjectParameter objectParameter = parameters.FindByName(item2.ParameterName) as IObjectParameter;
			if (objectParameter.ObjectType == entityType)
			{
				m_startingValues.Add(item2, item2.GetBoundObjectName());
				ParameterSourceClassBindingViewModel item = new ParameterSourceClassBindingViewModel(civTechSvc, objectParameter, item2, sourceFiles, InstancesToImport);
				SourceObjects.Add(item);
			}
		}
		List<IEnumerable<string>> list = new List<IEnumerable<string>>();
		foreach (ParameterSourceClassBindingViewModel sourceObject2 in SourceObjects)
		{
			list.Add(sourceObject2.ClassNames);
		}
		int num = SourceObjects.Count((ParameterSourceClassBindingViewModel source) => source.AutoMatchFound);
		SourcesMatchedString = $"{SourceObjects.Count} parameters found.  {num} automatic matches.";
		SetWidths(from param in parameters.ItemsByType<IObjectParameter>()
			select param.Name, sourceFiles, list);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		foreach (IDisposable item in SourceObjects.OfType<IDisposable>())
		{
			item.Dispose();
		}
		SourceObjects.Clear();
	}

	protected override void ExecuteCancelCommand(object context)
	{
		foreach (KeyValuePair<IObjectValue, string> startingValue in m_startingValues)
		{
			startingValue.Key.BindObject(startingValue.Value, startingValue.Key.GetBoundObjectType());
		}
		m_startingValues.Clear();
		base.ExecuteCancelCommand(context);
	}

	protected override void ExecuteOKCommand(object context)
	{
		RefreshCommands();
		IEnumerable<ParameterSourceClassBindingViewModel> enumerable = SourceObjects.Where((ParameterSourceClassBindingViewModel source) => !string.IsNullOrEmpty(source.ObjectValue) && !string.IsNullOrEmpty(source.SelectedClassName));
		bool flag = true;
		List<string> list = new List<string>();
		foreach (ParameterSourceClassBindingViewModel item in enumerable)
		{
			if (item.EntityAlreadyExists && !item.Overwrite)
			{
				flag = false;
				list.Add(item.ObjectValue);
			}
		}
		if (!flag)
		{
			DialogHelper.DisplayError(string.Format("Cannot continue with the import.  The following names are taken and are not being overwritten:\n\t{0}", string.Join("\n\t", list)), "Name Collision");
			return;
		}
		List<IImportedEntity> list2 = enumerable.Select((ParameterSourceClassBindingViewModel entity) => entity.AssociatedEntity as IImportedEntity).ToList();
		List<IImportedEntity> list3 = new List<IImportedEntity>();
		foreach (IImportedEntity item2 in list2)
		{
			if (!m_exportPredicate(item2))
			{
				list3.Add(item2);
			}
		}
		if (list3.Count > 0)
		{
			string text = string.Join("\n", list3.Select((IImportedEntity ent) => ent.Name));
			MessageBoxResult messageBoxResult = MessageBox.Show("The following entities cannot be exported because they are read-only (and not open in Perforce).  Continue anyway?\n\n" + text, "Continue with Export?", MessageBoxButton.YesNo);
			if (messageBoxResult != MessageBoxResult.Yes)
			{
				return;
			}
			list2 = list2.Except(list3).ToList();
		}
		bool flag2 = false;
		using (new WaitCursor())
		{
			Busy = true;
			IEnumerable<ImportOperationResult> allResults = ImportFunction(CivTechService, CivTechService.PrimaryProject.Name, list2);
			IEnumerable<ImportOperationResult> failedResults = allResults.GetFailedResults();
			flag2 = !failedResults.Any();
			Busy = false;
			if (!flag2)
			{
				DialogHelper.DisplayError(failedResults.GetCombinedFailureMessages());
			}
		}
		RefreshCommands();
		if (flag2)
		{
			base.ExecuteOKCommand(context);
		}
	}

	private void ExecuteOverwriteAllCommand(object context)
	{
		RefreshCommands();
		SourceObjects.Where((ParameterSourceClassBindingViewModel s) => s.OverwriteEnabled).ForEach(delegate(ParameterSourceClassBindingViewModel s)
		{
			s.Overwrite = (bool)context;
		});
	}

	private Size MeasureString(string candidate)
	{
		FormattedText formattedText = new FormattedText(candidate, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12.0, Brushes.White);
		return new Size(formattedText.Width, formattedText.Height);
	}

	private void SetWidths(IEnumerable<string> paramNames, IEnumerable<SourceFileModel> sourceFiles, IEnumerable<IEnumerable<string>> classLists)
	{
		ParameterLabelWidth = double.MinValue;
		foreach (string paramName in paramNames)
		{
			ParameterLabelWidth = Math.Max(ParameterLabelWidth, MeasureString(paramName).Width);
		}
		EntityNameEntryWidth = double.MinValue;
		SourceObjectDropdownWidth = double.MinValue;
		foreach (SourceFileModel sourceFile in sourceFiles)
		{
			foreach (SourceObjectModel sourceObject in sourceFile.SourceObjects)
			{
				SourceObjectDropdownWidth = Math.Max(SourceObjectDropdownWidth, MeasureString(sourceObject.DisplayName).Width);
				EntityNameEntryWidth = Math.Max(EntityNameEntryWidth, MeasureString(sourceObject.SmartName).Width);
			}
		}
		ClassesDropdownWidth = double.MinValue;
		foreach (IEnumerable<string> classList in classLists)
		{
			foreach (string item in classList)
			{
				ClassesDropdownWidth = Math.Max(ClassesDropdownWidth, MeasureString(item).Width);
			}
		}
		ParameterLabelWidth += 5.0;
		SourceObjectDropdownWidth += 25.0;
		EntityNameEntryWidth += 8.0;
		ClassesDropdownWidth += 25.0;
	}
}
