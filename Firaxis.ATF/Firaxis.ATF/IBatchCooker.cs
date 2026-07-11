using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IBatchCooker
{
	CookResult Cook(IEnumerable<Uri> urisToCook);

	IHotLoadData GetHotLoadData();
}
