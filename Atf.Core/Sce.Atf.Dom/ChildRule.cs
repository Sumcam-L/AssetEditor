namespace Sce.Atf.Dom;

public abstract class ChildRule
{
	public abstract bool Validate(DomNode parent, DomNode child, ChildInfo childInfo);
}
