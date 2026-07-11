using System.Collections.Generic;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;

namespace Firaxis.AssetCloudFramework.Data;

public class SourceControlServiceEntityResult : IQueryService, IValidateService, IServiceNameProvider
{
	public string Description { get; set; }

	public SerializableDictionary<InstanceType, List<string>> InstanceItems { get; set; }

	public SourceControlServiceEntityResult()
	{
		Description = string.Empty;
		InstanceItems = new SerializableDictionary<InstanceType, List<string>>();
		for (int i = 0; i < 13; i++)
		{
			InstanceItems.Add((InstanceType)i, new List<string>());
		}
	}

	public bool Validate()
	{
		if (Description != null)
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
		return false;
	}

	public string GetServiceName()
	{
		return "SourceControlService";
	}
}
