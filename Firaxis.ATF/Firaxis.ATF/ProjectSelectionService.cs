using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.Utility;
using Microsoft.Win32;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(IProjectSelectionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ProjectSelectionService : MarshalByRefObject, IProjectSelectionService, IInitializable
{
	[Flags]
	private enum AssetCloudEnvironmentState
	{
		NoEnvironmentPath = 1,
		SharedEnvironmentPath = 2,
		LocalEnvironmentPath = 4
	}

	public static object SelectProjectTag = "SelectProjectTag";

	private string m_activeProject;

	private IAssetCloudSettingService m_assetCloudSettingService;

	private IProjectInfoMap m_projectInfoMap = new ProjectInfoMap();

	private object m_projectLock = new object();

	[Import(AllowDefault = true)]
	private ISingleInstanceService m_singleInstanceService;

	private AssetCloudEnvironmentState m_environmentState = AssetCloudEnvironmentState.NoEnvironmentPath;

	private string m_localEnvironmentPath;

	private string m_sharedEnvironmentPath;

	public string ActiveProject
	{
		get
		{
			return m_activeProject;
		}
		set
		{
			if (value == m_activeProject)
			{
				return;
			}
			m_activeProject = value;
			Form form = Application.OpenForms.OfType<Form>().FirstOrDefault((Form fod) => fod.Visible);
			if (form != null && form.InvokeRequired)
			{
				form.Invoke((Action)delegate
				{
					this.ProjectChanged.Raise(this, null);
					SetActiveProjectRegistrySetting(value);
				});
			}
			else
			{
				this.ProjectChanged.Raise(this, null);
				SetActiveProjectRegistrySetting(value);
			}
		}
	}

	public IProjectInfoMap Projects => m_projectInfoMap;

	public event EventHandler ProjectChanged;

	[ImportingConstructor]
	public ProjectSelectionService(IAssetCloudSettingService assCldSetSvc, ICommonConfigsRootProvider ivcss)
	{
		DoProjectSelectionServiceConstruct(assCldSetSvc, ivcss, DetermineLocalEnvironmentPath, DetermineSharedEnvironmentPath);
	}

	public ProjectSelectionService(IAssetCloudSettingService assCldSetSvc, string overrideLocalPath, string overrideSharedPath)
	{
		DoProjectSelectionServiceConstruct(assCldSetSvc, null, () => overrideLocalPath, () => overrideSharedPath);
	}

	private bool SetupAndValidateSharedEnvironmentPaths(string sharedEnvPath)
	{
		m_sharedEnvironmentPath = sharedEnvPath;
		int num;
		if (!string.IsNullOrEmpty(m_sharedEnvironmentPath))
		{
			num = (File.Exists(m_sharedEnvironmentPath) ? 1 : 0);
			if (num != 0)
			{
				m_environmentState |= AssetCloudEnvironmentState.SharedEnvironmentPath;
				return (byte)num != 0;
			}
		}
		else
		{
			num = 0;
		}
		Outputs.WriteLine(OutputMessageType.Warning, "Shared environment file not found " + sharedEnvPath);
		return (byte)num != 0;
	}

	private bool SetupAndValidateLocalEnvironmentPath(string localEnvPath)
	{
		m_localEnvironmentPath = localEnvPath;
		int num;
		if (!string.IsNullOrEmpty(m_localEnvironmentPath))
		{
			num = (File.Exists(m_localEnvironmentPath) ? 1 : 0);
			if (num != 0)
			{
				m_environmentState |= AssetCloudEnvironmentState.LocalEnvironmentPath;
				return (byte)num != 0;
			}
		}
		else
		{
			num = 0;
		}
		Outputs.WriteLine(OutputMessageType.Warning, "Local environment file not found " + localEnvPath);
		return (byte)num != 0;
	}

	private void DoProjectSelectionServiceConstruct(IAssetCloudSettingService assCldSetSvc, ICommonConfigsRootProvider ivcss, Func<string> localEnvProvider, Func<string> sharedEnvProvider)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			SetupActiveProject(assCldSetSvc);
			if (SetupAndValidateSharedEnvironmentPaths(sharedEnvProvider()))
			{
				LoadEnvironmentFromFile(GetSharedEnvironmentPath());
			}
			Uri uri = new Uri((ivcss == null) ? localEnvProvider() : ((string.IsNullOrEmpty(ivcss.EnvironmentPath) || !File.Exists(ivcss.EnvironmentPath)) ? Path.Combine(ivcss.WorkspaceRoot, "Civ6", "AssetCloud.env") : ivcss.EnvironmentPath));
			if (SetupAndValidateLocalEnvironmentPath(uri.AbsolutePath))
			{
				LoadEnvironmentFromFile(GetLocalEnvironmentPath());
			}
			if (m_environmentState == AssetCloudEnvironmentState.NoEnvironmentPath)
			{
				throw new ProjectConfigEnvironmentException(m_activeProject, m_localEnvironmentPath, m_sharedEnvironmentPath);
			}
		}
	}

	private string DetermineLocalEnvironmentPath()
	{
		if (Projects.ContainsProject("Civ6"))
		{
			return Path.Combine(Projects["Civ6"].Paths.Pantry, "..", "AssetCloud.env");
		}
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "AssetCloud", "AssetCloud.env");
	}

	private string DetermineSharedEnvironmentPath()
	{
		return (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Firaxis\\Tools\\AssetCloudCiv6", "EnvironmentInfo", string.Empty);
	}

	public void Initialize()
	{
		if (m_singleInstanceService != null)
		{
			RemotingServices.Marshal(this, "IProjectSelectionService");
		}
	}

	public string GetLocalEnvironmentPath()
	{
		return m_localEnvironmentPath;
	}

	public string GetSharedEnvironmentPath()
	{
		return m_sharedEnvironmentPath;
	}

	public override object InitializeLifetimeService()
	{
		ILease lease = (ILease)base.InitializeLifetimeService();
		if (lease.CurrentState == LeaseState.Initial)
		{
			lease.InitialLeaseTime = TimeSpan.Zero;
		}
		return lease;
	}

	private void SetupActiveProject(IAssetCloudSettingService assCldSetSvc)
	{
		m_assetCloudSettingService = assCldSetSvc;
		m_activeProject = GetActiveProjectRegistrySetting();
	}

	private string GetActiveProjectRegistrySetting()
	{
		RegistryKey toolsRegistryKey = m_assetCloudSettingService.AssetCloudSettings.GetToolsRegistryKey("Civ6", "");
		if (toolsRegistryKey == null)
		{
			return "Civ6";
		}
		string text = toolsRegistryKey.GetValue("ActiveProject") as string;
		if (string.IsNullOrEmpty(text))
		{
			return "Civ6";
		}
		return text;
	}

	private void LoadEnvironmentFromFile(string filePath)
	{
		if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
		{
			string text = LoadJsonTextFromFile(filePath);
			if (string.IsNullOrEmpty(text))
			{
				throw new ProjectConfigEnvironmentLoadException(m_activeProject, filePath);
			}
			ProjectEnvironmentInfo projectEnvironmentInfo = LoadEnvironmentFromJsonText(text);
			if (projectEnvironmentInfo != null)
			{
				Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Normal, "Merging project environment from \"{0}\"", filePath);
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
			m_projectInfoMap.AddProject(project.Value);
		}
	}

	private void SetActiveProjectRegistrySetting(string actProj)
	{
		m_assetCloudSettingService.AssetCloudSettings.GetToolsRegistryKey("Civ6", "")?.SetValue("ActiveProject", actProj);
	}
}
