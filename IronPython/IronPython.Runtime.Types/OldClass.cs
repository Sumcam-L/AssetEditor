using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

[Serializable]
[DebuggerDisplay("old-style class {Name}")]
[PythonType("classobj")]
[DontMapGetMemberNamesToDir]
[DebuggerTypeProxy(typeof(OldClassDebugView))]
public sealed class OldClass : ICustomTypeDescriptor, ICodeFormattable, IDynamicMetaObjectProvider, IPythonMembersList, IMembersList, IWeakReferenceable
{
	internal class OldClassDebugView
	{
		private readonly OldClass _class;

		public List<OldClass> __bases__ => _class.BaseClasses;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		internal List<ObjectDebugView> Members
		{
			get
			{
				List<ObjectDebugView> list = new List<ObjectDebugView>();
				if (_class._dict != null)
				{
					foreach (KeyValuePair<object, object> item in _class._dict)
					{
						list.Add(new ObjectDebugView(item.Key, item.Value));
					}
				}
				return list;
			}
		}

		public OldClassDebugView(OldClass klass)
		{
			_class = klass;
		}
	}

	[NonSerialized]
	private List<OldClass> _bases;

	private PythonType _type;

	internal PythonDictionary _dict;

	private int _attrs;

	internal object _name;

	private int _optimizedInstanceNamesVersion;

	private string[] _optimizedInstanceNames;

	private WeakRefTracker _tracker;

	public static string __doc__ = "classobj(name, bases, dict)";

	internal string[] OptimizedInstanceNames => _optimizedInstanceNames;

	internal int OptimizedInstanceNamesVersion => _optimizedInstanceNamesVersion;

	internal string Name => _name.ToString();

	internal string FullName => _dict["__module__"].ToString() + '.' + _name;

	internal List<OldClass> BaseClasses => _bases;

	internal bool HasFinalizer
	{
		get
		{
			return (_attrs & 1) != 0;
		}
		set
		{
			int attrs;
			int value2;
			do
			{
				attrs = _attrs;
				value2 = (value ? (attrs | 1) : (attrs & -2));
			}
			while (Interlocked.CompareExchange(ref _attrs, value2, attrs) != attrs);
		}
	}

	internal bool HasSetAttr
	{
		get
		{
			return (_attrs & 2) != 0;
		}
		set
		{
			int attrs;
			int value2;
			do
			{
				attrs = _attrs;
				value2 = (value ? (attrs | 2) : (attrs & -3));
			}
			while (Interlocked.CompareExchange(ref _attrs, value2, attrs) != attrs);
		}
	}

	internal bool HasDelAttr
	{
		get
		{
			return (_attrs & 4) != 0;
		}
		set
		{
			int attrs;
			int value2;
			do
			{
				attrs = _attrs;
				value2 = (value ? (attrs | 4) : (attrs & -5));
			}
			while (Interlocked.CompareExchange(ref _attrs, value2, attrs) != attrs);
		}
	}

	internal PythonType TypeObject
	{
		get
		{
			if (_type == null)
			{
				Interlocked.CompareExchange(ref _type, new PythonType(this), null);
			}
			return _type;
		}
	}

	public static object __new__(CodeContext context, [NotNull] PythonType cls, string name, PythonTuple bases, PythonDictionary dict)
	{
		if (dict == null)
		{
			throw PythonOps.TypeError("dict must be a dictionary");
		}
		if (cls != TypeCache.OldClass)
		{
			throw PythonOps.TypeError("{0} is not a subtype of classobj", cls.Name);
		}
		if (!dict.ContainsKey("__module__") && context.TryGetGlobalVariable("__name__", out var res))
		{
			dict["__module__"] = res;
		}
		foreach (object basis in bases)
		{
			if (basis is PythonType)
			{
				return PythonOps.MakeClass(context, name, bases._data, string.Empty, dict);
			}
		}
		return new OldClass(name, bases, dict, string.Empty);
	}

	internal OldClass(string name, PythonTuple bases, PythonDictionary dict, string instanceNames)
	{
		_bases = ValidateBases(bases);
		Init(name, dict, instanceNames);
	}

	private void Init(string name, PythonDictionary dict, string instanceNames)
	{
		_name = name;
		InitializeInstanceNames(instanceNames);
		_dict = dict;
		if (!_dict._storage.Contains("__doc__"))
		{
			_dict._storage.Add(ref _dict._storage, "__doc__", null);
		}
		CheckSpecialMethods(_dict);
	}

	private void CheckSpecialMethods(PythonDictionary dict)
	{
		if (dict._storage.Contains("__del__"))
		{
			HasFinalizer = true;
		}
		if (dict._storage.Contains("__setattr__"))
		{
			HasSetAttr = true;
		}
		if (dict._storage.Contains("__delattr__"))
		{
			HasDelAttr = true;
		}
		foreach (OldClass basis in _bases)
		{
			if (basis.HasDelAttr)
			{
				HasDelAttr = true;
			}
			if (basis.HasSetAttr)
			{
				HasSetAttr = true;
			}
			if (basis.HasFinalizer)
			{
				HasFinalizer = true;
			}
		}
	}

