using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(StandardFileExitCommand))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardFileExitCommand : ICommandClient, IInitializable, IPartImportsSatisfiedNotification
{
	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow;

	[Import(AllowDefault = true)]
	private Form m_mainForm;

	private readonly ICommandService m_commandService;

	[ImportingConstructor]
	public StandardFileExitCommand(ICommandService commandService)
	{
		m_commandService = commandService;
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		if (m_mainWindow == null && m_mainForm != null)
		{
			m_mainWindow = new MainFormAdapter(m_mainForm);
		}
		if (m_mainWindow == null)
		{
			throw new InvalidOperationException("Can't get main window");
		}
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.FileExit, this);
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		return StandardCommand.FileExit.Equals(commandTag);
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		if (StandardCommand.FileExit.Equals(commandTag))
		{
			m_mainWindow.Close();
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}
}
