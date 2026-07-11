using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IGeometrySet
{
	uint ModelInstanceCount { get; }

	IEnumerable<IModelInstance> ModelInstances { get; }

	IModelInstance FindModelInstance(string name);

	IModelInstance AddModelInstance(string name, IGeometryInstance geo);

	void RemoveModelInstance(string name);
}
