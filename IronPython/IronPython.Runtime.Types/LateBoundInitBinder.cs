using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Modules;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

internal class LateBoundInitBinder : DynamicMetaObjectBinder
{
	private class FastInitSite
	{
		private readonly int _version;

		private readonly PythonFunction _slot;

		private readonly CallSite<Func<CallSite, CodeContext, PythonFunction, object, object>> _initSite;

		public FastInitSite(int version, PythonInvokeBinder binder, PythonFunction target)
		{
			_version = version;
			_slot = target;
			_initSite = CallSite<Func<CallSite, CodeContext, PythonFunction, object, object>>.Create(binder);
		}

		public object CallTarget(CallSite site, CodeContext context, object inst)
		{
			if (inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version)
			{
				_initSite.Target(_initSite, context, _slot, inst);
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, inst);
		}

		public object EmptyCallTarget(CallSite site, CodeContext context, object inst)
		{
			if ((inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version) || DynamicHelpers.GetPythonType(inst).Version == _version)
			{
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, inst);
		}
	}

	private class FastInitSite<T0>
	{
		private readonly int _version;

		private readonly PythonFunction _slot;

		private readonly CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, object>> _initSite;

		public FastInitSite(int version, PythonInvokeBinder binder, PythonFunction target)
		{
			_version = version;
			_slot = target;
			_initSite = CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, object>>.Create(binder);
		}

		public object CallTarget(CallSite site, CodeContext context, object inst, T0 arg0)
		{
			if (inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version)
			{
				_initSite.Target(_initSite, context, _slot, inst, arg0);
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, inst, arg0);
		}

		public object EmptyCallTarget(CallSite site, CodeContext context, object inst, T0 arg0)
		{
			if ((inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version) || DynamicHelpers.GetPythonType(inst).Version == _version)
			{
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, inst, arg0);
		}
	}

	private class FastInitSite<T0, T1>
	{
		private readonly int _version;

		private readonly PythonFunction _slot;

		private readonly CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, T1, object>> _initSite;

		public FastInitSite(int version, PythonInvokeBinder binder, PythonFunction target)
		{
			_version = version;
			_slot = target;
			_initSite = CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, T1, object>>.Create(binder);
		}

		public object CallTarget(CallSite site, CodeContext context, object inst, T0 arg0, T1 arg1)
		{
			if (inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version)
			{
				_initSite.Target(_initSite, context, _slot, inst, arg0, arg1);
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, inst, arg0, arg1);
		}

		public object EmptyCallTarget(CallSite site, CodeContext context, object inst, T0 arg0, T1 arg1)
		{
			if ((inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version) || DynamicHelpers.GetPythonType(inst).Version == _version)
			{
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, inst, arg0, arg1);
		}
	}

	private class FastInitSite<T0, T1, T2>
	{
		private readonly int _version;

		private readonly PythonFunction _slot;

		private readonly CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, T1, T2, object>> _initSite;

		public FastInitSite(int version, PythonInvokeBinder binder, PythonFunction target)
		{
			_version = version;
			_slot = target;
			_initSite = CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, T1, T2, object>>.Create(binder);
		}

		public object CallTarget(CallSite site, CodeContext context, object inst, T0 arg0, T1 arg1, T2 arg2)
		{
			if (inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version)
			{
				_initSite.Target(_initSite, context, _slot, inst, arg0, arg1, arg2);
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, inst, arg0, arg1, arg2);
		}

		public object EmptyCallTarget(CallSite site, CodeContext context, object inst, T0 arg0, T1 arg1, T2 arg2)
		{
			if ((inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version) || DynamicHelpers.GetPythonType(inst).Version == _version)
			{
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, inst, arg0, arg1, arg2);
		}
	}

	private class FastInitSite<T0, T1, T2, T3>
	{
		private readonly int _version;

		private readonly PythonFunction _slot;

		private readonly CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, T1, T2, T3, object>> _initSite;

		public FastInitSite(int version, PythonInvokeBinder binder, PythonFunction target)
		{
			_version = version;
			_slot = target;
			_initSite = CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, T1, T2, T3, object>>.Create(binder);
		}

