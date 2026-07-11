using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Types;

[PythonType]
public class PythonTypeSlot
{
	internal virtual bool IsAlwaysVisible => true;

	internal virtual bool CanOptimizeGets => false;

	internal virtual bool GetAlwaysSucceeds => false;

	internal virtual bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = null;
		return false;
	}

	internal virtual bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		return false;
	}

	internal virtual bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		return false;
	}

	internal virtual void MakeGetExpression(PythonBinder binder, Expression codeContext, DynamicMetaObject instance, DynamicMetaObject owner, ConditionalBuilder builder)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "slotTmp");
		Expression expression = Expression.Call(typeof(PythonOps).GetMethod("SlotTryGetValue"), codeContext, Utils.Convert(Utils.WeakConstant(this), typeof(PythonTypeSlot)), (instance != null) ? instance.Expression : Utils.Constant(null), owner.Expression, parameterExpression);
		builder.AddVariable(parameterExpression);
		if (!GetAlwaysSucceeds)
		{
			builder.AddCondition(expression, parameterExpression);
		}
		else
		{
			builder.FinishCondition(Expression.Block(expression, parameterExpression));
		}
	}

	internal virtual bool IsSetDescriptor(CodeContext context, PythonType owner)
	{
		return false;
	}

	public virtual object __get__(CodeContext context, object instance, [DefaultParameterValue(null)] object typeContext)
	{
		PythonType pythonType = typeContext as PythonType;
		if (TryGetValue(context, instance, pythonType, out var value))
		{
			return value;
		}
		throw PythonOps.AttributeErrorForMissingAttribute((pythonType == null) ? "?" : pythonType.Name, "__get__");
	}
}
