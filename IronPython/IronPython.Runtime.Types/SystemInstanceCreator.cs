using System;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Types;

internal class SystemInstanceCreator : InstanceCreator
{
	private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object[], object>> _ctorSite;

	private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object>> _ctorSite0;

	private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object>> _ctorSite1;

	private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, object>> _ctorSite2;

	private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, object, object>> _ctorSite3;

	public SystemInstanceCreator(PythonType type)
		: base(type)
	{
	}

	internal override object CreateInstance(CodeContext context)
	{
		if (_ctorSite0 == null)
		{
			Interlocked.CompareExchange(ref _ctorSite0, CallSite<Func<CallSite, CodeContext, BuiltinFunction, object>>.Create(PythonContext.GetContext(context).InvokeNone), null);
		}
		return _ctorSite0.Target(_ctorSite0, context, base.Type.Ctor);
	}

	internal override object CreateInstance(CodeContext context, object arg0)
	{
		if (_ctorSite1 == null)
		{
			Interlocked.CompareExchange(ref _ctorSite1, CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object>>.Create(PythonContext.GetContext(context).Invoke(new CallSignature(1))), null);
		}
		return _ctorSite1.Target(_ctorSite1, context, base.Type.Ctor, arg0);
	}

	internal override object CreateInstance(CodeContext context, object arg0, object arg1)
	{
		if (_ctorSite2 == null)
		{
			Interlocked.CompareExchange(ref _ctorSite2, CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, object>>.Create(PythonContext.GetContext(context).Invoke(new CallSignature(2))), null);
		}
		return _ctorSite2.Target(_ctorSite2, context, base.Type.Ctor, arg0, arg1);
	}

	internal override object CreateInstance(CodeContext context, object arg0, object arg1, object arg2)
	{
		if (_ctorSite3 == null)
		{
			Interlocked.CompareExchange(ref _ctorSite3, CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, object, object>>.Create(PythonContext.GetContext(context).Invoke(new CallSignature(3))), null);
		}
		return _ctorSite3.Target(_ctorSite3, context, base.Type.Ctor, arg0, arg1, arg2);
	}

	internal override object CreateInstance(CodeContext context, params object[] args)
	{
		if (_ctorSite == null)
		{
			Interlocked.CompareExchange(ref _ctorSite, CallSite<Func<CallSite, CodeContext, BuiltinFunction, object[], object>>.Create(PythonContext.GetContext(context).Invoke(new CallSignature(new Argument(ArgumentType.List)))), null);
		}
		return _ctorSite.Target(_ctorSite, context, base.Type.Ctor, args);
	}

	internal override object CreateInstance(CodeContext context, object[] args, string[] names)
	{
		return PythonOps.CallWithKeywordArgs(context, base.Type.Ctor, args, names);
	}
}
