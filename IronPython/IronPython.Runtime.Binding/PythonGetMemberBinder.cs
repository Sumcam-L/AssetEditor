using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonGetMemberBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable, ILightExceptionBinder
{
	private class FastErrorGet<TSelfType> : FastGetBase
	{
		private readonly Type _type;

		private readonly string _name;

		private readonly ExtensionMethodSet _extMethods;

		public FastErrorGet(Type type, string name, ExtensionMethodSet extMethodSet)
		{
			_type = type;
			_name = name;
			_extMethods = extMethodSet;
		}

		public override bool IsValid(PythonType type)
		{
			return true;
		}

		public object GetError(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type && (object)_extMethods == context.ModuleContext.ExtensionMethods)
			{
				throw PythonOps.AttributeErrorForObjectMissingAttribute(target, _name);
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}

		public object GetErrorLightThrow(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type && (object)_extMethods == context.ModuleContext.ExtensionMethods)
			{
				return LightExceptions.Throw(PythonOps.AttributeErrorForObjectMissingAttribute(target, _name));
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}

		public object GetErrorNoThrow(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type && (object)_extMethods == context.ModuleContext.ExtensionMethods)
			{
				return OperationFailed.Value;
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}

		public object GetAmbiguous(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type && (object)_extMethods == context.ModuleContext.ExtensionMethods)
			{
				throw new AmbiguousMatchException(_name);
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}
	}

	private class BuiltinBase<TSelfType> : FastGetBase
	{
		public override bool IsValid(PythonType type)
		{
			return true;
		}
	}

	private class FastMethodGet<TSelfType> : BuiltinBase<TSelfType>
	{
		private readonly Type _type;

		private readonly BuiltinMethodDescriptor _method;

		public FastMethodGet(Type type, BuiltinMethodDescriptor method)
		{
			_type = type;
			_method = method;
		}

		public object GetMethod(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type)
			{
				return _method.UncheckedGetAttribute(target);
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}
	}

	private class FastSlotGet<TSelfType> : BuiltinBase<TSelfType>
	{
		private readonly Type _type;

		private readonly PythonTypeSlot _slot;

		private readonly PythonType _owner;

		public FastSlotGet(Type type, PythonTypeSlot slot, PythonType owner)
		{
			_type = type;
			_slot = slot;
			_owner = owner;
		}

		public object GetRetSlot(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type)
			{
				return _slot;
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}

		public object GetBindSlot(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type)
			{
				_slot.TryGetValue(context, target, _owner, out var value);
				return value;
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}
	}

	private class FastTypeGet<TSelfType> : BuiltinBase<TSelfType>
	{
		private readonly Type _type;

		private readonly object _pyType;

		public FastTypeGet(Type type, object pythonType)
		{
			_type = type;
			_pyType = pythonType;
		}

		public object GetTypeObject(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type)
			{
				return _pyType;
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}
	}

	private class FastPropertyGet<TSelfType> : BuiltinBase<TSelfType>
	{
		private readonly Type _type;

		private readonly Func<object, object> _propGetter;

		public FastPropertyGet(Type type, Func<object, object> propGetter)
		{
			_type = type;
			_propGetter = propGetter;
		}

		public object GetProperty(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type)
			{
				return _propGetter(target);
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}

		public object GetPropertyBool(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type)
			{
				return ScriptingRuntimeHelpers.BooleanToObject((bool)_propGetter(target));
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}

		public object GetPropertyInt(CallSite site, TSelfType target, CodeContext context)
		{
			if (target != null && target.GetType() == _type)
			{
				return ScriptingRuntimeHelpers.Int32ToObject((int)_propGetter(target));
			}
			return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
		}
	}

	private class PythonModuleDelegate : FastGetBase
	{
		private readonly string _name;

		public PythonModuleDelegate(string name)
		{
			_name = name;
		}

		public object Target(CallSite site, object self, CodeContext context)
		{
			if (self != null && self.GetType() == typeof(PythonModule))
			{
				return ((PythonModule)self).__getattribute__(context, _name);
			}
			return FastGetBase.Update(site, self, context);
		}

		public object NoThrowTarget(CallSite site, object self, CodeContext context)
		{
			if (self != null && self.GetType() == typeof(PythonModule))
			{
				return ((PythonModule)self).GetAttributeNoThrow(context, _name);
			}
			return FastGetBase.Update(site, self, context);
		}

		public object LightThrowTarget(CallSite site, object self, CodeContext context)
		{
			if (self != null && self.GetType() == typeof(PythonModule))
			{
				object attributeNoThrow = ((PythonModule)self).GetAttributeNoThrow(context, _name);
				if (attributeNoThrow == OperationFailed.Value)
				{
					return LightExceptions.Throw(PythonOps.AttributeErrorForObjectMissingAttribute(self, _name));
				}
				return attributeNoThrow;
			}
			return FastGetBase.Update(site, self, context);
		}

		public override bool IsValid(PythonType type)
		{
			return true;
		}
	}

	private class NamespaceTrackerDelegate : FastGetBase
	{
		private readonly string _name;

		public NamespaceTrackerDelegate(string name)
		{
			_name = name;
		}

		public object Target(CallSite site, object self, CodeContext context)
		{
			if (self != null && self.GetType() == typeof(NamespaceTracker))
			{
				object customMember = NamespaceTrackerOps.GetCustomMember(context, (NamespaceTracker)self, _name);
				if (customMember != OperationFailed.Value)
				{
					return customMember;
				}
				throw PythonOps.AttributeErrorForMissingAttribute(self, _name);
			}
			return FastGetBase.Update(site, self, context);
		}

		public object NoThrowTarget(CallSite site, object self, CodeContext context)
		{
			if (self != null && self.GetType() == typeof(NamespaceTracker))
			{
				return NamespaceTrackerOps.GetCustomMember(context, (NamespaceTracker)self, _name);
			}
			return FastGetBase.Update(site, self, context);
		}

		public object GetName(CallSite site, object self, CodeContext context)
		{
			if (self != null && self.GetType() == typeof(NamespaceTracker))
			{
				return NamespaceTrackerOps.Get__name__(context, (NamespaceTracker)self);
			}
			return FastGetBase.Update(site, self, context);
		}

		public object GetFile(CallSite site, object self, CodeContext context)
		{
			if (self != null && self.GetType() == typeof(NamespaceTracker))
			{
				return NamespaceTrackerOps.Get__file__((NamespaceTracker)self);
			}
			return FastGetBase.Update(site, self, context);
		}

		public object GetDict(CallSite site, object self, CodeContext context)
		{
			if (self != null && self.GetType() == typeof(NamespaceTracker))
			{
				return NamespaceTrackerOps.Get__dict__(context, (NamespaceTracker)self);
			}
			return FastGetBase.Update(site, self, context);
		}

		public override bool IsValid(PythonType type)
		{
			return true;
		}
	}

	private class LightThrowBinder : PythonGetMemberBinder
	{
		public override bool SupportsLightThrow => true;

		public LightThrowBinder(PythonContext context, string name, bool isNoThrow)
			: base(context, name, isNoThrow)
		{
		}

		public override CallSiteBinder GetLightExceptionBinder()
		{
			return this;
		}
	}

	private readonly PythonContext _context;

	private readonly GetMemberOptions _options;

	private readonly string _name;

	private LightThrowBinder _lightThrowBinder;

	public string Name => _name;

	public PythonContext Context => _context;

	public bool IsNoThrow => (_options & GetMemberOptions.IsNoThrow) != 0;

	public virtual bool SupportsLightThrow => false;

	public PythonGetMemberBinder(PythonContext context, string name)
	{
		_context = context;
		_name = name;
	}

	public PythonGetMemberBinder(PythonContext context, string name, bool isNoThrow)
		: this(context, name)
	{
		_options = (isNoThrow ? GetMemberOptions.IsNoThrow : GetMemberOptions.None);
	}

	public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		DynamicMetaObject codeContext = args[0];
		if (target is IPythonGetable pythonGetable)
		{
			return pythonGetable.GetMember(this, codeContext);
		}
		if (target.Value is IDynamicMetaObjectProvider)
		{
			return GetForeignObject(target);
		}
		if (ComBinder.IsComObject(target.Value))
		{
			return GetForeignObject(target);
		}
		return Fallback(target, codeContext);
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		if (args[0] is IFastGettable fastGettable)
		{
			T val = fastGettable.MakeGetBinding(site, this, (CodeContext)args[1], Name);
			if (val != null)
			{
				return val;
			}
			return base.BindDelegate(site, args);
		}
		if (args[0] is IPythonObject self && !(args[0] is IProxyObject))
		{
			FastBindResult<T> fastBindResult = UserTypeOps.MakeGetBinding((CodeContext)args[1], site, self, this);
			if (fastBindResult.Target != null)
			{
				if (fastBindResult.ShouldCache)
				{
					CacheTarget(fastBindResult.Target);
				}
				return fastBindResult.Target;
			}
			return base.BindDelegate(site, args);
		}
		if (args[0] != null)
		{
			if (args[0].GetType() == typeof(PythonModule))
			{
				if (SupportsLightThrow)
				{
					return (T)(object)new Func<CallSite, object, CodeContext, object>(new PythonModuleDelegate(_name).LightThrowTarget);
				}
				if (!IsNoThrow)
				{
					return (T)(object)new Func<CallSite, object, CodeContext, object>(new PythonModuleDelegate(_name).Target);
				}
				return (T)(object)new Func<CallSite, object, CodeContext, object>(new PythonModuleDelegate(_name).NoThrowTarget);
			}
			if (args[0].GetType() == typeof(NamespaceTracker))
			{
				switch (Name)
				{
				case "__file__":
					return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).GetFile);
				case "__dict__":
					return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).GetDict);
				case "__name__":
					return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).GetName);
				default:
					if (IsNoThrow)
					{
						return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).NoThrowTarget);
					}
					return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).Target);
				case "__str__":
				case "__repr__":
				case "__doc__":
					break;
				}
			}
		}
		if (args[0] != null && !ComBinder.IsComObject(args[0]) && !(args[0] is IDynamicMetaObjectProvider))
		{
			Type parameterType = typeof(T).GetMethod("Invoke").GetParameters()[1].ParameterType;
			CodeContext context = (CodeContext)args[1];
			T val2 = null;
			if (parameterType == typeof(object))
			{
				val2 = (T)(object)MakeGetMemberTarget<object>(Name, args[0], context);
			}
			else if (parameterType == typeof(List))
			{
				val2 = (T)(object)MakeGetMemberTarget<List>(Name, args[0], context);
			}
			else if (parameterType == typeof(string))
			{
				val2 = (T)(object)MakeGetMemberTarget<string>(Name, args[0], context);
			}
			if (val2 != null)
			{
				return (T)val2;
			}
			return base.BindDelegate(site, args);
		}
		return this.LightBind<T>(args, Context.Options.CompilationThreshold);
	}

	private Func<CallSite, TSelfType, CodeContext, object> MakeGetMemberTarget<TSelfType>(string name, object target, CodeContext context)
	{
		Type type = CompilerHelpers.GetType(target);
		if (typeof(TypeTracker).IsAssignableFrom(type))
		{
			return null;
		}
		MemberGroup member = Context.Binder.GetMember(MemberRequestKind.Get, type, name);
		if (member.Count == 0 && type.IsInterface())
		{
			type = typeof(object);
			member = Context.Binder.GetMember(MemberRequestKind.Get, type, name);
		}
		if (member.Count == 0 && typeof(IStrongBox).IsAssignableFrom(type))
		{
			return null;
		}
		MethodInfo method = Context.Binder.GetMethod(type, "GetCustomMember");
		if (method != null && method.IsSpecialName)
		{
			return null;
		}
		Expression error;
		TrackerTypes memberType = Context.Binder.GetMemberType(member, out error);
		if (error == null)
		{
			PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(type);
			if (pythonTypeFromType.IsHiddenMember(name))
			{
				return null;
			}
			switch (memberType)
			{
			case TrackerTypes.Type:
			case TrackerTypes.TypeGroup:
			{
				object pythonType;
				if (member.Count == 1)
				{
					pythonType = DynamicHelpers.GetPythonTypeFromType(((TypeTracker)member[0]).Type);
				}
				else
				{
					TypeTracker typeTracker = (TypeTracker)member[0];
					for (int i = 1; i < member.Count; i++)
					{
						typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)member[i]);
					}
					pythonType = typeTracker;
				}
				return new FastTypeGet<TSelfType>(type, pythonType).GetTypeObject;
			}
			case TrackerTypes.Method:
			{
				PythonTypeSlot slot = PythonTypeOps.GetSlot(member, name, _context.DomainManager.Configuration.PrivateBinding);
				if (slot is BuiltinMethodDescriptor)
				{
					return new FastMethodGet<TSelfType>(type, (BuiltinMethodDescriptor)slot).GetMethod;
				}
				if (slot is BuiltinFunction)
				{
					return new FastSlotGet<TSelfType>(type, slot, DynamicHelpers.GetPythonTypeFromType(type)).GetRetSlot;
				}
				return new FastSlotGet<TSelfType>(type, slot, DynamicHelpers.GetPythonTypeFromType(type)).GetBindSlot;
			}
			case TrackerTypes.Event:
				if (member.Count == 1 && !((EventTracker)member[0]).IsStatic)
				{
					PythonTypeSlot slot = PythonTypeOps.GetSlot(member, name, _context.DomainManager.Configuration.PrivateBinding);
					return new FastSlotGet<TSelfType>(type, slot, DynamicHelpers.GetPythonTypeFromType(((EventTracker)member[0]).DeclaringType)).GetBindSlot;
				}
				return null;
			case TrackerTypes.Property:
				if (member.Count == 1)
				{
					PropertyTracker propertyTracker = (PropertyTracker)member[0];
					if (!propertyTracker.IsStatic && propertyTracker.GetIndexParameters().Length == 0)
					{
						MethodInfo getMethod = propertyTracker.GetGetMethod();
						ParameterInfo[] parameters;
						if (getMethod != null && (parameters = getMethod.GetParameters()).Length == 0)
						{
							if (getMethod.ReturnType == typeof(bool))
							{
								return new FastPropertyGet<TSelfType>(type, CallInstruction.Create(getMethod, parameters).Invoke).GetPropertyBool;
							}
							if (getMethod.ReturnType == typeof(int))
							{
								return new FastPropertyGet<TSelfType>(type, CallInstruction.Create(getMethod, parameters).Invoke).GetPropertyInt;
							}
							return new FastPropertyGet<TSelfType>(type, CallInstruction.Create(getMethod, parameters).Invoke).GetProperty;
						}
					}
				}
				return null;
			case TrackerTypes.All:
				method = Context.Binder.GetMethod(type, "GetBoundMember");
				if (method != null && method.IsSpecialName)
				{
					return null;
				}
				if (member.Count == 0)
				{
					member = context.ModuleContext.ExtensionMethods.GetBinder(_context).GetMember(MemberRequestKind.Get, type, name);
					if (member.Count == 0)
					{
						if (IsNoThrow)
						{
							return new FastErrorGet<TSelfType>(type, name, context.ModuleContext.ExtensionMethods).GetErrorNoThrow;
						}
						if (SupportsLightThrow)
						{
							return new FastErrorGet<TSelfType>(type, name, context.ModuleContext.ExtensionMethods).GetErrorLightThrow;
						}
						return new FastErrorGet<TSelfType>(type, name, context.ModuleContext.ExtensionMethods).GetError;
					}
				}
				return null;
			default:
				return null;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (MemberTracker item in member)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(item.MemberType);
			stringBuilder.Append(" : ");
			stringBuilder.Append(item.ToString());
		}
		return new FastErrorGet<TSelfType>(type, stringBuilder.ToString(), context.ModuleContext.ExtensionMethods).GetAmbiguous;
	}

	private DynamicMetaObject GetForeignObject(DynamicMetaObject self)
	{
		return new DynamicMetaObject(Expression.Dynamic(_context.CompatGetMember(Name, IsNoThrow), typeof(object), self.Expression), self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, self.GetLimitType())));
	}

	public DynamicMetaObject Fallback(DynamicMetaObject self, DynamicMetaObject codeContext)
	{
		return FallbackWorker(_context, self, codeContext, Name, _options, this, null);
	}

	public DynamicMetaObject Fallback(DynamicMetaObject self, DynamicMetaObject codeContext, DynamicMetaObject errorSuggestion)
	{
		return FallbackWorker(_context, self, codeContext, Name, _options, this, errorSuggestion);
	}

	internal static DynamicMetaObject FallbackWorker(PythonContext context, DynamicMetaObject self, DynamicMetaObject codeContext, string name, GetMemberOptions options, DynamicMetaObjectBinder action, DynamicMetaObject errorSuggestion)
	{
		if (self.NeedsDeferral())
		{
			return action.Defer(self);
		}
		PythonOverloadResolverFactory resolverFactory = new PythonOverloadResolverFactory(context.Binder, codeContext.Expression);
		bool isNoThrow = (((options & GetMemberOptions.IsNoThrow) != GetMemberOptions.None) ? true : false);
		Type limitType = self.GetLimitType();
		if (limitType == typeof(DynamicNull) || PythonBinder.IsPythonType(limitType))
		{
			PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(limitType);
			if (pythonTypeFromType.IsHiddenMember(name))
			{
				DynamicMetaObject member = PythonContext.GetPythonContext(action).Binder.GetMember(name, self, resolverFactory, isNoThrow, errorSuggestion);
				Expression failureExpression = GetFailureExpression(limitType, self, name, isNoThrow, action);
				return BindingHelpers.FilterShowCls(codeContext, action, member, failureExpression);
			}
		}
		DynamicMetaObject dynamicMetaObject = context.Binder.GetMember(name, self, resolverFactory, isNoThrow, errorSuggestion);
		if (dynamicMetaObject is ErrorMetaObject)
		{
			CodeContext codeContext2 = (CodeContext)codeContext.Value;
			ExtensionMethodSet extensionMethods = codeContext2.ModuleContext.ExtensionMethods;
			if (extensionMethods != null)
			{
				dynamicMetaObject = extensionMethods.GetBinder(context).GetMember(name, self, resolverFactory, isNoThrow, errorSuggestion);
			}
			dynamicMetaObject = new DynamicMetaObject(dynamicMetaObject.Expression, dynamicMetaObject.Restrictions.Merge(extensionMethods.GetRestriction(codeContext.Expression)));
		}
		if (dynamicMetaObject.Expression.Type.IsValueType())
		{
			dynamicMetaObject = new DynamicMetaObject(Utils.Convert(dynamicMetaObject.Expression, typeof(object)), dynamicMetaObject.Restrictions);
		}
		return dynamicMetaObject;
	}

	private static Expression GetFailureExpression(Type limitType, DynamicMetaObject self, string name, bool isNoThrow, DynamicMetaObjectBinder action)
	{
		if (!isNoThrow)
		{
			return DefaultBinder.MakeError(PythonContext.GetPythonContext(action).Binder.MakeMissingMemberError(limitType, self, name), typeof(object)).Expression;
		}
		return Expression.Field(null, typeof(OperationFailed).GetField("Value"));
	}

	public override int GetHashCode()
	{
		return _name.GetHashCode() ^ _context.Binder.GetHashCode() ^ (int)_options;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonGetMemberBinder pythonGetMemberBinder))
		{
			return false;
		}
		if (pythonGetMemberBinder._context.Binder == _context.Binder && pythonGetMemberBinder._options == _options)
		{
			return pythonGetMemberBinder._name == _name;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Python GetMember {Name} IsNoThrow: {_options} LightThrow: {SupportsLightThrow}";
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeGetAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant(Name), Utils.Constant(IsNoThrow));
	}

	public virtual CallSiteBinder GetLightExceptionBinder()
	{
		if (_lightThrowBinder == null)
		{
			_lightThrowBinder = new LightThrowBinder(_context, Name, IsNoThrow);
		}
		return _lightThrowBinder;
	}
}
