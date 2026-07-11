using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public struct BLPData : IComparable<BLPData>, IComparable
{
	public string BLPPath;

	public string Name;

	public string XLPPath;

	public BLPData(IBLPEntryValue value)
	{
		Name = value.EntryName;
		XLPPath = value.XLPPath;
		BLPPath = value.BLPPackage;
	}

	public BLPData(string entryName, string xlpPath, string blpPath)
	{
		Name = entryName;
		XLPPath = xlpPath;
		BLPPath = blpPath;
	}

	int IComparable.CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (obj is BLPData bLPData)
		{
			return Name.CompareTo(bLPData.Name);
		}
		throw new ArgumentException("Object is not a BLPData.");
	}

	int IComparable<BLPData>.CompareTo(BLPData other)
	{
		return Name.CompareTo(other.Name);
	}

	public override string ToString()
	{
		return Name;
	}
}
