using System;
using System.ComponentModel.Composition;
using System.Reflection;

namespace Sce.Atf.Applications.WebServices;

[Export(typeof(IInitializable))]
[Export(typeof(UserFeedbackService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class UserFeedbackService : IInitializable, ICommandClient
{
	private enum Command
	{
		HelpSendFeedback
	}

	private ICommandService m_commandService;

	private bool m_assemblyMappingFound;

	[Import(AllowDefault = true)]
	public ICommandService CommandService
	{
		get
		{
			return m_commandService;
		}
		set
		{
			m_commandService = value;
		}
	}

	public void ShowFeedbackForm()
	{
		FeedbackForm feedbackForm = new FeedbackForm(anon: false);
		feedbackForm.Text = "Send Feedback".Localize();
		feedbackForm.ShowDialog();
	}

	void IInitializable.Initialize()
	{
		Assembly element = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		ProjectMappingAttribute projectMappingAttribute = (ProjectMappingAttribute)Attribute.GetCustomAttribute(element, typeof(ProjectMappingAttribute));
		m_assemblyMappingFound = projectMappingAttribute != null && projectMappingAttribute.Mapping != null && projectMappingAttribute.Mapping.Trim().Length != 0;
		if (m_assemblyMappingFound)
		{
			m_commandService.RegisterCommand(new CommandInfo(Command.HelpSendFeedback, StandardMenu.Help, StandardCommandGroup.HelpUpdate, "Send Feedback...".Localize(), "Report bug or request feature".Localize()), this);
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
