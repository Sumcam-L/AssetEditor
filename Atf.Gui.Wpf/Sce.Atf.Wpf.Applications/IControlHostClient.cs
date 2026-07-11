namespace Sce.Atf.Wpf.Applications;

public interface IControlHostClient
{
	void Activate(object control);

	void Deactivate(object control);

	bool Close(object control, bool mainWindowClosing);
}
