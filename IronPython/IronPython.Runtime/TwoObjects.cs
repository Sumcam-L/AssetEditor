using System;

namespace IronPython.Runtime;

internal class TwoObjects
{
	internal object obj1;

	internal object obj2;

	public TwoObjects(object obj1, object obj2)
	{
		this.obj1 = obj1;
		this.obj2 = obj2;
	}

	public override int GetHashCode()
	{
		throw new NotSupportedException();
	}

	public override bool Equals(object other)
	{
		if (!(other is TwoObjects twoObjects))
		{
			return false;
		}
		if (twoObjects.obj1 == obj1)
		{
			return twoObjects.obj2 == obj2;
		}
		return false;
	}
}
