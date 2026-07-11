namespace Firaxis.CivTech.AssetObjects;

public interface IBLPEntryValue : IValue
{
	string EntryName { get; set; }

	string XLPPath { get; set; }

	string BLPPackage { get; set; }

	string XLPClass { get; set; }

	string LibraryName { get; set; }
}
