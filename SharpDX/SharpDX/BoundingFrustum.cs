using System;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct BoundingFrustum : IEquatable<BoundingFrustum>, IDataSerializable
{
	private Matrix pMatrix;

	private Plane pNear;

	private Plane pFar;

	private Plane pLeft;

	private Plane pRight;

	private Plane pTop;

	private Plane pBottom;

	public Matrix Matrix
	{
		get
		{
			return pMatrix;
		}
		set
		{
			pMatrix = value;
			GetPlanesFromMatrix(ref pMatrix, out pNear, out pFar, out pLeft, out pRight, out pTop, out pBottom);
		}
	}

	public Plane Near => pNear;

	public Plane Far => pFar;

	public Plane Left => pLeft;

	public Plane Right => pRight;

	public Plane Top => pTop;

	public Plane Bottom => pBottom;

	public bool IsOrthographic
	{
		get
		{
			if (pLeft.Normal == -pRight.Normal)
			{
				return pTop.Normal == -pBottom.Normal;
			}
			return false;
		}
	}

	public BoundingFrustum(Matrix matrix)
	{
		pMatrix = matrix;
		GetPlanesFromMatrix(ref pMatrix, out pNear, out pFar, out pLeft, out pRight, out pTop, out pBottom);
	}

	public override int GetHashCode()
	{
		return pMatrix.GetHashCode();
	}

	public bool Equals(BoundingFrustum other)
	{
		return pMatrix == other.pMatrix;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is BoundingFrustum)
		{
			return Equals((BoundingFrustum)obj);
		}
		return false;
	}

	public static bool operator ==(BoundingFrustum left, BoundingFrustum right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BoundingFrustum left, BoundingFrustum right)
	{
		return !left.Equals(right);
	}

	public Plane GetPlane(int index)
	{
		return index switch
		{
			0 => pLeft, 
			1 => pRight, 
			2 => pTop, 
			3 => pBottom, 
			4 => pNear, 
			5 => pFar, 
			_ => default(Plane), 
		};
	}

	private static void GetPlanesFromMatrix(ref Matrix matrix, out Plane near, out Plane far, out Plane left, out Plane right, out Plane top, out Plane bottom)
	{
		left.Normal.X = matrix.M14 + matrix.M11;
		left.Normal.Y = matrix.M24 + matrix.M21;
		left.Normal.Z = matrix.M34 + matrix.M31;
		left.D = matrix.M44 + matrix.M41;
		left.Normalize();
		right.Normal.X = matrix.M14 - matrix.M11;
		right.Normal.Y = matrix.M24 - matrix.M21;
		right.Normal.Z = matrix.M34 - matrix.M31;
		right.D = matrix.M44 - matrix.M41;
		right.Normalize();
		top.Normal.X = matrix.M14 - matrix.M12;
		top.Normal.Y = matrix.M24 - matrix.M22;
		top.Normal.Z = matrix.M34 - matrix.M32;
		top.D = matrix.M44 - matrix.M42;
		top.Normalize();
		bottom.Normal.X = matrix.M14 + matrix.M12;
		bottom.Normal.Y = matrix.M24 + matrix.M22;
		bottom.Normal.Z = matrix.M34 + matrix.M32;
		bottom.D = matrix.M44 + matrix.M42;
		bottom.Normalize();
		near.Normal.X = matrix.M13;
		near.Normal.Y = matrix.M23;
		near.Normal.Z = matrix.M33;
		near.D = matrix.M43;
		near.Normalize();
		far.Normal.X = matrix.M14 - matrix.M13;
		far.Normal.Y = matrix.M24 - matrix.M23;
		far.Normal.Z = matrix.M34 - matrix.M33;
		far.D = matrix.M44 - matrix.M43;
		far.Normalize();
	}

	private static Vector3 Get3PlanesInterPoint(ref Plane p1, ref Plane p2, ref Plane p3)
	{
		return (0f - p1.D) * Vector3.Cross(p2.Normal, p3.Normal) / Vector3.Dot(p1.Normal, Vector3.Cross(p2.Normal, p3.Normal)) - p2.D * Vector3.Cross(p3.Normal, p1.Normal) / Vector3.Dot(p2.Normal, Vector3.Cross(p3.Normal, p1.Normal)) - p3.D * Vector3.Cross(p1.Normal, p2.Normal) / Vector3.Dot(p3.Normal, Vector3.Cross(p1.Normal, p2.Normal));
	}

	public static BoundingFrustum FromCamera(Vector3 cameraPos, Vector3 lookDir, Vector3 upDir, float fov, float znear, float zfar, float aspect)
	{
		lookDir = Vector3.Normalize(lookDir);
		upDir = Vector3.Normalize(upDir);
		Vector3 vector = cameraPos + lookDir * znear;
		Vector3 vector2 = cameraPos + lookDir * zfar;
		float num = (float)((double)znear * Math.Tan(fov / 2f));
		float num2 = (float)((double)zfar * Math.Tan(fov / 2f));
		float num3 = num * aspect;
		float num4 = num2 * aspect;
		Vector3 vector3 = Vector3.Normalize(Vector3.Cross(upDir, lookDir));
		Vector3 vector4 = vector - num * upDir + num3 * vector3;
		Vector3 vector5 = vector + num * upDir + num3 * vector3;
		Vector3 vector6 = vector + num * upDir - num3 * vector3;
		Vector3 point = vector - num * upDir - num3 * vector3;
		Vector3 vector7 = vector2 - num2 * upDir + num4 * vector3;
		Vector3 point2 = vector2 + num2 * upDir + num4 * vector3;
		Vector3 vector8 = vector2 + num2 * upDir - num4 * vector3;
		Vector3 point3 = vector2 - num2 * upDir - num4 * vector3;
		BoundingFrustum result = default(BoundingFrustum);
		result.pNear = new Plane(vector4, vector5, vector6);
		result.pFar = new Plane(vector8, point2, vector7);
		result.pLeft = new Plane(point, vector6, vector8);
		result.pRight = new Plane(vector7, point2, vector5);
		result.pTop = new Plane(vector5, point2, vector8);
		result.pBottom = new Plane(point3, vector7, vector4);
		result.pNear.Normalize();
		result.pFar.Normalize();
		result.pLeft.Normalize();
		result.pRight.Normalize();
		result.pTop.Normalize();
		result.pBottom.Normalize();
		result.pMatrix = Matrix.LookAtLH(cameraPos, cameraPos + lookDir * 10f, upDir) * Matrix.PerspectiveFovLH(fov, aspect, znear, zfar);
		return result;
	}

	public static BoundingFrustum FromCamera(FrustumCameraParams cameraParams)
	{
		return FromCamera(cameraParams.Position, cameraParams.LookAtDir, cameraParams.UpDir, cameraParams.FOV, cameraParams.ZNear, cameraParams.ZFar, cameraParams.AspectRatio);
	}

	public Vector3[] GetCorners()
	{
		Vector3[] array = new Vector3[8];
		GetCorners(array);
		return array;
	}

	public void GetCorners(Vector3[] corners)
	{
		ref Vector3 reference = ref corners[0];
		reference = Get3PlanesInterPoint(ref pNear, ref pBottom, ref pRight);
		ref Vector3 reference2 = ref corners[1];
		reference2 = Get3PlanesInterPoint(ref pNear, ref pTop, ref pRight);
		ref Vector3 reference3 = ref corners[2];
		reference3 = Get3PlanesInterPoint(ref pNear, ref pTop, ref pLeft);
		ref Vector3 reference4 = ref corners[3];
		reference4 = Get3PlanesInterPoint(ref pNear, ref pBottom, ref pLeft);
		ref Vector3 reference5 = ref corners[4];
		reference5 = Get3PlanesInterPoint(ref pFar, ref pBottom, ref pRight);
		ref Vector3 reference6 = ref corners[5];
		reference6 = Get3PlanesInterPoint(ref pFar, ref pTop, ref pRight);
		ref Vector3 reference7 = ref corners[6];
		reference7 = Get3PlanesInterPoint(ref pFar, ref pTop, ref pLeft);
		ref Vector3 reference8 = ref corners[7];
		reference8 = Get3PlanesInterPoint(ref pFar, ref pBottom, ref pLeft);
	}

	public FrustumCameraParams GetCameraParams()
	{
		Vector3[] corners = GetCorners();
		FrustumCameraParams result = default(FrustumCameraParams);
		result.Position = Get3PlanesInterPoint(ref pRight, ref pTop, ref pLeft);
		result.LookAtDir = pNear.Normal;
		result.UpDir = Vector3.Normalize(Vector3.Cross(pRight.Normal, pNear.Normal));
		result.FOV = (float)((Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pTop.Normal))) * 2.0);
		result.AspectRatio = (corners[6] - corners[5]).Length() / (corners[4] - corners[5]).Length();
		result.ZNear = (result.Position + pNear.Normal * pNear.D).Length();
		result.ZFar = (result.Position + pFar.Normal * pFar.D).Length();
		return result;
	}

	public ContainmentType Contains(ref Vector3 point)
	{
		PlaneIntersectionType planeIntersectionType = PlaneIntersectionType.Front;
		PlaneIntersectionType planeIntersectionType2 = PlaneIntersectionType.Front;
		for (int i = 0; i < 6; i++)
		{
			switch (i)
			{
			case 0:
				planeIntersectionType2 = pNear.Intersects(ref point);
				break;
			case 1:
				planeIntersectionType2 = pFar.Intersects(ref point);
				break;
			case 2:
				planeIntersectionType2 = pLeft.Intersects(ref point);
				break;
			case 3:
				planeIntersectionType2 = pRight.Intersects(ref point);
				break;
			case 4:
				planeIntersectionType2 = pTop.Intersects(ref point);
				break;
			case 5:
				planeIntersectionType2 = pBottom.Intersects(ref point);
				break;
			}
			switch (planeIntersectionType2)
			{
			case PlaneIntersectionType.Back:
				return ContainmentType.Disjoint;
			case PlaneIntersectionType.Intersecting:
				planeIntersectionType = PlaneIntersectionType.Intersecting;
				break;
			}
		}
		PlaneIntersectionType planeIntersectionType3 = planeIntersectionType;
		if (planeIntersectionType3 == PlaneIntersectionType.Intersecting)
		{
			return ContainmentType.Intersects;
		}
		return ContainmentType.Contains;
	}

	public ContainmentType Contains(Vector3 point)
	{
		return Contains(ref point);
	}

	public ContainmentType Contains(Vector3[] points)
	{
		throw new NotImplementedException();
	}

	public void Contains(Vector3[] points, out ContainmentType result)
	{
		result = Contains(points);
	}

	private void GetBoxToPlanePVertexNVertex(ref BoundingBox box, ref Vector3 planeNormal, out Vector3 p, out Vector3 n)
	{
		p = box.Minimum;
		if (planeNormal.X >= 0f)
		{
			p.X = box.Maximum.X;
		}
		if (planeNormal.Y >= 0f)
		{
			p.Y = box.Maximum.Y;
		}
		if (planeNormal.Z >= 0f)
		{
			p.Z = box.Maximum.Z;
		}
		n = box.Maximum;
		if (planeNormal.X >= 0f)
		{
			n.X = box.Minimum.X;
		}
		if (planeNormal.Y >= 0f)
		{
			n.Y = box.Minimum.Y;
		}
		if (planeNormal.Z >= 0f)
		{
			n.Z = box.Minimum.Z;
		}
	}

	public ContainmentType Contains(ref BoundingBox box)
	{
		ContainmentType result = ContainmentType.Contains;
		for (int i = 0; i < 6; i++)
		{
			Plane plane = GetPlane(i);
			GetBoxToPlanePVertexNVertex(ref box, ref plane.Normal, out var p, out var n);
			if (Collision.PlaneIntersectsPoint(ref plane, ref p) == PlaneIntersectionType.Back)
			{
				return ContainmentType.Disjoint;
			}
			if (Collision.PlaneIntersectsPoint(ref plane, ref n) == PlaneIntersectionType.Back)
			{
				result = ContainmentType.Intersects;
			}
		}
		return result;
	}

	public ContainmentType Contains(BoundingBox box)
	{
		return Contains(ref box);
	}

	public void Contains(ref BoundingBox box, out ContainmentType result)
	{
		result = Contains(ref box);
	}

	public ContainmentType Contains(ref BoundingSphere sphere)
	{
		PlaneIntersectionType planeIntersectionType = PlaneIntersectionType.Front;
		PlaneIntersectionType planeIntersectionType2 = PlaneIntersectionType.Front;
		for (int i = 0; i < 6; i++)
		{
			switch (i)
			{
			case 0:
				planeIntersectionType2 = pNear.Intersects(ref sphere);
				break;
			case 1:
				planeIntersectionType2 = pFar.Intersects(ref sphere);
				break;
			case 2:
				planeIntersectionType2 = pLeft.Intersects(ref sphere);
				break;
			case 3:
				planeIntersectionType2 = pRight.Intersects(ref sphere);
				break;
			case 4:
				planeIntersectionType2 = pTop.Intersects(ref sphere);
				break;
			case 5:
				planeIntersectionType2 = pBottom.Intersects(ref sphere);
				break;
			}
			switch (planeIntersectionType2)
			{
			case PlaneIntersectionType.Back:
				return ContainmentType.Disjoint;
			case PlaneIntersectionType.Intersecting:
				planeIntersectionType = PlaneIntersectionType.Intersecting;
				break;
			}
		}
		PlaneIntersectionType planeIntersectionType3 = planeIntersectionType;
		if (planeIntersectionType3 == PlaneIntersectionType.Intersecting)
		{
			return ContainmentType.Intersects;
		}
		return ContainmentType.Contains;
	}

	public ContainmentType Contains(BoundingSphere sphere)
	{
		return Contains(ref sphere);
	}

	public void Contains(ref BoundingSphere sphere, out ContainmentType result)
	{
		result = Contains(ref sphere);
	}

	public bool Contains(ref BoundingFrustum frustum)
	{
		return Contains(frustum.GetCorners()) != ContainmentType.Disjoint;
	}

	public bool Contains(BoundingFrustum frustum)
	{
		return Contains(ref frustum);
	}

	public void Contains(ref BoundingFrustum frustum, out bool result)
	{
		result = Contains(frustum.GetCorners()) != ContainmentType.Disjoint;
	}

	public bool Intersects(ref BoundingSphere sphere)
	{
		return Contains(ref sphere) != ContainmentType.Disjoint;
	}

	public void Intersects(ref BoundingSphere sphere, out bool result)
	{
		result = Contains(ref sphere) != ContainmentType.Disjoint;
	}

	public bool Intersects(ref BoundingBox box)
	{
		return Contains(ref box) != ContainmentType.Disjoint;
	}

	public void Intersects(ref BoundingBox box, out bool result)
	{
		result = Contains(ref box) != ContainmentType.Disjoint;
	}

	private PlaneIntersectionType PlaneIntersectsPoints(ref Plane plane, Vector3[] points)
	{
		PlaneIntersectionType planeIntersectionType = Collision.PlaneIntersectsPoint(ref plane, ref points[0]);
		for (int i = 1; i < points.Length; i++)
		{
			if (Collision.PlaneIntersectsPoint(ref plane, ref points[i]) != planeIntersectionType)
			{
				return PlaneIntersectionType.Intersecting;
			}
		}
		return planeIntersectionType;
	}

	public PlaneIntersectionType Intersects(ref Plane plane)
	{
		return PlaneIntersectsPoints(ref plane, GetCorners());
	}

	public void Intersects(ref Plane plane, out PlaneIntersectionType result)
	{
		result = PlaneIntersectsPoints(ref plane, GetCorners());
	}

	public float GetWidthAtDepth(float depth)
	{
		float num = (float)(Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pLeft.Normal)));
		return (float)(Math.Tan(num) * (double)depth * 2.0);
	}

	public float GetHeightAtDepth(float depth)
	{
		float num = (float)(Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pTop.Normal)));
		return (float)(Math.Tan(num) * (double)depth * 2.0);
	}

	private BoundingFrustum GetInsideOutClone()
	{
		BoundingFrustum result = this;
		result.pNear.Normal = -result.pNear.Normal;
		result.pFar.Normal = -result.pFar.Normal;
		result.pLeft.Normal = -result.pLeft.Normal;
		result.pRight.Normal = -result.pRight.Normal;
		result.pTop.Normal = -result.pTop.Normal;
		result.pBottom.Normal = -result.pBottom.Normal;
		return result;
	}

	public bool Intersects(ref Ray ray)
	{
		float? inDistance;
		float? outDistance;
		return Intersects(ref ray, out inDistance, out outDistance);
	}

	public bool Intersects(ref Ray ray, out float? inDistance, out float? outDistance)
	{
		if (Contains(ray.Position) != ContainmentType.Disjoint)
		{
			float num = float.MaxValue;
			for (int i = 0; i < 6; i++)
			{
				Plane plane = GetPlane(i);
				if (Collision.RayIntersectsPlane(ref ray, ref plane, out float distance) && distance < num)
				{
					num = distance;
				}
			}
			inDistance = num;
			outDistance = null;
			return true;
		}
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		for (int j = 0; j < 6; j++)
		{
			Plane plane2 = GetPlane(j);
			if (Collision.RayIntersectsPlane(ref ray, ref plane2, out float distance2))
			{
				num2 = Math.Min(num2, distance2);
				num3 = Math.Max(num3, distance2);
			}
		}
		Vector3 vector = ray.Position + ray.Direction * num2;
		Vector3 vector2 = ray.Position + ray.Direction * num3;
		Vector3 point = (vector + vector2) / 2f;
		if (Contains(ref point) != ContainmentType.Disjoint)
		{
			inDistance = num2;
			outDistance = num3;
			return true;
		}
		inDistance = null;
		outDistance = null;
		return false;
	}

	public float GetZoomToExtentsShiftDistance(Vector3[] points)
	{
		float num = (float)(Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pTop.Normal)));
		float num2 = (float)Math.Sin(num);
		float num3 = (float)(Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pLeft.Normal)));
		float num4 = (float)Math.Sin(num3);
		float num5 = num2 / num4;
		BoundingFrustum insideOutClone = GetInsideOutClone();
		float num6 = float.MinValue;
		for (int i = 0; i < points.Length; i++)
		{
			float val = Collision.DistancePlanePoint(ref insideOutClone.pTop, ref points[i]);
			val = Math.Max(val, Collision.DistancePlanePoint(ref insideOutClone.pBottom, ref points[i]));
			val = Math.Max(val, Collision.DistancePlanePoint(ref insideOutClone.pLeft, ref points[i]) * num5);
			val = Math.Max(val, Collision.DistancePlanePoint(ref insideOutClone.pRight, ref points[i]) * num5);
			num6 = Math.Max(num6, val);
		}
		return (0f - num6) / num2;
	}

	public float GetZoomToExtentsShiftDistance(ref BoundingBox boundingBox)
	{
		return GetZoomToExtentsShiftDistance(boundingBox.GetCorners());
	}

	public Vector3 GetZoomToExtentsShiftVector(Vector3[] points)
	{
		return GetZoomToExtentsShiftDistance(points) * pNear.Normal;
	}

	public Vector3 GetZoomToExtentsShiftVector(ref BoundingBox boundingBox)
	{
		return GetZoomToExtentsShiftDistance(boundingBox.GetCorners()) * pNear.Normal;
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref pMatrix);
		serializer.Serialize(ref pNear);
		serializer.Serialize(ref pFar);
		serializer.Serialize(ref pLeft);
		serializer.Serialize(ref pRight);
		serializer.Serialize(ref pTop);
		serializer.Serialize(ref pBottom);
	}
}
