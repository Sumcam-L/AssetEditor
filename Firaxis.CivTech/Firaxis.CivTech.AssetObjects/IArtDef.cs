using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IArtDef : IAssemblyInstance, IDisposable, ISerializable, IVersionedData
{
	string ArtDefTemplate { get; set; }

	IEnumerable<IArtDefCollection> RootCollections { get; }

	IArtDefCollection AddCollection(string name);

	void RemoveCollection(string name);

	void UpdateRootCollectionsFromTemplate(IArtDefTemplate artDefTmpl);
}
