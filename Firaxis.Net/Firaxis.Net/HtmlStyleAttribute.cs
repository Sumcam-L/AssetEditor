using System.Web.UI;

namespace Firaxis.Net;

public class HtmlStyleAttribute : IHtmlObject
{
	public HtmlTextWriterStyle Style;

	public string Contents;

	public HtmlStyleAttribute(HtmlTextWriterStyle style, string contents)
	{
		Style = style;
		Contents = contents;
	}
}
