using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IAssetClass : IClassEntity, ICloudEntity, INameProvider, IVersionedData, IAnimatableClass, IEntityContainerClass, IAttachmentContainer
{
	IEnumerable<IAssetClassState> States { get; }

	IEnumerable<IAssetArtDefReference> ArtDefReferences { get; }

	IEnumerable<string> AllowedGeometryClasses { get; }

	IEnumerable<string> AllowedParticleEffectClasses { get; }

	IEnumerable<string> AllowedBehaviorClasses { get; }

	IEnumerable<string> AllowedSplineClasses { get; }

	IAssetClassState AddState(string name);

	IAssetClassState AddState(string name, string description);

	void RemoveState(IAssetClassState state);

	IAssetArtDefReference AddArtDefReference(string templateName);

	IAssetArtDefReference AddArtDefReference(string templateName, string collectionName);

	void RemoveArtDefReference(IAssetArtDefReference artDefReference);

	void AllowGeometryClass(string name);

	void AllowParticleEffectClass(string name);

	void AllowBehaviorClass(string name);

	void AllowSplineClass(string name);

	void AllowPlatform(Platforms ePlatform);

	bool IsGeometryClassAllowed(string name);

	bool IsParticleEffectClassAllowed(string name);

	bool IsBehaviorClassAllowed(string name);

	bool IsSplineClassAllowed(string name);

	bool IsPlatformAllowed(Platforms ePlatform);

	void ClearAllowedPlatformsAndClasses();
}
