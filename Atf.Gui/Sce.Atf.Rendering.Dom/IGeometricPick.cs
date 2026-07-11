using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public interface IGeometricPick : IIntersectable
{
	bool IntersectRay(Ray3F ray, Camera camera, RenderState renderState, Matrix4F localToWorld, IRenderAction renderAction, out Vec3F intersectionPoint, out Vec3F nearestVert, out Vec3F surfaceNormal, List<uint> userData);

	bool IntersectFrustum(Frustum frustum, Vec3F eye, RenderState renderState, List<uint> userData);
}
