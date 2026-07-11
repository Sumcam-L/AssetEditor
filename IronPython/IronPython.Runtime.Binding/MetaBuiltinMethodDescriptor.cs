using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Binding;

internal class MetaBuiltinMethodDescriptor : MetaPythonObject, IPythonInvokable, IPythonOperable
{
	public new BuiltinMethodDescriptor Value => (BuiltinMethodDescriptor)base.Value;

	public MetaBuiltinMethodDescriptor(Expression expression, BindingRestrictions restrictions, BuiltinMethodDescriptor value)
		: base(expression, BindingRestrictions.Empty, value)
	{
	}

	public DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return InvokeWorker(pythonInvoke, codeContext, args);
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args)
	{
		return BindingHelpers.GenericInvokeMember(action, null, this, args);
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder call, DynamicMetaObject[] args)
	{
		return InvokeWorker(call, PythonContext.GetCodeContext(call), args);
	}

	private DynamicMetaObject InvokeWorker(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject[] args)
	{
		CallSignature signature = BindingHelpers.GetCallSignature(call);
		BindingRestrictions selfRestrict = BindingRestrictions.GetInstanceRestriction(base.Expression, Value).Merge(base.Restrictions);
		selfRestrict = selfRestrict.Merge(BindingRestrictions.GetExpressionRestriction(MakeFunctionTest(Expression.Call(typeof(PythonOps).GetMethod("GetBuiltinMethodDescriptorTemplate"), Expression.Convert(base.Expression, typeof(BuiltinMethodDescriptor))))));
		return Value.Template.MakeBuiltinFunctionCall(call, codeContext, this, args, hasSelf: false, selfRestrict, delegate(DynamicMetaObject[] newArgs)
		{
			PythonContext pythonContext = PythonContext.GetPythonContext(call);
			BindingTarget target;
			DynamicMetaObject res = pythonContext.Binder.CallMethod(new PythonOverloadResolver(pythonContext.Binder, newArgs, signature, codeContext), Value.Template.Targets, selfRestrict, Value.Template.Name, NarrowingLevel.None, Value.Template.IsBinaryOperator ? NarrowingLevel.Two : NarrowingLevel.All, out target);
			return BindingHelpers.CheckLightThrow(call, res, target);
		});
	}

	internal Expression MakeFunctionTest(Expression functionTarget)
	{
		return Expression.Equal(functionTarget, Utils.Constant(Value.Template));
	}

	DynamicMetaObject IPythonOperable.BindOperation(PythonOperationBinder action, DynamicMetaObject[] args)
	{
		PythonOperationKind operation = action.Operation;
		if (operation == PythonOperationKind.CallSignatures)
		{
			return PythonProtocol.MakeCallSignatureOperation(this, Value.Template.Targets);
		}
		return null;
	}
}
