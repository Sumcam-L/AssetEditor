using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IFlattenedArtDef : IArtDef, IAssemblyInstance, IDisposable, ISerializable, IVersionedData
{
	void AddArtDef(IArtDef artDef);

	void AddArtDefs(IEnumerable<IArtDef> artDefs);

	void Reset();
}
