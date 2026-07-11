using System.Runtime.CompilerServices;

namespace IronPython.Runtime.Binding;

internal interface IFastSettable
{
	T MakeSetBinding<T>(CallSite<T> site, PythonSetMemberBinder binder) where T : class;
}
