using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.Utility;

namespace Firaxis.ATF;

public class ModProjectSelectionService : IProjectSelectionService
{
	private string m_activeProject;

	private IProjectInfoMap m_projects = new ProjectInfoMap();

	private object m_projectLock = new object();

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

	public ModProjectSelectionService(string gameFolder, IEnumerable<KeyValuePair<string, string>> projects)
	{
		KeyValuePair<string, string>[] array = projects.ToArray();
		KeyValuePair<string, string> keyValuePair = array[0];
		MergeEvnironmentInfo(CreateModEnvironment(gameFolder, keyValuePair.Key, keyValuePair.Value));
		for (int i = 1; i < array.Length; i++)
		{
			keyValuePair = array[i];
			MergeEvnironmentInfo(CreateLocalEnvironment(gameFolder, keyValuePair.Key, keyValuePair.Value));
		}
		m_activeProject = array[0].Key;
		_ = this.ProjectChanged;
	}

	public virtual string GetLocalEnvironmentPath()
	{
		return string.Empty;
	}

	public virtual string GetSharedEnvironmentPath()
	{
		return string.Empty;
	}

	private ProjectEnvironmentInfo CreateModEnvironment(string gameFolder, string projectName, string pantryRoot)
	{
		ProjectInfo projectInfo = new ProjectInfo();
		projectInfo.Name = projectName;
		projectInfo.ProjectType = ProjectType.eMod;
		projectInfo.Art = projectName + ".Art.xml";
		projectInfo.Config = Path.Combine(gameFolder, "Civ6.cfg");
		projectInfo.Paths.Pantry = pantryRoot;
		projectInfo.Paths.XLPRoot = Path.Combine(pantryRoot, "XLPs");
		projectInfo.Paths.ArtDefRoot = Path.Combine(pantryRoot, "ArtDefs");
		string directoryName = Path.GetDirectoryName(pantryRoot);
		projectInfo.Paths.LooseAssetRoot = Path.Combine(directoryName, "LooseAssets");
		projectInfo.Paths.ArtDev = Path.Combine(directoryName, "SourceArt");
		projectInfo.Paths.ArtDefOutputRoot = Path.Combine(directoryName, "Cooked", "ArtDefs");
		projectInfo.Paths.XLPOutputRoot = Path.Combine(directoryName, "Cooked", "BLPs");
		projectInfo.Paths.GameFolder = gameFolder;
		return new ProjectEnvironmentInfo
		{
			Projects = { [projectName] = projectInfo }
		};
	}

	private string DetermineWorkspaceRoot(string pantryPath)
	{
		string text = DetermineWorkspaceRootImpl("/Civ6", pantryPath);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		text = DetermineWorkspaceRootImpl("/DLC", pantryPath);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return Path.GetDirectoryName(pantryPath);
	}

	private string DetermineWorkspaceRootImpl(string markerPath, string pantryPath)
	{
		string pathRoot = Path.GetPathRoot(pantryPath);
		string directoryName = Path.GetDirectoryName(pantryPath);
		while (PathCompareHelper.StartsWith(directoryName, pathRoot, bIgnoreCase: true) && !PathCompareHelper.EndsWith(directoryName, markerPath, bIgnoreCase: true))
		{
			directoryName = Path.GetDirectoryName(directoryName);
		}
		if (PathCompareHelper.EndsWith(directoryName, markerPath, bIgnoreCase: true))
		{
			return Path.GetDirectoryName(directoryName);
		}
		return string.Empty;
	}

	private ProjectEnvironmentInfo CreateLocalEnvironment(string gameFolder, string projectName, string pantryRoot)
	{
		ProjectInfo projectInfo = new ProjectInfo();
		projectInfo.Name = projectName;
		projectInfo.ProjectType = ProjectType.eNormal;
		string text = projectName;
		string text2 = Directory.EnumerateFiles(pantryRoot, "*.Art.xml", SearchOption.TopDirectoryOnly).FirstOrDefault();
		if (!string.IsNullOrEmpty(text2))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(text2));
			if (!string.IsNullOrEmpty(fileNameWithoutExtension))
			{
				text = fileNameWithoutExtension;
			}
		}
		projectInfo.Art = text + ".Art.xml";
		projectInfo.Config = Path.Combine(gameFolder, "Civ6.cfg");
		projectInfo.Paths.Pantry = pantryRoot;
		projectInfo.Paths.XLPRoot = Path.Combine(pantryRoot, "XLPs");
		projectInfo.Paths.ArtDefRoot = Path.Combine(pantryRoot, "ArtDefs");
		string path = DetermineWorkspaceRoot(pantryRoot);
		projectInfo.Paths.LooseAssetRoot = Path.Combine(path, "LooseAssets");
		projectInfo.Paths.ArtDev = Path.Combine(path, "SourceArt");
		projectInfo.Paths.ArtDefOutputRoot = Path.Combine(path, "Cooked", "ArtDefs");
		projectInfo.Paths.XLPOutputRoot = Path.Combine(path, "Cooked", "BLPs");
		projectInfo.Paths.GameFolder = gameFolder;
		return new ProjectEnvironmentInfo
		{
			Projects = { [projectName] = projectInfo }
		};
	}

	private void MergeEvnironmentInfo(ProjectEnvironmentInfo projInfo)
	{
		foreach (KeyValuePair<string, ProjectInfo> project in projInfo.Projects)
		{
			m_projects.AddProject(project.Value);
		}
	}
}
