namespace Sce.Atf.Dom;

public abstract class AttributeRule
{
	public abstract bool Validate(object value, AttributeInfo info);
}
