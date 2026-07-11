using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public static class Graphs
{
	public static IEnumerable<TEdge> GetEdges<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node) where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
	{
		foreach (TEdge edge in graph.Edges)
		{
			if (edge.FromNode == node || edge.ToNode == node)
			{
				yield return edge;
			}
		}
	}

	public static IEnumerable<TEdge> GetOutputEdges<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node) where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
	{
		foreach (TEdge edge in graph.GetEdges(node))
		{
			if (edge.FromNode == node)
			{
				yield return edge;
			}
		}
	}

	public static IEnumerable<TEdge> GetInputEdges<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node) where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
	{
		foreach (TEdge edge in graph.GetEdges(node))
		{
			if (edge.ToNode == node)
			{
				yield return edge;
			}
		}
	}

	public static IEnumerable<TNode> GetOutputNodes<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node) where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
	{
		foreach (TEdge edge in graph.GetEdges(node))
		{
			if (edge.FromNode == node)
			{
				yield return edge.ToNode;
			}
		}
	}

	public static IEnumerable<TNode> GetInputNodes<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node) where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
	{
		foreach (TEdge edge in graph.GetEdges(node))
		{
			if (edge.ToNode == node)
			{
				yield return edge.FromNode;
			}
		}
	}

	public static IEnumerable<TNode> GetNodes<TNode, TEdge, TEdgeRoute>(this IGraph<TNode, TEdge, TEdgeRoute> graph, TNode node) where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
	{
		foreach (TEdge edge in graph.GetEdges(node))
		{
			if (edge.FromNode == node)
			{
				yield return edge.ToNode;
			}
			else if (edge.ToNode == node)
			{
				yield return edge.FromNode;
			}
		}
	}
}
