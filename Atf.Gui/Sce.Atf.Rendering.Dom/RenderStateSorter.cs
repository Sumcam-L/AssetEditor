using System;
using System.Collections;
using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public class RenderStateSorter : ICollection<TraverseNode>, IEnumerable<TraverseNode>, IEnumerable
{
	private class RenderBucket
	{
		public readonly RenderMode PassBits;

		public readonly RenderMode NoPassBits;

		private readonly List<TraverseNode> m_nodes = new List<TraverseNode>();

		private bool m_sorted;

		public List<TraverseNode> Nodes => m_nodes;

		public bool Sorted
		{
			get
			{
				return m_sorted;
			}
			set
			{
				m_sorted = value;
			}
		}

		public RenderBucket(RenderMode passBits, RenderMode noPassBits)
		{
			PassBits = passBits;
			NoPassBits = noPassBits;
		}

		public bool TryAdd(TraverseNode node)
		{
			bool flag = (node.RenderState.RenderMode & PassBits) == PassBits;
			bool flag2 = (node.RenderState.RenderMode & NoPassBits) == 0;
			if (flag && flag2)
			{
				m_nodes.Add(node);
				m_sorted = false;
				return true;
			}
			return false;
		}
	}

	private readonly List<RenderBucket> m_buckets;

	private readonly Matrix4F m_viewMatrix = new Matrix4F();

	public int Count
	{
		get
		{
			int num = 0;
			foreach (RenderBucket bucket in m_buckets)
			{
				num += bucket.Nodes.Count;
			}
			return num;
		}
	}

	public bool IsReadOnly => false;

	public RenderStateSorter()
	{
		m_buckets = new List<RenderBucket>();
		m_buckets.Add(new RenderBucket(RenderMode.Wireframe, RenderMode.Smooth | RenderMode.DisableZBuffer | RenderMode.Alpha));
		m_buckets.Add(new RenderBucket(RenderMode.Smooth, RenderMode.Textured | RenderMode.DisableZBuffer | RenderMode.Alpha));
		m_buckets.Add(new RenderBucket(RenderMode.Smooth | RenderMode.Textured, RenderMode.DisableZBuffer | RenderMode.Alpha));
		m_buckets.Add(new RenderBucket(RenderMode.Alpha, RenderMode.DisableZBuffer));
		m_buckets.Add(new RenderBucket(RenderMode.DisableZBuffer, (RenderMode)0));
	}

	public void Add(TraverseNode item)
	{
		foreach (RenderBucket bucket in m_buckets)
		{
			if (bucket.TryAdd(item))
			{
				break;
			}
		}
	}

	public void Clear()
	{
		foreach (RenderBucket bucket in m_buckets)
		{
			bucket.Nodes.Clear();
		}
	}

	public bool Contains(TraverseNode item)
	{
		throw new NotImplementedException("RenderStateSorter.Contains() is not implemented and would be expensive to do so.");
	}

	public void CopyTo(TraverseNode[] array, int arrayIndex)
	{
		using IEnumerator<TraverseNode> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TraverseNode current = enumerator.Current;
			array[arrayIndex++] = current;
		}
	}

	public bool Remove(TraverseNode item)
	{
		throw new NotImplementedException("RenderStateSorter.Remove() is not implemented and would be expensive to do so.");
	}

	public IEnumerator<TraverseNode> GetEnumerator()
	{
		return GetEnumeratorInternal();
	}

	private IEnumerator<TraverseNode> GetEnumeratorInternal()
	{
		foreach (RenderBucket bucket in m_buckets)
		{
			if (bucket.Nodes.Count == 0)
			{
				continue;
			}
			if (!bucket.Sorted)
			{
				if ((bucket.PassBits & RenderMode.Textured) != 0)
				{
					TraverseSortUtils.SortByTextureName(bucket.Nodes);
				}
				else if ((bucket.PassBits & RenderMode.Alpha) != 0)
				{
					TraverseSortUtils.SortByCameraSpaceDepth(bucket.Nodes, m_viewMatrix);
				}
				else
				{
					TraverseSortUtils.SortByRenderMode(bucket.Nodes);
				}
				bucket.Sorted = true;
			}
			foreach (TraverseNode node in bucket.Nodes)
			{
				yield return node;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumeratorInternal();
	}

	public void SetViewMatrix(Matrix4F viewMatrix)
	{
		m_viewMatrix.Set(viewMatrix);
	}
}
