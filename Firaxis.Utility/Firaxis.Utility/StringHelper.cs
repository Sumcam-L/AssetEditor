using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Firaxis.Utility;

public static class StringHelper
{
	private struct rtfLookup
	{
		public string html;

		public string rtf;

		public rtfLookup(string html, string rtf)
		{
			this.html = html;
			this.rtf = rtf;
		}
	}

	private static readonly string rtfHeader = "{{\\rtf1\\ansi\\deff0{{\\colortbl ;{0}}} ";

	private static readonly string rtfHeaderNoColor = "{\\rtf1\\ansi\\deff0 ";

	private static readonly string rtfFooter = "}";

	private static readonly string[] customTags = new string[1] { "color" };

	private static readonly rtfLookup[] lookupTable = new rtfLookup[29]
	{
		new rtfLookup("{", "\\new rtfLookup("),
		new rtfLookup("}", "\\}"),
		new rtfLookup("<p>", ""),
		new rtfLookup("</p>", "\\par "),
		new rtfLookup("<br>", "{\\par}"),
		new rtfLookup("<b>", "{\\b "),
		new rtfLookup("</b>", "}"),
		new rtfLookup("<i>", "{\\i "),
		new rtfLookup("</i>", "}"),
		new rtfLookup("<u>", "{\\ul "),
		new rtfLookup("</u>", "}"),
		new rtfLookup("<h1>", "{\\fs70 "),
		new rtfLookup("</h1>", "}"),
		new rtfLookup("<h2>", "{\\fs60 "),
		new rtfLookup("</h2>", "}"),
		new rtfLookup("<h3>", "{\\fs50 "),
		new rtfLookup("</h3>", "}"),
		new rtfLookup("<h4>", "{\\fs40 "),
		new rtfLookup("</h4>", "}"),
		new rtfLookup("<h5>", "{\\fs30 "),
		new rtfLookup("</h5>", "}"),
		new rtfLookup("<h6>", "{\\fs20 "),
		new rtfLookup("</h6>", "}"),
		new rtfLookup("<h1>", "{\\fs10 "),
		new rtfLookup("</h1>", "\\par}"),
		new rtfLookup("<div>", "{ \\par}{"),
		new rtfLookup("</div>", "}"),
		new rtfLookup("</color>", "}"),
		new rtfLookup("</link>", "Û")
	};

	public static string GetAssetTitle(string asset, string title)
	{
		if (string.IsNullOrEmpty(asset))
		{
			return title;
		}
		return Path.GetFileName(asset) + " - " + title;
	}

