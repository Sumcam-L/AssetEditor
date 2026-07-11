namespace Sce.Atf;

public interface IPropertyValueValidator
{
	bool Validate(string propertyName, object formattedValue, out string errorMessage);
}
