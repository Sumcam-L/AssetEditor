using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.CivTech;

public struct DepotFileInfo : IEquatable<DepotFileInfo>
{
	public static readonly DepotFileInfo Empty = default(DepotFileInfo);

	public string Filename;

	public long Timestamp;

	public int Status;

	public int Type;

	public int EntityType;

	public string EntityClass;

	public long Filesize;

	public IList<string> Tags;

	public IDictionary<string, int> Stats;

	public DepotFileInfo(string fname, long tstamp, FileStatus status, long fileSize, FileType ft, InstanceType it, string entClass)
	{
		Filename = fname;
		Timestamp = tstamp;
		Status = (int)status;
		Type = (int)ft;
		EntityType = (int)it;
		EntityClass = entClass;
		Filesize = fileSize;
		Tags = new List<string>();
		Stats = new ConcurrentDictionary<string, int>();
	}

	public override int GetHashCode()
	{
		return PathCompareHelper.GetHashCode(Filename, bIgnoreCase: true) ^ EntityClass.GetHashCode();
	}

	public bool Equals(DepotFileInfo other)
	{
		return PathCompareHelper.Equals(Filename, other.Filename, bIgnoreCase: true) && EntityClass.Equals(other.EntityClass);
	}
}
