using Sce.Atf.Input;

namespace Sce.Atf.Applications;

public static class CommandServices
{
	public static MenuInfo RegisterMenu(this ICommandService commandService, object menuTag, string menuText, string description)
	{
		MenuInfo menuInfo = new MenuInfo(menuTag, menuText, description);
		commandService.RegisterMenu(menuInfo);
		return menuInfo;
	}

	public static CommandInfo RegisterCommand(this ICommandService commandService, object commandTag, object menuTag, object groupTag, string menuText, string description, ICommandClient client)
	{
		CommandInfo commandInfo = new CommandInfo(commandTag, menuTag, groupTag, menuText, description);
		commandService.RegisterCommand(commandInfo, client);
		return commandInfo;
	}

	public static CommandInfo RegisterCommand(this ICommandService commandService, object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, string imageName, ICommandClient client)
	{
		CommandInfo commandInfo = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, shortcut, imageName);
		commandService.RegisterCommand(commandInfo, client);
		return commandInfo;
	}

	public static CommandInfo RegisterCommand(this ICommandService commandService, object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, string imageName, CommandVisibility visibility, ICommandClient client)
	{
		CommandInfo commandInfo = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, shortcut, imageName, visibility);
		commandService.RegisterCommand(commandInfo, client);
		return commandInfo;
	}

	public static CommandInfo RegisterCommand(this ICommandService commandService, StandardCommand commandTag, CommandVisibility visibility, ICommandClient client)
	{
		CommandInfo standardCommand = CommandInfo.GetStandardCommand(commandTag);
		commandService.RegisterCommand(standardCommand, client);
		standardCommand.Visibility = visibility;
		return standardCommand;
	}

	public static CommandInfo RegisterCommand(this ICommandService commandService, object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, object imageKey, ICommandClient client)
	{
		CommandInfo commandInfo = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, shortcut, imageKey);
		commandService.RegisterCommand(commandInfo, client);
		return commandInfo;
	}

	public static CommandInfo RegisterCommand(this ICommandService commandService, object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, object imageKey, CommandVisibility visibility, ICommandClient client)
	{
		CommandInfo commandInfo = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, shortcut, imageKey, visibility);
		commandService.RegisterCommand(commandInfo, client);
		return commandInfo;
	}
}
