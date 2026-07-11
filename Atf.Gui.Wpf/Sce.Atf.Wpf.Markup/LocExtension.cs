using System;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Markup;

[MarkupExtensionReturnType(typeof(string))]
[ContentProperty("Key")]
public class LocExtension : MarkupExtension
{
	public string Key { get; set; }

	public string Format { get; set; }

	public LocExtension()
	{
	}

	public LocExtension(string key)
	{
		Key = key;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (string.IsNullOrEmpty(Key))
		{
			return string.Empty;
		}
		return string.IsNullOrEmpty(Format) ? Key.Localize() : string.Format(Format, Key.Localize());
	}
}
