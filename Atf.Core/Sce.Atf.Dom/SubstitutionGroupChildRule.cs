using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Dom;

internal class SubstitutionGroupChildRule : ChildRule
{
	private readonly ChildInfo[] m_substitutions;

	public IEnumerable<ChildInfo> Substitutions => m_substitutions;

	public SubstitutionGroupChildRule(ChildInfo[] substitutions)
	{
		m_substitutions = substitutions;
	}

	public override bool Validate(DomNode parent, DomNode child, ChildInfo childInfo)
	{
		return m_substitutions.Any((ChildInfo x) => x.Type == child.Type);
	}
}
