using System.Dynamic;

namespace IronPython.Runtime.Binding;

internal interface IComConvertible
{
	DynamicMetaObject GetComMetaObject();
}
