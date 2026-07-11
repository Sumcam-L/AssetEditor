using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime;

[PythonType("enumerable_wrapper")]
public class IEnumerableOfTWrapper<T> : IEnumerable<T>, IEnumerable
{
	private IEnumerable enumerable;

	public IEnumerableOfTWrapper(IEnumerable enumerable)
	{
		this.enumerable = enumerable;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new IEnumeratorOfTWrapper<T>(enumerable.GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
