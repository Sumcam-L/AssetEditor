using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IEntityFilteringService
{
	IEnumerable<string> GetAvailableFilters();

	IEnumerable<IEntityFilterDefinition> GetFilters(string filterName);

	IProjectFilterDefinition GetDefaultProjectFilter();

	void RegisterEntityFilter(Type interfaceType, Type concreteType);

	T CreateFilterDefinition<T>() where T : class, IEntityFilterDefinition;

	IEntityFilteringContext GetFilteringContext(IEnumerable<InstanceType> allowedTypes, IEnumerable<string> allowedClassNames);
}
