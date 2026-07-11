using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering;

public interface IJoint : INameable
{
	IList<IJoint> ChildJoints { get; }

	Matrix4F Transform { get; set; }

	Vec3F Translation { get; set; }

	Vec3F Rotation { get; set; }

	Vec3F RotationAxis { get; set; }

	Vec3F RotatePivot { get; set; }

	Vec3F RotatePivotTranslation { get; set; }

	Vec3F Scale { get; set; }

	Vec3F ScalePivot { get; set; }

	Vec3F ScalePivotTranslation { get; set; }

	bool RotationFreedomInX { get; set; }

	bool RotationFreedomInY { get; set; }

	bool RotationFreedomInZ { get; set; }

	bool HasRotationMinX { get; }

	bool HasRotationMinY { get; }

	bool HasRotationMinZ { get; }

	float RotationMinX { get; set; }

	float RotationMinY { get; set; }

	float RotationMinZ { get; set; }

	bool HasRotationMaxX { get; }

	bool HasRotationMaxY { get; }

	bool HasRotationMaxZ { get; }

	float RotationMaxX { get; set; }

	float RotationMaxY { get; set; }

	float RotationMaxZ { get; set; }

	EulerAngles3F JointOrientEul { get; set; }

	bool ScaleCompensate { get; set; }
}
