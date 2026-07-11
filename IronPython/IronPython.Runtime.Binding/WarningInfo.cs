using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class WarningInfo
{
	private readonly string _message;

	private readonly PythonType _type;

	private readonly Expression _condition;

	public WarningInfo(PythonType type, string message)
	{
		_message = message;
		_type = type;
	}

	public WarningInfo(PythonType type, string message, Expression condition)
	{
		_message = message;
		_type = type;
		_condition = condition;
	}

	public DynamicMetaObject AddWarning(Expression codeContext, DynamicMetaObject result)
	{
		Expression expression = Expression.Call(typeof(PythonOps).GetMethod("Warn"), codeContext, Utils.Constant(_type), Utils.Constant(_message), Utils.Constant(ArrayUtils.EmptyObjects));
		if (_condition != null)
		{
			expression = Expression.Condition(_condition, expression, Utils.Empty());
		}
		return new DynamicMetaObject(Expression.Block(expression, result.Expression), result.Restrictions);
	}
}
