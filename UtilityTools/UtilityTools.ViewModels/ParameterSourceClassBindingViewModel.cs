using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.MVVMBase;
using Firaxis.Utility;

namespace UtilityTools.ViewModels;

public class ParameterSourceClassBindingViewModel : Notifier, IDisposable
{
	private IInstanceEntity m_associatedEntity;

	private bool m_autoMatchFound = false;

	private bool m_entityAlreadyExists;

	private bool m_overwrite;

	private bool m_overwriteEnabled;

	private string m_selectedClassName;

	private SourceObjectModel m_selectedSourceObject;

	public IInstanceEntity AssociatedEntity
	{
		get
		{
			return m_associatedEntity;
		}
		private set
		{
			if (m_associatedEntity != value)
			{
				m_associatedEntity = value;
				value.ClassName = SelectedClassName;
				Value.BindObject(value.Name, Parameter.ObjectType);
				OnPropertyChanged("ObjectValue");
				OnPropertyChanged("ObjectValueEnabled");
				OnPropertyChanged("SelectedClassName");
			}
		}
	}

	public bool AutoMatchFound
	{
		get
		{
			return m_autoMatchFound;
		}
		set
		{
			if (m_autoMatchFound != value)
			{
				m_autoMatchFound = value;
				OnPropertyChanged("AutoMatchFound");
			}
		}
	}

	public ObservableCollection<string> ClassNames { get; set; }

	public bool ClassNamesEnabled => ClassNames.Count > 1 && !string.IsNullOrEmpty(SelectedClassName) && AssociatedEntity != null;

	public bool EntityAlreadyExists
	{
		get
		{
			return m_entityAlreadyExists;
		}
		set
		{
			if (m_entityAlreadyExists != value)
			{
				m_entityAlreadyExists = value;
				OnPropertyChanged("EntityAlreadyExists");
			}
		}
	}

	public string ObjectValue
	{
		get
		{
			if (AssociatedEntity == null)
			{
				return string.Empty;
			}
			return AssociatedEntity.Name;
		}
		set
		{
			if (AssociatedEntity.Name != value)
			{
				AssociatedEntity.Name = value;
				SetSourceEntity();
				RefreshEntityExistance();
				if (SourceInstanceEntity != null && string.Equals(SourceInstanceEntity.Name, AssociatedEntity.Name, StringComparison.CurrentCultureIgnoreCase))
				{
					AssociatedEntity.Name = SourceInstanceEntity.Name;
				}
				Value.BindObject(AssociatedEntity.Name, Parameter.ObjectType);
				OnPropertyChanged("ObjectValue");
			}
		}
	}

	public bool ObjectValueEnabled => AssociatedEntity != null;

	public bool Overwrite
	{
		get
		{
			return m_overwrite;
		}
		set
		{
			if (m_overwrite != value)
			{
				m_overwrite = value;
				OnPropertyChanged("Overwrite");
			}
		}
	}

	public bool OverwriteEnabled
	{
		get
		{
			return m_overwriteEnabled;
		}
		set
		{
			if (m_overwriteEnabled != value)
			{
				m_overwriteEnabled = value;
				OnPropertyChanged("OverwriteEnabled");
			}
		}
	}

	public string ParameterName => Parameter.Name;

	public string SelectedClassName
	{
		get
		{
			return m_selectedClassName;
		}
		set
		{
			if (m_selectedClassName != value)
			{
				m_selectedClassName = value;
				OnPropertyChanged("SelectedClassName");
				if (AssociatedEntity != null)
				{
					AssociatedEntity.ClassName = value;
					RefreshEntityExistance();
				}
			}
		}
	}

	public SourceObjectModel SelectedSourceObject
	{
		get
		{
			return m_selectedSourceObject;
		}
		set
		{
			if (m_selectedSourceObject != value)
			{
				m_selectedSourceObject = value;
				OnPropertyChanged("SelectedSourceObject");
				SetAssociatedEntity();
				SetSourceEntity();
				RefreshEntityExistance();
			}
		}
	}

	public bool SourceObjectNamesEnabled => Sources.Count > 1;

	public ObservableCollection<SourceObjectModel> Sources { get; set; }

	protected ICivTechService CivTechService { get; private set; }

	private IInstanceSet EntitySet { get; set; }

