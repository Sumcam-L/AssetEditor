using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class MetaPythonFunction : MetaPythonObject, IPythonInvokable, IPythonOperable, IPythonConvertible, IInferableInvokable, IConvertibleMetaObject, IPythonGetable
{
	private class FunctionBinderHelper
	{
		private readonly MetaPythonFunction _func;

		private readonly DynamicMetaObject[] _args;

		private readonly DynamicMetaObject[] _originalArgs;

		private readonly DynamicMetaObjectBinder _call;

		private readonly Expression _codeContext;

		private List<ParameterExpression> _temps;

		private ParameterExpression _dict;

		private ParameterExpression _params;

		private ParameterExpression _paramsLen;

		private List<Expression> _init;

		private Expression _error;

		private bool _extractedParams;

		private bool _extractedDefault;

		private bool _needCodeTest;

		private Expression _deferTest;

		private Expression _userProvidedParams;

		private Expression _paramlessCheck;

		private CallSignature Signature => BindingHelpers.GetCallSignature(_call);

		public FunctionBinderHelper(DynamicMetaObjectBinder call, MetaPythonFunction function, Expression codeContext, DynamicMetaObject[] args)
		{
			_call = call;
			_func = function;
			_args = args;
			_originalArgs = args;
			_temps = new List<ParameterExpression>();
			_codeContext = codeContext;
			int num = Signature.IndexOf(ArgumentType.Instance);
			if (num > -1)
			{
				_args = ArrayUtils.RemoveAt(_args, num);
			}
		}

		public DynamicMetaObject MakeMetaObject()
		{
			Expression[] argumentsForRule = GetArgumentsForRule();
			BindingRestrictions restrictions = _func.Restrictions.Merge(GetRestrictions().Merge(BindingRestrictions.Combine(_args)));
			DynamicMetaObject dynamicMetaObject;
			if (argumentsForRule == null)
			{
				dynamicMetaObject = ((_error == null) ? new DynamicMetaObject(_call.Throw(Expression.Call(typeof(PythonOps).GetMethod(Signature.HasKeywordArgument() ? "BadKeywordArgumentError" : "FunctionBadArgumentError"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(Signature.GetProvidedPositionalArgumentCount())), typeof(object)), restrictions) : new DynamicMetaObject(_error, restrictions));
			}
			else
			{
				Expression expression = AddInitialization(MakeFunctionInvoke(argumentsForRule));
				if (_temps.Count > 0)
				{
					expression = Expression.Block(_temps, expression);
				}
				dynamicMetaObject = new DynamicMetaObject(expression, restrictions);
			}
			DynamicMetaObject[] array = ArrayUtils.Insert(_func, _originalArgs);
			if (_codeContext != null)
			{
				array = ArrayUtils.Insert(new DynamicMetaObject(_codeContext, BindingRestrictions.Empty), array);
			}
			return BindingHelpers.AddDynamicTestAndDefer(_call, dynamicMetaObject, array, new ValidationInfo(_deferTest), dynamicMetaObject.Expression.Type);
		}

		private BindingRestrictions GetRestrictions()
		{
			if (!Signature.HasKeywordArgument())
			{
				return GetSimpleRestriction();
			}
			return GetComplexRestriction();
		}

		private BindingRestrictions GetSimpleRestriction()
		{
			_deferTest = Expression.Equal(Expression.Call(typeof(PythonOps).GetMethod("FunctionGetCompatibility"), Expression.Convert(_func.Expression, typeof(PythonFunction))), Utils.Constant(_func.Value.FunctionCompatibility));
			return BindingRestrictionsHelpers.GetRuntimeTypeRestriction(_func.Expression, typeof(PythonFunction));
		}

		private BindingRestrictions GetComplexRestriction()
		{
			if (_extractedDefault)
			{
				return BindingRestrictions.GetInstanceRestriction(_func.Expression, _func.Value);
			}
			if (_needCodeTest)
			{
				return GetSimpleRestriction().Merge(BindingRestrictions.GetInstanceRestriction(Expression.Property(GetFunctionParam(), "__code__"), _func.Value.__code__));
			}
			return GetSimpleRestriction();
		}

		private Expression[] GetArgumentsForRule()
		{
			Expression[] array = new Expression[_func.Value.NormalArgumentCount + _func.Value.ExtraArguments];
			List<Expression> list = null;
			Dictionary<string, Expression> dictionary = null;
			int num = Signature.IndexOf(ArgumentType.Instance);
			for (int i = 0; i < _args.Length; i++)
			{
				int num2 = ((num == -1 || i < num) ? i : (i + 1));
				switch (Signature.GetArgumentKind(i))
				{
				case ArgumentType.Dictionary:
					_args[num2] = MakeDictionaryCopy(_args[num2]);
					break;
				case ArgumentType.List:
					_userProvidedParams = _args[num2].Expression;
					break;
				case ArgumentType.Named:
				{
					_needCodeTest = true;
					bool flag = false;
					for (int j = 0; j < _func.Value.NormalArgumentCount; j++)
					{
						if (_func.Value.ArgNames[j] == Signature.GetArgumentName(i))
						{
							if (array[j] != null)
							{
								if (_error == null)
								{
									_error = _call.Throw(Expression.Call(typeof(PythonOps).GetMethod("MultipleKeywordArgumentError"), GetFunctionParam(), Expression.Constant(_func.Value.ArgNames[j])), typeof(object));
								}
								return null;
							}
							array[j] = _args[num2].Expression;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						if (dictionary == null)
						{
							dictionary = new Dictionary<string, Expression>();
						}
						dictionary[Signature.GetArgumentName(i)] = _args[num2].Expression;
					}
					break;
				}
				default:
					if (i < _func.Value.NormalArgumentCount)
					{
						array[i] = _args[num2].Expression;
						break;
					}
					if (list == null)
					{
						list = new List<Expression>();
					}
					list.Add(_args[num2].Expression);
					break;
				}
			}
			if (!FinishArguments(array, list, dictionary))
			{
				if (dictionary != null && _func.Value.ExpandDictPosition == -1)
				{
					MakeUnexpectedKeywordError(dictionary);
				}
				return null;
			}
			return GetArgumentsForTargetType(array);
		}

		private bool FinishArguments(Expression[] exprArgs, List<Expression> paramsArgs, Dictionary<string, Expression> namedArgs)
		{
			int num = _func.Value.NormalArgumentCount - _func.Value.Defaults.Length;
			for (int i = 0; i < _func.Value.NormalArgumentCount; i++)
			{
				if (exprArgs[i] != null)
				{
					if (_userProvidedParams != null && i >= Signature.GetProvidedPositionalArgumentCount())
					{
						exprArgs[i] = ValidateNotDuplicate(exprArgs[i], _func.Value.ArgNames[i], i);
					}
				}
				else if (i < num)
				{
					exprArgs[i] = ExtractNonDefaultValue(_func.Value.ArgNames[i]);
					if (exprArgs[i] == null)
					{
						return false;
					}
				}
				else
				{
					exprArgs[i] = ExtractDefaultValue(i, i - num);
				}
			}
			if (!TryFinishList(exprArgs, paramsArgs) || !TryFinishDictionary(exprArgs, namedArgs))
			{
				return false;
			}
			AddCheckForNoExtraParameters(exprArgs);
			return true;
		}

		private bool TryFinishList(Expression[] exprArgs, List<Expression> paramsArgs)
		{
			if (_func.Value.ExpandListPosition != -1)
			{
				if (_userProvidedParams != null)
				{
					if (_params == null && paramsArgs == null)
					{
						exprArgs[_func.Value.ExpandListPosition] = Expression.Call(typeof(PythonOps).GetMethod("GetOrCopyParamsTuple"), GetFunctionParam(), Utils.Convert(_userProvidedParams, typeof(object)));
					}
					else
					{
						EnsureParams();
						exprArgs[_func.Value.ExpandListPosition] = Expression.Call(typeof(PythonOps).GetMethod("MakeTupleFromSequence"), Utils.Convert(_params, typeof(object)));
						if (paramsArgs != null)
						{
							MakeParamsAddition(paramsArgs);
						}
					}
				}
				else
				{
					exprArgs[_func.Value.ExpandListPosition] = MakeParamsTuple(paramsArgs);
				}
			}
			else if (paramsArgs != null)
			{
				return false;
			}
			return true;
		}

		private void MakeParamsAddition(List<Expression> paramsArgs)
		{
			_extractedParams = true;
			List<Expression> list = new List<Expression>(paramsArgs.Count + 1);
			list.Add(_params);
			list.AddRange(paramsArgs);
			EnsureInit();
			_init.Add(Utils.ComplexCallHelper(typeof(PythonOps).GetMethod("AddParamsArguments"), list.ToArray()));
		}

		private bool TryFinishDictionary(Expression[] exprArgs, Dictionary<string, Expression> namedArgs)
		{
			if (_func.Value.ExpandDictPosition != -1)
			{
				if (_dict != null)
				{
					exprArgs[_func.Value.ExpandDictPosition] = _dict;
					if (namedArgs != null)
					{
						foreach (KeyValuePair<string, Expression> namedArg in namedArgs)
						{
							MakeDictionaryAddition(namedArg);
						}
					}
				}
				else
				{
					exprArgs[_func.Value.ExpandDictPosition] = MakeDictionary(namedArgs);
				}
			}
			else if (namedArgs != null)
			{
				return false;
			}
			return true;
		}

		private void MakeDictionaryAddition(KeyValuePair<string, Expression> kvp)
		{
			_init.Add(Expression.Call(typeof(PythonOps).GetMethod("AddDictionaryArgument"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(kvp.Key), Utils.Convert(kvp.Value, typeof(object)), Utils.Convert(_dict, typeof(PythonDictionary))));
		}

		private void AddCheckForNoExtraParameters(Expression[] exprArgs)
		{
			List<Expression> list = new List<Expression>(3);
			if (_func.Value.ExpandListPosition == -1)
			{
				if (_params != null)
				{
					list.Add(Expression.Call(typeof(PythonOps).GetMethod("CheckParamsZero"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), _params));
				}
				else if (_userProvidedParams != null)
				{
					list.Add(Expression.Call(typeof(PythonOps).GetMethod("CheckUserParamsZero"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Convert(_userProvidedParams, typeof(object))));
				}
			}
			if (_func.Value.ExpandDictPosition == -1 && _dict != null)
			{
				list.Add(Expression.Call(typeof(PythonOps).GetMethod("CheckDictionaryZero"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Convert(_dict, typeof(IDictionary))));
			}
			if (list.Count != 0)
			{
				if (exprArgs.Length != 0)
				{
					Expression expression = exprArgs[exprArgs.Length - 1];
					ParameterExpression parameterExpression;
					_temps.Add(parameterExpression = Expression.Variable(expression.Type, "$temp"));
					list.Insert(0, Expression.Assign(parameterExpression, expression));
					list.Add(parameterExpression);
					exprArgs[exprArgs.Length - 1] = Expression.Block(list.ToArray());
				}
				else
				{
					_paramlessCheck = Expression.Block(list.ToArray());
				}
			}
		}

		private Expression ValidateNotDuplicate(Expression value, string name, int position)
		{
			EnsureParams();
			return Expression.Block(Expression.Call(typeof(PythonOps).GetMethod("VerifyUnduplicatedByPosition"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(name, typeof(string)), Utils.Constant(position), _paramsLen), value);
		}

		private Expression ExtractNonDefaultValue(string name)
		{
			if (_userProvidedParams != null)
			{
				if (_dict != null)
				{
					return ExtractFromListOrDictionary(name);
				}
				return ExtractNextParamsArg();
			}
			if (_dict != null)
			{
				return ExtractDictionaryArgument(name);
			}
			return null;
		}

		private Expression ExtractDictionaryArgument(string name)
		{
			_needCodeTest = true;
			return Expression.Call(typeof(PythonOps).GetMethod("ExtractDictionaryArgument"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(name, typeof(string)), Utils.Constant(Signature.ArgumentCount), Utils.Convert(_dict, typeof(PythonDictionary)));
		}

		private Expression ExtractDefaultValue(int index, int dfltIndex)
		{
			if (_dict == null && _userProvidedParams == null)
			{
				return Expression.Call(typeof(PythonOps).GetMethod("FunctionGetDefaultValue"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(dfltIndex));
			}
			if (_userProvidedParams != null)
			{
				EnsureParams();
			}
			_extractedDefault = true;
			return Expression.Call(typeof(PythonOps).GetMethod("GetFunctionParameterValue"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(dfltIndex), Utils.Constant(_func.Value.ArgNames[index], typeof(string)), VariableOrNull(_params, typeof(List)), VariableOrNull(_dict, typeof(PythonDictionary)));
		}

		private Expression ExtractFromListOrDictionary(string name)
		{
			EnsureParams();
			_needCodeTest = true;
			return Expression.Call(typeof(PythonOps).GetMethod("ExtractAnyArgument"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(name, typeof(string)), _paramsLen, _params, Utils.Convert(_dict, typeof(IDictionary)));
		}

		private void EnsureParams()
		{
			if (!_extractedParams)
			{
				MakeParamsCopy(_userProvidedParams);
				_extractedParams = true;
			}
		}

		private Expression ExtractNextParamsArg()
		{
			if (!_extractedParams)
			{
				MakeParamsCopy(_userProvidedParams);
				_extractedParams = true;
			}
			return Expression.Call(typeof(PythonOps).GetMethod("ExtractParamsArgument"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(Signature.ArgumentCount), _params);
		}

		private static Expression VariableOrNull(ParameterExpression var, Type type)
		{
			if (var != null)
			{
				return Utils.Convert(var, type);
			}
			return Utils.Constant(null, type);
		}

		private Expression[] GetArgumentsForTargetType(Expression[] exprArgs)
		{
			Type type = _func.Value.func_code.Target.GetType();
			if (type == typeof(Func<PythonFunction, object[], object>))
			{
				exprArgs = new Expression[1] { Utils.NewArrayHelper(typeof(object), exprArgs) };
			}
			return exprArgs;
		}

		private UnaryExpression GetFunctionParam()
		{
			return Expression.Convert(_func.Expression, typeof(PythonFunction));
		}

		private DynamicMetaObject MakeDictionaryCopy(DynamicMetaObject userDict)
		{
			userDict = userDict.Restrict(userDict.GetLimitType());
			_temps.Add(_dict = Expression.Variable(typeof(PythonDictionary), "$dict"));
			EnsureInit();
			string name = (typeof(PythonDictionary).IsAssignableFrom(userDict.GetLimitType()) ? "CopyAndVerifyPythonDictionary" : ((!typeof(IDictionary).IsAssignableFrom(userDict.GetLimitType())) ? "CopyAndVerifyUserMapping" : "CopyAndVerifyDictionary"));
			_init.Add(Expression.Assign(_dict, Expression.Call(typeof(PythonOps).GetMethod(name), GetFunctionParam(), Utils.Convert(userDict.Expression, userDict.GetLimitType()))));
			return userDict;
		}

		private void MakeParamsCopy(Expression userList)
		{
			_temps.Add(_params = Expression.Variable(typeof(List), "$list"));
			_temps.Add(_paramsLen = Expression.Variable(typeof(int), "$paramsLen"));
			EnsureInit();
			_init.Add(Expression.Assign(_params, Expression.Call(typeof(PythonOps).GetMethod("CopyAndVerifyParamsList"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Convert(userList, typeof(object)))));
			_init.Add(Expression.Assign(_paramsLen, Expression.Add(Expression.Call(_params, typeof(List).GetMethod("__len__")), Utils.Constant(Signature.GetProvidedPositionalArgumentCount()))));
		}

		private Expression MakeDictionary(Dictionary<string, Expression> namedArgs)
		{
			_temps.Add(_dict = Expression.Variable(typeof(PythonDictionary), "$dict"));
			if (namedArgs != null)
			{
				Expression[] array = new Expression[namedArgs.Count * 2];
				int num = 0;
				foreach (KeyValuePair<string, Expression> namedArg in namedArgs)
				{
					array[num++] = Utils.Convert(namedArg.Value, typeof(object));
					array[num++] = Utils.Constant(namedArg.Key, typeof(object));
				}
				return Expression.Assign(_dict, Expression.Call(typeof(PythonOps).GetMethod("MakeHomogeneousDictFromItems"), Expression.NewArrayInit(typeof(object), array)));
			}
			return Expression.Assign(_dict, Expression.Call(typeof(PythonOps).GetMethod("MakeDict"), Utils.Constant(0)));
		}

		private static Expression MakeParamsTuple(List<Expression> extraArgs)
		{
			if (extraArgs != null)
			{
				return Utils.ComplexCallHelper(typeof(PythonOps).GetMethod("MakeTuple"), extraArgs.ToArray());
			}
			return Expression.Call(typeof(PythonOps).GetMethod("MakeTuple"), Expression.NewArrayInit(typeof(object[])));
		}

		private Expression MakeFunctionInvoke(Expression[] invokeArgs)
		{
			Type type = _func.Value.func_code.Target.GetType();
			MethodInfo method = type.GetMethod("Invoke");
			invokeArgs = ArrayUtils.Insert(GetFunctionParam(), invokeArgs);
			Expression expression = Utils.SimpleCallHelper(Expression.Convert(Expression.Call(_call.SupportsLightThrow() ? typeof(PythonOps).GetMethod("FunctionGetLightThrowTarget") : typeof(PythonOps).GetMethod("FunctionGetTarget"), GetFunctionParam()), type), method, invokeArgs);
			if (_paramlessCheck != null)
			{
				expression = Expression.Block(_paramlessCheck, expression);
			}
			return expression;
		}

		private Expression AddInitialization(Expression body)
		{
			if (_init == null)
			{
				return body;
			}
			List<Expression> list = new List<Expression>(_init);
			list.Add(body);
			return Expression.Block(list);
		}

		private void MakeUnexpectedKeywordError(Dictionary<string, Expression> namedArgs)
		{
			string value = null;
			using (Dictionary<string, Expression>.KeyCollection.Enumerator enumerator = namedArgs.Keys.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					value = current;
				}
			}
			_error = _call.Throw(Expression.Call(typeof(PythonOps).GetMethod("UnexpectedKeywordArgumentError"), Utils.Convert(GetFunctionParam(), typeof(PythonFunction)), Utils.Constant(value, typeof(string))), typeof(PythonOps));
		}

		private void EnsureInit()
		{
			if (_init == null)
			{
				_init = new List<Expression>();
			}
		}
	}

	public new PythonFunction Value => (PythonFunction)base.Value;

	public MetaPythonFunction(Expression expression, BindingRestrictions restrictions, PythonFunction value)
		: base(expression, BindingRestrictions.Empty, value)
	{
	}

	public DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return new FunctionBinderHelper(pythonInvoke, this, codeContext, args).MakeMetaObject();
	}

	public DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext)
	{
		return BindGetMemberWorker(member, member.Name, codeContext);
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
		DynamicMetaObject dynamicMetaObject = action.FallbackInvokeMember(this, args);
		return action.FallbackInvokeMember(this, args, new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.NotEqual(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("PythonFunctionGetMember"), Utils.Convert(base.Expression, typeof(PythonFunction)), Expression.Constant(action.Name))), Expression.Constant(OperationFailed.Value)), action.FallbackInvoke(new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty), args, null).Expression, Utils.Convert(dynamicMetaObject.Expression, typeof(object)))), BindingRestrictions.GetTypeRestriction(base.Expression, typeof(PythonFunction)).Merge(dynamicMetaObject.Restrictions)));
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder call, DynamicMetaObject[] args)
	{
		return new FunctionBinderHelper(call, this, null, args).MakeMetaObject();
	}

	public override DynamicMetaObject BindConvert(ConvertBinder conversion)
	{
		return ConvertWorker(conversion, conversion.Type, conversion.Explicit ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast);
	}

	public DynamicMetaObject BindConvert(PythonConversionBinder binder)
	{
		return ConvertWorker(binder, binder.Type, binder.ResultKind);
	}

	public DynamicMetaObject ConvertWorker(DynamicMetaObjectBinder binder, Type type, ConversionResultKind kind)
	{
		if (type.IsSubclassOf(typeof(Delegate)))
		{
			return MetaPythonObject.MakeDelegateTarget(binder, type, Restrict(typeof(PythonFunction)));
		}
		return FallbackConvert(binder);
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		foreach (object o in Value.__dict__.Keys)
		{
			if (o is string)
			{
				yield return (string)o;
			}
		}
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
	{
		return BindGetMemberWorker(binder, binder.Name, PythonContext.GetCodeContextMO(binder));
	}

	private DynamicMetaObject BindGetMemberWorker(DynamicMetaObjectBinder binder, string name, DynamicMetaObject codeContext)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
		DynamicMetaObject dynamicMetaObject = FallbackGetMember(binder, this, codeContext);
		return FallbackGetMember(binder, this, codeContext, new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.NotEqual(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("PythonFunctionGetMember"), Utils.Convert(base.Expression, typeof(PythonFunction)), Expression.Constant(name))), Expression.Constant(OperationFailed.Value)), parameterExpression, Utils.Convert(dynamicMetaObject.Expression, typeof(object)))), BindingRestrictions.GetTypeRestriction(base.Expression, typeof(PythonFunction)).Merge(dynamicMetaObject.Restrictions)));
	}

	private static DynamicMetaObject FallbackGetMember(DynamicMetaObjectBinder binder, DynamicMetaObject self, DynamicMetaObject codeContext)
	{
		return FallbackGetMember(binder, self, codeContext, null);
	}

	private static DynamicMetaObject FallbackGetMember(DynamicMetaObjectBinder binder, DynamicMetaObject self, DynamicMetaObject codeContext, DynamicMetaObject errorSuggestion)
	{
		if (binder is PythonGetMemberBinder pythonGetMemberBinder)
		{
			return pythonGetMemberBinder.Fallback(self, codeContext, errorSuggestion);
		}
		return ((GetMemberBinder)binder).FallbackGetMember(self, errorSuggestion);
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
	{
		return binder.FallbackSetMember(this, value, new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("PythonFunctionSetMember"), Utils.Convert(base.Expression, typeof(PythonFunction)), Expression.Constant(binder.Name), Utils.Convert(value.Expression, typeof(object))), BindingRestrictions.GetTypeRestriction(base.Expression, typeof(PythonFunction))));
	}

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
	{
		switch (binder.Name)
		{
		case "func_dict":
		case "__dict__":
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("PythonFunctionDeleteDict")), BindingRestrictions.GetTypeRestriction(base.Expression, typeof(PythonFunction)));
		case "__doc__":
		case "func_doc":
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("PythonFunctionDeleteDoc"), Expression.Convert(base.Expression, typeof(PythonFunction))), BindingRestrictions.GetTypeRestriction(base.Expression, typeof(PythonFunction)));
		case "func_defaults":
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("PythonFunctionDeleteDefaults"), Expression.Convert(base.Expression, typeof(PythonFunction))), BindingRestrictions.GetTypeRestriction(base.Expression, typeof(PythonFunction)));
		default:
		{
			DynamicMetaObject dynamicMetaObject = binder.FallbackDeleteMember(this);
			return binder.FallbackDeleteMember(this, new DynamicMetaObject(Expression.Condition(Expression.Call(typeof(PythonOps).GetMethod("PythonFunctionDeleteMember"), Utils.Convert(base.Expression, typeof(PythonFunction)), Expression.Constant(binder.Name)), Expression.Default(typeof(void)), Utils.Convert(dynamicMetaObject.Expression, typeof(void))), BindingRestrictions.GetTypeRestriction(base.Expression, typeof(PythonFunction)).Merge(dynamicMetaObject.Restrictions)));
		}
		}
	}

	private static DynamicMetaObject MakeCallSignatureRule(DynamicMetaObject self)
	{
		return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("GetFunctionSignature"), Utils.Convert(self.Expression, typeof(PythonFunction))), BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, typeof(PythonFunction)));
	}

	private static DynamicMetaObject MakeIsCallableRule(DynamicMetaObject self)
	{
		return new DynamicMetaObject(Utils.Constant(true), BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, typeof(PythonFunction)));
	}

	DynamicMetaObject IPythonOperable.BindOperation(PythonOperationBinder action, DynamicMetaObject[] args)
	{
		return action.Operation switch
		{
			PythonOperationKind.CallSignatures => MakeCallSignatureRule(this), 
			PythonOperationKind.IsCallable => MakeIsCallableRule(this), 
			_ => null, 
		};
	}

	InferenceResult IInferableInvokable.GetInferredType(Type delegateType, Type parameterType)
	{
		if (!delegateType.IsSubclassOf(typeof(Delegate)))
		{
			throw new InvalidOperationException();
		}
		MethodInfo method = delegateType.GetMethod("Invoke");
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length == Value.NormalArgumentCount)
		{
			return new InferenceResult(typeof(object), base.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(base.Expression, typeof(PythonFunction)).Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(Expression.Call(typeof(PythonOps).GetMethod("FunctionGetCompatibility"), Expression.Convert(base.Expression, typeof(PythonFunction))), Expression.Constant(Value.FunctionCompatibility))))));
		}
		return null;
	}

	bool IConvertibleMetaObject.CanConvertTo(Type type, bool @explicit)
	{
		return type.IsSubclassOf(typeof(Delegate));
	}
}
