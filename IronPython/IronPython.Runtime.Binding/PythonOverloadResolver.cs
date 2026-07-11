using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal sealed class PythonOverloadResolver : DefaultOverloadResolver
{
	private readonly Expression _context;

	public Expression ContextExpression => _context;

	private new PythonBinder Binder => (PythonBinder)base.Binder;

	public PythonOverloadResolver(PythonBinder binder, DynamicMetaObject instance, IList<DynamicMetaObject> args, CallSignature signature, Expression codeContext)
		: base(binder, instance, args, signature)
	{
		_context = codeContext;
	}

	public PythonOverloadResolver(PythonBinder binder, IList<DynamicMetaObject> args, CallSignature signature, Expression codeContext)
		: this(binder, args, signature, CallTypes.None, codeContext)
	{
	}

	public PythonOverloadResolver(PythonBinder binder, IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType, Expression codeContext)
		: base(binder, args, signature, callType)
	{
		_context = codeContext;
	}

	public override bool CanConvertFrom(Type fromType, DynamicMetaObject fromArg, ParameterWrapper toParameter, NarrowingLevel level)
	{
		if (fromType == typeof(List) || fromType.IsSubclassOf(typeof(List)))
		{
			if (toParameter.Type.IsGenericType() && toParameter.Type.GetGenericTypeDefinition() == typeof(IList<>) && (toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), inherit: false) || toParameter.ParameterInfo.IsDefined(typeof(BytesConversionNoStringAttribute), inherit: false)))
			{
				return false;
			}
		}
		else if (fromType == typeof(string))
		{
			if (toParameter.Type == typeof(IList<byte>) && !Binder.Context.PythonOptions.Python30 && toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), inherit: false))
			{
				return true;
			}
		}
		else if (fromType == typeof(Bytes) && toParameter.Type == typeof(string) && !Binder.Context.PythonOptions.Python30 && toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), inherit: false))
		{
			return true;
		}
		return base.CanConvertFrom(fromType, fromArg, toParameter, level);
	}

	protected override BitArray MapSpecialParameters(ParameterMapping mapping)
	{
		IList<ParameterInfo> parameters = mapping.Overload.Parameters;
		BitArray bitArray = base.MapSpecialParameters(mapping);
		if (parameters.Count > 0)
		{
			bool flag = false;
			for (int i = 0; i < parameters.Count; i++)
			{
				bool flag2 = false;
				if (parameters[i].ParameterType.IsSubclassOf(typeof(SiteLocalStorage)))
				{
					mapping.AddBuilder(new SiteLocalStorageBuilder(parameters[i]));
					flag2 = true;
				}
				else if (parameters[i].ParameterType == typeof(CodeContext) && !flag)
				{
					mapping.AddBuilder(new ContextArgBuilder(parameters[i]));
					flag2 = true;
				}
				else
				{
					flag = true;
				}
				if (flag2)
				{
					BitArray obj = bitArray ?? new BitArray(parameters.Count);
					bitArray = obj;
					obj[i] = true;
				}
			}
		}
		return bitArray;
	}

	protected override Expression GetByRefArrayExpression(Expression argumentArrayExpression)
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeTuple"), argumentArrayExpression);
	}

	protected override bool AllowMemberInitialization(OverloadInfo method)
	{
		if (method.IsInstanceFactory)
		{
			return !method.DeclaringType.IsDefined(typeof(PythonTypeAttribute), inherit: true);
		}
		return false;
	}

	public override Expression Convert(DynamicMetaObject metaObject, Type restrictedType, ParameterInfo info, Type toType)
	{
		return Binder.ConvertExpression(metaObject.Expression, toType, ConversionResultKind.ExplicitCast, new PythonOverloadResolverFactory(Binder, _context));
	}

	public override Expression GetDynamicConversion(Expression value, Type type)
	{
		return Expression.Dynamic(Binder.Context.Convert(type, ConversionResultKind.ExplicitCast), type, value);
	}

	public override Type GetGenericInferenceType(DynamicMetaObject dynamicObject)
	{
		Type finalSystemType = PythonTypeOps.GetFinalSystemType(dynamicObject.LimitType);
		if (finalSystemType == typeof(ExtensibleString) || finalSystemType == typeof(ExtensibleComplex) || (finalSystemType.IsGenericType() && finalSystemType.GetGenericTypeDefinition() == typeof(Extensible<>)))
		{
			return typeof(object);
		}
		return finalSystemType;
	}
}
