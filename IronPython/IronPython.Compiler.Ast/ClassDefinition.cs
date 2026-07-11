using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public class ClassDefinition : ScopeStatement
{
	private class SelfNameFinder : PythonWalker
	{
		private readonly FunctionDefinition _function;

		private readonly Parameter _self;

		private Dictionary<string, bool> _names = new Dictionary<string, bool>(StringComparer.Ordinal);

		public SelfNameFinder(FunctionDefinition function, Parameter self)
		{
			_function = function;
			_self = self;
		}

		public static string[] FindNames(FunctionDefinition function)
		{
			IList<Parameter> parameters = function.Parameters;
			if (parameters.Count > 0)
			{
				SelfNameFinder selfNameFinder = new SelfNameFinder(function, parameters[0]);
				function.Body.Walk(selfNameFinder);
				return ArrayUtils.ToArray(selfNameFinder._names.Keys);
			}
			return ArrayUtils.EmptyStrings;
		}

		private bool IsSelfReference(Expression expr)
		{
			if (!(expr is NameExpression nameExpression))
			{
				return false;
			}
			if (_function.TryGetVariable(nameExpression.Name, out var variable) && variable == _self.PythonVariable)
			{
				return true;
			}
			return false;
		}

		public override bool Walk(ClassDefinition node)
		{
			return false;
		}

		public override bool Walk(FunctionDefinition node)
		{
			return false;
		}

		public override bool Walk(AssignmentStatement node)
		{
			foreach (Expression item in node.Left)
			{
				if (item is MemberExpression memberExpression && IsSelfReference(memberExpression.Target))
				{
					_names[memberExpression.Name] = true;
				}
			}
			return true;
		}
	}

	private int _headerIndex;

	private readonly string _name;

	private Statement _body;

	private readonly Expression[] _bases;

	private IList<Expression> _decorators;

	private PythonVariable _variable;

	private PythonVariable _modVariable;

	private PythonVariable _docVariable;

	private PythonVariable _modNameVariable;

	private LightLambdaExpression _dlrBody;

	private static int _classId;

	private static ParameterExpression _parentContextParam = System.Linq.Expressions.Expression.Parameter(typeof(CodeContext), "$parentContext");

	private static System.Linq.Expressions.Expression _tupleExpression = System.Linq.Expressions.Expression.Call(AstMethods.GetClosureTupleFromContext, _parentContextParam);

	private static System.Linq.Expressions.Expression NullLambda = Utils.Default(typeof(Func<CodeContext, CodeContext>));

	public SourceLocation Header => base.GlobalParent.IndexToLocation(_headerIndex);

	public int HeaderIndex
	{
		get
		{
			return _headerIndex;
		}
		set
		{
			_headerIndex = value;
		}
	}

	public override string Name => _name;

	public IList<Expression> Bases => _bases;

	public Statement Body => _body;

	public IList<Expression> Decorators
	{
		get
		{
			return _decorators;
		}
		internal set
		{
			_decorators = value;
		}
	}

	internal PythonVariable PythonVariable
	{
		get
		{
			return _variable;
		}
		set
		{
			_variable = value;
		}
	}

	internal PythonVariable ModVariable
	{
		get
		{
			return _modVariable;
		}
		set
		{
			_modVariable = value;
		}
	}

	internal PythonVariable DocVariable
	{
		get
		{
			return _docVariable;
		}
		set
		{
			_docVariable = value;
		}
	}

	internal PythonVariable ModuleNameVariable
	{
		get
		{
			return _modNameVariable;
		}
		set
		{
			_modNameVariable = value;
		}
	}

	internal override bool HasLateBoundVariableSets
	{
		get
		{
			if (!base.HasLateBoundVariableSets)
			{
				return base.NeedsLocalsDictionary;
			}
			return true;
		}
		set
		{
			base.HasLateBoundVariableSets = value;
		}
	}

	internal override string ScopeDocumentation => GetDocumentation(_body);

	public ClassDefinition(string name, Expression[] bases, Statement body)
	{
		ContractUtils.RequiresNotNull(body, "body");
		ContractUtils.RequiresNotNullItems(bases, "bases");
		_name = name;
		_bases = bases;
		_body = body;
	}

	internal override bool ExposesLocalVariable(PythonVariable variable)
	{
		return true;
	}

	internal override PythonVariable BindReference(PythonNameBinder binder, PythonReference reference)
	{
		if (TryGetVariable(reference.Name, out var variable))
		{
			if (variable.Kind == VariableKind.Global)
			{
				AddReferencedGlobal(reference.Name);
			}
			else if (variable.Kind == VariableKind.Local)
			{
				return null;
			}
			return variable;
		}
		for (ScopeStatement parent = base.Parent; parent != null; parent = parent.Parent)
		{
			if (parent.TryBindOuter(this, reference, out variable))
			{
				return variable;
			}
		}
		return null;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		FunctionCode codeObj = GetOrMakeFunctionCode();
		System.Linq.Expressions.Expression expression = (base.FuncCodeExpr = base.GlobalParent.Constant(codeObj));
		System.Linq.Expressions.Expression expression3;
		if (base.EmitDebugSymbols)
		{
			expression3 = GetLambda();
		}
		else
		{
			expression3 = NullLambda;
			ThreadPool.QueueUserWorkItem(delegate
			{
				codeObj.UpdateDelegate(base.PyContext, forceCreation: true);
			});
		}
		System.Linq.Expressions.Expression ret = System.Linq.Expressions.Expression.Call(AstMethods.MakeClass, expression, expression3, base.Parent.LocalContext, Utils.Constant(_name), System.Linq.Expressions.Expression.NewArrayInit(typeof(object), Node.ToObjectArray(_bases)), Utils.Constant(FindSelfNames()));
		ret = AddDecorators(ret, _decorators);
		return base.GlobalParent.AddDebugInfoAndVoid(Node.AssignValue(base.Parent.GetVariableExpression(_variable), ret), new SourceSpan(base.GlobalParent.IndexToLocation(base.StartIndex), base.GlobalParent.IndexToLocation(HeaderIndex)));
	}

	private LightExpression<Func<CodeContext, CodeContext>> MakeClassBody()
	{
		List<System.Linq.Expressions.Expression> list = new List<System.Linq.Expressions.Expression>();
		ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>();
		readOnlyCollectionBuilder.Add(ScopeStatement.LocalCodeContextVariable);
		readOnlyCollectionBuilder.Add(PythonAst._globalContext);
		list.Add(System.Linq.Expressions.Expression.Assign(PythonAst._globalContext, new GetGlobalContextExpression(_parentContextParam)));
		base.GlobalParent.PrepareScope(readOnlyCollectionBuilder, list);
		CreateVariables(readOnlyCollectionBuilder, list);
		MethodCallExpression right = CreateLocalContext(_parentContextParam);
		list.Add(System.Linq.Expressions.Expression.Assign(ScopeStatement.LocalCodeContextVariable, right));
		List<System.Linq.Expressions.Expression> list2 = new List<System.Linq.Expressions.Expression>();
		System.Linq.Expressions.Expression expression = _body;
		System.Linq.Expressions.Expression arg = Node.AssignValue(GetVariableExpression(_modVariable), GetVariableExpression(_modNameVariable));
		string documentation = GetDocumentation(_body);
		if (documentation != null)
		{
			list2.Add(Node.AssignValue(GetVariableExpression(_docVariable), Utils.Constant(documentation)));
		}
		if (_body.CanThrow && base.GlobalParent.PyContext.PythonOptions.Frames)
		{
			expression = Node.AddFrame(LocalContext, base.FuncCodeExpr, expression);
			readOnlyCollectionBuilder.Add(Node.FunctionStackVariable);
		}
		expression = WrapScopeStatements(System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Block(list), (list2.Count == 0) ? Node.EmptyBlock : System.Linq.Expressions.Expression.Block(new ReadOnlyCollection<System.Linq.Expressions.Expression>(list2)), arg, expression, LocalContext), _body.CanThrow);
		return Utils.LightLambda<Func<CodeContext, CodeContext>>(typeof(CodeContext), System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder, expression), Name + "$" + Interlocked.Increment(ref _classId), new ParameterExpression[1] { _parentContextParam });
	}

	internal override LightLambdaExpression GetLambda()
	{
		if (_dlrBody == null)
		{
			_dlrBody = MakeClassBody();
		}
		return _dlrBody;
	}

	internal override System.Linq.Expressions.Expression GetParentClosureTuple()
	{
		return _tupleExpression;
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_decorators != null)
			{
				foreach (Expression decorator in _decorators)
				{
					decorator.Walk(walker);
				}
			}
			if (_bases != null)
			{
				Expression[] bases = _bases;
				foreach (Expression expression in bases)
				{
					expression.Walk(walker);
				}
			}
			if (_body != null)
			{
				_body.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}

	private string FindSelfNames()
	{
		if (!(Body is SuiteStatement suiteStatement))
		{
			return "";
		}
		foreach (Statement statement in suiteStatement.Statements)
		{
			if (statement is FunctionDefinition { Name: "__init__" } functionDefinition)
			{
				return string.Join(",", SelfNameFinder.FindNames(functionDefinition));
			}
		}
		return "";
	}

	internal override void RewriteBody(PythonAst.LookupVisitor visitor)
	{
		_dlrBody = null;
		_body = new PythonAst.RewrittenBodyStatement(Body, visitor.Visit(Body));
	}
}
