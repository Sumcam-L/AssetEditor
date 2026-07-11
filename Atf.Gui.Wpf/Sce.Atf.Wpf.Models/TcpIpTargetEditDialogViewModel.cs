using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

public class TcpIpTargetEditDialogViewModel : DialogViewModelBase
{
	public TcpIpTarget Target { get; private set; }

	public TcpIpTargetEditDialogViewModel(TcpIpTarget target)
	{
		base.Title = "Edit TCP/IP Target".Localize();
		Target = target;
	}
}
