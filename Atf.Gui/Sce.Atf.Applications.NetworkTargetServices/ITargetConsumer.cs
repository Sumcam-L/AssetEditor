using System.Collections.Generic;

namespace Sce.Atf.Applications.NetworkTargetServices;

public interface ITargetConsumer
{
	IEnumerable<TargetInfo> SelectedTargets { get; set; }

	IEnumerable<TargetInfo> AllTargets { get; }

	void TargetsChanged(ITargetProvider targetProvider, IEnumerable<TargetInfo> targets);
}
