using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using Firaxis.CivTech;

namespace Firaxis.ATF;

public class BasicProjectSelectionService : IProjectSelectionService
{
	private string m_activeProject;

	private string m_envFilePath;

	private IProjectInfoMap m_projects = new ProjectInfoMap();

	public string ActiveProject
	{
		get
		{
			return m_activeProject;
		}
		set
		{
			m_activeProject = value;
		}
	}

	public IProjectInfoMap Projects => m_projects;

	public virtual event EventHandler ProjectChanged;

	public BasicProjectSelectionService(string projectName, string envFilePath)
	{
		m_activeProject = projectName;
		m_envFilePath = envFilePath;
		LoadEnvironmentFromFile(GetSharedEnvironmentPath());
		_ = this.ProjectChanged;
	}

	public virtual string GetLocalEnvironmentPath()
	{
		return string.Empty;
	}

	public virtual string GetSharedEnvironmentPath()
	{
		return m_envFilePath;
	}

	private void LoadEnvironmentFromFile(string filePath)
	{
		if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
		{
			return;
		}
		string text = LoadJsonTextFromFile(filePath);
		if (!string.IsNullOrEmpty(text))
		{
			ProjectEnvironmentInfo projectEnvironmentInfo = LoadEnvironmentFromJsonText(text);
			if (projectEnvironmentInfo != null)
			{
				MergeEvnironmentInfo(projectEnvironmentInfo);
			}
		}
	}

	private ProjectEnvironmentInfo LoadEnvironmentFromJsonText(string jsonText)
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 20971520;
		try
		{
			return javaScriptSerializer.Deserialize<ProjectEnvironmentInfo>(jsonText);
		}
		catch (System.Exception)
		{
		}
		return null;
	}

	private string LoadJsonTextFromFile(string filePath)
	{
		try
		{
			TextReader textReader = File.OpenText(filePath);
			try
			{
				return textReader.ReadToEnd();
			}
			catch (IOException)
			{
			}
			finally
			{
				textReader.Dispose();
			}
		}
		catch (IOException)
		{
		}
		return string.Empty;
	}

	private void MergeEvnironmentInfo(ProjectEnvironmentInfo projInfo)
	{
		foreach (KeyValuePair<string, ProjectInfo> project in projInfo.Projects)
		{
			m_projects.AddProject(project.Value);
		}
	}
}
