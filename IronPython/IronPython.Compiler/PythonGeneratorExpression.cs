using System;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler;

internal sealed class PythonGeneratorExpression : Expression
{
	private readonly LightLambdaExpression _lambda;

	private readonly int _compilationThreshold;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => _lambda.Type;

	public override bool CanReduce => true;

	public PythonGeneratorExpression(LightLambdaExpression lambda, int compilationThreshold)
	{
		_lambda = lambda;
		_compilationThreshold = compilationThreshold;
	}

	public override Expression Reduce()
	{
		return _lambda.ToGenerator(shouldInterpret: false, debuggable: true, _compilationThreshold);
	}
}
