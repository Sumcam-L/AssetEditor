using System;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;

namespace IronPython.Runtime.Types;

internal class ChainedUserGet : UserGetBase
{
	internal override bool ShouldCache => false;

	public ChainedUserGet(PythonGetMemberBinder binder, int version, Func<CallSite, object, CodeContext, object> func)
		: base(binder, version)
	{
		_func = func;
	}
}
