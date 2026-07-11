using System;
using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

public static class CommandInfos
{
	public static ToolStripMenuItem GetMenuItem(this CommandInfo commandInfo)
	{
		return commandInfo.GetCommandControls().MenuItem;
	}

	public static ToolStripButton GetButton(this CommandInfo commandInfo)
	{
		return commandInfo.GetCommandControls().Button;
	}

	public static void GetMenuItemAndButton(this CommandInfo commandInfo, out ToolStripMenuItem menuItem, out ToolStripButton button)
	{
		CommandService.CommandControls commandControls = commandInfo.GetCommandControls();
		menuItem = commandControls.MenuItem;
		button = commandControls.Button;
	}

	private static CommandService.CommandControls GetCommandControls(this CommandInfo commandInfo)
	{
		if (commandInfo.CommandService == null)
		{
			throw new InvalidOperationException("CommandInfo has not been registered to a CommandService.");
		}
		CommandService commandService = (CommandService)commandInfo.CommandService;
		if (commandService == null)
		{
			throw new InvalidOperationException("CommandInfo was registered to an ICommandService, but not specifically to a WinFormsCommandService.");
		}
		CommandService.CommandControls commandControls = commandService.GetCommandControls(commandInfo);
		if (commandControls == null)
		{
			throw new InvalidOperationException("WinForms CommandService to which CommandInfo thinks it's registered has no record of it.");
		}
		return commandControls;
	}

	public static void RebuildShortcutDisplayString(this CommandInfo commandInfo)
	{
		ToolStripMenuItem menuItem = commandInfo.GetMenuItem();
		if (menuItem == null)
		{
			return;
		}
		string text = string.Empty;
		foreach (Sce.Atf.Input.Keys shortcut in commandInfo.Shortcuts)
		{
			if (shortcut != Sce.Atf.Input.Keys.None)
			{
				if (text != string.Empty)
				{
					text += " ; ";
				}
				text += KeysUtil.KeysToString(shortcut, digitOnly: true);
			}
		}
		menuItem.ShortcutKeyDisplayString = text;
	}
}
