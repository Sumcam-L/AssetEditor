using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Applications.WebServices;

[Export(typeof(IInitializable))]
[Export(typeof(UserFeedbackService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class UserFeedbackService : IInitializable, ICommandClient
{
	private enum Command
	{
		HelpSendFeedback
	}

	[Import(AllowDefault = true)]
	private ICommandService m_commandService = null;

	public void ShowFeedbackForm()
	{
		FeedbackFormViewModel feedbackFormViewModel = new FeedbackFormViewModel();
		FeedbackForm feedbackForm = new FeedbackForm();
		feedbackForm.ShowDialog();
	}

	void IInitializable.Initialize()
	{
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		ProjectMappingAttribute projectMappingAttribute = (ProjectMappingAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(ProjectMappingAttribute));
		if (projectMappingAttribute != null && projectMappingAttribute.Mapping != null && projectMappingAttribute.Mapping.Trim().Length != 0 && m_commandService != null)
		{
			m_commandService.RegisterCommand(Command.HelpSendFeedback, StandardMenu.Help, StandardCommandGroup.HelpUpdate, "Send Feedback...".Localize(), "Report bug or request feature".Localize(), Keys.None, null, CommandVisibility.Menu, this);
		}
	}

	public bool CanDoCommand(object tag)
	{
		return Command.HelpSendFeedback.Equals(tag);
	}

	public void DoCommand(object tag)
	{
		if (Command.HelpSendFeedback.Equals(tag))
		{
			ShowFeedbackForm();
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
	}
}
