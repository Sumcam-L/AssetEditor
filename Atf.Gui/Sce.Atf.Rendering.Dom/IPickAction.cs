using System;
using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public interface IPickAction : IRenderAction
{
	Type TypeFilter { set; }

	ICollection<Type> TypesFilter { get; set; }

	void Init(Camera camera, int x1, int y1, int x2, int y2, bool multiPick, bool usePickingFrustum);

	HitRecord[] GetHits();

	HitRecord[] GetHits(bool multiPick);

	HitRecord[] GetHits(ICollection<TraverseNode> traverseList);

	HitRecord[] GetHits(ICollection<TraverseNode> traverseList, bool multiPick);

	bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point);

	bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point, out Vec3F surfaceNormal);

	bool Intersect(Camera camera, int x, int y, Scene scene, ICollection<TraverseNode> traverseList, ref Vec3F point);

	bool Intersect(Camera camera, int x, int y, Scene scene, ref Vec3F point, out HitRecord firstHit);
}
