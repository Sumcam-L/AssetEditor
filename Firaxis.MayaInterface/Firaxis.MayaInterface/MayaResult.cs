using System.Collections.Generic;
using System.Linq;

namespace Firaxis.MayaInterface;

internal class MayaResult
{
	public IEnumerable<string> result { get; set; }

	public bool success { get; set; }

	public MayaResult()
	{
		result = Enumerable.Empty<string>();
		success = false;
	}
}