	public static bool IsNullEmptyOrWhiteSpace(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return true;
		}
		foreach (char c in s)
		{
			if (c != ' ' && c != '\t')
			{
				return false;
			}
		}
		return true;
	}

	public static string GetStringIndex(string s, int index)
	{
		return GetStringIndex(s, index, '|');
	}

	public static string GetStringIndex(string s, int index, params char[] separator)
	{
		string[] array = s.Split(separator, StringSplitOptions.None);
		if (index < array.Length)
		{
			return array[index].Trim();
		}
		return "";
	}

	public static string[] ToArray(string s)
	{
		return ToArray(s, '|');
	}

	public static string[] ToArray(string s, params char[] separator)
	{
		return s.Split(separator, StringSplitOptions.None);
	}

	public static string SetStringIndex(string s, int index, string value)
	{
		return SetStringIndex(s, index, value, '|');
	}

	public static T EnumValue<T>(string name)
	{
		Type typeFromHandle = typeof(T);
		if (!typeFromHandle.IsEnum)
		{
			throw new Exception("Type must be an enumeration");
		}
		Array values = Enum.GetValues(typeFromHandle);
		foreach (object item in values)
		{
			if (name.CompareTo(item.ToString()) == 0)
			{
				return (T)item;
			}
		}
		return (T)values.GetValue(0);
	}

	public static string SetStringIndex(string s, int index, string value, params char[] separator)
	{
		List<string> list = new List<string>(s.Split(separator, StringSplitOptions.None));
		while (list.Count <= index)
		{
			list.Add("");
		}
		list[index] = value;
		string text = "";
		bool flag = false;
		foreach (string item in list)
		{
			if (!flag)
			{
				flag = true;
			}
			else
			{
				text += "| ";
			}
			text += item;
		}
		return text;
	}

	public static string SanitizeForXML(string str)
	{
		string text = new string(str.ToCharArray());
		text = text.Replace("&", "&amp;");
		text = text.Replace("<", "&lt;");
		text = text.Replace(">", "&gt;");
		text = text.Replace("\"", "&quot;");
		return text.Replace("'", "&apos;");
	}

	public static string DesanitizeFromXML(string str)
	{
		string text = new string(str.ToCharArray());
		text = text.Replace("&amp;", "&");
		text = text.Replace("&lt;", "<");
		text = text.Replace("&gt;", ">");
		text = text.Replace("&quot;", "\"");
		return text.Replace("&apos;", "'");
	}

	public static int FindLeastCommonSubstringLength(IList<string> strings)
	{
		return FindLeastCommonSubstringLength(strings, 0);
	}

	public static int FindLeastCommonSubstringLength(IList<string> strings, int startPosition)
	{
		int num = int.MaxValue;
		for (int i = 0; i < strings.Count; i++)
		{
			for (int j = i + 1; j < strings.Count; j++)
			{
				int num2 = Math.Min(strings[i].Length, strings[j].Length);
				int num3 = 0;
				for (int k = startPosition; k < num2 && strings[i][k] == strings[j][k]; k++)
				{
					num3++;
				}
				if (num3 < num)
				{
					num = num3;
				}
			}
		}
		if (num == int.MaxValue)
		{
			num = 0;
		}
		return num;
	}

	public static string HTMLToRTF(string html)
	{
		string text = html.Replace("\n", "<br>").Replace("\\", "\\\\");
		rtfLookup[] array = lookupTable;
		for (int i = 0; i < array.Length; i++)
		{
			rtfLookup rtfLookup2 = array[i];
			text = text.Replace(rtfLookup2.html, rtfLookup2.rtf);
		}
		List<Color> list = new List<Color>();
		text = ScanCustomTags(text, list);
		string text2 = "";
		foreach (Color item in list)
		{
			text2 += $"\\red{item.R}\\green{item.G}\\blue{item.B};";
		}
		string text3 = ((text2.Length != 0) ? string.Format(rtfHeader, text2) : rtfHeaderNoColor);
		text3 += text;
		return text3 + rtfFooter;
	}

	private static string ScanCustomTags(string html, List<Color> colors)
	{
		string[] array = customTags;
		foreach (string text in array)
		{
			for (int num = html.IndexOf($"<{text}"); num != -1; num = html.IndexOf($"<{text}"))
			{
				int num2 = html.IndexOf('>', num);
				if (num2 == -1)
				{
					return "";
				}
				string text2 = html.Substring(num, num2 - num + 1);
				string text3 = "";
				if (text.CompareTo("color") == 0)
				{
					Color item = Color.Black;
					try
					{
						item = Transpose.FromString<Color>(GetAttribute(text2, "name"));
					}
					catch
					{
					}
					colors.Add(item);
					text3 = $"{{\\cf{colors.Count} ";
				}
				else if (text.CompareTo("link") != 0)
				{
				}
				if (text3.Length != 0)
				{
					html = html.Replace(text2, text3);
				}
			}
		}
		return html;
	}

	private static string GetAttribute(string tag, string attrib)
	{
		int num = tag.IndexOf(attrib);
		if (num != -1)
		{
			int num2 = tag.IndexOf("\"", num);
			if (num2 != -1)
			{
				int num3 = tag.IndexOf("\"", num2 + 1);
				if (num3 != -1)
				{
					return tag.Substring(num2 + 1, num3 - num2 - 1);
				}
			}
		}
		return "";
	}
}
