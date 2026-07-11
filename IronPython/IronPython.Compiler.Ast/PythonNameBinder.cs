using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler.Ast;

internal class PythonNameBinder : PythonWalker
{
	private PythonAst _globalScope;

	internal ScopeStatement _currentScope;

	private List<ScopeStatement> _scopes = new List<ScopeStatement>();

	private List<ILoopStatement> _loops = new List<ILoopStatement>();

	private List<int> _finallyCount = new List<int>();

	private DefineBinder _define;

	private DeleteBinder _delete;

	private ParameterBinder _parameter;

	private readonly CompilerContext _context;

	public CompilerContext Context => _context;

	private PythonNameBinder(CompilerContext context)
	{
		_define = new DefineBinder(this);
		_delete = new DeleteBinder(this);
		_parameter = new ParameterBinder(this);
		_context = context;
	}

	internal static void BindAst(PythonAst ast, CompilerContext context)
	{
		PythonNameBinder pythonNameBinder = new PythonNameBinder(context);
		pythonNameBinder.Bind(ast);
	}

	private void Bind(PythonAst unboundAst)
	{
		_currentScope = (_globalScope = unboundAst);
		_finallyCount.Add(0);
		unboundAst.Walk(this);
		foreach (ScopeStatement scope in _scopes)
		{
			scope.Bind(this);
		}
		unboundAst.Bind(this);
		for (int num = _scopes.Count - 1; num >= 0; num--)
		{
			_scopes[num].FinishBind(this);
		}
		unboundAst.FinishBind(this);
		foreach (ScopeStatement scope2 in _scopes)
		{
			FlowChecker.Check(scope2);
		}
	}

	private void PushScope(ScopeStatement node)
	{
		node.Parent = _currentScope;
		_currentScope = node;
		_finallyCount.Add(0);
	}

	private void PopScope()
	{
		_scopes.Add(_currentScope);
		_currentScope = _currentScope.Parent;
		_finallyCount.RemoveAt(_finallyCount.Count - 1);
	}

	internal PythonReference Reference(string name)
	{
		return _currentScope.Reference(name);
	}

	internal PythonVariable DefineName(string name)
	{
		return _currentScope.EnsureVariable(name);
	}

	internal PythonVariable DefineParameter(string name)
	{
		return _currentScope.DefineParameter(name);
	}

	internal PythonVariable DefineDeleted(string name)
	{
		PythonVariable pythonVariable = _currentScope.EnsureVariable(name);
		pythonVariable.Deleted = true;
		return pythonVariable;
	}

	internal void ReportSyntaxWarning(string message, Node node)
	{
		_context.Errors.Add(_context.SourceUnit, message, node.Span, -1, Severity.Warning);
	}

	internal void ReportSyntaxError(string message, Node node)
	{
		_context.Errors.Add(_context.SourceUnit, message, node.Span, -1, Severity.FatalError);
		throw PythonOps.SyntaxError(message, _context.SourceUnit, node.Span, -1);
	}

	public override bool Walk(AssignmentStatement node)
	{
		node.Parent = _currentScope;
		foreach (Expression item in node.Left)
		{
			item.Walk(_define);
		}
		return true;
	}

	public override bool Walk(AugmentedAssignStatement node)
	{
		node.Parent = _currentScope;
		node.Left.Walk(_define);
		return true;
	}

	public override void PostWalk(CallExpression node)
	{
		if (node.NeedsLocalsDictionary())
		{
			_currentScope.NeedsLocalsDictionary = true;
		}
	}

	public override bool Walk(ClassDefinition node)
	{
		node.PythonVariable = DefineName(node.Name);
		foreach (Expression basis in node.Bases)
		{
			basis.Walk(this);
		}
		if (node.Decorators != null)
		{
			foreach (Expression decorator in node.Decorators)
			{
				decorator.Walk(this);
			}
		}
		PushScope(node);
		node.ModuleNameVariable = _globalScope.EnsureGlobalVariable("__name__");
		if (node.Body.Documentation != null)
		{
			node.DocVariable = DefineName("__doc__");
		}
		node.ModVariable = DefineName("__module__");
		node.Body.Walk(this);
		return false;
	}

	public override void PostWalk(ClassDefinition node)
	{
		PopScope();
	}

	public override bool Walk(DelStatement node)
	{
		node.Parent = _currentScope;
		foreach (Expression expression in node.Expressions)
		{
			expression.Walk(_delete);
		}
		return true;
	}

	public override bool Walk(ExecStatement node)
	{
		node.Parent = _currentScope;
		if (node.Locals == null && node.Globals == null)
		{
			_currentScope.ContainsUnqualifiedExec = true;
		}
		return true;
	}

