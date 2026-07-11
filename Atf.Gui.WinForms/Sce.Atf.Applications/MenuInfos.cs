using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public static class MenuInfos
{
	public static ToolStripMenuItem GetMenuItem(this MenuInfo menuInfo)
	{
		if (menuInfo.CommandService == null)
		{
			throw new NullReferenceException("MenuInfo has not been registered to a CommandService.");
		}
		CommandService commandService = (CommandService)menuInfo.CommandService;
		if (commandService == null)
		{
			throw new InvalidTransactionException("MenuInfo was registered to an ICommandService, but not specifically to Sce.Atf.Applications.CommandService.");
		}
		ToolStripMenuItem menuToolStripItem = commandService.GetMenuToolStripItem(menuInfo);
		if (menuToolStripItem == null)
		{
			throw new InvalidTransactionException("The MenuInfo specified has no ToolStripMenuItem associated with it, which should have been set up in CommandService.RegisterMenuInfo()");
		}
		return menuToolStripItem;
	}

	public static ToolStrip GetToolStrip(this MenuInfo menuInfo)
	{
		if (menuInfo.CommandService == null)
		{
			throw new NullReferenceException("MenuInfo has not been registered to a CommandService.");
		}
		CommandService commandService = (CommandService)menuInfo.CommandService;
		if (commandService == null)
		{
			throw new InvalidTransactionException("MenuInfo was registered to an ICommandService, but not specifically to Sce.Atf.Applications.CommandService.");
		}
		ToolStrip menuToolStrip = commandService.GetMenuToolStrip(menuInfo);
		if (menuToolStrip == null)
		{
			throw new InvalidTransactionException("The MenuInfo specified has no ToolStrip associated with it, which should have been set up in (or before) CommandService.RegisterMenuInfo()");
		}
		return menuToolStrip;
	}

	public static void SetToolStrip(this MenuInfo menuInfo, ToolStrip toolStrip, CommandService commandService)
	{
		if (menuInfo.CommandService != null && menuInfo.CommandService != commandService)
		{
			throw new NullReferenceException("MenuInfo has already been registered to a CommandService, and it is not the one specified.");
		}
		if (toolStrip == null)
		{
			throw new InvalidTransactionException("ToolStrip cannot be null");
		}
		if (commandService == null)
		{
			throw new InvalidTransactionException("CommandService cannot be null");
		}
		commandService.SetMenuToolStrip(menuInfo, toolStrip);
	}
}
