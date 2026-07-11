using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IArtDefTemplateSet
{
	IEnumerable<IArtDefTemplate> Items { get; }

	T Push<T>() where T : IArtDefTemplate;

	void Clear();

	void Remove(IArtDefTemplate toRemove);

	IList<IArtDefTemplate> GetTemplates();
}
