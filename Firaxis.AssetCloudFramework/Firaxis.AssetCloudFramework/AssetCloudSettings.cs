using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using Firaxis.AssetCloudFramework.Properties;
using Firaxis.CivTech;
using Firaxis.Controls;
using Firaxis.Reflection;
using Microsoft.Win32;
using Sce.Atf.Applications;

namespace Firaxis.AssetCloudFramework;

public class AssetCloudSettings : IAssetCloudSettings, IAssetCloudSettingValidation
{
	private string _toolHost;

	private string _assetCooker;

	private bool _showAssertions;

	private string _projectConfigLocal;

	private string _activeConfigPath;

	private string _toolHostLocal;

	private string _toolHostPlayer = GetDefaultToolHostPlayerPath();

	private bool _useLocalToolHost = false;

	private bool _useLocalConfig = false;

	private bool _enableNotifications;

	private bool _resetLayout;

	private float _notificationDuration;

	private bool _validatePantry = true;

	private bool _syncBeforeCooking = false;

	private bool _useCookOutPutDirectory = false;

	private string _customCookDirectory = null;

	private bool _cookTestProjects = false;

	private bool _enableVerboseCookerLog = false;

	private bool _throttleGameFramerate = true;

	private bool _RecordPreviewerTraces = false;

	[ReadOnly(true)]
	[Category("Project Config")]
	[DisplayName("Tool Host")]
	[Description("The location of the tool host dll.")]
	[Filter("Tool Host (*.dll)|*.dll")]
	public string ToolHost
	{
		get
		{
			return _toolHost;
		}
		set
		{
			_toolHost = value;
			RaiseSettingsChanged();
		}
	}

	[ReadOnly(true)]
	[Category("Project Config")]
	[DisplayName("Asset Cooker")]
	[Description("The location of the Asset Cooker.")]
	[Filter("Asset Cooker (*.exe)|*.exe")]
	public string AssetCooker
	{
		get
		{
			return _assetCooker;
		}
		set
		{
			_assetCooker = value;
			RaiseSettingsChanged();
		}
	}

	[Category("Local Overrides")]
	[DisplayName("Enable Assertions")]
	[Description("Should assertions in the ToolHost DLL present the user with a dialog box?")]
	public bool ShowAssertions
	{
		get
		{
			return _showAssertions;
		}
		set
		{
			_showAssertions = value;
			RaiseSettingsChanged();
		}
	}

	[Category("Local Overrides")]
	[DisplayName("Project Config File")]
	[Description("The location of the local project configuration file.")]
	[Editor(typeof(OpenFileEditor), typeof(UITypeEditor))]
	[Filter("Project Config (*.cfg)|*.cfg")]
	public string ProjectConfigLocal
	{
		get
		{
			return _projectConfigLocal;
		}
		set
		{
			_projectConfigLocal = value;
			RaiseSettingsChanged();
		}
	}

	public string ActiveConfigPath
	{
		get
		{
			if (_activeConfigPath == null)
			{
				return Path.Combine("Civ6", "game", "Base", "Binaries", "Win64", "Civ6.cfg");
			}
			return _activeConfigPath;
		}
		set
		{
			_activeConfigPath = value;
			RaiseSettingsChanged();
		}
	}

	[Category("Local Overrides")]
	[DisplayName("Tool Host")]
	[Description("The location of the built/local tool host dll.")]
	[Editor(typeof(OpenFileEditor), typeof(UITypeEditor))]
	[Filter("Tool Host (*.dll)|*.dll")]
	public string ToolHostLocal
	{
		get
		{
			return _toolHostLocal;
		}
		set
		{
			_toolHostLocal = value;
			RaiseSettingsChanged();
		}
	}

	[Category("Local Overrides")]
	[DisplayName("ToolHost Player")]
	[Description("The location of the built/local tool host trace player.")]
	[Editor(typeof(OpenFileEditor), typeof(UITypeEditor))]
	[Filter("Tool Host Player (*.exe)|*.exe")]
	public string ToolHostPlayer
	{
		get
		{
			return _toolHostPlayer;
		}
		set
		{
			_toolHostPlayer = value;
			RaiseSettingsChanged();
		}
	}

