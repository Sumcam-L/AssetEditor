using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public interface IIntersectable
{
	bool CanIntersect { get; }

	bool IntersectRay(Ray3F ray, bool backfaceCull, out Vec3F intersectionPoint, out Vec3F nearestVert, out Vec3F normal);

	bool IntersectRay(Ray3F ray, bool backfaceCull, out Vec3F intersectionPoint);
}
