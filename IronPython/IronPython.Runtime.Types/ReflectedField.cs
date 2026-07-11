using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

[PythonType("field#")]
public sealed class ReflectedField : PythonTypeSlot, ICodeFormattable
{
	internal const string UpdateValueTypeFieldWarning = "Setting field {0} on value type {1} may result in updating a copy.  Use {1}.{0}.SetValue(instance, value) if this is safe.  For more information help({1}.{0}.SetValue).";

	private readonly NameType _nameType;

	internal readonly FieldInfo _info;

	public FieldInfo Info
	{
		[PythonHidden]
		get
		{
			return _info;
		}
	}

	public string __doc__ => DocBuilder.DocOneInfo(_info);

	public PythonType FieldType
	{
		[PythonHidden]
		get
		{
			return DynamicHelpers.GetPythonTypeFromType(_info.FieldType);
		}
	}

	internal override bool GetAlwaysSucceeds => true;

	internal override bool CanOptimizeGets => !_info.IsLiteral;

	internal override bool IsAlwaysVisible => _nameType == NameType.PythonField;

	public ReflectedField(FieldInfo info, NameType nameType)
	{
		_nameType = nameType;
		_info = info;
	}

	public ReflectedField(FieldInfo info)
		: this(info, NameType.PythonField)
	{
	}

	public object GetValue(CodeContext context, object instance)
	{
		if (TryGetValue(context, instance, DynamicHelpers.GetPythonType(instance), out var value))
		{
			return value;
		}
		throw new InvalidOperationException("cannot get field");
	}

	public void SetValue(CodeContext context, object instance, object value)
	{
		if (!TrySetValueWorker(context, instance, DynamicHelpers.GetPythonType(instance), value, suppressWarning: true))
		{
			throw new InvalidOperationException("cannot set field");
		}
	}

	public void __set__(CodeContext context, object instance, object value)
	{
		if (instance == null && _info.IsStatic)
		{
			DoSet(context, null, value, suppressWarning: false);
			return;
		}
		if (!_info.IsStatic)
		{
			DoSet(context, instance, value, suppressWarning: false);
			return;
		}
		throw PythonOps.AttributeErrorForReadonlyAttribute(_info.DeclaringType.Name, _info.Name);
	}

	[SpecialName]
	public void __delete__(object instance)
	{
		throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(_info.DeclaringType.Name, _info.Name);
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (instance == null)
		{
			if (_info.IsStatic)
			{
				value = _info.GetValue(null);
			}
			else
			{
				value = this;
			}
		}
		else
		{
			value = _info.GetValue(context.LanguageContext.Binder.Convert(instance, _info.DeclaringType));
		}
		return true;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		return TrySetValueWorker(context, instance, owner, value, suppressWarning: false);
	}

	private bool TrySetValueWorker(CodeContext context, object instance, PythonType owner, object value, bool suppressWarning)
	{
		if (ShouldSetOrDelete(owner))
		{
			DoSet(context, instance, value, suppressWarning);
			return true;
		}
		return false;
	}

	internal override bool IsSetDescriptor(CodeContext context, PythonType owner)
	{
		if ((_info.Attributes & FieldAttributes.InitOnly) == 0)
		{
			return !_info.IsLiteral;
		}
		return false;
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		if (ShouldSetOrDelete(owner))
		{
			throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(_info.DeclaringType.Name, _info.Name);
		}
		return false;
	}

	internal override void MakeGetExpression(PythonBinder binder, Expression codeContext, DynamicMetaObject instance, DynamicMetaObject owner, ConditionalBuilder builder)
	{
		if (!_info.IsPublic || _info.DeclaringType.ContainsGenericParameters())
		{
			base.MakeGetExpression(binder, codeContext, instance, owner, builder);
		}
		else if (instance == null)
		{
			if (_info.IsStatic)
			{
				builder.FinishCondition(Utils.Convert(Expression.Field(null, _info), typeof(object)));
			}
			else
			{
				builder.FinishCondition(Expression.Constant(this));
			}
		}
		else
		{
			builder.FinishCondition(Utils.Convert(Expression.Field(binder.ConvertExpression(instance.Expression, _info.DeclaringType, ConversionResultKind.ExplicitCast, new PythonOverloadResolverFactory(binder, codeContext)), _info), typeof(object)));
		}
	}

	private void DoSet(CodeContext context, object instance, object val, bool suppressWarning)
	{
		if (_info.IsInitOnly || _info.IsLiteral)
		{
			throw PythonOps.AttributeErrorForReadonlyAttribute(_info.DeclaringType.Name, _info.Name);
		}
		if (!suppressWarning && instance != null && instance.GetType().IsValueType())
		{
			PythonOps.Warn(context, PythonExceptions.RuntimeWarning, "Setting field {0} on value type {1} may result in updating a copy.  Use {1}.{0}.SetValue(instance, value) if this is safe.  For more information help({1}.{0}.SetValue).", _info.Name, _info.DeclaringType.Name);
		}
		_info.SetValue(instance, context.LanguageContext.Binder.Convert(val, _info.FieldType));
	}

	private bool ShouldSetOrDelete(PythonType type)
	{
		if ((type == null || !(_info.DeclaringType == type.UnderlyingSystemType)) && _info.IsStatic && !_info.IsLiteral)
		{
			return _info.IsInitOnly;
		}
		return true;
	}

	public string __repr__(CodeContext context)
	{
		return $"<field# {_info.Name} on {_info.DeclaringType.Name}>";
	}
}
