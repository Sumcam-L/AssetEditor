using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class MetaUserObject : MetaPythonObject, IPythonInvokable, IPythonConvertible, IPythonOperable, IPythonGetable
{
	internal abstract class GetOrInvokeBinderHelper<TResult>
	{
		protected readonly IPythonObject _value;

		protected bool _extensionMethodRestriction;

		protected abstract bool IsFinal { get; }

		public IPythonObject Value => _value;

		public GetOrInvokeBinderHelper(IPythonObject value)
		{
			_value = value;
		}

		public TResult Bind(CodeContext context, string name)
		{
			IPythonObject value = Value;
			if (TryGetGetAttribute(context, value.PythonType, out var dts))
			{
				return BindGetAttribute(dts);
			}
			dts = FindSlot(context, name, value, out var isOldStyle, out var systemTypeResolution, out var extensionMethodResolution);
			_extensionMethodRestriction = extensionMethodResolution;
			if (!isOldStyle || dts is ReflectedSlotProperty)
			{
				if (value.PythonType.HasDictionary && (dts == null || !dts.IsSetDescriptor(context, value.PythonType)))
				{
					MakeDictionaryAccess();
				}
				if (dts != null)
				{
					MakeSlotAccess(dts, systemTypeResolution);
				}
			}
			else
			{
				MakeOldStyleAccess();
			}
			if (!IsFinal)
			{
				if (Value.PythonType.TryResolveSlot(context, "__getattr__", out var slot))
				{
					MakeGetAttrAccess(slot);
				}
				MakeTypeError();
			}
			return FinishRule();
		}

		protected abstract void MakeTypeError();

		protected abstract void MakeGetAttrAccess(PythonTypeSlot getattr);

		protected abstract void MakeSlotAccess(PythonTypeSlot foundSlot, bool systemTypeResolution);

		protected abstract TResult BindGetAttribute(PythonTypeSlot foundSlot);

		protected abstract TResult FinishRule();

		protected abstract void MakeDictionaryAccess();

		protected abstract void MakeOldStyleAccess();
	}

	private abstract class MetaGetBinderHelper : GetOrInvokeBinderHelper<DynamicMetaObject>
	{
		private readonly DynamicMetaObject _self;

		private readonly GetBindingInfo _bindingInfo;

		protected readonly MetaUserObject _target;

		private readonly DynamicMetaObjectBinder _binder;

		protected readonly DynamicMetaObject _codeContext;

		private string _resolution = "GetMember ";

		protected override bool IsFinal => _bindingInfo.Body.IsFinal;

		public Expression Expression => _target.Expression;

		public MetaGetBinderHelper(MetaUserObject target, DynamicMetaObjectBinder binder, DynamicMetaObject codeContext)
			: base(target.Value)
		{
			_target = target;
			_self = _target.Restrict(base.Value.GetType());
			_binder = binder;
			_codeContext = codeContext;
			_bindingInfo = new GetBindingInfo(_binder, new DynamicMetaObject[1] { _target }, Expression.Variable(Expression.Type, "self"), Expression.Variable(typeof(object), "lookupRes"), new ConditionalBuilder(_binder), BindingHelpers.GetValidationInfo(_self, base.Value.PythonType));
		}

		private DynamicMetaObject MakeGetAttributeRule(GetBindingInfo info, IPythonObject obj, PythonTypeSlot slot, DynamicMetaObject codeContext)
		{
			CodeContext sharedContext = PythonContext.GetPythonContext(info.Action).SharedContext;
			Type finalSystemType = obj.PythonType.FinalSystemType;
			if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(finalSystemType) && TryGetGetAttribute(sharedContext, DynamicHelpers.GetPythonTypeFromType(finalSystemType), out var dts) && dts == slot)
			{
				return FallbackError();
			}
			obj.PythonType.TryResolveSlot(sharedContext, "__getattr__", out var slot2);
			DynamicMetaObject dynamicMetaObject = _target.Restrict(base.Value.GetType());
			string name = (BindingHelpers.IsNoThrow(info.Action) ? "GetAttributeNoThrow" : "GetAttribute");
			return BindingHelpers.AddDynamicTestAndDefer(info.Action, new DynamicMetaObject(Expression.Call(typeof(UserTypeOps).GetMethod(name), Expression.Constant(PythonContext.GetPythonContext(info.Action).SharedContext), info.Args[0].Expression, Expression.Constant(MetaPythonObject.GetGetMemberName(info.Action)), Expression.Constant(slot, typeof(PythonTypeSlot)), Expression.Constant(slot2, typeof(PythonTypeSlot)), Expression.Constant(new SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>>())), dynamicMetaObject.Restrictions), info.Args, info.Validation);
		}

		protected abstract DynamicMetaObject FallbackError();

		protected abstract DynamicMetaObject Fallback();

		protected virtual Expression Invoke(Expression res)
		{
			return Invoke(new DynamicMetaObject(res, BindingRestrictions.Empty)).Expression;
		}

		protected virtual DynamicMetaObject Invoke(DynamicMetaObject res)
		{
			return res;
		}

		protected override DynamicMetaObject BindGetAttribute(PythonTypeSlot foundSlot)
		{
			return Invoke(MakeGetAttributeRule(_bindingInfo, base.Value, foundSlot, _codeContext));
		}

		protected override void MakeGetAttrAccess(PythonTypeSlot getattr)
		{
			_resolution += "GetAttr ";
			MakeGetAttrRule(_bindingInfo, GetWeakSlot(getattr), _codeContext);
		}

		protected override void MakeTypeError()
		{
			_bindingInfo.Body.FinishCondition(FallbackError().Expression);
		}

		protected override DynamicMetaObject FinishRule()
		{
			DynamicMetaObject metaObject = _bindingInfo.Body.GetMetaObject(_target);
			metaObject = new DynamicMetaObject(Expression.Block(new ParameterExpression[2] { _bindingInfo.Self, _bindingInfo.Result }, Expression.Assign(_bindingInfo.Self, _self.Expression), metaObject.Expression), _self.Restrictions.Merge(metaObject.Restrictions));
			if (_extensionMethodRestriction)
			{
				metaObject = new DynamicMetaObject(metaObject.Expression, metaObject.Restrictions.Merge(((CodeContext)_codeContext.Value).ModuleContext.ExtensionMethods.GetRestriction(_codeContext.Expression)));
			}
			return BindingHelpers.AddDynamicTestAndDefer(_binder, metaObject, new DynamicMetaObject[1] { _target }, _bindingInfo.Validation);
		}

		private void MakeGetAttrRule(GetBindingInfo info, Expression getattr, DynamicMetaObject codeContext)
		{
			info.Body.AddCondition(MakeGetAttrTestAndGet(info, getattr), Invoke(MakeGetAttrCall(info, codeContext)));
		}

		private Expression MakeGetAttrCall(GetBindingInfo info, DynamicMetaObject codeContext)
		{
			Expression expr = Expression.Dynamic(PythonContext.GetPythonContext(info.Action).InvokeOne, typeof(object), PythonContext.GetCodeContext(info.Action), info.Result, Expression.Constant(MetaPythonObject.GetGetMemberName(info.Action)));
			return MaybeMakeNoThrow(info, expr);
		}

		private Expression MaybeMakeNoThrow(GetBindingInfo info, Expression expr)
		{
			if (BindingHelpers.IsNoThrow(info.Action))
			{
				DynamicMetaObject dynamicMetaObject = FallbackError();
				ParameterExpression parameterExpression = Expression.Variable(typeof(object), "getAttrRes");
				expr = Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Block(Utils.Try(Expression.Assign(parameterExpression, Utils.Convert(expr, typeof(object)))).Catch(typeof(MissingMemberException), Expression.Assign(parameterExpression, Utils.Convert(dynamicMetaObject.Expression, typeof(object)))), parameterExpression));
			}
			return expr;
		}

		protected override void MakeSlotAccess(PythonTypeSlot foundSlot, bool systemTypeResolution)
		{
			_resolution = string.Concat(_resolution, CompilerHelpers.GetType(foundSlot), " ");
			if (systemTypeResolution)
			{
				_bindingInfo.Body.FinishCondition(Fallback().Expression);
			}
			else
			{
				MakeSlotAccess(foundSlot);
			}
		}

		private void MakeSlotAccess(PythonTypeSlot dts)
		{
			if (dts is ReflectedSlotProperty reflectedSlotProperty)
			{
				_bindingInfo.Body.AddCondition(Expression.NotEqual(Expression.Assign(_bindingInfo.Result, Expression.ArrayAccess(GetSlots(_target), Expression.Constant(reflectedSlotProperty.Index))), Expression.Field(null, typeof(Uninitialized).GetField("Instance"))), Invoke(_bindingInfo.Result));
				return;
			}
			if (dts is PythonTypeUserDescriptorSlot value)
			{
				_bindingInfo.Body.FinishCondition(Expression.Call(typeof(PythonOps).GetMethod("GetUserSlotValue"), Expression.Constant(PythonContext.GetPythonContext(_bindingInfo.Action).SharedContext), Expression.Convert(Utils.WeakConstant(value), typeof(PythonTypeUserDescriptorSlot)), _target.Expression, Expression.Property(Expression.Convert(_bindingInfo.Self, typeof(IPythonObject)), IronPython.Runtime.Types.TypeInfo._IPythonObject.PythonType)));
			}
			if (dts.GetType() == typeof(PythonProperty))
			{
				Expression right = Expression.Property(Expression.Convert(Utils.WeakConstant(dts), typeof(PythonProperty)), "fget");
				ParameterExpression parameterExpression = Expression.Variable(typeof(object), "tmpGet");
				_bindingInfo.Body.AddVariable(parameterExpression);
				_bindingInfo.Body.FinishCondition(Expression.Block(Expression.Assign(parameterExpression, right), Expression.Condition(Expression.NotEqual(parameterExpression, Expression.Constant(null)), Invoke(Expression.Dynamic(PythonContext.GetPythonContext(_bindingInfo.Action).InvokeOne, typeof(object), Expression.Constant(PythonContext.GetPythonContext(_bindingInfo.Action).SharedContext), parameterExpression, _bindingInfo.Self)), _binder.Throw(Expression.Call(typeof(PythonOps).GetMethod("UnreadableProperty")), typeof(object)))));
				return;
			}
			Expression expression = Expression.Call(IronPython.Runtime.Types.TypeInfo._PythonOps.SlotTryGetBoundValue, Expression.Constant(PythonContext.GetPythonContext(_bindingInfo.Action).SharedContext), Expression.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)), Utils.Convert(_bindingInfo.Self, typeof(object)), Expression.Property(Expression.Convert(_bindingInfo.Self, typeof(IPythonObject)), IronPython.Runtime.Types.TypeInfo._IPythonObject.PythonType), _bindingInfo.Result);
			Expression expression2 = Invoke(_bindingInfo.Result);
			if (dts.GetAlwaysSucceeds)
			{
				_bindingInfo.Body.FinishCondition(Expression.Block(expression, expression2));
			}
			else
			{
				_bindingInfo.Body.AddCondition(expression, expression2);
			}
		}

		protected override void MakeDictionaryAccess()
		{
			_resolution += "Dictionary ";
			FieldInfo field = _target.LimitType.GetField(".dict");
			Expression expression = ((!(field != null)) ? Expression.Property(Expression.Convert(_bindingInfo.Self, typeof(IPythonObject)), IronPython.Runtime.Types.TypeInfo._IPythonObject.Dict) : Expression.Field(Expression.Convert(_bindingInfo.Self, _target.LimitType), field));
			IList<string> optimizedInstanceNames = base.Value.PythonType.GetOptimizedInstanceNames();
			int num;
			if (optimizedInstanceNames != null && (num = optimizedInstanceNames.IndexOf(MetaPythonObject.GetGetMemberName(_bindingInfo.Action))) != -1)
			{
				_bindingInfo.Body.AddCondition(Expression.Call(typeof(UserTypeOps).GetMethod("TryGetDictionaryValue"), expression, Utils.Constant(MetaPythonObject.GetGetMemberName(_bindingInfo.Action)), Expression.Constant(base.Value.PythonType.GetOptimizedInstanceVersion()), Expression.Constant(num), _bindingInfo.Result), Invoke(new DynamicMetaObject(_bindingInfo.Result, BindingRestrictions.Empty)).Expression);
			}
			else
			{
				_bindingInfo.Body.AddCondition(Expression.AndAlso(Expression.NotEqual(expression, Expression.Constant(null)), Expression.Call(expression, IronPython.Runtime.Types.TypeInfo._PythonDictionary.TryGetvalue, Utils.Constant(MetaPythonObject.GetGetMemberName(_bindingInfo.Action)), _bindingInfo.Result)), Invoke(new DynamicMetaObject(_bindingInfo.Result, BindingRestrictions.Empty)).Expression);
			}
		}

		protected override void MakeOldStyleAccess()
		{
			_resolution += "MixedOldStyle ";
			_bindingInfo.Body.AddCondition(Expression.Call(typeof(UserTypeOps).GetMethod("TryGetMixedNewStyleOldStyleSlot"), Expression.Constant(PythonContext.GetPythonContext(_bindingInfo.Action).SharedContext), Utils.Convert(_bindingInfo.Self, typeof(object)), Utils.Constant(MetaPythonObject.GetGetMemberName(_bindingInfo.Action)), _bindingInfo.Result), Invoke(_bindingInfo.Result));
		}
	}

	internal class FastGetBinderHelper : GetOrInvokeBinderHelper<FastGetBase>
	{
		private readonly int _version;

		private readonly PythonGetMemberBinder _binder;

		private readonly CallSite<Func<CallSite, object, CodeContext, object>> _site;

		private readonly CodeContext _context;

		private bool _dictAccess;

		private bool _noOptimizedForm;

		private PythonTypeSlot _slot;

		private PythonTypeSlot _getattrSlot;

		protected override bool IsFinal
		{
			get
			{
				if (_slot != null)
				{
					return _slot.GetAlwaysSucceeds;
				}
				return false;
			}
		}

		public FastGetBinderHelper(CodeContext context, CallSite<Func<CallSite, object, CodeContext, object>> site, IPythonObject value, PythonGetMemberBinder binder)
			: base(value)
		{
			_version = value.PythonType.Version;
			_binder = binder;
			_site = site;
			_context = context;
		}

		protected override void MakeTypeError()
		{
		}

		protected override void MakeSlotAccess(PythonTypeSlot foundSlot, bool systemTypeResolution)
		{
			if (systemTypeResolution)
			{
				_binder.Context.Binder.TryResolveSlot(_context, base.Value.PythonType, base.Value.PythonType, _binder.Name, out foundSlot);
			}
			_slot = foundSlot;
		}

		public FastBindResult<Func<CallSite, object, CodeContext, object>> GetBinding(CodeContext context, string name)
		{
			Dictionary<CachedGetKey, FastGetBase> cachedGets = GetCachedGets();
			CachedGetKey key = CachedGetKey.Make(name, context.ModuleContext.ExtensionMethods);
			FastGetBase value;
			lock (cachedGets)
			{
				if (!cachedGets.TryGetValue(key, out value) || !value.IsValid(base.Value.PythonType))
				{
					FastGetBase fastGetBase = Bind(context, name);
					if (fastGetBase != null)
					{
						value = fastGetBase;
						if (value.ShouldCache)
						{
							cachedGets[key] = value;
						}
					}
				}
			}
			if (value != null && value.ShouldUseNonOptimizedSite)
			{
				return new FastBindResult<Func<CallSite, object, CodeContext, object>>(value._func, value.ShouldCache);
			}
			return default(FastBindResult<Func<CallSite, object, CodeContext, object>>);
		}

		private Dictionary<CachedGetKey, FastGetBase> GetCachedGets()
		{
			if (_binder.IsNoThrow)
			{
				Dictionary<CachedGetKey, FastGetBase> cachedTryGets = base.Value.PythonType._cachedTryGets;
				if (cachedTryGets == null)
				{
					Interlocked.CompareExchange(ref base.Value.PythonType._cachedTryGets, new Dictionary<CachedGetKey, FastGetBase>(), null);
					cachedTryGets = base.Value.PythonType._cachedTryGets;
				}
				return cachedTryGets;
			}
			Dictionary<CachedGetKey, FastGetBase> cachedGets = base.Value.PythonType._cachedGets;
			if (cachedGets == null)
			{
				Interlocked.CompareExchange(ref base.Value.PythonType._cachedGets, new Dictionary<CachedGetKey, FastGetBase>(), null);
				cachedGets = base.Value.PythonType._cachedGets;
			}
			return cachedGets;
		}

		protected override FastGetBase FinishRule()
		{
			if (_noOptimizedForm)
			{
				return null;
			}
			if (_slot is ReflectedSlotProperty reflectedSlotProperty)
			{
				return new GetMemberDelegates(OptimizedGetKind.PropertySlot, base.Value.PythonType, _binder, _binder.Name, _version, _slot, _getattrSlot, reflectedSlotProperty.Getter, FallbackError(), _context.ModuleContext.ExtensionMethods);
			}
			if (_dictAccess)
			{
				if (_slot is PythonTypeUserDescriptorSlot)
				{
					return new GetMemberDelegates(OptimizedGetKind.UserSlotDict, base.Value.PythonType, _binder, _binder.Name, _version, _slot, _getattrSlot, null, FallbackError(), _context.ModuleContext.ExtensionMethods);
				}
				return new GetMemberDelegates(OptimizedGetKind.SlotDict, base.Value.PythonType, _binder, _binder.Name, _version, _slot, _getattrSlot, null, FallbackError(), _context.ModuleContext.ExtensionMethods);
			}
			if (_slot is PythonTypeUserDescriptorSlot)
			{
				return new GetMemberDelegates(OptimizedGetKind.UserSlotOnly, base.Value.PythonType, _binder, _binder.Name, _version, _slot, _getattrSlot, null, FallbackError(), _context.ModuleContext.ExtensionMethods);
			}
			return new GetMemberDelegates(OptimizedGetKind.SlotOnly, base.Value.PythonType, _binder, _binder.Name, _version, _slot, _getattrSlot, null, FallbackError(), _context.ModuleContext.ExtensionMethods);
		}

		private Func<CallSite, object, CodeContext, object> FallbackError()
		{
			Type finalSystemType = base.Value.PythonType.FinalSystemType;
			if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(finalSystemType))
			{
				return ((IFastGettable)base.Value).MakeGetBinding(_site, _binder, _context, _binder.Name);
			}
			if (_binder.IsNoThrow)
			{
				return (CallSite site, object self, CodeContext context) => OperationFailed.Value;
			}
			string name = _binder.Name;
			return delegate(CallSite site, object self, CodeContext context)
			{
				throw PythonOps.AttributeErrorForMissingAttribute(((IPythonObject)self).PythonType.Name, name);
			};
		}

		protected override void MakeDictionaryAccess()
		{
			_dictAccess = true;
		}

		protected override FastGetBase BindGetAttribute(PythonTypeSlot foundSlot)
		{
			Type finalSystemType = base.Value.PythonType.FinalSystemType;
			if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(finalSystemType) && TryGetGetAttribute(_context, DynamicHelpers.GetPythonTypeFromType(finalSystemType), out var dts) && dts == foundSlot)
			{
				return new ChainedUserGet(_binder, _version, FallbackError());
			}
			base.Value.PythonType.TryResolveSlot(_context, "__getattr__", out var slot);
			return new GetAttributeDelegates(_binder, _binder.Name, _version, foundSlot, slot);
		}

		protected override void MakeGetAttrAccess(PythonTypeSlot getattr)
		{
			_getattrSlot = getattr;
		}

		protected override void MakeOldStyleAccess()
		{
			_noOptimizedForm = true;
		}
	}

	private class GetBinderHelper : MetaGetBinderHelper
	{
		private readonly DynamicMetaObjectBinder _binder;

		public GetBinderHelper(MetaUserObject target, DynamicMetaObjectBinder binder, DynamicMetaObject codeContext)
			: base(target, binder, codeContext)
		{
			_binder = binder;
		}

		protected override DynamicMetaObject Fallback()
		{
			return MetaPythonObject.GetMemberFallback(_target, _binder, _codeContext);
		}

		protected override DynamicMetaObject FallbackError()
		{
			return _target.FallbackGetError(_binder, _codeContext);
		}
	}

	private class InvokeBinderHelper : MetaGetBinderHelper
	{
		private readonly InvokeMemberBinder _binder;

		private readonly DynamicMetaObject[] _args;

		public InvokeBinderHelper(MetaUserObject target, InvokeMemberBinder binder, DynamicMetaObject[] args, DynamicMetaObject codeContext)
			: base(target, binder, codeContext)
		{
			_binder = binder;
			_args = args;
		}

		protected override DynamicMetaObject Fallback()
		{
			return _binder.FallbackInvokeMember(_target, _args);
		}

		protected override DynamicMetaObject FallbackError()
		{
			if (_target._baseMetaObject != null)
			{
				return _target._baseMetaObject.BindInvokeMember(_binder, _args);
			}
			return Fallback();
		}

		protected override DynamicMetaObject Invoke(DynamicMetaObject res)
		{
			return _binder.FallbackInvoke(res, _args, null);
		}
	}

	internal abstract class SetBinderHelper<TResult>
	{
		private readonly IPythonObject _instance;

		private readonly object _value;

		protected readonly CodeContext _context;

		public IPythonObject Instance => _instance;

		public object Value => _value;

		public SetBinderHelper(CodeContext context, IPythonObject instance, object value)
		{
			_instance = instance;
			_value = value;
			_context = context;
		}

		public TResult Bind(string name)
		{
			bool flag = false;
			if (_instance.PythonType.TryResolveSlot(_context, "__setattr__", out var slot) && !IsStandardObjectMethod(slot) && slot != null)
			{
				MakeSetAttrTarget(slot);
				flag = true;
			}
			if (!flag)
			{
				slot = FindSlot(_context, name, _instance, out var _, out var systemTypeResolution, out var _);
				if (slot is ReflectedSlotProperty prop)
				{
					MakeSlotsSetTarget(prop);
					flag = true;
				}
				else if (slot != null && slot.IsSetDescriptor(_context, _instance.PythonType))
				{
					MakeSlotSetOrFallback(slot, systemTypeResolution);
					flag = systemTypeResolution || slot.GetType() == typeof(PythonProperty);
				}
			}
			if (!flag)
			{
				if (_instance.PythonType.HasDictionary)
				{
					MakeDictionarySetTarget();
				}
				else
				{
					MakeFallback();
				}
			}
			return Finish();
		}

		protected abstract TResult Finish();

		protected abstract void MakeSetAttrTarget(PythonTypeSlot dts);

		protected abstract void MakeSlotsSetTarget(ReflectedSlotProperty prop);

		protected abstract void MakeSlotSetOrFallback(PythonTypeSlot dts, bool systemTypeResolution);

		protected abstract void MakeDictionarySetTarget();

		protected abstract void MakeFallback();
	}

	internal class FastSetBinderHelper<TValue> : SetBinderHelper<SetMemberDelegates<TValue>>
	{
		private readonly PythonSetMemberBinder _binder;

		private readonly int _version;

		private PythonTypeSlot _setattrSlot;

		private ReflectedSlotProperty _slotProp;

		private bool _unsupported;

		private bool _dictSet;

		public FastSetBinderHelper(CodeContext context, IPythonObject self, object value, PythonSetMemberBinder binder)
			: base(context, self, value)
		{
			_binder = binder;
			_version = self.PythonType.Version;
		}

		protected override SetMemberDelegates<TValue> Finish()
		{
			if (_unsupported)
			{
				return new SetMemberDelegates<TValue>(_context, base.Instance.PythonType, OptimizedSetKind.None, _binder.Name, _version, _setattrSlot, null);
			}
			if (_setattrSlot != null)
			{
				return new SetMemberDelegates<TValue>(_context, base.Instance.PythonType, OptimizedSetKind.SetAttr, _binder.Name, _version, _setattrSlot, null);
			}
			if (_slotProp != null)
			{
				return new SetMemberDelegates<TValue>(_context, base.Instance.PythonType, OptimizedSetKind.UserSlot, _binder.Name, _version, null, _slotProp.Setter);
			}
			if (_dictSet)
			{
				return new SetMemberDelegates<TValue>(_context, base.Instance.PythonType, OptimizedSetKind.SetDict, _binder.Name, _version, null, null);
			}
			return new SetMemberDelegates<TValue>(_context, base.Instance.PythonType, OptimizedSetKind.Error, _binder.Name, _version, null, null);
		}

		public FastBindResult<Func<CallSite, object, TValue, object>> MakeSet()
		{
			Dictionary<SetMemberKey, FastSetBase> cachedSets = GetCachedSets();
			FastSetBase value;
			lock (cachedSets)
			{
				SetMemberKey key = new SetMemberKey(typeof(TValue), _binder.Name);
				if (!cachedSets.TryGetValue(key, out value) || value._version != base.Instance.PythonType.Version)
				{
					value = Bind(_binder.Name);
					if (value != null)
					{
						cachedSets[key] = value;
					}
				}
			}
			if (value.ShouldUseNonOptimizedSite)
			{
				return new FastBindResult<Func<CallSite, object, TValue, object>>((Func<CallSite, object, TValue, object>)value._func, shouldCache: false);
			}
			return default(FastBindResult<Func<CallSite, object, TValue, object>>);
		}

		private Dictionary<SetMemberKey, FastSetBase> GetCachedSets()
		{
			Dictionary<SetMemberKey, FastSetBase> cachedSets = base.Instance.PythonType._cachedSets;
			if (cachedSets == null)
			{
				Interlocked.CompareExchange(ref base.Instance.PythonType._cachedSets, new Dictionary<SetMemberKey, FastSetBase>(), null);
				cachedSets = base.Instance.PythonType._cachedSets;
			}
			return cachedSets;
		}

		protected override void MakeSlotSetOrFallback(PythonTypeSlot dts, bool systemTypeResolution)
		{
			_unsupported = true;
		}

		protected override void MakeSlotsSetTarget(ReflectedSlotProperty prop)
		{
			_slotProp = prop;
		}

		protected override void MakeFallback()
		{
		}

		protected override void MakeSetAttrTarget(PythonTypeSlot dts)
		{
			_setattrSlot = dts;
		}

		protected override void MakeDictionarySetTarget()
		{
			_dictSet = true;
		}
	}

	internal class MetaSetBinderHelper : SetBinderHelper<DynamicMetaObject>
	{
		private readonly MetaUserObject _target;

		private readonly DynamicMetaObject _value;

		private readonly SetBindingInfo _info;

		private DynamicMetaObject _result;

		private string _resolution = "SetMember ";

		public MetaSetBinderHelper(MetaUserObject target, DynamicMetaObject value, SetMemberBinder binder)
			: base(PythonContext.GetPythonContext(binder).SharedContext, target.Value, value.Value)
		{
			_target = target;
			_value = value;
			_info = new SetBindingInfo(binder, new DynamicMetaObject[2] { target, value }, new ConditionalBuilder(binder), BindingHelpers.GetValidationInfo(target, base.Instance.PythonType));
		}

		protected override void MakeSetAttrTarget(PythonTypeSlot dts)
		{
			ParameterExpression parameterExpression = Expression.Variable(typeof(object), "boundVal");
			_info.Body.AddVariable(parameterExpression);
			_info.Body.AddCondition(Expression.Call(typeof(PythonOps).GetMethod("SlotTryGetValue"), Utils.Constant(PythonContext.GetPythonContext(_info.Action).SharedContext), Utils.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)), Utils.Convert(_info.Args[0].Expression, typeof(object)), Utils.Convert(Utils.WeakConstant(base.Instance.PythonType), typeof(PythonType)), parameterExpression), Expression.Dynamic(PythonContext.GetPythonContext(_info.Action).Invoke(new CallSignature(2)), typeof(object), PythonContext.GetCodeContext(_info.Action), parameterExpression, Utils.Constant(_info.Action.Name), _info.Args[1].Expression));
			_info.Body.FinishCondition(FallbackSetError(_info.Action, _info.Args[1]).Expression);
			_result = _info.Body.GetMetaObject(_target, _value);
			_resolution += "SetAttr ";
		}

		protected override DynamicMetaObject Finish()
		{
			_result = new DynamicMetaObject(_result.Expression, _target.Restrict(base.Instance.GetType()).Restrictions.Merge(_result.Restrictions));
			return BindingHelpers.AddDynamicTestAndDefer(_info.Action, _result, new DynamicMetaObject[2] { _target, _value }, _info.Validation);
		}

		protected override void MakeFallback()
		{
			_info.Body.FinishCondition(FallbackSetError(_info.Action, _value).Expression);
			_result = _info.Body.GetMetaObject(_target, _value);
		}

		protected override void MakeDictionarySetTarget()
		{
			_resolution += "Dictionary ";
			FieldInfo field = _info.Args[0].LimitType.GetField(".dict");
			if (field != null)
			{
				FieldInfo field2 = _info.Args[0].LimitType.GetField(".class");
				IList<string> optimizedInstanceNames = base.Instance.PythonType.GetOptimizedInstanceNames();
				int num;
				if (field2 != null && optimizedInstanceNames != null && (num = optimizedInstanceNames.IndexOf(_info.Action.Name)) != -1)
				{
					_info.Body.FinishCondition(Expression.Call(typeof(UserTypeOps).GetMethod("FastSetDictionaryValueOptimized"), Expression.Field(Expression.Convert(_info.Args[0].Expression, _info.Args[0].LimitType), field2), Expression.Field(Expression.Convert(_info.Args[0].Expression, _info.Args[0].LimitType), field), Utils.Constant(_info.Action.Name), Utils.Convert(_info.Args[1].Expression, typeof(object)), Expression.Constant(base.Instance.PythonType.GetOptimizedInstanceVersion()), Expression.Constant(num)));
				}
				else
				{
					_info.Body.FinishCondition(Expression.Call(typeof(UserTypeOps).GetMethod("FastSetDictionaryValue"), Expression.Field(Expression.Convert(_info.Args[0].Expression, _info.Args[0].LimitType), field), Utils.Constant(_info.Action.Name), Utils.Convert(_info.Args[1].Expression, typeof(object))));
				}
			}
			else
			{
				_info.Body.FinishCondition(Expression.Call(typeof(UserTypeOps).GetMethod("SetDictionaryValue"), Expression.Convert(_info.Args[0].Expression, typeof(IPythonObject)), Utils.Constant(_info.Action.Name), Utils.Convert(_info.Args[1].Expression, typeof(object))));
			}
			_result = _info.Body.GetMetaObject(_target, _value);
		}

		protected override void MakeSlotSetOrFallback(PythonTypeSlot dts, bool systemTypeResolution)
		{
			if (systemTypeResolution)
			{
				_result = _target.Fallback(_info.Action, _value);
			}
			else
			{
				_result = MakeSlotSet(_info, dts);
			}
		}

		protected override void MakeSlotsSetTarget(ReflectedSlotProperty prop)
		{
			_resolution += "Slot ";
			MakeSlotsSetTargetHelper(_info, prop, _value.Expression);
			_result = _info.Body.GetMetaObject(_target, _value);
		}

		private DynamicMetaObject FallbackSetError(SetMemberBinder action, DynamicMetaObject value)
		{
			if (_target._baseMetaObject != null)
			{
				return _target._baseMetaObject.BindSetMember(action, value);
			}
			if (action is PythonSetMemberBinder)
			{
				return new DynamicMetaObject(MakeTypeError(action, action.Name, base.Instance.PythonType), BindingRestrictions.Empty);
			}
			return _info.Action.FallbackSetMember(_target.Restrict(_target.GetLimitType()), value);
		}
	}

	private class MemberBindingInfo
	{
		public readonly ConditionalBuilder Body;

		public readonly DynamicMetaObject[] Args;

		public readonly ValidationInfo Validation;

		public MemberBindingInfo(DynamicMetaObject[] args, ConditionalBuilder body, ValidationInfo validation)
		{
			Body = body;
			Validation = validation;
			Args = args;
		}
	}

	private class DeleteBindingInfo : MemberBindingInfo
	{
		public readonly DeleteMemberBinder Action;

		public DeleteBindingInfo(DeleteMemberBinder action, DynamicMetaObject[] args, ConditionalBuilder body, ValidationInfo validation)
			: base(args, body, validation)
		{
			Action = action;
		}
	}

	private class SetBindingInfo : MemberBindingInfo
	{
		public readonly SetMemberBinder Action;

		public SetBindingInfo(SetMemberBinder action, DynamicMetaObject[] args, ConditionalBuilder body, ValidationInfo validation)
			: base(args, body, validation)
		{
			Action = action;
		}
	}

	private class GetBindingInfo : MemberBindingInfo
	{
		public readonly DynamicMetaObjectBinder Action;

		public readonly ParameterExpression Self;

		public readonly ParameterExpression Result;

		public GetBindingInfo(DynamicMetaObjectBinder action, DynamicMetaObject[] args, ParameterExpression self, ParameterExpression result, ConditionalBuilder body, ValidationInfo validationInfo)
			: base(args, body, validationInfo)
		{
			Action = action;
			Self = self;
			Result = result;
		}
	}

	private readonly DynamicMetaObject _baseMetaObject;

	public new IPythonObject Value => (IPythonObject)base.Value;

	public MetaUserObject(Expression expression, BindingRestrictions restrictions, DynamicMetaObject baseMetaObject, IPythonObject value)
		: base(expression, restrictions, value)
	{
		_baseMetaObject = baseMetaObject;
	}

	public DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return InvokeWorker(pythonInvoke, codeContext, args);
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args)
	{
		return new InvokeBinderHelper(this, action, args, PythonContext.GetCodeContextMO(action)).Bind(PythonContext.GetPythonContext(action).SharedContext, action.Name);
	}

	public override DynamicMetaObject BindConvert(ConvertBinder conversion)
	{
		return ConvertWorker(conversion, conversion.Type, conversion.Type, conversion.Explicit ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast);
	}

	public DynamicMetaObject BindConvert(PythonConversionBinder binder)
	{
		return ConvertWorker(binder, binder.Type, binder.ReturnType, binder.ResultKind);
	}

	public DynamicMetaObject ConvertWorker(DynamicMetaObjectBinder binder, Type type, Type retType, ConversionResultKind kind)
	{
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(this, Value.PythonType);
		return BindingHelpers.AddDynamicTestAndDefer(binder, TryPythonConversion(binder, type) ?? FallbackConvert(binder), new DynamicMetaObject[1] { this }, validationInfo, retType);
	}

	public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
	{
		return PythonProtocol.Operation(binder, this, arg, null);
	}

	public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
	{
		return PythonProtocol.Operation(binder, this, null);
	}

	public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
	{
		return PythonProtocol.Index(binder, PythonIndexType.GetItem, ArrayUtils.Insert(this, indexes));
	}

	public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		return PythonProtocol.Index(binder, PythonIndexType.SetItem, ArrayUtils.Insert(this, ArrayUtils.Append(indexes, value)));
	}

	public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
	{
		return PythonProtocol.Index(binder, PythonIndexType.DeleteItem, ArrayUtils.Insert(this, indexes));
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder action, DynamicMetaObject[] args)
	{
		Expression codeContext = Expression.Call(typeof(PythonOps).GetMethod("GetPythonTypeContext"), Expression.Property(Utils.Convert(base.Expression, typeof(IPythonObject)), "PythonType"));
		return InvokeWorker(action, codeContext, args);
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		foreach (object o in Value.PythonType.GetMemberNames(Value.PythonType.PythonContext.SharedContext, Value))
		{
			if (o is string)
			{
				yield return (string)o;
			}
		}
	}

	private DynamicMetaObject InvokeWorker(DynamicMetaObjectBinder action, Expression codeContext, DynamicMetaObject[] args)
	{
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(this, Value.PythonType);
		if (Value is PythonType)
		{
			PythonContext pythonContext = PythonContext.GetPythonContext(action);
			if (Value.PythonType.TryResolveMixedSlot(pythonContext.SharedContext, "__call__", out var slot) && TypeCache.PythonType.TryResolveSlot(pythonContext.SharedContext, "__call__", out var slot2) && slot == slot2)
			{
				return InvokeFallback(action, codeContext, args);
			}
		}
		return BindingHelpers.AddDynamicTestAndDefer(action, PythonProtocol.Call(action, this, args) ?? InvokeFallback(action, codeContext, args), args, validationInfo);
	}

	private DynamicMetaObject InvokeFallback(DynamicMetaObjectBinder action, Expression codeContext, DynamicMetaObject[] args)
	{
		if (action is InvokeBinder invokeBinder)
		{
			if (_baseMetaObject != null)
			{
				return _baseMetaObject.BindInvoke(invokeBinder, args);
			}
			return invokeBinder.FallbackInvoke(Restrict(this.GetLimitType()), args);
		}
		if (action is PythonInvokeBinder pythonInvokeBinder)
		{
			if (_baseMetaObject is IPythonInvokable pythonInvokable)
			{
				return pythonInvokable.Invoke(pythonInvokeBinder, codeContext, this, args);
			}
			if (_baseMetaObject != null)
			{
				return pythonInvokeBinder.InvokeForeignObject(this, args);
			}
			return pythonInvokeBinder.Fallback(codeContext, this, args);
		}
		throw new InvalidOperationException();
	}

	private DynamicMetaObject TryPythonConversion(DynamicMetaObjectBinder conversion, Type type)
	{
		if (!type.IsEnum())
		{
			switch (type.GetTypeCode())
			{
			case TypeCode.Object:
				if (type == typeof(Complex))
				{
					return MakeConvertRuleForCall(conversion, type, this, "__complex__", "ConvertToComplex", () => MakeConvertRuleForCall(conversion, type, this, "__float__", "ConvertToFloat", () => FallbackConvert(conversion), (Expression x) => Expression.Call(null, typeof(PythonOps).GetMethod("ConvertFloatToComplex"), x)), (Expression x) => x);
				}
				if (type == typeof(BigInteger))
				{
					return MakeConvertRuleForCall(conversion, type, this, "__long__", "ConvertToLong");
				}
				if (type == typeof(IEnumerable))
				{
					return PythonConversionBinder.ConvertToIEnumerable(conversion, Restrict(Value.GetType()));
				}
				if (type == typeof(IEnumerator))
				{
					return PythonConversionBinder.ConvertToIEnumerator(conversion, Restrict(Value.GetType()));
				}
				if (type.IsSubclassOf(typeof(Delegate)))
				{
					return MetaPythonObject.MakeDelegateTarget(conversion, type, Restrict(Value.GetType()));
				}
				break;
			case TypeCode.Int32:
				return MakeConvertRuleForCall(conversion, type, this, "__int__", "ConvertToInt");
			case TypeCode.Double:
				return MakeConvertRuleForCall(conversion, type, this, "__float__", "ConvertToFloat");
			case TypeCode.Boolean:
				return PythonProtocol.ConvertToBool(conversion, this);
			case TypeCode.String:
				if (!typeof(Extensible<string>).IsAssignableFrom(base.LimitType))
				{
					return MakeConvertRuleForCall(conversion, type, this, "__str__", "ConvertToString");
				}
				break;
			}
		}
		return null;
	}

	private DynamicMetaObject MakeConvertRuleForCall(DynamicMetaObjectBinder convertToAction, Type toType, DynamicMetaObject self, string name, string returner, Func<DynamicMetaObject> fallback, Func<Expression, Expression> resultConverter)
	{
		PythonType pythonType = ((IPythonObject)self.Value).PythonType;
		CodeContext sharedContext = PythonContext.GetPythonContext(convertToAction).SharedContext;
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(this, pythonType);
		if (pythonType.TryResolveSlot(sharedContext, name, out var slot) && !IsBuiltinConversion(sharedContext, slot, name, pythonType))
		{
			ParameterExpression parameterExpression = Expression.Variable(typeof(object), "func");
			Expression expression = resultConverter(Expression.Call(PythonOps.GetConversionHelper(returner, GetResultKind(convertToAction)), Expression.Dynamic(PythonContext.GetPythonContext(convertToAction).InvokeNone, typeof(object), PythonContext.GetCodeContext(convertToAction), parameterExpression)));
			if (typeof(Extensible<>).MakeGenericType(toType).IsAssignableFrom(self.GetLimitType()))
			{
				expression = Utils.Convert(AddExtensibleSelfCheck(convertToAction, toType, self, expression), typeof(object));
			}
			return BindingHelpers.AddDynamicTestAndDefer(convertToAction, new DynamicMetaObject(Expression.Condition(MetaPythonObject.MakeTryGetTypeMember(PythonContext.GetPythonContext(convertToAction), slot, self.Expression, parameterExpression), expression, Utils.Convert(ConversionFallback(convertToAction), typeof(object))), self.Restrict(self.GetRuntimeType()).Restrictions), new DynamicMetaObject[1] { this }, validationInfo, parameterExpression);
		}
		return fallback();
	}

	private DynamicMetaObject MakeConvertRuleForCall(DynamicMetaObjectBinder convertToAction, Type toType, DynamicMetaObject self, string name, string returner)
	{
		return MakeConvertRuleForCall(convertToAction, toType, self, name, returner, () => FallbackConvert(convertToAction), (Expression x) => x);
	}

	private static Expression AddExtensibleSelfCheck(DynamicMetaObjectBinder convertToAction, Type toType, DynamicMetaObject self, Expression callExpr)
	{
		ParameterExpression parameterExpression = Expression.Variable(callExpr.Type, "tmp");
		ConversionResultKind resultKind = GetResultKind(convertToAction);
		Type type = ((resultKind == ConversionResultKind.ExplicitTry || resultKind == ConversionResultKind.ImplicitTry) ? typeof(object) : toType);
		callExpr = Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Block(Expression.Assign(parameterExpression, callExpr), Expression.Condition(Expression.Equal(parameterExpression, self.Expression), Utils.Convert(Expression.Property(Utils.Convert(self.Expression, self.GetLimitType()), self.GetLimitType().GetProperty("Value")), type), Expression.Dynamic(new PythonConversionBinder(PythonContext.GetPythonContext(convertToAction), toType, GetResultKind(convertToAction)), type, parameterExpression))));
		return callExpr;
	}

	private static ConversionResultKind GetResultKind(DynamicMetaObjectBinder convertToAction)
	{
		if (convertToAction is PythonConversionBinder pythonConversionBinder)
		{
			return pythonConversionBinder.ResultKind;
		}
		if (((ConvertBinder)convertToAction).Explicit)
		{
			return ConversionResultKind.ExplicitCast;
		}
		return ConversionResultKind.ImplicitCast;
	}

	private Expression ConversionFallback(DynamicMetaObjectBinder convertToAction)
	{
		if (convertToAction is PythonConversionBinder convertToAction2)
		{
			return GetConversionFailedReturnValue(convertToAction2, this).Expression;
		}
		return convertToAction.GetUpdateExpression(typeof(object));
	}

	private static bool IsBuiltinConversion(CodeContext context, PythonTypeSlot pts, string name, PythonType selfType)
	{
		Type type = selfType.UnderlyingSystemType.BaseType;
		Type type2 = type;
		do
		{
			if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(Extensible<>))
			{
				type = type2.GetGenericArguments()[0];
				break;
			}
			type2 = type2.BaseType;
		}
		while (type2 != null);
		PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(type);
		if (pythonTypeFromType.TryResolveSlot(context, name, out var slot) && pts == slot)
		{
			return true;
		}
		return false;
	}

	private static DynamicMetaObject GetConversionFailedReturnValue(PythonConversionBinder convertToAction, DynamicMetaObject self)
	{
		switch (convertToAction.ResultKind)
		{
		case ConversionResultKind.ImplicitTry:
		case ConversionResultKind.ExplicitTry:
			return new DynamicMetaObject(DefaultBinder.GetTryConvertReturnValue(convertToAction.Type), BindingRestrictions.Empty);
		case ConversionResultKind.ImplicitCast:
		case ConversionResultKind.ExplicitCast:
		{
			DefaultBinder binder = PythonContext.GetPythonContext(convertToAction).Binder;
			return DefaultBinder.MakeError(binder.MakeConversionError(convertToAction.Type, self.Expression), typeof(object));
		}
		default:
			throw new InvalidOperationException(convertToAction.ResultKind.ToString());
		}
	}

	private DynamicMetaObject Fallback(DynamicMetaObjectBinder action, DynamicMetaObject codeContext)
	{
		if (_baseMetaObject != null)
		{
			if (_baseMetaObject is IPythonGetable pythonGetable && action is PythonGetMemberBinder member)
			{
				return pythonGetable.GetMember(member, codeContext);
			}
			if (action is GetMemberBinder binder)
			{
				return _baseMetaObject.BindGetMember(binder);
			}
			return _baseMetaObject.BindGetMember(PythonContext.GetPythonContext(action).CompatGetMember(MetaPythonObject.GetGetMemberName(action), isNoThrow: false));
		}
		return MetaPythonObject.GetMemberFallback(this, action, codeContext);
	}

	private DynamicMetaObject Fallback(SetMemberBinder action, DynamicMetaObject value)
	{
		if (_baseMetaObject != null)
		{
			return _baseMetaObject.BindSetMember(action, value);
		}
		return action.FallbackSetMember(this, value);
	}

	DynamicMetaObject IPythonOperable.BindOperation(PythonOperationBinder action, DynamicMetaObject[] args)
	{
		if (action.Operation == PythonOperationKind.IsCallable)
		{
			DynamicMetaObject dynamicMetaObject = Restrict(Value.GetType());
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("UserObjectIsCallable"), Utils.Constant(PythonContext.GetPythonContext(action).SharedContext), dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
		}
		return null;
	}

	public DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext)
	{
		return GetMemberWorker(member, codeContext);
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder action)
	{
		return GetMemberWorker(action, PythonContext.GetCodeContextMO(action));
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder action, DynamicMetaObject value)
	{
		return new MetaSetBinderHelper(this, value, action).Bind(action.Name);
	}

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder action)
	{
		return MakeDeleteMemberRule(new DeleteBindingInfo(action, new DynamicMetaObject[1] { this }, new ConditionalBuilder(action), BindingHelpers.GetValidationInfo(this, base.PythonType)));
	}

	private DynamicMetaObject GetMemberWorker(DynamicMetaObjectBinder member, DynamicMetaObject codeContext)
	{
		return new GetBinderHelper(this, member, codeContext).Bind((CodeContext)codeContext.Value, MetaPythonObject.GetGetMemberName(member));
	}

	private static bool TryGetGetAttribute(CodeContext context, PythonType type, out PythonTypeSlot dts)
	{
		if (type.TryResolveSlot(context, "__getattribute__", out dts) && (!(dts is BuiltinMethodDescriptor builtinMethodDescriptor) || builtinMethodDescriptor.DeclaringType != typeof(object) || builtinMethodDescriptor.Template.Targets.Count != 1 || builtinMethodDescriptor.Template.Targets[0].DeclaringType != typeof(ObjectOps) || builtinMethodDescriptor.Template.Targets[0].Name != "__getattribute__"))
		{
			return dts != null;
		}
		return false;
	}

	private static MethodCallExpression MakeGetAttrTestAndGet(GetBindingInfo info, Expression getattr)
	{
		return Expression.Call(IronPython.Runtime.Types.TypeInfo._PythonOps.SlotTryGetBoundValue, Utils.Constant(PythonContext.GetPythonContext(info.Action).SharedContext), Utils.Convert(getattr, typeof(PythonTypeSlot)), Utils.Convert(info.Self, typeof(object)), Expression.Convert(Expression.Property(Expression.Convert(info.Self, typeof(IPythonObject)), IronPython.Runtime.Types.TypeInfo._IPythonObject.PythonType), typeof(PythonType)), info.Result);
	}

	private static Expression GetWeakSlot(PythonTypeSlot slot)
	{
		return Utils.Convert(Utils.WeakConstant(slot), typeof(PythonTypeSlot));
	}

	private static Expression MakeTypeError(DynamicMetaObjectBinder binder, string name, PythonType type)
	{
		return binder.Throw(Expression.Call(typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[2]
		{
			typeof(string),
			typeof(string)
		}), Utils.Constant(type.Name), Utils.Constant(name)), typeof(object));
	}

	private static bool IsStandardObjectMethod(PythonTypeSlot dts)
	{
		if (!(dts is BuiltinMethodDescriptor builtinMethodDescriptor))
		{
			return false;
		}
		return builtinMethodDescriptor.Template.Targets[0].DeclaringType == typeof(ObjectOps);
	}

	private static void MakeSlotsDeleteTarget(MemberBindingInfo info, ReflectedSlotProperty rsp)
	{
		MakeSlotsSetTargetHelper(info, rsp, Expression.Field(null, typeof(Uninitialized).GetField("Instance")));
	}

	private static void MakeSlotsSetTargetHelper(MemberBindingInfo info, ReflectedSlotProperty rsp, Expression value)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "res");
		info.Body.AddVariable(parameterExpression);
		info.Body.FinishCondition(Expression.Block(Expression.Assign(parameterExpression, Expression.Convert(Expression.Assign(Expression.ArrayAccess(GetSlots(info.Args[0]), Utils.Constant(rsp.Index)), Utils.Convert(value, typeof(object))), parameterExpression.Type)), parameterExpression));
	}

	private static DynamicMetaObject MakeSlotSet(SetBindingInfo info, PythonTypeSlot dts)
	{
		ParameterExpression parameterExpression = Expression.Variable(info.Args[1].Expression.Type, "res");
		info.Body.AddVariable(parameterExpression);
		if (dts.GetType() == typeof(PythonProperty))
		{
			Expression right = Expression.Property(Expression.Convert(Utils.WeakConstant(dts), typeof(PythonProperty)), "fset");
			ParameterExpression parameterExpression2 = Expression.Variable(typeof(object), "tmpSet");
			info.Body.AddVariable(parameterExpression2);
			info.Body.FinishCondition(Expression.Block(Expression.Assign(parameterExpression2, right), Expression.Condition(Expression.NotEqual(parameterExpression2, Utils.Constant(null)), Expression.Block(Expression.Assign(parameterExpression, info.Args[1].Expression), Expression.Dynamic(PythonContext.GetPythonContext(info.Action).InvokeOne, typeof(object), Utils.Constant(PythonContext.GetPythonContext(info.Action).SharedContext), parameterExpression2, info.Args[0].Expression, Utils.Convert(parameterExpression, typeof(object))), Expression.Convert(parameterExpression, typeof(object))), info.Action.Throw(Expression.Call(typeof(PythonOps).GetMethod("UnsetableProperty")), typeof(object)))));
			return info.Body.GetMetaObject();
		}
		CodeContext sharedContext = PythonContext.GetPythonContext(info.Action).SharedContext;
		info.Body.AddCondition(Expression.Block(Expression.Assign(parameterExpression, info.Args[1].Expression), Expression.Call(typeof(PythonOps).GetMethod("SlotTrySetValue"), Utils.Constant(sharedContext), Utils.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)), Utils.Convert(info.Args[0].Expression, typeof(object)), Expression.Convert(Expression.Property(Expression.Convert(info.Args[0].Expression, typeof(IPythonObject)), IronPython.Runtime.Types.TypeInfo._IPythonObject.PythonType), typeof(PythonType)), Utils.Convert(parameterExpression, typeof(object)))), Utils.Convert(parameterExpression, typeof(object)));
		return null;
	}

	private DynamicMetaObject MakeDeleteMemberRule(DeleteBindingInfo info)
	{
		CodeContext sharedContext = PythonContext.GetPythonContext(info.Action).SharedContext;
		DynamicMetaObject dynamicMetaObject = info.Args[0].Restrict(info.Args[0].GetRuntimeType());
		IPythonObject pythonObject = info.Args[0].Value as IPythonObject;
		if (info.Action.Name == "__class__")
		{
			return new DynamicMetaObject(info.Action.Throw(Expression.New(typeof(TypeErrorException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant("can't delete __class__ attribute")), typeof(object)), dynamicMetaObject.Restrictions);
		}
		if (pythonObject.PythonType.TryResolveSlot(sharedContext, "__delattr__", out var slot) && !IsStandardObjectMethod(slot))
		{
			MakeDeleteAttrTarget(info, pythonObject, slot);
		}
		pythonObject.PythonType.TryResolveSlot(sharedContext, info.Action.Name, out slot);
		if (slot is ReflectedSlotProperty rsp)
		{
			MakeSlotsDeleteTarget(info, rsp);
		}
		if (!info.Body.IsFinal && slot != null)
		{
			MakeSlotDelete(info, slot);
		}
		if (!info.Body.IsFinal && pythonObject.PythonType.HasDictionary)
		{
			MakeDictionaryDeleteTarget(info);
		}
		if (!info.Body.IsFinal)
		{
			info.Body.FinishCondition(FallbackDeleteError(info.Action, info.Args).Expression);
		}
		DynamicMetaObject metaObject = info.Body.GetMetaObject(info.Args);
		metaObject = new DynamicMetaObject(metaObject.Expression, dynamicMetaObject.Restrictions.Merge(metaObject.Restrictions));
		return BindingHelpers.AddDynamicTestAndDefer(info.Action, metaObject, info.Args, info.Validation);
	}

	private static DynamicMetaObject MakeSlotDelete(DeleteBindingInfo info, PythonTypeSlot dts)
	{
		if (dts.GetType() == typeof(PythonProperty))
		{
			Expression right = Expression.Property(Expression.Convert(Utils.WeakConstant(dts), typeof(PythonProperty)), "fdel");
			ParameterExpression parameterExpression = Expression.Variable(typeof(object), "tmpDel");
			info.Body.AddVariable(parameterExpression);
			info.Body.FinishCondition(Expression.Block(Expression.Assign(parameterExpression, right), Expression.Condition(Expression.NotEqual(parameterExpression, Utils.Constant(null)), Expression.Dynamic(PythonContext.GetPythonContext(info.Action).InvokeOne, typeof(object), Utils.Constant(PythonContext.GetPythonContext(info.Action).SharedContext), parameterExpression, info.Args[0].Expression), info.Action.Throw(Expression.Call(typeof(PythonOps).GetMethod("UndeletableProperty")), typeof(object)))));
			return info.Body.GetMetaObject();
		}
		info.Body.AddCondition(Expression.Call(typeof(PythonOps).GetMethod("SlotTryDeleteValue"), Utils.Constant(PythonContext.GetPythonContext(info.Action).SharedContext), Utils.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)), Utils.Convert(info.Args[0].Expression, typeof(object)), Expression.Convert(Expression.Property(Expression.Convert(info.Args[0].Expression, typeof(IPythonObject)), IronPython.Runtime.Types.TypeInfo._IPythonObject.PythonType), typeof(PythonType))), Utils.Constant(null));
		return null;
	}

	private static void MakeDeleteAttrTarget(DeleteBindingInfo info, IPythonObject self, PythonTypeSlot dts)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "boundVal");
		info.Body.AddVariable(parameterExpression);
		info.Body.AddCondition(Expression.Call(IronPython.Runtime.Types.TypeInfo._PythonOps.SlotTryGetBoundValue, Utils.Constant(PythonContext.GetPythonContext(info.Action).SharedContext), Utils.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)), Utils.Convert(info.Args[0].Expression, typeof(object)), Utils.Convert(Utils.WeakConstant(self.PythonType), typeof(PythonType)), parameterExpression), Expression.Dynamic(PythonContext.GetPythonContext(info.Action).InvokeOne, typeof(object), PythonContext.GetCodeContext(info.Action), parameterExpression, Utils.Constant(info.Action.Name)));
	}

	private static void MakeDictionaryDeleteTarget(DeleteBindingInfo info)
	{
		info.Body.FinishCondition(Expression.Call(typeof(UserTypeOps).GetMethod("RemoveDictionaryValue"), Expression.Convert(info.Args[0].Expression, typeof(IPythonObject)), Utils.Constant(info.Action.Name)));
	}

	private static PythonTypeSlot FindSlot(CodeContext context, string name, IPythonObject sdo, out bool isOldStyle, out bool systemTypeResolution, out bool extensionMethodResolution)
	{
		PythonTypeSlot slot = null;
		isOldStyle = false;
		systemTypeResolution = false;
		foreach (PythonType item in sdo.PythonType.ResolutionOrder)
		{
			if (item.IsOldClass)
			{
				isOldStyle = true;
			}
			if (item.TryLookupSlot(context, name, out slot))
			{
				if (!(slot is ClassMethodDescriptor))
				{
					systemTypeResolution = item.IsSystemType;
				}
				break;
			}
		}
		extensionMethodResolution = false;
		if (slot == null)
		{
			extensionMethodResolution = true;
			MemberGroup member = context.ModuleContext.ExtensionMethods.GetBinder(context.LanguageContext).GetMember(MemberRequestKind.Get, sdo.PythonType.UnderlyingSystemType, name);
			if (member.Count > 0)
			{
				slot = PythonTypeOps.GetSlot(member, name, privateBinding: false);
			}
		}
		return slot;
	}

	private DynamicMetaObject FallbackGetError(DynamicMetaObjectBinder action, DynamicMetaObject codeContext)
	{
		if (_baseMetaObject != null)
		{
			return Fallback(action, codeContext);
		}
		if (BindingHelpers.IsNoThrow(action))
		{
			return new DynamicMetaObject(Expression.Field(null, typeof(OperationFailed).GetField("Value")), BindingRestrictions.Empty);
		}
		if (action is PythonGetMemberBinder)
		{
			return new DynamicMetaObject(MakeTypeError(action, MetaPythonObject.GetGetMemberName(action), base.PythonType), BindingRestrictions.Empty);
		}
		return MetaPythonObject.GetMemberFallback(this, action, codeContext);
	}

	private DynamicMetaObject FallbackDeleteError(DeleteMemberBinder action, DynamicMetaObject[] args)
	{
		if (_baseMetaObject != null)
		{
			return _baseMetaObject.BindDeleteMember(action);
		}
		if (action is PythonDeleteMemberBinder)
		{
			return new DynamicMetaObject(MakeTypeError(action, action.Name, ((IPythonObject)args[0].Value).PythonType), BindingRestrictions.Empty);
		}
		return action.FallbackDeleteMember(Restrict(this.GetLimitType()));
	}

	private static Expression GetSlots(DynamicMetaObject self)
	{
		FieldInfo field = self.LimitType.GetField(".slots_and_weakref");
		if (field != null)
		{
			return Expression.Field(Expression.Convert(self.Expression, self.LimitType), field);
		}
		return Expression.Call(Expression.Convert(self.Expression, typeof(IPythonObject)), typeof(IPythonObject).GetMethod("GetSlots"));
	}
}
