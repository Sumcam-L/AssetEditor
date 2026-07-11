using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;

namespace IronPython.Compiler.Ast;

internal static class AstMethods
{
	public static readonly MethodInfo IsTrue = GetMethod(new Func<object, bool>(PythonOps.IsTrue));

	public static readonly MethodInfo RaiseAssertionError = GetMethod(new Action<object>(PythonOps.RaiseAssertionError));

	public static readonly MethodInfo Repr = GetMethod(new Func<CodeContext, object, string>(PythonOps.Repr));

	public static readonly MethodInfo WarnDivision = GetMethod(new Action<CodeContext, PythonDivisionOptions, object, object>(PythonOps.WarnDivision));

	public static readonly MethodInfo MakeClass = GetMethod(new Func<FunctionCode, Func<CodeContext, CodeContext>, CodeContext, string, object[], string, object>(PythonOps.MakeClass));

	public static readonly MethodInfo UnqualifiedExec = GetMethod(new Action<CodeContext, object>(PythonOps.UnqualifiedExec));

	public static readonly MethodInfo QualifiedExec = GetMethod(new Action<CodeContext, object, PythonDictionary, object>(PythonOps.QualifiedExec));

	public static readonly MethodInfo PrintExpressionValue = GetMethod(new Action<CodeContext, object>(PythonOps.PrintExpressionValue));

	public static readonly MethodInfo PrintCommaWithDest = GetMethod(new Action<CodeContext, object, object>(PythonOps.PrintCommaWithDest));

	public static readonly MethodInfo PrintWithDest = GetMethod(new Action<CodeContext, object, object>(PythonOps.PrintWithDest));

	public static readonly MethodInfo PrintComma = GetMethod(new Action<CodeContext, object>(PythonOps.PrintComma));

	public static readonly MethodInfo Print = GetMethod(new Action<CodeContext, object>(PythonOps.Print));

	public static readonly MethodInfo ImportWithNames = GetMethod(new Func<CodeContext, string, string[], int, object>(PythonOps.ImportWithNames));

	public static readonly MethodInfo ImportFrom = GetMethod(new Func<CodeContext, object, string, object>(PythonOps.ImportFrom));

	public static readonly MethodInfo ImportStar = GetMethod(new Action<CodeContext, string, int>(PythonOps.ImportStar));

	public static readonly MethodInfo SaveCurrentException = GetMethod(new Func<Exception>(PythonOps.SaveCurrentException));

	public static readonly MethodInfo RestoreCurrentException = GetMethod(new Action<Exception>(PythonOps.RestoreCurrentException));

	public static readonly MethodInfo MakeGeneratorExpression = GetMethod(new Func<object, object, object>(PythonOps.MakeGeneratorExpression));

	public static readonly MethodInfo ListAddForComprehension = GetMethod(new Action<List, object>(PythonOps.ListAddForComprehension));

	public static readonly MethodInfo SetAddForComprehension = GetMethod(new Action<SetCollection, object>(PythonOps.SetAddForComprehension));

	public static readonly MethodInfo DictAddForComprehension = GetMethod(new Action<PythonDictionary, object, object>(PythonOps.DictAddForComprehension));

	public static readonly MethodInfo MakeEmptyListFromCode = GetMethod(new Func<List>(PythonOps.MakeEmptyListFromCode));

	public static readonly MethodInfo CheckUninitialized = GetMethod(new Func<object, string, object>(PythonOps.CheckUninitialized));

	public static readonly MethodInfo PrintNewlineWithDest = GetMethod(new Action<CodeContext, object>(PythonOps.PrintNewlineWithDest));

	public static readonly MethodInfo PrintNewline = GetMethod(new Action<CodeContext>(PythonOps.PrintNewline));

	public static readonly MethodInfo PublishModule = GetMethod(new Func<CodeContext, string, object>(PythonOps.PublishModule));

	public static readonly MethodInfo RemoveModule = GetMethod(new Action<CodeContext, string, object>(PythonOps.RemoveModule));

	public static readonly MethodInfo ModuleStarted = GetMethod(new Action<CodeContext, ModuleOptions>(PythonOps.ModuleStarted));

	public static readonly MethodInfo MakeRethrownException = GetMethod(new Func<CodeContext, Exception>(PythonOps.MakeRethrownException));

	public static readonly MethodInfo MakeRethrowExceptionWorker = GetMethod(new Func<Exception, Exception>(PythonOps.MakeRethrowExceptionWorker));

	public static readonly MethodInfo MakeException = GetMethod(new Func<CodeContext, object, object, object, Exception>(PythonOps.MakeException));

	public static readonly MethodInfo MakeSlice = GetMethod(new Func<object, object, object, Slice>(PythonOps.MakeSlice));

	public static readonly MethodInfo ExceptionHandled = GetMethod(new Action<CodeContext>(PythonOps.ExceptionHandled));

	public static readonly MethodInfo GetExceptionInfoLocal = GetMethod(new Func<CodeContext, Exception, PythonTuple>(PythonOps.GetExceptionInfoLocal));

	public static readonly MethodInfo CheckException = GetMethod(new Func<CodeContext, object, object, object>(PythonOps.CheckException));

	public static readonly MethodInfo SetCurrentException = GetMethod(new Func<CodeContext, Exception, object>(PythonOps.SetCurrentException));

	public static readonly MethodInfo BuildExceptionInfo = GetMethod(new Action<CodeContext, Exception>(PythonOps.BuildExceptionInfo));

