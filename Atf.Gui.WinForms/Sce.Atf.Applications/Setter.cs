namespace Sce.Atf.Applications;

public class Setter
{
	public readonly string PropertyName;

	public readonly ValueInfo ValueInfo;

	public readonly ListInfo ListInfo;

	public Setter(string propertyName, ValueInfo valueInfo)
	{
		PropertyName = propertyName;
		ValueInfo = valueInfo;
	}

	public Setter(string propertyName, ListInfo listInfo)
	{
		PropertyName = propertyName;
		ListInfo = listInfo;
	}

	public override string ToString()
	{
		return "PropertyName = " + PropertyName;
	}
}
