using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding;

internal class BinaryRetTypeBinder : ComboBinder, IExpressionSerializable
{
	private readonly DynamicMetaObjectBinder _opBinder;

	private readonly PythonConversionBinder _convBinder;

	public override Type ReturnType => _convBinder.Type;

	public BinaryRetTypeBinder(DynamicMetaObjectBinder operationBinder, PythonConversionBinder conversionBinder)
		: base(new BinderMappingInfo(operationBinder, ParameterMappingInfo.Parameter(0), ParameterMappingInfo.Parameter(1)), new BinderMappingInfo(conversionBinder, ParameterMappingInfo.Action(0)))
	{
		_opBinder = operationBinder;
		_convBinder = conversionBinder;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeComboAction"), BindingHelpers.CreateBinderStateExpression(), ((IExpressionSerializable)_opBinder).CreateExpression(), _convBinder.CreateExpression());
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		if (_convBinder.Type == typeof(bool) && _opBinder is PythonBinaryOperationBinder)
		{
			T val = null;
			BinaryOperationBinder opBinder = (BinaryOperationBinder)_opBinder;
			if (CompilerHelpers.GetType(args[0]) == typeof(int) && CompilerHelpers.GetType(args[1]) == typeof(int))
			{
				if (typeof(T) == typeof(Func<CallSite, int, int, bool>))
				{
					val = (T)(object)GetIntIntIntDelegate(opBinder);
				}
				else if (typeof(T) == typeof(Func<CallSite, int, object, bool>))
				{
					val = (T)(object)GetIntIntObjectDelegate(opBinder);
				}
				else if (typeof(T) == typeof(Func<CallSite, object, int, bool>))
				{
					val = (T)(object)GetIntObjectIntDelegate(opBinder);
				}
				else if (typeof(T) == typeof(Func<CallSite, object, object, bool>))
				{
					val = (T)(object)GetIntObjectObjectDelegate(opBinder);
				}
			}
			if (val != null)
			{
				CacheTarget(val);
				return val;
			}
		}
		return base.BindDelegate(site, args);
	}

	private Func<CallSite, object, object, bool> GetIntObjectObjectDelegate(BinaryOperationBinder opBinder)
	{
		return opBinder.Operation switch
		{
			ExpressionType.Equal => IntEqualRetBool, 
			ExpressionType.NotEqual => IntNotEqualRetBool, 
			ExpressionType.GreaterThan => IntGreaterThanRetBool, 
			ExpressionType.LessThan => IntLessThanRetBool, 
			ExpressionType.GreaterThanOrEqual => IntGreaterThanOrEqualRetBool, 
			ExpressionType.LessThanOrEqual => IntLessThanOrEqualRetBool, 
			_ => null, 
		};
	}

	public bool IntEqualRetBool(CallSite site, object self, object other)
	{
		if (self != null && self.GetType() == typeof(int) && other != null && other.GetType() == typeof(int))
		{
			return (int)self == (int)other;
		}
		return ((CallSite<Func<CallSite, object, object, bool>>)site).Update(site, self, other);
	}

	public bool IntNotEqualRetBool(CallSite site, object self, object other)
	{
		if (self != null && self.GetType() == typeof(int) && other != null && other.GetType() == typeof(int))
		{
			return (int)self != (int)other;
		}
		return ((CallSite<Func<CallSite, object, object, bool>>)site).Update(site, self, other);
	}

	public bool IntGreaterThanRetBool(CallSite site, object self, object other)
	{
		if (self != null && self.GetType() == typeof(int) && other != null && other.GetType() == typeof(int))
		{
			return (int)self > (int)other;
		}
		return ((CallSite<Func<CallSite, object, object, bool>>)site).Update(site, self, other);
	}

	public bool IntLessThanRetBool(CallSite site, object self, object other)
	{
		if (self != null && self.GetType() == typeof(int) && other != null && other.GetType() == typeof(int))
		{
			return (int)self < (int)other;
		}
		return ((CallSite<Func<CallSite, object, object, bool>>)site).Update(site, self, other);
	}

	public bool IntGreaterThanOrEqualRetBool(CallSite site, object self, object other)
	{
		if (self != null && self.GetType() == typeof(int) && other != null && other.GetType() == typeof(int))
		{
			return (int)self >= (int)other;
		}
		return ((CallSite<Func<CallSite, object, object, bool>>)site).Update(site, self, other);
	}

	public bool IntLessThanOrEqualRetBool(CallSite site, object self, object other)
	{
		if (self != null && self.GetType() == typeof(int) && other != null && other.GetType() == typeof(int))
		{
			return (int)self <= (int)other;
		}
		return ((CallSite<Func<CallSite, object, object, bool>>)site).Update(site, self, other);
	}

	private Func<CallSite, object, int, bool> GetIntObjectIntDelegate(BinaryOperationBinder opBinder)
	{
		return opBinder.Operation switch
		{
			ExpressionType.Equal => IntEqualRetBool, 
			ExpressionType.NotEqual => IntNotEqualRetBool, 
			ExpressionType.GreaterThan => IntGreaterThanRetBool, 
			ExpressionType.LessThan => IntLessThanRetBool, 
			ExpressionType.GreaterThanOrEqual => IntGreaterThanOrEqualRetBool, 
			ExpressionType.LessThanOrEqual => IntLessThanOrEqualRetBool, 
			_ => null, 
		};
	}

