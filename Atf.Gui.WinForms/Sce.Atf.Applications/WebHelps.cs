using System.Windows.Forms;

namespace Sce.Atf.Applications;

public static class WebHelps
{
	public static WebHelp AddHelp(this Control control, string url)
	{
		return new WebHelp(control)
		{
			Url = url
		};
	}
}
