using System;
using System.Collections.Generic;

namespace Firaxis.Granny;

public interface IGrannyFile : IDisposable
{
	string Source { get; set; }

	string Filename { get; set; }

	List<string> BoneNames { get; }

	List<string> TrackMaskNames { get; }

	List<IGrannyModel> Models { get; }

	List<IGrannyAnimation> Animations { get; }

	List<IGrannyMesh> Meshes { get; }

	List<IGrannyMaterial> Materials { get; }

	IGrannyMaterial AddMaterial();

	IGrannyModel AddModel();
	IGrannyAnimation AddAnimation();
	IGrannyTrackGroup AddTrackGroup();

    bool RemoveMaterial(IGrannyMaterial kMaterial);

	bool AddArtToolAndExporterReference(IGrannyFile kFile);

	bool AddMeshReference(IGrannyMesh kMesh);

	bool AddModelReference(IGrannyModel kModel);

	bool AddMaterialReference(IGrannyMaterial kMaterial);

	bool AddAnimationReference(IGrannyAnimation kAnimation);


    bool CompressAnimations(CurveCompressionParameters positionParams, CurveCompressionParameters orientationParams, CurveCompressionParameters shearParams);

	bool Save();
}
