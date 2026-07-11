using System.Windows.Forms;

namespace Sce.Atf.Applications;

public static class WinFormsCommandServices
{
	public static CommandInfo RegisterCommand(this ICommandService commandService, object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, string imageName, ICommandClient client)
	{
		CommandInfo commandInfo = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, KeysInterop.ToAtf(shortcut), imageName);
		commandService.RegisterCommand(commandInfo, client);
		return commandInfo;
	}

	public static CommandInfo RegisterCommand(this ICommandService commandService, object commandTag, object menuTag, object groupTag, string menuText, string description, Keys shortcut, string imageName, CommandVisibility visibility, ICommandClient client)
	{
		CommandInfo commandInfo = new CommandInfo(commandTag, menuTag, groupTag, menuText, description, KeysInterop.ToAtf(shortcut), imageName, visibility);
		commandService.RegisterCommand(commandInfo, client);
		return commandInfo;
	}

	public static bool ProcessKey(this ICommandService commandService, Keys key)
	{
		return commandService.ProcessKey(KeysInterop.ToAtf(key));
	}
}
