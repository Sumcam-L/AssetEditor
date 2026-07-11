using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Interop;

[Export(typeof(IContextMenuService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ContextMenuService : IContextMenuService
{
	[Import]
	private CommandService m_commandService = null;

	public bool AutoCompact { get; set; }

	public ContextMenuService()
	{
		AutoCompact = true;
	}

	public ContextMenu GetContextMenu(IEnumerable<object> commandTags)
	{
		CommandManager.InvalidateRequerySuggested();
		ContextMenu contextMenu = new ContextMenu();
		contextMenu.SetResourceReference(FrameworkElement.StyleProperty, Resources.ContextMenuStyleKey);
		List<ICommandItem> list = new List<ICommandItem>();
		foreach (object commandTag in commandTags)
		{
			ICommandItem command = m_commandService.GetCommand(commandTag);
			if (command != null)
			{
				if (!AutoCompact || command.CanExecute(null))
				{
					list.Add(command);
				}
			}
			else if (commandTag is ICommandItem commandItem && (!AutoCompact || commandItem.CanExecute(null)))
			{
				list.Add(commandItem);
			}
		}
		list.Sort(new CommandComparer());
		RootMenuModel rootMenuModel = new RootMenuModel(null, null, null);
		foreach (ICommandItem item in list)
		{
			rootMenuModel.AddItem(item);
		}
		contextMenu.ItemsSource = rootMenuModel.Children;
		return contextMenu;
	}
}
