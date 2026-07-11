using System.Collections.Generic;

namespace Sce.Atf.Applications.NetworkTargetServices;

public interface ITargetProvider
{
	string Name { get; }

	bool CanCreateNew { get; }

	IEnumerable<TargetInfo> GetTargets(ITargetConsumer targetConsumer);

	TargetInfo CreateNew();

	bool AddTarget(TargetInfo target);

	bool Remove(TargetInfo target);
}
