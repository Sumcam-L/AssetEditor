using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.CookerInterface;
using Firaxis.Utility;

namespace Firaxis.AssetCloudFramework;

public static class CookHelpers
{
	public static ICookerOptions CreateCookerArgs(ICivTechService civTechSvc, bool useAbsolutePaths)
	{
		return CreateCookerArgs(civTechSvc, civTechSvc.PrimaryProject, useAbsolutePaths);
	}

	public static ICookerOptions CreateCookerArgs(ICivTechService civTechSvc, ProjectEnvironment project, bool useAbsolutePaths)
	{
		string workspaceRoot = project.VersionControl.WorkspaceRoot;
		if (string.IsNullOrEmpty(workspaceRoot))
		{
			return null;
		}
		IEnumerable<string> projectPantryPaths = civTechSvc.ProjectMapService.GetProjectPantryPaths(project);
		ICookerOptions cookerOptions = Context.EnsureCreated<CivTechContext>().CreateInstance<ICookerOptions>(new object[2] { projectPantryPaths, project.Name });
		cookerOptions.UseAbsolutePaths = useAbsolutePaths;
		string empty = string.Empty;
		string name = project.Name;
		cookerOptions.Mode = CookerMode.XLP;
		cookerOptions.PantryRoots = projectPantryPaths;
		cookerOptions.UpdateShaderDefRoot(civTechSvc);
		cookerOptions.Platform = Platforms.PLATFORM_WINDOWS;
		empty = StaticMethods.GetPlatformDirectory(cookerOptions.Platform);
		cookerOptions.PackageRoot = project.Paths.XLPOutputRoot.Replace("{PLATFORM}", empty);
		cookerOptions.UpdateConfigPath(civTechSvc);
		cookerOptions.UpdateBLPLogging(civTechSvc);
		cookerOptions.ArtDefDestinationRoot = project.Paths.ArtDefOutputRoot;
		cookerOptions.DependencyOutputRoot = Path.GetDirectoryName(project.Paths.ArtDefOutputRoot);
		return cookerOptions;
	}

	public static string GetUserCookProfilesDirectory()
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		string text = Path.Combine(folderPath, "My Games", "AssetCloud", "Cook Profiles");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	public static void UpdateBLPLogging(this ICookerOptions args, ICivTechService settings)
	{
		args.LogBLPStats = settings.PrimaryProject.Settings.EnableVerboseCookerLog;
	}

	public static void UpdateConfigPath(this ICookerOptions args, ICivTechService civTechSvc)
	{
		args.ConfigPath = civTechSvc.PrimaryProject.ActiveConfigPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	public static void UpdatePackageRoot(this ICookerOptions options)
	{
		string platformDirectory = StaticMethods.GetPlatformDirectory(options.Platform);
		if (options.PackageRoot.Contains(platformDirectory))
		{
			return;
		}
		IEnumerable<Platforms> usablePlatforms = PlatformsAssistant.GetUsablePlatforms();
		foreach (Platforms item in usablePlatforms)
		{
			if (item != Platforms.PLATFORM_ALL)
			{
				string platformDirectory2 = StaticMethods.GetPlatformDirectory(item);
				if (options.PackageRoot.Contains(platformDirectory2))
				{
					options.PackageRoot = options.PackageRoot.Replace(platformDirectory2, platformDirectory);
					break;
				}
			}
		}
	}

	public static void UpdateShaderDefRoot(this ICookerOptions args, ICivTechService settings)
	{
		string toolHostDllPath = settings.ToolHostLoader.ToolHostDllPath;
		args.ShaderDefRoot = Path.GetDirectoryName(toolHostDllPath).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	public static void UpdateSwitchableSettings(this ICookerOptions args, ICivTechService settings)
	{
		args.UpdateShaderDefRoot(settings);
		args.UpdateConfigPath(settings);
		args.UpdateBLPLogging(settings);
	}

	private static string ConvertToUniversal(this string path)
	{
		return path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	private static string ConvertToWindowsPath(this string path)
	{
		return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	private static string GetProjectName(IAssetCloudSettings settings)
	{
		return "Civ6";
	}
}
