using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class NameExpression : Expression
{
	private readonly string _name;

	private PythonReference _reference;

	private bool _assigned;

	public string Name => _name;

	internal PythonReference Reference
	{
		get
		{
			return _reference;
		}
		set
		{
			_reference = value;
		}
	}

	internal bool Assigned
	{
		get
		{
			return _assigned;
		}
		set
		{
			_assigned = value;
		}
	}

	internal override bool CanThrow => !Assigned;

	public NameExpression(string name)
	{
		_name = name;
	}

	public override string ToString()
	{
		return base.ToString() + ":" + _name;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		System.Linq.Expressions.Expression expression = ((_reference.PythonVariable != null) ? base.Parent.GetVariableExpression(_reference.PythonVariable) : System.Linq.Expressions.Expression.Call(AstMethods.LookupName, base.Parent.LocalContext, System.Linq.Expressions.Expression.Constant(_name)));
		if (!_assigned && !(expression is IPythonGlobalExpression))
		{
			expression = System.Linq.Expressions.Expression.Call(AstMethods.CheckUninitialized, expression, System.Linq.Expressions.Expression.Constant(_name));
		}
		return expression;
	}

	internal override System.Linq.Expressions.Expression TransformSet(SourceSpan span, System.Linq.Expressions.Expression right, PythonOperationKind op)
	{
		if (op != PythonOperationKind.None)
		{
			right = base.GlobalParent.Operation(typeof(object), op, this, right);
		}
		SourceSpan location = (span.IsValid ? new SourceSpan(base.Span.Start, span.End) : SourceSpan.None);
		System.Linq.Expressions.Expression expression = ((_reference.PythonVariable == null) ? System.Linq.Expressions.Expression.Call(null, AstMethods.SetName, base.Parent.LocalContext, System.Linq.Expressions.Expression.Constant(_name), Utils.Convert(right, typeof(object))) : Node.AssignValue(base.Parent.GetVariableExpression(_reference.PythonVariable), Node.ConvertIfNeeded(right, typeof(object))));
		return base.GlobalParent.AddDebugInfoAndVoid(expression, location);
	}

	internal override string CheckAssign()
	{
		return null;
	}

	internal override string CheckDelete()
	{
		return null;
	}

	internal override System.Linq.Expressions.Expression TransformDelete()
	{
		if (_reference.PythonVariable != null)
		{
			System.Linq.Expressions.Expression variableExpression = base.Parent.GetVariableExpression(_reference.PythonVariable);
			System.Linq.Expressions.Expression expression = System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Call(AstMethods.KeepAlive, variableExpression), Node.Delete(variableExpression));
			if (!_assigned)
			{
				expression = System.Linq.Expressions.Expression.Block(this, expression, Utils.Empty());
			}
			return expression;
		}
		return System.Linq.Expressions.Expression.Call(AstMethods.RemoveName, base.Parent.LocalContext, System.Linq.Expressions.Expression.Constant(_name));
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}
