using System;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public static class IXLPRegistryExtensions
{
	public static void FixupBLPReference(this IXLPRegistry xlpRegistry, IBLPEntryValue blpEntryValue, Action<string> outputDelegate)
	{
		if (string.IsNullOrEmpty(blpEntryValue.EntryName) || string.IsNullOrEmpty(blpEntryValue.XLPPath))
		{
			return;
		}
		string xLPPath = blpEntryValue.XLPPath;
		string entryName = blpEntryValue.EntryName;
		if (!xlpRegistry.IsEntryValid(entryName, xLPPath))
		{
			IXLPCacheData iXLPCacheData = xlpRegistry.FindXLPData(entryName);
			if (iXLPCacheData != null)
			{
				string xLPPath2 = iXLPCacheData.XLPPath;
				string bLPPackage = iXLPCacheData.BLPPackage;
				string obj = "Replaced broken XLP reference " + xLPPath + " with " + xLPPath2 + ".  Package changed from " + blpEntryValue.BLPPackage + " to " + bLPPackage + ".";
				outputDelegate(obj);
				blpEntryValue.XLPPath = xLPPath2;
				blpEntryValue.BLPPackage = bLPPackage;
			}
		}
	}

	public static bool IsEntryValid(this IXLPRegistry xlpRegistry, string entryName, string relativeXLPPath)
	{
		return xlpRegistry.GetEntityID(relativeXLPPath, entryName).Type != InstanceType.IT_INVALID;
	}
}
