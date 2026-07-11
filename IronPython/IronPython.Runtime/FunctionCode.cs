using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Debugging.CompilerServices;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[PythonType("code")]
public class FunctionCode : IExpressionSerializable
{
	private class TargetUpdaterForCompilation
	{
		private readonly PythonContext _context;

		private readonly FunctionCode _code;

		public TargetUpdaterForCompilation(PythonContext context, FunctionCode code)
		{
			_code = code;
			_context = context;
		}

		public void SetCompiledTarget(object sender, LightLambdaCompileEventArgs e)
		{
			_code.SetTarget(_code.AddRecursionCheck(_context, _code._normalDelegate = e.Compiled));
		}

		public void SetCompiledTargetTracing(object sender, LightLambdaCompileEventArgs e)
		{
			_code.SetTarget(_code.AddRecursionCheck(_context, _code._tracingDelegate = e.Compiled));
		}
	}

	internal class CodeList
	{
		public readonly WeakReference Code;

		public CodeList Next;

		public CodeList()
		{
		}

		public CodeList(WeakReference code, CodeList next)
		{
			Code = code;
			Next = next;
		}
	}

	[PythonHidden]
	internal Delegate Target;

	[PythonHidden]
	internal Delegate LightThrowTarget;

	internal Delegate _normalDelegate;

	private ScopeStatement _lambda;

	internal readonly string _initialDoc;

	private readonly int _localCount;

	private readonly int _argCount;

	private bool _compilingLight;

	private int _exceptionCount;

	private System.Linq.Expressions.LambdaExpression _tracingLambda;

	internal Delegate _tracingDelegate;

	private static CodeList _CodeCreateAndUpdateDelegateLock = new CodeList();

	private bool IsOnDiskCode
	{
		get
		{
			if (_lambda is SerializedScopeStatement)
			{
				return true;
			}
			if (_lambda is PythonAst)
			{
				return ((PythonAst)_lambda).OnDiskProxy;
			}
			return false;
		}
	}

	public SourceSpan Span
	{
		[PythonHidden]
		get
		{
			return _lambda.Span;
		}
	}

	internal string[] ArgNames => _lambda.ParameterNames;

	internal FunctionAttributes Flags => _lambda.Flags;

	internal bool IsModule => _lambda is PythonAst;

	public PythonTuple co_varnames => SymbolListToTuple(_lambda.GetVarNames());

	public int co_argcount => _argCount;

	public PythonTuple co_cellvars => SymbolListToTuple((_lambda.CellVariables != null) ? ArrayUtils.ToArray(_lambda.CellVariables) : null);

	public object co_code => string.Empty;

	public PythonTuple co_consts
	{
		get
		{
			if (_initialDoc != null)
			{
				return PythonTuple.MakeTuple(_initialDoc, null);
			}
			object[] items = new object[1];
			return PythonTuple.MakeTuple(items);
		}
	}

	public string co_filename => _lambda.Filename;

	public int co_firstlineno => Span.Start.Line;

	public int co_flags => (int)Flags;

	public PythonTuple co_freevars => SymbolListToTuple((_lambda.FreeVariables != null) ? CollectionUtils.ConvertAll(_lambda.FreeVariables, (PythonVariable x) => x.Name) : null);

	public object co_lnotab
	{
		get
		{
			throw PythonOps.NotImplementedError("");
		}
	}

	public string co_name => _lambda.Name;

	public PythonTuple co_names => SymbolListToTuple(_lambda.GlobalVariables);

	public object co_nlocals => _localCount;

	public object co_stacksize
	{
		get
		{
			throw PythonOps.NotImplementedError("");
		}
	}

	internal LightLambdaExpression Code => _lambda.GetLambda();

	internal ScopeStatement PythonCode => _lambda;

	internal FunctionCode(PythonContext context, Delegate code, ScopeStatement scope, string documentation, int localCount)
	{
		_normalDelegate = code;
		_lambda = scope;
		_argCount = CalculateArgumentCount();
		_initialDoc = documentation;
		lock (_CodeCreateAndUpdateDelegateLock)
		{
			SetTarget(AddRecursionCheck(context, code));
		}
		RegisterFunctionCode(context);
	}

