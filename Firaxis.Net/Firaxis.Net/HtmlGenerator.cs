using System.IO;
using System.Linq;
using System.Web.UI;

namespace Firaxis.Net;

public static class HtmlGenerator
{
	public static string Generate(HtmlStyleSection htmlStyleSection = null, params HtmlTag[] tags)
	{
		using StringWriter stringWriter = new StringWriter();
		using HtmlTextWriter htmlTextWriter = new HtmlTextWriter(stringWriter);
		htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Html);
		if (htmlStyleSection != null)
		{
			htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Head);
			htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Style);
			bool flag = false;
			foreach (HtmlSelectorStyle item in htmlStyleSection)
			{
				if (item.Values.Count == 0)
				{
					continue;
				}
				if (flag)
				{
					htmlTextWriter.WriteLine();
				}
				flag = true;
				htmlTextWriter.Write($"{item.Selector} {{");
				foreach (HtmlStyleValue value in item.Values)
				{
					htmlTextWriter.WriteLine();
					htmlTextWriter.Write($"  {value.Name}: {value.Value};");
				}
				htmlTextWriter.WriteLine();
				htmlTextWriter.Write("}");
			}
			htmlTextWriter.RenderEndTag();
			htmlTextWriter.RenderEndTag();
			htmlTextWriter.WriteLine();
		}
		htmlTextWriter.RenderBeginTag(HtmlTextWriterTag.Body);
		bool flag2 = false;
		foreach (HtmlTag tag in tags)
		{
			if (flag2)
			{
				htmlTextWriter.WriteLine();
			}
			flag2 = true;
			AddTag(htmlTextWriter, tag);
		}
		htmlTextWriter.RenderEndTag();
		htmlTextWriter.RenderEndTag();
		return stringWriter.ToString();
	}

	private static void AddTag(HtmlTextWriter writer, HtmlTag tag)
	{
		foreach (HtmlAttribute item in tag.Inner.OfType<HtmlAttribute>())
		{
			writer.AddAttribute(item.Attribute, item.Contents);
		}
		foreach (HtmlStyleAttribute item2 in tag.Inner.OfType<HtmlStyleAttribute>())
		{
			writer.AddStyleAttribute(item2.Style, item2.Contents);
		}
		writer.RenderBeginTag(tag.Tag);
		if (tag.Contents != null)
		{
			writer.Write(tag.Contents);
		}
		bool flag = false;
		foreach (HtmlTag item3 in tag.Inner.OfType<HtmlTag>())
		{
			if (flag)
			{
				writer.WriteLine();
			}
			flag = true;
			AddTag(writer, item3);
		}
		writer.RenderEndTag();
	}
}
