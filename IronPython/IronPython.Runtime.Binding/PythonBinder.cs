using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Threading;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonBinder : DefaultBinder
{
	private class ExtensionTypeInfo
	{
		public Type ExtensionType;

		public string PythonName;

		public ExtensionTypeInfo(Type extensionType, string pythonName)
		{
			ExtensionType = extensionType;
			PythonName = pythonName;
		}
	}

	private class SlotCache
	{
		private class CachedInfoKey : IEquatable<CachedInfoKey>
		{
			public readonly Type Type;

			public readonly bool IsGetMember;

			public CachedInfoKey(Type type, bool isGetMember)
			{
				Type = type;
				IsGetMember = isGetMember;
			}

			public bool Equals(CachedInfoKey other)
			{
				if (other.Type == Type)
				{
					return other.IsGetMember == IsGetMember;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is CachedInfoKey other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return Type.GetHashCode() ^ (IsGetMember ? (-1) : 0);
			}
		}

		private class SlotCacheInfo
		{
			public Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>> Members;

			public bool ResolvedAll;

			public SlotCacheInfo()
			{
				Members = new Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>>(StringComparer.Ordinal);
			}

			public bool TryGetSlot(string name, out PythonTypeSlot slot)
			{
				if (Members.TryGetValue(name, out var value))
				{
					slot = value.Key;
					return true;
				}
				slot = null;
				return false;
			}

			public bool TryGetMember(string name, out MemberGroup group)
			{
				if (Members.TryGetValue(name, out var value))
				{
					group = value.Value;
					return true;
				}
				group = MemberGroup.EmptyGroup;
				return false;
			}

			public IEnumerable<KeyValuePair<string, PythonTypeSlot>> GetAllSlots()
			{
				foreach (KeyValuePair<string, KeyValuePair<PythonTypeSlot, MemberGroup>> kvp in Members)
				{
					KeyValuePair<string, KeyValuePair<PythonTypeSlot, MemberGroup>> keyValuePair = kvp;
					string key = keyValuePair.Key;
					KeyValuePair<string, KeyValuePair<PythonTypeSlot, MemberGroup>> keyValuePair2 = kvp;
					yield return new KeyValuePair<string, PythonTypeSlot>(key, keyValuePair2.Value.Key);
				}
			}
		}

		private Dictionary<CachedInfoKey, SlotCacheInfo> _cachedInfos;

		public void CacheSlot(Type type, bool isGetMember, string name, PythonTypeSlot slot, MemberGroup memberGroup)
		{
			EnsureInfo();
			lock (_cachedInfos)
			{
				SlotCacheInfo slotForType = GetSlotForType(type, isGetMember);
				if (!slotForType.ResolvedAll || slot != null || memberGroup.Count != 0)
				{
					slotForType.Members[name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(slot, memberGroup);
				}
			}
		}

		public bool TryGetCachedSlot(Type type, bool isGetMember, string name, out PythonTypeSlot slot)
		{
			if (_cachedInfos != null)
			{
				lock (_cachedInfos)
				{
					if (_cachedInfos.TryGetValue(new CachedInfoKey(type, isGetMember), out var value) && (value.TryGetSlot(name, out slot) || value.ResolvedAll))
					{
						return true;
					}
				}
			}
			slot = null;
			return false;
		}

		public bool TryGetCachedMember(Type type, string name, bool getMemberAction, out MemberGroup group)
		{
			if (_cachedInfos != null)
			{
				lock (_cachedInfos)
				{
					if (_cachedInfos.TryGetValue(new CachedInfoKey(type, getMemberAction), out var value) && (value.TryGetMember(name, out group) || (getMemberAction && value.ResolvedAll)))
					{
						return true;
					}
				}
			}
			group = MemberGroup.EmptyGroup;
			return false;
		}

		public bool IsFullyCached(Type type, bool isGetMember)
		{
			if (_cachedInfos != null)
			{
				lock (_cachedInfos)
				{
					if (_cachedInfos.TryGetValue(new CachedInfoKey(type, isGetMember), out var value))
					{
						return value.ResolvedAll;
					}
				}
			}
			return false;
		}

		public void CacheAll(Type type, bool isGetMember, Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>> members)
		{
			EnsureInfo();
			lock (_cachedInfos)
			{
				SlotCacheInfo slotForType = GetSlotForType(type, isGetMember);
				slotForType.Members = members;
				slotForType.ResolvedAll = true;
			}
		}

		public IEnumerable<KeyValuePair<string, PythonTypeSlot>> GetAllMembers(Type type, bool isGetMember)
		{
			SlotCacheInfo info = GetSlotForType(type, isGetMember);
			foreach (KeyValuePair<string, PythonTypeSlot> slot in info.GetAllSlots())
			{
				KeyValuePair<string, PythonTypeSlot> keyValuePair = slot;
				if (keyValuePair.Value != null)
				{
					yield return slot;
				}
			}
		}

		private SlotCacheInfo GetSlotForType(Type type, bool isGetMember)
		{
			CachedInfoKey key = new CachedInfoKey(type, isGetMember);
			if (!_cachedInfos.TryGetValue(key, out var value))
			{
				value = (_cachedInfos[key] = new SlotCacheInfo());
			}
			return value;
		}

		private void EnsureInfo()
		{
			if (_cachedInfos == null)
			{
				Interlocked.CompareExchange(ref _cachedInfos, new Dictionary<CachedInfoKey, SlotCacheInfo>(), null);
			}
		}
	}

	private PythonContext _context;

	private SlotCache _typeMembers = new SlotCache();

	private SlotCache _resolvedMembers = new SlotCache();

	private Dictionary<Type, IList<Type>> _dlrExtensionTypes;

	private bool _registeredInterfaceExtensions;

	private static readonly Dictionary<Type, ExtensionTypeInfo> _sysTypes = MakeSystemTypes();

	public override bool PrivateBinding => _context.DomainManager.Configuration.PrivateBinding;

	internal ScriptDomainManager DomainManager => _context.DomainManager;

	internal PythonContext Context => _context;

	public DynamicMetaObject Create(CallSignature signature, DynamicMetaObject target, DynamicMetaObject[] args, Expression contextExpression)
	{
		Type targetType = GetTargetType(target.Value);
		if (targetType != null)
		{
			if (typeof(Delegate).IsAssignableFrom(targetType) && args.Length == 1)
			{
				return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("GetDelegate"), contextExpression, Utils.Convert(args[0].Expression, typeof(object)), Expression.Constant(targetType)), target.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value)));
			}
			return CallMethod(new PythonOverloadResolver(this, args, signature, contextExpression), CompilerHelpers.GetConstructors(targetType, PrivateBinding), target.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value)));
		}
		return null;
	}

	private static Type GetTargetType(object target)
	{
		if (target is TypeTracker typeTracker)
		{
			return typeTracker.Type;
		}
		return target as Type;
	}

	public PythonBinder(PythonContext pythonContext, CodeContext context)
	{
		ContractUtils.RequiresNotNull(pythonContext, "pythonContext");
		_dlrExtensionTypes = MakeExtensionTypes();
		_context = pythonContext;
		if (context == null)
		{
			return;
		}
		context.LanguageContext.DomainManager.AssemblyLoaded += DomainManager_AssemblyLoaded;
		foreach (Assembly loadedAssembly in pythonContext.DomainManager.GetLoadedAssemblyList())
		{
			DomainManager_AssemblyLoaded(this, new AssemblyLoadedEventArgs(loadedAssembly));
		}
	}

	public PythonBinder(PythonBinder binder)
	{
		_context = binder._context;
		_typeMembers = binder._typeMembers;
		_resolvedMembers = binder._resolvedMembers;
		_dlrExtensionTypes = binder._dlrExtensionTypes;
		_registeredInterfaceExtensions = binder._registeredInterfaceExtensions;
	}

	public override Expression ConvertExpression(Expression expr, Type toType, ConversionResultKind kind, OverloadResolverFactory factory)
	{
		ContractUtils.RequiresNotNull(expr, "expr");
		ContractUtils.RequiresNotNull(toType, "toType");
		Type type = expr.Type;
		if (toType == typeof(object))
		{
			if (type.IsValueType())
			{
				return Utils.Convert(expr, toType);
			}
			return expr;
		}
		if (toType.IsAssignableFrom(type))
		{
			return expr;
		}
		Type type2 = (Context.Binder.PrivateBinding ? toType : CompilerHelpers.GetVisibleType(toType));
		if (type == typeof(PythonType) && type2 == typeof(Type))
		{
			return Utils.Convert(expr, type2);
		}
		return Binders.Convert(((PythonOverloadResolverFactory)factory)._codeContext, _context, type2, (!(type2 == typeof(char))) ? kind : ConversionResultKind.ImplicitCast, expr);
	}

	internal static MethodInfo GetGenericConvertMethod(Type toType)
	{
		if (toType.IsValueType())
		{
			if (toType.IsGenericType() && toType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				return typeof(Converter).GetMethod("ConvertToNullableType");
			}
			return typeof(Converter).GetMethod("ConvertToValueType");
		}
		return typeof(Converter).GetMethod("ConvertToReferenceType");
	}

	internal static MethodInfo GetFastConvertMethod(Type toType)
	{
		if (toType == typeof(char))
		{
			return typeof(Converter).GetMethod("ConvertToChar");
		}
		if (toType == typeof(int))
		{
			return typeof(Converter).GetMethod("ConvertToInt32");
		}
		if (toType == typeof(string))
		{
			return typeof(Converter).GetMethod("ConvertToString");
		}
		if (toType == typeof(long))
		{
			return typeof(Converter).GetMethod("ConvertToInt64");
		}
		if (toType == typeof(double))
		{
			return typeof(Converter).GetMethod("ConvertToDouble");
		}
		if (toType == typeof(bool))
		{
			return typeof(Converter).GetMethod("ConvertToBoolean");
		}
		if (toType == typeof(BigInteger))
		{
			return typeof(Converter).GetMethod("ConvertToBigInteger");
		}
		if (toType == typeof(Complex))
		{
			return typeof(Converter).GetMethod("ConvertToComplex");
		}
		if (toType == typeof(IEnumerable))
		{
			return typeof(Converter).GetMethod("ConvertToIEnumerable");
		}
		if (toType == typeof(float))
		{
			return typeof(Converter).GetMethod("ConvertToSingle");
		}
		if (toType == typeof(byte))
		{
			return typeof(Converter).GetMethod("ConvertToByte");
		}
		if (toType == typeof(sbyte))
		{
			return typeof(Converter).GetMethod("ConvertToSByte");
		}
		if (toType == typeof(short))
		{
			return typeof(Converter).GetMethod("ConvertToInt16");
		}
		if (toType == typeof(uint))
		{
			return typeof(Converter).GetMethod("ConvertToUInt32");
		}
		if (toType == typeof(ulong))
		{
			return typeof(Converter).GetMethod("ConvertToUInt64");
		}
		if (toType == typeof(ushort))
		{
			return typeof(Converter).GetMethod("ConvertToUInt16");
		}
		if (toType == typeof(Type))
		{
			return typeof(Converter).GetMethod("ConvertToType");
		}
		return null;
	}

	public override object Convert(object obj, Type toType)
	{
		return Converter.Convert(obj, toType);
	}

	public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level)
	{
		return Converter.CanConvertFrom(fromType, toType, level);
	}

	public override Candidate PreferConvert(Type t1, Type t2)
	{
		return Converter.PreferConvert(t1, t2);
	}

	public override ErrorInfo MakeSetValueTypeFieldError(FieldTracker field, DynamicMetaObject instance, DynamicMetaObject value)
	{
		return ErrorInfo.FromValueNoError(Expression.Block(Expression.Call(typeof(PythonOps).GetMethod("Warn"), Expression.Constant(_context.SharedContext), Expression.Constant(PythonExceptions.RuntimeWarning), Expression.Constant("Setting field {0} on value type {1} may result in updating a copy.  Use {1}.{0}.SetValue(instance, value) if this is safe.  For more information help({1}.{0}.SetValue)."), Expression.Constant(new object[2]
		{
			field.Name,
			field.DeclaringType.Name
		})), Expression.Assign(Expression.Field(Utils.Convert(instance.Expression, field.DeclaringType), field.Field), ConvertExpression(value.Expression, field.FieldType, ConversionResultKind.ExplicitCast, new PythonOverloadResolverFactory(this, Expression.Constant(_context.SharedContext))))));
	}

	public override ErrorInfo MakeConversionError(Type toType, Expression value)
	{
		return ErrorInfo.FromException(Expression.Call(typeof(PythonOps).GetMethod("TypeErrorForTypeMismatch"), Utils.Constant(DynamicHelpers.GetPythonTypeFromType(toType).Name), Utils.Convert(value, typeof(object))));
	}

	public override ErrorInfo MakeNonPublicMemberGetError(OverloadResolverFactory resolverFactory, MemberTracker member, Type type, DynamicMetaObject instance)
	{
		if (PrivateBinding)
		{
			return base.MakeNonPublicMemberGetError(resolverFactory, member, type, instance);
		}
		return ErrorInfo.FromValue(BindingHelpers.TypeErrorForProtectedMember(type, member.Name));
	}

	public override ErrorInfo MakeStaticAssignFromDerivedTypeError(Type accessingType, DynamicMetaObject instance, MemberTracker info, DynamicMetaObject assignedValue, OverloadResolverFactory factory)
	{
		return MakeMissingMemberError(accessingType, instance, info.Name);
	}

	public override ErrorInfo MakeStaticPropertyInstanceAccessError(PropertyTracker tracker, bool isAssignment, IList<DynamicMetaObject> parameters)
	{
		ContractUtils.RequiresNotNull(tracker, "tracker");
		ContractUtils.RequiresNotNull(parameters, "parameters");
		if (isAssignment)
		{
			return ErrorInfo.FromException(Expression.Call(typeof(PythonOps).GetMethod("StaticAssignmentFromInstanceError"), Utils.Constant(tracker), Utils.Constant(isAssignment)));
		}
		return ErrorInfo.FromValue(Expression.Property(null, tracker.GetGetMethod(DomainManager.Configuration.PrivateBinding)));
	}

	public override string GetTypeName(Type t)
	{
		return DynamicHelpers.GetPythonTypeFromType(t).Name;
	}

	public override MemberGroup GetMember(MemberRequestKind actionKind, Type type, string name)
	{
		if (!_resolvedMembers.TryGetCachedMember(type, name, actionKind == MemberRequestKind.Get, out var group))
		{
			group = IronPython.Runtime.Types.TypeInfo.GetMemberAll(this, actionKind, type, name);
			_resolvedMembers.CacheSlot(type, actionKind == MemberRequestKind.Get, name, PythonTypeOps.GetSlot(group, name, PrivateBinding), group);
		}
		return group ?? MemberGroup.EmptyGroup;
	}

	public override ErrorInfo MakeEventValidation(MemberGroup members, DynamicMetaObject eventObject, DynamicMetaObject value, OverloadResolverFactory factory)
	{
		EventTracker tracker = (EventTracker)members[0];
		return ErrorInfo.FromValueNoError(Expression.Block(Expression.Call(typeof(PythonOps).GetMethod("SlotTrySetValue"), ((PythonOverloadResolverFactory)factory)._codeContext, Utils.Constant(PythonTypeOps.GetReflectedEvent(tracker)), (eventObject != null) ? Utils.Convert(eventObject.Expression, typeof(object)) : Utils.Constant(null), Utils.Constant(null, typeof(PythonType)), Utils.Convert(value.Expression, typeof(object))), Expression.Constant(null)));
	}

	public override ErrorInfo MakeMissingMemberError(Type type, DynamicMetaObject self, string name)
	{
		string arg = ((!typeof(TypeTracker).IsAssignableFrom(type)) ? NameConverter.GetTypeName(type) : "type");
		return ErrorInfo.FromException(Expression.New(typeof(MissingMemberException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant($"'{arg}' object has no attribute '{name}'")));
	}

	public override ErrorInfo MakeMissingMemberErrorForAssign(Type type, DynamicMetaObject self, string name)
	{
		if (self != null)
		{
			return MakeMissingMemberError(type, self, name);
		}
		return ErrorInfo.FromException(Expression.New(typeof(TypeErrorException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant($"can't set attributes of built-in/extension type '{NameConverter.GetTypeName(type)}'")));
	}

	public override ErrorInfo MakeMissingMemberErrorForAssignReadOnlyProperty(Type type, DynamicMetaObject self, string name)
	{
		return ErrorInfo.FromException(Expression.New(typeof(MissingMemberException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant($"can't assign to read-only property {name} of type '{NameConverter.GetTypeName(type)}'")));
	}

	public override ErrorInfo MakeMissingMemberErrorForDelete(Type type, DynamicMetaObject self, string name)
	{
		return MakeMissingMemberErrorForAssign(type, self, name);
	}

	public override ErrorInfo MakeReadOnlyMemberError(Type type, string name)
	{
		return ErrorInfo.FromException(Expression.New(typeof(MissingMemberException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant($"attribute '{name}' of '{NameConverter.GetTypeName(type)}' object is read-only")));
	}

	public override ErrorInfo MakeUndeletableMemberError(Type type, string name)
	{
		return ErrorInfo.FromException(Expression.New(typeof(MissingMemberException).GetConstructor(new Type[1] { typeof(string) }), Utils.Constant($"cannot delete attribute '{name}' of builtin type '{NameConverter.GetTypeName(type)}'")));
	}

	internal IList<Type> GetExtensionTypesInternal(Type t)
	{
		List<Type> list = new List<Type>(base.GetExtensionTypes(t));
		AddExtensionTypes(t, list);
		return list.ToArray();
	}

	public override bool IncludeExtensionMember(MemberInfo member)
	{
		return !member.DeclaringType.IsDefined(typeof(PythonHiddenBaseClassAttribute), inherit: false);
	}

	public override IList<Type> GetExtensionTypes(Type t)
	{
		List<Type> list = new List<Type>();
		list.Add(t);
		list.AddRange(base.GetExtensionTypes(t));
		AddExtensionTypes(t, list);
		return list;
	}

	private void AddExtensionTypes(Type t, List<Type> list)
	{
		if (_sysTypes.TryGetValue(t, out var value))
		{
			list.Add(value.ExtensionType);
		}
		lock (_dlrExtensionTypes)
		{
			if (_dlrExtensionTypes.TryGetValue(t, out var value2))
			{
				list.AddRange(value2);
			}
			if (_registeredInterfaceExtensions)
			{
				Type[] interfaces = t.GetInterfaces();
				foreach (Type key in interfaces)
				{
					if (_dlrExtensionTypes.TryGetValue(key, out var value3))
					{
						list.AddRange(value3);
					}
				}
			}
			if (!t.IsGenericType)
			{
				return;
			}
			Type genericTypeDefinition = t.GetGenericTypeDefinition();
			Type[] genericArguments = t.GetGenericArguments();
			if (!_dlrExtensionTypes.TryGetValue(genericTypeDefinition, out value2))
			{
				return;
			}
			foreach (Type item in value2)
			{
				list.Add(item.MakeGenericType(genericArguments));
			}
		}
	}

	public bool HasExtensionTypes(Type t)
	{
		return _dlrExtensionTypes.ContainsKey(t);
	}

	public override DynamicMetaObject ReturnMemberTracker(Type type, MemberTracker memberTracker)
	{
		DynamicMetaObject dynamicMetaObject = ReturnMemberTracker(type, memberTracker, PrivateBinding);
		return dynamicMetaObject ?? base.ReturnMemberTracker(type, memberTracker);
	}

	private static DynamicMetaObject ReturnMemberTracker(Type type, MemberTracker memberTracker, bool privateBinding)
	{
		switch (memberTracker.MemberType)
		{
		case TrackerTypes.TypeGroup:
			return new DynamicMetaObject(Utils.Constant(memberTracker), BindingRestrictions.Empty, memberTracker);
		case TrackerTypes.Type:
			return ReturnTypeTracker((TypeTracker)memberTracker);
		case TrackerTypes.Bound:
			return new DynamicMetaObject(ReturnBoundTracker((BoundMemberTracker)memberTracker, privateBinding), BindingRestrictions.Empty);
		case TrackerTypes.Property:
			return new DynamicMetaObject(ReturnPropertyTracker((PropertyTracker)memberTracker, privateBinding), BindingRestrictions.Empty);
		case TrackerTypes.Event:
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MakeBoundEvent"), Utils.Constant(PythonTypeOps.GetReflectedEvent((EventTracker)memberTracker)), Utils.Constant(null), Utils.Constant(type)), BindingRestrictions.Empty);
		case TrackerTypes.Field:
			return new DynamicMetaObject(ReturnFieldTracker((FieldTracker)memberTracker), BindingRestrictions.Empty);
		case TrackerTypes.MethodGroup:
			return new DynamicMetaObject(ReturnMethodGroup((MethodGroup)memberTracker), BindingRestrictions.Empty);
		case TrackerTypes.Constructor:
		{
			MethodBase[] constructors = CompilerHelpers.GetConstructors(type, privateBinding, includeProtected: true);
			object value = ((!PythonTypeOps.IsDefaultNew(constructors)) ? PythonTypeOps.GetConstructor(type, InstanceOps.NonDefaultNewInst, constructors) : ((!IsPythonType(type)) ? InstanceOps.NewCls : InstanceOps.New));
			return new DynamicMetaObject(Utils.Constant(value), BindingRestrictions.Empty, value);
		}
		case TrackerTypes.Custom:
			return new DynamicMetaObject(Utils.Constant(((PythonCustomTracker)memberTracker).GetSlot(), typeof(PythonTypeSlot)), BindingRestrictions.Empty, ((PythonCustomTracker)memberTracker).GetSlot());
		default:
			return null;
		}
	}

	public static PythonBinder GetBinder(CodeContext context)
	{
		return PythonContext.GetContext(context).Binder;
	}

	public bool TryLookupSlot(CodeContext context, PythonType type, string name, out PythonTypeSlot slot)
	{
		return TryLookupProtectedSlot(context, type, name, out slot);
	}

	internal bool TryLookupProtectedSlot(CodeContext context, PythonType type, string name, out PythonTypeSlot slot)
	{
		Type underlyingSystemType = type.UnderlyingSystemType;
		if (!_typeMembers.TryGetCachedSlot(underlyingSystemType, isGetMember: true, name, out slot))
		{
			MemberGroup member = IronPython.Runtime.Types.TypeInfo.GetMember(this, MemberRequestKind.Get, underlyingSystemType, name);
			slot = PythonTypeOps.GetSlot(member, name, PrivateBinding);
			_typeMembers.CacheSlot(underlyingSystemType, isGetMember: true, name, slot, member);
		}
		if (slot != null && (slot.IsAlwaysVisible || PythonOps.IsClsVisible(context)))
		{
			return true;
		}
		slot = null;
		return false;
	}

	public bool TryResolveSlot(CodeContext context, PythonType type, PythonType owner, string name, out PythonTypeSlot slot)
	{
		Type underlyingSystemType = type.UnderlyingSystemType;
		if (!_resolvedMembers.TryGetCachedSlot(underlyingSystemType, isGetMember: true, name, out slot))
		{
			MemberGroup memberAll = IronPython.Runtime.Types.TypeInfo.GetMemberAll(this, MemberRequestKind.Get, underlyingSystemType, name);
			slot = PythonTypeOps.GetSlot(memberAll, name, PrivateBinding);
			_resolvedMembers.CacheSlot(underlyingSystemType, isGetMember: true, name, slot, memberAll);
		}
		if (slot != null && (slot.IsAlwaysVisible || PythonOps.IsClsVisible(context)))
		{
			return true;
		}
		slot = null;
		return false;
	}

	public void LookupMembers(CodeContext context, PythonType type, PythonDictionary memberNames)
	{
		if (!_typeMembers.IsFullyCached(type.UnderlyingSystemType, isGetMember: true))
		{
			Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>> dictionary = new Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>>();
			foreach (ResolvedMember member in IronPython.Runtime.Types.TypeInfo.GetMembers(this, MemberRequestKind.Get, type.UnderlyingSystemType))
			{
				if (!dictionary.ContainsKey(member.Name))
				{
					dictionary[member.Name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(PythonTypeOps.GetSlot(member.Member, member.Name, PrivateBinding), member.Member);
				}
			}
			_typeMembers.CacheAll(type.UnderlyingSystemType, isGetMember: true, dictionary);
		}
		foreach (KeyValuePair<string, PythonTypeSlot> allMember in _typeMembers.GetAllMembers(type.UnderlyingSystemType, isGetMember: true))
		{
			PythonTypeSlot value = allMember.Value;
			string key = allMember.Key;
			if (value.IsAlwaysVisible || PythonOps.IsClsVisible(context))
			{
				memberNames[key] = value;
			}
		}
	}

	public void ResolveMemberNames(CodeContext context, PythonType type, PythonType owner, Dictionary<string, string> memberNames)
	{
		if (!_resolvedMembers.IsFullyCached(type.UnderlyingSystemType, isGetMember: true))
		{
			Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>> dictionary = new Dictionary<string, KeyValuePair<PythonTypeSlot, MemberGroup>>();
			foreach (ResolvedMember item in IronPython.Runtime.Types.TypeInfo.GetMembersAll(this, MemberRequestKind.Get, type.UnderlyingSystemType))
			{
				if (!dictionary.ContainsKey(item.Name))
				{
					dictionary[item.Name] = new KeyValuePair<PythonTypeSlot, MemberGroup>(PythonTypeOps.GetSlot(item.Member, item.Name, PrivateBinding), item.Member);
				}
			}
			_resolvedMembers.CacheAll(type.UnderlyingSystemType, isGetMember: true, dictionary);
		}
		foreach (KeyValuePair<string, PythonTypeSlot> allMember in _resolvedMembers.GetAllMembers(type.UnderlyingSystemType, isGetMember: true))
		{
			PythonTypeSlot value = allMember.Value;
			string key = allMember.Key;
			if (value.IsAlwaysVisible || PythonOps.IsClsVisible(context))
			{
				memberNames[key] = key;
			}
		}
	}

	private static Expression ReturnFieldTracker(FieldTracker fieldTracker)
	{
		return Utils.Constant(PythonTypeOps.GetReflectedField(fieldTracker.Field));
	}

	private static Expression ReturnMethodGroup(MethodGroup methodGroup)
	{
		return Utils.Constant(PythonTypeOps.GetFinalSlotForFunction(GetBuiltinFunction(methodGroup)));
	}

	private static Expression ReturnBoundTracker(BoundMemberTracker boundMemberTracker, bool privateBinding)
	{
		MemberTracker boundTo = boundMemberTracker.BoundTo;
		switch (boundTo.MemberType)
		{
		case TrackerTypes.Property:
		{
			PropertyTracker propertyTracker = (PropertyTracker)boundTo;
			return Expression.New(typeof(ReflectedIndexer).GetConstructor(new Type[2]
			{
				typeof(ReflectedIndexer),
				typeof(object)
			}), Utils.Constant(new ReflectedIndexer(((ReflectedPropertyTracker)propertyTracker).Property, NameType.Property, privateBinding)), boundMemberTracker.Instance.Expression);
		}
		case TrackerTypes.Event:
			return Expression.Call(typeof(PythonOps).GetMethod("MakeBoundEvent"), Utils.Constant(PythonTypeOps.GetReflectedEvent((EventTracker)boundMemberTracker.BoundTo)), boundMemberTracker.Instance.Expression, Utils.Constant(boundMemberTracker.DeclaringType));
		case TrackerTypes.MethodGroup:
			return Expression.Call(typeof(PythonOps).GetMethod("MakeBoundBuiltinFunction"), Utils.Constant(GetBuiltinFunction((MethodGroup)boundTo)), Utils.Convert(boundMemberTracker.Instance.Expression, typeof(object)));
		default:
			throw new NotImplementedException();
		}
	}

	private static BuiltinFunction GetBuiltinFunction(MethodGroup mg)
	{
		MethodBase[] array = new MethodBase[mg.Methods.Count];
		for (int i = 0; i < mg.Methods.Count; i++)
		{
			array[i] = mg.Methods[i].Method;
		}
		return PythonTypeOps.GetBuiltinFunction(mg.DeclaringType, mg.Methods[0].Name, (FunctionType)((int)((uint)(PythonTypeOps.GetMethodFunctionType(mg.DeclaringType, array) & ~FunctionType.FunctionMethodMask) | (uint)(mg.ContainsInstance ? 2 : 0)) | (mg.ContainsStatic ? 1 : 0)), mg.GetMethodBases());
	}

	private static Expression ReturnPropertyTracker(PropertyTracker propertyTracker, bool privateBinding)
	{
		return Utils.Constant(PythonTypeOps.GetReflectedProperty(propertyTracker, new MemberGroup(propertyTracker), privateBinding));
	}

	private static DynamicMetaObject ReturnTypeTracker(TypeTracker memberTracker)
	{
		object pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(memberTracker.Type);
		return new DynamicMetaObject(Expression.Constant(pythonTypeFromType), BindingRestrictions.Empty, pythonTypeFromType);
	}

	internal static void AssertNotExtensionType(Type t)
	{
		foreach (ExtensionTypeInfo value in _sysTypes.Values)
		{
			_ = value;
		}
	}

	private static Dictionary<Type, IList<Type>> MakeExtensionTypes()
	{
		Dictionary<Type, IList<Type>> dictionary = new Dictionary<Type, IList<Type>>();
		dictionary[typeof(DBNull)] = new Type[1] { typeof(DBNullOps) };
		dictionary[typeof(List<>)] = new Type[1] { typeof(ListOfTOps<>) };
		dictionary[typeof(Dictionary<, >)] = new Type[1] { typeof(DictionaryOfTOps<, >) };
		dictionary[typeof(Array)] = new Type[1] { typeof(ArrayOps) };
		dictionary[typeof(Assembly)] = new Type[1] { typeof(PythonAssemblyOps) };
		dictionary[typeof(Enum)] = new Type[1] { typeof(EnumOps) };
		dictionary[typeof(Delegate)] = new Type[1] { typeof(DelegateOps) };
		dictionary[typeof(byte)] = new Type[1] { typeof(ByteOps) };
		dictionary[typeof(sbyte)] = new Type[1] { typeof(SByteOps) };
		dictionary[typeof(short)] = new Type[1] { typeof(Int16Ops) };
		dictionary[typeof(ushort)] = new Type[1] { typeof(UInt16Ops) };
		dictionary[typeof(uint)] = new Type[1] { typeof(UInt32Ops) };
		dictionary[typeof(long)] = new Type[1] { typeof(Int64Ops) };
		dictionary[typeof(ulong)] = new Type[1] { typeof(UInt64Ops) };
		dictionary[typeof(char)] = new Type[1] { typeof(CharOps) };
		dictionary[typeof(decimal)] = new Type[1] { typeof(DecimalOps) };
		dictionary[typeof(float)] = new Type[1] { typeof(SingleOps) };
		return dictionary;
	}

	private static Dictionary<Type, ExtensionTypeInfo> MakeSystemTypes()
	{
		Dictionary<Type, ExtensionTypeInfo> dictionary = new Dictionary<Type, ExtensionTypeInfo>();
		dictionary[typeof(object)] = new ExtensionTypeInfo(typeof(ObjectOps), "object");
		dictionary[typeof(string)] = new ExtensionTypeInfo(typeof(StringOps), "str");
		dictionary[typeof(int)] = new ExtensionTypeInfo(typeof(Int32Ops), "int");
		dictionary[typeof(bool)] = new ExtensionTypeInfo(typeof(BoolOps), "bool");
		dictionary[typeof(double)] = new ExtensionTypeInfo(typeof(DoubleOps), "float");
		dictionary[typeof(ValueType)] = new ExtensionTypeInfo(typeof(ValueType), "ValueType");
		dictionary[typeof(BigInteger)] = new ExtensionTypeInfo(typeof(BigIntegerOps), "long");
		dictionary[typeof(Complex)] = new ExtensionTypeInfo(typeof(ComplexOps), "complex");
		dictionary[typeof(DynamicNull)] = new ExtensionTypeInfo(typeof(NoneTypeOps), "NoneType");
		dictionary[typeof(IDictionary<object, object>)] = new ExtensionTypeInfo(typeof(DictionaryOps), "dict");
		dictionary[typeof(NamespaceTracker)] = new ExtensionTypeInfo(typeof(NamespaceTrackerOps), "namespace#");
		dictionary[typeof(TypeGroup)] = new ExtensionTypeInfo(typeof(TypeGroupOps), "type-collision");
		dictionary[typeof(TypeTracker)] = new ExtensionTypeInfo(typeof(TypeTrackerOps), "type-collision");
		return dictionary;
	}

	internal static string GetTypeNameInternal(Type t)
	{
		if (_sysTypes.TryGetValue(t, out var value))
		{
			return value.PythonName;
		}
		PythonTypeAttribute[] array = (PythonTypeAttribute[])t.GetCustomAttributes(typeof(PythonTypeAttribute), inherit: false);
		if (array.Length > 0 && array[0].Name != null)
		{
			return array[0].Name;
		}
		return t.Name;
	}

	public static bool IsExtendedType(Type t)
	{
		return _sysTypes.ContainsKey(t);
	}

	public static bool IsPythonType(Type t)
	{
		if (!_sysTypes.ContainsKey(t))
		{
			return t.IsDefined(typeof(PythonTypeAttribute), inherit: false);
		}
		return true;
	}

	private void DomainManager_AssemblyLoaded(object sender, AssemblyLoadedEventArgs e)
	{
		Assembly assembly = e.Assembly;
		ExtensionTypeAttribute[] array = (ExtensionTypeAttribute[])assembly.GetCustomAttributes(typeof(ExtensionTypeAttribute), inherit: true);
		if (array.Length > 0)
		{
			lock (_dlrExtensionTypes)
			{
				ExtensionTypeAttribute[] array2 = array;
				foreach (ExtensionTypeAttribute extensionTypeAttribute in array2)
				{
					if (extensionTypeAttribute.Extends.IsInterface)
					{
						_registeredInterfaceExtensions = true;
					}
					if (!_dlrExtensionTypes.TryGetValue(extensionTypeAttribute.Extends, out var value))
					{
						value = (_dlrExtensionTypes[extensionTypeAttribute.Extends] = new List<Type>());
					}
					else if (value.IsReadOnly)
					{
						value = (_dlrExtensionTypes[extensionTypeAttribute.Extends] = new List<Type>(value));
					}
					if (!value.Contains(extensionTypeAttribute.ExtensionType))
					{
						value.Add(extensionTypeAttribute.ExtensionType);
					}
				}
			}
		}
		TopNamespaceTracker.PublishComTypes(assembly);
		ClrModule.ReferencesList referencedAssemblies = _context.ReferencedAssemblies;
		lock (referencedAssemblies)
		{
			referencedAssemblies.Add(assembly);
		}
		LoadScriptCode(_context, assembly);
		_context.LoadBuiltins(_context.BuiltinModules, assembly, updateSys: true);
		NewTypeMaker.LoadNewTypes(assembly);
	}

	private static void LoadScriptCode(PythonContext pc, Assembly asm)
	{
		ScriptCode[] array = SavableScriptCode.LoadFromAssembly(pc.DomainManager, asm);
		ScriptCode[] array2 = array;
		foreach (ScriptCode code in array2)
		{
			pc.GetCompiledLoader().AddScriptCode(code);
		}
	}
}