	internal FunctionCode(PythonContext context, Delegate initialDelegate, ScopeStatement scope, string documentation, bool? tracing, bool register)
	{
		_lambda = scope;
		Target = (LightThrowTarget = initialDelegate);
		_initialDoc = documentation;
		_localCount = ((scope.Variables != null) ? scope.Variables.Count : 0);
		_argCount = CalculateArgumentCount();
		if (tracing.HasValue)
		{
			if (tracing.Value)
			{
				_tracingDelegate = initialDelegate;
			}
			else
			{
				_normalDelegate = initialDelegate;
			}
		}
		if (register)
		{
			RegisterFunctionCode(context);
		}
	}

	private static PythonTuple SymbolListToTuple(IList<string> vars)
	{
		if (vars != null && vars.Count != 0)
		{
			object[] array = new object[vars.Count];
			for (int i = 0; i < vars.Count; i++)
			{
				array[i] = vars[i];
			}
			return PythonTuple.MakeTuple(array);
		}
		return PythonTuple.EMPTY;
	}

	private static PythonTuple StringArrayToTuple(string[] closureVars)
	{
		if (closureVars != null && closureVars.Length != 0)
		{
			return PythonTuple.MakeTuple(closureVars);
		}
		return PythonTuple.EMPTY;
	}

	private void RegisterFunctionCode(PythonContext context)
	{
		if (_lambda == null)
		{
			return;
		}
		WeakReference code = new WeakReference(this);
		lock (_CodeCreateAndUpdateDelegateLock)
		{
			CodeList allCodes;
			do
			{
				allCodes = context._allCodes;
			}
			while (Interlocked.CompareExchange(ref context._allCodes, new CodeList(code, allCodes), allCodes) != allCodes);
			if (context._codeCount++ == context._nextCodeCleanup)
			{
				CleanFunctionCodes(context, synchronous: false);
			}
		}
	}

	internal static void CleanFunctionCodes(PythonContext context, bool synchronous)
	{
		if (synchronous)
		{
			CodeCleanup(context);
		}
		else
		{
			ThreadPool.QueueUserWorkItem(CodeCleanup, context);
		}
	}

	internal void SetTarget(Delegate target)
	{
		Target = (LightThrowTarget = target);
	}

