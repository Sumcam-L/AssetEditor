using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Firaxis.Net;

public class HtmlStyleSection : IEnumerable<HtmlSelectorStyle>, IEnumerable
{
	public List<HtmlSelectorStyle> Classes;

	public HtmlStyleSection(params HtmlSelectorStyle[] classes)
	{
		Classes = new List<HtmlSelectorStyle>(classes);
	}

	public IEnumerator<HtmlSelectorStyle> GetEnumerator()
	{
		IEnumerable<HtmlSelectorStyle> classes = Classes;
		return (classes ?? Enumerable.Empty<HtmlSelectorStyle>()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
