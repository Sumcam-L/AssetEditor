using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonConversionBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable
{
	private class IdentityConversion
	{
		private readonly Type _type;

		public IdentityConversion(Type type)
		{
			_type = type;
		}

		public object Convert(CallSite site, object value)
		{
			if (value != null && value.GetType() == _type)
			{
				return value;
			}
			return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
		}
	}

	private class IdentityConversion<T>
	{
		private readonly Type _type;

		public IdentityConversion(Type type)
		{
			_type = type;
		}

		public T Convert(CallSite site, object value)
		{
			if (value != null && value.GetType() == _type)
			{
				return (T)value;
			}
			return ((CallSite<Func<CallSite, object, T>>)site).Update(site, value);
		}
	}

	private readonly PythonContext _context;

	private readonly ConversionResultKind _kind;

	private readonly Type _type;

	private readonly bool _retObject;

	private CompatConversionBinder _compatConvert;

	public Type Type => _type;

	public ConversionResultKind ResultKind => _kind;

	public override Type ReturnType
	{
		get
		{
			if (_retObject)
			{
				return typeof(object);
			}
			if (_kind != ConversionResultKind.ExplicitCast && _kind != ConversionResultKind.ImplicitCast)
			{
				if (!_type.IsValueType())
				{
					return _type;
				}
				return typeof(object);
			}
			return Type;
		}
	}

	internal CompatConversionBinder CompatBinder
	{
		get
		{
			if (_compatConvert == null)
			{
				_compatConvert = new CompatConversionBinder(this, Type, _kind == ConversionResultKind.ExplicitCast || _kind == ConversionResultKind.ExplicitTry);
			}
			return _compatConvert;
		}
	}

	public PythonContext Context => _context;

	public PythonConversionBinder(PythonContext context, Type type, ConversionResultKind resultKind)
	{
		_context = context;
		_kind = resultKind;
		_type = type;
	}

	public PythonConversionBinder(PythonContext context, Type type, ConversionResultKind resultKind, bool retObject)
	{
		_context = context;
		_kind = resultKind;
		_type = type;
		_retObject = retObject;
	}

	public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		DynamicMetaObject dynamicMetaObject = null;
		if (target.NeedsDeferral())
		{
			return MyDefer(target);
		}
		if (target is IPythonConvertible pythonConvertible)
		{
			dynamicMetaObject = pythonConvertible.BindConvert(this);
		}
		else if (dynamicMetaObject == null)
		{
			dynamicMetaObject = BindConvert(target);
		}
		if (_retObject)
		{
			dynamicMetaObject = new DynamicMetaObject(Utils.Convert(dynamicMetaObject.Expression, typeof(object)), dynamicMetaObject.Restrictions);
		}
		return dynamicMetaObject;
	}

	private DynamicMetaObject MyDefer(DynamicMetaObject self)
	{
		return new DynamicMetaObject(Expression.Dynamic(this, ReturnType, self.Expression), self.Restrictions);
	}

	private DynamicMetaObject BindConvert(DynamicMetaObject self)
	{
		DynamicMetaObject result;
		DynamicMetaObject dynamicMetaObject = ((!ComBinder.TryConvert(CompatBinder, self, out result)) ? self.BindConvert(CompatBinder) : result);
		if (ReturnType == typeof(object) && dynamicMetaObject.Expression.Type != typeof(object) && dynamicMetaObject.Expression.NodeType == ExpressionType.Convert)
		{
			dynamicMetaObject = new DynamicMetaObject(((UnaryExpression)dynamicMetaObject.Expression).Operand, dynamicMetaObject.Restrictions);
		}
		return dynamicMetaObject;
	}

	internal DynamicMetaObject FallbackConvert(Type returnType, DynamicMetaObject self, DynamicMetaObject errorSuggestion)
	{
		Type type = Type;
		DynamicMetaObject dynamicMetaObject = null;
		switch (type.GetTypeCode())
		{
		case TypeCode.Boolean:
			dynamicMetaObject = MakeToBoolConversion(self);
			break;
		case TypeCode.Char:
			dynamicMetaObject = TryToCharConversion(self);
			break;
		case TypeCode.String:
			if (self.GetLimitType() == typeof(Bytes) && !_context.PythonOptions.Python30)
			{
				dynamicMetaObject = new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MakeString"), Utils.Convert(self.Expression, typeof(IList<byte>))), BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, typeof(Bytes)));
			}
			break;
		case TypeCode.Object:
			if (type.IsArray && self.Value is PythonTuple && type.GetArrayRank() == 1)
			{
				dynamicMetaObject = MakeToArrayConversion(self, type);
			}
			else if (type.IsGenericType && !type.IsAssignableFrom(CompilerHelpers.GetType(self.Value)))
			{
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				if (genericTypeDefinition == typeof(IList<>))
				{
					dynamicMetaObject = ((!(self.LimitType == typeof(string))) ? TryToGenericInterfaceConversion(self, type, typeof(IList<object>), typeof(ListGenericWrapper<>)) : new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MakeByteArray"), Utils.Convert(self.Expression, typeof(string))), BindingRestrictions.GetTypeRestriction(self.Expression, typeof(string))));
				}
				else if (genericTypeDefinition == typeof(IDictionary<, >))
				{
					dynamicMetaObject = TryToGenericInterfaceConversion(self, type, typeof(IDictionary<object, object>), typeof(DictionaryGenericWrapper<, >));
				}
				else if (genericTypeDefinition == typeof(IEnumerable<>))
				{
					dynamicMetaObject = TryToGenericInterfaceConversion(self, type, typeof(IEnumerable), typeof(IEnumerableOfTWrapper<>));
				}
			}
			else if (type == typeof(IEnumerable))
			{
				if (!typeof(IEnumerable).IsAssignableFrom(self.GetLimitType()) && IsIndexless(self))
				{
					dynamicMetaObject = ConvertToIEnumerable(this, self.Restrict(self.GetLimitType()));
				}
			}
			else if (type == typeof(IEnumerator) && !typeof(IEnumerator).IsAssignableFrom(self.GetLimitType()) && !typeof(IEnumerable).IsAssignableFrom(self.GetLimitType()) && IsIndexless(self))
			{
				dynamicMetaObject = ConvertToIEnumerator(this, self.Restrict(self.GetLimitType()));
			}
			break;
		}
		if (type.IsEnum() && Enum.GetUnderlyingType(type) == self.GetLimitType())
		{
			object value = Activator.CreateInstance(type);
			return new DynamicMetaObject(Expression.Condition(Expression.Equal(Utils.Convert(self.Expression, Enum.GetUnderlyingType(type)), Utils.Constant(Activator.CreateInstance(self.GetLimitType()))), Utils.Constant(value), Expression.Call(typeof(PythonOps).GetMethod("TypeErrorForBadEnumConversion").MakeGenericMethod(type), Utils.Convert(self.Expression, typeof(object)))), self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, self.GetLimitType())), value);
		}
		return dynamicMetaObject ?? EnsureReturnType(returnType, Context.Binder.ConvertTo(Type, ResultKind, self, _context.SharedOverloadResolverFactory, errorSuggestion));
	}

	private static DynamicMetaObject EnsureReturnType(Type returnType, DynamicMetaObject dynamicMetaObject)
	{
		if (dynamicMetaObject.Expression.Type != returnType)
		{
			dynamicMetaObject = new DynamicMetaObject(Utils.Convert(dynamicMetaObject.Expression, returnType), dynamicMetaObject.Restrictions);
		}
		return dynamicMetaObject;
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		object obj = args[0];
		T val = null;
		if (typeof(T) == typeof(Func<CallSite, object, string>) && obj is string)
		{
			val = (T)(object)new Func<CallSite, object, string>(StringConversion);
		}
		else if (typeof(T) == typeof(Func<CallSite, object, int>))
		{
			if (obj is int)
			{
				val = (T)(object)new Func<CallSite, object, int>(IntConversion);
			}
			else if (obj is bool)
			{
				val = (T)(object)new Func<CallSite, object, int>(BoolToIntConversion);
			}
		}
		else if (typeof(T) == typeof(Func<CallSite, bool, int>))
		{
			val = (T)(object)new Func<CallSite, bool, int>(BoolToIntConversion);
		}
		else if (typeof(T) == typeof(Func<CallSite, object, bool>))
		{
			if (obj is bool)
			{
				val = (T)(object)new Func<CallSite, object, bool>(BoolConversion);
			}
			else if (obj is string)
			{
				val = (T)(object)new Func<CallSite, object, bool>(StringToBoolConversion);
			}
			else if (obj is int)
			{
				val = (T)(object)new Func<CallSite, object, bool>(IntToBoolConversion);
			}
			else if (obj == null)
			{
				val = (T)(object)new Func<CallSite, object, bool>(NullToBoolConversion);
			}
			else if (obj.GetType() == typeof(object))
			{
				val = (T)(object)new Func<CallSite, object, bool>(ObjectToBoolConversion);
			}
			else if (obj.GetType() == typeof(List))
			{
				val = (T)(object)new Func<CallSite, object, bool>(ListToBoolConversion);
			}
			else if (obj.GetType() == typeof(PythonTuple))
			{
				val = (T)(object)new Func<CallSite, object, bool>(TupleToBoolConversion);
			}
		}
		else if (obj != null)
		{
			if (obj is BigInteger)
			{
				if (typeof(T) == typeof(Func<CallSite, BigInteger, Complex>))
				{
					val = (T)(object)new Func<CallSite, BigInteger, Complex>(BigIntegerToComplexConversion);
				}
				else if (typeof(T) == typeof(Func<CallSite, object, Complex>))
				{
					val = (T)(object)new Func<CallSite, object, Complex>(BigIntegerObjectToComplexConversion);
				}
				else if (typeof(T) == typeof(Func<CallSite, BigInteger, object>))
				{
					val = (T)(object)new Func<CallSite, BigInteger, object>(BigIntegerToComplexObjectConversion);
				}
			}
			else if (obj is string)
			{
				if (typeof(T) == typeof(Func<CallSite, string, IEnumerable>))
				{
					val = (T)(object)new Func<CallSite, string, IEnumerable>(StringToIEnumerableConversion);
				}
				else if (typeof(T) == typeof(Func<CallSite, string, IEnumerator>))
				{
					val = (T)(object)new Func<CallSite, string, IEnumerator>(StringToIEnumeratorConversion);
				}
				else if (typeof(T) == typeof(Func<CallSite, object, IEnumerable>))
				{
					val = (T)(object)new Func<CallSite, object, IEnumerable>(ObjectToIEnumerableConversion);
				}
				else if (typeof(T) == typeof(Func<CallSite, object, IEnumerator>))
				{
					val = (T)(object)new Func<CallSite, object, IEnumerator>(ObjectToIEnumeratorConversion);
				}
			}
			else if (obj.GetType() == typeof(Bytes))
			{
				if (typeof(T) == typeof(Func<CallSite, Bytes, IEnumerable>))
				{
					val = (T)(object)new Func<CallSite, Bytes, IEnumerable>(BytesToIEnumerableConversion);
				}
				else if (typeof(T) == typeof(Func<CallSite, Bytes, IEnumerator>))
				{
					val = (T)(object)new Func<CallSite, Bytes, IEnumerator>(BytesToIEnumeratorConversion);
				}
				else if (typeof(T) == typeof(Func<CallSite, object, IEnumerable>))
				{
					val = (T)(object)new Func<CallSite, object, IEnumerable>(ObjectToIEnumerableConversion);
				}
				else if (typeof(T) == typeof(Func<CallSite, object, IEnumerator>))
				{
					val = (T)(object)new Func<CallSite, object, IEnumerator>(ObjectToIEnumeratorConversion);
				}
			}
			if (val == null && (obj.GetType() == Type || Type.IsAssignableFrom(obj.GetType())))
			{
				if (typeof(T) == typeof(Func<CallSite, object, object>))
				{
					val = (T)(object)new Func<CallSite, object, object>(new IdentityConversion(obj.GetType()).Convert);
				}
				else if (typeof(T).GetMethod("Invoke").GetParameters()[1].ParameterType == typeof(object))
				{
					object obj2 = Activator.CreateInstance(typeof(IdentityConversion<>).MakeGenericType(Type), obj.GetType());
					val = (T)(object)ReflectionUtils.CreateDelegate(obj2.GetType().GetMethod("Convert"), typeof(T), obj2);
				}
			}
		}
		if (val != null)
		{
			CacheTarget(val);
			return val;
		}
		return base.BindDelegate(site, args);
	}

	public string StringConversion(CallSite site, object value)
	{
		if (value is string result)
		{
			return result;
		}
		return ((CallSite<Func<CallSite, object, string>>)site).Update(site, value);
	}

	public int IntConversion(CallSite site, object value)
	{
		if (value is int)
		{
			return (int)value;
		}
		return ((CallSite<Func<CallSite, object, int>>)site).Update(site, value);
	}

	public int BoolToIntConversion(CallSite site, object value)
	{
		if (value is bool)
		{
			if (!(bool)value)
			{
				return 0;
			}
			return 1;
		}
		return ((CallSite<Func<CallSite, object, int>>)site).Update(site, value);
	}

	public int BoolToIntConversion(CallSite site, bool value)
	{
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	public bool BoolConversion(CallSite site, object value)
	{
		if (value is bool)
		{
			return (bool)value;
		}
		if (value == null)
		{
			return false;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	public bool IntToBoolConversion(CallSite site, object value)
	{
		if (value is int)
		{
			return (int)value != 0;
		}
		if (value == null)
		{
			return false;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	public bool StringToBoolConversion(CallSite site, object value)
	{
		if (value is string)
		{
			return ((string)value).Length > 0;
		}
		if (value == null)
		{
			return false;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	public bool NullToBoolConversion(CallSite site, object value)
	{
		if (value == null)
		{
			return false;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	public bool ObjectToBoolConversion(CallSite site, object value)
	{
		if (value != null && value.GetType() == typeof(object))
		{
			return true;
		}
		if (value == null)
		{
			return false;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	public bool ListToBoolConversion(CallSite site, object value)
	{
		if (value == null)
		{
			return false;
		}
		if (value.GetType() == typeof(List))
		{
			return ((List)value).Count != 0;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	public bool TupleToBoolConversion(CallSite site, object value)
	{
		if (value == null)
		{
			return false;
		}
		if (value.GetType() == typeof(PythonTuple))
		{
			return ((PythonTuple)value).Count != 0;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	public IEnumerable StringToIEnumerableConversion(CallSite site, string value)
	{
		if (value == null)
		{
			return ((CallSite<Func<CallSite, string, IEnumerable>>)site).Update(site, value);
		}
		return PythonOps.StringEnumerable(value);
	}

	public IEnumerator StringToIEnumeratorConversion(CallSite site, string value)
	{
		if (value == null)
		{
			return ((CallSite<Func<CallSite, string, IEnumerator>>)site).Update(site, value);
		}
		return PythonOps.StringEnumerator(value).Key;
	}

	public IEnumerable BytesToIEnumerableConversion(CallSite site, Bytes value)
	{
		if (value == null)
		{
			return ((CallSite<Func<CallSite, Bytes, IEnumerable>>)site).Update(site, value);
		}
		if (!_context.PythonOptions.Python30)
		{
			return PythonOps.BytesEnumerable(value);
		}
		return PythonOps.BytesIntEnumerable(value);
	}

	public IEnumerator BytesToIEnumeratorConversion(CallSite site, Bytes value)
	{
		if (value == null)
		{
			return ((CallSite<Func<CallSite, Bytes, IEnumerator>>)site).Update(site, value);
		}
		if (!_context.PythonOptions.Python30)
		{
			return PythonOps.BytesEnumerator(value).Key;
		}
		return PythonOps.BytesIntEnumerator(value).Key;
	}

	public IEnumerable ObjectToIEnumerableConversion(CallSite site, object value)
	{
		if (value != null)
		{
			if (value is string)
			{
				return PythonOps.StringEnumerable((string)value);
			}
			if (value.GetType() == typeof(Bytes))
			{
				if (!_context.PythonOptions.Python30)
				{
					return PythonOps.BytesEnumerable((Bytes)value);
				}
				return PythonOps.BytesIntEnumerable((Bytes)value);
			}
		}
		return ((CallSite<Func<CallSite, object, IEnumerable>>)site).Update(site, value);
	}

	public IEnumerator ObjectToIEnumeratorConversion(CallSite site, object value)
	{
		if (value != null)
		{
			if (value is string)
			{
				return PythonOps.StringEnumerator((string)value).Key;
			}
			if (value.GetType() == typeof(Bytes))
			{
				if (!_context.PythonOptions.Python30)
				{
					return PythonOps.BytesEnumerator((Bytes)value).Key;
				}
				return PythonOps.BytesIntEnumerator((Bytes)value).Key;
			}
		}
		return ((CallSite<Func<CallSite, object, IEnumerator>>)site).Update(site, value);
	}

	public Complex BigIntegerToComplexConversion(CallSite site, BigInteger value)
	{
		return BigIntegerOps.ConvertToComplex(value);
	}

	public Complex BigIntegerObjectToComplexConversion(CallSite site, object value)
	{
		if (value is BigInteger)
		{
			return BigIntegerOps.ConvertToComplex((BigInteger)value);
		}
		return ((CallSite<Func<CallSite, object, Complex>>)site).Update(site, value);
	}

	public object BigIntegerToComplexObjectConversion(CallSite site, BigInteger value)
	{
		return BigIntegerOps.ConvertToComplex(value);
	}

	internal static bool IsIndexless(DynamicMetaObject arg)
	{
		return arg.GetLimitType() != typeof(OldInstance);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode() ^ _kind.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonConversionBinder pythonConversionBinder))
		{
			return false;
		}
		if (pythonConversionBinder._context.Binder == _context.Binder && _kind == pythonConversionBinder._kind && base.Equals(obj))
		{
			return _retObject == pythonConversionBinder._retObject;
		}
		return false;
	}

	private static DynamicMetaObject TryToGenericInterfaceConversion(DynamicMetaObject self, Type toType, Type fromType, Type wrapperType)
	{
		if (fromType.IsAssignableFrom(CompilerHelpers.GetType(self.Value)))
		{
			Type type = wrapperType.MakeGenericType(toType.GetGenericArguments());
			self = self.Restrict(CompilerHelpers.GetType(self.Value));
			return new DynamicMetaObject(Expression.New(type.GetConstructor(new Type[1] { fromType }), Utils.Convert(self.Expression, fromType)), self.Restrictions);
		}
		return null;
	}

	private static DynamicMetaObject MakeToArrayConversion(DynamicMetaObject self, Type toType)
	{
		self = self.Restrict(typeof(PythonTuple));
		return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("ConvertTupleToArray").MakeGenericMethod(toType.GetElementType()), self.Expression), self.Restrictions);
	}

	private DynamicMetaObject TryToCharConversion(DynamicMetaObject self)
	{
		string text = self.Value as string;
		Expression expression = self.Expression;
		if (text == null && self.Value is Extensible<string> extensible)
		{
			text = extensible.Value;
			expression = Expression.Property(Utils.Convert(expression, typeof(Extensible<string>)), typeof(Extensible<string>).GetProperty("Value"));
		}
		if (text != null)
		{
			self = self.Restrict(self.GetRuntimeType());
			Expression left = Expression.Property(Utils.Convert(expression, typeof(string)), typeof(string).GetProperty("Length"));
			if (text.Length == 1)
			{
				return new DynamicMetaObject(Expression.Call(Utils.Convert(expression, typeof(string)), typeof(string).GetMethod("get_Chars"), Utils.Constant(0)), self.Restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(left, Utils.Constant(1)))));
			}
			return new DynamicMetaObject(this.Throw(Expression.Call(typeof(PythonOps).GetMethod("TypeError"), Utils.Constant("expected string of length 1 when converting to char, got '{0}'"), Expression.NewArrayInit(typeof(object), self.Expression)), ReturnType), self.Restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Expression.NotEqual(left, Utils.Constant(1)))));
		}
		return null;
	}

	private DynamicMetaObject MakeToBoolConversion(DynamicMetaObject self)
	{
		if (self.HasValue)
		{
			self = self.Restrict(self.GetRuntimeType());
		}
		if (self.Expression.NodeType == ExpressionType.Convert && self.Expression.Type == typeof(object))
		{
			UnaryExpression unaryExpression = (UnaryExpression)self.Expression;
			if (unaryExpression.Operand.Type == typeof(bool))
			{
				return new DynamicMetaObject(unaryExpression.Operand, self.Restrictions);
			}
		}
		if (self.GetLimitType() == typeof(DynamicNull))
		{
			return MakeNoneToBoolConversion(self);
		}
		if (self.GetLimitType() == typeof(bool))
		{
			return self;
		}
		if (typeof(IStrongBox).IsAssignableFrom(self.GetLimitType()))
		{
			return MakeStrongBoxToBoolConversionError(self);
		}
		if (self.GetLimitType().IsPrimitive() || self.GetLimitType().IsEnum())
		{
			return MakePrimitiveToBoolComparison(self);
		}
		return PythonProtocol.ConvertToBool(this, self) ?? new DynamicMetaObject(Utils.Constant(true), self.Restrictions);
	}

	private static DynamicMetaObject MakeNoneToBoolConversion(DynamicMetaObject self)
	{
		return new DynamicMetaObject(Utils.Constant(false), self.Restrictions);
	}

	private static DynamicMetaObject MakePrimitiveToBoolComparison(DynamicMetaObject self)
	{
		object value = Activator.CreateInstance(self.GetLimitType());
		return new DynamicMetaObject(Expression.NotEqual(Utils.Constant(value), self.Expression), self.Restrictions);
	}

	private DynamicMetaObject MakeStrongBoxToBoolConversionError(DynamicMetaObject self)
	{
		return new DynamicMetaObject(this.Throw(Expression.Call(typeof(ScriptingRuntimeHelpers).GetMethod("SimpleTypeError"), Utils.Constant("Can't convert a Reference<> instance to a bool")), ReturnType), self.Restrictions);
	}

	internal static DynamicMetaObject ConvertToIEnumerable(DynamicMetaObjectBinder conversion, DynamicMetaObject metaUserObject)
	{
		PythonType pythonType = MetaPythonObject.GetPythonType(metaUserObject);
		PythonContext pythonContext = PythonContext.GetPythonContext(conversion);
		CodeContext sharedContext = pythonContext.SharedContext;
		if (pythonType.TryResolveSlot(sharedContext, "__iter__", out var slot))
		{
			return MakeIterRule(metaUserObject, "CreatePythonEnumerable");
		}
		if (pythonType.TryResolveSlot(sharedContext, "__getitem__", out slot))
		{
			return MakeGetItemIterable(metaUserObject, pythonContext, slot, "CreateItemEnumerable");
		}
		return null;
	}

	internal static DynamicMetaObject ConvertToIEnumerator(DynamicMetaObjectBinder conversion, DynamicMetaObject metaUserObject)
	{
		PythonType pythonType = MetaPythonObject.GetPythonType(metaUserObject);
		PythonContext pythonContext = PythonContext.GetPythonContext(conversion);
		CodeContext sharedContext = pythonContext.SharedContext;
		if (pythonType.TryResolveSlot(sharedContext, "__iter__", out var slot))
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "iterVal");
			return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Call(typeof(PythonOps).GetMethod("CreatePythonEnumerator"), Expression.Block(MetaPythonObject.MakeTryGetTypeMember(pythonContext, slot, metaUserObject.Expression, parameterExpression), Expression.Dynamic(new PythonInvokeBinder(pythonContext, new CallSignature(0)), typeof(object), Utils.Constant(sharedContext), parameterExpression)))), metaUserObject.Restrictions);
		}
		if (pythonType.TryResolveSlot(sharedContext, "__getitem__", out slot))
		{
			return MakeGetItemIterable(metaUserObject, pythonContext, slot, "CreateItemEnumerator");
		}
		return null;
	}

	private static DynamicMetaObject MakeGetItemIterable(DynamicMetaObject metaUserObject, PythonContext state, PythonTypeSlot pts, string method)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "getitemVal");
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Call(typeof(PythonOps).GetMethod(method), Expression.Block(MetaPythonObject.MakeTryGetTypeMember(state, pts, parameterExpression, metaUserObject.Expression, Expression.Call(typeof(DynamicHelpers).GetMethod("GetPythonType"), Utils.Convert(metaUserObject.Expression, typeof(object)))), parameterExpression), Utils.Constant(CallSite<Func<CallSite, CodeContext, object, int, object>>.Create(new PythonInvokeBinder(state, new CallSignature(1)))))), metaUserObject.Restrictions);
	}

	private static DynamicMetaObject MakeIterRule(DynamicMetaObject self, string methodName)
	{
		return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod(methodName), Utils.Convert(self.Expression, typeof(object))), self.Restrictions);
	}

	public override string ToString()
	{
		return $"Python Convert {Type} {ResultKind}";
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeConversionAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant(Type), Utils.Constant(ResultKind));
	}
}
