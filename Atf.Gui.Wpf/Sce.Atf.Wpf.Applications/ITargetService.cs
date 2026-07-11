using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications;

public interface ITargetService
{
	IEnumerable<IProtocol> Protocols { get; }

	IEnumerable<ITarget> Targets { get; }

	IEnumerable<ITarget> SelectedTargets { get; }

	ITarget SelectedTarget { get; set; }

	bool? ShowTargetDialog();
}
