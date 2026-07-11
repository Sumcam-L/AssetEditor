using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

internal class DynamicGetMemberExpression : System.Linq.Expressions.Expression, IInstructionProvider
{
	private class GetMemberInstruction : Instruction
	{
		private CallSite<Func<CallSite, object, CodeContext, object>> _site;

		private readonly PythonGetMemberBinder _binder;

		public override int ConsumedStack => 2;

		public override int ProducedStack => 1;

		public GetMemberInstruction(PythonGetMemberBinder binder)
		{
			_binder = binder;
		}

		public override int Run(InterpretedFrame frame)
		{
			if (_site == null)
			{
				_site = CallSite<Func<CallSite, object, CodeContext, object>>.Create(_binder);
			}
			CodeContext arg = (CodeContext)frame.Pop();
			frame.Push(_site.Target(_site, frame.Pop(), arg));
			return 1;
		}
	}

	private readonly PythonGetMemberBinder _binder;

	private readonly CompilationMode _mode;

	private readonly System.Linq.Expressions.Expression _target;

	private readonly System.Linq.Expressions.Expression _codeContext;

	public override bool CanReduce => true;

	public override ExpressionType NodeType => ExpressionType.Extension;

	public override Type Type => typeof(object);

	public DynamicGetMemberExpression(PythonGetMemberBinder binder, CompilationMode mode, System.Linq.Expressions.Expression target, System.Linq.Expressions.Expression codeContext)
	{
		_binder = binder;
		_mode = mode;
		_target = target;
		_codeContext = codeContext;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _mode.ReduceDynamic(_binder, typeof(object), _target, _codeContext);
	}

	public void AddInstructions(LightCompiler compiler)
	{
		compiler.Compile(_target);
		compiler.Compile(_codeContext);
		compiler.Instructions.Emit(new GetMemberInstruction(_binder));
	}
}
