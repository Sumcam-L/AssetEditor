using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

public interface IVirtualPantry : IAssemblyInstance, IDisposable
{
	IEnumerable<string> PantryRoots { get; }

	void AddPantryRoot(string pantryRoot);

	string GetPrimaryPantryPath(string entityName, InstanceType entityType);

	string GetPantryPath(string entityName, InstanceType entityType);

	string GetPantryPath(string entityName, InstanceType entityType, ProjectEnvironment project);

	string GetXLPPantryPath(string relativePath);

	string GetArtDefPantryPath(string relativePath);
}
