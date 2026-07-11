using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.FireFX;

public interface IFireFXEmitter
{
	string FullName { get; }

	string Name { get; }

	EmitterFlags Flags { get; }

	BlendMode BlendMode { get; }

	IEnumerable<IMaterialClass> ValidMaterials { get; }

	IEnumerable<IGeometryClass> ValidGeometries { get; }
}