		public object CallTarget(CallSite site, CodeContext context, object inst, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			if (inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version)
			{
				_initSite.Target(_initSite, context, _slot, inst, arg0, arg1, arg2, arg3);
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, inst, arg0, arg1, arg2, arg3);
		}

		public object EmptyCallTarget(CallSite site, CodeContext context, object inst, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			if ((inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version) || DynamicHelpers.GetPythonType(inst).Version == _version)
			{
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, inst, arg0, arg1, arg2, arg3);
		}
	}

	private class FastInitSite<T0, T1, T2, T3, T4>
	{
		private readonly int _version;

		private readonly PythonFunction _slot;

		private readonly CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, T1, T2, T3, T4, object>> _initSite;

		public FastInitSite(int version, PythonInvokeBinder binder, PythonFunction target)
		{
			_version = version;
			_slot = target;
			_initSite = CallSite<Func<CallSite, CodeContext, PythonFunction, object, T0, T1, T2, T3, T4, object>>.Create(binder);
		}

		public object CallTarget(CallSite site, CodeContext context, object inst, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version)
			{
				_initSite.Target(_initSite, context, _slot, inst, arg0, arg1, arg2, arg3, arg4);
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, inst, arg0, arg1, arg2, arg3, arg4);
		}

		public object EmptyCallTarget(CallSite site, CodeContext context, object inst, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if ((inst is IPythonObject pythonObject && pythonObject.PythonType.Version == _version) || DynamicHelpers.GetPythonType(inst).Version == _version)
			{
				return inst;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, inst, arg0, arg1, arg2, arg3, arg4);
		}
	}

	public const int MaxFastLateBoundInitArgs = 6;

	private readonly PythonType _newType;

	private readonly CallSignature _signature;

	public LateBoundInitBinder(PythonType type, CallSignature signature)
	{
		_newType = type;
		_signature = signature;
	}

	public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		DynamicMetaObject dynamicMetaObject = target;
		CodeContext codeContext = (CodeContext)dynamicMetaObject.Value;
		target = args[0];
		args = ArrayUtils.RemoveFirst(args);
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(target);
		PythonType pythonType = DynamicHelpers.GetPythonType(target.Value);
		BindingRestrictions restrictions = BindingRestrictions.Empty;
		Expression arg;
		if (Builtin.isinstance(target.Value, _newType) && NeedsInitCall((CodeContext)dynamicMetaObject.Value, pythonType, args.Length))
		{
			pythonType.TryResolveSlot(codeContext, "__init__", out var slot);
			if (pythonType.IsMixedNewStyleOldStyle())
			{
				arg = MakeDynamicInitInvoke(codeContext, args, Expression.Call(typeof(PythonOps).GetMethod("GetMixedMember"), dynamicMetaObject.Expression, Expression.Convert(Utils.WeakConstant(pythonType), typeof(PythonType)), Utils.Convert(target.Expression, typeof(object)), Utils.Constant("__init__")), dynamicMetaObject.Expression);
			}
			else if (slot is PythonFunction)
			{
				Expression[] array = new Expression[args.Length + 3];
				array[0] = dynamicMetaObject.Expression;
				array[1] = Utils.WeakConstant(slot);
				array[2] = target.Expression;
				for (int i = 0; i < args.Length; i++)
				{
					array[3 + i] = args[i].Expression;
				}
				arg = Expression.Dynamic(codeContext.LanguageContext.Invoke(_signature.InsertArgument(Argument.Simple)), typeof(object), array);
			}
			else if (slot is BuiltinMethodDescriptor || slot is BuiltinFunction)
			{
				IList<MethodBase> targets = ((!(slot is BuiltinMethodDescriptor)) ? ((BuiltinFunction)slot).Targets : ((BuiltinMethodDescriptor)slot).Template.Targets);
				PythonBinder binder = codeContext.LanguageContext.Binder;
				DynamicMetaObject dynamicMetaObject2 = binder.CallMethod(new PythonOverloadResolver(binder, target, args, _signature, dynamicMetaObject.Expression), targets, BindingRestrictions.Empty);
				arg = dynamicMetaObject2.Expression;
				restrictions = dynamicMetaObject2.Restrictions;
			}
			else
			{
				arg = MakeDynamicInitInvoke(codeContext, args, Expression.Call(typeof(PythonOps).GetMethod("GetInitSlotMember"), dynamicMetaObject.Expression, Expression.Convert(Utils.WeakConstant(_newType), typeof(PythonType)), Expression.Convert(Utils.WeakConstant(slot), typeof(PythonTypeSlot)), Utils.Convert(target.Expression, typeof(object))), dynamicMetaObject.Expression);
			}
		}
		else
		{
			arg = Utils.Empty();
		}
		if (pythonType.TryResolveSlot(codeContext, "__del__", out var _))
		{
			arg = Expression.Block(arg, Expression.Call(typeof(PythonOps).GetMethod("InitializeForFinalization"), dynamicMetaObject.Expression, Utils.Convert(target.Expression, typeof(object))));
		}
		return BindingHelpers.AddDynamicTestAndDefer(this, new DynamicMetaObject(Expression.Block(arg, target.Expression), target.Restrict(target.LimitType).Restrictions.Merge(restrictions)), args, validationInfo);
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		if (args.Length <= 6)
		{
			CodeContext codeContext = (CodeContext)args[0];
			object o = args[1];
			PythonType pythonType = DynamicHelpers.GetPythonType(o);
			PythonFunction pythonFunction = null;
			string text = null;
			if (!pythonType.TryResolveSlot(codeContext, "__del__", out var _) && !pythonType.IsMixedNewStyleOldStyle())
			{
				if (Builtin.isinstance(o, _newType) && NeedsInitCall(codeContext, pythonType, args.Length))
				{
					pythonType.TryResolveSlot(codeContext, "__init__", out var slot2);
					if (slot2 is PythonFunction)
					{
						text = "CallTarget";
						pythonFunction = (PythonFunction)slot2;
					}
				}
				else
				{
					text = "EmptyCallTarget";
				}
			}
			if (text != null)
			{
				int num = args.Length - 2;
				PythonInvokeBinder pythonInvokeBinder = codeContext.LanguageContext.Invoke(_signature.InsertArgument(Argument.Simple));
				if (num == 0)
				{
					FastInitSite fastInitSite = new FastInitSite(pythonType.Version, pythonInvokeBinder, pythonFunction);
					if (text == "CallTarget")
					{
						return (T)(object)new Func<CallSite, CodeContext, object, object>(fastInitSite.CallTarget);
					}
					return (T)(object)new Func<CallSite, CodeContext, object, object>(fastInitSite.EmptyCallTarget);
				}
				Type[] array = ArrayUtils.ConvertAll(typeof(T).GetMethod("Invoke").GetParameters(), (ParameterInfo x) => x.ParameterType);
				Type type = (args.Length switch
				{
					3 => typeof(FastInitSite<>), 
					4 => typeof(FastInitSite<, >), 
					5 => typeof(FastInitSite<, , >), 
					6 => typeof(FastInitSite<, , , >), 
					7 => typeof(FastInitSite<, , , , >), 
					_ => throw new InvalidOperationException(), 
				}).MakeGenericType(ArrayUtils.ShiftLeft(array, 3));
				object target = Activator.CreateInstance(type, pythonType.Version, pythonInvokeBinder, pythonFunction);
				return (T)(object)ReflectionUtils.CreateDelegate(type.GetMethod(text), typeof(T), target);
			}
		}
		return base.BindDelegate(site, args);
	}

	private bool NeedsInitCall(CodeContext context, PythonType type, int argCount)
	{
		if (!type.HasObjectInit(context) || type.IsMixedNewStyleOldStyle())
		{
			return true;
		}
		if (_newType.HasObjectNew(context))
		{
			return argCount > 0;
		}
		return false;
	}

	private DynamicExpression MakeDynamicInitInvoke(CodeContext context, DynamicMetaObject[] args, Expression initFunc, Expression codeContext)
	{
		return Expression.Dynamic(context.LanguageContext.Invoke(_signature), typeof(object), ArrayUtils.Insert(codeContext, initFunc, DynamicUtils.GetExpressions(args)));
	}
}
