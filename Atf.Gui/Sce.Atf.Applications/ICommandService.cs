using System;
using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

public interface ICommandService
{
	event EventHandler<KeyEventArgs> ProcessingKey;

	event EventHandler ContextMenuClosed;

	void RegisterMenu(MenuInfo info);

	void RegisterCommand(CommandInfo info, ICommandClient client);

	void UnregisterCommand(object commandTag, ICommandClient client);

	void RunContextMenu(IEnumerable<object> commandTags, Point screenPoint);

	void SetActiveClient(ICommandClient client);

	void ReserveKey(Keys key, string reason);

	bool ProcessKey(Keys key);
}
