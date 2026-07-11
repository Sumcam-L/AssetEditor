using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Firaxis.Net;

public class HtmlSelectorStyle : IEnumerable<HtmlStyleValue>, IEnumerable
{
	public string Selector;

	public List<HtmlStyleValue> Values;

	public HtmlSelectorStyle(string selector, params HtmlStyleValue[] values)
	{
		Selector = selector;
		Values = new List<HtmlStyleValue>(values);
	}

	public IEnumerator<HtmlStyleValue> GetEnumerator()
	{
		IEnumerable<HtmlStyleValue> values = Values;
		return (values ?? Enumerable.Empty<HtmlStyleValue>()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
