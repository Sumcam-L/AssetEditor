using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class ChildCountRule : ChildRule
{
	private readonly int m_min;

	private readonly int m_max;

	public int Min => m_min;

	public int Max => m_max;

	public ChildCountRule(int min, int max)
	{
		m_min = min;
		m_max = max;
	}

	public override bool Validate(DomNode parent, DomNode child, ChildInfo childInfo)
	{
		if (childInfo.IsList)
		{
			IList<DomNode> childList = parent.GetChildList(childInfo);
			int count = childList.Count;
			return count >= m_min && count <= m_max;
		}
		return true;
	}
}
