using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Firaxis.Utility;

namespace Firaxis.CivTech;

[Export(typeof(IToolHostLoaderService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ToolHostLoaderService : IToolHostLoaderService
{
	private IAssetCloudSettingService m_assetCloudSettingService;

	private ILogFileProvider m_logFileProvider;

	public virtual string ToolHostDllPath { get; private set; }

	public virtual IToolHostInterface ToolHostInterface { get; private set; }

	public virtual event EventHandler<ToolHostEventArgs> Loaded;

	public virtual event EventHandler<ToolHostEventArgs> Unloaded;

	[ImportingConstructor]
	public ToolHostLoaderService(IAssetCloudSettingService acss, ILogFileProvider logProvider)
	{
		m_assetCloudSettingService = acss;
		m_logFileProvider = logProvider;
		ToolHostDllPath = DetermineToolHostPath();
	}

	public virtual bool LoadToolHost()
	{
		ToolHostInterface = Context.EnsureCreated<CivTechContext>().CreateInstance<IToolHostInterface>(new object[2]
		{
			ToolHostDllPath,
			Path.GetDirectoryName(m_logFileProvider.LogFilePath)
		});
		if (ToolHostInterface != null && ToolHostInterface.IsLoaded)
		{
			RaiseEvent(this.Loaded, new ToolHostEventArgs(ToolHostInterface.DllPath));
			return true;
		}
		return false;
	}

	public virtual void UnloadToolHost()
	{
		if (ToolHostInterface != null)
		{
			RaiseEvent(this.Unloaded, new ToolHostEventArgs(ToolHostInterface.DllPath));
			ToolHostInterface.Dispose();
			ToolHostInterface = null;
		}
	}

	private void RaiseEvent(EventHandler<ToolHostEventArgs> evt, ToolHostEventArgs args)
	{
		evt?.Invoke(this, args);
	}

	private string GetConfigurationName()
	{
		return "Release";
	}

	private string GetGraphicsPlatformName()
	{
		return "DX11";
	}

	private string GetConfigurationToolHostPath()
	{
		string codeBase = Assembly.GetExecutingAssembly().CodeBase;
		Uri uri = new Uri(codeBase);
		string toolhostName = $"Civ6ToolHost_Win64_{GetGraphicsPlatformName()}_{GetConfigurationName()}";
		string directoryName = Path.GetDirectoryName(uri.LocalPath);
		return Directory.EnumerateFiles(directoryName, "*.dll").FirstOrDefault(delegate(string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			return fileName.StartsWith(toolhostName, StringComparison.InvariantCultureIgnoreCase);
		});
	}

	private string DetermineToolHostPath()
	{
		string configurationToolHostPath = GetConfigurationToolHostPath();
		if (Debugger.IsAttached && !string.IsNullOrEmpty(configurationToolHostPath))
		{
			return configurationToolHostPath;
		}
		if (m_assetCloudSettingService.AssetCloudSettings.UseLocalToolHost)
		{
			string toolHostLocal = m_assetCloudSettingService.AssetCloudSettings.ToolHostLocal;
			if (File.Exists(toolHostLocal))
			{
				return toolHostLocal;
			}
		}
		if (!string.IsNullOrEmpty(configurationToolHostPath))
		{
			return configurationToolHostPath;
		}
		// Fallback: read ToolsPath from ModBuddy registry
		string sdkRoot = ReadModBuddyToolsPath();
		if (sdkRoot != null)
		{
			string dir = Path.Combine(sdkRoot, "AssetModTools", "AssetEditor");
			string toolhostName = $"Civ6ToolHost_Win64_{GetGraphicsPlatformName()}_{GetConfigurationName()}";
			if (Directory.Exists(dir))
			{
				string found = Directory.EnumerateFiles(dir, "*.dll").FirstOrDefault(f =>
					Path.GetFileName(f).StartsWith(toolhostName, StringComparison.InvariantCultureIgnoreCase));
				if (found != null) return found;
			}
		}
		return m_assetCloudSettingService.AssetCloudSettings.ToolHost;
	}

	private static string ReadModBuddyToolsPath()
	{
		try
		{
			using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
				@"Software\Firaxis\Civilization6_ModBuddy\2013\DialogPage\Firaxis.VisualStudio.Projects.Civ6.OptionsPages.OptionsDialogPage");
			return key?.GetValue("ToolsPath") as string;
		}
		catch
		{
			return null;
		}
	}
}