	private string LocalPantry => CivTechService.PrimaryProject.Paths.GamePantry;

	private IObjectParameter Parameter { get; set; }

	private IInstanceEntity SourceInstanceEntity { get; set; }

	private IInstanceSet SourceInstanceSet { get; set; }

	private IObjectValue Value { get; set; }

	public ParameterSourceClassBindingViewModel(ICivTechService civTechSvc, IObjectParameter objectParameter, IObjectValue objectValue, IEnumerable<SourceFileModel> sourceFiles, IInstanceSet entitySet)
	{
		CivTechService = civTechSvc;
		Parameter = objectParameter;
		Value = objectValue;
		EntitySet = entitySet;
		SourceInstanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() });
		ClassNames = new ObservableCollection<string>(objectParameter.AllowedClasses);
		BuildSourcesList(sourceFiles);
		AutomaticallyAssignSourceObject();
		AutoMatchFound = SelectedSourceObject != null;
		if (AutoMatchFound)
		{
			SetAssociatedEntity();
			SetSourceEntity();
			EnsureNameCaseCorrectness();
		}
		if (ClassNames.Count == 1)
		{
			SelectedClassName = ClassNames[0];
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && SourceInstanceSet != null)
		{
			SourceInstanceSet.Dispose();
			SourceInstanceSet = null;
		}
	}

	private void AutomaticallyAssignSourceObject()
	{
		if (Sources.Count == 1)
		{
			m_selectedSourceObject = Sources[0];
			return;
		}
		string text = ParameterName.ToLower();
		char c = text.First();
		bool flag = text.Contains("fow");
		IEnumerator<SourceObjectModel> enumerator = Sources.GetEnumerator();
		while (m_selectedSourceObject == null && enumerator.MoveNext())
		{
			SourceObjectModel current = enumerator.Current;
			string text2 = current.DisplayName.ToLower();
			if (text2 == text || text2.Last() == c || (flag && text2.Contains("fow")))
			{
				m_selectedSourceObject = current;
			}
		}
	}

	private void BuildSourcesList(IEnumerable<SourceFileModel> sourceFiles)
	{
		List<SourceObjectModel> list = new List<SourceObjectModel>();
		foreach (SourceFileModel sourceFile in sourceFiles)
		{
			list.AddRange(sourceFile.SourceObjects);
		}
		Sources = new ObservableCollection<SourceObjectModel>(list.OrderBy((SourceObjectModel x) => x.DisplayName));
	}

	private void EnsureNameCaseCorrectness()
	{
		if (SourceInstanceEntity != null && AssociatedEntity != null)
		{
			string name = SourceInstanceEntity.Name;
			if (string.Equals(name, AssociatedEntity.Name, StringComparison.CurrentCultureIgnoreCase))
			{
				AssociatedEntity.Name = name;
			}
		}
	}

	private void RefreshEntityExistance()
	{
		if (AssociatedEntity == null)
		{
			return;
		}
		if (global::DatabaseWrapper.DatabaseWrapper.DoesEntityExist(AssociatedEntity))
		{
			EntityAlreadyExists = true;
			if (SelectedClassName != null && SourceInstanceEntity != null)
			{
				OverwriteEnabled = SourceInstanceEntity.ClassName == SelectedClassName;
			}
			else
			{
				OverwriteEnabled = false;
			}
		}
		else
		{
			EntityAlreadyExists = false;
			Overwrite = false;
			OverwriteEnabled = false;
		}
		OnPropertyChanged("EntityAlreadyExists");
		OnPropertyChanged("OverwriteEnabled");
		OnPropertyChanged("OverwriteSelected");
	}

	private void SetAssociatedEntity()
	{
		string selectedSourcePath = SelectedSourceObject.SourceFilePath.LocalPath;
		string selectedSourceName = SelectedSourceObject.SourceObjectName;
		AssociatedEntity = EntitySet.Items.OfType<IImportedEntity>().FirstOrDefault((IImportedEntity ent) => ent.SourceFilePath == selectedSourcePath && ent.SourceObjectName == selectedSourceName);
	}

	private void SetSourceEntity()
	{
		if (AssociatedEntity != null)
		{
			SourceInstanceSet.Clear();
			SourceInstanceEntity = SourceInstanceSet.LoadEntityByName(AssociatedEntity.Name, AssociatedEntity.Type);
		}
	}
}
