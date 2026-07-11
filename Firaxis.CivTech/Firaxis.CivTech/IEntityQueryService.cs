using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

public interface IEntityQueryService
{
	IQueryService FindFilesByTag(IEnumerable<string> projFilter, string tagFilter, IEnumerable<string> classFilter, IEnumerable<InstanceType> instanceTypes);

	IQueryService FindFilesByName(IEnumerable<string> projFilter, string nameFilter, IEnumerable<string> classFilter, IEnumerable<InstanceType> instanceTypes);
}
