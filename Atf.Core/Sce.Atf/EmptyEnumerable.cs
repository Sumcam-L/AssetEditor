using System;
using System.Collections;
using System.Collections.Generic;

namespace Sce.Atf;

public static class EmptyEnumerable<T>
{
	private class Enumerable : IEnumerable<T>, IEnumerable
	{
		private class Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			public T Current
			{
				get
				{
					throw new InvalidOperationException("no current value");
				}
			}

			object IEnumerator.Current => Current;

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return false;
			}

			public void Reset()
			{
			}
		}

		private static readonly Enumerator s_enumerator = new Enumerator();

		public IEnumerator<T> GetEnumerator()
		{
			return s_enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return s_enumerator;
		}
	}

	public static readonly IEnumerable<T> Instance = new Enumerable();
}
