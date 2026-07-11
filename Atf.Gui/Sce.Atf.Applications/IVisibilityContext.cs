namespace Sce.Atf.Applications;

public interface IVisibilityContext
{
	bool IsVisible(object item);

	bool CanSetVisible(object item);

	void SetVisible(object item, bool value);
}
