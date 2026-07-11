namespace Sce.Atf.Wpf.Models;

public class PaletteItem : NotifyPropertyChangedBase
{
	public object Item { get; private set; }

	public string Category { get; private set; }

	public PaletteItem(object item, string categoryName)
	{
		Item = item;
		Category = categoryName;
	}
}
