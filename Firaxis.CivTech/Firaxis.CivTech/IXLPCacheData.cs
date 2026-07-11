namespace Firaxis.CivTech;

public interface IXLPCacheData
{
	string BLPPackage { get; }

	string EntryName { get; }

	string ObjectName { get; }

	string XLPClassName { get; }

	string XLPPath { get; }
}
