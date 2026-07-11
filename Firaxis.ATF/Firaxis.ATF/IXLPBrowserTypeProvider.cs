using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IXLPBrowserTypeProvider
{
	IEnumerable<string> ValidTypes { get; }
}
