using System.Collections.Generic;
using IronPython.Compiler.Ast;

namespace IronPython.Runtime;

internal class PythonDebuggingPayload
{
	private class TracingWalker : PythonWalker
	{
		private bool _inLoop;

		private bool _inFinally;

		private int _loopId;

		public Dictionary<int, bool> HandlerLocations = new Dictionary<int, bool>();

		public Dictionary<int, Dictionary<int, bool>> LoopAndFinallyLocations = new Dictionary<int, Dictionary<int, bool>>();

		private Dictionary<int, bool> _loopIds = new Dictionary<int, bool>();

		public Dictionary<int, bool> LoopOrFinallyIds
		{
			get
			{
				if (_loopIds == null)
				{
					_loopIds = new Dictionary<int, bool>();
				}
				return _loopIds;
			}
		}

		public override bool Walk(ForStatement node)
		{
			UpdateLoops(node);
			WalkLoopBody(node.Body, isFinally: false);
			if (node.Else != null)
			{
				node.Else.Walk(this);
			}
			return false;
		}

		private void WalkLoopBody(Statement body, bool isFinally)
		{
			bool inLoop = _inLoop;
			bool inFinally = _inFinally;
			int key = ++_loopId;
			_inFinally = false;
			_inLoop = true;
			_loopIds.Add(key, isFinally);
			body.Walk(this);
			_inLoop = inLoop;
			_inFinally = inFinally;
			LoopOrFinallyIds.Remove(key);
		}

		public override bool Walk(WhileStatement node)
		{
			UpdateLoops(node);
			WalkLoopBody(node.Body, isFinally: false);
			if (node.ElseStatement != null)
			{
				node.ElseStatement.Walk(this);
			}
			return false;
		}

		public override bool Walk(TryStatement node)
		{
			UpdateLoops(node);
			node.Body.Walk(this);
			if (node.Handlers != null)
			{
				foreach (TryStatementHandler handler in node.Handlers)
				{
					HandlerLocations[handler.Span.Start.Line] = false;
					handler.Body.Walk(this);
				}
			}
			if (node.Finally != null)
			{
				WalkLoopBody(node.Finally, isFinally: true);
			}
			return false;
		}

		public override bool Walk(AssertStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(AssignmentStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(AugmentedAssignStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(BreakStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(ClassDefinition node)
		{
			UpdateLoops(node);
			return false;
		}

		public override bool Walk(ContinueStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(ExecStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override void PostWalk(EmptyStatement node)
		{
			UpdateLoops(node);
			base.PostWalk(node);
		}

		public override bool Walk(DelStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(EmptyStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(GlobalStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(FromImportStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(ExpressionStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(FunctionDefinition node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(IfStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(ImportStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(RaiseStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		public override bool Walk(WithStatement node)
		{
			UpdateLoops(node);
			return base.Walk(node);
		}

		private void UpdateLoops(Statement stmt)
		{
			if ((_inFinally || _inLoop) && !LoopAndFinallyLocations.ContainsKey(stmt.Span.Start.Line))
			{
				LoopAndFinallyLocations.Add(stmt.Span.Start.Line, new Dictionary<int, bool>(LoopOrFinallyIds));
			}
		}
	}

	public FunctionCode Code;

	private Dictionary<int, bool> _handlerLocations;

	private Dictionary<int, Dictionary<int, bool>> _loopAndFinallyLocations;

	public Dictionary<int, bool> HandlerLocations
	{
		get
		{
			if (_handlerLocations == null)
			{
				GatherLocations();
			}
			return _handlerLocations;
		}
	}

	public Dictionary<int, Dictionary<int, bool>> LoopAndFinallyLocations
	{
		get
		{
			if (_loopAndFinallyLocations == null)
			{
				GatherLocations();
			}
			return _loopAndFinallyLocations;
		}
	}

	public PythonDebuggingPayload(FunctionCode code)
	{
		Code = code;
	}

	private void GatherLocations()
	{
		TracingWalker tracingWalker = new TracingWalker();
		Code.PythonCode.Walk(tracingWalker);
		_loopAndFinallyLocations = tracingWalker.LoopAndFinallyLocations;
		_handlerLocations = tracingWalker.HandlerLocations;
	}
}
