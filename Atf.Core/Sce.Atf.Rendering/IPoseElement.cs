using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering;

public interface IPoseElement
{
	object Target { get; set; }

	Vec3F Translation { get; set; }

	EulerAngles3F Rotation { get; set; }

	Vec3F Scale { get; set; }
}
