using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetBrowser;

internal class StackFilterFactory
{
	private class FilterFactory
	{
		private Func<IEnumerable<string>> DomainFunction { get; }

		private Func<IEnumerable<string>, IEntityFilterDefinition> FactoryFunction { get; }

		public FilterFactory(Func<IEnumerable<string>> domainFunction, Func<IEnumerable<string>, IEntityFilterDefinition> factoryFunction)
		{
			DomainFunction = domainFunction;
			FactoryFunction = factoryFunction;
		}

		public FilterBuilderViewModel CreateFilterBuilder(string filterName)
		{
			if (DomainFunction == null)
			{
				return null;
			}
			return new FilterBuilderViewModel(filterName, DomainFunction());
		}

		public IEntityFilterDefinition CreateFilterDefinition(IEnumerable<string> input)
		{
			if (FactoryFunction == null)
			{
				return null;
			}
			return FactoryFunction(input);
		}
	}

	private Dictionary<string, FilterFactory> Factories { get; } = new Dictionary<string, FilterFactory>();

	private ICivTechService CivTechService { get; }

	private IEntityCacheService EntityCacheService { get; }

	private IEntityFilteringService FilterService { get; }

	public IEnumerable<string> RegisteredFilterNames => Factories.Keys;

	public StackFilterFactory(ICivTechService civTechService, IEntityCacheService entityCacheService, IEntityFilteringService filterService)
	{
		CivTechService = civTechService;
		EntityCacheService = entityCacheService;
		FilterService = filterService;
		PopulateFilterFactories();
	}

	private void PopulateFilterFactories()
	{
		RegisterFilterFactory("Class", GetAvailableClasses, CreateClassFilter);
		RegisterFilterFactory("Tag", GetAvailableTags, CreateTagFilter);
		RegisterFilterFactory("Project", GetAvailableProjects, CreateProjectFilter);
		RegisterFilterFactory("Type", GetAvailableInstanceTypes, CreateTypeFilter);
	}

	private IEnumerable<string> GetAvailableClasses()
	{
		return CivTechService.PrimaryProject.Config.Classes.Items.Select((IClassEntity item) => item.Name);
	}

	private IEntityFilterDefinition CreateClassFilter(IEnumerable<string> classNames)
	{
		if (!classNames.Any())
		{
			return null;
		}
		IClassFilterDefinition classFilterDefinition = FilterService.CreateFilterDefinition<IClassFilterDefinition>();
		foreach (string className in classNames)
		{
			classFilterDefinition.ValidClassNames.Add(className);
		}
		return classFilterDefinition;
	}

	private IEnumerable<string> GetAvailableTags()
	{
		return EntityCacheService.GetAllTags();
	}

	private IEntityFilterDefinition CreateTagFilter(IEnumerable<string> tags)
	{
		if (!tags.Any())
		{
			return null;
		}
		ITagFilterDefinition tagFilterDefinition = FilterService.CreateFilterDefinition<ITagFilterDefinition>();
		tagFilterDefinition.FilterText = string.Join("|", tags);
		return tagFilterDefinition;
	}

	private IEnumerable<string> GetAvailableProjects()
	{
		yield return CivTechService.PrimaryProject.Name;
		foreach (string dependency in CivTechService.PrimaryProject.Dependencies)
		{
			yield return dependency;
		}
	}

	private IEntityFilterDefinition CreateProjectFilter(IEnumerable<string> selectedProjects)
	{
		if (!selectedProjects.Any())
		{
			return null;
		}
		IProjectFilterDefinition projectFilterDefinition = FilterService.CreateFilterDefinition<IProjectFilterDefinition>();
		foreach (string selectedProject in selectedProjects)
		{
			projectFilterDefinition.ValidProjectNames.Add(selectedProject);
		}
		return projectFilterDefinition;
	}

	private IEnumerable<string> GetAvailableInstanceTypes()
	{
		foreach (InstanceType type in StaticMethods.GetValidInstanceTypes())
		{
			yield return EnumToStringConverter.GetNameFromType(type);
		}
	}

	private IEntityFilterDefinition CreateTypeFilter(IEnumerable<string> selectedTypes)
	{
		if (!selectedTypes.Any())
		{
			return null;
		}
		IInstanceTypeFilterDefinition instanceTypeFilterDefinition = FilterService.CreateFilterDefinition<IInstanceTypeFilterDefinition>();
		foreach (string selectedType in selectedTypes)
		{
			InstanceType typeFromName = EnumToStringConverter.GetTypeFromName(selectedType);
			if (typeFromName != InstanceType.IT_COUNT && typeFromName != InstanceType.IT_INVALID)
			{
				instanceTypeFilterDefinition.ValidInstanceTypes.Add(typeFromName);
				instanceTypeFilterDefinition.SelectedInstanceTypes.Add(typeFromName);
			}
		}
		return instanceTypeFilterDefinition;
	}

	private void RegisterFilterFactory(string name, Func<IEnumerable<string>> domainFunction, Func<IEnumerable<string>, IEntityFilterDefinition> factoryFunction)
	{
		Factories[name] = new FilterFactory(domainFunction, factoryFunction);
	}

	public FilterBuilderViewModel CreateFilterBuilderViewModel(string name)
	{
		if (Factories.TryGetValue(name, out var value))
		{
			return value.CreateFilterBuilder(name);
		}
		return null;
	}

	public IEntityFilterDefinition CreateFilterDefinition(string name, IEnumerable<FilterBuilderViewModel> viewModels)
	{
		if (Factories.TryGetValue(name, out var value))
		{
			return value.CreateFilterDefinition(from vm in viewModels
				select vm.SelectedItem into x
				where !string.IsNullOrEmpty(x)
				select x);
		}
		return null;
	}
}
