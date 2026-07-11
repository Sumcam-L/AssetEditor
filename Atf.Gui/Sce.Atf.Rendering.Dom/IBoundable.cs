using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public interface IBoundable
{
	Box BoundingBox { get; }
}
