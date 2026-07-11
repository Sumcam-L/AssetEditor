using System;
using Microsoft.Win32;

namespace Firaxis.CivTech;

public interface IAssetCloudSettings
{
	bool ModTools { get; set; }

	string ToolsetName { get; }

	bool UseLocalConfig { get; set; }

	string ProjectConfigLocal { get; set; }

	string ActiveConfigPath { get; set; }

	bool UseLocalToolHost { get; set; }

	string ToolHost { get; set; }

	string ToolHostLocal { get; set; }

	string ActiveToolHost { get; }

	string AssetCooker { get; set; }

	string ToolHostPlayer { get; set; }

	bool ShowAssertions { get; set; }

	bool EnableVerboseCookerLog { get; set; }

	bool EnableNotifications { get; set; }

	float NotificationDuration { get; set; }

	bool ValidatePantry { get; set; }

	bool ThrottleGameFramerate { get; set; }

	bool SyncBeforeCooking { get; set; }

	bool CookTestProjects { get; set; }

	bool UseCustomCookDirectory { get; set; }

	string CustomCookDirectory { get; set; }

	bool EnablePreviewerTracing { get; set; }

	string PreviewerTraceLocation { get; }

	event EventHandler SettingsChanged;

	RegistryKey GetToolsRegistryKey(string configName, string keyName);

	bool Load();

	void Save();
}
