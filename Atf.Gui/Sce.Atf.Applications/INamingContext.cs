namespace Sce.Atf.Applications;

public interface INamingContext
{
	string GetName(object item);

	bool CanSetName(object item);

	void SetName(object item, string name);
}
