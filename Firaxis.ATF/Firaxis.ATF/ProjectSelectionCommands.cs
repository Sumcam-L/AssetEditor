using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.Properties;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IProjectSelectionCommands))]
[Export(typeof(ProjectSelectionCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ProjectSelectionCommands : ICommandClient, IInitializable, IProjectSelectionCommands
{
	private readonly string[] m_applications = new string[4] { "AssetEditor", "AssetCloud", "ProjectBuilder", "StateGraphCreator" };

	private readonly ICommandService m_commandService;

	private readonly IProjectMapService m_projectMapService;

	private readonly IProjectSelectionService m_projectSelectionService;

	[ImportingConstructor]
	public ProjectSelectionCommands(IProjectMapService pms, IProjectSelectionService pss, ICommandService commandService)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			m_projectMapService = pms;
			m_projectSelectionService = pss;
			m_commandService = commandService;
		}
	}

	public bool CanDoCommand(object commandTag)
	{
		return true;
	}

	public void DoCommand(object commandTag)
	{
		if (commandTag is ProjectInfo projectInfo && !(projectInfo.Name == m_projectSelectionService.ActiveProject))
		{
			m_projectSelectionService.ActiveProject = projectInfo.Name;
			string[] applications = m_applications;
			foreach (string appName in applications)
			{
				ChangeProject(appName, projectInfo.Name);
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
		if (commandTag is ProjectInfo projectInfo)
		{
			bool flag = projectInfo.Name == m_projectSelectionService.ActiveProject;
			if (flag != commandState.Check)
			{
				commandState.Check = flag;
			}
		}
	}

	public void Initialize()
	{
		AddAllCommands();
	}

	public void HandleProjectChange()
	{
		UpdateProjectMenus();
	}

	private void AddAllCommands()
	{
		foreach (ProjectInfo item in m_projectSelectionService.Projects.ProjectInfos.OrderBy((ProjectInfo pi) => pi.Name))
		{
			CommandInfo info = new CommandInfo(item, ProjectSelectionService.SelectProjectTag, ProjectSelectionService.SelectProjectTag, item.Name, "Change to the " + item.Name + " project");
			m_commandService.RegisterCommand(info, this);
		}
	}

	private void ChangeProject(string appName, string prjName)
	{
		if (appName == Application.ProductName)
		{
			return;
		}
		string text = appName + "-" + Application.ProductVersion + "-" + Environment.UserName;
		if (text.Length > 250)
		{
			text = text.Substring(0, 250);
		}
		text = text.Replace('/', '-');
		text = text.Replace('\\', '-');
		string url = "ipc://" + text + "/IProjectSelectionService";
		try
		{
			IProjectSelectionService pss = (IProjectSelectionService)RemotingServices.Connect(typeof(IProjectSelectionService), url);
			Task.Factory.StartNew(() => pss.ActiveProject = prjName);
		}
		catch (RemotingException ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Failed to change project for application {0} because of:\n{1}", appName, ex.Message);
		}
	}

	private void RemoveAllCommands()
	{
		foreach (ProjectInfo projectInfo in m_projectSelectionService.Projects.ProjectInfos)
		{
			m_commandService.UnregisterCommand(projectInfo, this);
		}
	}

	private void UpdateProjectMenus()
	{
		RemoveAllCommands();
		AddAllCommands();
	}
}
