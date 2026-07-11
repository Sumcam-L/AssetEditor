using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface ITreeView
{
	object Root { get; }

	IEnumerable<object> GetChildren(object parent);
}
