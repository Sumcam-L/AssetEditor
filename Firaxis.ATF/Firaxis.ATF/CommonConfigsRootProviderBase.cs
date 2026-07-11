using System.IO;
using System.Reflection;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

public abstract class CommonConfigsRootProviderBase : ICommonConfigsRootProvider, IProjectRootProvider
{
	public abstract string WorkspaceRoot { get; protected set; }

	public ToolsBuildType BuildType { get; private set; }

	public string ConfigPath { get; protected set; }

	public string EnvironmentPath { get; protected set; }

	protected bool SetWorkspaceRootFromExecutablePath()
	{
		BuildType = ToolsBuildType.kLocalBuild;
		ConfigPath = string.Empty;
		EnvironmentPath = string.Empty;
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		if (entryAssembly == null)
		{
			return false;
		}
		string location = entryAssembly.Location;
		Outputs.WriteLine(OutputMessageType.Info, "Attempting to locate common configs relative to \"{0}\"", location);
		if (PathCompareHelper.StartsWith(location, "C:\\Program Files", bIgnoreCase: true))
		{
			Outputs.WriteLine(OutputMessageType.Info, "Executing installed version of tools");
			BuildType = ToolsBuildType.kLegacyInstalled;
			return false;
		}
		string directoryName = Path.GetDirectoryName(location);
		if (string.IsNullOrEmpty(directoryName))
		{
			return false;
		}
		string directoryName2 = Path.GetDirectoryName(directoryName);
		if (string.IsNullOrEmpty(directoryName2))
		{
			return false;
		}
		string text = Path.Combine(directoryName2, "Config", "Civ6.cfg");
		if (File.Exists(text))
		{
			Outputs.WriteLine(OutputMessageType.Info, "Executing autobuild version of tools");
			BuildType = ToolsBuildType.kAutobuild;
			ConfigPath = text;
			EnvironmentPath = Path.Combine(directoryName2, "Config", "AssetCloud.env");
			return true;
		}
		string directoryName3 = Path.GetDirectoryName(directoryName2);
		if (string.IsNullOrEmpty(directoryName3))
		{
			return false;
		}
		string directoryName4 = Path.GetDirectoryName(directoryName3);
		if (string.IsNullOrEmpty(directoryName4))
		{
			return false;
		}
		string directoryName5 = Path.GetDirectoryName(directoryName4);
		if (string.IsNullOrEmpty(directoryName5))
		{
			return false;
		}
		string directoryName6 = Path.GetDirectoryName(directoryName5);
		if (string.IsNullOrEmpty(directoryName6))
		{
			return false;
		}
		if (!PathCompareHelper.EndsWith(directoryName6, "game", bIgnoreCase: true))
		{
			return false;
		}
		string directoryName7 = Path.GetDirectoryName(directoryName6);
		if (string.IsNullOrEmpty(directoryName7))
		{
			return false;
		}
		WorkspaceRoot = Path.GetDirectoryName(directoryName7);
		if (string.IsNullOrEmpty(WorkspaceRoot))
		{
			return false;
		}
		Outputs.WriteLine(OutputMessageType.Info, "Executing local build of tools");
		ConfigPath = Path.Combine(WorkspaceRoot, "Civ6", "pantry", "Civ6.cfg");
		if (!File.Exists(ConfigPath))
		{
			return false;
		}
		EnvironmentPath = Path.Combine(WorkspaceRoot, "Civ6", "AssetCloud.env");
		if (!File.Exists(EnvironmentPath))
		{
			return false;
		}
		Outputs.WriteLine(OutputMessageType.Info, "Use data relative to build location");
		return true;
	}
}
