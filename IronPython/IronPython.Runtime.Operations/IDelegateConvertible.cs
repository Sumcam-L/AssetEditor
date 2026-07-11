using System;

namespace IronPython.Runtime.Operations;

internal interface IDelegateConvertible
{
	Delegate ConvertToDelegate(Type type);
}
