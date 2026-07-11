using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications;

public interface ITargetDiscovery
{
	string Id { get; }

	string ProtocolName { get; }

	IEnumerable<ITarget> Targets { get; }
}
