using System.Runtime.CompilerServices;

namespace IronPython.Runtime.Binding;

internal interface IFastGettable
{
	T MakeGetBinding<T>(CallSite<T> site, PythonGetMemberBinder binder, CodeContext state, string name) where T : class;
}
