using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.ATF;

[Export(typeof(IProjectChangeWatcher))]
[Export(typeof(IEntityFilteringService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntityFilteringService : IEntityFilteringService, IProjectChangeWatcher, IDisposable
{
	private bool disposedValue;

	private ICivTechService CivTechService { get; set; }

	private IEntityCacheService EntityCacheService { get; set; }

	private ICollection<IEntityFilteringContext> RegisteredFilteringContexts { get; set; } = new List<IEntityFilteringContext>();

	private IDictionary<Type, Type> RegisteredEntityFilterTypes { get; set; } = new Dictionary<Type, Type>();

	private ICollection<IEntityFilterDefinition> RegisteredFilterTypeConcretes { get; set; } = new List<IEntityFilterDefinition>();

	private IProjectFilterDefinition DefaultProjectFilter { get; set; }

	[ImportingConstructor]
	public EntityFilteringService(ICivTechService civTechService, IEntityCacheService entityCacheService)
	{
		CivTechService = civTechService;
		EntityCacheService = entityCacheService;
		RegisterDefaultFilterTypes();
		DefaultProjectFilter = CreateFilterDefinition<IProjectFilterDefinition>();
		string name = CivTechService.PrimaryProject.Name;
		DefaultProjectFilter.ValidProjectNames.Add(name);
		foreach (string dependency in CivTechService.PrimaryProject.Dependencies)
		{
			DefaultProjectFilter.ValidProjectNames.Add(CivTechService.AllProjectsMap[dependency].Name);
		}
		Context.Add(this);
	}

	private void RegisterDefaultFilterTypes()
	{
		RegisterEntityFilter(typeof(INameFilterDefinition), typeof(NameFilter));
		RegisterEntityFilter(typeof(ITagFilterDefinition), typeof(TagFilter));
		RegisterEntityFilter(typeof(IClassFilterDefinition), typeof(ClassFilter));
		RegisterEntityFilter(typeof(IProjectFilterDefinition), typeof(ProjectFilter));
		RegisterEntityFilter(typeof(IInstanceTypeFilterDefinition), typeof(InstanceTypeFilter));
	}

	public IEnumerable<string> GetAvailableFilters()
	{
		return RegisteredFilterTypeConcretes.Select((IEntityFilterDefinition filter) => filter.FilterName);
	}

	public IEnumerable<IEntityFilterDefinition> GetFilters(string filterName)
	{
		return RegisteredFilterTypeConcretes.Where((IEntityFilterDefinition filter) => filter.FilterName.Equals(filterName, StringComparison.InvariantCulture));
	}

	public IProjectFilterDefinition GetDefaultProjectFilter()
	{
		return DefaultProjectFilter;
	}

	public T CreateFilterDefinition<T>() where T : class, IEntityFilterDefinition
	{
		Type typeFromHandle = typeof(T);
		Type value = null;
		if (!RegisteredEntityFilterTypes.TryGetValue(typeFromHandle, out value) && !typeFromHandle.IsAbstract && !typeFromHandle.IsInterface && RegisteredEntityFilterTypes.Values.Contains(typeFromHandle))
		{
			value = typeFromHandle;
		}
		if (value == null)
		{
			return null;
		}
		T val = Activator.CreateInstance(value) as T;
		if (val != null)
		{
			if (val is ICivTechServiceConsumer civTechServiceConsumer)
			{
				civTechServiceConsumer.CivTechService = CivTechService;
			}
			if (val is IEntityCacheServiceConsumer entityCacheServiceConsumer)
			{
				entityCacheServiceConsumer.EntityCacheService = EntityCacheService;
			}
		}
		return val;
	}

	public void RegisterEntityFilter(Type interfaceType, Type concreteType)
	{
		ValidateRegisterEntityFilterArguments(interfaceType, concreteType);
		RegisteredEntityFilterTypes[interfaceType] = concreteType;
		if (Activator.CreateInstance(concreteType) is IEntityFilterDefinition entityFilterDefinition)
		{
			if (entityFilterDefinition is ICivTechServiceConsumer civTechServiceConsumer)
			{
				civTechServiceConsumer.CivTechService = CivTechService;
			}
			if (entityFilterDefinition is IEntityCacheServiceConsumer entityCacheServiceConsumer)
			{
				entityCacheServiceConsumer.EntityCacheService = EntityCacheService;
			}
			RegisteredFilterTypeConcretes.Add(entityFilterDefinition);
		}
	}

	private void ValidateRegisterEntityFilterArguments(Type interfaceType, Type concreteType)
	{
		if (interfaceType == null)
		{
			throw new ArgumentNullException("interfaceType");
		}
		if (concreteType == null)
		{
			throw new ArgumentNullException("concreteType");
		}
		Type typeFromHandle = typeof(IEntityFilterDefinition);
		if (!typeFromHandle.IsAssignableFrom(interfaceType))
		{
			throw new ArgumentException(string.Format("The argument must implement the {0} interface.", typeFromHandle.ToString(), "interfaceType"));
		}
		if (!typeFromHandle.IsAssignableFrom(concreteType))
		{
			throw new ArgumentException(string.Format("The argument must implement the {0} interface.", typeFromHandle.ToString(), "concreteType"));
		}
		if (concreteType.IsAbstract || concreteType.IsInterface)
		{
			throw new ArgumentException("The argument must be a concrete implementation.", "concreteType");
		}
		if (concreteType.GetConstructor(Type.EmptyTypes) == null)
		{
			throw new ArgumentException("The concrete type must provide a default constructor.", "concreteType");
		}
	}

	public IEntityFilteringContext GetFilteringContext(IEnumerable<InstanceType> allowedTypes, IEnumerable<string> allowedClassNames)
	{
		foreach (IEntityFilteringContext registeredFilteringContext in RegisteredFilteringContexts)
		{
			IInstanceTypeFilterDefinition filterDefinition = registeredFilteringContext.GetFilterDefinition<IInstanceTypeFilterDefinition>();
			IClassFilterDefinition filterDefinition2 = registeredFilteringContext.GetFilterDefinition<IClassFilterDefinition>();
			if (AreEquivalent(allowedTypes, filterDefinition) && AreEquivalent(allowedClassNames, filterDefinition2))
			{
				return registeredFilteringContext;
			}
		}
		IEntityFilteringContext entityFilteringContext = new EntityFilteringContext();
		if (allowedTypes != null)
		{
			InstanceTypeFilter instanceTypeFilter = new InstanceTypeFilter();
			foreach (InstanceType allowedType in allowedTypes)
			{
				instanceTypeFilter.ValidInstanceTypes.Add(allowedType);
			}
			entityFilteringContext.FilterDefinitions.Add(instanceTypeFilter);
		}
		if (allowedClassNames != null)
		{
			ClassFilter classFilter = new ClassFilter();
			classFilter.EntityCacheService = EntityCacheService;
			foreach (string allowedClassName in allowedClassNames)
			{
				classFilter.ValidClassNames.Add(allowedClassName);
			}
			entityFilteringContext.FilterDefinitions.Add(classFilter);
		}
		entityFilteringContext.FilterDefinitions.Add(DefaultProjectFilter);
		RegisteredFilteringContexts.Add(entityFilteringContext);
		return entityFilteringContext;
	}

	public void HandleProjectChange(Action<string> statusMessagePrinter)
	{
		statusMessagePrinter("Resetting project filter...");
		DefaultProjectFilter.ValidProjectNames.Clear();
		string name = CivTechService.PrimaryProject.Name;
		DefaultProjectFilter.ValidProjectNames.Add(name);
		foreach (string dependency in CivTechService.PrimaryProject.Dependencies)
		{
			DefaultProjectFilter.ValidProjectNames.Add(CivTechService.AllProjectsMap[dependency].Name);
		}
	}

	private bool AreEquivalent(IEnumerable<InstanceType> allowedTypes, IInstanceTypeFilterDefinition typeFiler)
	{
		if (allowedTypes == null && typeFiler == null)
		{
			return true;
		}
		if (allowedTypes == null && typeFiler != null)
		{
			return false;
		}
		if (allowedTypes != null && typeFiler == null)
		{
			return false;
		}
		foreach (InstanceType allowedType in allowedTypes)
		{
			if (!typeFiler.ValidInstanceTypes.Contains(allowedType))
			{
				return false;
			}
		}
		foreach (InstanceType validInstanceType in typeFiler.ValidInstanceTypes)
		{
			if (!allowedTypes.Contains(validInstanceType))
			{
				return false;
			}
		}
		return true;
	}

	private bool AreEquivalent(IEnumerable<string> allowedClassNames, IClassFilterDefinition classFilter)
	{
		if (allowedClassNames == null && classFilter == null)
		{
			return true;
		}
		if (allowedClassNames == null && classFilter != null)
		{
			return false;
		}
		if (allowedClassNames != null && classFilter == null)
		{
			return false;
		}
		foreach (string allowedClassName in allowedClassNames)
		{
			if (!classFilter.ValidClassNames.Contains(allowedClassName))
			{
				return false;
			}
		}
		foreach (string validClassName in classFilter.ValidClassNames)
		{
			if (!allowedClassNames.Contains(validClassName))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				CivTechService = null;
				EntityCacheService = null;
				Context.Remove(this);
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
