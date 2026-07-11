using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IProjectSpecification : IAssemblyInstance, IDisposable, ISerializable
{
	string Name { get; set; }

	string GUID { get; set; }

	string ProjectConfig { get; set; }

	string ArtDev { get; set; }

	string GamePantry { get; set; }

	string CloudStream { get; set; }

	string ArtDefRoot { get; set; }

	string ArtDefOutputRoot { get; set; }

	string LooseAssetRoot { get; set; }

	IDictionary<string, ProjectSpecificationDependency> Dependencies { get; }
}
