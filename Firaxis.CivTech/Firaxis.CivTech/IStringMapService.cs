using System.Collections.Generic;

namespace Firaxis.CivTech;

public interface IStringMapService : IQueryService
{
	List<string> StringKeys { get; }
}
