using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Interop;

internal class ControlHostClientAdapter : Sce.Atf.Wpf.Applications.IControlHostClient
{
	private Sce.Atf.Applications.IControlHostClient m_adaptee;

	public ControlHostClientAdapter(Sce.Atf.Applications.IControlHostClient adaptee)
	{
		m_adaptee = adaptee;
	}

	public void Activate(object control)
	{
		m_adaptee.Activate((Control)control);
	}

	public void Deactivate(object control)
	{
		m_adaptee.Deactivate((Control)control);
	}

	public bool Close(object control, bool mainWindowClosing)
	{
		return m_adaptee.Close((Control)control);
	}
}
