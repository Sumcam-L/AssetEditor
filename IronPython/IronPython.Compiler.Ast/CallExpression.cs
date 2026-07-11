using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

public class CallExpression : Expression, IInstructionProvider
{
	private abstract class InvokeInstruction : Instruction
	{
		public override int ProducedStack => 1;

		public override string InstructionName => "Python Invoke" + (ConsumedStack - 1);
	}

	private class Invoke0Instruction : InvokeInstruction
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object>> _site;

		public override int ConsumedStack => 2;

		public Invoke0Instruction(PythonContext context)
		{
			_site = context.CallSite0;
		}

		public override int Run(InterpretedFrame frame)
		{
			object arg = frame.Pop();
			frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), arg));
			return 1;
		}
	}

	private class Invoke1Instruction : InvokeInstruction
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, object>> _site;

		public override int ConsumedStack => 3;

		public Invoke1Instruction(PythonContext context)
		{
			_site = context.CallSite1;
		}

		public override int Run(InterpretedFrame frame)
		{
			object arg = frame.Pop();
			object arg2 = frame.Pop();
			frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), arg2, arg));
			return 1;
		}
	}

	private class Invoke2Instruction : InvokeInstruction
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object>> _site;

		public override int ConsumedStack => 4;

		public Invoke2Instruction(PythonContext context)
		{
			_site = context.CallSite2;
		}

		public override int Run(InterpretedFrame frame)
		{
			object arg = frame.Pop();
			object arg2 = frame.Pop();
			object arg3 = frame.Pop();
			frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), arg3, arg2, arg));
			return 1;
		}
	}

	private class Invoke3Instruction : InvokeInstruction
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object, object>> _site;

		public override int ConsumedStack => 5;

		public Invoke3Instruction(PythonContext context)
		{
			_site = context.CallSite3;
		}

		public override int Run(InterpretedFrame frame)
		{
			object arg = frame.Pop();
			object arg2 = frame.Pop();
			object arg3 = frame.Pop();
			object arg4 = frame.Pop();
			frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), arg4, arg3, arg2, arg));
			return 1;
		}
	}

	private class Invoke4Instruction : InvokeInstruction
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object>> _site;

		public override int ConsumedStack => 6;

		public Invoke4Instruction(PythonContext context)
		{
			_site = context.CallSite4;
		}

		public override int Run(InterpretedFrame frame)
		{
			object arg = frame.Pop();
			object arg2 = frame.Pop();
			object arg3 = frame.Pop();
			object arg4 = frame.Pop();
			object arg5 = frame.Pop();
			frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), arg5, arg4, arg3, arg2, arg));
			return 1;
		}
	}

	private class Invoke5Instruction : InvokeInstruction
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object>> _site;

		public override int ConsumedStack => 7;

		public Invoke5Instruction(PythonContext context)
		{
			_site = context.CallSite5;
		}

		public override int Run(InterpretedFrame frame)
		{
			object arg = frame.Pop();
			object arg2 = frame.Pop();
			object arg3 = frame.Pop();
			object arg4 = frame.Pop();
			object arg5 = frame.Pop();
			object arg6 = frame.Pop();
			frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), arg6, arg5, arg4, arg3, arg2, arg));
			return 1;
		}
	}

	private class Invoke6Instruction : InvokeInstruction
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object, object>> _site;

		public override int ConsumedStack => 8;

		public Invoke6Instruction(PythonContext context)
		{
			_site = context.CallSite6;
		}

		public override int Run(InterpretedFrame frame)
		{
			object arg = frame.Pop();
			object arg2 = frame.Pop();
			object arg3 = frame.Pop();
			object arg4 = frame.Pop();
			object arg5 = frame.Pop();
			object arg6 = frame.Pop();
			object arg7 = frame.Pop();
			frame.Push(_site.Target(_site, (CodeContext)frame.Pop(), arg7, arg6, arg5, arg4, arg3, arg2, arg));
			return 1;
		}
	}

	private readonly Expression _target;

	private readonly Arg[] _args;

	private static MethodCallExpression _GetUnicode = System.Linq.Expressions.Expression.Call(AstMethods.GetUnicodeFunction);

	public Expression Target => _target;

	public IList<Arg> Args => _args;

	public CallExpression(Expression target, Arg[] args)
	{
		_target = target;
		_args = args;
	}

	public bool NeedsLocalsDictionary()
	{
		if (!(_target is NameExpression nameExpression))
		{
			return false;
		}
		if (_args.Length == 0)
		{
			if (nameExpression.Name == "locals")
			{
				return true;
			}
			if (nameExpression.Name == "vars")
			{
				return true;
			}
			if (nameExpression.Name == "dir")
			{
				return true;
			}
			return false;
		}
		if (_args.Length == 1 && (nameExpression.Name == "dir" || nameExpression.Name == "vars"))
		{
			if (_args[0].Name == "*" || _args[0].Name == "**")
			{
				return true;
			}
		}
		else if (_args.Length == 2 && (nameExpression.Name == "dir" || nameExpression.Name == "vars"))
		{
			if (_args[0].Name == "*" && _args[1].Name == "**")
			{
				return true;
			}
		}
		else
		{
			if (nameExpression.Name == "eval")
			{
				return true;
			}
			if (nameExpression.Name == "execfile")
			{
				return true;
			}
		}
		return false;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return UnicodeCall() ?? NormalCall(_target);
	}

	private System.Linq.Expressions.Expression NormalCall(System.Linq.Expressions.Expression target)
	{
		System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[_args.Length + 2];
		Argument[] array2 = new Argument[_args.Length];
		array[0] = base.Parent.LocalContext;
		array[1] = target;
		for (int i = 0; i < _args.Length; i++)
		{
			ref Argument reference = ref array2[i];
			reference = _args[i].GetArgumentInfo();
			array[i + 2] = _args[i].Expression;
		}
		return base.Parent.Invoke(new CallSignature(array2), array);
	}

	private System.Linq.Expressions.Expression UnicodeCall()
	{
		if (_target is NameExpression && ((NameExpression)_target).Name == "unicode")
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(object));
			return System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, System.Linq.Expressions.Expression.Assign(parameterExpression, _target), System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Call(AstMethods.IsUnicode, parameterExpression), NormalCall(_GetUnicode), NormalCall(parameterExpression)));
		}
		return null;
	}

	void IInstructionProvider.AddInstructions(LightCompiler compiler)
	{
		if (_target is NameExpression && ((NameExpression)_target).Name == "unicode")
		{
			compiler.Compile(Reduce());
			return;
		}
		for (int i = 0; i < _args.Length; i++)
		{
			if (!_args[i].GetArgumentInfo().IsSimple)
			{
				compiler.Compile(Reduce());
				return;
			}
		}
		switch (_args.Length)
		{
		case 0:
			compiler.Compile(base.Parent.LocalContext);
			compiler.Compile(_target);
			compiler.Instructions.Emit(new Invoke0Instruction(base.Parent.PyContext));
			break;
		case 1:
			compiler.Compile(base.Parent.LocalContext);
			compiler.Compile(_target);
			compiler.Compile(_args[0].Expression);
			compiler.Instructions.Emit(new Invoke1Instruction(base.Parent.PyContext));
			break;
		case 2:
			compiler.Compile(base.Parent.LocalContext);
			compiler.Compile(_target);
			compiler.Compile(_args[0].Expression);
			compiler.Compile(_args[1].Expression);
			compiler.Instructions.Emit(new Invoke2Instruction(base.Parent.PyContext));
			break;
		case 3:
			compiler.Compile(base.Parent.LocalContext);
			compiler.Compile(_target);
			compiler.Compile(_args[0].Expression);
			compiler.Compile(_args[1].Expression);
			compiler.Compile(_args[2].Expression);
			compiler.Instructions.Emit(new Invoke3Instruction(base.Parent.PyContext));
			break;
		case 4:
			compiler.Compile(base.Parent.LocalContext);
			compiler.Compile(_target);
			compiler.Compile(_args[0].Expression);
			compiler.Compile(_args[1].Expression);
			compiler.Compile(_args[2].Expression);
			compiler.Compile(_args[3].Expression);
			compiler.Instructions.Emit(new Invoke4Instruction(base.Parent.PyContext));
			break;
		case 5:
			compiler.Compile(base.Parent.LocalContext);
			compiler.Compile(_target);
			compiler.Compile(_args[0].Expression);
			compiler.Compile(_args[1].Expression);
			compiler.Compile(_args[2].Expression);
			compiler.Compile(_args[3].Expression);
			compiler.Compile(_args[4].Expression);
			compiler.Instructions.Emit(new Invoke5Instruction(base.Parent.PyContext));
			break;
		case 6:
			compiler.Compile(base.Parent.LocalContext);
			compiler.Compile(_target);
			compiler.Compile(_args[0].Expression);
			compiler.Compile(_args[1].Expression);
			compiler.Compile(_args[2].Expression);
			compiler.Compile(_args[3].Expression);
			compiler.Compile(_args[4].Expression);
			compiler.Compile(_args[5].Expression);
			compiler.Instructions.Emit(new Invoke6Instruction(base.Parent.PyContext));
			break;
		default:
			compiler.Compile(Reduce());
			break;
		}
	}

	internal override string CheckAssign()
	{
		return "can't assign to function call";
	}

	internal override string CheckDelete()
	{
		return "can't delete function call";
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_target != null)
			{
				_target.Walk(walker);
			}
			if (_args != null)
			{
				Arg[] args = _args;
				foreach (Arg arg in args)
				{
					arg.Walk(walker);
				}
			}
		}
		walker.PostWalk(this);
	}
}
