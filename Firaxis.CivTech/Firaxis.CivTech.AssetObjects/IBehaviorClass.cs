using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IBehaviorClass : IClassEntity, ICloudEntity, INameProvider, IVersionedData, IAnimatableClass, IAttachmentContainer
{
	IEnumerable<IAssetArtDefReference> ArtDefReferences { get; }

	IEnumerable<string> AllowedGeometryClasses { get; }

	IAssetArtDefReference AddArtDefReference(string templateName);

	IAssetArtDefReference AddArtDefReference(string templateName, string collectionName);

	void RemoveArtDefReference(IAssetArtDefReference artDefReference);

	void AllowGeometryClass(string name);

	bool IsGeometryClassAllowed(string name);

	void ClearAllowedClasses();
}
