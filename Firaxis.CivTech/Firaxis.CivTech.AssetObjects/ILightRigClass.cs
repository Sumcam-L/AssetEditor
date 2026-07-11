using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ILightRigClass : IAnimatableClass, INameProvider, IClassEntity, ICloudEntity, IVersionedData, IEntityContainerClass
{
	IEnumerable<string> AllowedLightClasses { get; }

	void ClearAllowedClasses();

	void AllowLightClass(string name);

	bool IsLightClassAllowed(string name);
}
