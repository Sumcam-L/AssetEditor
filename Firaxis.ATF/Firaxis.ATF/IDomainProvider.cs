using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IDomainProvider<T>
{
	IEnumerable<T> PossibleValues { get; }
}
