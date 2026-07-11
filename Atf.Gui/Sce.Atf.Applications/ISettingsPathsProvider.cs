namespace Sce.Atf.Applications;

public interface ISettingsPathsProvider
{
	string SettingsPath { get; }

	string DefaultSettingsPath { get; }
}