	public bool IntEqualRetBool(CallSite site, object self, int other)
	{
		if (self != null && self.GetType() == typeof(int))
		{
			return (int)self == other;
		}
		return ((CallSite<Func<CallSite, object, int, bool>>)site).Update(site, self, other);
	}

	public bool IntNotEqualRetBool(CallSite site, object self, int other)
	{
		if (self != null && self.GetType() == typeof(int))
		{
			return (int)self != other;
		}
		return ((CallSite<Func<CallSite, object, int, bool>>)site).Update(site, self, other);
	}

	public bool IntGreaterThanRetBool(CallSite site, object self, int other)
	{
		if (self != null && self.GetType() == typeof(int))
		{
			return (int)self > other;
		}
		return ((CallSite<Func<CallSite, object, int, bool>>)site).Update(site, self, other);
	}

	public bool IntLessThanRetBool(CallSite site, object self, int other)
	{
		if (self != null && self.GetType() == typeof(int))
		{
			return (int)self < other;
		}
		return ((CallSite<Func<CallSite, object, int, bool>>)site).Update(site, self, other);
	}

	public bool IntGreaterThanOrEqualRetBool(CallSite site, object self, int other)
	{
		if (self != null && self.GetType() == typeof(int))
		{
			return (int)self >= other;
		}
		return ((CallSite<Func<CallSite, object, int, bool>>)site).Update(site, self, other);
	}

	public bool IntLessThanOrEqualRetBool(CallSite site, object self, int other)
	{
		if (self != null && self.GetType() == typeof(int))
		{
			return (int)self <= other;
		}
		return ((CallSite<Func<CallSite, object, int, bool>>)site).Update(site, self, other);
	}

	private Func<CallSite, int, object, bool> GetIntIntObjectDelegate(BinaryOperationBinder opBinder)
	{
		return opBinder.Operation switch
		{
			ExpressionType.Equal => IntEqualRetBool, 
			ExpressionType.NotEqual => IntNotEqualRetBool, 
			ExpressionType.GreaterThan => IntGreaterThanRetBool, 
			ExpressionType.LessThan => IntLessThanRetBool, 
			ExpressionType.GreaterThanOrEqual => IntGreaterThanOrEqualRetBool, 
			ExpressionType.LessThanOrEqual => IntLessThanOrEqualRetBool, 
			_ => null, 
		};
	}

	public bool IntEqualRetBool(CallSite site, int self, object other)
	{
		if (other != null && other.GetType() == typeof(int))
		{
			return self == (int)other;
		}
		return ((CallSite<Func<CallSite, int, object, bool>>)site).Update(site, self, other);
	}

	public bool IntNotEqualRetBool(CallSite site, int self, object other)
	{
		if (other != null && other.GetType() == typeof(int))
		{
			return self != (int)other;
		}
		return ((CallSite<Func<CallSite, int, object, bool>>)site).Update(site, self, other);
	}

	public bool IntGreaterThanRetBool(CallSite site, int self, object other)
	{
		if (other != null && other.GetType() == typeof(int))
		{
			return self > (int)other;
		}
		return ((CallSite<Func<CallSite, int, object, bool>>)site).Update(site, self, other);
	}

	public bool IntLessThanRetBool(CallSite site, int self, object other)
	{
		if (other != null && other.GetType() == typeof(int))
		{
			return self < (int)other;
		}
		return ((CallSite<Func<CallSite, int, object, bool>>)site).Update(site, self, other);
	}

	public bool IntGreaterThanOrEqualRetBool(CallSite site, int self, object other)
	{
		if (other != null && other.GetType() == typeof(int))
		{
			return self >= (int)other;
		}
		return ((CallSite<Func<CallSite, int, object, bool>>)site).Update(site, self, other);
	}

	public bool IntLessThanOrEqualRetBool(CallSite site, int self, object other)
	{
		if (other != null && other.GetType() == typeof(int))
		{
			return self <= (int)other;
		}
		return ((CallSite<Func<CallSite, int, object, bool>>)site).Update(site, self, other);
	}

	private Func<CallSite, int, int, bool> GetIntIntIntDelegate(BinaryOperationBinder opBinder)
	{
		return opBinder.Operation switch
		{
			ExpressionType.Equal => IntEqualRetBool, 
			ExpressionType.NotEqual => IntNotEqualRetBool, 
			ExpressionType.GreaterThan => IntGreaterThanRetBool, 
			ExpressionType.LessThan => IntLessThanRetBool, 
			ExpressionType.GreaterThanOrEqual => IntGreaterThanOrEqualRetBool, 
			ExpressionType.LessThanOrEqual => IntLessThanOrEqualRetBool, 
			_ => null, 
		};
	}

	public bool IntEqualRetBool(CallSite site, int self, int other)
	{
		return self == other;
	}

	public bool IntNotEqualRetBool(CallSite site, int self, int other)
	{
		return self != other;
	}

	public bool IntGreaterThanRetBool(CallSite site, int self, int other)
	{
		return self > other;
	}

	public bool IntLessThanRetBool(CallSite site, int self, int other)
	{
		return self < other;
	}

	public bool IntGreaterThanOrEqualRetBool(CallSite site, int self, int other)
	{
		return self >= other;
	}

	public bool IntLessThanOrEqualRetBool(CallSite site, int self, int other)
	{
		return self <= other;
	}
}
