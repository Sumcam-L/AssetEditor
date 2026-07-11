using System;
using System.Collections.Generic;

namespace Sce.Atf;

public static class Lazies
{
	public static IEnumerable<T> GetValues<T>(this IEnumerable<Lazy<T>> lazies)
	{
		foreach (Lazy<T> lazy in lazies)
		{
			yield return lazy.Value;
		}
	}
}