	public static readonly MethodInfo MakeTuple = GetMethod(new Func<object[], PythonTuple>(PythonOps.MakeTuple));

	public static readonly MethodInfo IsNot = GetMethod(new Func<object, object, object>(PythonOps.IsNot));

	public static readonly MethodInfo Is = GetMethod(new Func<object, object, object>(PythonOps.Is));

	public static readonly MethodInfo ImportTop = GetMethod(new Func<CodeContext, string, int, object>(PythonOps.ImportTop));

	public static readonly MethodInfo ImportBottom = GetMethod(new Func<CodeContext, string, int, object>(PythonOps.ImportBottom));

	public static readonly MethodInfo MakeList = GetMethod(new Func<List>(PythonOps.MakeList));

	public static readonly MethodInfo MakeListNoCopy = GetMethod(new Func<object[], List>(PythonOps.MakeListNoCopy));

	public static readonly MethodInfo GetEnumeratorValues = GetMethod(new Func<CodeContext, object, int, object>(PythonOps.GetEnumeratorValues));

	public static readonly MethodInfo GetEnumeratorValuesNoComplexSets = GetMethod(new Func<CodeContext, object, int, object>(PythonOps.GetEnumeratorValuesNoComplexSets));

	public static readonly MethodInfo GetGlobalContext = GetMethod(new Func<CodeContext, CodeContext>(PythonOps.GetGlobalContext));

	public static readonly MethodInfo GetParentContextFromFunction = GetMethod(new Func<PythonFunction, CodeContext>(PythonOps.GetParentContextFromFunction));

	public static readonly MethodInfo MakeFunction = GetMethod(new Func<CodeContext, FunctionCode, object, object[], object>(PythonOps.MakeFunction));

	public static readonly MethodInfo MakeFunctionDebug = GetMethod(new Func<CodeContext, FunctionCode, object, object[], Delegate, object>(PythonOps.MakeFunctionDebug));

	public static readonly MethodInfo MakeClosureCell = GetMethod(new Func<ClosureCell>(PythonOps.MakeClosureCell));

	public static readonly MethodInfo MakeClosureCellWithValue = GetMethod(new Func<object, ClosureCell>(PythonOps.MakeClosureCellWithValue));

	public static readonly MethodInfo LookupName = GetMethod(new Func<CodeContext, string, object>(PythonOps.LookupName));

	public static readonly MethodInfo RemoveName = GetMethod(new Action<CodeContext, string>(PythonOps.RemoveName));

	public static readonly MethodInfo SetName = GetMethod(new Func<CodeContext, string, object, object>(PythonOps.SetName));

	public static readonly MethodInfo KeepAlive = GetMethod(new Action<object>(GC.KeepAlive));

	public static readonly MethodInfo MakeDict = GetMethod(new Func<int, PythonDictionary>(PythonOps.MakeDict));

	public static readonly MethodInfo MakeEmptyDict = GetMethod(new Func<PythonDictionary>(PythonOps.MakeEmptyDict));

	public static readonly MethodInfo MakeDictFromItems = GetMethod(new Func<object[], PythonDictionary>(PythonOps.MakeDictFromItems));

	public static readonly MethodInfo MakeConstantDict = GetMethod(new Func<object, PythonDictionary>(PythonOps.MakeConstantDict));

	public static readonly MethodInfo MakeSet = GetMethod(new Func<object[], SetCollection>(PythonOps.MakeSet));

	public static readonly MethodInfo MakeEmptySet = GetMethod(new Func<SetCollection>(PythonOps.MakeEmptySet));

	public static readonly MethodInfo MakeHomogeneousDictFromItems = GetMethod(new Func<object[], PythonDictionary>(PythonOps.MakeHomogeneousDictFromItems));

	public static readonly MethodInfo CreateLocalContext = GetMethod(new Func<CodeContext, MutableTuple, string[], CodeContext>(PythonOps.CreateLocalContext));

	public static readonly MethodInfo UpdateStackTrace = GetMethod(new Action<Exception, CodeContext, FunctionCode, int>(PythonOps.UpdateStackTrace));

	public static readonly MethodInfo ForLoopDispose = GetMethod(new Action<KeyValuePair<IEnumerator, IDisposable>>(PythonOps.ForLoopDispose));

	public static readonly MethodInfo GetClosureTupleFromContext = GetMethod(new Func<CodeContext, MutableTuple>(PythonOps.GetClosureTupleFromContext));

	public static readonly MethodInfo IsUnicode = GetMethod(new Func<object, bool>(PythonOps.IsUnicode));

	public static readonly MethodInfo PushFrame = GetMethod(new Func<CodeContext, FunctionCode, List<FunctionStack>>(PythonOps.PushFrame));

	public static readonly MethodInfo FormatUnicode = GetMethod(new Func<CodeContext, string, object, string>(PythonOps.FormatUnicode));

	public static readonly MethodInfo FormatString = GetMethod(new Func<CodeContext, string, object, string>(PythonOps.FormatString));

	public static readonly MethodInfo GetUnicodeFunction = GetMethod(new Func<BuiltinFunction>(PythonOps.GetUnicodeFuntion));

	public static readonly MethodInfo GeneratorCheckThrowableAndReturnSendValue = GetMethod(new Func<object, object>(PythonOps.GeneratorCheckThrowableAndReturnSendValue));

	private static MethodInfo GetMethod(Delegate x)
	{
		return x.Method;
	}
}
