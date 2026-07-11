using System.Collections.Generic;
using System.IO;

namespace IronPython.Modules;

internal static class StreamReaderExtensions
{
	public static IEnumerable<string> ReadLines(this StreamReader reader)
	{
		while (true)
		{
			string text;
			string line = (text = reader.ReadLine());
			if (text != null)
			{
				yield return line;
				continue;
			}
			break;
		}
	}
}
