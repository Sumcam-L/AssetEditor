using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public class InstanceTypeFilter : IInstanceTypeFilterDefinition, IEntityFilterDefinition, IEntityFilter
{
	public string FilterName => "Instance Type";

	public ICollection<InstanceType> ValidInstanceTypes { get; private set; } = new List<InstanceType>();

	public ICollection<InstanceType> SelectedInstanceTypes { get; private set; } = new List<InstanceType>();

	public int GetRanking()
	{
		return 1;
	}

	public bool PassesFilter(EntityID entity)
	{
		return SelectedInstanceTypes.Count > 0 && SelectedInstanceTypes.Contains(entity.Type);
	}

	public IEntityFilterDefinition DeepCopy()
	{
		return DeepCopyImpl();
	}

	public IEntityFilter CreateFilter()
	{
		return DeepCopyImpl();
	}

	private InstanceTypeFilter DeepCopyImpl()
	{
		InstanceTypeFilter instanceTypeFilter = new InstanceTypeFilter();
		foreach (InstanceType selectedInstanceType in SelectedInstanceTypes)
		{
			instanceTypeFilter.SelectedInstanceTypes.Add(selectedInstanceType);
		}
		return instanceTypeFilter;
	}
}
