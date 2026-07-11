using System.ComponentModel.Composition;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(StandardEditHistoryCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardEditHistoryCommands : ICommandClient, IInitializable
{
	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	[ImportingConstructor]
	public StandardEditHistoryCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.EditUndo, this);
		m_commandService.RegisterCommand(CommandInfo.EditRedo, this);
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		bool result = false;
		IHistoryContext activeContext = m_contextRegistry.GetActiveContext<IHistoryContext>();
		switch ((StandardCommand)commandTag)
		{
		case StandardCommand.EditUndo:
			result = activeContext?.CanUndo ?? false;
			break;
		case StandardCommand.EditRedo:
			result = activeContext?.CanRedo ?? false;
			break;
		}
		return result;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		IHistoryContext activeContext = m_contextRegistry.GetActiveContext<IHistoryContext>();
		switch ((StandardCommand)commandTag)
		{
		case StandardCommand.EditUndo:
			activeContext.Undo();
			break;
		case StandardCommand.EditRedo:
			activeContext.Redo();
			break;
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
		IHistoryContext activeContext = m_contextRegistry.GetActiveContext<IHistoryContext>();
		if (activeContext != null)
		{
			if (commandTag.Equals(StandardCommand.EditUndo))
			{
				commandState.Text = string.Format("Undo {0}".Localize("{0} is the name of the command"), activeContext.UndoDescription);
			}
			else if (commandTag.Equals(StandardCommand.EditRedo))
			{
				commandState.Text = string.Format("Redo {0}".Localize("{0} is the name of the command"), activeContext.RedoDescription);
			}
		}
	}
}
