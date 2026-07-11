using System.Windows;
using System.Windows.Forms;

namespace Firaxis.ATF;

public interface IDialogHostService
{
	bool? ShowDialog(Window window);

	DialogResult ShowDialog(Form window);
}
