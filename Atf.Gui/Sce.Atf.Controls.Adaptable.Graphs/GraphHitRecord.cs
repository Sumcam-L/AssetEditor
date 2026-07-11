using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class GraphHitRecord<TNode, TEdge, TEdgeRoute> : DiagramHitRecord where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	public readonly TNode Node;

	public readonly TEdge Edge;

	public readonly TEdgeRoute FromRoute;

	public readonly TEdgeRoute ToRoute;

	private IEnumerable<TNode> m_pathInversed;

	public IEnumerable<TNode> HitPathInversed
	{
		get
		{
			return m_pathInversed;
		}
		set
		{
			m_pathInversed = value;
			if (value != null)
			{
				base.HitPath = new AdaptablePath<object>(HitPathInversed.Reverse());
			}
			else
			{
				base.HitPath = null;
			}
		}
	}

	public TNode SubNode => base.SubItem.As<TNode>();

	public PointF FromRoutePos { get; set; }

	public PointF ToRoutePos { get; set; }

	public GraphHitRecord()
	{
	}

	public GraphHitRecord(TNode node, object part)
	{
		Node = node;
		base.Item = node;
		base.Part = part;
	}

	public GraphHitRecord(TEdge edge, object part)
	{
		Edge = edge;
		base.Item = edge;
		base.Part = part;
	}

	public GraphHitRecord(TEdgeRoute edgeRoute, object part)
	{
		base.Item = edgeRoute;
		FromRoute = edgeRoute;
		ToRoute = edgeRoute;
		base.Part = part;
	}

	public GraphHitRecord(TNode node, TEdge edge, TEdgeRoute fromRoute, TEdgeRoute toRoute)
	{
		Node = node;
		Edge = edge;
		FromRoute = fromRoute;
		ToRoute = toRoute;
		if (Node != null)
		{
			base.Item = Node;
			if (FromRoute != null)
			{
				base.Part = FromRoute;
			}
			else if (ToRoute != null)
			{
				base.Part = ToRoute;
			}
		}
		else if (Edge != null)
		{
			base.Item = Edge;
		}
	}
}
