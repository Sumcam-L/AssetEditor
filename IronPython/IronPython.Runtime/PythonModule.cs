using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("module")]
[DebuggerDisplay("module: {GetName()}")]
[DebuggerTypeProxy(typeof(DebugProxy))]
public class PythonModule : IDynamicMetaObjectProvider, IPythonMembersList, IMembersList
{
	private class MetaModule : MetaPythonObject, IPythonGetable
	{
		public MetaModule(PythonModule module, Expression self)
			: base(self, BindingRestrictions.Empty, module)
		{
		}

		public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
		{
			return GetMemberWorker(binder, PythonContext.GetCodeContextMO(binder));
		}

		public DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext)
		{
			return GetMemberWorker(member, codeContext);
		}

		private DynamicMetaObject GetMemberWorker(DynamicMetaObjectBinder binder, DynamicMetaObject codeContext)
		{
			string getMemberName = MetaPythonObject.GetGetMemberName(binder);
			ParameterExpression parameterExpression = Expression.Variable(typeof(object), "res");
			return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.Call(typeof(PythonOps).GetMethod("ModuleTryGetMember"), PythonContext.GetCodeContext(binder), Utils.Convert(base.Expression, typeof(PythonModule)), Expression.Constant(getMemberName), parameterExpression), parameterExpression, Expression.Convert(MetaPythonObject.GetMemberFallback(this, binder, codeContext).Expression, typeof(object)))), BindingRestrictions.GetTypeRestriction(base.Expression, base.Value.GetType()));
		}

		public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args)
		{
			return BindingHelpers.GenericInvokeMember(action, null, this, args);
		}

		public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
		{
			return new DynamicMetaObject(Expression.Block(Expression.Call(Utils.Convert(base.Expression, typeof(PythonModule)), typeof(PythonModule).GetMethod("__setattr__"), PythonContext.GetCodeContext(binder), Expression.Constant(binder.Name), Expression.Convert(value.Expression, typeof(object))), Expression.Convert(value.Expression, typeof(object))), BindingRestrictions.GetTypeRestriction(base.Expression, base.Value.GetType()));
		}

		public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
		{
			return new DynamicMetaObject(Expression.Call(Utils.Convert(base.Expression, typeof(PythonModule)), typeof(PythonModule).GetMethod("__delattr__"), PythonContext.GetCodeContext(binder), Expression.Constant(binder.Name)), BindingRestrictions.GetTypeRestriction(base.Expression, base.Value.GetType()));
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			foreach (object o in ((PythonModule)base.Value).__dict__.Keys)
			{
				if (o is string str)
				{
					yield return str;
				}
			}
		}
	}

	internal class DebugProxy
	{
		private readonly PythonModule _module;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public List<ObjectDebugView> Members
		{
			get
			{
				List<ObjectDebugView> list = new List<ObjectDebugView>();
				foreach (KeyValuePair<object, object> item in _module._dict)
				{
					list.Add(new ObjectDebugView(item.Key, item.Value));
				}
				return list;
			}
		}

		public DebugProxy(PythonModule module)
		{
			_module = module;
		}
	}

	private readonly PythonDictionary _dict;

	private Scope _scope;

	internal PythonDictionary __dict__ => _dict;

	internal Scope Scope
	{
		get
		{
			if (_scope == null)
			{
				Interlocked.CompareExchange(ref _scope, new Scope(new ObjectDictionaryExpando(_dict)), null);
			}
			return _scope;
		}
	}

	public PythonModule()
	{
		_dict = new PythonDictionary();
		if (GetType() != typeof(PythonModule) && this is IPythonObject)
		{
			((IPythonObject)this).ReplaceDict(_dict);
		}
	}

	internal PythonModule(PythonContext context, Scope scope)
	{
		_dict = new PythonDictionary(new ScopeDictionaryStorage(context, scope));
		_scope = scope;
	}

	internal PythonModule(PythonDictionary dict)
	{
		_dict = dict;
	}

	public static PythonModule __new__(CodeContext context, PythonType cls, params object[] argsø)
	{
		if (cls == TypeCache.Module)
		{
			return new PythonModule();
		}
		if (cls.IsSubclassOf(TypeCache.Module))
		{
			return (PythonModule)cls.CreateInstance(context);
		}
		throw PythonOps.TypeError("{0} is not a subtype of module", cls.Name);
	}

	[StaticExtensionMethod]
	public static PythonModule __new__(CodeContext context, PythonType cls, [ParamDictionary] PythonDictionary kwDictø, params object[] argsø)
	{
		return __new__(context, cls, argsø);
	}

	public void __init__(string name)
	{
		__init__(name, null);
	}

	public void __init__(string name, string documentation)
	{
		_dict["__name__"] = name;
		_dict["__doc__"] = documentation;
	}

	public object __getattribute__(CodeContext context, string name)
	{
		if (GetType() != typeof(PythonModule) && DynamicHelpers.GetPythonType(this).TryResolveMixedSlot(context, name, out var slot) && slot.TryGetValue(context, this, DynamicHelpers.GetPythonType(this), out var value))
		{
			return value;
		}
		switch (name)
		{
		case "__dict__":
			return __dict__;
		case "__class__":
			return DynamicHelpers.GetPythonType(this);
		default:
			if (_dict.TryGetValue(name, out value))
			{
				return value;
			}
			return ObjectOps.__getattribute__(context, this, name);
		}
	}

	internal object GetAttributeNoThrow(CodeContext context, string name)
	{
		if (GetType() != typeof(PythonModule) && DynamicHelpers.GetPythonType(this).TryResolveMixedSlot(context, name, out var slot) && slot.TryGetValue(context, this, DynamicHelpers.GetPythonType(this), out var value))
		{
			return value;
		}
		switch (name)
		{
		case "__dict__":
			return __dict__;
		case "__class__":
			return DynamicHelpers.GetPythonType(this);
		default:
			if (_dict.TryGetValue(name, out value))
			{
				return value;
			}
			if (DynamicHelpers.GetPythonType(this).TryGetNonCustomMember(context, this, name, out value))
			{
				return value;
			}
			return OperationFailed.Value;
		}
	}

	public void __setattr__(CodeContext context, string name, object value)
	{
		if (!(GetType() != typeof(PythonModule)) || !DynamicHelpers.GetPythonType(this).TryResolveMixedSlot(context, name, out var slot) || !slot.TrySetValue(context, this, DynamicHelpers.GetPythonType(this), value))
		{
			switch (name)
			{
			case "__dict__":
				throw PythonOps.TypeError("readonly attribute");
			case "__class__":
				throw PythonOps.TypeError("__class__ assignment: only for heap types");
			}
			_dict[name] = value;
		}
	}

	public void __delattr__(CodeContext context, string name)
	{
		if (GetType() != typeof(PythonModule) && DynamicHelpers.GetPythonType(this).TryResolveMixedSlot(context, name, out var slot) && slot.TryDeleteValue(context, this, DynamicHelpers.GetPythonType(this)))
		{
			return;
		}
		switch (name)
		{
		case "__dict__":
			throw PythonOps.TypeError("readonly attribute");
		case "__class__":
			throw PythonOps.TypeError("can't delete __class__ attribute");
		}
		if (!_dict.TryRemoveValue(name, out var _))
		{
			throw PythonOps.AttributeErrorForMissingAttribute("module", name);
		}
	}

	public string __repr__()
	{
		return __str__();
	}

	public string __str__()
	{
		if (!_dict.TryGetValue("__file__", out var value))
		{
			value = null;
		}
		if (!_dict._storage.TryGetName(out var value2))
		{
			value2 = null;
		}
		string text = value as string;
		string arg = (value2 as string) ?? "?";
		if (text == null)
		{
			return $"<module '{arg}' (built-in)>";
		}
		return $"<module '{arg}' from '{text}'>";
	}

	[SpecialName]
	[PropertyMethod]
	public PythonDictionary Get__dict__()
	{
		return _dict;
	}

	[SpecialName]
	[PropertyMethod]
	public void Set__dict__(object value)
	{
		throw PythonOps.TypeError("readonly attribute");
	}

	[SpecialName]
	[PropertyMethod]
	public void Delete__dict__()
	{
		throw PythonOps.TypeError("readonly attribute");
	}

	[PythonHidden]
	public DynamicMetaObject GetMetaObject(Expression parameter)
	{
		return new MetaModule(this, parameter);
	}

	internal string GetFile()
	{
		if (_dict.TryGetValue("__file__", out var value))
		{
			return value as string;
		}
		return null;
	}

	internal string GetName()
	{
		if (_dict._storage.TryGetName(out var value))
		{
			return value as string;
		}
		return null;
	}

	IList<object> IPythonMembersList.GetMemberNames(CodeContext context)
	{
		return new List<object>(__dict__.Keys);
	}

	IList<string> IMembersList.GetMemberNames()
	{
		List<string> list = new List<string>(__dict__.Keys.Count);
		foreach (object key in __dict__.Keys)
		{
			if (key is string item)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
