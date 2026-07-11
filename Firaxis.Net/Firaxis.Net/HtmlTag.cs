using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Firaxis.Net;

public class HtmlTag : IHtmlObject, IEnumerable<IHtmlObject>, IEnumerable
{
	public HtmlTextWriterTag Tag;

	public string Contents;

	public List<IHtmlObject> Inner;

	public HtmlTag(HtmlTextWriterTag tag, params IHtmlObject[] inner)
	{
		Tag = tag;
		Inner = new List<IHtmlObject>(inner);
	}

	public HtmlTag(HtmlTextWriterTag tag, string contents, params IHtmlObject[] inner)
		: this(tag, inner)
	{
		Contents = contents;
	}

	public HtmlTag AddTag(HtmlTextWriterTag tag, params IHtmlObject[] inner)
	{
		HtmlTag htmlTag = new HtmlTag(tag, inner);
		Inner.Add(htmlTag);
		return htmlTag;
	}

	public HtmlTag AddTag(HtmlTextWriterTag tag, string contents, params IHtmlObject[] inner)
	{
		HtmlTag htmlTag = new HtmlTag(tag, contents, inner);
		Inner.Add(htmlTag);
		return htmlTag;
	}

	public HtmlAttribute AddAttribute(HtmlTextWriterAttribute attribute, string contents)
	{
		HtmlAttribute htmlAttribute = new HtmlAttribute(attribute, contents);
		Inner.Add(htmlAttribute);
		return htmlAttribute;
	}

	public IEnumerator<IHtmlObject> GetEnumerator()
	{
		IEnumerable<IHtmlObject> inner = Inner;
		return (inner ?? Enumerable.Empty<IHtmlObject>()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
