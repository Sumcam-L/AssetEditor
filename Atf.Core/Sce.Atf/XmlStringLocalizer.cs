using System;
using System.Collections.Generic;
using System.Xml;

namespace Sce.Atf;

public abstract class XmlStringLocalizer : StringLocalizer
{
	private readonly Dictionary<string, List<Tuple<string, string>>> m_translations = new Dictionary<string, List<Tuple<string, string>>>();

	public override string Localize(string s, string context)
	{
		if (m_translations.TryGetValue(s, out var value))
		{
			if (value.Count == 1)
			{
				return value[0].Item2;
			}
			int num = value.FindIndex((Tuple<string, string> pair) => pair.Item1.Equals(context));
			if (num >= 0)
			{
				return value[num].Item2;
			}
			return value[0].Item2;
		}
		return s;
	}

	protected void AddLocalizedStrings(XmlDocument xmlDoc)
	{
		XmlElement documentElement = xmlDoc.DocumentElement;
		if (documentElement == null || documentElement.Name != "StringLocalizationTable")
		{
			throw new InvalidOperationException("invalid localization file: " + xmlDoc.BaseURI);
		}
		List<string> list = new List<string>();
		foreach (XmlElement item in documentElement.GetElementsByTagName("StringItem"))
		{
			string attribute = item.GetAttribute("id");
			string context = item.GetAttribute("context");
			string text = item.GetAttribute("translation");
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			if (text == "*")
			{
				text = attribute;
			}
			if (!m_translations.TryGetValue(attribute, out var value))
			{
				m_translations.Add(attribute, new List<Tuple<string, string>>
				{
					new Tuple<string, string>(context, text)
				});
				continue;
			}
			int num = value.FindIndex((Tuple<string, string> pair) => pair.Item1.Equals(context));
			if (num < 0)
			{
				value.Add(new Tuple<string, string>(context, text));
			}
			else if (value[num].Item2 != text)
			{
				list.Add($"1. \"{attribute}\", context: \"{context}\" => \"{value[num].Item2}\"");
				list.Add($"2. \"{attribute}\", context: \"{context}\" => \"{text}\"");
			}
		}
		if (list.Count > 0)
		{
			throw new InvalidOperationException("Conflicting translations in a localized XML file: \n\t" + string.Join("\n\t", list));
		}
	}
}