	internal void LightThrowCompile(CodeContext context)
	{
		if (++_exceptionCount <= 20 || _compilingLight || (object)Target != LightThrowTarget)
		{
			return;
		}
		_compilingLight = true;
		if (IsOnDiskCode)
		{
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			PythonContext languageContext = context.LanguageContext;
			bool enableTracing;
			lock (languageContext._codeUpdateLock)
			{
				enableTracing = context.LanguageContext.EnableTracing;
			}
			Delegate lightThrowTarget = ((!enableTracing) ? ((System.Linq.Expressions.LambdaExpression)LightExceptions.Rewrite(GetGeneratorOrNormalLambda().Reduce())).Compile() : ((System.Linq.Expressions.LambdaExpression)LightExceptions.Rewrite(GetGeneratorOrNormalLambdaTracing(languageContext).Reduce())).Compile());
			lock (languageContext._codeUpdateLock)
			{
				if (context.LanguageContext.EnableTracing == enableTracing)
				{
					LightThrowTarget = lightThrowTarget;
				}
			}
		});
	}

	private static IEnumerable<FunctionCode> GetAllCode(PythonContext context)
	{
		lock (_CodeCreateAndUpdateDelegateLock)
		{
			CodeList curCodeList = Interlocked.Exchange(ref context._allCodes, _CodeCreateAndUpdateDelegateLock);
			CodeList initialCode = curCodeList;
			try
			{
				while (curCodeList != null)
				{
					WeakReference codeRef = curCodeList.Code;
					FunctionCode target = (FunctionCode)codeRef.Target;
					if (target != null)
					{
						yield return target;
					}
					curCodeList = curCodeList.Next;
				}
			}
			finally
			{
				Interlocked.Exchange(ref context._allCodes, initialCode);
			}
		}
	}

	internal static void UpdateAllCode(PythonContext context)
	{
		foreach (FunctionCode item in GetAllCode(context))
		{
			item.UpdateDelegate(context, forceCreation: false);
		}
	}

	private static void CodeCleanup(object state)
	{
		PythonContext pythonContext = (PythonContext)state;
		lock (pythonContext._codeCleanupLock)
		{
			int num = 0;
			int num2 = 0;
			CodeList codeList = null;
			CodeList codeList2 = GetRootCodeNoUpdating(pythonContext);
			while (codeList2 != null)
			{
				if (!codeList2.Code.IsAlive)
				{
					if (codeList == null)
					{
						if (Interlocked.CompareExchange(ref pythonContext._allCodes, codeList2.Next, codeList2) != codeList2)
						{
							codeList2 = GetRootCodeNoUpdating(pythonContext);
							continue;
						}
						codeList2 = codeList2.Next;
						num++;
					}
					else
					{
						num++;
						codeList2 = (codeList.Next = codeList2.Next);
					}
				}
				else
				{
					num2++;
					codeList = codeList2;
					codeList2 = codeList2.Next;
				}
			}
			lock (_CodeCreateAndUpdateDelegateLock)
			{
				if (pythonContext._codeCount == 0)
				{
					pythonContext._nextCodeCleanup = 200;
					return;
				}
				double num3 = (double)num / (double)pythonContext._codeCount;
				double num4 = num3 / 0.5;
				int num5 = Interlocked.Add(ref pythonContext._codeCount, -num);
				int num6 = ((num4 != 0.0) ? (num5 + (int)((double)pythonContext._nextCodeCleanup / num4)) : (-1));
				if (num6 > 0)
				{
					pythonContext._nextCodeCleanup = num6;
				}
				else
				{
					pythonContext._nextCodeCleanup += 500;
				}
			}
		}
	}

	private static CodeList GetRootCodeNoUpdating(PythonContext context)
	{
		CodeList allCodes = context._allCodes;
		if (allCodes == _CodeCreateAndUpdateDelegateLock)
		{
			lock (_CodeCreateAndUpdateDelegateLock)
			{
				allCodes = context._allCodes;
			}
		}
		return allCodes;
	}

	private int CalculateArgumentCount()
	{
		int num = _lambda.ArgCount;
		FunctionAttributes flags = Flags;
		if ((flags & FunctionAttributes.ArgumentList) != FunctionAttributes.None)
		{
			num--;
		}
		if ((flags & FunctionAttributes.KeywordDictionary) != FunctionAttributes.None)
		{
			num--;
		}
		return num;
	}

	internal object Call(CodeContext context)
	{
		if (co_freevars != PythonTuple.EMPTY)
		{
			throw PythonOps.TypeError("cannot exec code object that contains free variables: {0}", co_freevars.__repr__(context));
		}
		if ((object)Target == null || (Target.GetMethod() != null && Target.GetMethod().DeclaringType == typeof(PythonCallTargets)))
		{
			UpdateDelegate(context.LanguageContext, forceCreation: true);
		}
		if (Target is Func<CodeContext, CodeContext> func)
		{
			return func(context);
		}
		if (Target is LookupCompilationDelegate lookupCompilationDelegate)
		{
			return lookupCompilationDelegate(context, this);
		}
		if (Target is Func<FunctionCode, object> func2)
		{
			return func2(this);
		}
		PythonFunction arg = new PythonFunction(context, this, null, ArrayUtils.EmptyObjects, new MutableTuple<object>());
		CallSite<Func<CallSite, CodeContext, PythonFunction, object>> functionCallSite = PythonContext.GetContext(context).FunctionCallSite;
		return functionCallSite.Target(functionCallSite, context, arg);
	}

	internal static FunctionCode FromSourceUnit(SourceUnit sourceUnit, PythonCompilerOptions options, bool register)
	{
		ScriptCode scriptCode = PythonContext.CompilePythonCode(sourceUnit, options, ThrowingErrorSink.Default);
		return ((RunnableScriptCode)scriptCode).GetFunctionCode(register);
	}

	private void ExpandArgsTuple(List<string> names, PythonTuple toExpand)
	{
		for (int i = 0; i < toExpand.__len__(); i++)
		{
			if (toExpand[i] is PythonTuple)
			{
				ExpandArgsTuple(names, toExpand[i] as PythonTuple);
			}
			else
			{
				names.Add(toExpand[i] as string);
			}
		}
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public int __cmp__(CodeContext context, [NotNull] FunctionCode other)
	{
		if (other == this)
		{
			return 0;
		}
		long num = IdDispenser.GetId(this) - IdDispenser.GetId(other);
		if (num <= 0)
		{
			return -1;
		}
		return 1;
	}

	[Python3Warning("code inequality comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator >(FunctionCode self, FunctionCode other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("code inequality comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator <(FunctionCode self, FunctionCode other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("code inequality comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator >=(FunctionCode self, FunctionCode other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("code inequality comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator <=(FunctionCode self, FunctionCode other)
	{
		return PythonOps.NotImplemented;
	}

	internal void LazyCompileFirstTarget(PythonFunction function)
	{
		lock (_CodeCreateAndUpdateDelegateLock)
		{
			UpdateDelegate(PythonContext.GetContext(function.Context), forceCreation: true);
		}
	}

	internal void UpdateDelegate(PythonContext context, bool forceCreation)
	{
		Delegate finalTarget;
		if (context.EnableTracing && _lambda != null)
		{
			if (_tracingLambda == null)
			{
				if (!forceCreation)
				{
					PythonCallTargets.GetPythonTargetType(_lambda.ParameterNames.Length > 15, _lambda.ParameterNames.Length, out Target);
					LightThrowTarget = Target;
					return;
				}
				_tracingLambda = GetGeneratorOrNormalLambdaTracing(context);
			}
			if ((object)_tracingDelegate == null)
			{
				_tracingDelegate = CompileLambda(_tracingLambda, new TargetUpdaterForCompilation(context, this).SetCompiledTargetTracing);
			}
			finalTarget = _tracingDelegate;
		}
		else
		{
			if ((object)_normalDelegate == null)
			{
				if (!forceCreation)
				{
					PythonCallTargets.GetPythonTargetType(_lambda.ParameterNames.Length > 15, _lambda.ParameterNames.Length, out Target);
					LightThrowTarget = Target;
					return;
				}
				_normalDelegate = CompileLambda(GetGeneratorOrNormalLambda(), new TargetUpdaterForCompilation(context, this).SetCompiledTarget);
			}
			finalTarget = _normalDelegate;
		}
		finalTarget = AddRecursionCheck(context, finalTarget);
		SetTarget(finalTarget);
	}

	internal void SetDebugTarget(PythonContext context, Delegate target)
	{
		_normalDelegate = target;
		SetTarget(AddRecursionCheck(context, target));
	}

	private System.Linq.Expressions.LambdaExpression GetGeneratorOrNormalLambdaTracing(PythonContext context)
	{
		PythonDebuggingPayload customPayload = new PythonDebuggingPayload(this);
		DebugLambdaInfo debugInfo = new DebugLambdaInfo(null, null, optimizeForLeafFrames: false, null, null, customPayload);
		if ((Flags & FunctionAttributes.Generator) == 0)
		{
			return context.DebugContext.TransformLambda((System.Linq.Expressions.LambdaExpression)Node.RemoveFrame(_lambda.GetLambda()), debugInfo);
		}
		return System.Linq.Expressions.Expression.Lambda(Code.Type, new GeneratorRewriter(_lambda.Name, Node.RemoveFrame(Code.Body)).Reduce(_lambda.ShouldInterpret, _lambda.EmitDebugSymbols, context.Options.CompilationThreshold, Code.Parameters, (Expression<Func<MutableTuple, object>> x) => (Expression<Func<MutableTuple, object>>)context.DebugContext.TransformLambda(x, debugInfo)), Code.Name, Code.Parameters);
	}

	private LightLambdaExpression GetGeneratorOrNormalLambda()
	{
		if ((Flags & FunctionAttributes.Generator) == 0)
		{
			return Code;
		}
		return Code.ToGenerator(_lambda.ShouldInterpret, _lambda.EmitDebugSymbols, _lambda.GlobalParent.PyContext.Options.CompilationThreshold);
	}

	private Delegate CompileLambda(LightLambdaExpression code, EventHandler<LightLambdaCompileEventArgs> handler)
	{
		if (_lambda.ShouldInterpret)
		{
			Delegate obj = code.Compile(_lambda.GlobalParent.PyContext.Options.CompilationThreshold);
			if (obj.Target is LightLambda lightLambda)
			{
				lightLambda.Compile += handler;
			}
			return obj;
		}
		return code.Compile();
	}

	private Delegate CompileLambda(System.Linq.Expressions.LambdaExpression code, EventHandler<LightLambdaCompileEventArgs> handler)
	{
		if (_lambda.ShouldInterpret)
		{
			Delegate obj = code.LightCompile(_lambda.GlobalParent.PyContext.Options.CompilationThreshold);
			if (obj.Target is LightLambda lightLambda)
			{
				lightLambda.Compile += handler;
			}
			return obj;
		}
		return code.Compile();
	}

	internal Delegate AddRecursionCheck(PythonContext context, Delegate finalTarget)
	{
		if (context.RecursionLimit != int.MaxValue)
		{
			if (finalTarget is Func<CodeContext, CodeContext> || finalTarget is Func<FunctionCode, object> || finalTarget is LookupCompilationDelegate)
			{
				return finalTarget;
			}
			finalTarget = _lambda.ParameterNames.Length switch
			{
				0 => new Func<PythonFunction, object>(new PythonFunctionRecursionCheck0((Func<PythonFunction, object>)finalTarget).CallTarget), 
				1 => new Func<PythonFunction, object, object>(new PythonFunctionRecursionCheck1((Func<PythonFunction, object, object>)finalTarget).CallTarget), 
				2 => new Func<PythonFunction, object, object, object>(new PythonFunctionRecursionCheck2((Func<PythonFunction, object, object, object>)finalTarget).CallTarget), 
				3 => new Func<PythonFunction, object, object, object, object>(new PythonFunctionRecursionCheck3((Func<PythonFunction, object, object, object, object>)finalTarget).CallTarget), 
				4 => new Func<PythonFunction, object, object, object, object, object>(new PythonFunctionRecursionCheck4((Func<PythonFunction, object, object, object, object, object>)finalTarget).CallTarget), 
				5 => new Func<PythonFunction, object, object, object, object, object, object>(new PythonFunctionRecursionCheck5((Func<PythonFunction, object, object, object, object, object, object>)finalTarget).CallTarget), 
				6 => new Func<PythonFunction, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck6((Func<PythonFunction, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				7 => new Func<PythonFunction, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck7((Func<PythonFunction, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				8 => new Func<PythonFunction, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck8((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				9 => new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck9((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				10 => new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck10((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				11 => new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck11((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				12 => new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck12((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				13 => new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck13((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				14 => new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck14((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				15 => new Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(new PythonFunctionRecursionCheck15((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)finalTarget).CallTarget), 
				_ => new Func<PythonFunction, object[], object>(new PythonFunctionRecursionCheckN((Func<PythonFunction, object[], object>)finalTarget).CallTarget), 
			};
		}
		return finalTarget;
	}

	System.Linq.Expressions.Expression IExpressionSerializable.CreateExpression()
	{
		return System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("MakeFunctionCode"), PythonAst._globalContext, System.Linq.Expressions.Expression.Constant(_lambda.Name), System.Linq.Expressions.Expression.Constant(_initialDoc, typeof(string)), System.Linq.Expressions.Expression.NewArrayInit(typeof(string), ArrayUtils.ConvertAll(_lambda.ParameterNames, (string x) => System.Linq.Expressions.Expression.Constant(x))), System.Linq.Expressions.Expression.Constant(Flags), System.Linq.Expressions.Expression.Constant(_lambda.IndexSpan.Start), System.Linq.Expressions.Expression.Constant(_lambda.IndexSpan.End), System.Linq.Expressions.Expression.Constant(_lambda.Filename), GetGeneratorOrNormalLambda(), TupleToStringArray(co_freevars), TupleToStringArray(co_names), TupleToStringArray(co_cellvars), TupleToStringArray(co_varnames), System.Linq.Expressions.Expression.Constant(_localCount));
	}

	private static System.Linq.Expressions.Expression TupleToStringArray(PythonTuple tuple)
	{
		if (tuple.Count <= 0)
		{
			return System.Linq.Expressions.Expression.Constant(null, typeof(string[]));
		}
		return System.Linq.Expressions.Expression.NewArrayInit(typeof(string), ArrayUtils.ConvertAll(tuple._data, (object x) => System.Linq.Expressions.Expression.Constant(x)));
	}
}
