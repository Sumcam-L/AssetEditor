using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls;

public interface IDialogSite
{
	Panel Site { get; }

	void ShowSite();

	void HideSite();
}
