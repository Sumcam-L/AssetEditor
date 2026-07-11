using System;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels;

public class EntityReimportedEventArgs : EventArgs
{
	public string EntityName { get; private set; }

	public InstanceType EntityType { get; private set; }

	public EntityReimportedEventArgs(string name, InstanceType type)
	{
		EntityName = name;
		EntityType = type;
	}
}
