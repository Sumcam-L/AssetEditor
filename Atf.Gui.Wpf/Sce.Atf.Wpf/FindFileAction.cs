using System.ComponentModel;

namespace Sce.Atf.Wpf;

public enum FindFileAction
{
	[DisplayString("Accept suggestion")]
	[Description("Accept suggestion for this file.")]
	AcceptSuggestion,
	[DisplayString("Accept all suggestion")]
	[Description("Accept suggestion for all files.")]
	AcceptAllSuggestions,
	[DisplayString("Search directory")]
	[Description("Search for file in a specified directory and its sub-directories.")]
	SearchDirectory,
	[DisplayString("Search directory for all")]
	[Description("Search a specified directory and its sub-directories for all missing files.")]
	SearchDirectoryForAll,
	[DisplayString("Specify a new location")]
	[Description("Manually specify a new location for the file.")]
	UserSpecify,
	[DisplayString("Ignore")]
	[Description("Ignore this missing file (don't try to find it).")]
	Ignore,
	[DisplayString("Ignore all")]
	[Description("Ignore all missing files (don't prompt user again).")]
	IgnoreAll,
	[DisplayString("Quit")]
	[Description("Quit searching for remaining files.")]
	Quit
}
