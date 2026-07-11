using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ILightReferenceCollection
{
	IEnumerable<ILightReference> AnalyticLightReferences { get; }

	IEnumerable<ILightReference> EnvironmentLightReferences { get; }

	ILightReference AddLightReference(ILightInstance light);

	void RemoveLightReference(ILightReference r);
}
