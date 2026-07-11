using System;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public static class CommandInfos
{
	public static ICommandItem GetCommandItem(this CommandInfo commandInfo)
	{
		if (commandInfo.CommandService == null)
		{
			throw new NullReferenceException("CommandInfo has not been registered to a CommandService.");
		}
		CommandService commandService = (CommandService)commandInfo.CommandService;
		if (commandService == null)
		{
			throw new InvalidTransactionException("CommandInfo was registered to an ICommandService, but not specifically to a CommandService.");
		}
		ICommandItem command = commandService.GetCommand(commandInfo.CommandTag);
		if (command == null)
		{
			throw new InvalidTransactionException("CommandService to which CommandInfo thinks it's registered has no record of it.");
		}
		return command;
	}
}
