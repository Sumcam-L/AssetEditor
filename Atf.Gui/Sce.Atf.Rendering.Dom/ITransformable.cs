using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public interface ITransformable : IBoundable, IVisible
{
	Matrix4F Transform { get; set; }

	Vec3F Translation { get; set; }

	Vec3F Rotation { get; set; }

	Vec3F Scale { get; set; }

	Vec3F ScalePivot { get; set; }

	Vec3F ScalePivotTranslation { get; set; }

	Vec3F RotatePivot { get; set; }

	Vec3F RotatePivotTranslation { get; set; }

	TransformationTypes TransformationType { get; set; }
}
