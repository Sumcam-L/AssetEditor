using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding;

internal sealed class PythonOverloadResolverFactory : OverloadResolverFactory
{
	private readonly PythonBinder _binder;

	internal readonly Expression _codeContext;

	public PythonOverloadResolverFactory(PythonBinder binder, Expression codeContext)
	{
		_binder = binder;
		_codeContext = codeContext;
	}

	public override DefaultOverloadResolver CreateOverloadResolver(IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType)
	{
		return new PythonOverloadResolver(_binder, args, signature, callType, _codeContext);
	}
}
