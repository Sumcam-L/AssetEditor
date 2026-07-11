using System.Collections.Generic;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;

namespace Firaxis.AssetCloudFramework.Data;

public class DependencyService : IQueryService, IValidateService, IServiceNameProvider
{
	public SerializableDictionary<InstanceType, List<string>> InstanceItems { get; set; }

	public DependencyService()
	{
		InstanceItems = new SerializableDictionary<InstanceType, List<string>>();
		for (int i = 0; i < 13; i++)
		{
			InstanceItems.Add((InstanceType)i, new List<string>());
		}
	}

	public bool Validate()
	{
		for (int i = 0; i < 13; i++)
		{
			if (InstanceItems[(InstanceType)i].Contains(null))
			{
				return false;
			}
		}
		return true;
	}

	public string GetServiceName()
	{
		return GetType().Name;
	}
}
