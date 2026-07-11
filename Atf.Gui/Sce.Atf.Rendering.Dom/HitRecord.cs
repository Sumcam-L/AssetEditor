using System;
using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public class HitRecord
{
	private class HitRecordComparer : IComparer<HitRecord>
	{
		public Vec3F Eye;

		public int Compare(HitRecord a, HitRecord b)
		{
			if (a == b)
			{
				return 0;
			}
			float lengthSquared = (a.WorldIntersection - Eye).LengthSquared;
			float lengthSquared2 = (b.WorldIntersection - Eye).LengthSquared;
			if (lengthSquared < lengthSquared2)
			{
				return -1;
			}
			if (lengthSquared > lengthSquared2)
			{
				return 1;
			}
			return 0;
		}
	}

	private readonly SceneNode[] m_graphPath;

	private readonly IRenderObject m_renderObject;

	private readonly Matrix4F m_transform;

	private readonly uint[] m_renderObjectData;

	private bool m_useIntersectionPt;

	private Vec3F m_intersectionPt;

	private bool m_hasNormal;

	private Vec3F m_normal;

	private bool m_hasNearestVert;

	private Vec3F m_nearestVert;

	private static readonly HitRecordComparer s_comparer = new HitRecordComparer();

	public SceneNode[] GraphPath => m_graphPath;

	public IRenderObject RenderObject => m_renderObject;

	public Matrix4F Transform => m_transform;

	public uint[] RenderObjectData => m_renderObjectData;

	public bool HasWorldIntersection => m_useIntersectionPt;

	public Vec3F WorldIntersection
	{
		get
		{
			if (!m_useIntersectionPt)
			{
				throw new InvalidOperationException("The world intersection point is not available.");
			}
			return m_intersectionPt;
		}
		set
		{
			m_useIntersectionPt = true;
			m_intersectionPt = value;
		}
	}

	public bool HasNormal => m_hasNormal;

	public Vec3F Normal
	{
		get
		{
			if (!m_hasNormal)
			{
				throw new InvalidOperationException("The surface normal has not been set.");
			}
			return m_normal;
		}
		set
		{
			m_hasNormal = true;
			m_normal = value;
		}
	}

	public bool HasNearestVert => m_hasNearestVert;

	public Vec3F NearestVert
	{
		get
		{
			if (!m_hasNearestVert)
			{
				throw new InvalidOperationException("The nearest vertex has not been set.");
			}
			return m_nearestVert;
		}
		set
		{
			m_hasNearestVert = true;
			m_nearestVert = value;
		}
	}

	public HitRecord(SceneNode[] graphPath, IRenderObject renderObject, Matrix4F transform, uint[] renderObjectData)
	{
		m_graphPath = graphPath;
		m_renderObject = renderObject;
		m_transform = transform;
		m_renderObjectData = renderObjectData;
		m_useIntersectionPt = false;
	}

	public static void Sort(HitRecord[] hits, Vec3F eye)
	{
		s_comparer.Eye = eye;
		Array.Sort(hits, s_comparer);
	}
}
