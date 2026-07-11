using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firaxis.AssetCloudFramework.Data;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.ATF;

[Export(typeof(IEntityQueryService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntityQueryService : IEntityQueryService
{
	private ICivTechService CivTechService { get; set; }

	private IEntityCacheService EntityCacheService { get; set; }

	[ImportingConstructor]
	public EntityQueryService(ICivTechService civTechSvc, IEntityCacheService entityCacheService)
	{
		CivTechService = civTechSvc;
		EntityCacheService = entityCacheService;
		Context.Add(this);
	}

	public IQueryService FindFilesByName(IEnumerable<string> projFilter, string nameFilter, IEnumerable<string> classFilter, IEnumerable<InstanceType> instanceTypes)
	{
		DependencyService dependencyService = new DependencyService();
		if (!instanceTypes.Any())
		{
			return dependencyService;
		}
		Regex nameMatcher = BuildNameMatcher(SanitizeFilter(nameFilter));
		Regex classMatcher = BuildClassMatcher(classFilter);
		bool bTestProject = projFilter != null && projFilter.Any() && !projFilter.Contains(CivTechService.AnyProject);
		Parallel.ForEach(EntityCacheService.GetAllEntities(), delegate(EntityID entityID)
		{
			if (instanceTypes.Contains(entityID.Type) && (nameMatcher == null || nameMatcher.Match(entityID.Name).Success))
			{
				bool flag = !bTestProject && classMatcher == null;
				if (!flag)
				{
					IEnumerable<IEntityCacheData> cacheData = EntityCacheService.GetCacheData(entityID);
					flag = true;
					if (bTestProject)
					{
						flag = cacheData.Any((IEntityCacheData entity) => projFilter.Contains(entity.Project));
					}
					if (flag && classMatcher != null)
					{
						flag = cacheData.Any((IEntityCacheData entity) => classMatcher.Match(entity.Class).Success);
					}
				}
				if (flag)
				{
					List<string> list = dependencyService.InstanceItems[entityID.Type];
					lock (list)
					{
						list.Add(entityID.Name);
					}
				}
			}
		});
		return dependencyService;
	}

	public IQueryService FindFilesByTag(IEnumerable<string> projFilter, string tagFilter, IEnumerable<string> classFilter, IEnumerable<InstanceType> instanceTypes)
	{
		DependencyService dependencyService = new DependencyService();
		if (!instanceTypes.Any())
		{
			return dependencyService;
		}
		Regex tagMatcher = BuildTagMatcher(SanitizeFilter(tagFilter));
		Regex classMatcher = BuildClassMatcher(classFilter);
		bool bTestProject = projFilter != null && projFilter.Any() && !projFilter.Contains(CivTechService.AnyProject);
		Parallel.ForEach(EntityCacheService.GetAllEntities(), delegate(EntityID entityID)
		{
			if (instanceTypes.Contains(entityID.Type))
			{
				bool flag = !bTestProject && classMatcher == null && tagMatcher == null;
				if (!flag)
				{
					IEnumerable<IEntityCacheData> cacheData = EntityCacheService.GetCacheData(entityID);
					flag = true;
					if (bTestProject)
					{
						flag = cacheData.Any((IEntityCacheData entity) => projFilter.Contains(entity.Project));
					}
					if (flag && classMatcher != null)
					{
						flag = cacheData.Any((IEntityCacheData entity) => classMatcher.Match(entity.Class).Success);
					}
					if (flag && tagMatcher != null)
					{
						flag = cacheData.Any((IEntityCacheData entity) => entity.Tags.Any((string tag) => tagMatcher.Match(tag).Success));
					}
				}
				if (flag)
				{
					List<string> list = dependencyService.InstanceItems[entityID.Type];
					lock (list)
					{
						list.Add(entityID.Name);
					}
				}
			}
		});
		return dependencyService;
	}

	private string SanitizeFilter(string inFilter)
	{
		if (string.IsNullOrEmpty(inFilter))
		{
			return inFilter;
		}
		if (inFilter.StartsWith("REGEX:", StringComparison.CurrentCultureIgnoreCase))
		{
			return inFilter.Substring("REGEX:".Length);
		}
		if (inFilter == "*")
		{
			return ".*";
		}
		return inFilter.Replace(" ", ".*");
	}

	private Regex BuildClassMatcher(IEnumerable<string> classFilter)
	{
		if (classFilter == null || !classFilter.Any())
		{
			return null;
		}
		return new Regex(string.Join("|", classFilter.Select((string filter) => "\\A" + filter + "\\z")), RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}

	private Regex BuildNameMatcher(string nameFilter)
	{
		if (string.IsNullOrEmpty(nameFilter))
		{
			return null;
		}
		return new Regex(nameFilter, RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}

	private Regex BuildTagMatcher(string tagFilter)
	{
		if (string.IsNullOrEmpty(tagFilter))
		{
			return null;
		}
		return new Regex(tagFilter, RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}
}
