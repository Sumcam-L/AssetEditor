using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public class FunctionDefinition : ScopeStatement, IInstructionProvider
{
	private class FunctionDefinitionInstruction : Instruction
	{
		private readonly FunctionDefinition _def;

		private readonly int _defaultCount;

		private readonly CodeContext _context;

		private readonly PythonGlobal _name;

		public override int ConsumedStack => _defaultCount + ((_context == null) ? 1 : 0) + ((_name == null) ? 1 : 0);

		public override int ProducedStack => 1;

		public FunctionDefinitionInstruction(CodeContext context, FunctionDefinition definition, int defaultCount, PythonGlobal name)
		{
			_context = context;
			_defaultCount = defaultCount;
			_def = definition;
			_name = name;
		}

		public override int Run(InterpretedFrame frame)
		{
			object[] array;
			if (_defaultCount > 0)
			{
				array = new object[_defaultCount];
				for (int i = 0; i < _defaultCount; i++)
				{
					array[i] = frame.Pop();
				}
			}
			else
			{
				array = ArrayUtils.EmptyObjects;
			}
			object modName = ((_name == null) ? frame.Pop() : _name.RawValue);
			CodeContext context = (CodeContext)frame.Pop();
			frame.Push(PythonOps.MakeFunction(context, _def.FunctionCode, modName, array));
			return 1;
		}
	}

	protected Statement _body;

	private readonly string _name;

	private readonly Parameter[] _parameters;

	private IList<Expression> _decorators;

	private bool _generator;

	private bool _isLambda;

	private bool _canSetSysExcInfo;

	private bool _containsTryFinally;

	private PythonVariable _variable;

	internal PythonVariable _nameVariable;

	private LightLambdaExpression _dlrBody;

	internal bool _hasReturn;

	private int _headerIndex;

	private static int _lambdaId;

	internal static readonly ParameterExpression _functionParam = System.Linq.Expressions.Expression.Parameter(typeof(PythonFunction), "$function");

	private static readonly System.Linq.Expressions.Expression _GetClosureTupleFromFunctionCall = System.Linq.Expressions.Expression.Call(null, typeof(PythonOps).GetMethod("GetClosureTupleFromFunction"), _functionParam);

	private static readonly System.Linq.Expressions.Expression _parentContext = new GetParentContextFromFunctionExpression();

	internal static readonly LabelTarget _returnLabel = System.Linq.Expressions.Expression.Label(typeof(object), "return");

	internal override System.Linq.Expressions.Expression LocalContext
	{
		get
		{
			if (base.NeedsLocalContext)
			{
				return base.LocalContext;
			}
			return base.GlobalParent.LocalContext;
		}
	}

	public bool IsLambda => _isLambda;

	public IList<Parameter> Parameters => _parameters;

	internal override string[] ParameterNames => ArrayUtils.ConvertAll(_parameters, (Parameter val) => val.Name);

	internal override int ArgCount => _parameters.Length;

	public Statement Body
	{
		get
		{
			return _body;
		}
		set
		{
			_body = value;
		}
	}

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

	internal override bool IsGeneratorMethod => IsGenerator;

	public bool IsGenerator
	{
		get
		{
			return _generator;
		}
		set
		{
			_generator = value;
		}
	}

	internal bool CanSetSysExcInfo
	{
		set
		{
			_canSetSysExcInfo = value;
		}
	}

	internal bool ContainsTryFinally
	{
		get
		{
			return _containsTryFinally;
		}
		set
		{
			_containsTryFinally = value;
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

	internal override FunctionAttributes Flags
	{
		get
		{
			FunctionAttributes functionAttributes = FunctionAttributes.None;
			if (_parameters != null)
			{
				int i;
				for (i = 0; i < _parameters.Length; i++)
				{
					Parameter parameter = _parameters[i];
					if (parameter.IsDictionary || parameter.IsList)
					{
						break;
					}
				}
				if (i < _parameters.Length && _parameters[i].IsList)
				{
					i++;
					functionAttributes |= FunctionAttributes.ArgumentList;
				}
				if (i < _parameters.Length && _parameters[i].IsDictionary)
				{
					i++;
					functionAttributes |= FunctionAttributes.KeywordDictionary;
				}
			}
			if (_canSetSysExcInfo)
			{
				functionAttributes |= FunctionAttributes.CanSetSysExcInfo;
			}
			if (ContainsTryFinally)
			{
				functionAttributes |= FunctionAttributes.ContainsTryFinally;
			}
			if (IsGenerator)
			{
				functionAttributes |= FunctionAttributes.Generator;
			}
			return functionAttributes;
		}
	}

	internal override Delegate OriginalDelegate
	{
		get
		{
			bool wrapper = _parameters.Length > 15;
			GetDelegateType(_parameters, wrapper, out var originalTarget);
			return originalTarget;
		}
	}

	internal override string ScopeDocumentation => GetDocumentation(_body);

	internal FunctionCode FunctionCode => GetOrMakeFunctionCode();

	internal override string ProfilerName
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder("def ");
			stringBuilder.Append(Name);
			stringBuilder.Append('(');
			bool flag = false;
			Parameter[] parameters = _parameters;
			foreach (Parameter parameter in parameters)
			{
				if (flag)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					flag = true;
				}
				stringBuilder.Append(parameter.Name);
			}
			stringBuilder.Append(')');
			return stringBuilder.ToString();
		}
	}

	internal override bool CanThrow => false;

	public FunctionDefinition(string name, Parameter[] parameters)
		: this(name, parameters, (Statement)null)
	{
	}

	public FunctionDefinition(string name, Parameter[] parameters, Statement body)
	{
		ContractUtils.RequiresNotNullItems(parameters, "parameters");
		if (name == null)
		{
			_name = "<lambda$" + Interlocked.Increment(ref _lambdaId) + ">";
			_isLambda = true;
		}
		else
		{
			_name = name;
		}
		_parameters = parameters;
		_body = body;
	}

	[Obsolete("sourceUnit is now ignored.  FunctionDefinitions should belong to a PythonAst which has a SourceUnit")]
	public FunctionDefinition(string name, Parameter[] parameters, SourceUnit sourceUnit)
		: this(name, parameters, (Statement)null)
	{
	}

	[Obsolete("sourceUnit is now ignored.  FunctionDefinitions should belong to a PythonAst which has a SourceUnit")]
	public FunctionDefinition(string name, Parameter[] parameters, Statement body, SourceUnit sourceUnit)
		: this(name, parameters, body)
	{
	}

	internal override bool ExposesLocalVariable(PythonVariable variable)
	{
		return base.NeedsLocalsDictionary;
	}

	internal override bool TryBindOuter(ScopeStatement from, PythonReference reference, out PythonVariable variable)
	{
		base.ContainsNestedFreeVariables = true;
		if (TryGetVariable(reference.Name, out variable))
		{
			variable.AccessedInNestedScope = true;
			if (variable.Kind == VariableKind.Local || variable.Kind == VariableKind.Parameter)
			{
				from.AddFreeVariable(variable, accessedInScope: true);
				for (ScopeStatement parent = from.Parent; parent != this; parent = parent.Parent)
				{
					parent.AddFreeVariable(variable, accessedInScope: false);
				}
				AddCellVariable(variable);
			}
			else
			{
				from.AddReferencedGlobal(reference.Name);
			}
			return true;
		}
		return false;
	}

	internal override PythonVariable BindReference(PythonNameBinder binder, PythonReference reference)
	{
		if (TryGetVariable(reference.Name, out var variable))
		{
			if (variable.Kind == VariableKind.Global)
			{
				AddReferencedGlobal(reference.Name);
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

	internal override void Bind(PythonNameBinder binder)
	{
		base.Bind(binder);
		Verify(binder);
		if (((PythonContext)binder.Context.SourceUnit.LanguageContext).PythonOptions.FullFrames)
		{
			base.NeedsLocalsDictionary = true;
		}
	}

	internal override void FinishBind(PythonNameBinder binder)
	{
		Parameter[] parameters = _parameters;
		foreach (Parameter parameter in parameters)
		{
			_variableMapping[parameter.PythonVariable] = parameter.FinishBind(base.NeedsLocalsDictionary);
		}
		base.FinishBind(binder);
	}

	private void Verify(PythonNameBinder binder)
	{
		if (base.ContainsImportStar && base.IsClosure)
		{
			binder.ReportSyntaxError(string.Format(CultureInfo.InvariantCulture, "import * is not allowed in function '{0}' because it is a nested function", new object[1] { Name }), this);
		}
		if (base.ContainsImportStar && base.Parent is FunctionDefinition)
		{
			binder.ReportSyntaxError(string.Format(CultureInfo.InvariantCulture, "import * is not allowed in function '{0}' because it is a nested function", new object[1] { Name }), this);
		}
		if (base.ContainsImportStar && base.ContainsNestedFreeVariables)
		{
			binder.ReportSyntaxError(string.Format(CultureInfo.InvariantCulture, "import * is not allowed in function '{0}' because it contains a nested function with free variables", new object[1] { Name }), this);
		}
		if (base.ContainsUnqualifiedExec && base.ContainsNestedFreeVariables)
		{
			binder.ReportSyntaxError(string.Format(CultureInfo.InvariantCulture, "unqualified exec is not allowed in function '{0}' because it contains a nested function with free variables", new object[1] { Name }), this);
		}
		if (base.ContainsUnqualifiedExec && base.IsClosure)
		{
			binder.ReportSyntaxError(string.Format(CultureInfo.InvariantCulture, "unqualified exec is not allowed in function '{0}' because it is a nested function", new object[1] { Name }), this);
		}
	}

	internal override System.Linq.Expressions.Expression GetParentClosureTuple()
	{
		return _GetClosureTupleFromFunctionCall;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		System.Linq.Expressions.Expression value = MakeFunctionExpression();
		return base.GlobalParent.AddDebugInfoAndVoid(Node.AssignValue(base.Parent.GetVariableExpression(_variable), value), new SourceSpan(base.GlobalParent.IndexToLocation(base.StartIndex), base.GlobalParent.IndexToLocation(HeaderIndex)));
	}

	internal System.Linq.Expressions.Expression MakeFunctionExpression()
	{
		List<System.Linq.Expressions.Expression> list = new List<System.Linq.Expressions.Expression>(0);
		Parameter[] parameters = _parameters;
		foreach (Parameter parameter in parameters)
		{
			if (parameter.DefaultValue != null)
			{
				list.Add(Utils.Convert(parameter.DefaultValue, typeof(object)));
			}
		}
		System.Linq.Expressions.Expression funcCodeExpr = base.GlobalParent.Constant(GetOrMakeFunctionCode());
		base.FuncCodeExpr = funcCodeExpr;
		System.Linq.Expressions.Expression ret;
		if (EmitDebugFunction())
		{
			LightLambdaExpression lightLambdaExpression = CreateFunctionLambda();
			ret = System.Linq.Expressions.Expression.Call(AstMethods.MakeFunctionDebug, base.Parent.LocalContext, base.FuncCodeExpr, ((IPythonGlobalExpression)GetVariableExpression(_nameVariable)).RawValue(), (list.Count == 0) ? ((System.Linq.Expressions.Expression)Utils.Constant(null, typeof(object[]))) : ((System.Linq.Expressions.Expression)System.Linq.Expressions.Expression.NewArrayInit(typeof(object), list)), IsGenerator ? ((System.Linq.Expressions.Expression)new PythonGeneratorExpression(lightLambdaExpression, base.GlobalParent.PyContext.Options.CompilationThreshold)) : ((System.Linq.Expressions.Expression)lightLambdaExpression));
		}
		else
		{
			ret = System.Linq.Expressions.Expression.Call(AstMethods.MakeFunction, base.Parent.LocalContext, base.FuncCodeExpr, ((IPythonGlobalExpression)GetVariableExpression(_nameVariable)).RawValue(), (list.Count == 0) ? ((System.Linq.Expressions.Expression)Utils.Constant(null, typeof(object[]))) : ((System.Linq.Expressions.Expression)System.Linq.Expressions.Expression.NewArrayInit(typeof(object), list)));
		}
		return AddDecorators(ret, _decorators);
	}

	void IInstructionProvider.AddInstructions(LightCompiler compiler)
	{
		if (_decorators != null)
		{
			compiler.Compile(Reduce());
			return;
		}
		System.Linq.Expressions.Expression funcCodeExpr = base.GlobalParent.Constant(GetOrMakeFunctionCode());
		base.FuncCodeExpr = funcCodeExpr;
		System.Linq.Expressions.Expression variableExpression = base.Parent.GetVariableExpression(_variable);
		CompileAssignment(compiler, variableExpression, CreateFunctionInstructions);
	}

	private void CreateFunctionInstructions(LightCompiler compiler)
	{
		CodeContext context = null;
		compiler.Compile(base.Parent.LocalContext);
		PythonGlobalVariableExpression pythonGlobalVariableExpression = GetVariableExpression(_nameVariable) as PythonGlobalVariableExpression;
		PythonGlobal name = null;
		if (pythonGlobalVariableExpression == null)
		{
			compiler.Compile(((IPythonGlobalExpression)GetVariableExpression(_nameVariable)).RawValue());
		}
		else
		{
			name = pythonGlobalVariableExpression.Global;
		}
		int num = 0;
		for (int num2 = _parameters.Length - 1; num2 >= 0; num2--)
		{
			Parameter parameter = _parameters[num2];
			if (parameter.DefaultValue != null)
			{
				compiler.Compile(Utils.Convert(parameter.DefaultValue, typeof(object)));
				num++;
			}
		}
		compiler.Instructions.Emit(new FunctionDefinitionInstruction(context, this, num, name));
	}

	private static void CompileAssignment(LightCompiler compiler, System.Linq.Expressions.Expression variable, Action<LightCompiler> compileValue)
	{
		InstructionList instructions = compiler.Instructions;
		ClosureExpression closureExpression = variable as ClosureExpression;
		if (closureExpression != null)
		{
			compiler.Compile(closureExpression.ClosureCell);
		}
		LookupGlobalVariable lookupGlobalVariable = variable as LookupGlobalVariable;
		if (lookupGlobalVariable != null)
		{
			compiler.Compile(lookupGlobalVariable.CodeContext);
			instructions.EmitLoad(lookupGlobalVariable.Name);
		}
		compileValue(compiler);
		if (closureExpression != null)
		{
			instructions.EmitStoreField(ClosureExpression._cellField);
		}
		else if (lookupGlobalVariable != null)
		{
			MethodInfo method = typeof(PythonOps).GetMethod(lookupGlobalVariable.IsLocal ? "SetLocal" : "SetGlobal");
			instructions.Emit(CallInstruction.Create(method));
		}
		else if (variable is ParameterExpression var)
		{
			instructions.EmitStoreLocal(compiler.Locals.GetLocalIndex(var));
		}
		else if (variable is PythonGlobalVariableExpression pythonGlobalVariableExpression)
		{
			instructions.Emit(new PythonSetGlobalInstruction(pythonGlobalVariableExpression.Global));
			instructions.EmitPop();
		}
	}

	private LightLambdaExpression EnsureFunctionLambda()
	{
		if (_dlrBody == null)
		{
			_dlrBody = CreateFunctionLambda();
		}
		return _dlrBody;
	}

	private LightLambdaExpression CreateFunctionLambda()
	{
		bool flag = _parameters.Length > 15;
		Delegate originalTarget;
		Type delegateType = GetDelegateType(_parameters, flag, out originalTarget);
		ParameterExpression parameterExpression = null;
		ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>();
		if (base.NeedsLocalContext)
		{
			parameterExpression = ScopeStatement.LocalCodeContextVariable;
			readOnlyCollectionBuilder.Add(parameterExpression);
		}
		ParameterExpression[] array = CreateParameters(flag, readOnlyCollectionBuilder);
		List<System.Linq.Expressions.Expression> list = new List<System.Linq.Expressions.Expression>();
		Parameter[] parameters = _parameters;
		foreach (Parameter parameter in parameters)
		{
			if (GetVariableExpression(parameter.PythonVariable) is IPythonVariableExpression pythonVariableExpression)
			{
				System.Linq.Expressions.Expression expression = pythonVariableExpression.Create();
				if (expression != null)
				{
					list.Add(expression);
				}
			}
		}
		list.Add(System.Linq.Expressions.Expression.ClearDebugInfo(base.GlobalParent.Document));
		readOnlyCollectionBuilder.Add(PythonAst._globalContext);
		list.Add(System.Linq.Expressions.Expression.Assign(PythonAst._globalContext, new GetGlobalContextExpression(_parentContext)));
		base.GlobalParent.PrepareScope(readOnlyCollectionBuilder, list);
		CreateFunctionVariables(readOnlyCollectionBuilder, list);
		InitializeParameters(list, flag, array);
		List<System.Linq.Expressions.Expression> list2 = new List<System.Linq.Expressions.Expression>();
		SourceLocation sourceLocation = base.GlobalParent.IndexToLocation(base.StartIndex);
		list2.Add(base.GlobalParent.AddDebugInfo(Utils.Empty(), new SourceSpan(new SourceLocation(0, sourceLocation.Line, sourceLocation.Column), new SourceLocation(0, sourceLocation.Line, int.MaxValue))));
		if (IsGenerator)
		{
			System.Linq.Expressions.Expression item = YieldExpression.CreateCheckThrowExpression(SourceSpan.None);
			list2.Add(item);
		}
		ParameterExpression parameterExpression2 = null;
		if (!IsGenerator && _canSetSysExcInfo)
		{
			parameterExpression2 = System.Linq.Expressions.Expression.Parameter(typeof(Exception), "$ex");
			readOnlyCollectionBuilder.Add(parameterExpression2);
		}
		if (_body.CanThrow && !(_body is SuiteStatement) && _body.StartIndex != -1)
		{
			list2.Add(Node.UpdateLineNumber(base.GlobalParent.IndexToLocation(_body.StartIndex).Line));
		}
		list2.Add(Body);
		System.Linq.Expressions.Expression expression2 = System.Linq.Expressions.Expression.Block(list2);
		if (parameterExpression2 != null)
		{
			System.Linq.Expressions.Expression expression3 = Utils.Try(System.Linq.Expressions.Expression.Assign(parameterExpression2, System.Linq.Expressions.Expression.Call(AstMethods.SaveCurrentException)), expression2).Finally(System.Linq.Expressions.Expression.Call(AstMethods.RestoreCurrentException, parameterExpression2));
			expression2 = expression3;
		}
		if (_body.CanThrow && base.GlobalParent.PyContext.PythonOptions.Frames)
		{
			expression2 = Node.AddFrame(LocalContext, System.Linq.Expressions.Expression.Property(_functionParam, typeof(PythonFunction).GetProperty("__code__")), expression2);
			readOnlyCollectionBuilder.Add(Node.FunctionStackVariable);
		}
		expression2 = AddProfiling(expression2);
		expression2 = WrapScopeStatements(expression2, _body.CanThrow);
		expression2 = System.Linq.Expressions.Expression.Block(expression2, Utils.Empty());
		expression2 = AddReturnTarget(expression2);
		System.Linq.Expressions.Expression item2 = expression2;
		if (parameterExpression != null)
		{
			MethodCallExpression right = CreateLocalContext(_parentContext);
			list.Add(System.Linq.Expressions.Expression.Assign(parameterExpression, right));
		}
		list.Add(item2);
		item2 = System.Linq.Expressions.Expression.Block(list);
		item2 = System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder.ToReadOnlyCollection(), item2);
		return Utils.LightLambda(typeof(object), delegateType, AddDefaultReturn(item2, typeof(object)), Name + "$" + Interlocked.Increment(ref _lambdaId), array);
	}

	internal override LightLambdaExpression GetLambda()
	{
		return EnsureFunctionLambda();
	}

	private static System.Linq.Expressions.Expression AddDefaultReturn(System.Linq.Expressions.Expression body, Type returnType)
	{
		if (body.Type == typeof(void) && returnType != typeof(void))
		{
			body = System.Linq.Expressions.Expression.Block(body, System.Linq.Expressions.Expression.Default(returnType));
		}
		return body;
	}

	private ParameterExpression[] CreateParameters(bool needsWrapperMethod, ReadOnlyCollectionBuilder<ParameterExpression> locals)
	{
		ParameterExpression[] array;
		if (needsWrapperMethod)
		{
			array = new ParameterExpression[2]
			{
				_functionParam,
				System.Linq.Expressions.Expression.Parameter(typeof(object[]), "allArgs")
			};
			Parameter[] parameters = _parameters;
			foreach (Parameter parameter in parameters)
			{
				locals.Add(parameter.ParameterExpression);
			}
		}
		else
		{
			array = new ParameterExpression[_parameters.Length + 1];
			for (int j = 1; j < array.Length; j++)
			{
				array[j] = _parameters[j - 1].ParameterExpression;
			}
			array[0] = _functionParam;
		}
		return array;
	}

	internal void CreateFunctionVariables(ReadOnlyCollectionBuilder<ParameterExpression> locals, List<System.Linq.Expressions.Expression> init)
	{
		CreateVariables(locals, init);
	}

	internal System.Linq.Expressions.Expression AddReturnTarget(System.Linq.Expressions.Expression expression)
	{
		if (_hasReturn)
		{
			return System.Linq.Expressions.Expression.Label(_returnLabel, Utils.Convert(expression, typeof(object)));
		}
		return expression;
	}

	private bool EmitDebugFunction()
	{
		if (base.EmitDebugSymbols)
		{
			return !base.GlobalParent.PyContext.EnableTracing;
		}
		return false;
	}

	internal override IList<string> GetVarNames()
	{
		List<string> list = new List<string>();
		Parameter[] parameters = _parameters;
		foreach (Parameter parameter in parameters)
		{
			list.Add(parameter.Name);
		}
		AppendVariables(list);
		return list;
	}

	private void InitializeParameters(List<System.Linq.Expressions.Expression> init, bool needsWrapperMethod, System.Linq.Expressions.Expression[] parameters)
	{
		for (int i = 0; i < _parameters.Length; i++)
		{
			Parameter parameter = _parameters[i];
			if (needsWrapperMethod)
			{
				init.Add(Node.AssignValue(GetVariableExpression(parameter.PythonVariable), System.Linq.Expressions.Expression.ArrayIndex(parameters[1], System.Linq.Expressions.Expression.Constant(i))));
			}
			parameter.Init(init);
		}
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_parameters != null)
			{
				Parameter[] parameters = _parameters;
				foreach (Parameter parameter in parameters)
				{
					parameter.Walk(walker);
				}
			}
			if (_decorators != null)
			{
				foreach (Expression decorator in _decorators)
				{
					decorator.Walk(walker);
				}
			}
			if (_body != null)
			{
				_body.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}

	private static Type GetDelegateType(Parameter[] parameters, bool wrapper, out Delegate originalTarget)
	{
		return PythonCallTargets.GetPythonTargetType(wrapper, parameters.Length, out originalTarget);
	}

	internal override void RewriteBody(PythonAst.LookupVisitor visitor)
	{
		_dlrBody = null;
		System.Linq.Expressions.Expression funcCodeExpr = base.GlobalParent.Constant(GetOrMakeFunctionCode());
		base.FuncCodeExpr = funcCodeExpr;
		Body = new PythonAst.RewrittenBodyStatement(Body, visitor.Visit(Body));
	}
}
