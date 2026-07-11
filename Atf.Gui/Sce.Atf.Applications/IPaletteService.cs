namespace Sce.Atf.Applications;

public interface IPaletteService
{
	void AddItem(object item, string categoryName, IPaletteClient client);

	void RemoveItem(object item);
}
