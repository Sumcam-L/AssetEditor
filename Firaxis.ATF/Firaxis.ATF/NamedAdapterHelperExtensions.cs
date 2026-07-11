using System.Collections.Generic;
using System.Linq;

namespace Firaxis.ATF;

public static class NamedAdapterHelperExtensions
{
	public static string GenerateUniqueName<T>(this IEnumerable<T> values, string baseName) where T : INamedAdapter
	{
		int num = 0;
		string uniqueName = string.Empty;
		do
		{
			int num2 = num + 1;
			num = num2;
			uniqueName = baseName + num2.ToString("D3");
		}
		while (values.Any((T value) => value.Name == uniqueName));
		return uniqueName;
	}
}
