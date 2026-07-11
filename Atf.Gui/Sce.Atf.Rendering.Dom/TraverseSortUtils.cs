using System;
using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public static class TraverseSortUtils
{
	private class TextureNameComparer : IComparer<TraverseNode>
	{
		public int Compare(TraverseNode x, TraverseNode y)
		{
			if (x.RenderState.TextureName < y.RenderState.TextureName)
			{
				return -1;
			}
			if (x.RenderState.TextureName > y.RenderState.TextureName)
			{
				return 1;
			}
			return 0;
		}
	}

	private class CamSpaceDistanceComparer : IComparer<KeyValuePair<float, TraverseNode>>
	{
		public int Compare(KeyValuePair<float, TraverseNode> x, KeyValuePair<float, TraverseNode> y)
		{
			if (x.Key < y.Key)
			{
				return -1;
			}
			if (x.Key > y.Key)
			{
				return 1;
			}
			return 0;
		}
	}

	private class RenderModeComparer : IComparer<TraverseNode>
	{
		public int Compare(TraverseNode x, TraverseNode y)
		{
			if (x.RenderState.RenderMode < y.RenderState.RenderMode)
			{
				return -1;
			}
			if (x.RenderState.RenderMode > y.RenderState.RenderMode)
			{
				return -1;
			}
			return 0;
		}
	}

	public static void SortByTextureName(List<TraverseNode> list)
	{
		list.Sort(new TextureNameComparer());
	}

	public static void SortByCameraSpaceDepth(List<TraverseNode> list, Matrix4F viewMatrix)
	{
		KeyValuePair<float, TraverseNode>[] array = new KeyValuePair<float, TraverseNode>[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			TraverseNode traverseNode = list[i];
			Vec3F centroid = traverseNode.WorldSpaceBoundingBox.Centroid;
			viewMatrix.Transform(centroid, out var result);
			array[i] = new KeyValuePair<float, TraverseNode>(result.Z, traverseNode);
		}
		Array.Sort(array, new CamSpaceDistanceComparer());
		list.Clear();
		KeyValuePair<float, TraverseNode>[] array2 = array;
		foreach (KeyValuePair<float, TraverseNode> keyValuePair in array2)
		{
			list.Add(keyValuePair.Value);
		}
	}

	public static void SortByRenderMode(List<TraverseNode> list)
	{
		list.Sort(new RenderModeComparer());
	}
}
