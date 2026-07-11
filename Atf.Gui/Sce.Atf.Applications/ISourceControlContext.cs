using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface ISourceControlContext
{
	IEnumerable<IResource> Resources { get; }
}
