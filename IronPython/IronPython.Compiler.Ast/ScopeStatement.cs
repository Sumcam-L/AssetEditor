using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public abstract class ScopeStatement : Statement
{
	private class DelayedFunctionCode : System.Linq.Expressions.Expression
	{
		private System.Linq.Expressions.Expression _funcCode;

		public override bool CanReduce => true;

		public System.Linq.Expressions.Expression Code
		{
			get
			{
				return _funcCode;
			}
			set
			{
				_funcCode = value;
			}
		}

		public override Type Type => typeof(FunctionCode);

		public override ExpressionType NodeType => ExpressionType.Extension;

		protected override System.Linq.Expressions.Expression VisitChildren(ExpressionVisitor visitor)
		{
			if (_funcCode != null)
			{
				System.Linq.Expressions.Expression expression = visitor.Visit(_funcCode);
				if (expression != _funcCode)
				{
					DelayedFunctionCode delayedFunctionCode = new DelayedFunctionCode();
					delayedFunctionCode._funcCode = expression;
					return delayedFunctionCode;
				}
			}
			return this;
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			return _funcCode;
		}
	}

	private struct ClosureInfo
	{
		public PythonVariable Variable;

		public bool AccessedInScope;

		public ClosureInfo(PythonVariable variable, bool accessedInScope)
		{
			Variable = variable;
			AccessedInScope = accessedInScope;
		}
	}

	private class DelayedProfiling : System.Linq.Expressions.Expression
	{
		private readonly ScopeStatement _ast;

		private readonly System.Linq.Expressions.Expression _body;

		private readonly ParameterExpression _tick;

		public override bool CanReduce => true;

		public override Type Type => _body.Type;

		public override ExpressionType NodeType => ExpressionType.Extension;

		public DelayedProfiling(ScopeStatement ast, System.Linq.Expressions.Expression body, ParameterExpression tick)
		{
			_ast = ast;
			_body = body;
			_tick = tick;
		}

		protected override System.Linq.Expressions.Expression VisitChildren(ExpressionVisitor visitor)
		{
			return visitor.Visit(_body);
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			string profilerName = _ast.ProfilerName;
			bool unique = profilerName == "module: <exec>";
			return System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { _tick }, _ast.GlobalParent._profiler.AddProfiling(_body, _tick, profilerName, unique));
		}
	}

	internal const string NameForExec = "module: <exec>";

	private bool _importStar;

	private bool _unqualifiedExec;

	private bool _nestedFreeVariables;

	private bool _locals;

	private bool _hasLateboundVarSets;

	private bool _containsExceptionHandling;

	private bool _forceCompile;

	private FunctionCode _funcCode;

	private Dictionary<string, PythonVariable> _variables;

	private ClosureInfo[] _closureVariables;

	private List<PythonVariable> _freeVars;

	private List<string> _globalVars;

	private List<string> _cellVars;

	private Dictionary<string, PythonReference> _references;

	internal Dictionary<PythonVariable, System.Linq.Expressions.Expression> _variableMapping = new Dictionary<PythonVariable, System.Linq.Expressions.Expression>();

	private ParameterExpression _localParentTuple;

	private readonly DelayedFunctionCode _funcCodeExpr = new DelayedFunctionCode();

	internal static ParameterExpression LocalCodeContextVariable = System.Linq.Expressions.Expression.Parameter(typeof(CodeContext), "$localContext");

	private static ParameterExpression _catchException = System.Linq.Expressions.Expression.Parameter(typeof(Exception), "$updException");

	internal bool ContainsImportStar
	{
		get
		{
			return _importStar;
		}
		set
		{
			_importStar = value;
		}
	}

	internal bool ContainsExceptionHandling
	{
		get
		{
			return _containsExceptionHandling;
		}
		set
		{
			_containsExceptionHandling = value;
		}
	}

	internal bool ContainsUnqualifiedExec
	{
		get
		{
			return _unqualifiedExec;
		}
		set
		{
			_unqualifiedExec = value;
		}
	}

	internal virtual bool IsGeneratorMethod => false;

	internal ParameterExpression LocalParentTuple => _localParentTuple;

	internal virtual System.Linq.Expressions.Expression LocalContext => LocalCodeContextVariable;

	internal bool IsClosure
	{
		get
		{
			if (FreeVariables != null)
			{
				return FreeVariables.Count > 0;
			}
			return false;
		}
	}

	internal bool ContainsNestedFreeVariables
	{
		get
		{
			return _nestedFreeVariables;
		}
		set
		{
			_nestedFreeVariables = value;
		}
	}

	internal bool NeedsLocalsDictionary
	{
		get
		{
			return _locals;
		}
		set
		{
			_locals = value;
		}
	}

	public virtual string Name => "<unknown>";

	internal virtual string Filename => base.GlobalParent.SourceUnit.Path ?? "<string>";

	internal virtual bool HasLateBoundVariableSets
	{
		get
		{
			return _hasLateboundVarSets;
		}
		set
		{
			_hasLateboundVarSets = value;
		}
	}

	internal Dictionary<string, PythonVariable> Variables => _variables;

	internal virtual bool IsGlobal => false;

	internal bool NeedsLocalContext
	{
		get
		{
			if (!NeedsLocalsDictionary)
			{
				return ContainsNestedFreeVariables;
			}
			return true;
		}
	}

	internal virtual string[] ParameterNames => ArrayUtils.EmptyStrings;

	internal virtual int ArgCount => 0;

	internal virtual FunctionAttributes Flags => FunctionAttributes.None;

	internal virtual string ScopeDocumentation => null;

	internal virtual Delegate OriginalDelegate => null;

	internal bool ShouldInterpret
	{
		get
		{
			if (_forceCompile)
			{
				return false;
			}
			if (base.GlobalParent.CompilationMode == CompilationMode.Lookup)
			{
				return true;
			}
			CompilerContext compilerContext = base.GlobalParent.CompilerContext;
			return ((PythonContext)compilerContext.SourceUnit.LanguageContext).ShouldInterpret((PythonCompilerOptions)compilerContext.Options, compilerContext.SourceUnit);
		}
		set
		{
			_forceCompile = !value;
		}
	}

	internal IList<PythonVariable> FreeVariables => _freeVars;

	internal IList<string> GlobalVariables => _globalVars;

	internal IList<string> CellVariables => _cellVars;

	internal virtual int TupleCells
	{
		get
		{
			if (_closureVariables == null)
			{
				return 0;
			}
			return _closureVariables.Length;
		}
	}

	internal PythonContext PyContext => (PythonContext)base.GlobalParent.CompilerContext.SourceUnit.LanguageContext;

	private SymbolDocumentInfo Document => base.GlobalParent.Document;

	internal System.Linq.Expressions.Expression FuncCodeExpr
	{
		get
		{
			return _funcCodeExpr.Code;
		}
		set
		{
			_funcCodeExpr.Code = value;
		}
	}

	internal virtual bool PrintExpressions => false;

	internal virtual string ProfilerName => Name;

	internal abstract LightLambdaExpression GetLambda();

	internal FunctionCode GetOrMakeFunctionCode()
	{
		if (_funcCode == null)
		{
			Interlocked.CompareExchange(ref _funcCode, new FunctionCode(base.GlobalParent.PyContext, OriginalDelegate, this, ScopeDocumentation, null, register: true), null);
		}
		return _funcCode;
	}

	internal virtual IList<string> GetVarNames()
	{
		List<string> list = new List<string>();
		AppendVariables(list);
		return list;
	}

	internal void AddFreeVariable(PythonVariable variable, bool accessedInScope)
	{
		if (_freeVars == null)
		{
			_freeVars = new List<PythonVariable>();
		}
		if (!_freeVars.Contains(variable))
		{
			_freeVars.Add(variable);
		}
	}

	internal string AddReferencedGlobal(string name)
	{
		if (_globalVars == null)
		{
			_globalVars = new List<string>();
		}
		if (!_globalVars.Contains(name))
		{
			_globalVars.Add(name);
		}
		return name;
	}

	internal void AddCellVariable(PythonVariable variable)
	{
		if (_cellVars == null)
		{
			_cellVars = new List<string>();
		}
		if (!_cellVars.Contains(variable.Name))
		{
			_cellVars.Add(variable.Name);
		}
	}

	internal List<string> AppendVariables(List<string> res)
	{
		if (Variables != null)
		{
			foreach (KeyValuePair<string, PythonVariable> variable in Variables)
			{
				if (variable.Value.Kind == VariableKind.Local && (CellVariables == null || !CellVariables.Contains(variable.Key)))
				{
					res.Add(variable.Key);
				}
			}
		}
		return res;
	}

	internal Type GetClosureTupleType()
	{
		if (TupleCells > 0)
		{
			Type[] array = new Type[TupleCells];
			for (int i = 0; i < TupleCells; i++)
			{
				array[i] = typeof(ClosureCell);
			}
			return MutableTuple.MakeTupleType(array);
		}
		return null;
	}

	internal abstract bool ExposesLocalVariable(PythonVariable variable);

	internal virtual System.Linq.Expressions.Expression GetParentClosureTuple()
	{
		throw new NotSupportedException();
	}

	private bool TryGetAnyVariable(string name, out PythonVariable variable)
	{
		if (_variables != null)
		{
			return _variables.TryGetValue(name, out variable);
		}
		variable = null;
		return false;
	}

	internal bool TryGetVariable(string name, out PythonVariable variable)
	{
		if (TryGetAnyVariable(name, out variable))
		{
			return true;
		}
		variable = null;
		return false;
	}

	internal virtual bool TryBindOuter(ScopeStatement from, PythonReference reference, out PythonVariable variable)
	{
		variable = null;
		return false;
	}

	internal abstract PythonVariable BindReference(PythonNameBinder binder, PythonReference reference);

	internal virtual void Bind(PythonNameBinder binder)
	{
		if (_references == null)
		{
			return;
		}
		foreach (PythonReference value in _references.Values)
		{
			PythonVariable pythonVariable = (value.PythonVariable = BindReference(binder, value));
			if (pythonVariable != null && pythonVariable.Deleted && pythonVariable.Scope != this && !pythonVariable.Scope.IsGlobal)
			{
				binder.ReportSyntaxError(string.Format(CultureInfo.InvariantCulture, "can not delete variable '{0}' referenced in nested scope", new object[1] { value.Name }), this);
			}
		}
	}

	internal virtual void FinishBind(PythonNameBinder binder)
	{
		List<ClosureInfo> list = null;
		if (FreeVariables != null && FreeVariables.Count > 0)
		{
			_localParentTuple = System.Linq.Expressions.Expression.Parameter(base.Parent.GetClosureTupleType(), "$tuple");
			foreach (PythonVariable freeVar in _freeVars)
			{
				ClosureInfo[] closureVariables = base.Parent._closureVariables;
				for (int i = 0; i < closureVariables.Length; i++)
				{
					if (closureVariables[i].Variable == freeVar)
					{
						_variableMapping[freeVar] = new ClosureExpression(freeVar, System.Linq.Expressions.Expression.Property(_localParentTuple, $"Item{i:D3}"), null);
						break;
					}
				}
				if (list == null)
				{
					list = new List<ClosureInfo>();
				}
				list.Add(new ClosureInfo(freeVar, !(this is ClassDefinition)));
			}
		}
		if (Variables != null)
		{
			foreach (PythonVariable value in Variables.Values)
			{
				if (!HasClosureVariable(list, value) && !value.IsGlobal && (value.AccessedInNestedScope || ExposesLocalVariable(value)))
				{
					if (list == null)
					{
						list = new List<ClosureInfo>();
					}
					list.Add(new ClosureInfo(value, accessedInScope: true));
				}
				if (value.Kind == VariableKind.Local)
				{
					if (value.AccessedInNestedScope || ExposesLocalVariable(value))
					{
						_variableMapping[value] = new ClosureExpression(value, System.Linq.Expressions.Expression.Parameter(typeof(ClosureCell), value.Name), null);
					}
					else
					{
						_variableMapping[value] = System.Linq.Expressions.Expression.Parameter(typeof(object), value.Name);
					}
				}
			}
		}
		if (list != null)
		{
			_closureVariables = list.ToArray();
		}
		_references = null;
	}

	private static bool HasClosureVariable(List<ClosureInfo> closureVariables, PythonVariable variable)
	{
		if (closureVariables == null)
		{
			return false;
		}
		for (int i = 0; i < closureVariables.Count; i++)
		{
			if (closureVariables[i].Variable == variable)
			{
				return true;
			}
		}
		return false;
	}

	private void EnsureVariables()
	{
		if (_variables == null)
		{
			_variables = new Dictionary<string, PythonVariable>(StringComparer.Ordinal);
		}
	}

	internal void AddGlobalVariable(PythonVariable variable)
	{
		EnsureVariables();
		_variables[variable.Name] = variable;
	}

	internal PythonReference Reference(string name)
	{
		if (_references == null)
		{
			_references = new Dictionary<string, PythonReference>(StringComparer.Ordinal);
		}
		if (!_references.TryGetValue(name, out var value))
		{
			value = (_references[name] = new PythonReference(name));
		}
		return value;
	}

	internal bool IsReferenced(string name)
	{
		PythonReference value;
		if (_references != null)
		{
			return _references.TryGetValue(name, out value);
		}
		return false;
	}

	internal PythonVariable CreateVariable(string name, VariableKind kind)
	{
		EnsureVariables();
		return _variables[name] = new PythonVariable(name, kind, this);
	}

	internal PythonVariable EnsureVariable(string name)
	{
		if (!TryGetVariable(name, out var variable))
		{
			return CreateVariable(name, VariableKind.Local);
		}
		return variable;
	}

	internal PythonVariable DefineParameter(string name)
	{
		return CreateVariable(name, VariableKind.Parameter);
	}

	internal System.Linq.Expressions.Expression AddDebugInfo(System.Linq.Expressions.Expression expression, SourceLocation start, SourceLocation end)
	{
		if (PyContext.PythonOptions.GCStress.HasValue)
		{
			expression = System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Call(typeof(GC).GetMethod("Collect", new Type[1] { typeof(int) }), System.Linq.Expressions.Expression.Constant(PyContext.PythonOptions.GCStress.Value)), expression);
		}
		return Utils.AddDebugInfo(expression, Document, start, end);
	}

	internal System.Linq.Expressions.Expression AddDebugInfo(System.Linq.Expressions.Expression expression, SourceSpan location)
	{
		return AddDebugInfo(expression, location.Start, location.End);
	}

	internal System.Linq.Expressions.Expression AddDebugInfoAndVoid(System.Linq.Expressions.Expression expression, SourceSpan location)
	{
		if (expression.Type != typeof(void))
		{
			expression = Utils.Void(expression);
		}
		return AddDebugInfo(expression, location);
	}

	internal System.Linq.Expressions.Expression GetUpdateTrackbackExpression(ParameterExpression exception)
	{
		if (!_containsExceptionHandling)
		{
			return UpdateStackTrace(exception);
		}
		return GetSaveLineNumberExpression(exception, preventAdditionalAdds: true);
	}

	private System.Linq.Expressions.Expression UpdateStackTrace(ParameterExpression exception)
	{
		return System.Linq.Expressions.Expression.Call(AstMethods.UpdateStackTrace, exception, LocalContext, _funcCodeExpr, Node.LineNumberExpression);
	}

	internal System.Linq.Expressions.Expression GetSaveLineNumberExpression(ParameterExpression exception, bool preventAdditionalAdds)
	{
		return System.Linq.Expressions.Expression.Block(Utils.If(System.Linq.Expressions.Expression.Not(Node.LineNumberUpdated), UpdateStackTrace(exception)), System.Linq.Expressions.Expression.Assign(Node.LineNumberUpdated, Utils.Constant(preventAdditionalAdds)), Utils.Empty());
	}

	internal System.Linq.Expressions.Expression WrapScopeStatements(System.Linq.Expressions.Expression body, bool canThrow)
	{
		if (canThrow)
		{
			body = System.Linq.Expressions.Expression.Block(new ParameterExpression[2]
			{
				Node.LineNumberExpression,
				Node.LineNumberUpdated
			}, System.Linq.Expressions.Expression.TryCatch(body, System.Linq.Expressions.Expression.Catch(_catchException, System.Linq.Expressions.Expression.Block(GetUpdateTrackbackExpression(_catchException), System.Linq.Expressions.Expression.Rethrow(body.Type)))));
		}
		return body;
	}

	internal MethodCallExpression CreateLocalContext(System.Linq.Expressions.Expression parentContext)
	{
		ClosureInfo[] input = _closureVariables;
		if (_closureVariables == null)
		{
			input = new ClosureInfo[0];
		}
		return System.Linq.Expressions.Expression.Call(AstMethods.CreateLocalContext, parentContext, MutableTuple.Create(ArrayUtils.ConvertAll(input, (ClosureInfo x) => GetClosureCell(x))), System.Linq.Expressions.Expression.Constant(ArrayUtils.ConvertAll(input, (ClosureInfo x) => (!x.AccessedInScope) ? null : x.Variable.Name)));
	}

	private System.Linq.Expressions.Expression GetClosureCell(ClosureInfo variable)
	{
		return ((ClosureExpression)GetVariableExpression(variable.Variable)).ClosureCell;
	}

	internal virtual System.Linq.Expressions.Expression GetVariableExpression(PythonVariable variable)
	{
		if (variable.IsGlobal)
		{
			return base.GlobalParent.ModuleVariables[variable];
		}
		return _variableMapping[variable];
	}

	internal void CreateVariables(ReadOnlyCollectionBuilder<ParameterExpression> locals, List<System.Linq.Expressions.Expression> init)
	{
		if (Variables != null)
		{
			foreach (PythonVariable value in Variables.Values)
			{
				if (value.Kind == VariableKind.Global)
				{
					continue;
				}
				if (GetVariableExpression(value) is ClosureExpression closureExpression)
				{
					init.Add(closureExpression.Create());
					locals.Add((ParameterExpression)closureExpression.ClosureCell);
				}
				else if (value.Kind == VariableKind.Local)
				{
					locals.Add((ParameterExpression)GetVariableExpression(value));
					if (value.ReadBeforeInitialized)
					{
						init.Add(Node.AssignValue(GetVariableExpression(value), System.Linq.Expressions.Expression.Field(null, typeof(Uninitialized).GetField("Instance"))));
					}
				}
			}
		}
		if (IsClosure)
		{
			Type closureTupleType = base.Parent.GetClosureTupleType();
			init.Add(System.Linq.Expressions.Expression.Assign(LocalParentTuple, System.Linq.Expressions.Expression.Convert(GetParentClosureTuple(), closureTupleType)));
			locals.Add(LocalParentTuple);
		}
	}

	internal System.Linq.Expressions.Expression AddDecorators(System.Linq.Expressions.Expression ret, IList<Expression> decorators)
	{
		if (decorators != null)
		{
			for (int num = decorators.Count - 1; num >= 0; num--)
			{
				Expression expression = decorators[num];
				ret = base.Parent.Invoke(new CallSignature(1), base.Parent.LocalContext, expression, ret);
			}
		}
		return ret;
	}

	internal System.Linq.Expressions.Expression Invoke(CallSignature signature, params System.Linq.Expressions.Expression[] args)
	{
		PythonInvokeBinder binder = PyContext.Invoke(signature);
		return args.Length switch
		{
			1 => base.GlobalParent.CompilationMode.Dynamic(binder, typeof(object), args[0]), 
			2 => base.GlobalParent.CompilationMode.Dynamic(binder, typeof(object), args[0], args[1]), 
			3 => base.GlobalParent.CompilationMode.Dynamic(binder, typeof(object), args[0], args[1], args[2]), 
			4 => base.GlobalParent.CompilationMode.Dynamic(binder, typeof(object), args[0], args[1], args[2], args[3]), 
			_ => base.GlobalParent.CompilationMode.Dynamic(binder, typeof(object), args), 
		};
	}

	internal ScopeStatement CopyForRewrite()
	{
		return (ScopeStatement)MemberwiseClone();
	}

	internal virtual void RewriteBody(PythonAst.LookupVisitor visitor)
	{
		_funcCode = null;
	}

	internal System.Linq.Expressions.Expression AddProfiling(System.Linq.Expressions.Expression body)
	{
		if (base.GlobalParent._profiler != null)
		{
			ParameterExpression tick = System.Linq.Expressions.Expression.Variable(typeof(long), "$tick");
			return new DelayedProfiling(this, body, tick);
		}
		return body;
	}
}
