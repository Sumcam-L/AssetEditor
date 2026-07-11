using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.Packages;

public interface IXLPClass
{
	string Name { get; set; }

	string ErrorEntityName { get; set; }

	string CookModuleName { get; set; }

	InstanceType InstanceType { get; set; }

	IEnumerable<string> AllowedEntityClasses { get; }

	void ClearAllowedEntityClasses();

	void AllowEntityClass(string name);

	void DisallowEntityClass(string name);
}
