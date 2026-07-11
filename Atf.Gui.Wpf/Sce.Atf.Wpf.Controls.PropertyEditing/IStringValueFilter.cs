namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public interface IStringValueFilter
{
	string FilterStringValue(object instance, string stringValue, string token);
}
