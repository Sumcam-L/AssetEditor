using System.ComponentModel.Composition;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(HelpAboutCommand))]
[PartCreationPolicy(CreationPolicy.Shared)]
public abstract class HelpAboutCommand : ICommandClient, IInitializable
{
	[Import]
	protected ICommandService CommandService;

	protected abstract void ShowHelpAbout();

	void IInitializable.Initialize()
	{
		CommandInfo.HelpAbout.EnableCheckCanDoEvent(this);
		CommandService.RegisterCommand(CommandInfo.HelpAbout, this);
	}

	public bool CanDoCommand(object commandTag)
	{
		return StandardCommand.HelpAbout.Equals(commandTag);
	}

	public void DoCommand(object commandTag)
	{
		if (StandardCommand.HelpAbout.Equals(commandTag))
		{
			ShowHelpAbout();
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
	}
}
