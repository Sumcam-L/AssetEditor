using System;

namespace Sce.Atf;

public static class Localizer
{
	private static StringLocalizer s_stringLocalizer;

	public static void SetStringLocalizer(StringLocalizer stringLocalizer)
	{
		if (s_stringLocalizer != null)
		{
			throw new InvalidOperationException("Some strings have already been localized by another string localizer");
		}
		s_stringLocalizer = stringLocalizer;
	}

	private static StringLocalizer GetStringLocalizer()
	{
		return s_stringLocalizer ?? (s_stringLocalizer = new StringLocalizer());
	}

	public static string Localize(this string s)
	{
		return GetStringLocalizer().Localize(s, string.Empty);
	}

	public static string Localize(this string s, string context)
	{
		return GetStringLocalizer().Localize(s, context);
	}
}
