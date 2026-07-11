using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[DontMapGetMemberNamesToDir]
[PythonType("instancemethod")]
public sealed class Method : PythonTypeSlot, IWeakReferenceable, IPythonMembersList, IMembersList, IDynamicMetaObjectProvider, ICodeFormattable, IFastInvokable
{
	private abstract class BaseMethodBinding
	{
		public abstract Delegate GetSelfTarget();

		public abstract Delegate GetSelflessTarget();
	}

	private class MethodBinding : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, target);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			throw new InvalidOperationException();
		}
	}

	private class MethodBinding<T0> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, target, arg0);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, object>>)site).Update(site, context, target, arg0, arg1);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, target, arg0, arg1);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, object>>)site).Update(site, context, target, arg0, arg1, arg2);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, target, arg0, arg1, arg2);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3, T4> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3, arg4);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4, arg5);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3, T4, T5> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3, arg4, arg5);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4, arg5, arg6);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3, T4, T5, T6> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3, T4, T5, T6, T7> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3, T4, T5, T6, T7, T8> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10, T10 arg11)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>(SelflessTarget);
		}
	}

	private class MethodBinding<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : BaseMethodBinding
	{
		private CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>> _site;

		public MethodBinding(PythonInvokeBinder binder)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>>.Create(binder);
		}

		public object SelfTarget(CallSite site, CodeContext context, object target, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			if (target is Method { _inst: not null } method)
			{
				return _site.Target(_site, context, method._func, method._inst, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
		}

		public object SelflessTarget(CallSite site, CodeContext context, object target, object arg0, T0 arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5, T5 arg6, T6 arg7, T7 arg8, T8 arg9, T9 arg10, T10 arg11, T11 arg12)
		{
			if (target is Method { _inst: null } method)
			{
				return _site.Target(_site, context, method._func, PythonOps.MethodCheckSelf(context, method, arg0), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>>)site).Update(site, context, target, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
		}

		public override Delegate GetSelfTarget()
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>(SelfTarget);
		}

		public override Delegate GetSelflessTarget()
		{
			return new Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>(SelflessTarget);
		}
	}

	private readonly object _func;

	private readonly object _inst;

	private readonly object _declaringClass;

	private WeakRefTracker _weakref;

	internal string Name => (string)PythonOps.GetBoundAttr(DefaultContext.Default, _func, "__name__");

	public string __doc__ => PythonOps.GetBoundAttr(DefaultContext.Default, _func, "__doc__") as string;

	public object im_func => _func;

	public object __func__ => _func;

	public object im_self => _inst;

	public object __self__ => _inst;

	public object im_class => PythonOps.ToPythonType(_declaringClass as PythonType) ?? _declaringClass;

	internal override bool GetAlwaysSucceeds => true;

	FastBindResult<T> IFastInvokable.MakeInvokeBinding<T>(CallSite<T> site, PythonInvokeBinder binder, CodeContext context, object[] args)
	{
		if (binder.Signature.IsSimple)
		{
			BaseMethodBinding binding = null;
			if (__self__ == null)
			{
				if (args.Length != 0)
				{
					binding = GetMethodBinding<T>(binder, GetTypeArgsSelfless<T>(), binding);
					if (binding != null)
					{
						return new FastBindResult<T>((T)(object)binding.GetSelflessTarget(), shouldCache: true);
					}
				}
			}
			else
			{
				PythonInvokeBinder selfBinder = GetSelfBinder(binder, context);
				binding = ((args.Length != 0) ? GetMethodBinding<T>(selfBinder, GetTypeArgs<T>(), binding) : new MethodBinding(selfBinder));
				if (binding != null)
				{
					return new FastBindResult<T>((T)(object)binding.GetSelfTarget(), shouldCache: true);
				}
			}
		}
		return default(FastBindResult<T>);
	}

	private static BaseMethodBinding GetMethodBinding<T>(PythonInvokeBinder binder, Type[] typeArgs, BaseMethodBinding binding) where T : class
	{
		switch (typeArgs.Length)
		{
		case 1:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<>).MakeGenericType(typeArgs), binder);
			break;
		case 2:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, >).MakeGenericType(typeArgs), binder);
			break;
		case 3:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , >).MakeGenericType(typeArgs), binder);
			break;
		case 4:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , >).MakeGenericType(typeArgs), binder);
			break;
		case 5:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , , >).MakeGenericType(typeArgs), binder);
			break;
		case 6:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , , , >).MakeGenericType(typeArgs), binder);
			break;
		case 7:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , , , , >).MakeGenericType(typeArgs), binder);
			break;
		case 8:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , , , , , >).MakeGenericType(typeArgs), binder);
			break;
		case 9:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , , , , , , >).MakeGenericType(typeArgs), binder);
			break;
		case 10:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , , , , , , , >).MakeGenericType(typeArgs), binder);
			break;
		case 11:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , , , , , , , , >).MakeGenericType(typeArgs), binder);
			break;
		case 12:
			binding = (BaseMethodBinding)Activator.CreateInstance(typeof(MethodBinding<, , , , , , , , , , , >).MakeGenericType(typeArgs), binder);
			break;
		}
		return binding;
	}

	private static Type[] GetTypeArgs<T>() where T : class
	{
		return ArrayUtils.ShiftLeft(ArrayUtils.ConvertAll(typeof(T).GetMethod("Invoke").GetParameters(), (ParameterInfo pi) => pi.ParameterType), 3);
	}

	private static Type[] GetTypeArgsSelfless<T>() where T : class
	{
		return ArrayUtils.ShiftLeft(ArrayUtils.ConvertAll(typeof(T).GetMethod("Invoke").GetParameters(), (ParameterInfo pi) => pi.ParameterType), 4);
	}

	private static PythonInvokeBinder GetSelfBinder(PythonInvokeBinder binder, CodeContext context)
	{
		return context.LanguageContext.Invoke(new CallSignature(ArrayUtils.Insert(new Argument(ArgumentType.Simple), binder.Signature.GetArgumentInfos())));
	}

	public Method(object function, object instance, object @class)
	{
		_func = function;
		_inst = instance;
		_declaringClass = @class;
	}

	public Method(object function, object instance)
	{
		if (instance == null)
		{
			throw PythonOps.TypeError("unbound methods must have a class provided");
		}
		_func = function;
		_inst = instance;
	}

	[SpecialName]
	public object Call(CodeContext context, params object[] args)
	{
		return PythonContext.GetContext(context).CallSplat(this, args);
	}

	[SpecialName]
	public object Call(CodeContext context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args)
	{
		return PythonContext.GetContext(context).CallWithKeywords(this, args, kwArgs);
	}

	private Exception BadSelf(object got)
	{
		OldClass oldClass = im_class as OldClass;
		string text = ((got != null) ? (PythonOps.GetPythonTypeName(got) + " instance") : "nothing");
		PythonType pythonType = im_class as PythonType;
		return PythonOps.TypeError("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)", Name, (oldClass != null) ? oldClass.Name : ((pythonType != null) ? pythonType.Name : im_class), text);
	}

	internal object CheckSelf(CodeContext context, object self)
	{
		if (!PythonOps.IsInstance(context, self, im_class))
		{
			throw BadSelf(self);
		}
		return self;
	}

	private string DeclaringClassAsString()
	{
		if (im_class == null)
		{
			return "?";
		}
		if (im_class is PythonType pythonType)
		{
			return pythonType.Name;
		}
		if (im_class is OldClass oldClass)
		{
			return oldClass.Name;
		}
		return im_class.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Method method))
		{
			return false;
		}
		if (PythonOps.EqualRetBool(_inst, method._inst))
		{
			return PythonOps.EqualRetBool(_func, method._func);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_inst == null)
		{
			return PythonOps.Hash(DefaultContext.Default, _func);
		}
		return PythonOps.Hash(DefaultContext.Default, _inst) ^ PythonOps.Hash(DefaultContext.Default, _func);
	}

	WeakRefTracker IWeakReferenceable.GetWeakRef()
	{
		return _weakref;
	}

	bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
	{
		_weakref = value;
		return true;
	}

	void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
	{
		((IWeakReferenceable)this).SetWeakRef(value);
	}

	[SpecialName]
	public object GetCustomMember(CodeContext context, string name)
	{
		switch (name)
		{
		case "__module__":
			return PythonOps.GetBoundAttr(context, _func, "__module__");
		case "__name__":
			return PythonOps.GetBoundAttr(DefaultContext.Default, _func, "__name__");
		default:
		{
			if (TypeCache.Method.TryGetBoundMember(context, this, name, out var value) || PythonOps.TryGetBoundAttr(context, _func, name, out value))
			{
				return value;
			}
			return OperationFailed.Value;
		}
		}
	}

	[SpecialName]
	public void SetMemberAfter(CodeContext context, string name, object value)
	{
		TypeCache.Method.SetMember(context, this, name, value);
	}

	[SpecialName]
	public void DeleteMember(CodeContext context, string name)
	{
		TypeCache.Method.DeleteMember(context, this, name);
	}

	IList<string> IMembersList.GetMemberNames()
	{
		return PythonOps.GetStringMemberList(this);
	}

	IList<object> IPythonMembersList.GetMemberNames(CodeContext context)
	{
		List memberNames = TypeCache.Method.GetMemberNames(context);
		memberNames.AddNoLockNoDups("__module__");
		if (_func is PythonFunction { func_dict: var func_dict })
		{
			foreach (KeyValuePair<object, object> item in func_dict)
			{
				memberNames.AddNoLockNoDups(item.Key);
			}
		}
		return memberNames;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (im_self == null && (owner == null || owner == im_class || PythonOps.IsSubClass(context, owner, im_class)))
		{
			value = new Method(_func, instance, owner);
			return true;
		}
		value = this;
		return true;
	}

	public string __repr__(CodeContext context)
	{
		if (!PythonOps.TryGetBoundAttr(context, _func, "__name__", out var ret))
		{
			ret = "?";
		}
		if (_inst != null)
		{
			return $"<bound method {DeclaringClassAsString()}.{ret} of {PythonOps.Repr(context, _inst)}>";
		}
		return $"<unbound method {DeclaringClassAsString()}.{ret}>";
	}

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		return new MetaMethod(parameter, BindingRestrictions.Empty, this);
	}
}
