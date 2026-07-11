using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IAnimatableClass : INameProvider
{
	IEnumerable<string> AllowedAnimationClasses { get; }

	IEnumerable<string> AllowedDSGClasses { get; }

	IEnumerable<string> AllowedTriggerClasses { get; }

	void AllowDSGClass(string name);

	void AllowAnimationClass(string name);

	void AllowTriggerClass(string name);

	bool IsAnimationClassAllowed(string name);

	bool IsDSGClassAllowed(string name);

	bool IsTriggerClassAllowed(string name);
}
