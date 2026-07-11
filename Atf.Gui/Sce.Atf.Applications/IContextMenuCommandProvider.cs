using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IContextMenuCommandProvider
{
	IEnumerable<object> GetCommands(object context, object target);
}
