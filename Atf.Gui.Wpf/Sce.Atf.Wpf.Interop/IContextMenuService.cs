using System.Collections.Generic;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Interop;

public interface IContextMenuService
{
	bool AutoCompact { get; set; }

	ContextMenu GetContextMenu(IEnumerable<object> commandTags);
}
