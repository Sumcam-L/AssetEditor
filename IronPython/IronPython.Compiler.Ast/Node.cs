using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public abstract class Node : System.Linq.Expressions.Expression
{
	private class FramedCodeVisitor : ExpressionVisitor
	{
		public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression node)
		{
			if (node is FramedCodeExpression framedCodeExpression)
			{
				return framedCodeExpression.Body;
			}
			return base.Visit(node);
		}
	}

	private sealed class FramedCodeExpression : System.Linq.Expressions.Expression
	{
		private readonly System.Linq.Expressions.Expression _localContext;

		private readonly System.Linq.Expressions.Expression _codeObject;

		private readonly System.Linq.Expressions.Expression _body;

		public override ExpressionType NodeType => ExpressionType.Extension;

		public System.Linq.Expressions.Expression Body => _body;

		public override Type Type => _body.Type;

		public override bool CanReduce => true;

		public FramedCodeExpression(System.Linq.Expressions.Expression localContext, System.Linq.Expressions.Expression codeObject, System.Linq.Expressions.Expression body)
		{
			_localContext = localContext;
			_codeObject = codeObject;
			_body = body;
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			return Utils.Try(System.Linq.Expressions.Expression.Assign(FunctionStackVariable, System.Linq.Expressions.Expression.Call(AstMethods.PushFrame, _localContext, _codeObject)), _body).Finally(System.Linq.Expressions.Expression.Call(FunctionStackVariable, typeof(List<FunctionStack>).GetMethod("RemoveAt"), System.Linq.Expressions.Expression.Add(System.Linq.Expressions.Expression.Property(FunctionStackVariable, "Count"), System.Linq.Expressions.Expression.Constant(-1))));
		}

		protected override System.Linq.Expressions.Expression VisitChildren(ExpressionVisitor visitor)
		{
			System.Linq.Expressions.Expression expression = visitor.Visit(_localContext);
			System.Linq.Expressions.Expression expression2 = visitor.Visit(_codeObject);
			System.Linq.Expressions.Expression expression3 = visitor.Visit(_body);
			if (expression != _localContext || _codeObject != expression2 || expression3 != _body)
			{
				return new FramedCodeExpression(expression, expression2, expression3);
			}
			return this;
		}
	}

	private ScopeStatement _parent;

	private IndexSpan _span;

	internal static readonly BlockExpression EmptyBlock = System.Linq.Expressions.Expression.Block(Utils.Empty());

	internal static readonly System.Linq.Expressions.Expression[] EmptyExpression = new System.Linq.Expressions.Expression[0];

	internal static ParameterExpression FunctionStackVariable = System.Linq.Expressions.Expression.Variable(typeof(List<FunctionStack>), "$funcStack");

	internal static readonly LabelTarget GeneratorLabel = System.Linq.Expressions.Expression.Label(typeof(object), "$generatorLabel");

	private static ParameterExpression _lineNumberUpdated = System.Linq.Expressions.Expression.Variable(typeof(bool), "$lineUpdated");

	private static readonly ParameterExpression _lineNoVar = System.Linq.Expressions.Expression.Parameter(typeof(int), "$lineNo");

	public ScopeStatement Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	public IndexSpan IndexSpan
	{
		get
		{
			return _span;
		}
		set
		{
			_span = value;
		}
	}

	public SourceLocation Start => GlobalParent.IndexToLocation(StartIndex);

	public SourceLocation End => GlobalParent.IndexToLocation(EndIndex);

	public int EndIndex
	{
		get
		{
			return _span.End;
		}
		set
		{
			_span = new IndexSpan(_span.Start, value - _span.Start);
		}
	}

	public int StartIndex
	{
		get
		{
			return _span.Start;
		}
		set
		{
			_span = new IndexSpan(value, 0);
		}
	}

	public SourceSpan Span => new SourceSpan(Start, End);

	public virtual string NodeName => GetType().Name;

	internal virtual bool CanThrow => true;

	public override bool CanReduce => true;

	public override ExpressionType NodeType => ExpressionType.Extension;

	internal PythonAst GlobalParent
	{
		get
		{
			Node node = this;
			while (!(node is PythonAst))
			{
				node = node.Parent;
			}
			return (PythonAst)node;
		}
	}

	internal bool EmitDebugSymbols => GlobalParent.EmitDebugSymbols;

	internal bool StripDocStrings => GlobalParent.PyContext.PythonOptions.StripDocStrings;

	internal bool Optimize => GlobalParent.PyContext.PythonOptions.Optimize;

	internal static ParameterExpression LineNumberUpdated => _lineNumberUpdated;

	internal static ParameterExpression LineNumberExpression => _lineNoVar;

	public void SetLoc(PythonAst globalParent, int start, int end)
	{
		_span = new IndexSpan(start, (end > start) ? (end - start) : start);
		_parent = globalParent;
	}

	public void SetLoc(PythonAst globalParent, IndexSpan span)
	{
		_span = span;
		_parent = globalParent;
	}

	internal SourceLocation IndexToLocation(int index)
	{
		if (index == -1)
		{
			return SourceLocation.Invalid;
		}
		int[] lineLocations = GlobalParent._lineLocations;
		int num = Array.BinarySearch(lineLocations, index);
		if (num < 0)
		{
			if (num == -1)
			{
				return new SourceLocation(index, 1, index + 1);
			}
			num = ~num - 1;
		}
		return new SourceLocation(index, num + 2, index - lineLocations[num] + 1);
	}

	public abstract void Walk(PythonWalker walker);

	public override string ToString()
	{
		return GetType().Name;
	}

	internal virtual string GetDocumentation(Statement stmt)
	{
		if (StripDocStrings)
		{
			return null;
		}
		return stmt.Documentation;
	}

	internal static System.Linq.Expressions.Expression[] ToObjectArray(IList<Expression> expressions)
	{
		System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[expressions.Count];
		for (int i = 0; i < expressions.Count; i++)
		{
			array[i] = Utils.Convert(expressions[i], typeof(object));
		}
		return array;
	}

	internal static System.Linq.Expressions.Expression TransformOrConstantNull(Expression expression, Type type)
	{
		if (expression == null)
		{
			return Utils.Constant(null, type);
		}
		return Utils.Convert(expression, type);
	}

	internal System.Linq.Expressions.Expression TransformAndDynamicConvert(Expression expression, Type type)
	{
		System.Linq.Expressions.Expression result = expression;
		if (!CanAssign(type, expression.Type))
		{
			System.Linq.Expressions.Expression expression2 = expression.Reduce();
			if (expression2 is LightDynamicExpression)
			{
				expression2 = expression2.Reduce();
			}
			DynamicExpression dynamicExpression = expression2 as DynamicExpression;
			ReducableDynamicExpression reducableDynamicExpression = expression2 as ReducableDynamicExpression;
			if ((dynamicExpression != null && dynamicExpression.Binder is PythonBinaryOperationBinder) || (reducableDynamicExpression != null && reducableDynamicExpression.Binder is PythonBinaryOperationBinder))
			{
				PythonBinaryOperationBinder opBinder;
				IList<System.Linq.Expressions.Expression> list;
				if (dynamicExpression != null)
				{
					opBinder = (PythonBinaryOperationBinder)dynamicExpression.Binder;
					list = ArrayUtils.ToArray(dynamicExpression.Arguments);
				}
				else
				{
					opBinder = (PythonBinaryOperationBinder)reducableDynamicExpression.Binder;
					list = reducableDynamicExpression.Args;
				}
				ParameterMappingInfo[] array = new ParameterMappingInfo[list.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = ParameterMappingInfo.Parameter(i);
				}
				result = System.Linq.Expressions.Expression.Dynamic(GlobalParent.PyContext.BinaryOperationRetType(opBinder, GlobalParent.PyContext.Convert(type, ConversionResultKind.ExplicitCast)), type, list);
			}
			else
			{
				result = GlobalParent.Convert(type, ConversionResultKind.ExplicitCast, expression2);
			}
		}
		return result;
	}

	internal static bool CanAssign(Type to, Type from)
	{
		if (to.IsAssignableFrom(from))
		{
			return to.IsValueType == from.IsValueType;
		}
		return false;
	}

	internal static System.Linq.Expressions.Expression ConvertIfNeeded(System.Linq.Expressions.Expression expression, Type type)
	{
		if (!CanAssign(type, expression.Type))
		{
			expression = Utils.Convert(expression, type);
		}
		return expression;
	}

	internal static System.Linq.Expressions.Expression TransformMaybeSingleLineSuite(Statement body, SourceLocation prevStart)
	{
		if (body.GlobalParent.IndexToLocation(body.StartIndex).Line != prevStart.Line)
		{
			return body;
		}
		System.Linq.Expressions.Expression res = body.Reduce();
		res = RemoveDebugInfo(prevStart.Line, res);
		if (res.Type != typeof(void))
		{
			res = Utils.Void(res);
		}
		return res;
	}

	internal static System.Linq.Expressions.Expression RemoveDebugInfo(int prevStart, System.Linq.Expressions.Expression res)
	{
		if (res is BlockExpression blockExpression && blockExpression.Expressions.Count > 0 && blockExpression.Expressions[0] is DebugInfoExpression debugInfoExpression && debugInfoExpression.StartLine == prevStart)
		{
			res = ((!(blockExpression.Type == typeof(void))) ? ((System.Linq.Expressions.BinaryExpression)blockExpression.Expressions[1]).Right : blockExpression.Expressions[1]);
		}
		return res;
	}

	internal static System.Linq.Expressions.Expression AddFrame(System.Linq.Expressions.Expression localContext, System.Linq.Expressions.Expression codeObject, System.Linq.Expressions.Expression body)
	{
		return new FramedCodeExpression(localContext, codeObject, body);
	}

	internal static System.Linq.Expressions.Expression RemoveFrame(System.Linq.Expressions.Expression expression)
	{
		return new FramedCodeVisitor().Visit(expression);
	}

	internal static System.Linq.Expressions.Expression MakeAssignment(ParameterExpression variable, System.Linq.Expressions.Expression right)
	{
		return System.Linq.Expressions.Expression.Assign(variable, Utils.Convert(right, variable.Type));
	}

	internal System.Linq.Expressions.Expression MakeAssignment(ParameterExpression variable, System.Linq.Expressions.Expression right, SourceSpan span)
	{
		return GlobalParent.AddDebugInfoAndVoid(System.Linq.Expressions.Expression.Assign(variable, Utils.Convert(right, variable.Type)), span);
	}

	internal static System.Linq.Expressions.Expression AssignValue(System.Linq.Expressions.Expression expression, System.Linq.Expressions.Expression value)
	{
		if (expression is IPythonVariableExpression pythonVariableExpression)
		{
			return pythonVariableExpression.Assign(value);
		}
		return System.Linq.Expressions.Expression.Assign(expression, value);
	}

	internal static System.Linq.Expressions.Expression Delete(System.Linq.Expressions.Expression expression)
	{
		if (expression is IPythonVariableExpression pythonVariableExpression)
		{
			return pythonVariableExpression.Delete();
		}
		return System.Linq.Expressions.Expression.Assign(expression, System.Linq.Expressions.Expression.Field(null, typeof(Uninitialized).GetField("Instance")));
	}

	internal static System.Linq.Expressions.Expression UpdateLineNumber(int line)
	{
		return System.Linq.Expressions.Expression.Assign(LineNumberExpression, Utils.Constant(line));
	}

	internal static System.Linq.Expressions.Expression UpdateLineUpdated(bool updated)
	{
		return System.Linq.Expressions.Expression.Assign(LineNumberUpdated, Utils.Constant(updated));
	}

	internal static System.Linq.Expressions.Expression PushLineUpdated(bool updated, ParameterExpression saveCurrent)
	{
		return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(saveCurrent, LineNumberUpdated), System.Linq.Expressions.Expression.Assign(LineNumberUpdated, Utils.Constant(updated)));
	}

	internal static System.Linq.Expressions.Expression PopLineUpdated(ParameterExpression saveCurrent)
	{
		return System.Linq.Expressions.Expression.Assign(LineNumberUpdated, saveCurrent);
	}
}
