using System.Windows.Forms;

namespace Sce.Atf.Applications;

public interface IControlHostClient
{
	void Activate(Control control);

	void Deactivate(Control control);

	bool Close(Control control);
}
