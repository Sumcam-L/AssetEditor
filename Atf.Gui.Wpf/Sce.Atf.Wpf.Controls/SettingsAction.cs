using System.ComponentModel;

namespace Sce.Atf.Wpf.Controls;

internal enum SettingsAction
{
	[DisplayString("Save")]
	[Description("Settings will be saved to a file so they can be loaded at a later time or on a different machine.")]
	Save,
	[DisplayString("Load")]
	[Description("Load settings from a file and apply them to the application.")]
	Load
}
