using System.Web.UI;

namespace Firaxis.Net;

public class HtmlAttribute : IHtmlObject
{
	public HtmlTextWriterAttribute Attribute;

	public string Contents;

	public HtmlAttribute(HtmlTextWriterAttribute attribute, string contents)
	{
		Attribute = attribute;
		Contents = contents;
	}
}