	[Category("User Preferences")]
	[DisplayName("Use Local ToolHost")]
	[Description("Set to true to use a local ToolHost.")]
	public bool UseLocalToolHost
	{
		get
		{
			return _useLocalToolHost;
		}
		set
		{
			_useLocalToolHost = value;
			RaiseSettingsChanged();
		}
	}

	[Category("User Preferences")]
	[DisplayName("Use Local Project Config")]
	[Description("Set to true to use a local Project Config.")]
	public bool UseLocalConfig
	{
		get
		{
			return _useLocalConfig;
		}
		set
		{
			_useLocalConfig = value;
			RaiseSettingsChanged();
		}
	}

	[Category("User Preferences")]
	[DisplayName("Enable Notifications")]
	[Description("Set to true to see pop-up notifications.")]
	public bool EnableNotifications
	{
		get
		{
			return _enableNotifications;
		}
		set
		{
			_enableNotifications = value;
			RaiseSettingsChanged();
		}
	}

	[Category("User Preferences")]
	[DisplayName("Reset Layout")]
	[Description("Return layout to its default state.")]
	public bool ResetLayout
	{
		get
		{
			return _resetLayout;
		}
		set
		{
			_resetLayout = value;
			RaiseSettingsChanged();
		}
	}

	[Category("User Preferences")]
	[DisplayName("Notification Duration")]
	[Description("Duration of popup notifications in seconds.")]
	public float NotificationDuration
	{
		get
		{
			return _notificationDuration;
		}
		set
		{
			_notificationDuration = value;
			RaiseSettingsChanged();
		}
	}

	[Category("User Preferences")]
	[DisplayName("Validate Pantry on Sync")]
	[Description("When this is set to true, whenever you sync, you will be prompted to handle any uncontrolled local files.")]
	public bool ValidatePantry
	{
		get
		{
			return _validatePantry;
		}
		set
		{
			_validatePantry = value;
			RaiseSettingsChanged();
		}
	}

	[Category("User Preferences")]
	[DisplayName("Sync Before Cooking")]
	[Description("When this is set to true, the Asset Cloud will perform a sync of the pantries that it is about to cook prior to cooking.")]
	public bool SyncBeforeCooking
	{
		get
		{
			return _syncBeforeCooking;
		}
		set
		{
			if (_syncBeforeCooking != value)
			{
				_syncBeforeCooking = value;
				RaiseSettingsChanged();
			}
		}
	}

	[Category("User Preferences")]
	[DisplayName("Set Cook Output Directory")]
	[Description("When this is set to true, the Asset Cloud will use a cook directory specified by the user.")]
	public bool UseCustomCookDirectory
	{
		get
		{
			return _useCookOutPutDirectory;
		}
		set
		{
			if (_useCookOutPutDirectory != value)
			{
				_useCookOutPutDirectory = value;
				RaiseSettingsChanged();
			}
		}
	}

	public string CustomCookDirectory
	{
		get
		{
			return _customCookDirectory;
		}
		set
		{
			_customCookDirectory = value;
		}
	}

	[Category("User Preferences")]
	[DisplayName("Cook Test Projects")]
	[Description("When this is set to true, the Asset Cloud will perform a cook of the pantries associated with test projects. Projects are defined as test or not in ProjectBuilder when setting up a project ENV file.")]
	public bool CookTestProjects
	{
		get
		{
			return _cookTestProjects;
		}
		set
		{
			if (_cookTestProjects != value)
			{
				_cookTestProjects = value;
				RaiseSettingsChanged();
			}
		}
	}

	[Category("User Preferences")]
	[DisplayName("Enable Verbose Cooker Log")]
	[Description("When this is set to true, the cooker will log in verbose mode.")]
	public bool EnableVerboseCookerLog
	{
		get
		{
			return _enableVerboseCookerLog;
		}
		set
		{
			_enableVerboseCookerLog = value;
			RaiseSettingsChanged();
		}
	}

	[Category("User Preferences")]
	[DisplayName("Throttle Game Frame rate")]
	[Description("When this is set to true, whenever the game is in the background it's framerate will be throttled.")]
	public bool ThrottleGameFramerate
	{
		get
		{
			return _throttleGameFramerate;
		}
		set
		{
			_throttleGameFramerate = value;
			RaiseSettingsChanged();
		}
	}

