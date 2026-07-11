using System.Windows.Forms;

namespace Sce.Atf.Applications;

public interface IDialogService
{
	DialogResult ShowParentedDialog(Form form);
}
