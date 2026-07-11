using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.CookerInterface;

public interface ICookerOptions : IAssemblyInstance, IDisposable, ISerializable
{
	string ProfileName { get; set; }

	bool IsCustomCook { get; set; }

	bool UseAbsolutePaths { get; set; }

	ICollection<string> XLPs { get; }

	ICollection<string> ArtDefs { get; }

	ICollection<string> ArtSpecificationFiles { get; }

	IEnumerable<string> FilesToCook { get; }

	int NumCores { get; set; }

	Platforms Platform { get; set; }

	CookerMode Mode { get; set; }

	bool AllXLPs { get; set; }

	bool AllArtDefs { get; set; }

	string PackageRoot { get; set; }

	IEnumerable<string> PantryRoots { get; set; }

	string ConfigPath { get; set; }

	string ShaderDefRoot { get; set; }

	string ArtDefDestinationRoot { get; set; }

	string Layout { get; set; }

	string DependencyOutputRoot { get; set; }

	uint TempHeapSize { get; set; }

	bool LogBLPStats { get; set; }

	bool MultithreadingEnabled { get; set; }

	bool AppendLogging { get; set; }
}
