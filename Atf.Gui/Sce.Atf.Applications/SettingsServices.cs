using System.ComponentModel;

namespace Sce.Atf.Applications;

public static class SettingsServices
{
	public static void RegisterSettings(this ISettingsService settingsService, object owner, params PropertyDescriptor[] properties)
	{
		settingsService.RegisterSettings(owner.GetType().FullName, properties);
	}

	public static void RegisterUserSettings(this ISettingsService settingsService, string pathName, params PropertyDescriptor[] properties)
	{
		settingsService.RegisterUserSettings(pathName, properties);
	}
}
