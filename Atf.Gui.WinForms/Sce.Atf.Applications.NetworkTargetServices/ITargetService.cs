using System.Windows.Forms;

namespace Sce.Atf.Applications.NetworkTargetServices;

public interface ITargetService
{
	bool SingleSelectionMode { get; set; }

	int DefaultPortNumber { get; set; }

	bool CanEditPortNumber { get; set; }

	Target[] GetAllTargets();

	Target[] GetSelectedTargets();

	Target GetSelectedTarget();

	DialogResult ShowTargetDialog();

	void SetProtocols(string[] protocols);

	void SelectTarget(string name);

	void AddTarget(string name, string host, int port);
}
