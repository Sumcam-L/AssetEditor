using System.Collections.Generic;
using Firaxis.Reflection;

namespace Firaxis.CivTech.AssetObjects;

public static class PlatformsAssistant
{
	public static IEnumerable<Platforms> GetUsablePlatforms()
	{
		yield return Platforms.PLATFORM_WINDOWS;
		yield return Platforms.PLATFORM_MACOS;
		yield return Platforms.PLATFORM_IOS;
		yield return Platforms.PLATFORM_LINUX;
		yield return Platforms.PLATFORM_XBONE;
		yield return Platforms.PLATFORM_PS4;
		yield return Platforms.PLATFORM_SWITCH;
		yield return Platforms.PLATFORM_STADIA;
	}

	public static string GetNameFromPlatform(Platforms platform)
	{
		return ReflectionHelper.GetDisplayName(platform);
	}
}
