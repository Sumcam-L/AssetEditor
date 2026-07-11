using System;
using System.ComponentModel.DataAnnotations;

namespace Firaxis.CivTech.AssetObjects;

[Flags]
public enum Platforms
{
	[Display(Name = "Invalid")]
	PLATFORM_INVALID = 0,
	[Display(Name = "Windows")]
	PLATFORM_WINDOWS = 1,
	[Display(Name = "iOS")]
	PLATFORM_IOS = 2,
	[Display(Name = "MacOS")]
	PLATFORM_MACOS = 4,
	[Display(Name = "Linux")]
	PLATFORM_LINUX = 8,
	[Display(Name = "Xbox One")]
	PLATFORM_XBONE = 0x10,
	[Display(Name = "Switch")]
	PLATFORM_SWITCH = 0x20,
	[Display(Name = "PS4")]
	PLATFORM_PS4 = 0x40,
	[Display(Name = "Stadia")]
	PLATFORM_STADIA = 0x80,
	[Display(Name = "All")]
	PLATFORM_ALL = 0xFF
}
