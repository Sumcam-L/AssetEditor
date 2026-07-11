using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Firaxis.CivTech.AssetObjects;

public class ProjectFilter : IProjectFilterDefinition, IEntityFilterDefinition, IEntityFilter, ICivTechServiceConsumer, IEntityCacheServiceConsumer
{
	public string FilterName => "Project";

	public ICollection<string> ValidProjectNames { get; private set; }

	public IEntityCacheService EntityCacheService { get; set; }

	public ICivTechService CivTechService { get; set; }

	private bool TriviallyPasses { get; set; } = false;

	public ProjectFilter()
	{
		((ObservableCollection<string>)(ValidProjectNames = new ObservableCollection<string>())).CollectionChanged += HandleProjectNamesChanged;
	}

	private void HandleProjectNamesChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		TriviallyPasses = ValidProjectNames.Count == 0 || ValidProjectNames.Contains(CivTechService.AnyProject);
	}

	public int GetRanking()
	{
		return (!TriviallyPasses) ? 5 : 0;
	}

	public bool PassesFilter(EntityID entityID)
	{
		if (TriviallyPasses)
		{
			return true;
		}
		IEnumerable<IEntityCacheData> cacheData = EntityCacheService.GetCacheData(entityID);
		return cacheData.Any((IEntityCacheData entity) => ValidProjectNames.Contains(entity.Project));
	}

	public IEntityFilterDefinition DeepCopy()
	{
		return DeepCopyImpl();
	}

	public IEntityFilter CreateFilter()
	{
		return DeepCopyImpl();
	}

	private ProjectFilter DeepCopyImpl()
	{
		ProjectFilter projectFilter = new ProjectFilter();
		projectFilter.EntityCacheService = EntityCacheService;
		projectFilter.CivTechService = CivTechService;
		foreach (string validProjectName in ValidProjectNames)
		{
			projectFilter.ValidProjectNames.Add(validProjectName);
		}
		return projectFilter;
	}
}
