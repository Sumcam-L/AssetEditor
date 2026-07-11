using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IHotloadable
{
	IEnumerable<string> ConsumerNames { get; }

	string SubSystem { get; }

	Uri Uri { get; }
}
