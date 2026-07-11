using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class MiniSourceObjectViewModel : SelectableItemViewModel, IComparable<MiniSourceObjectViewModel>
{
	private ObservableCollection<string> m_entityClasses;

	private bool m_entityAlreadyExists;

	private bool m_overwriteEnabled;

	private bool m_overwriteSelected;

	private bool m_selectionEnabled = true;

	private string m_selectedEntityClass = null;

	public ObservableCollection<string> AllowedEntityClasses
	{
		get
		{
			return m_entityClasses;
		}
		set
		{
			if (m_entityClasses != value)
			{
				m_entityClasses = value;
				OnPropertyChanged("AllowedEntityClasses");
			}
		}
	}

	public string ClassName
	{
		get
		{
			IEntityCacheData relevantCacheData = GetRelevantCacheData();
			if (relevantCacheData == null)
			{
				return string.Empty;
			}
			return relevantCacheData.Class;
		}
	}

	public IImportedEntity Entity { get; private set; }

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

	public bool HasInvalidClass
	{
		get
		{
			string className = ClassName;
			return !string.IsNullOrEmpty(className) && !AllowedEntityClasses.Contains(ClassName);
		}
	}

	public string Name
	{
		get
		{
			return Entity.Name;
		}
		set
		{
			if (Entity.Name != value)
			{
				Entity.Name = value;
				RefreshEntityExistance();
				IEntityCacheData relevantCacheData = GetRelevantCacheData();
				if (relevantCacheData != null && string.Equals(relevantCacheData.Name, Entity.Name, StringComparison.CurrentCultureIgnoreCase))
				{
					Entity.Name = relevantCacheData.Name;
				}
				OnPropertyChanged("Name");
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

	public bool OverwriteSelected
	{
		get
		{
			return m_overwriteSelected;
		}
		set
		{
			if (m_overwriteSelected != value)
			{
				m_overwriteSelected = value;
				OnPropertyChanged("OverwriteSelected");
			}
		}
	}

	public string SelectedEntityClass
	{
		get
		{
			return m_selectedEntityClass;
		}
		set
		{
			if (m_selectedEntityClass != value)
			{
				m_selectedEntityClass = value;
				OnPropertyChanged("SelectedEntityClass");
				Entity.ClassName = value;
				RefreshEntityExistance();
			}
		}
	}

	public bool SelectionEnabled
	{
		get
		{
			return m_selectionEnabled;
		}
		set
		{
			if (m_selectionEnabled != value)
			{
				m_selectionEnabled = value;
				OnPropertyChanged("SelectionEnabled");
			}
		}
	}

	protected ICivTechService CivTechService { get; private set; }

	private IEntityCacheService CacheService { get; }

	public MiniSourceObjectViewModel(ICivTechService civTechSvc, IImportedEntity entity, IEntityContainerClass assetClass, IEntityCacheService cacheService)
		: this(civTechSvc, entity, assetClass.GetAllowedClasses(entity.Type), cacheService)
	{
	}

	public MiniSourceObjectViewModel(ICivTechService civTechSvc, IImportedEntity entity, IEnumerable<string> allowedClassNames, IEntityCacheService cacheService)
	{
		CivTechService = civTechSvc;
		Entity = entity;
		AllowedEntityClasses = new ObservableCollection<string>(allowedClassNames);
		CacheService = cacheService;
		SelectedEntityClass = GetDefaultClassName(entity, AllowedEntityClasses);
	}

	private string GetDefaultClassName(IInstanceEntity entity, ICollection<string> allowedClassNames)
	{
		if (!string.IsNullOrEmpty(entity.ClassName))
		{
			return entity.ClassName;
		}
		IEntityCacheData relevantCacheData = GetRelevantCacheData();
		if (relevantCacheData != null)
		{
			return relevantCacheData.Class;
		}
		string classNameFromCache = GetClassNameFromCache(entity);
		if (!string.IsNullOrEmpty(classNameFromCache))
		{
			return classNameFromCache;
		}
		if (allowedClassNames.Count == 1)
		{
			return allowedClassNames.First();
		}
		return string.Empty;
	}

	private IEntityCacheData GetRelevantCacheData()
	{
		EntityID entityID = new EntityID(Entity);
		IEnumerable<IEntityCacheData> cacheData = CacheService.GetCacheData(entityID);
		string currentProjectName = CivTechService.PrimaryProject.Name;
		return cacheData.FirstOrDefault((IEntityCacheData data) => data.Project.Equals(currentProjectName, StringComparison.CurrentCultureIgnoreCase));
	}

	private string GetClassNameFromCache(IInstanceEntity entity)
	{
		EntityID entityID = new EntityID(entity);
		IEnumerable<IEntityCacheData> cacheData = CacheService.GetCacheData(entityID);
		if (!cacheData.Any())
		{
			return string.Empty;
		}
		return cacheData.First().Class;
	}

	public int CompareTo(MiniSourceObjectViewModel other)
	{
		if (other == null)
		{
			return 1;
		}
		return Name.CompareTo(other.Name);
	}

	private void RefreshEntityExistance()
	{
		IEntityCacheData relevantCacheData = GetRelevantCacheData();
		if (relevantCacheData != null)
		{
			EntityAlreadyExists = true;
			if (SelectedEntityClass != null)
			{
				OverwriteEnabled = relevantCacheData.Class.Equals(SelectedEntityClass, StringComparison.CurrentCultureIgnoreCase);
			}
			else
			{
				Entity.ClassName = relevantCacheData.Class;
				m_selectedEntityClass = relevantCacheData.Class;
				OverwriteEnabled = true;
				OnPropertyChanged("SelectedEntityClass");
			}
		}
		else
		{
			EntityAlreadyExists = false;
			OverwriteSelected = false;
			OverwriteEnabled = false;
		}
		OnPropertyChanged("EntityAlreadyExists");
		OnPropertyChanged("OverwriteEnabled");
		OnPropertyChanged("OverwriteSelected");
	}
}
