using System.Windows.Forms;
using Firaxis.Utility.Properties;
using Microsoft.Win32;

namespace Firaxis.Utility;

public static class ApplicationHelper
{
	public static string LocalUserCommonAppDataPath
	{
		get
		{
			string localUserAppDataPath = Application.LocalUserAppDataPath;
			return localUserAppDataPath.Remove(localUserAppDataPath.IndexOf(Application.ProductVersion) - 1);
		}
	}

	public static string ProductVersion => GetApplicationVersion(Application.ProductName);

	public static string GetApplicationVersion(string sApplicationName)
	{
		RegistryKey registryKey = null;
		string result = Application.ProductVersion;
		try
		{
			registryKey = Registry.CurrentUser.OpenSubKey(Resources.ToolsRegKey + "\\" + sApplicationName);
			if (registryKey != null)
			{
				result = (string)registryKey.GetValue("Version");
			}
		}
		finally
		{
			registryKey?.Close();
		}
		return result;
	}
}
