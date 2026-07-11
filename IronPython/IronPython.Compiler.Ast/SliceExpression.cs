using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class SliceExpression : Expression
{
	private readonly Expression _sliceStart;

	private readonly Expression _sliceStop;

	private readonly Expression _sliceStep;

	private readonly bool _stepProvided;

	public Expression SliceStart => _sliceStart;

	public Expression SliceStop => _sliceStop;

	public Expression SliceStep => _sliceStep;

	public bool StepProvided => _stepProvided;

	public SliceExpression(Expression start, Expression stop, Expression step, bool stepProvided)
	{
		_sliceStart = start;
		_sliceStop = stop;
		_sliceStep = step;
		_stepProvided = stepProvided;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return System.Linq.Expressions.Expression.Call(AstMethods.MakeSlice, Node.TransformOrConstantNull(_sliceStart, typeof(object)), Node.TransformOrConstantNull(_sliceStop, typeof(object)), Node.TransformOrConstantNull(_sliceStep, typeof(object)));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_sliceStart != null)
			{
				_sliceStart.Walk(walker);
			}
			if (_sliceStop != null)
			{
				_sliceStop.Walk(walker);
			}
			if (_sliceStep != null)
			{
				_sliceStep.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
