using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class MetaPythonType : MetaPythonObject, IPythonInvokable, IPythonConvertible, IPythonGetable
{
	private class CallAdapter
	{
		private readonly ArgumentValues _argInfo;

		private readonly PythonContext _state;

		private readonly Expression _context;

		protected PythonContext PythonContext => _state;

		protected Expression CodeContext => _context;

		protected ArgumentValues Arguments => _argInfo;

		public CallAdapter(ArgumentValues ai, PythonContext state, Expression codeContext)
		{
			_argInfo = ai;
			_state = state;
			_context = codeContext;
		}
	}

	private class ArgumentValues
	{
		public readonly DynamicMetaObject Self;

		public readonly DynamicMetaObject[] Arguments;

		public readonly CallSignature Signature;

		public ArgumentValues(CallSignature signature, DynamicMetaObject self, DynamicMetaObject[] args)
		{
			Self = self;
			Signature = signature;
			Arguments = args;
		}
	}

	private class NewAdapter : CallAdapter
	{
		public NewAdapter(ArgumentValues ai, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
		}

		public virtual DynamicMetaObject GetExpression(PythonBinder binder)
		{
			return MakeDefaultNew(binder, Expression.Call(typeof(PythonOps).GetMethod("PythonTypeGetMember"), base.CodeContext, Utils.Convert(base.Arguments.Self.Expression, typeof(PythonType)), Utils.Constant(null), Utils.Constant("__new__")));
		}

		protected DynamicMetaObject MakeDefaultNew(DefaultBinder binder, Expression function)
		{
			List<Expression> list = new List<Expression>();
			list.Add(base.CodeContext);
			list.Add(function);
			AppendNewArgs(list);
			return new DynamicMetaObject(Expression.Dynamic(base.PythonContext.Invoke(GetDynamicNewSignature()), typeof(object), list.ToArray()), base.Arguments.Self.Restrictions);
		}

		private void AppendNewArgs(List<Expression> args)
		{
			args.Add(base.Arguments.Self.Expression);
			DynamicMetaObject[] arguments = base.Arguments.Arguments;
			foreach (DynamicMetaObject dynamicMetaObject in arguments)
			{
				args.Add(dynamicMetaObject.Expression);
			}
		}

		protected CallSignature GetDynamicNewSignature()
		{
			return base.Arguments.Signature.InsertArgument(Argument.Simple);
		}
	}

	private class DefaultNewAdapter : NewAdapter
	{
		private readonly PythonType _creating;

		public DefaultNewAdapter(ArgumentValues ai, PythonType creating, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
			_creating = creating;
		}

		public override DynamicMetaObject GetExpression(PythonBinder binder)
		{
			PythonOverloadResolver resolver = ((!_creating.IsSystemType && !_creating.HasSystemCtor) ? new PythonOverloadResolver(binder, new DynamicMetaObject[1] { base.Arguments.Self }, new CallSignature(1), base.CodeContext) : new PythonOverloadResolver(binder, DynamicMetaObject.EmptyMetaObjects, new CallSignature(0), base.CodeContext));
			return binder.CallMethod(resolver, _creating.UnderlyingSystemType.GetConstructors(), BindingRestrictions.Empty, _creating.Name);
		}
	}

	private class ConstructorNewAdapter : NewAdapter
	{
		private readonly PythonType _creating;

		public ConstructorNewAdapter(ArgumentValues ai, PythonType creating, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
			_creating = creating;
		}

		public override DynamicMetaObject GetExpression(PythonBinder binder)
		{
			PythonOverloadResolver resolver = ((!_creating.IsSystemType && !_creating.HasSystemCtor) ? new PythonOverloadResolver(binder, ArrayUtils.Insert(base.Arguments.Self, base.Arguments.Arguments), GetDynamicNewSignature(), base.CodeContext) : new PythonOverloadResolver(binder, base.Arguments.Arguments, base.Arguments.Signature, base.CodeContext));
			return binder.CallMethod(resolver, _creating.UnderlyingSystemType.GetConstructors(), base.Arguments.Self.Restrictions, _creating.Name);
		}
	}

	private class BuiltinNewAdapter : NewAdapter
	{
		private readonly PythonType _creating;

		private readonly BuiltinFunction _ctor;

		public BuiltinNewAdapter(ArgumentValues ai, PythonType creating, BuiltinFunction ctor, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
			_creating = creating;
			_ctor = ctor;
		}

		public override DynamicMetaObject GetExpression(PythonBinder binder)
		{
			return binder.CallMethod(new PythonOverloadResolver(binder, ArrayUtils.Insert(base.Arguments.Self, base.Arguments.Arguments), base.Arguments.Signature.InsertArgument(new Argument(ArgumentType.Simple)), base.CodeContext), _ctor.Targets, _creating.Name);
		}
	}

	private class MixedNewAdapter : NewAdapter
	{
		public MixedNewAdapter(ArgumentValues ai, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
		}

		public override DynamicMetaObject GetExpression(PythonBinder binder)
		{
			return MakeDefaultNew(binder, Expression.Call(typeof(PythonOps).GetMethod("GetMixedMember"), base.CodeContext, base.Arguments.Self.Expression, Utils.Constant(null), Utils.Constant("__new__")));
		}
	}

	private abstract class InitAdapter : CallAdapter
	{
		protected InitAdapter(ArgumentValues ai, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
		}

		public abstract DynamicMetaObject MakeInitCall(PythonBinder binder, DynamicMetaObject createExpr);

		protected DynamicMetaObject MakeDefaultInit(PythonBinder binder, DynamicMetaObject createExpr, Expression init)
		{
			List<Expression> list = new List<Expression>();
			list.Add(base.CodeContext);
			list.Add(Expression.Convert(createExpr.Expression, typeof(object)));
			DynamicMetaObject[] arguments = base.Arguments.Arguments;
			foreach (DynamicMetaObject dynamicMetaObject in arguments)
			{
				list.Add(dynamicMetaObject.Expression);
			}
			return new DynamicMetaObject(Expression.Dynamic(((PythonType)base.Arguments.Self.Value).GetLateBoundInitBinder(base.Arguments.Signature), typeof(object), list.ToArray()), base.Arguments.Self.Restrictions.Merge(createExpr.Restrictions));
		}
	}

	private class SlotInitAdapter : InitAdapter
	{
		private readonly PythonTypeSlot _slot;

		public SlotInitAdapter(PythonTypeSlot slot, ArgumentValues ai, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
			_slot = slot;
		}

		public override DynamicMetaObject MakeInitCall(PythonBinder binder, DynamicMetaObject createExpr)
		{
			Expression init = Expression.Call(typeof(PythonOps).GetMethod("GetInitSlotMember"), base.CodeContext, Expression.Convert(base.Arguments.Self.Expression, typeof(PythonType)), Expression.Convert(Utils.WeakConstant(_slot), typeof(PythonTypeSlot)), Utils.Convert(createExpr.Expression, typeof(object)));
			return MakeDefaultInit(binder, createExpr, init);
		}
	}

	private class DefaultInitAdapter : InitAdapter
	{
		public DefaultInitAdapter(ArgumentValues ai, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
		}

		public override DynamicMetaObject MakeInitCall(PythonBinder binder, DynamicMetaObject createExpr)
		{
			return createExpr;
		}
	}

	private class BuiltinInitAdapter : InitAdapter
	{
		private readonly BuiltinFunction _method;

		public BuiltinInitAdapter(ArgumentValues ai, BuiltinFunction method, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
			_method = method;
		}

		public override DynamicMetaObject MakeInitCall(PythonBinder binder, DynamicMetaObject createExpr)
		{
			if (_method == InstanceOps.Init.Template)
			{
				return createExpr;
			}
			return binder.CallMethod(new PythonOverloadResolver(binder, createExpr, base.Arguments.Arguments, base.Arguments.Signature, base.CodeContext), _method.Targets, base.Arguments.Self.Restrictions);
		}
	}

	private class MixedInitAdapter : InitAdapter
	{
		public MixedInitAdapter(ArgumentValues ai, PythonContext state, Expression codeContext)
			: base(ai, state, codeContext)
		{
		}

		public override DynamicMetaObject MakeInitCall(PythonBinder binder, DynamicMetaObject createExpr)
		{
			Expression init = Expression.Call(typeof(PythonOps).GetMethod("GetMixedMember"), base.CodeContext, Expression.Convert(base.Arguments.Self.Expression, typeof(PythonType)), Utils.Convert(createExpr.Expression, typeof(object)), Utils.Constant("__init__"));
			return MakeDefaultInit(binder, createExpr, init);
		}
	}

	public abstract class GetBinderHelper<TResult>
	{
		private readonly PythonType _value;

		private readonly string _name;

		internal readonly CodeContext _context;

		protected PythonType Value => _value;

		public GetBinderHelper(PythonType value, CodeContext context, string name)
		{
			_value = value;
			_name = name;
			_context = context;
		}

		protected abstract TResult Finish(bool metaOnly);

		protected abstract void AddError();

		protected abstract void AddMetaGetAttribute(PythonType metaType, PythonTypeSlot pts);

		protected abstract bool AddMetaSlotAccess(PythonType pt, PythonTypeSlot pts);

		protected abstract void AddMetaOldClassAccess();

		protected abstract bool AddSlotAccess(PythonType pt, PythonTypeSlot pts);

		protected abstract void AddOldClassAccess(PythonType pt);

		public TResult MakeTypeGetMember()
		{
			bool flag = false;
			bool metaOnly = false;
			CodeContext sharedClsContext = PythonContext.GetContext(_context).SharedClsContext;
			PythonType pythonType = DynamicHelpers.GetPythonType(Value);
			PythonTypeSlot slot;
			foreach (PythonType item in pythonType.ResolutionOrder)
			{
				if (item.TryLookupSlot(sharedClsContext, _name, out slot) && slot.IsSetDescriptor(sharedClsContext, pythonType) && AddMetaSlotAccess(pythonType, slot))
				{
					metaOnly = (flag = true);
					break;
				}
			}
			if (!flag)
			{
				foreach (PythonType item2 in Value.ResolutionOrder)
				{
					if (item2.IsOldClass)
					{
						AddOldClassAccess(item2);
					}
					else if (item2.TryLookupSlot(sharedClsContext, _name, out slot) && AddSlotAccess(item2, slot))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				foreach (PythonType item3 in pythonType.ResolutionOrder)
				{
					if (item3.OldClass != null)
					{
						AddMetaOldClassAccess();
						flag = true;
						break;
					}
					if (item3.TryLookupSlot(sharedClsContext, _name, out slot) && AddMetaSlotAccess(pythonType, slot))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag && pythonType.TryResolveSlot(_context, "__getattr__", out slot) && !slot.IsSetDescriptor(sharedClsContext, pythonType))
			{
				AddMetaGetAttribute(pythonType, slot);
				flag = slot.GetAlwaysSucceeds;
			}
			if (!flag)
			{
				AddError();
			}
			return Finish(metaOnly);
		}
	}

	private class MetaGetBinderHelper : GetBinderHelper<DynamicMetaObject>
	{
		private readonly DynamicMetaObjectBinder _member;

		private readonly MetaPythonType _type;

		private readonly Expression _codeContext;

		private readonly DynamicMetaObject _restrictedSelf;

		private readonly ConditionalBuilder _cb;

		private readonly string _symName;

		private readonly PythonContext _state;

		private readonly ValidationInfo _valInfo;

		private readonly ValidationInfo _metaValInfo;

		private ParameterExpression _tmp;

		private Expression Expression => _type.Expression;

		private BindingRestrictions Restrictions => _type.Restrictions;

		public MetaGetBinderHelper(MetaPythonType type, DynamicMetaObjectBinder member, Expression codeContext, ValidationInfo validationInfo, ValidationInfo metaValidation)
			: base(type.Value, PythonContext.GetPythonContext(member).SharedContext, MetaPythonObject.GetGetMemberName(member))
		{
			_member = member;
			_codeContext = codeContext;
			_type = type;
			_cb = new ConditionalBuilder(member);
			_symName = MetaPythonObject.GetGetMemberName(member);
			_restrictedSelf = new DynamicMetaObject(Utils.Convert(Expression, base.Value.GetType()), Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(Expression, base.Value)), base.Value);
			_state = PythonContext.GetPythonContext(member);
			_valInfo = validationInfo;
			_metaValInfo = metaValidation;
		}

		protected override void AddOldClassAccess(PythonType pt)
		{
			EnsureTmp();
			_cb.AddCondition(Expression.Call(typeof(PythonOps).GetMethod("OldClassTryLookupOneSlot"), Utils.Constant(pt), Utils.Constant(pt.OldClass), Utils.Constant(_symName), _tmp), _tmp);
		}

		private void EnsureTmp()
		{
			if (_tmp == null)
			{
				_tmp = Expression.Variable(typeof(object), "tmp");
				_cb.AddVariable(_tmp);
			}
		}

		protected override bool AddSlotAccess(PythonType pt, PythonTypeSlot pts)
		{
			pts.MakeGetExpression(_state.Binder, _codeContext, null, new DynamicMetaObject(Utils.Convert(Utils.WeakConstant(base.Value), typeof(PythonType)), BindingRestrictions.Empty, base.Value), _cb);
			if (!pts.IsAlwaysVisible)
			{
				_cb.AddCondition(Expression.Call(typeof(PythonOps).GetMethod("IsClsVisible"), _codeContext));
				return false;
			}
			return pts.GetAlwaysSucceeds;
		}

		protected override void AddMetaOldClassAccess()
		{
			_cb.FinishCondition(Expression.Call(Utils.Convert(Expression, typeof(PythonType)), typeof(PythonType).GetMethod("__getattribute__"), _codeContext, Utils.Constant(MetaPythonObject.GetGetMemberName(_member))));
		}

		protected override void AddError()
		{
			_cb.FinishCondition(GetFallbackError(_member).Expression);
		}

		protected override void AddMetaGetAttribute(PythonType metaType, PythonTypeSlot pts)
		{
			EnsureTmp();
			_cb.AddCondition(Expression.Call(typeof(PythonOps).GetMethod("SlotTryGetBoundValue"), _codeContext, Utils.Constant(pts, typeof(PythonTypeSlot)), Expression, Utils.Constant(metaType), _tmp), Expression.Dynamic(_state.InvokeOne, typeof(object), _codeContext, _tmp, Utils.Constant(MetaPythonObject.GetGetMemberName(_member))));
		}

		protected override bool AddMetaSlotAccess(PythonType metaType, PythonTypeSlot pts)
		{
			pts.MakeGetExpression(_state.Binder, _codeContext, _type, new DynamicMetaObject(Utils.Constant(metaType), BindingRestrictions.Empty, metaType), _cb);
			if (!pts.IsAlwaysVisible)
			{
				_cb.AddCondition(Expression.Call(typeof(PythonOps).GetMethod("IsClsVisible"), _codeContext));
				return false;
			}
			return pts.GetAlwaysSucceeds;
		}

		protected override DynamicMetaObject Finish(bool metaOnly)
		{
			DynamicMetaObject dynamicMetaObject = _cb.GetMetaObject(_restrictedSelf);
			if (metaOnly)
			{
				dynamicMetaObject = BindingHelpers.AddDynamicTestAndDefer(_member, dynamicMetaObject, new DynamicMetaObject[1] { _type }, _metaValInfo);
			}
			else if (!base.Value.IsSystemType)
			{
				dynamicMetaObject = BindingHelpers.AddDynamicTestAndDefer(_member, dynamicMetaObject, new DynamicMetaObject[1] { _type }, _valInfo);
			}
			return dynamicMetaObject;
		}

		private DynamicMetaObject GetFallbackError(DynamicMetaObjectBinder member)
		{
			if (member is PythonGetMemberBinder)
			{
				PythonGetMemberBinder pythonGetMemberBinder = member as PythonGetMemberBinder;
				if (pythonGetMemberBinder.IsNoThrow)
				{
					return new DynamicMetaObject(Expression.Constant(OperationFailed.Value), BindingRestrictions.GetInstanceRestriction(Expression, base.Value).Merge(Restrictions));
				}
				return new DynamicMetaObject(member.Throw(Expression.Call(typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[2]
				{
					typeof(string),
					typeof(string)
				}), Utils.Constant(DynamicHelpers.GetPythonType(base.Value).Name), Utils.Constant(pythonGetMemberBinder.Name)), typeof(object)), BindingRestrictions.GetInstanceRestriction(Expression, base.Value).Merge(Restrictions));
			}
			return ((GetMemberBinder)member).FallbackGetMember(_type);
		}
	}

	internal class FastGetBinderHelper : GetBinderHelper<TypeGetBase>
	{
		private class OldClassDelegate
		{
			private readonly WeakReference _type;

			private readonly WeakReference _declType;

			private readonly string _name;

			public OldClassDelegate(PythonType declType, PythonType oldClass, string name)
			{
				_type = oldClass.GetSharedWeakReference();
				_declType = declType.GetSharedWeakReference();
				_name = name;
			}

			public bool Target(CodeContext context, object self, out object result)
			{
				return PythonOps.OldClassTryLookupOneSlot((PythonType)_declType.Target, ((PythonType)_type.Target).OldClass, _name, out result);
			}
		}

		private class SlotAccessDelegate
		{
			private readonly PythonTypeSlot _slot;

			private readonly PythonType _owner;

			private readonly WeakReference _weakOwner;

			private readonly WeakReference _weakSlot;

			private PythonType Type => _owner ?? ((PythonType)_weakOwner.Target);

			private PythonTypeSlot Slot => _slot ?? ((PythonTypeSlot)_weakSlot.Target);

			public SlotAccessDelegate(PythonTypeSlot slot, PythonType owner)
			{
				if (owner.IsSystemType)
				{
					_owner = owner;
					_slot = slot;
				}
				else
				{
					_weakOwner = owner.GetSharedWeakReference();
					_weakSlot = new WeakReference(slot);
				}
			}

			public bool TargetCheckCls(CodeContext context, object self, out object result)
			{
				if (PythonOps.IsClsVisible(context))
				{
					return Slot.TryGetValue(context, null, Type, out result);
				}
				result = null;
				return false;
			}

			public bool Target(CodeContext context, object self, out object result)
			{
				return Slot.TryGetValue(context, null, Type, out result);
			}

			public bool MetaTargetCheckCls(CodeContext context, object self, out object result)
			{
				if (PythonOps.IsClsVisible(context))
				{
					return Slot.TryGetValue(context, self, Type, out result);
				}
				result = null;
				return false;
			}

			public bool MetaTarget(CodeContext context, object self, out object result)
			{
				return Slot.TryGetValue(context, self, Type, out result);
			}
		}

		private class MetaOldClassDelegate
		{
			private readonly string _name;

			public MetaOldClassDelegate(string name)
			{
				_name = name;
			}

			public bool Target(CodeContext context, object self, out object result)
			{
				result = ((PythonType)self).__getattribute__(context, _name);
				return true;
			}
		}

		private class MetaGetAttributeDelegate
		{
			private readonly string _name;

			private readonly PythonType _metaType;

			private readonly WeakReference _weakMetaType;

			private readonly PythonTypeSlot _slot;

			private readonly WeakReference _weakSlot;

			private readonly CallSite<Func<CallSite, CodeContext, object, string, object>> _invokeSite;

			private PythonType MetaType => _metaType ?? ((PythonType)_weakMetaType.Target);

			private PythonTypeSlot Slot => _slot ?? ((PythonTypeSlot)_weakSlot.Target);

			public MetaGetAttributeDelegate(CodeContext context, PythonTypeSlot slot, PythonType metaType, string name)
			{
				_name = name;
				if (metaType.IsSystemType)
				{
					_metaType = metaType;
					_slot = slot;
				}
				else
				{
					_weakMetaType = metaType.GetSharedWeakReference();
					_weakSlot = new WeakReference(slot);
				}
				_invokeSite = CallSite<Func<CallSite, CodeContext, object, string, object>>.Create(PythonContext.GetContext(context).InvokeOne);
			}

			public bool Target(CodeContext context, object self, out object result)
			{
				if (Slot.TryGetValue(context, self, MetaType, out var value))
				{
					result = _invokeSite.Target(_invokeSite, context, value, _name);
					return true;
				}
				result = null;
				return false;
			}
		}

		private class ErrorBinder
		{
			private readonly string _name;

			public ErrorBinder(string name)
			{
				_name = name;
			}

			public bool TargetNoThrow(CodeContext context, object self, out object result)
			{
				result = OperationFailed.Value;
				return true;
			}

			public bool Target(CodeContext context, object self, out object result)
			{
				throw PythonOps.AttributeErrorForObjectMissingAttribute(self, _name);
			}
		}

		private readonly PythonGetMemberBinder _binder;

		private readonly int _version;

		private readonly int _metaVersion;

		private bool _canOptimize;

		private List<FastGetDelegate> _gets = new List<FastGetDelegate>();

		public FastGetBinderHelper(PythonType type, CodeContext context, PythonGetMemberBinder binder)
			: base(type, context, binder.Name)
		{
			_version = type.Version;
			_metaVersion = DynamicHelpers.GetPythonType(type).Version;
			_binder = binder;
		}

		public Func<CallSite, object, CodeContext, object> GetBinding()
		{
			Dictionary<string, TypeGetBase> cachedGets = GetCachedGets();
			TypeGetBase value;
			lock (cachedGets)
			{
				if (!cachedGets.TryGetValue(_binder.Name, out value) || !value.IsValid(base.Value))
				{
					TypeGetBase typeGetBase = MakeTypeGetMember();
					if (typeGetBase != null)
					{
						TypeGetBase typeGetBase2 = (cachedGets[_binder.Name] = typeGetBase);
						value = typeGetBase2;
					}
				}
			}
			if (value != null && value.ShouldUseNonOptimizedSite)
			{
				return value._func;
			}
			return null;
		}

		private Dictionary<string, TypeGetBase> GetCachedGets()
		{
			if (_binder.IsNoThrow)
			{
				Dictionary<string, TypeGetBase> cachedTypeTryGets = base.Value._cachedTypeTryGets;
				if (cachedTypeTryGets == null)
				{
					Interlocked.CompareExchange(ref base.Value._cachedTypeTryGets, new Dictionary<string, TypeGetBase>(), null);
					cachedTypeTryGets = base.Value._cachedTypeTryGets;
				}
				return cachedTypeTryGets;
			}
			Dictionary<string, TypeGetBase> cachedTypeGets = base.Value._cachedTypeGets;
			if (cachedTypeGets == null)
			{
				Interlocked.CompareExchange(ref base.Value._cachedTypeGets, new Dictionary<string, TypeGetBase>(), null);
				cachedTypeGets = base.Value._cachedTypeGets;
			}
			return cachedTypeGets;
		}

		protected override void AddOldClassAccess(PythonType pt)
		{
			_gets.Add(new OldClassDelegate(base.Value, pt, _binder.Name).Target);
		}

		protected override bool AddSlotAccess(PythonType pt, PythonTypeSlot pts)
		{
			if (pts.CanOptimizeGets)
			{
				_canOptimize = true;
			}
			if (pts.IsAlwaysVisible)
			{
				_gets.Add(new SlotAccessDelegate(pts, base.Value).Target);
				return pts.GetAlwaysSucceeds;
			}
			_gets.Add(new SlotAccessDelegate(pts, base.Value).TargetCheckCls);
			return false;
		}

		protected override void AddMetaOldClassAccess()
		{
			_gets.Add(new MetaOldClassDelegate(_binder.Name).Target);
		}

		protected override void AddError()
		{
			if (_binder.IsNoThrow)
			{
				_gets.Add(new ErrorBinder(_binder.Name).TargetNoThrow);
			}
			else
			{
				_gets.Add(new ErrorBinder(_binder.Name).Target);
			}
		}

		protected override void AddMetaGetAttribute(PythonType metaType, PythonTypeSlot pts)
		{
			_gets.Add(new MetaGetAttributeDelegate(_context, pts, metaType, _binder.Name).Target);
		}

		protected override bool AddMetaSlotAccess(PythonType metaType, PythonTypeSlot pts)
		{
			if (pts.CanOptimizeGets)
			{
				_canOptimize = true;
			}
			if (pts.IsAlwaysVisible)
			{
				_gets.Add(new SlotAccessDelegate(pts, metaType).MetaTarget);
				return pts.GetAlwaysSucceeds;
			}
			_gets.Add(new SlotAccessDelegate(pts, metaType).MetaTargetCheckCls);
			return false;
		}

		protected override TypeGetBase Finish(bool metaOnly)
		{
			if (metaOnly)
			{
				if (DynamicHelpers.GetPythonType(base.Value).IsSystemType)
				{
					return new SystemTypeGet(_binder, _gets.ToArray(), base.Value, metaOnly, _canOptimize);
				}
				return new TypeGet(_binder, _gets.ToArray(), metaOnly ? _metaVersion : _version, metaOnly, _canOptimize);
			}
			if (base.Value.IsSystemType)
			{
				return new SystemTypeGet(_binder, _gets.ToArray(), base.Value, metaOnly, _canOptimize);
			}
			return new TypeGet(_binder, _gets.ToArray(), metaOnly ? _metaVersion : _version, metaOnly, _canOptimize);
		}
	}

	public new PythonType Value => (PythonType)base.Value;

	public DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		DynamicMetaObject dynamicMetaObject = BuiltinFunction.TranslateArguments(pythonInvoke, codeContext, target, args, hasSelf: false, Value.Name);
		if (dynamicMetaObject != null)
		{
			return dynamicMetaObject;
		}
		return InvokeWorker(pythonInvoke, args, codeContext);
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args)
	{
		foreach (PythonType item in Value.ResolutionOrder)
		{
			if (item.IsSystemType)
			{
				return action.FallbackInvokeMember(this, args);
			}
			if (item.TryResolveSlot(DefaultContext.DefaultCLS, action.Name, out var _) || item.IsOldClass)
			{
				break;
			}
		}
		return BindingHelpers.GenericInvokeMember(action, null, this, args);
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder call, DynamicMetaObject[] args)
	{
		return InvokeWorker(call, args, PythonContext.GetCodeContext(call));
	}

	private DynamicMetaObject InvokeWorker(DynamicMetaObjectBinder call, DynamicMetaObject[] args, Expression codeContext)
	{
		if (this.NeedsDeferral())
		{
			return call.Defer(ArrayUtils.Insert(this, args));
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].NeedsDeferral())
			{
				return call.Defer(ArrayUtils.Insert(this, args));
			}
		}
		DynamicMetaObject res = ((!IsStandardDotNetType(call)) ? MakePythonTypeCall(call, codeContext, args) : MakeStandardDotNetTypeCall(call, codeContext, args));
		return BindingHelpers.AddPythonBoxing(res);
	}

	private DynamicMetaObject MakeStandardDotNetTypeCall(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject[] args)
	{
		CallSignature callSignature = BindingHelpers.GetCallSignature(call);
		PythonContext pythonContext = PythonContext.GetPythonContext(call);
		MethodBase[] constructors = CompilerHelpers.GetConstructors(Value.UnderlyingSystemType, pythonContext.Binder.PrivateBinding);
		if (constructors.Length > 0)
		{
			return pythonContext.Binder.CallMethod(new PythonOverloadResolver(pythonContext.Binder, args, callSignature, codeContext), constructors, base.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(base.Expression, Value)));
		}
		string value = ((!Value.UnderlyingSystemType.IsAbstract()) ? $"Cannot create instances of {Value.Name} because it has no public constructors" : $"Cannot create instances of {Value.Name} because it is abstract");
		return new DynamicMetaObject(call.Throw(Expression.New(typeof(TypeErrorException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant(value))), base.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(base.Expression, Value)));
	}

	private DynamicMetaObject MakePythonTypeCall(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject[] args)
	{
		ValidationInfo validationInfo = MakeVersionCheck();
		DynamicMetaObject dynamicMetaObject = new RestrictedMetaObject(Utils.Convert(base.Expression, base.LimitType), BindingRestrictionsHelpers.GetRuntimeTypeRestriction(base.Expression, base.LimitType), Value);
		CallSignature callSignature = BindingHelpers.GetCallSignature(call);
		ArgumentValues ai = new ArgumentValues(callSignature, dynamicMetaObject, args);
		if (TooManyArgsForDefaultNew(call, args))
		{
			return MakeIncorrectArgumentsForCallError(call, ai, validationInfo);
		}
		if (Value.UnderlyingSystemType.IsGenericTypeDefinition())
		{
			return MakeGenericTypeDefinitionError(call, ai, validationInfo);
		}
		if (Value.HasAbstractMethods(PythonContext.GetPythonContext(call).SharedContext))
		{
			return MakeAbstractInstantiationError(call, ai, validationInfo);
		}
		DynamicMetaObject dynamicMetaObject2 = BuiltinFunction.TranslateArguments(call, codeContext, dynamicMetaObject, args, hasSelf: false, Value.Name);
		if (dynamicMetaObject2 != null)
		{
			return dynamicMetaObject2;
		}
		GetAdapters(ai, call, codeContext, out var newAdapter, out var initAdapter);
		PythonContext pythonContext = PythonContext.GetPythonContext(call);
		DynamicMetaObject expression = newAdapter.GetExpression(pythonContext.Binder);
		if (expression.Expression.Type == typeof(void))
		{
			return BindingHelpers.AddDynamicTestAndDefer(call, expression, args, validationInfo);
		}
		BindingRestrictions empty = BindingRestrictions.Empty;
		Expression expression2;
		if (!Value.IsSystemType && (!(newAdapter is DefaultNewAdapter) || HasFinalizer(call)))
		{
			expression2 = Expression.Dynamic(Value.GetLateBoundInitBinder(callSignature), typeof(object), ArrayUtils.Insert(codeContext, Expression.Convert(expression.Expression, typeof(object)), DynamicUtils.GetExpressions(args)));
			empty = expression.Restrictions;
		}
		else
		{
			ParameterExpression parameterExpression = Expression.Variable(expression.GetLimitType(), "newInst");
			Expression expression3 = parameterExpression;
			DynamicMetaObject dynamicMetaObject3 = initAdapter.MakeInitCall(pythonContext.Binder, new RestrictedMetaObject(Utils.Convert(parameterExpression, Value.UnderlyingSystemType), expression.Restrictions));
			List<Expression> list = new List<Expression>();
			if (dynamicMetaObject3.Expression != expression3)
			{
				DynamicMetaObject dynamicMetaObject4 = dynamicMetaObject3;
				if (list.Count == 0)
				{
					list.Add(Expression.Assign(parameterExpression, expression.Expression));
				}
				if (!Value.UnderlyingSystemType.IsAssignableFrom(expression.Expression.Type))
				{
					list.Add(Utils.IfThen(Expression.TypeIs(parameterExpression, Value.UnderlyingSystemType), dynamicMetaObject4.Expression));
				}
				else
				{
					list.Add(dynamicMetaObject4.Expression);
				}
			}
			if (list.Count == 0)
			{
				expression2 = expression.Expression;
			}
			else
			{
				list.Add(parameterExpression);
				expression2 = Expression.Block(list);
			}
			expression2 = Expression.Block(new ParameterExpression[1] { parameterExpression }, expression2);
			empty = dynamicMetaObject3.Restrictions;
		}
		return BindingHelpers.AddDynamicTestAndDefer(call, new DynamicMetaObject(expression2, dynamicMetaObject.Restrictions.Merge(empty)), ArrayUtils.Insert(this, args), validationInfo);
	}

	private void GetAdapters(ArgumentValues ai, DynamicMetaObjectBinder call, Expression codeContext, out NewAdapter newAdapter, out InitAdapter initAdapter)
	{
		Value.TryResolveSlot(PythonContext.GetPythonContext(call).SharedContext, "__new__", out var slot);
		Value.TryResolveSlot(PythonContext.GetPythonContext(call).SharedContext, "__init__", out var slot2);
		newAdapter = GetNewAdapter(ai, slot, call, codeContext);
		initAdapter = GetInitAdapter(ai, slot2, call, codeContext);
	}

	private InitAdapter GetInitAdapter(ArgumentValues ai, PythonTypeSlot init, DynamicMetaObjectBinder call, Expression codeContext)
	{
		PythonContext pythonContext = PythonContext.GetPythonContext(call);
		if (Value.IsMixedNewStyleOldStyle())
		{
			return new MixedInitAdapter(ai, pythonContext, codeContext);
		}
		if ((init == InstanceOps.Init && !HasFinalizer(call)) || (Value == TypeCache.PythonType && ai.Arguments.Length == 2))
		{
			return new DefaultInitAdapter(ai, pythonContext, codeContext);
		}
		if (init is BuiltinMethodDescriptor)
		{
			return new BuiltinInitAdapter(ai, ((BuiltinMethodDescriptor)init).Template, pythonContext, codeContext);
		}
		if (init is BuiltinFunction)
		{
			return new BuiltinInitAdapter(ai, (BuiltinFunction)init, pythonContext, codeContext);
		}
		return new SlotInitAdapter(init, ai, pythonContext, codeContext);
	}

	private NewAdapter GetNewAdapter(ArgumentValues ai, PythonTypeSlot newInst, DynamicMetaObjectBinder call, Expression codeContext)
	{
		PythonContext pythonContext = PythonContext.GetPythonContext(call);
		if (Value.IsMixedNewStyleOldStyle())
		{
			return new MixedNewAdapter(ai, pythonContext, codeContext);
		}
		if (newInst == InstanceOps.New)
		{
			return new DefaultNewAdapter(ai, Value, pythonContext, codeContext);
		}
		if (newInst is ConstructorFunction)
		{
			return new ConstructorNewAdapter(ai, Value, pythonContext, codeContext);
		}
		if (newInst is BuiltinFunction)
		{
			return new BuiltinNewAdapter(ai, Value, (BuiltinFunction)newInst, pythonContext, codeContext);
		}
		return new NewAdapter(ai, pythonContext, codeContext);
	}

	private DynamicMetaObject MakeIncorrectArgumentsForCallError(DynamicMetaObjectBinder call, ArgumentValues ai, ValidationInfo valInfo)
	{
		string value = ((!Value.IsSystemType) ? "object.__new__() takes no parameters" : ((Value.UnderlyingSystemType.GetConstructors().Length != 0) ? "object.__new__() takes no parameters" : ("cannot create instances of " + Value.Name)));
		return BindingHelpers.AddDynamicTestAndDefer(call, new DynamicMetaObject(call.Throw(Expression.New(typeof(TypeErrorException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant(value))), GetErrorRestrictions(ai)), ai.Arguments, valInfo);
	}

	private DynamicMetaObject MakeGenericTypeDefinitionError(DynamicMetaObjectBinder call, ArgumentValues ai, ValidationInfo valInfo)
	{
		string value = "cannot create instances of " + Value.Name + " because it is a generic type definition";
		return BindingHelpers.AddDynamicTestAndDefer(call, new DynamicMetaObject(call.Throw(Expression.New(typeof(TypeErrorException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant(value)), typeof(object)), GetErrorRestrictions(ai)), ai.Arguments, valInfo);
	}

	private DynamicMetaObject MakeAbstractInstantiationError(DynamicMetaObjectBinder call, ArgumentValues ai, ValidationInfo valInfo)
	{
		CodeContext sharedContext = PythonContext.GetPythonContext(call).SharedContext;
		string abstractErrorMessage = Value.GetAbstractErrorMessage(sharedContext);
		return BindingHelpers.AddDynamicTestAndDefer(call, new DynamicMetaObject(Expression.Throw(Expression.New(typeof(ArgumentTypeException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant(abstractErrorMessage)), typeof(object)), GetErrorRestrictions(ai)), ai.Arguments, valInfo);
	}

	private BindingRestrictions GetErrorRestrictions(ArgumentValues ai)
	{
		BindingRestrictions restrictions = Restrict(this.GetRuntimeType()).Restrictions;
		restrictions = restrictions.Merge(GetInstanceRestriction(ai));
		DynamicMetaObject[] arguments = ai.Arguments;
		foreach (DynamicMetaObject dynamicMetaObject in arguments)
		{
			if (dynamicMetaObject.HasValue)
			{
				restrictions = restrictions.Merge(dynamicMetaObject.Restrict(dynamicMetaObject.GetRuntimeType()).Restrictions);
			}
		}
		return restrictions;
	}

	private static BindingRestrictions GetInstanceRestriction(ArgumentValues ai)
	{
		return BindingRestrictions.GetInstanceRestriction(ai.Self.Expression, ai.Self.Value);
	}

	private bool HasFinalizer(DynamicMetaObjectBinder action)
	{
		if (Value.IsSystemType)
		{
			return false;
		}
		PythonTypeSlot slot;
		return Value.TryResolveSlot(PythonContext.GetPythonContext(action).SharedContext, "__del__", out slot);
	}

	private bool HasDefaultNew(DynamicMetaObjectBinder action)
	{
		Value.TryResolveSlot(PythonContext.GetPythonContext(action).SharedContext, "__new__", out var slot);
		return slot == InstanceOps.New;
	}

	private bool HasDefaultInit(DynamicMetaObjectBinder action)
	{
		Value.TryResolveSlot(PythonContext.GetPythonContext(action).SharedContext, "__init__", out var slot);
		return slot == InstanceOps.Init;
	}

	private bool HasDefaultNewAndInit(DynamicMetaObjectBinder action)
	{
		if (HasDefaultNew(action))
		{
			return HasDefaultInit(action);
		}
		return false;
	}

	private bool TooManyArgsForDefaultNew(DynamicMetaObjectBinder action, DynamicMetaObject[] args)
	{
		if (args.Length > 0 && HasDefaultNewAndInit(action))
		{
			Argument[] argumentInfos = BindingHelpers.GetCallSignature(action).GetArgumentInfos();
			for (int i = 0; i < argumentInfos.Length; i++)
			{
				Argument argument = argumentInfos[i];
				switch (argument.Kind)
				{
				case ArgumentType.List:
					if (((IList<object>)args[i].Value).Count > 0)
					{
						return true;
					}
					break;
				case ArgumentType.Dictionary:
					if (PythonOps.Length(args[i].Value) > 0)
					{
						return true;
					}
					break;
				default:
					return true;
				}
			}
		}
		return false;
	}

	private ValidationInfo MakeVersionCheck()
	{
		int version = Value.Version;
		return new ValidationInfo(Expression.Equal(Expression.Call(typeof(PythonOps).GetMethod("GetTypeVersion"), Expression.Convert(base.Expression, typeof(PythonType))), Utils.Constant(version)));
	}

	private bool IsStandardDotNetType(DynamicMetaObjectBinder action)
	{
		PythonContext pythonContext = PythonContext.GetPythonContext(action);
		if (Value.IsSystemType && !Value.IsPythonType && !pythonContext.Binder.HasExtensionTypes(Value.UnderlyingSystemType) && !typeof(Delegate).IsAssignableFrom(Value.UnderlyingSystemType))
		{
			return !Value.UnderlyingSystemType.IsArray;
		}
		return false;
	}

	public MetaPythonType(Expression expression, BindingRestrictions restrictions, PythonType value)
		: base(expression, BindingRestrictions.Empty, value)
	{
	}

	public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder create, DynamicMetaObject[] args)
	{
		return InvokeWorker(create, args, Utils.Constant(PythonContext.GetPythonContext(create).SharedContext));
	}

	public override DynamicMetaObject BindConvert(ConvertBinder conversion)
	{
		return ConvertWorker(conversion, conversion.Type, conversion.Explicit ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast);
	}

	public DynamicMetaObject BindConvert(PythonConversionBinder binder)
	{
		return ConvertWorker(binder, binder.Type, binder.ResultKind);
	}

	public DynamicMetaObject ConvertWorker(DynamicMetaObjectBinder binder, Type type, ConversionResultKind kind)
	{
		if (type.IsSubclassOf(typeof(Delegate)))
		{
			return MetaPythonObject.MakeDelegateTarget(binder, type, Restrict(Value.GetType()));
		}
		return FallbackConvert(binder);
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		PythonContext pc = Value.PythonContext ?? DefaultContext.DefaultPythonContext;
		foreach (object o in Value.GetMemberNames(pc.SharedContext))
		{
			if (o is string)
			{
				yield return (string)o;
			}
		}
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder member)
	{
		return GetMemberWorker(member, PythonContext.GetCodeContext(member));
	}

	private ValidationInfo GetTypeTest()
	{
		int version = Value.Version;
		return new ValidationInfo(Expression.Call(typeof(PythonOps).GetMethod("CheckSpecificTypeVersion"), Utils.Convert(base.Expression, typeof(PythonType)), Utils.Constant(version)));
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder member, DynamicMetaObject value)
	{
		PythonContext pythonContext = PythonContext.GetPythonContext(member);
		if (Value.IsSystemType)
		{
			MemberTracker value2 = MemberTracker.FromMemberInfo(Value.UnderlyingSystemType);
			MemberGroup member2 = pythonContext.Binder.GetMember(MemberRequestKind.Set, Value.UnderlyingSystemType, member.Name);
			foreach (MemberTracker item in member2)
			{
				if (IsProtectedSetter(item))
				{
					return new DynamicMetaObject(BindingHelpers.TypeErrorForProtectedMember(Value.UnderlyingSystemType, member.Name), base.Restrictions.Merge(value.Restrictions).Merge(BindingRestrictions.GetInstanceRestriction(base.Expression, Value)));
				}
			}
			return new DynamicMetaObject(pythonContext.Binder.SetMember(member.Name, new DynamicMetaObject(Utils.Constant(value2), BindingRestrictions.Empty, value2), value, new PythonOverloadResolverFactory(pythonContext.Binder, Utils.Constant(pythonContext.SharedContext))).Expression, base.Restrictions.Merge(value.Restrictions).Merge(BindingRestrictions.GetInstanceRestriction(base.Expression, Value)));
		}
		return MakeSetMember(member, value);
	}

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder member)
	{
		if (Value.IsSystemType)
		{
			PythonContext pythonContext = PythonContext.GetPythonContext(member);
			MemberTracker value = MemberTracker.FromMemberInfo(Value.UnderlyingSystemType);
			return new DynamicMetaObject(pythonContext.Binder.DeleteMember(member.Name, new DynamicMetaObject(Utils.Constant(value), BindingRestrictions.Empty, value), pythonContext.SharedOverloadResolverFactory).Expression, BindingRestrictions.GetInstanceRestriction(base.Expression, Value).Merge(base.Restrictions));
		}
		return MakeDeleteMember(member);
	}

	public DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext)
	{
		return GetMemberWorker(member, codeContext.Expression);
	}

	private DynamicMetaObject GetMemberWorker(DynamicMetaObjectBinder member, Expression codeContext)
	{
		return new MetaGetBinderHelper(this, member, codeContext, GetTypeTest(), MakeMetaTypeTest(Restrict(this.GetRuntimeType()).Expression)).MakeTypeGetMember();
	}

	private ValidationInfo MakeMetaTypeTest(Expression self)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(Value);
		if (!pythonType.IsSystemType)
		{
			int version = pythonType.Version;
			return new ValidationInfo(Expression.Call(typeof(PythonOps).GetMethod("CheckTypeVersion"), self, Utils.Constant(version)));
		}
		return ValidationInfo.Empty;
	}

	private DynamicMetaObject MakeSetMember(SetMemberBinder member, DynamicMetaObject value)
	{
		PythonContext pythonContext = PythonContext.GetPythonContext(member);
		DynamicMetaObject dynamicMetaObject = Restrict(Value.GetType());
		if (Value.GetType() != typeof(PythonType) && DynamicHelpers.GetPythonType(Value).IsSystemType && Value.TryGetCustomSetAttr(pythonContext.SharedContext, out var pts))
		{
			ParameterExpression parameterExpression = Expression.Variable(typeof(object), "boundVal");
			return BindingHelpers.AddDynamicTestAndDefer(member, new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Dynamic(pythonContext.Invoke(new CallSignature(2)), typeof(object), Utils.Constant(pythonContext.SharedContext), Expression.Block(Expression.Call(typeof(PythonOps).GetMethod("SlotTryGetValue"), Utils.Constant(pythonContext.SharedContext), Utils.Convert(Utils.WeakConstant(pts), typeof(PythonTypeSlot)), Utils.Convert(base.Expression, typeof(object)), Utils.Convert(Utils.WeakConstant(DynamicHelpers.GetPythonType(Value)), typeof(PythonType)), parameterExpression), parameterExpression), Expression.Constant(member.Name), value.Expression)), dynamicMetaObject.Restrictions), new DynamicMetaObject[2] { this, value }, TestUserType());
		}
		return BindingHelpers.AddDynamicTestAndDefer(member, new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("PythonTypeSetCustomMember"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), dynamicMetaObject.Expression, Utils.Constant(member.Name), Utils.Convert(value.Expression, typeof(object))), dynamicMetaObject.Restrictions.Merge(value.Restrictions)), new DynamicMetaObject[2] { this, value }, TestUserType());
	}

	private static bool IsProtectedSetter(MemberTracker mt)
	{
		if (mt is PropertyTracker propertyTracker)
		{
			MethodInfo setMethod = propertyTracker.GetSetMethod(privateMembers: true);
			if (setMethod != null && setMethod.IsProtected())
			{
				return true;
			}
		}
		if (mt is FieldTracker fieldTracker)
		{
			return fieldTracker.Field.IsProtected();
		}
		return false;
	}

	private DynamicMetaObject MakeDeleteMember(DeleteMemberBinder member)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(Value.GetType());
		return BindingHelpers.AddDynamicTestAndDefer(member, new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("PythonTypeDeleteCustomMember"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), dynamicMetaObject.Expression, Utils.Constant(member.Name)), dynamicMetaObject.Restrictions), new DynamicMetaObject[1] { this }, TestUserType());
	}

	private ValidationInfo TestUserType()
	{
		return new ValidationInfo(Expression.Not(Expression.Call(typeof(PythonOps).GetMethod("IsPythonType"), Utils.Convert(base.Expression, typeof(PythonType)))));
	}
}
