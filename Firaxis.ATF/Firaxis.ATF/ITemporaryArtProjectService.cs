using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;

namespace Firaxis.ATF;

public interface ITemporaryArtProjectService : ITemporaryArtOutputPaths
{
	bool SingleAssetCookEnabled { get; }

	IEnumerable<Uri> GetCookUris(IEnumerable<EntityID> changedEntities);

	IEnumerable<Uri> RemoveOverlappingTempXLPEntries(IXLP xlp);
}
