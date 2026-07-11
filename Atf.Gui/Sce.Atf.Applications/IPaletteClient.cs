namespace Sce.Atf.Applications;

public interface IPaletteClient
{
	void GetInfo(object item, ItemInfo info);

	object Convert(object item);
}
