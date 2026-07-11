using System;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public static class TransformUtils
{
	private static readonly string Pivot = "Pivot";

	private static readonly string Origin = "Origin";

	private static readonly string BottomCenter = "BottomCenter";

	public static string[] SnapFromModes => new string[3] { Pivot, Origin, BottomCenter };

	public static Matrix4F CalcPathTransform(SceneNode[] path)
	{
		return CalcPathTransform(path, 0);
	}

	public static Matrix4F CalcPathTransform(SceneNode[] path, int start)
	{
		Matrix4F matrix4F = new Matrix4F();
		for (int i = start; i < path.Length; i++)
		{
			if (path[i].Source != null)
			{
				ITransformable transformable = path[i].Source.As<ITransformable>();
				if (transformable != null)
				{
					matrix4F.Mul(matrix4F, transformable.Transform);
				}
			}
		}
		return matrix4F;
	}

	public static Matrix4F CalcPathTransform(Path<DomNode> path, int start)
	{
		Matrix4F matrix4F = new Matrix4F();
		for (int num = start; num >= 0; num--)
		{
			if (path[num] != null)
			{
				ITransformable transformable = path[num].As<ITransformable>();
				if (transformable != null)
				{
					matrix4F.Mul(matrix4F, transformable.Transform);
				}
			}
		}
		return matrix4F;
	}

	public static Box CalcWorldBoundingBox(Path<DomNode> path)
	{
		int num = path.Count;
		IBoundable boundable = null;
		while (--num >= 0)
		{
			if (path[num] != null)
			{
				boundable = path[num].As<IBoundable>();
				if (boundable != null)
				{
					break;
				}
			}
		}
		if (boundable == null)
		{
			return new Box();
		}
		Matrix4F m = CalcPathTransform(path, num - 1);
		Box boundingBox = boundable.BoundingBox;
		boundingBox.Transform(m);
		return boundingBox;
	}

	public static SnapFromMode GetSnapFromMode(string modeString)
	{
		if (modeString == null || modeString == Pivot)
		{
			return SnapFromMode.Pivot;
		}
		if (modeString == Origin)
		{
			return SnapFromMode.Origin;
		}
		if (modeString == BottomCenter)
		{
			return SnapFromMode.BottomCenter;
		}
		throw new ArgumentException("Unknown snap-from mode string", "modeString");
	}

	public static string GetSnapFromMode(SnapFromMode mode)
	{
		return mode switch
		{
			SnapFromMode.Pivot => Pivot, 
			SnapFromMode.Origin => Origin, 
			SnapFromMode.BottomCenter => BottomCenter, 
			_ => throw new ArgumentOutOfRangeException("mode"), 
		};
	}

	public static Vec3F CalcSnapFromOffset(ITransformable node, string snapFromMode, AxisSystemType axisType, Vec3F pivot)
	{
		SnapFromMode snapFromMode2 = GetSnapFromMode(snapFromMode);
		return CalcSnapFromOffset(node, snapFromMode2, axisType, pivot);
	}

	public static Vec3F CalcSnapFromOffset(ITransformable node, SnapFromMode snapFromMode, AxisSystemType axisType, Vec3F pivot)
	{
		switch (snapFromMode)
		{
		case SnapFromMode.Pivot:
		{
			Path<DomNode> path2 = new Path<DomNode>(node.Cast<DomNode>().Ancestry);
			Matrix4F matrix4F2 = CalcPathTransform(path2, path2.Count - 2);
			node.Transform.TransformVector(pivot, out var result2);
			matrix4F2.TransformVector(result2, out result2);
			return result2;
		}
		case SnapFromMode.Origin:
			return new Vec3F(0f, 0f, 0f);
		case SnapFromMode.BottomCenter:
		{
			Box boundingBox = node.BoundingBox;
			Vec3F vec3F = ((axisType != AxisSystemType.YIsUp) ? new Vec3F((boundingBox.Min.X + boundingBox.Max.X) * 0.5f, (boundingBox.Min.Y + boundingBox.Max.Y) * 0.5f, boundingBox.Min.Z) : new Vec3F((boundingBox.Min.X + boundingBox.Max.X) * 0.5f, boundingBox.Min.Y, (boundingBox.Min.Z + boundingBox.Max.Z) * 0.5f));
			Vec3F translation = node.Transform.Translation;
			Vec3F result = vec3F - translation;
			Path<DomNode> path = new Path<DomNode>(node.Cast<DomNode>().GetPath());
			Matrix4F matrix4F = CalcPathTransform(path, path.Count - 2);
			matrix4F.TransformVector(result, out result);
			return result;
		}
		default:
			throw new ArgumentException("Invalid snap-from node");
		}
	}

	public static Vec3F CalcSnapFromOffset(ITransformable node, string snapFromMode, AxisSystemType axisType)
	{
		SnapFromMode snapFromMode2 = GetSnapFromMode(snapFromMode);
		return CalcSnapFromOffset(node, snapFromMode2, axisType);
	}

	public static Vec3F CalcSnapFromOffset(ITransformable node, SnapFromMode snapFromMode, AxisSystemType axisType)
	{
		Vec3F pivot = default(Vec3F);
		if ((node.TransformationType & TransformationTypes.RotatePivot) != 0)
		{
			pivot = node.RotatePivot;
		}
		return CalcSnapFromOffset(node, snapFromMode, axisType, pivot);
	}

	public static Vec3F RotateToVector(Vec3F originalEulers, Vec3F surfaceNormal, AxisSystemType upAxis)
	{
		Matrix3F matrix3F = new Matrix3F();
		matrix3F.Rotation(originalEulers);
		Vec3F xAxis = matrix3F.XAxis;
		Vec3F yAxis = matrix3F.YAxis;
		Vec3F zAxis = matrix3F.ZAxis;
		Vec3F vec3F2;
		Vec3F v;
		Vec3F vec3F;
		if (upAxis == AxisSystemType.YIsUp)
		{
			vec3F = new Vec3F(surfaceNormal);
			float value = Vec3F.Dot(xAxis, surfaceNormal);
			float value2 = Vec3F.Dot(zAxis, surfaceNormal);
			if (Math.Abs(value) < Math.Abs(value2))
			{
				v = new Vec3F(xAxis);
				vec3F2 = Vec3F.Cross(v, vec3F);
				v = Vec3F.Cross(vec3F, vec3F2);
			}
			else
			{
				v = Vec3F.Cross(v2: new Vec3F(zAxis), v1: vec3F);
				vec3F2 = Vec3F.Cross(v, vec3F);
			}
		}
		else
		{
			vec3F2 = new Vec3F(surfaceNormal);
			float value3 = Vec3F.Dot(xAxis, surfaceNormal);
			float value4 = Vec3F.Dot(yAxis, surfaceNormal);
			if (Math.Abs(value3) < Math.Abs(value4))
			{
				vec3F = Vec3F.Cross(v2: new Vec3F(xAxis), v1: vec3F2);
				v = Vec3F.Cross(vec3F, vec3F2);
			}
			else
			{
				vec3F = new Vec3F(yAxis);
				v = Vec3F.Cross(vec3F, vec3F2);
				vec3F = Vec3F.Cross(vec3F2, v);
			}
		}
		v.Normalize();
		vec3F.Normalize();
		vec3F2.Normalize();
		matrix3F.XAxis = v;
		matrix3F.YAxis = vec3F;
		matrix3F.ZAxis = vec3F2;
		Vec3F result = default(Vec3F);
		matrix3F.GetEulerAngles(out result.X, out result.Y, out result.Z);
		return result;
	}

	public static Vec3F GetUpVector(AxisSystemType axis)
	{
		if (axis == AxisSystemType.YIsUp)
		{
			return new Vec3F(0f, 1f, 0f);
		}
		return new Vec3F(0f, 0f, 1f);
	}

	public static void CalcWorldDimensions(Camera camera, Matrix4F globalTransform, out float h, out float w)
	{
		Matrix4F matrix4F = new Matrix4F();
		matrix4F.Mul(globalTransform, camera.ViewMatrix);
		if (camera.Frustum.IsOrtho)
		{
			w = camera.Frustum.Right - camera.Frustum.Left;
			h = camera.Frustum.Top - camera.Frustum.Bottom;
		}
		else
		{
			float num = 0f - matrix4F.Translation.Z;
			h = num * (float)Math.Tan(camera.Frustum.FovY / 2f) * 2f;
			w = num * (float)Math.Tan(camera.Frustum.FovX / 2f) * 2f;
		}
	}

	public static Ray3F CreateRay(float x, float y, Camera camera)
	{
		return camera.CreateRay(x, y);
	}

	public static void AddChild(ITransformable parent, ITransformable child)
	{
		Path<DomNode> path = new Path<DomNode>(parent.Cast<DomNode>().GetPath());
		Matrix4F m = CalcPathTransform(path, path.Count - 1);
		Matrix4F matrix4F = new Matrix4F();
		matrix4F.Invert(m);
		Matrix4F transform = child.Transform;
		Matrix4F matrix4F2 = Matrix4F.Multiply(transform, matrix4F);
		Vec3F v = child.Translation;
		matrix4F.Transform(ref v);
		Vec3F rotation = default(Vec3F);
		matrix4F2.GetEulerAngles(out rotation.X, out rotation.Y, out rotation.Z);
		child.Rotation = rotation;
		Matrix4F matrix4F3 = CalcTransform(scale: child.Scale = matrix4F2.GetScale(), translation: v, rotation: rotation, scalePivot: child.ScalePivot, scalePivotTranslate: child.ScalePivotTranslation, rotatePivot: child.RotatePivot, rotatePivotTranslate: child.RotatePivotTranslation);
		Vec3F translation = matrix4F2.Translation;
		Vec3F translation2 = matrix4F3.Translation;
		Vec3F vec3F = translation - translation2;
		Matrix4F m2 = new Matrix4F(vec3F);
		matrix4F3.Mul(matrix4F3, m2);
		child.Translation = v + vec3F;
		child.Transform = matrix4F3;
	}

	public static void RemoveChild(ITransformable parent, ITransformable child)
	{
		Path<DomNode> path = new Path<DomNode>(parent.Cast<DomNode>().GetPath());
		Matrix4F matrix4F = CalcPathTransform(path, path.Count - 1);
		Matrix4F transform = child.Transform;
		Matrix4F matrix4F2 = Matrix4F.Multiply(transform, matrix4F);
		Vec3F v = child.Translation;
		matrix4F.Transform(ref v);
		Vec3F rotation = default(Vec3F);
		matrix4F2.GetEulerAngles(out rotation.X, out rotation.Y, out rotation.Z);
		child.Rotation = rotation;
		Matrix4F matrix4F3 = CalcTransform(scale: child.Scale = matrix4F2.GetScale(), translation: v, rotation: rotation, scalePivot: child.ScalePivot, scalePivotTranslate: child.ScalePivotTranslation, rotatePivot: child.RotatePivot, rotatePivotTranslate: child.RotatePivotTranslation);
		Vec3F translation = matrix4F2.Translation;
		Vec3F translation2 = matrix4F3.Translation;
		Vec3F vec3F = translation - translation2;
		Matrix4F m = new Matrix4F(vec3F);
		matrix4F3.Mul(matrix4F3, m);
		child.Translation = v + vec3F;
		child.Transform = matrix4F3;
	}

	public static Matrix4F CalcTransform(ITransformable node)
	{
		return CalcTransform(node.Translation, node.Rotation, node.Scale, node.ScalePivot, node.ScalePivotTranslation, node.RotatePivot, node.RotatePivotTranslation);
	}

	public static Matrix4F CalcTransform(Vec3F translation, Vec3F rotation, Vec3F scale, Vec3F scalePivot, Vec3F scalePivotTranslate, Vec3F rotatePivot, Vec3F rotatePivotTranslate)
	{
		Matrix4F matrix4F = new Matrix4F();
		Matrix4F matrix4F2 = new Matrix4F();
		matrix4F.Set(-scalePivot);
		matrix4F2.Scale(scale);
		matrix4F.Mul(matrix4F, matrix4F2);
		matrix4F2.Set(scalePivot + scalePivotTranslate - rotatePivot);
		matrix4F.Mul(matrix4F, matrix4F2);
		if (rotation.X != 0f)
		{
			matrix4F2.RotX(rotation.X);
			matrix4F.Mul(matrix4F, matrix4F2);
		}
		if (rotation.Y != 0f)
		{
			matrix4F2.RotY(rotation.Y);
			matrix4F.Mul(matrix4F, matrix4F2);
		}
		if (rotation.Z != 0f)
		{
			matrix4F2.RotZ(rotation.Z);
			matrix4F.Mul(matrix4F, matrix4F2);
		}
		matrix4F2.Set(rotatePivot + rotatePivotTranslate + translation);
		matrix4F.Mul(matrix4F, matrix4F2);
		return matrix4F;
	}

	public static Vec2F TransformToViewport(Vec3F localPoint, Matrix4F localToScreen, float viewportWidth, float viewportHeight)
	{
		Vec4F result = new Vec4F(localPoint);
		localToScreen.Transform(result, out result);
		result = Vec4F.Mul(result, 1f / result.W);
		return new Vec2F((result.X + 1f) * 0.5f * viewportWidth, (1f - (result.Y + 1f) * 0.5f) * viewportHeight);
	}
}
