using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ISplineSet
{
	IEnumerable<ISpline> Splines { get; }

	ISpline AddSpline(string className);

	void RemoveSpline(ISpline spline);
}
