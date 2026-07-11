using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class PinTarget
{
	private readonly DomNode m_leafDomNode;

	private readonly int m_leafPinIndex;

	private DomNode m_instancingNode;

	public DomNode LeafDomNode => m_leafDomNode;

	public int LeafPinIndex => m_leafPinIndex;

	public DomNode InstancingNode
	{
		get
		{
			return m_instancingNode;
		}
		set
		{
			m_instancingNode = value;
		}
	}

	public PinTarget(DomNode targetDomNode, int targetPinIndex, DomNode referencingDomNode)
	{
		m_leafDomNode = targetDomNode;
		m_leafPinIndex = targetPinIndex;
		m_instancingNode = referencingDomNode;
	}

	public override int GetHashCode()
	{
		int hashCode = LeafDomNode.GetHashCode();
		int hashCode2 = LeafPinIndex.GetHashCode();
		return hashCode ^ hashCode2;
	}

	public override bool Equals(object other)
	{
		bool result = false;
		if (other is PinTarget)
		{
			result = Equals((PinTarget)other);
		}
		return result;
	}

	public static bool operator ==(PinTarget lhs, PinTarget rhs)
	{
		if (object.Equals(lhs, null))
		{
			return object.Equals(rhs, null);
		}
		return lhs.Equals(rhs);
	}

	public static bool operator !=(PinTarget lhs, PinTarget rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(PinTarget other)
	{
		if (other == null)
		{
			return false;
		}
		return LeafDomNode == other.LeafDomNode && LeafPinIndex == other.LeafPinIndex;
	}

	public bool FullyEquals(PinTarget other)
	{
		if (other == null)
		{
			return false;
		}
		return LeafDomNode == other.LeafDomNode && LeafPinIndex == other.LeafPinIndex && InstancingNode == other.InstancingNode;
	}
}
