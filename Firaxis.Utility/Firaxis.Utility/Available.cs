using System;
using Firaxis.Error;
using Microsoft.Win32;

namespace Firaxis.Utility;

public static class Available
{
	public static bool Initialized { get; private set; }

	public static bool SourceControl { get; private set; }

	public static bool Enterprise { get; private set; }

	public static string ProjectName { get; private set; }

	public static bool EnableVirtualSpace { get; set; }

	static Available()
	{
		Initialized = false;
		SourceControl = false;
		Enterprise = false;
		ProjectName = null;
		EnableVirtualSpace = true;
	}

	public static void Startup()
	{
		Startup(null, disconnected: false);
	}

	public static void Startup(string projectName)
	{
		Startup(projectName, disconnected: false);
	}

	public static void Startup(string projectName, bool disconnected)
	{
		ProjectConfig projectConfig = new ProjectConfig();
		projectConfig.Init();
		Context.Add(projectConfig);
		if (!string.IsNullOrEmpty(projectName))
		{
			ChangeProject(projectName);
		}
		Initialized = true;
	}

	public static void ChangeProject(string sProject)
	{
		ProjectName = sProject;
		if (!string.IsNullOrEmpty(sProject))
		{
			ProjectConfig service = Context.GetService<ProjectConfig>();
			if (service != null)
			{
				service.CurrentProjectName = sProject;
			}
		}
	}

	private static void SaveBasePath(RegistryKey baseKey, string regKey, string keyValue, string basePath)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = baseKey.CreateSubKey(regKey);
			registryKey?.SetValue(keyValue, basePath);
		}
		catch (Exception exception)
		{
			ErrorHandling.Error(exception, ErrorLevel.Log);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	private static string GetRegistryBasePath(RegistryKey baseKey, string regKey, string keyValue)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = baseKey.OpenSubKey(regKey);
			if (registryKey != null)
			{
				return (string)registryKey.GetValue(keyValue);
			}
		}
		catch (Exception exception)
		{
			ErrorHandling.Error(exception, ErrorLevel.Log);
		}
		finally
		{
			registryKey?.Close();
		}
		return null;
	}
}
