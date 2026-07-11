using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace IronPython.Compiler.Ast;

internal class FlowChecker : PythonWalker
{
	private BitArray _bits;

	private Stack<BitArray> _loops;

	private Dictionary<string, PythonVariable> _variables;

	private readonly ScopeStatement _scope;

	private readonly FlowDefiner _fdef;

	private readonly FlowDeleter _fdel;

	private FlowChecker(ScopeStatement scope)
	{
		_variables = scope.Variables;
		_bits = new BitArray(_variables.Count * 2);
		int num = 0;
		foreach (KeyValuePair<string, PythonVariable> variable in _variables)
		{
			variable.Value.Index = num++;
		}
		_scope = scope;
		_fdef = new FlowDefiner(this);
		_fdel = new FlowDeleter(this);
	}

	[Conditional("DEBUG")]
	public void Dump(BitArray bits)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("FlowChecker ({0})", (_scope is FunctionDefinition) ? ((FunctionDefinition)_scope).Name : ((_scope is ClassDefinition) ? ((ClassDefinition)_scope).Name : ""));
		stringBuilder.Append('{');
		bool flag = false;
		foreach (KeyValuePair<string, PythonVariable> variable in _variables)
		{
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				flag = true;
			}
			int num = 2 * variable.Value.Index;
			stringBuilder.AppendFormat("{0}:{1}{2}", variable.Key, bits.Get(num) ? "*" : "-", bits.Get(num + 1) ? "-" : "*");
			if (variable.Value.ReadBeforeInitialized)
			{
				stringBuilder.Append("#");
			}
		}
		stringBuilder.Append('}');
	}

	private void SetAssigned(PythonVariable variable, bool value)
	{
		_bits.Set(variable.Index * 2, value);
	}

	private void SetInitialized(PythonVariable variable, bool value)
	{
		_bits.Set(variable.Index * 2 + 1, value);
	}

	private bool IsAssigned(PythonVariable variable)
	{
		return _bits.Get(variable.Index * 2);
	}

	private bool IsInitialized(PythonVariable variable)
	{
		return _bits.Get(variable.Index * 2 + 1);
	}

	public static void Check(ScopeStatement scope)
	{
		if (scope.Variables != null)
		{
			FlowChecker walker = new FlowChecker(scope);
			scope.Walk(walker);
		}
	}

	public void Define(string name)
	{
		if (_variables.TryGetValue(name, out var value))
		{
			SetAssigned(value, value: true);
			SetInitialized(value, value: true);
		}
	}

	public void Delete(string name)
	{
		if (_variables.TryGetValue(name, out var value))
		{
			SetAssigned(value, value: false);
			SetInitialized(value, value: true);
		}
	}

	private void PushLoop(BitArray ba)
	{
		if (_loops == null)
		{
			_loops = new Stack<BitArray>();
		}
		_loops.Push(ba);
	}

	private BitArray PeekLoop()
	{
		if (_loops == null || _loops.Count <= 0)
		{
			return null;
		}
		return _loops.Peek();
	}

	private void PopLoop()
	{
		if (_loops != null)
		{
			_loops.Pop();
		}
	}

	public override bool Walk(LambdaExpression node)
	{
		return false;
	}

	public override bool Walk(ListComprehension node)
	{
		BitArray bits = _bits;
		_bits = new BitArray(_bits);
		foreach (ComprehensionIterator iterator in node.Iterators)
		{
			iterator.Walk(this);
		}
		node.Item.Walk(this);
		_bits = bits;
		return false;
	}

	public override bool Walk(SetComprehension node)
	{
		BitArray bits = _bits;
		_bits = new BitArray(_bits);
		foreach (ComprehensionIterator iterator in node.Iterators)
		{
			iterator.Walk(this);
		}
		node.Item.Walk(this);
		_bits = bits;
		return false;
	}

	public override bool Walk(DictionaryComprehension node)
	{
		BitArray bits = _bits;
		_bits = new BitArray(_bits);
		foreach (ComprehensionIterator iterator in node.Iterators)
		{
			iterator.Walk(this);
		}
		node.Key.Walk(this);
		node.Value.Walk(this);
		_bits = bits;
		return false;
	}

	public override bool Walk(NameExpression node)
	{
		if (_variables.TryGetValue(node.Name, out var value))
		{
			node.Assigned = IsAssigned(value);
			if (!IsInitialized(value))
			{
				value.ReadBeforeInitialized = true;
			}
		}
		return true;
	}

	public override void PostWalk(NameExpression node)
	{
	}

	public override bool Walk(AssignmentStatement node)
	{
		node.Right.Walk(this);
		foreach (Expression item in node.Left)
		{
			item.Walk(_fdef);
		}
		return false;
	}

	public override void PostWalk(AssignmentStatement node)
	{
	}

	public override bool Walk(AugmentedAssignStatement node)
	{
		return true;
	}

	public override void PostWalk(AugmentedAssignStatement node)
	{
		node.Left.Walk(_fdef);
	}

	public override bool Walk(BreakStatement node)
	{
		PeekLoop()?.And(_bits);
		return true;
	}

	public override bool Walk(ClassDefinition node)
	{
		if (_scope == node)
		{
			return true;
		}
		Define(node.Name);
		foreach (Expression basis in node.Bases)
		{
			basis.Walk(this);
		}
		return false;
	}

	public override bool Walk(ContinueStatement node)
	{
		return true;
	}

	public override void PostWalk(DelStatement node)
	{
		foreach (Expression expression in node.Expressions)
		{
			expression.Walk(_fdel);
		}
	}

	public override bool Walk(ForStatement node)
	{
		node.List.Walk(this);
		BitArray bitArray = new BitArray(_bits);
		BitArray bitArray2 = new BitArray(_bits.Length, defaultValue: true);
		PushLoop(bitArray2);
		node.Left.Walk(_fdef);
		node.Body.Walk(this);
		PopLoop();
		_bits.And(bitArray2);
		if (node.Else != null)
		{
			BitArray bits = _bits;
			_bits = bitArray;
			node.Else.Walk(this);
			_bits = bits;
		}
		_bits.And(bitArray);
		return false;
	}

	public override bool Walk(FromImportStatement node)
	{
		if (node.Names != FromImportStatement.Star)
		{
			for (int i = 0; i < node.Names.Count; i++)
			{
				Define((node.AsNames[i] != null) ? node.AsNames[i] : node.Names[i]);
			}
		}
		return true;
	}

	public override bool Walk(FunctionDefinition node)
	{
		if (node == _scope)
		{
			foreach (Parameter parameter in node.Parameters)
			{
				parameter.Walk(_fdef);
			}
			return true;
		}
		Define(node.Name);
		foreach (Parameter parameter2 in node.Parameters)
		{
			if (parameter2.DefaultValue != null)
			{
				parameter2.DefaultValue.Walk(this);
			}
		}
		return false;
	}

	public override bool Walk(IfStatement node)
	{
		BitArray bitArray = new BitArray(_bits.Length, defaultValue: true);
		BitArray bits = _bits;
		_bits = new BitArray(_bits.Length);
		foreach (IfStatementTest test in node.Tests)
		{
			_bits.SetAll(value: false);
			_bits.Or(bits);
			test.Test.Walk(this);
			test.Body.Walk(this);
			bitArray.And(_bits);
		}
		_bits.SetAll(value: false);
		_bits.Or(bits);
		if (node.ElseStatement != null)
		{
			node.ElseStatement.Walk(this);
		}
		bitArray.And(_bits);
		_bits = bits;
		_bits.SetAll(value: false);
		_bits.Or(bitArray);
		return false;
	}

	public override bool Walk(ImportStatement node)
	{
		for (int i = 0; i < node.Names.Count; i++)
		{
			Define((node.AsNames[i] != null) ? node.AsNames[i] : node.Names[i].Names[0]);
		}
		return true;
	}

	public override void PostWalk(ReturnStatement node)
	{
	}

	public override bool Walk(WithStatement node)
	{
		node.ContextManager.Walk(this);
		BitArray bits = _bits;
		_bits = new BitArray(_bits);
		if (node.Variable != null)
		{
			node.Variable.Walk(_fdef);
		}
		node.Body.Walk(this);
		_bits = bits;
		return false;
	}

	public override bool Walk(TryStatement node)
	{
		BitArray bits = _bits;
		_bits = new BitArray(_bits);
		node.Body.Walk(this);
		if (node.Else != null)
		{
			node.Else.Walk(this);
		}
		if (node.Handlers != null)
		{
			foreach (TryStatementHandler handler in node.Handlers)
			{
				_bits.SetAll(value: false);
				_bits.Or(bits);
				if (handler.Test != null)
				{
					handler.Test.Walk(this);
				}
				if (handler.Target != null)
				{
					handler.Target.Walk(_fdef);
				}
				handler.Body.Walk(this);
			}
		}
		_bits = bits;
		if (node.Finally != null)
		{
			node.Finally.Walk(this);
		}
		return false;
	}

	public override bool Walk(WhileStatement node)
	{
		node.Test.Walk(this);
		BitArray bitArray = ((node.ElseStatement != null) ? new BitArray(_bits) : null);
		BitArray bitArray2 = new BitArray(_bits.Length, defaultValue: true);
		PushLoop(bitArray2);
		node.Body.Walk(this);
		PopLoop();
		_bits.And(bitArray2);
		if (node.ElseStatement != null)
		{
			BitArray bits = _bits;
			_bits = bitArray;
			node.ElseStatement.Walk(this);
			_bits = bits;
			_bits.And(bitArray);
		}
		return false;
	}
}