	[XmlIgnore]
	[Browsable(false)]
	public string ActiveToolHost => UseLocalToolHost ? ToolHostLocal : ToolHost;

	[Browsable(false)]
	public string ToolsetName => ModTools ? "AssetModTools" : "AssetCloud";

	[Browsable(false)]
	public bool ModTools { get; set; }

	[Category("Previewer Tracing")]
	[DisplayName("Enable Previewer Tracing")]
	[Description("When this is set to true, the asset editor will record previewer traces")]
	public bool EnablePreviewerTracing
	{
		get
		{
			return _RecordPreviewerTraces;
		}
		set
		{
			_RecordPreviewerTraces = value;
			RaiseSettingsChanged();
		}
	}

	[XmlIgnore]
	[ReadOnly(true)]
	[Category("Previewer Tracing")]
	[DisplayName("Trace Location")]
	[Description("This is where the traces go")]
	public string PreviewerTraceLocation
	{
		get
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			return Path.Combine(folderPath, "Documents\\My Games", ToolsetName, "ToolTraces");
		}
	}

	public virtual event EventHandler SettingsChanged;

	private void RaiseSettingsChanged()
	{
		this.SettingsChanged?.Invoke(this, EventArgs.Empty);
	}

	public static string GetDefaultToolHostPlayerPath()
	{
		string executablePath = Application.ExecutablePath;
		executablePath = Path.GetDirectoryName(executablePath);
		return Path.Combine(executablePath, "ToolHostPlayer.exe");
	}

	public AssetCloudSettings()
		: this(modTools: false)
	{
	}

	public AssetCloudSettings(bool modTools)
	{
		ModTools = modTools;
		ShowAssertions = false;
		ToolHost = string.Empty;
		AssetCooker = string.Empty;
		ProjectConfigLocal = string.Empty;
		ToolHostLocal = string.Empty;
		EnableNotifications = true;
		NotificationDuration = 0.75f;
	}

	public AssetCloudSettings(IAssetCloudSettings other)
	{
		Copy(other, this, notifyUser: false);
	}

	public static void Copy(IAssetCloudSettings src, IAssetCloudSettings dest)
	{
		Copy(src, dest, notifyUser: true);
	}

	private static void Copy(IAssetCloudSettings src, IAssetCloudSettings dest, bool notifyUser)
	{
		if (notifyUser && dest.ShowAssertions != src.ShowAssertions)
		{
			MessageBox.Show("Show Assertions has changed. This requires a restart to take affect.");
		}
		dest.ModTools = src.ModTools;
		dest.ToolHost = src.ToolHost;
		dest.AssetCooker = src.AssetCooker;
		dest.ProjectConfigLocal = src.ProjectConfigLocal;
		dest.ShowAssertions = src.ShowAssertions;
		dest.ToolHostLocal = src.ToolHostLocal;
		dest.UseLocalConfig = src.UseLocalConfig;
		dest.UseLocalToolHost = src.UseLocalToolHost;
		dest.EnableNotifications = src.EnableNotifications;
		dest.NotificationDuration = src.NotificationDuration;
		dest.ValidatePantry = src.ValidatePantry;
		dest.EnableVerboseCookerLog = src.EnableVerboseCookerLog;
		dest.ThrottleGameFramerate = src.ThrottleGameFramerate;
		dest.SyncBeforeCooking = src.SyncBeforeCooking;
		dest.EnablePreviewerTracing = src.EnablePreviewerTracing;
		dest.ToolHostPlayer = src.ToolHostPlayer;
	}

	public virtual void Save()
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		string sFilename = Path.Combine(folderPath, "Documents\\My Games", ToolsetName, Resources.AssetCloudSettings);
		SerializationHelper.Serialize(sFilename, this);
	}

	public virtual bool Load()
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		string sFilename = Path.Combine(folderPath, "Documents\\My Games", ToolsetName, Resources.AssetCloudSettings);
		AssetCloudSettings assetCloudSettings = SerializationHelper.Deserialize<AssetCloudSettings>(sFilename);
		if (assetCloudSettings == null)
		{
			return false;
		}
		Copy(assetCloudSettings, this, notifyUser: false);
		return true;
	}

	public virtual RegistryKey GetToolsRegistryKey(string configName, string keyName)
	{
		string[] value = new string[5]
		{
			"Software",
			"Firaxis",
			"Tools",
			$"{ToolsetName}{configName}",
			keyName.Trim('\\', '/')
		};
		return Registry.CurrentUser.OpenSubKey(string.Join("\\", value), writable: true);
	}

	private bool HasBeenInstalled(string configName)
	{
		RegistryKey toolsRegistryKey = GetToolsRegistryKey(configName, "");
		if (toolsRegistryKey == null)
		{
			return false;
		}
		if (toolsRegistryKey.GetValue("GameStream") == null)
		{
			return false;
		}
		return true;
	}

	private string GetGameStreamPath(string configName)
	{
		RegistryKey toolsRegistryKey = GetToolsRegistryKey(configName, "");
		return toolsRegistryKey.GetValue("GameStream") as string;
	}

	private string GetConfigurationName()
	{
		return "Release";
	}

	private string GetGraphicsPlatformName()
	{
		return "DX11";
	}

	public bool ValidateAndUpdateReleaseToolHost(string configName)
	{
		if (!ModTools)
		{
			if (HasBeenInstalled(configName))
			{
				ToolHost = Path.Combine(GetGameStreamPath(configName) + "\\" + configName + "\\game\\Base\\Binaries\\Win64", configName + "ToolHost_Win64_DX11_Release.dll");
			}
			else
			{
				ToolHost = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configName + "ToolHost_Win64_" + GetGraphicsPlatformName() + "_" + GetConfigurationName() + ".dll");
			}
		}
		else
		{
			ToolHost = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configName + "ToolHost_Win64_DX11_Release.dll");
		}
		return File.Exists(ToolHost) || Debugger.IsAttached;
	}

	public bool ValidateAndUpdateReleaseAssetCooker(string configName)
	{
		if (!ModTools)
		{
			if (HasBeenInstalled(configName))
			{
				AssetCooker = Path.Combine(GetGameStreamPath(configName) + "\\" + configName + "\\game\\Base\\Binaries\\Win64", configName + "AssetCooker_FinalRelease.exe");
			}
			else
			{
				AssetCooker = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configName + "AssetCooker_" + GetConfigurationName() + ".exe");
			}
		}
		else
		{
			AssetCooker = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), "Cooker", configName + "AssetCooker_FinalRelease.exe");
		}
		return File.Exists(AssetCooker) || Debugger.IsAttached;
	}

	public static bool ValidatePath(string sClientRoot, string sPath, bool bIsFile)
	{
		if (string.IsNullOrEmpty(sClientRoot))
		{
			MessageBoxes.Show("A client root must be set within source control", "Asset Cloud Error", MessageBoxButton.OK, MessageBoxImage.Error);
			return false;
		}
		if (string.IsNullOrEmpty(sPath))
		{
			MessageBoxes.Show(string.Format("A {0} must be selected", bIsFile ? "file" : "directory"), "Asset Cloud Error", MessageBoxButton.OK, MessageBoxImage.Error);
			return false;
		}
		if (bIsFile)
		{
			if (!File.Exists(sPath))
			{
				MessageBoxes.Show($"The file '{sPath}' does not exist", "Asset Cloud Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}
		else if (!Directory.Exists(sPath))
		{
			MessageBoxes.Show($"The directory '{sPath}' does not exist", "Asset Cloud Error", MessageBoxButton.OK, MessageBoxImage.Error);
			return false;
		}
		return true;
	}

	public virtual bool ValidateUserLocalProjectConfig(string mainWorkspaceRoot)
	{
		if (string.IsNullOrEmpty(ProjectConfigLocal))
		{
			return false;
		}
		if (!File.Exists(ProjectConfigLocal))
		{
			return false;
		}
		if (!ValidatePath(mainWorkspaceRoot, ProjectConfigLocal, bIsFile: true))
		{
			return false;
		}
		return true;
	}

	public virtual bool ValidateUserLocalToolHost()
	{
		if (Debugger.IsAttached)
		{
			return true;
		}
		if (string.IsNullOrEmpty(ToolHostLocal))
		{
			return false;
		}
		if (!File.Exists(ToolHostLocal))
		{
			return false;
		}
		return true;
	}
}
