using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ICurveParameter : IParameter
{
	IEnumerable<string> AllowedCurveClasses { get; }

	float DomainMaxValue { get; set; }

	float DomainMinValue { get; set; }

	bool AllowEmpty { get; set; }

	bool ClampDomain { get; set; }

	void AllowCurveClass(string name);

	bool IsCurveClassAllowed(string name);

	void ClearAllowedCurveClasses();
}
