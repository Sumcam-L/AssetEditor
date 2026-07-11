using System.Linq.Expressions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

internal sealed class LazyCode<T> : IExpressionSerializable where T : class
{
	public Expression<T> Code;

	private T Delegate;

	private readonly bool _shouldInterpret;

	private readonly int _compilationThreshold;

	public LazyCode(Expression<T> code, bool shouldInterpret, int compilationThreshold)
	{
		Code = code;
		_shouldInterpret = shouldInterpret;
		_compilationThreshold = compilationThreshold;
	}

	public T EnsureDelegate()
	{
		if (Delegate == null)
		{
			lock (this)
			{
				if (Delegate == null)
				{
					Delegate = Compile();
					Code = null;
				}
			}
		}
		return Delegate;
	}

	private T Compile()
	{
		if (_shouldInterpret)
		{
			return (T)Code.LightCompile(_compilationThreshold);
		}
		return Code.Compile();
	}

	public Expression CreateExpression()
	{
		return Code;
	}
}
