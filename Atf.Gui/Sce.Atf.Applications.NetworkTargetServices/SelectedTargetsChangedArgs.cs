using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications.NetworkTargetServices;

public class SelectedTargetsChangedArgs : EventArgs
{
	public readonly IEnumerable<TargetInfo> OldTargets;

	public readonly IEnumerable<TargetInfo> NewTargets;

	public SelectedTargetsChangedArgs(IEnumerable<TargetInfo> oldTargets, IEnumerable<TargetInfo> newTargets)
	{
		OldTargets = oldTargets;
		NewTargets = newTargets;
	}
}
