namespace Firaxis.CivTech.AssetObjects;

public interface IBLPEntryParameter : IParameter
{
	bool IsNullAllowed { get; set; }

	string DefaultEntryName { get; set; }

	string DefaultBLPPackage { get; set; }

	string DefaultXLPPath { get; set; }

	string LibraryName { get; set; }

	string XLPClassName { get; set; }
}
