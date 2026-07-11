using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class DomNodeQueryRoot : QueryRoot
{
	public override IQueryPredicate GetPredicate()
	{
		DomNodePropertyPredicate domNodePropertyPredicate = new DomNodePropertyPredicate();
		BuildPredicate(domNodePropertyPredicate);
		return domNodePropertyPredicate;
	}
}
