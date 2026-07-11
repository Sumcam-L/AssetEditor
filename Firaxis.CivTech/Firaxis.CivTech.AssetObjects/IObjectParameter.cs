using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IObjectParameter : IParameter
{
	InstanceType ObjectType { get; }

	bool IsNullAllowed { get; set; }

	IEnumerable<string> AllowedClasses { get; }

	void AllowClass(string name);

	bool IsClassAllowed(string name);

	void ClearAllowedClasses();
}
