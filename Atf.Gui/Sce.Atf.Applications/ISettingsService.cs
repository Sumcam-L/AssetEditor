using System;
using System.ComponentModel;

namespace Sce.Atf.Applications;

public interface ISettingsService
{
	event EventHandler Saving;

	event EventHandler Loading;

	event EventHandler Reloaded;

	void RegisterSettings(string uid, params PropertyDescriptor[] properties);

	void RegisterUserSettings(string pathName, params PropertyDescriptor[] properties);

	void PresentUserSettings(string pathName);

	void SaveSettings();
}
