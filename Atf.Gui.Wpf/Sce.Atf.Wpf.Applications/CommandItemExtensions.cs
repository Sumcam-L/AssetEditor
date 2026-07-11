using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public static class CommandItemExtensions
{
	public static bool IsVisible(this ICommandItem cmd, CommandVisibility visibility)
	{
		return (cmd.Visibility & visibility) > CommandVisibility.None;
	}
}