	private OldClass(SerializationInfo info, StreamingContext context)
	{
		_bases = (List<OldClass>)info.GetValue("__class__", typeof(List<OldClass>));
		_name = info.GetValue("__name__", typeof(object));
		_dict = new PythonDictionary();
		InitializeInstanceNames("");
		List<object> list = (List<object>)info.GetValue("keys", typeof(List<object>));
		List<object> list2 = (List<object>)info.GetValue("values", typeof(List<object>));
		for (int i = 0; i < list.Count; i++)
		{
			_dict[list[i]] = list2[i];
		}
		if (_dict.has_key("__del__"))
		{
			HasFinalizer = true;
		}
	}

	private void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		ContractUtils.RequiresNotNull(info, "info");
		info.AddValue("__bases__", _bases);
		info.AddValue("__name__", _name);
		List<object> list = new List<object>();
		List<object> list2 = new List<object>();
		foreach (KeyValuePair<object, object> item in _dict._storage.GetItems())
		{
			list.Add(item.Key);
			list2.Add(item.Value);
		}
		info.AddValue("keys", list);
		info.AddValue("values", list2);
	}

	private void InitializeInstanceNames(string instanceNames)
	{
		if (instanceNames.Length == 0)
		{
			_optimizedInstanceNames = ArrayUtils.EmptyStrings;
			_optimizedInstanceNamesVersion = 0;
			return;
		}
		string[] array = instanceNames.Split(',');
		_optimizedInstanceNames = new string[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_optimizedInstanceNames[i] = array[i];
		}
		_optimizedInstanceNamesVersion = CustomInstanceDictionaryStorage.AllocateVersion();
	}

	internal bool TryLookupSlot(string name, out object ret)
	{
		if (_dict._storage.TryGetValue(name, out ret))
		{
			return true;
		}
		foreach (OldClass basis in _bases)
		{
			if (basis.TryLookupSlot(name, out ret))
			{
				return true;
			}
		}
		ret = null;
		return false;
	}

	internal bool TryLookupOneSlot(PythonType lookingType, string name, out object ret)
	{
		if (_dict._storage.TryGetValue(name, out ret))
		{
			ret = GetOldStyleDescriptor(TypeObject.Context.SharedContext, ret, null, lookingType);
			return true;
		}
		return false;
	}

	internal object GetOldStyleDescriptor(CodeContext context, object self, object instance, object type)
	{
		if (self is PythonTypeSlot pythonTypeSlot && pythonTypeSlot.TryGetValue(context, instance, TypeObject, out var value))
		{
			return value;
		}
		return PythonOps.GetUserDescriptor(self, instance, type);
	}

	internal static object GetOldStyleDescriptor(CodeContext context, object self, object instance, PythonType type)
	{
		if (self is PythonTypeSlot pythonTypeSlot && pythonTypeSlot.TryGetValue(context, instance, type, out var value))
		{
			return value;
		}
		return PythonOps.GetUserDescriptor(self, instance, type);
	}

	public override string ToString()
	{
		return FullName;
	}

	[SpecialName]
	public object Call(CodeContext context, [NotNull] params object[] argsø)
	{
		OldInstance oldInstance = new OldInstance(context, this);
		if (TryLookupSlot("__init__", out var ret))
		{
			PythonOps.CallWithContext(context, GetOldStyleDescriptor(context, ret, oldInstance, this), argsø);
		}
		else if (argsø.Length > 0)
		{
			MakeCallError();
		}
		return oldInstance;
	}

	[SpecialName]
	public object Call(CodeContext context, [ParamDictionary] IDictionary<object, object> dictø, [NotNull] params object[] argsø)
	{
		OldInstance oldInstance = new OldInstance(context, this);
		if (PythonOps.TryGetBoundAttr(oldInstance, "__init__", out var ret))
		{
			PythonCalls.CallWithKeywordArgs(context, ret, argsø, dictø);
		}
		else if (dictø.Count > 0 || argsø.Length > 0)
		{
			MakeCallError();
		}
		return oldInstance;
	}

	private List<OldClass> ValidateBases(object value)
	{
		if (!(value is PythonTuple pythonTuple))
		{
			throw PythonOps.TypeError("__bases__ must be a tuple object");
		}
		List<OldClass> list = new List<OldClass>(pythonTuple.__len__());
		foreach (object item in pythonTuple)
		{
			if (!(item is OldClass oldClass))
			{
				throw PythonOps.TypeError("__bases__ items must be classes (got {0})", PythonTypeOps.GetName(item));
			}
			if (oldClass.IsSubclassOf(this))
			{
				throw PythonOps.TypeError("a __bases__ item causes an inheritance cycle");
			}
			list.Add(oldClass);
		}
		return list;
	}

	internal object GetMember(CodeContext context, string name)
	{
		if (!TryGetBoundCustomMember(context, name, out var value))
		{
			throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'", Name, name);
		}
		return value;
	}

	internal bool TryGetBoundCustomMember(CodeContext context, string name, out object value)
	{
		switch (name)
		{
		case "__bases__":
			value = PythonTuple.Make(_bases);
			return true;
		case "__name__":
			value = _name;
			return true;
		case "__dict__":
		{
			bool hasDelAttr = (HasSetAttr = true);
			HasDelAttr = hasDelAttr;
			value = _dict;
			return true;
		}
		default:
			if (TryLookupSlot(name, out value))
			{
				value = GetOldStyleDescriptor(context, value, null, this);
				return true;
			}
			return false;
		}
	}

	internal bool DeleteCustomMember(CodeContext context, string name)
	{
		if (!_dict._storage.Remove(ref _dict._storage, name))
		{
			throw PythonOps.AttributeError("{0} is not a valid attribute", name);
		}
		if (name == "__del__")
		{
			HasFinalizer = false;
		}
		if (name == "__setattr__")
		{
			HasSetAttr = false;
		}
		if (name == "__delattr__")
		{
			HasDelAttr = false;
		}
		return true;
	}

	internal static void RecurseAttrHierarchy(OldClass oc, IDictionary<object, object> attrs)
	{
		foreach (KeyValuePair<object, object> item in oc._dict._storage.GetItems())
		{
			if (!attrs.ContainsKey(item.Key))
			{
				attrs.Add(item.Key, item.Key);
			}
		}
		if (oc._bases.Count == 0)
		{
			return;
		}
		foreach (OldClass basis in oc._bases)
		{
			RecurseAttrHierarchy(basis, attrs);
		}
	}

	IList<string> IMembersList.GetMemberNames()
	{
		return PythonOps.GetStringMemberList(this);
	}

	IList<object> IPythonMembersList.GetMemberNames(CodeContext context)
	{
		PythonDictionary pythonDictionary = new PythonDictionary(_dict);
		RecurseAttrHierarchy(this, pythonDictionary);
		return PythonOps.MakeListFromSequence(pythonDictionary);
	}

	internal bool IsSubclassOf(object other)
	{
		if (this == other)
		{
			return true;
		}
		if (!(other is OldClass))
		{
			return false;
		}
		List<OldClass> bases = _bases;
		foreach (OldClass item in bases)
		{
			if (item.IsSubclassOf(other))
			{
				return true;
			}
		}
		return false;
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return CustomTypeDescHelpers.GetAttributes(this);
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return CustomTypeDescHelpers.GetClassName(this);
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return CustomTypeDescHelpers.GetComponentName(this);
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return CustomTypeDescHelpers.GetConverter(this);
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return CustomTypeDescHelpers.GetDefaultEvent(this);
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return CustomTypeDescHelpers.GetDefaultProperty(this);
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return CustomTypeDescHelpers.GetEditor(this, editorBaseType);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return CustomTypeDescHelpers.GetEvents(attributes);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return CustomTypeDescHelpers.GetEvents(this);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return CustomTypeDescHelpers.GetProperties(attributes);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return CustomTypeDescHelpers.GetProperties(this);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return CustomTypeDescHelpers.GetPropertyOwner(this, pd);
	}

	public string __repr__(CodeContext context)
	{
		return $"<class {FullName} at {PythonOps.HexId(this)}>";
	}

	internal bool TryLookupInit(object inst, out object ret)
	{
		if (TryLookupSlot("__init__", out ret))
		{
			ret = GetOldStyleDescriptor(DefaultContext.Default, ret, inst, this);
			return true;
		}
		return false;
	}

	internal static object MakeCallError()
	{
		throw PythonOps.TypeError("this constructor takes no arguments");
	}

	internal void SetBases(object value)
	{
		_bases = ValidateBases(value);
	}

	internal void SetName(object value)
	{
		if (!(value is string name))
		{
			throw PythonOps.TypeError("TypeError: __name__ must be a string object");
		}
		_name = name;
	}

	internal void SetDictionary(object value)
	{
		if (!(value is PythonDictionary dict))
		{
			throw PythonOps.TypeError("__dict__ must be set to dictionary");
		}
		_dict = dict;
	}

	internal void SetNameHelper(string name, object value)
	{
		_dict._storage.Add(ref _dict._storage, name, value);
		switch (name)
		{
		case "__del__":
			HasFinalizer = true;
			break;
		case "__setattr__":
			HasSetAttr = true;
			break;
		case "__delattr__":
			HasDelAttr = true;
			break;
		}
	}

	internal object LookupValue(CodeContext context, string name)
	{
		if (TryLookupValue(context, name, out var value))
		{
			return value;
		}
		throw PythonOps.AttributeErrorForMissingAttribute(this, name);
	}

	internal bool TryLookupValue(CodeContext context, string name, out object value)
	{
		if (TryLookupSlot(name, out value))
		{
			value = GetOldStyleDescriptor(context, value, null, this);
			return true;
		}
		return false;
	}

	internal void DictionaryIsPublic()
	{
		HasDelAttr = true;
		HasSetAttr = true;
	}

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		return new MetaOldClass(parameter, BindingRestrictions.Empty, this);
	}

	WeakRefTracker IWeakReferenceable.GetWeakRef()
	{
		return _tracker;
	}

	bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
	{
		_tracker = value;
		return true;
	}

	void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
	{
		_tracker = value;
	}
}
