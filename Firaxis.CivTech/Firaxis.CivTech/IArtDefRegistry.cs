using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;

namespace Firaxis.CivTech;

public interface IArtDefRegistry
{
	string PrimaryArtDefPantry { get; }

	ArtDefReferenceInfo GetArtDefInfo(string elementName, IValue artDefRefVal, IParameter artDefRefParam);

	string GetArtDefString(string relativeArtDefPath);

	IEnumerable<IXLPEntry> GetOrphanedEntries(IXLP xlp);

	IEnumerable<string> GetRelativeArtDefPaths();

	string[] GetSuitableCollections(IValue artDefRefVal, IParameter artDefRefParam);

	string[] GetSuitableElements(string templateName, string collectionName);

	string[] GetSuitableElements(IValue artDefRefVal, IParameter artDefRefParam);

	void HandleProjectChange();

	bool IsEntryOrphaned(IXLP xlp, IXLPEntry xlpEntry, ICollection<IArtDef> referrers);

	IArtDefCollection[] GetSuitableCollections(string templateName, string collectionName);
}
