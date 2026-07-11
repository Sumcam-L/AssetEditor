using System.Windows.Forms;

namespace Firaxis.Controls;

public interface INotifyPopupView : IToolTipView
{
	event MouseEventHandler ClickedNotification;

	void Dismiss();
}
