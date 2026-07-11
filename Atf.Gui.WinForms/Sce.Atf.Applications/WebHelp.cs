using System.Diagnostics;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class WebHelp
{
	internal static bool SupressHelpRequests;

	public string Url { get; set; }

	public WebHelp(Control control)
	{
		control.HelpRequested += ControlHelpRequested;
	}

	public void OpenUrl()
	{
		Process.Start(Url);
	}

	private void ControlHelpRequested(object sender, HelpEventArgs helpEvent)
	{
		if (!SupressHelpRequests && !string.IsNullOrEmpty(Url))
		{
			helpEvent.Handled = true;
			OpenUrl();
		}
	}
}
