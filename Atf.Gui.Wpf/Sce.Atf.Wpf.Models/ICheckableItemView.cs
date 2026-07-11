namespace Sce.Atf.Wpf.Models;

public interface ICheckableItemView
{
	bool? GetIsChecked(object item);

	void SetIsChecked(object item, bool? value);
}
