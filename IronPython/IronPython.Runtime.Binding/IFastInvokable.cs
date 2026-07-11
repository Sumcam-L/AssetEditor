using System.Runtime.CompilerServices;

namespace IronPython.Runtime.Binding;

internal interface IFastInvokable
{
	FastBindResult<T> MakeInvokeBinding<T>(CallSite<T> site, PythonInvokeBinder binder, CodeContext context, object[] args) where T : class;
}
