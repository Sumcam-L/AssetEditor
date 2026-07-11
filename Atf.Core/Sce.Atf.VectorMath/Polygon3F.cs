using System;
using System.Collections.Generic;
using System.Text;

namespace Sce.Atf.VectorMath;

public class Polygon3F : IFormattable
{
	private class TriVx
	{
		public readonly Vec3F V;

		public bool IsEar;

		public bool IsReflex;

		public TriVx(Vec3F v)
		{
			V = v;
		}
	}

	public Vec3F[] Vertices;

	public Polygon3F(Vec3F[] vertices)
	{
		Vertices = new Vec3F[vertices.Length];
		vertices.CopyTo(Vertices, 0);
	}

	public IList<Triangle3F> Triangulate(Vec3F normal)
	{
		List<Triangle3F> list = new List<Triangle3F>();
		LinkedList<TriVx> linkedList = new LinkedList<TriVx>();
		Vec3F[] vertices = Vertices;
		foreach (Vec3F v in vertices)
		{
			linkedList.AddLast(new TriVx(v));
		}
		if (linkedList.Count < 3)
		{
			throw new InvalidOperationException("Polygon has less than 3 vertices");
		}
		List<LinkedListNode<TriVx>> list2 = new List<LinkedListNode<TriVx>>();
		if (linkedList.Count == 3)
		{
			linkedList.First.Value.IsEar = true;
			list2.Add(linkedList.First);
		}
		else
		{
			LinkedListNode<TriVx> linkedListNode = null;
			for (linkedListNode = linkedList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				linkedListNode.Value.IsReflex = IsReflex(linkedListNode, normal);
			}
			for (linkedListNode = linkedList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				if (!linkedListNode.Value.IsReflex && IsEar(linkedListNode, normal))
				{
					linkedListNode.Value.IsEar = true;
					list2.Add(linkedListNode);
				}
			}
		}
		while (linkedList.Count > 0 && list2.Count > 0)
		{
			LinkedListNode<TriVx> linkedListNode2 = list2[0];
			LinkedListNode<TriVx> linkedListNode3 = PrevNode(linkedListNode2);
			LinkedListNode<TriVx> linkedListNode4 = NextNode(linkedListNode2);
			list.Add(new Triangle3F(linkedListNode3.Value.V, linkedListNode2.Value.V, linkedListNode4.Value.V));
			if (linkedList.Count == 3)
			{
				linkedList.Clear();
				continue;
			}
			list2.Remove(linkedListNode2);
			linkedList.Remove(linkedListNode2);
			if (linkedListNode3.Value.IsReflex)
			{
				linkedListNode3.Value.IsReflex = IsReflex(linkedListNode3, normal);
			}
			if (!linkedListNode3.Value.IsReflex)
			{
				UpdateEar(linkedListNode3, list2, normal);
			}
			if (linkedListNode4.Value.IsReflex)
			{
				linkedListNode4.Value.IsReflex = IsReflex(linkedListNode4, normal);
			}
			if (!linkedListNode4.Value.IsReflex)
			{
				UpdateEar(linkedListNode4, list2, normal);
			}
		}
		return list;
	}

	private bool IsEar(LinkedListNode<TriVx> node, Vec3F normal)
	{
		bool result = true;
		TriVx value = PrevNode(node).Value;
		TriVx value2 = node.Value;
		TriVx value3 = NextNode(node).Value;
		foreach (TriVx item in node.List)
		{
			if (item.IsReflex && item != value && item != value2 && item != value3 && IsLeftOf(value, value2, item, normal) && IsLeftOf(value2, value3, item, normal) && IsLeftOf(value3, value, item, normal))
			{
				result = false;
			}
		}
		return result;
	}

	private void UpdateEar(LinkedListNode<TriVx> node, List<LinkedListNode<TriVx>> ears, Vec3F normal)
	{
		if (IsEar(node, normal))
		{
			if (!node.Value.IsEar)
			{
				ears.Add(node);
				node.Value.IsEar = true;
			}
		}
		else if (node.Value.IsEar)
		{
			ears.Remove(node);
			node.Value.IsEar = false;
		}
	}

	private bool IsReflex(LinkedListNode<TriVx> node, Vec3F normal)
	{
		TriVx value = PrevNode(node).Value;
		TriVx value2 = node.Value;
		TriVx value3 = NextNode(node).Value;
		return !IsLeftOf(value, value2, value3, normal);
	}

	private bool IsLeftOf(TriVx v0, TriVx v1, TriVx v2, Vec3F normal)
	{
		Vec3F u = Vec3F.Cross(v1.V - v0.V, v2.V - v0.V);
		return Vec3F.Dot(u, normal) > 0f;
	}

	private LinkedListNode<TriVx> NextNode(LinkedListNode<TriVx> node)
	{
		if (node.Next != null)
		{
			return node.Next;
		}
		return node.List.First;
	}

	private LinkedListNode<TriVx> PrevNode(LinkedListNode<TriVx> node)
	{
		if (node.Previous != null)
		{
			return node.Previous;
		}
		return node.List.Last;
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Vertices.Length.ToString("D", formatProvider));
		for (int i = 0; i < Vertices.Length; i++)
		{
			stringBuilder.Append(string.Format("({3} {0}{3} {1}{3} {2})", Vertices[i].X.ToString(format, formatProvider), Vertices[i].Y.ToString(format, formatProvider), Vertices[i].Z.ToString(format, formatProvider), numberListSeparator));
		}
		return stringBuilder.ToString();
	}
}
