using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Applications;

[Export]
[Export(typeof(IInitializable))]
[Export(typeof(ICommandClient))]
[Export(typeof(IContextMenuCommandProvider))]
public class EditLabelCommand : IInitializable, ICommandClient, IContextMenuCommandProvider
{
	public enum Commands
	{
		EditLabel
	}

	[Import(typeof(ICommandService))]
	private ICommandService m_commandService = null;

	[Import(typeof(IContextRegistry))]
	private IContextRegistry m_contextRegistry = null;

	private static readonly CommandInfo s_renameCommandDef = new CommandInfo(Commands.EditLabel, StandardMenu.Edit, null, "Rename".Localize(), "Rename".Localize(), Keys.F2, null, CommandVisibility.None);

	public void Initialize()
	{
		m_commandService.RegisterCommand(s_renameCommandDef, this);
	}

	public bool CanDoCommand(object tag)
	{
		if (tag is Commands)
		{
			object commandTarget = m_contextRegistry.GetCommandTarget<object>();
			if (commandTarget != null)
			{
				return m_contextRegistry.GetActiveContext<ILabelEditingContext>()?.CanEditLabel(commandTarget) ?? false;
			}
		}
		return false;
	}

	public void DoCommand(object tag)
	{
		if ((Commands)tag == Commands.EditLabel)
		{
			object commandTarget = m_contextRegistry.GetCommandTarget<object>();
			if (commandTarget != null)
			{
				m_contextRegistry.GetActiveContext<ILabelEditingContext>()?.EditLabel(commandTarget);
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public IEnumerable<object> GetCommands(object context, object target)
	{
		if (target != null && context.Is<ILabelEditingContext>())
		{
			yield return Commands.EditLabel;
		}
	}
}