	public override void PostWalk(ExecStatement node)
	{
		if (node.NeedsLocalsDictionary())
		{
			_currentScope.NeedsLocalsDictionary = true;
		}
		if (node.Locals == null)
		{
			_currentScope.HasLateBoundVariableSets = true;
		}
	}

	public override bool Walk(ExpressionStatement node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(BinaryExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(AndExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(CallExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(ConditionalExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(IndexExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(ListComprehension node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(SetComprehension node)
	{
		node.Parent = _currentScope;
		PushScope(node.Scope);
		return base.Walk(node);
	}

	public override void PostWalk(SetComprehension node)
	{
		base.PostWalk(node);
		PopScope();
		if (node.Scope.NeedsLocalsDictionary)
		{
			_currentScope.NeedsLocalsDictionary = true;
		}
	}

	public override bool Walk(DictionaryComprehension node)
	{
		node.Parent = _currentScope;
		PushScope(node.Scope);
		return base.Walk(node);
	}

	public override void PostWalk(DictionaryComprehension node)
	{
		base.PostWalk(node);
		PopScope();
		if (node.Scope.NeedsLocalsDictionary)
		{
			_currentScope.NeedsLocalsDictionary = true;
		}
	}

	public override bool Walk(ComprehensionIf node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(MemberExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(TupleExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(ListExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(DictionaryExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(YieldExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(UnaryExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(SliceExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override void PostWalk(ConditionalExpression node)
	{
		node.Parent = _currentScope;
		base.PostWalk(node);
	}

	public override bool Walk(BackQuoteExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(ConstantExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(GeneratorExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(OrExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(LambdaExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(ParenthesisExpression node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(EmptyStatement node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(RaiseStatement node)
	{
		node.Parent = _currentScope;
		node.InFinally = _finallyCount[_finallyCount.Count - 1] != 0;
		return base.Walk(node);
	}

	public override bool Walk(SuiteStatement node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(ForStatement node)
	{
		node.Parent = _currentScope;
		if (_currentScope is FunctionDefinition)
		{
			_currentScope.ShouldInterpret = false;
		}
		node.Left.Walk(_define);
		if (node.Left != null)
		{
			node.Left.Walk(this);
		}
		if (node.List != null)
		{
			node.List.Walk(this);
		}
		PushLoop(node);
		if (node.Body != null)
		{
			node.Body.Walk(this);
		}
		PopLoop();
		if (node.Else != null)
		{
			node.Else.Walk(this);
		}
		return false;
	}

	private void PushLoop(ILoopStatement node)
	{
		node.BreakLabel = System.Linq.Expressions.Expression.Label("break");
		node.ContinueLabel = System.Linq.Expressions.Expression.Label("continue");
		_loops.Add(node);
	}

	private void PopLoop()
	{
		_loops.RemoveAt(_loops.Count - 1);
	}

	public override bool Walk(WhileStatement node)
	{
		node.Parent = _currentScope;
		if (node.Test != null)
		{
			node.Test.Walk(this);
		}
		PushLoop(node);
		if (node.Body != null)
		{
			node.Body.Walk(this);
		}
		PopLoop();
		if (node.ElseStatement != null)
		{
			node.ElseStatement.Walk(this);
		}
		return false;
	}

	public override bool Walk(BreakStatement node)
	{
		node.Parent = _currentScope;
		node.LoopStatement = _loops[_loops.Count - 1];
		return base.Walk(node);
	}

	public override bool Walk(ContinueStatement node)
	{
		node.Parent = _currentScope;
		node.LoopStatement = _loops[_loops.Count - 1];
		return base.Walk(node);
	}

	public override bool Walk(ReturnStatement node)
	{
		node.Parent = _currentScope;
		if (_currentScope is FunctionDefinition functionDefinition)
		{
			functionDefinition._hasReturn = true;
		}
		return base.Walk(node);
	}

	public override bool Walk(WithStatement node)
	{
		node.Parent = _currentScope;
		_currentScope.ContainsExceptionHandling = true;
		if (node.Variable != null)
		{
			node.Variable.Walk(_define);
		}
		return true;
	}

	public override bool Walk(FromImportStatement node)
	{
		node.Parent = _currentScope;
		if (node.Names != FromImportStatement.Star)
		{
			PythonVariable[] array = new PythonVariable[node.Names.Count];
			node.Root.Parent = _currentScope;
			for (int i = 0; i < node.Names.Count; i++)
			{
				string name = ((node.AsNames[i] != null) ? node.AsNames[i] : node.Names[i]);
				array[i] = DefineName(name);
			}
			node.Variables = array;
		}
		else
		{
			_currentScope.ContainsImportStar = true;
			_currentScope.NeedsLocalsDictionary = true;
			_currentScope.HasLateBoundVariableSets = true;
		}
		return true;
	}

	public override bool Walk(FunctionDefinition node)
	{
		node._nameVariable = _globalScope.EnsureGlobalVariable("__name__");
		if (!node.IsLambda)
		{
			node.PythonVariable = DefineName(node.Name);
		}
		foreach (Parameter parameter in node.Parameters)
		{
			if (parameter.DefaultValue != null)
			{
				parameter.DefaultValue.Walk(this);
			}
		}
		if (node.Decorators != null)
		{
			foreach (Expression decorator in node.Decorators)
			{
				decorator.Walk(this);
			}
		}
		PushScope(node);
		foreach (Parameter parameter2 in node.Parameters)
		{
			parameter2.Walk(_parameter);
		}
		node.Body.Walk(this);
		return false;
	}

	public override void PostWalk(FunctionDefinition node)
	{
		PopScope();
	}

	public override bool Walk(GlobalStatement node)
	{
		node.Parent = _currentScope;
		foreach (string name in node.Names)
		{
			bool flag = false;
			if (_currentScope.TryGetVariable(name, out var variable))
			{
				switch (variable.Kind)
				{
				case VariableKind.Local:
				case VariableKind.Global:
					flag = true;
					ReportSyntaxWarning(string.Format(CultureInfo.InvariantCulture, "name '{0}' is assigned to before global declaration", new object[1] { name }), node);
					break;
				case VariableKind.Parameter:
					ReportSyntaxError(string.Format(CultureInfo.InvariantCulture, "Name '{0}' is a function parameter and declared global", new object[1] { name }), node);
					break;
				}
			}
			if (_currentScope.IsReferenced(name) && !flag)
			{
				ReportSyntaxWarning(string.Format(CultureInfo.InvariantCulture, "name '{0}' is used prior to global declaration", new object[1] { name }), node);
			}
			PythonVariable pythonVariable = _globalScope.EnsureGlobalVariable(name);
			pythonVariable.Kind = VariableKind.Global;
			if (variable == null)
			{
				_currentScope.AddGlobalVariable(pythonVariable);
			}
		}
		return true;
	}

	public override bool Walk(NameExpression node)
	{
		node.Parent = _currentScope;
		node.Reference = Reference(node.Name);
		return true;
	}

	public override bool Walk(PrintStatement node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(IfStatement node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(AssertStatement node)
	{
		node.Parent = _currentScope;
		return base.Walk(node);
	}

	public override bool Walk(PythonAst node)
	{
		if (node.Module)
		{
			node.NameVariable = DefineName("__name__");
			node.FileVariable = DefineName("__file__");
			node.DocVariable = DefineName("__doc__");
			DefineName("__path__");
			DefineName("__builtins__");
			DefineName("__package__");
		}
		return true;
	}

	public override void PostWalk(PythonAst node)
	{
		_currentScope = _currentScope.Parent;
		_finallyCount.RemoveAt(_finallyCount.Count - 1);
	}

	public override bool Walk(ImportStatement node)
	{
		node.Parent = _currentScope;
		PythonVariable[] array = new PythonVariable[node.Names.Count];
		for (int i = 0; i < node.Names.Count; i++)
		{
			string name = ((node.AsNames[i] != null) ? node.AsNames[i] : node.Names[i].Names[0]);
			array[i] = DefineName(name);
			node.Names[i].Parent = _currentScope;
		}
		node.Variables = array;
		return true;
	}

	public override bool Walk(TryStatement node)
	{
		node.Parent = _currentScope;
		_currentScope.ContainsExceptionHandling = true;
		node.Body.Walk(this);
		if (node.Handlers != null)
		{
			foreach (TryStatementHandler handler in node.Handlers)
			{
				if (handler.Target != null)
				{
					handler.Target.Walk(_define);
				}
				handler.Parent = _currentScope;
				handler.Walk(this);
			}
		}
		if (node.Else != null)
		{
			node.Else.Walk(this);
		}
		if (node.Finally != null)
		{
			_finallyCount[_finallyCount.Count - 1]++;
			node.Finally.Walk(this);
			_finallyCount[_finallyCount.Count - 1]--;
		}
		return false;
	}

	public override bool Walk(ComprehensionFor node)
	{
		node.Parent = _currentScope;
		node.Left.Walk(_define);
		return true;
	}
}
