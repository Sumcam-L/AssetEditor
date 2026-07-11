using System.Dynamic;

namespace IronPython.Runtime.Binding;

internal interface IPythonConvertible
{
	DynamicMetaObject BindConvert(PythonConversionBinder binder);
}
