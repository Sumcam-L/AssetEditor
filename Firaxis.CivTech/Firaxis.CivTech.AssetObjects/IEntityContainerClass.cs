using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IEntityContainerClass : INameProvider
{
	IEnumerable<string> GetAllowedClasses(InstanceType entityType);

	bool AllowClass(string className, InstanceType entityType);

	bool IsClassAllowed(string className, InstanceType entityType);
}
