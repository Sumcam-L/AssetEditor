using System.Collections.Generic;
using System.Linq;

namespace Firaxis.CivTech.AssetObjects;

public static class IEntityFilteringContextExtensions
{
	public static T GetOrCreateFilterDefinition<T>(this IEntityFilteringContext filteringContext, IEntityFilteringService entityFilterService) where T : class, IEntityFilterDefinition
	{
		T val = filteringContext.FilterDefinitions.OfType<T>().FirstOrDefault();
		if (val == null)
		{
			val = entityFilterService.CreateFilterDefinition<T>();
			if (val != null)
			{
				filteringContext.FilterDefinitions.Add(val);
			}
		}
		return val;
	}

	public static T GetFilterDefinition<T>(this IEntityFilteringContext filteringContext) where T : class, IEntityFilterDefinition
	{
		return filteringContext.FilterDefinitions.OfType<T>().FirstOrDefault();
	}

	public static void RemoveFilterDefinition<T>(this IEntityFilteringContext filteringContext) where T : class, IEntityFilterDefinition
	{
		IEnumerable<T> enumerable = filteringContext.FilterDefinitions.OfType<T>().ToArray();
		foreach (T item in enumerable)
		{
			filteringContext.FilterDefinitions.Remove(item);
		}
	}
}
