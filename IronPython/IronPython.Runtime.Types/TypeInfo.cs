using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

internal static class TypeInfo
{
	public static class _Object
	{
		public new static readonly MethodInfo GetType = typeof(object).GetMethod("GetType");
	}

	public static class _IPythonObject
	{
		public static readonly PropertyInfo PythonType = typeof(IPythonObject).GetProperty("PythonType");

		public static readonly PropertyInfo Dict = typeof(IPythonObject).GetProperty("Dict");
	}

	public static class _PythonOps
	{
		public static readonly MethodInfo SlotTryGetBoundValue = typeof(PythonOps).GetMethod("SlotTryGetBoundValue");

		public static readonly MethodInfo GetTypeVersion = typeof(PythonOps).GetMethod("GetTypeVersion");

		public static readonly MethodInfo CheckTypeVersion = typeof(PythonOps).GetMethod("CheckTypeVersion");
	}

	public static class _OperationFailed
	{
		public static readonly FieldInfo Value = typeof(OperationFailed).GetField("Value");
	}

	public static class _PythonDictionary
	{
		public static readonly MethodInfo TryGetvalue = typeof(PythonDictionary).GetMethod("TryGetValue");
	}

	public static class _PythonGenerator
	{
		public static readonly ConstructorInfo Ctor = typeof(PythonGenerator).GetConstructor(new Type[1] { typeof(PythonFunction) });
	}

	private abstract class MemberResolver
	{
		public abstract MemberGroup ResolveMember(MemberBinder binder, MemberRequestKind action, Type type, string name);

		public IList<ResolvedMember> ResolveMembers(MemberBinder binder, MemberRequestKind action, Type type)
		{
			Dictionary<string, ResolvedMember> dictionary = new Dictionary<string, ResolvedMember>();
			foreach (string candidateName in GetCandidateNames(binder, action, type))
			{
				if (!dictionary.ContainsKey(candidateName))
				{
					MemberGroup memberGroup = ResolveMember(binder, action, type, candidateName);
					if (memberGroup.Count > 0)
					{
						dictionary[candidateName] = new ResolvedMember(candidateName, memberGroup);
					}
				}
			}
			ResolvedMember[] array = new ResolvedMember[dictionary.Count];
			dictionary.Values.CopyTo(array, 0);
			return array;
		}

		protected abstract IEnumerable<string> GetCandidateNames(MemberBinder binder, MemberRequestKind action, Type type);
	}

	private class OneOffResolver : MemberResolver
	{
		private string _name;

		private Func<MemberBinder, Type, MemberGroup> _resolver;

		public OneOffResolver(string name, Func<MemberBinder, Type, MemberGroup> resolver)
		{
			_name = name;
			_resolver = resolver;
		}

		public override MemberGroup ResolveMember(MemberBinder binder, MemberRequestKind action, Type type, string name)
		{
			if (name == _name)
			{
				return _resolver(binder, type);
			}
			return MemberGroup.EmptyGroup;
		}

		protected override IEnumerable<string> GetCandidateNames(MemberBinder binder, MemberRequestKind action, Type type)
		{
			yield return _name;
		}
	}

	private class StandardResolver : MemberResolver
	{
		public override MemberGroup ResolveMember(MemberBinder binder, MemberRequestKind action, Type type, string name)
		{
			if (name == ".ctor" || name == ".cctor")
			{
				return MemberGroup.EmptyGroup;
			}
			foreach (Type contributingType in binder.GetContributingTypes(type))
			{
				MemberGroup memberGroup = FilterSpecialNames(binder.GetMember(contributingType, name), name, action);
				if (memberGroup.Count > 0)
				{
					return memberGroup;
				}
			}
			if (type.IsInterface)
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					MemberGroup memberGroup = FilterSpecialNames(binder.GetMember(type2, name), name, action);
					if (memberGroup.Count > 0)
					{
						return memberGroup;
					}
				}
			}
			return MemberGroup.EmptyGroup;
		}

		protected override IEnumerable<string> GetCandidateNames(MemberBinder binder, MemberRequestKind action, Type type)
		{
			foreach (Type curType in binder.GetContributingTypes(type))
			{
				try
				{
					MemberInfo[] members = curType.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
					foreach (MemberInfo mi in members)
					{
						if (mi.MemberType == MemberTypes.Method)
						{
							MethodInfo meth = (MethodInfo)mi;
							if (meth.IsSpecialName)
							{
								if (meth.IsDefined(typeof(PropertyMethodAttribute), inherit: true))
								{
									if (meth.Name.StartsWith("Get") || meth.Name.StartsWith("Set"))
									{
										yield return meth.Name.Substring(3);
									}
									else
									{
										yield return meth.Name.Substring(6);
									}
								}
								continue;
							}
						}
						yield return mi.Name;
					}
				}
				finally
				{
				}
			}
		}
	}

	private class EqualityResolver : MemberResolver
	{
		public static readonly EqualityResolver Instance = new EqualityResolver();

		private EqualityResolver()
		{
		}

		public override MemberGroup ResolveMember(MemberBinder binder, MemberRequestKind action, Type type, string name)
		{
			bool flag;
			switch (name)
			{
			case "__eq__":
				flag = true;
				break;
			case "__ne__":
				flag = false;
				break;
			default:
				return MemberGroup.EmptyGroup;
			}
			if (typeof(IStructuralEquatable).IsAssignableFrom(type))
			{
				return new MemberGroup(GetEqualityMethods(type, flag ? "StructuralEqualityMethod" : "StructuralInequalityMethod"));
			}
			return MemberGroup.EmptyGroup;
		}

		protected override IEnumerable<string> GetCandidateNames(MemberBinder binder, MemberRequestKind action, Type type)
		{
			yield return "__eq__";
			yield return "__ne__";
		}
	}

	private class ComparisonResolver : MemberResolver
	{
		private readonly bool _excludePrimitiveTypes;

		private readonly Type _comparable;

		private readonly Dictionary<string, string> _helperMap;

		public ComparisonResolver(Type comparable, string helperPrefix)
		{
			_excludePrimitiveTypes = comparable == typeof(IComparable);
			_comparable = comparable;
			_helperMap = new Dictionary<string, string>();
			_helperMap["__eq__"] = helperPrefix + "Equality";
			_helperMap["__ne__"] = helperPrefix + "Inequality";
			_helperMap["__gt__"] = helperPrefix + "GreaterThan";
			_helperMap["__lt__"] = helperPrefix + "LessThan";
			_helperMap["__ge__"] = helperPrefix + "GreaterEqual";
			_helperMap["__le__"] = helperPrefix + "LessEqual";
		}

		public override MemberGroup ResolveMember(MemberBinder binder, MemberRequestKind action, Type type, string name)
		{
			if (_excludePrimitiveTypes && (type.IsPrimitive || type == typeof(BigInteger) || type == typeof(string) || type == typeof(decimal)))
			{
				return MemberGroup.EmptyGroup;
			}
			if (_helperMap.TryGetValue(name, out var value) && _comparable.IsAssignableFrom(type))
			{
				return new MemberGroup(GetEqualityMethods(type, value));
			}
			return MemberGroup.EmptyGroup;
		}

		protected override IEnumerable<string> GetCandidateNames(MemberBinder binder, MemberRequestKind action, Type type)
		{
			return _helperMap.Keys;
		}
	}

	private class OperatorResolver : MemberResolver
	{
		public override MemberGroup ResolveMember(MemberBinder binder, MemberRequestKind action, Type type, string name)
		{
			if (type.IsSealed() && type.IsAbstract())
			{
				return MemberGroup.EmptyGroup;
			}
			EnsureOperatorTable();
			if (_pythonOperatorTable.TryGetValue(name, out var value) && IncludeOperatorMethod(type, value))
			{
				OperatorMapping operatorMapping = ((!IsReverseOperator(value)) ? OperatorMapping.GetOperatorMapping(value) : OperatorMapping.GetOperatorMapping(value & (PythonOperationKind)(-268435457)));
				if (operatorMapping != null)
				{
					foreach (Type contributingType in binder.GetContributingTypes(type))
					{
						if (contributingType == typeof(double))
						{
							if ((operatorMapping.Operator & PythonOperationKind.Comparison) != PythonOperationKind.None)
							{
								continue;
							}
						}
						else if (contributingType == typeof(BigInteger))
						{
							if (operatorMapping.Operator == PythonOperationKind.Mod || operatorMapping.Operator == PythonOperationKind.RightShift || operatorMapping.Operator == PythonOperationKind.LeftShift || operatorMapping.Operator == PythonOperationKind.Compare || operatorMapping.Operator == PythonOperationKind.Divide)
							{
								continue;
							}
						}
						else if (contributingType == typeof(Complex) && operatorMapping.Operator == PythonOperationKind.Divide)
						{
							continue;
						}
						MemberGroup memberGroup = binder.GetMember(contributingType, operatorMapping.Name);
						if (memberGroup.Count == 0 && operatorMapping.AlternateName != null)
						{
							memberGroup = binder.GetMember(contributingType, operatorMapping.AlternateName);
							if (operatorMapping.AlternateName == "Equals")
							{
								memberGroup = FilterObjectEquality(memberGroup);
							}
							memberGroup = FilterAlternateMethods(operatorMapping, memberGroup);
						}
						if (memberGroup.Count > 0)
						{
							return FilterForwardReverseMethods(name, memberGroup, type, value);
						}
					}
				}
			}
			if (name == "__call__")
			{
				MemberGroup member = binder.GetMember(type, "Call");
				if (member.Count > 0)
				{
					return member;
				}
			}
			return MemberGroup.EmptyGroup;
		}

		private static MemberGroup FilterAlternateMethods(OperatorMapping opInfo, MemberGroup res)
		{
			if (res.Count > 0 && opInfo.AlternateExpectedType != null)
			{
				List<MemberTracker> list = new List<MemberTracker>();
				for (int i = 0; i < res.Count; i++)
				{
					MemberTracker memberTracker = res[i];
					if (memberTracker.MemberType == TrackerTypes.Method && ((MethodTracker)memberTracker).Method.ReturnType == opInfo.AlternateExpectedType)
					{
						list.Add(memberTracker);
					}
				}
				res = ((list.Count != 0) ? new MemberGroup(list.ToArray()) : MemberGroup.EmptyGroup);
			}
			return res;
		}

		private static MemberGroup FilterObjectEquality(MemberGroup group)
		{
			List<MemberTracker> list = null;
			for (int i = 0; i < group.Count; i++)
			{
				MemberTracker memberTracker = group[i];
				if (memberTracker.MemberType == TrackerTypes.Method && (memberTracker.DeclaringType == typeof(object) || memberTracker.DeclaringType == typeof(double) || memberTracker.DeclaringType == typeof(float)) && memberTracker.Name == "Equals")
				{
					if (list == null)
					{
						list = new List<MemberTracker>();
						for (int j = 0; j < i; j++)
						{
							list.Add(group[j]);
						}
					}
				}
				else if (memberTracker.MemberType == TrackerTypes.Method && memberTracker.DeclaringType == typeof(ValueType) && memberTracker.Name == "Equals")
				{
					if (list == null)
					{
						list = new List<MemberTracker>();
						for (int k = 0; k < i; k++)
						{
							list.Add(group[k]);
						}
					}
					list.Add(MemberTracker.FromMemberInfo(typeof(object).GetMethod("Equals", new Type[1] { typeof(object) })));
				}
				else
				{
					list?.Add(group[i]);
				}
			}
			if (list != null)
			{
				if (list.Count == 0)
				{
					return MemberGroup.EmptyGroup;
				}
				return new MemberGroup(list.ToArray());
			}
			return group;
		}

		protected override IEnumerable<string> GetCandidateNames(MemberBinder binder, MemberRequestKind action, Type type)
		{
			EnsureOperatorTable();
			foreach (string key in _pythonOperatorTable.Keys)
			{
				yield return key;
			}
			yield return "__call__";
		}
	}

	private class PrivateBindingResolver : MemberResolver
	{
		private const BindingFlags _privateFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

		public override MemberGroup ResolveMember(MemberBinder binder, MemberRequestKind action, Type type, string name)
		{
			if (binder.DomainManager.Configuration.PrivateBinding)
			{
				string text = "_" + type.Name + "__";
				if (name.StartsWith(text))
				{
					string name2 = name.Substring(text.Length);
					MemberGroup memberGroup = new MemberGroup(type.GetMember(name2, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));
					if (memberGroup.Count > 0)
					{
						return FilterFieldAndEvent(memberGroup);
					}
					memberGroup = new MemberGroup(type.GetMember(name2, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy));
					if (memberGroup.Count > 0)
					{
						return FilterFieldAndEvent(memberGroup);
					}
				}
			}
			return MemberGroup.EmptyGroup;
		}

		protected override IEnumerable<string> GetCandidateNames(MemberBinder binder, MemberRequestKind action, Type type)
		{
			if (!binder.DomainManager.Configuration.PrivateBinding)
			{
				yield break;
			}
			try
			{
				MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				foreach (MemberInfo mi in members)
				{
					yield return "_" + mi.DeclaringType.Name + "__" + mi.Name;
				}
			}
			finally
			{
			}
		}
	}

	private class ProtectedMemberResolver : MemberResolver
	{
		public override MemberGroup ResolveMember(MemberBinder binder, MemberRequestKind action, Type type, string name)
		{
			using (IEnumerator<Type> enumerator = binder.GetContributingTypes(type).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Type current = enumerator.Current;
					MemberGroup memberGroup = new MemberGroup(current.GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(ProtectedOnly).ToArray());
					for (int i = 0; i < memberGroup.Count; i++)
					{
						if (!(memberGroup[i] is MethodTracker { Name: "Finalize" } methodTracker) || !(methodTracker.Method.GetBaseDefinition() == typeof(object).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic)))
						{
							continue;
						}
						MemberTracker[] array = new MemberTracker[memberGroup.Count - 1];
						if (memberGroup.Count == 1)
						{
							memberGroup = MemberGroup.EmptyGroup;
							break;
						}
						for (int j = 0; j < i; j++)
						{
							array[j] = memberGroup[j];
						}
						for (int k = i + 1; k < memberGroup.Count; k++)
						{
							array[k - 1] = memberGroup[k];
						}
						memberGroup = new MemberGroup(array);
						break;
					}
					return FilterSpecialNames(memberGroup, name, action);
				}
			}
			return MemberGroup.EmptyGroup;
		}

		protected override IEnumerable<string> GetCandidateNames(MemberBinder binder, MemberRequestKind action, Type type)
		{
			foreach (Type t in binder.GetContributingTypes(type))
			{
				try
				{
					MemberInfo[] members = t.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
					foreach (MemberInfo mi in members)
					{
						if (ProtectedOnly(mi))
						{
							yield return mi.Name;
						}
					}
				}
				finally
				{
				}
			}
		}
	}

	private class DocumentationDescriptor : PythonTypeSlot
	{
		internal override bool GetAlwaysSucceeds => true;

		internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
		{
			if (owner.IsSystemType)
			{
				value = PythonTypeOps.GetDocumentation(owner.UnderlyingSystemType);
				return true;
			}
			value = null;
			return false;
		}

		internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
		{
			if (!(instance is IPythonObject pythonObject) || !pythonObject.PythonType.HasDictionary)
			{
				return false;
			}
			UserTypeOps.GetDictionary(pythonObject)["__doc__"] = value;
			return true;
		}
	}

	private class OneOffOperatorBinder
	{
		private string _methodName;

		private string _pythonName;

		private PythonOperationKind _op;

		public OneOffOperatorBinder(string methodName, string pythonName, PythonOperationKind opMap)
		{
			_methodName = methodName;
			_pythonName = pythonName;
			_op = opMap;
		}

		public MemberGroup Resolver(MemberBinder binder, Type type)
		{
			if (type.IsSealed() && type.IsAbstract())
			{
				return MemberGroup.EmptyGroup;
			}
			foreach (Type contributingType in binder.GetContributingTypes(type))
			{
				MemberGroup member = binder.GetMember(contributingType, _methodName);
				if (member.Count > 0)
				{
					return FilterForwardReverseMethods(_pythonName, member, type, _op);
				}
			}
			return MemberGroup.EmptyGroup;
		}
	}

	private class OneOffPowerBinder
	{
		private string _pythonName;

		private PythonOperationKind _op;

		public OneOffPowerBinder(string pythonName, PythonOperationKind op)
		{
			_pythonName = pythonName;
			_op = op;
		}

		public MemberGroup Resolver(MemberBinder binder, Type type)
		{
			if (type.IsSealed() && type.IsAbstract())
			{
				return MemberGroup.EmptyGroup;
			}
			foreach (Type contributingType in binder.GetContributingTypes(type))
			{
				if (!(contributingType == typeof(BigInteger)))
				{
					MemberGroup member = binder.GetMember(contributingType, "op_Power");
					if (member.Count > 0)
					{
						return FilterForwardReverseMethods(_pythonName, member, type, _op);
					}
					member = binder.GetMember(contributingType, "Power");
					if (member.Count > 0)
					{
						return FilterForwardReverseMethods(_pythonName, member, type, _op);
					}
				}
			}
			return MemberGroup.EmptyGroup;
		}
	}

	private abstract class MemberBinder
	{
		private PythonBinder _binder;

		public PythonBinder Binder => _binder;

		public ScriptDomainManager DomainManager => _binder.DomainManager;

		public MemberBinder(PythonBinder binder)
		{
			_binder = binder;
		}

		public abstract IList<Type> GetContributingTypes(Type t);

		public abstract IList<Type> GetInterfaces(Type t);

		public abstract MemberGroup GetBaseInstanceMethod(Type type, params string[] name);

		public abstract MemberGroup GetMember(Type type, string name);

		protected MemberGroup GetMember(Type type, string name, BindingFlags flags)
		{
			IEnumerable<MemberInfo> enumerable = type.GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | flags);
			if (!Binder.DomainManager.Configuration.PrivateBinding)
			{
				enumerable = CompilerHelpers.FilterNonVisibleMembers(type, enumerable);
			}
			MemberInfo[] array = enumerable.ToArray();
			List<MemberInfo> list = null;
			for (int i = 0; i < array.Length; i++)
			{
				MemberInfo memberInfo = array[i];
				if (memberInfo.DeclaringType.IsDefined(typeof(PythonHiddenBaseClassAttribute), inherit: false))
				{
					if (list == null)
					{
						list = new List<MemberInfo>();
						for (int j = 0; j < i; j++)
						{
							list.Add(array[j]);
						}
					}
				}
				else
				{
					list?.Add(array[i]);
				}
			}
			if (list != null)
			{
				array = list.ToArray();
			}
			MemberGroup memberGroup = new MemberGroup(array);
			Type[] nestedTypes = type.GetNestedTypes(BindingFlags.Public | flags);
			string value = name + '`';
			List<Type> list2 = null;
			Type[] array2 = nestedTypes;
			foreach (Type type2 in array2)
			{
				if (type2.Name.StartsWith(value))
				{
					if (list2 == null)
					{
						list2 = new List<Type>();
					}
					list2.Add(type2);
				}
			}
			if (list2 != null)
			{
				List<MemberTracker> list3 = new List<MemberTracker>(memberGroup);
				foreach (Type item in list2)
				{
					list3.Add(MemberTracker.FromMemberInfo(item));
				}
				return new MemberGroup(list3.ToArray());
			}
			if (memberGroup.Count == 0)
			{
				memberGroup = (((flags & BindingFlags.DeclaredOnly) != BindingFlags.Default) ? Binder.GetExtensionMembers(type, name) : Binder.GetAllExtensionMembers(type, name));
			}
			return memberGroup;
		}
	}

	private class ResolveBinder : MemberBinder
	{
		public ResolveBinder(PythonBinder binder)
			: base(binder)
		{
		}

		public override IList<Type> GetInterfaces(Type t)
		{
			return t.GetInterfaces();
		}

		public override MemberGroup GetBaseInstanceMethod(Type type, params string[] name)
		{
			return GetInstanceOpsMethod(type, name);
		}

		public override IList<Type> GetContributingTypes(Type t)
		{
			List<Type> list = new List<Type>();
			IList<PythonType> resolutionOrder = DynamicHelpers.GetPythonTypeFromType(t).ResolutionOrder;
			foreach (PythonType item2 in resolutionOrder)
			{
				list.Add(item2.UnderlyingSystemType);
			}
			foreach (PythonType item3 in resolutionOrder)
			{
				list.AddRange(base.Binder.GetExtensionTypesInternal(item3.UnderlyingSystemType));
			}
			if (t.IsInterface)
			{
				Type[] interfaces = t.GetInterfaces();
				foreach (Type item in interfaces)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public override MemberGroup GetMember(Type type, string name)
		{
			return GetMember(type, name, BindingFlags.Default);
		}
	}

	private class LookupBinder : MemberBinder
	{
		public LookupBinder(PythonBinder binder)
			: base(binder)
		{
		}

		public override IList<Type> GetInterfaces(Type t)
		{
			if (t.IsInterface)
			{
				return t.GetInterfaces();
			}
			Type[] interfaces = t.GetInterfaces();
			List<Type> list = new List<Type>();
			Type[] array = interfaces;
			foreach (Type type in array)
			{
				try
				{
					MethodInfo[] targetMethods = t.GetInterfaceMap(type).TargetMethods;
					foreach (MethodInfo methodInfo in targetMethods)
					{
						if (methodInfo != null && methodInfo.DeclaringType == t)
						{
							list.Add(type);
							break;
						}
					}
				}
				catch (ArgumentException)
				{
				}
			}
			return list;
		}

		public override MemberGroup GetBaseInstanceMethod(Type type, params string[] name)
		{
			if (type.BaseType == typeof(object) || type.BaseType == typeof(ValueType))
			{
				return GetInstanceOpsMethod(type, name);
			}
			return MemberGroup.EmptyGroup;
		}

		public override IList<Type> GetContributingTypes(Type t)
		{
			List<Type> list = new List<Type>();
			list.Add(t);
			list.AddRange(base.Binder.GetExtensionTypesInternal(t));
			return list;
		}

		public override MemberGroup GetMember(Type type, string name)
		{
			return GetMember(type, name, BindingFlags.DeclaredOnly);
		}
	}

	private static readonly MemberResolver[] _resolvers = MakeResolverTable();

	private static DocumentationDescriptor _docDescr;

	internal static Dictionary<string, PythonOperationKind> _pythonOperatorTable;

	private static Func<MemberBinder, Type, MemberGroup> _ComplexResolver;

	private static Func<MemberBinder, Type, MemberGroup> _FloatResolver;

	private static Func<MemberBinder, Type, MemberGroup> _IntResolver;

	private static Func<MemberBinder, Type, MemberGroup> _BigIntegerResolver;

	private static Func<MemberBinder, Type, MemberGroup> _GetItemResolver;

	private static Func<MemberBinder, Type, MemberGroup> _SetItemResolver;

	private static readonly string[] CastNames = new string[2] { "op_Implicit", "op_Explicit" };

	private static Func<MemberBinder, Type, MemberGroup> ComplexResolver
	{
		get
		{
			if (_ComplexResolver != null)
			{
				return _ComplexResolver;
			}
			List<Type> list = new List<Type>();
			list.Add(typeof(Complex));
			list.Add(typeof(ExtensibleComplex));
			list.Add(typeof(Extensible<Complex>));
			list.Add(typeof(double));
			list.Add(typeof(Extensible<double>));
			_ComplexResolver = MakeConversionResolver(list);
			return _ComplexResolver;
		}
	}

	private static Func<MemberBinder, Type, MemberGroup> FloatResolver
	{
		get
		{
			if (_FloatResolver != null)
			{
				return _FloatResolver;
			}
			List<Type> list = new List<Type>();
			list.Add(typeof(double));
			list.Add(typeof(Extensible<double>));
			_FloatResolver = MakeConversionResolver(list);
			return _FloatResolver;
		}
	}

	private static Func<MemberBinder, Type, MemberGroup> IntResolver
	{
		get
		{
			if (_IntResolver != null)
			{
				return _IntResolver;
			}
			List<Type> list = new List<Type>();
			list.Add(typeof(int));
			list.Add(typeof(Extensible<int>));
			list.Add(typeof(BigInteger));
			list.Add(typeof(Extensible<BigInteger>));
			_IntResolver = MakeConversionResolver(list);
			return _IntResolver;
		}
	}

	private static Func<MemberBinder, Type, MemberGroup> BigIntegerResolver
	{
		get
		{
			if (_BigIntegerResolver != null)
			{
				return _BigIntegerResolver;
			}
			List<Type> list = new List<Type>();
			list.Add(typeof(BigInteger));
			list.Add(typeof(Extensible<BigInteger>));
			list.Add(typeof(int));
			list.Add(typeof(Extensible<int>));
			_BigIntegerResolver = MakeConversionResolver(list);
			return _BigIntegerResolver;
		}
	}

	private static Func<MemberBinder, Type, MemberGroup> GetItemResolver
	{
		get
		{
			if (_GetItemResolver == null)
			{
				_GetItemResolver = MakeIndexerResolver(set: false);
			}
			return _GetItemResolver;
		}
	}

	private static Func<MemberBinder, Type, MemberGroup> SetItemResolver
	{
		get
		{
			if (_SetItemResolver == null)
			{
				_SetItemResolver = MakeIndexerResolver(set: true);
			}
			return _SetItemResolver;
		}
	}

	public static MemberGroup GetMemberAll(PythonBinder binder, MemberRequestKind action, Type type, string name)
	{
		return GetMemberGroup(new ResolveBinder(binder), action, type, name);
	}

	public static IList<ResolvedMember> GetMembersAll(PythonBinder binder, MemberRequestKind action, Type type)
	{
		return GetResolvedMembers(new ResolveBinder(binder), action, type);
	}

	public static MemberGroup GetMember(PythonBinder binder, MemberRequestKind action, Type type, string name)
	{
		return GetMemberGroup(new LookupBinder(binder), action, type, name);
	}

	public static IList<ResolvedMember> GetMembers(PythonBinder binder, MemberRequestKind action, Type type)
	{
		return GetResolvedMembers(new LookupBinder(binder), action, type);
	}

	private static MemberResolver[] MakeResolverTable()
	{
		return new MemberResolver[41]
		{
			new OneOffResolver("__str__", StringResolver),
			new OneOffResolver("__new__", NewResolver),
			new OneOffResolver("__repr__", ReprResolver),
			new OneOffResolver("__hash__", HashResolver),
			new OneOffResolver("__iter__", IterResolver),
			new OneOffResolver("__reduce_ex__", SerializationResolver),
			new StandardResolver(),
			EqualityResolver.Instance,
			new ComparisonResolver(typeof(IStructuralComparable), "StructuralComparable"),
			new OneOffResolver("__all__", AllResolver),
			new OneOffResolver("__contains__", ContainsResolver),
			new OneOffResolver("__dir__", DirResolver),
			new OneOffResolver("__doc__", DocResolver),
			new OneOffResolver("__enter__", EnterResolver),
			new OneOffResolver("__exit__", ExitResolver),
			new OneOffResolver("__len__", LengthResolver),
			new OneOffResolver("__format__", FormatResolver),
			new OneOffResolver("next", NextResolver),
			new OneOffResolver("__complex__", ComplexResolver),
			new OneOffResolver("__float__", FloatResolver),
			new OneOffResolver("__int__", IntResolver),
			new OneOffResolver("__long__", BigIntegerResolver),
			new OneOffResolver("__truediv__", new OneOffOperatorBinder("TrueDivide", "__truediv__", PythonOperationKind.TrueDivide).Resolver),
			new OneOffResolver("__rtruediv__", new OneOffOperatorBinder("TrueDivide", "__rtruediv__", PythonOperationKind.ReverseTrueDivide).Resolver),
			new OneOffResolver("__itruediv__", new OneOffOperatorBinder("InPlaceTrueDivide", "__itruediv__", PythonOperationKind.InPlaceTrueDivide).Resolver),
			new OneOffResolver("__floordiv__", new OneOffOperatorBinder("FloorDivide", "__floordiv__", PythonOperationKind.FloorDivide).Resolver),
			new OneOffResolver("__rfloordiv__", new OneOffOperatorBinder("FloorDivide", "__rfloordiv__", PythonOperationKind.ReverseFloorDivide).Resolver),
			new OneOffResolver("__ifloordiv__", new OneOffOperatorBinder("InPlaceFloorDivide", "__ifloordiv__", PythonOperationKind.InPlaceFloorDivide).Resolver),
			new OneOffResolver("__pow__", new OneOffPowerBinder("__pow__", PythonOperationKind.Power).Resolver),
			new OneOffResolver("__rpow__", new OneOffPowerBinder("__rpow__", PythonOperationKind.ReversePower).Resolver),
			new OneOffResolver("__ipow__", new OneOffOperatorBinder("InPlacePower", "__ipow__", PythonOperationKind.InPlacePower).Resolver),
			new OneOffResolver("__abs__", new OneOffOperatorBinder("Abs", "__abs__", PythonOperationKind.AbsoluteValue).Resolver),
			new OneOffResolver("__divmod__", new OneOffOperatorBinder("DivMod", "__divmod__", PythonOperationKind.DivMod).Resolver),
			new OneOffResolver("__rdivmod__", new OneOffOperatorBinder("DivMod", "__rdivmod__", PythonOperationKind.DivMod).Resolver),
			new OperatorResolver(),
			new OneOffResolver("__getitem__", GetItemResolver),
			new OneOffResolver("__setitem__", SetItemResolver),
			new OneOffResolver("__ne__", FallbackInequalityResolver),
			new ComparisonResolver(typeof(IComparable), "Comparable"),
			new ProtectedMemberResolver(),
			new PrivateBindingResolver()
		};
	}

	private static MemberGroup StringResolver(MemberBinder binder, Type type)
	{
		if (type != typeof(double) && type != typeof(float) && type != typeof(Complex))
		{
			MethodInfo method = type.GetMethod("ToString", ReflectionUtils.EmptyTypes);
			if (method != null && method.DeclaringType != typeof(object))
			{
				return GetInstanceOpsMethod(type, "ToStringMethod");
			}
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup ReprResolver(MemberBinder binder, Type type)
	{
		if (!PythonBinder.IsPythonType(type) && (!type.IsSealed() || !type.IsAbstract()))
		{
			foreach (Type contributingType in binder.GetContributingTypes(type))
			{
				if (!(contributingType == typeof(ObjectOps)) || !(type != typeof(object)))
				{
					if (contributingType.GetMember("__repr__").Length > 0)
					{
						return MemberGroup.EmptyGroup;
					}
					continue;
				}
				break;
			}
			return binder.GetBaseInstanceMethod(type, "FancyRepr");
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup SerializationResolver(MemberBinder binder, Type type)
	{
		if (type.IsSerializable && !PythonBinder.IsPythonType(type))
		{
			string methodName = "__reduce_ex__";
			if (!TypeOverridesMethod(binder, type, methodName))
			{
				return GetInstanceOpsMethod(type, "SerializeReduce");
			}
		}
		return MemberGroup.EmptyGroup;
	}

	private static bool TypeOverridesMethod(MemberBinder binder, Type type, string methodName)
	{
		foreach (Type contributingType in binder.GetContributingTypes(type))
		{
			if (PythonBinder.IsPythonType(type) || !(contributingType == typeof(ObjectOps)) || !(type != typeof(object)))
			{
				MemberInfo[] member = contributingType.GetMember(methodName);
				if (member.Length > 0)
				{
					return true;
				}
				continue;
			}
			break;
		}
		return false;
	}

	private static MemberGroup HashResolver(MemberBinder binder, Type type)
	{
		if (typeof(IStructuralEquatable).IsAssignableFrom(type) && !type.IsInterface)
		{
			foreach (Type contributingType in binder.GetContributingTypes(type))
			{
				if (!(contributingType == typeof(ObjectOps)) && !(contributingType == typeof(object)))
				{
					MemberInfo[] member = contributingType.GetMember("__hash__");
					if (member.Length > 0)
					{
						return MemberGroup.EmptyGroup;
					}
					continue;
				}
				break;
			}
			return GetInstanceOpsMethod(type, "StructuralHashMethod");
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup NewResolver(MemberBinder binder, Type type)
	{
		if (type.IsSealed && type.IsAbstract)
		{
			return MemberGroup.EmptyGroup;
		}
		bool flag = typeof(IPythonObject).IsAssignableFrom(type);
		foreach (Type contributingType in binder.GetContributingTypes(type))
		{
			if (flag || !(contributingType == typeof(ObjectOps)) || !(type != typeof(object)))
			{
				MemberInfo[] member = contributingType.GetMember("__new__");
				if (member.Length > 0)
				{
					return GetExtensionMemberGroup(type, member);
				}
				continue;
			}
			break;
		}
		ConstructorInfo[] array = CompilerHelpers.FilterConstructorsToPublicAndProtected(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).ToArray();
		if (!PythonTypeOps.IsDefaultNew(array))
		{
			return new MemberGroup(array);
		}
		return MemberGroup.EmptyGroup;
	}

	internal static MemberGroup GetExtensionMemberGroup(Type type, MemberInfo[] news)
	{
		List<MemberTracker> list = new List<MemberTracker>();
		foreach (MemberInfo memberInfo in news)
		{
			if (memberInfo.MemberType == MemberTypes.Method)
			{
				if (memberInfo.DeclaringType.IsAssignableFrom(type))
				{
					list.Add(MemberTracker.FromMemberInfo(memberInfo));
				}
				else
				{
					list.Add(MemberTracker.FromMemberInfo(memberInfo, type));
				}
			}
		}
		return new MemberGroup(list.ToArray());
	}

	private static MemberGroup NextResolver(MemberBinder binder, Type type)
	{
		if (typeof(IEnumerator).IsAssignableFrom(type))
		{
			return GetInstanceOpsMethod(type, "NextMethod");
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup LengthResolver(MemberBinder binder, Type type)
	{
		if (!type.IsDefined(typeof(DontMapICollectionToLenAttribute), inherit: true))
		{
			if (binder.GetInterfaces(type).Contains(typeof(ICollection)))
			{
				return GetInstanceOpsMethod(type, "LengthMethod");
			}
			foreach (Type @interface in binder.GetInterfaces(type))
			{
				if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(ICollection<>))
				{
					MethodInfo method = typeof(InstanceOps).GetMethod("GenericLengthMethod");
					return new MemberGroup(MemberTracker.FromMemberInfo(method.MakeGenericMethod(@interface.GetGenericArguments()), type));
				}
			}
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup IterResolver(MemberBinder binder, Type type)
	{
		if (type == typeof(string))
		{
			if (binder.Binder.Context.PythonOptions.Python30)
			{
				return GetInstanceOpsMethod(type, "IterMethodForString");
			}
			return MemberGroup.EmptyGroup;
		}
		if (typeof(Bytes).IsAssignableFrom(type))
		{
			if (binder.Binder.Context.PythonOptions.Python30)
			{
				return GetInstanceOpsMethod(type, "IterMethodForBytes");
			}
			return MemberGroup.EmptyGroup;
		}
		foreach (Type contributingType in binder.GetContributingTypes(type))
		{
			MemberInfo[] member = contributingType.GetMember("__iter__");
			if (member.Length > 0)
			{
				return MemberGroup.EmptyGroup;
			}
		}
		if (!type.IsDefined(typeof(DontMapIEnumerableToIterAttribute), inherit: true))
		{
			if (typeof(IEnumerable<>).IsAssignableFrom(type))
			{
				return GetInstanceOpsMethod(type, "IterMethodForGenericEnumerable");
			}
			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				return GetInstanceOpsMethod(type, "IterMethodForEnumerable");
			}
			if (typeof(IEnumerator<>).IsAssignableFrom(type))
			{
				return GetInstanceOpsMethod(type, "IterMethodForGenericEnumerator");
			}
			if (typeof(IEnumerator).IsAssignableFrom(type))
			{
				return GetInstanceOpsMethod(type, "IterMethodForEnumerator");
			}
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup FallbackInequalityResolver(MemberBinder binder, Type type)
	{
		if (IncludeOperatorMethod(type, PythonOperationKind.NotEqual))
		{
			foreach (Type contributingType in binder.GetContributingTypes(type))
			{
				MemberGroup member = binder.GetMember(contributingType, "Equals");
				foreach (MemberTracker item in member)
				{
					if (item.MemberType != TrackerTypes.Method || item.DeclaringType == typeof(object))
					{
						continue;
					}
					MethodTracker methodTracker = (MethodTracker)item;
					if ((methodTracker.Method.Attributes & MethodAttributes.VtableLayoutMask) == 0 && !methodTracker.Method.IsDefined(typeof(PythonHiddenAttribute), inherit: false))
					{
						ParameterInfo[] parameters = methodTracker.Method.GetParameters();
						if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object))
						{
							return new MemberGroup(MemberTracker.FromMemberInfo(typeof(InstanceOps).GetMethod("NotEqualsMethod"), contributingType));
						}
					}
				}
			}
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup AllResolver(MemberBinder binder, Type type)
	{
		if (type.IsAbstract && type.IsSealed)
		{
			return new MemberGroup(new ExtensionPropertyTracker("__all__", typeof(InstanceOps).GetMethod("Get__all__").MakeGenericMethod(type), null, null, type));
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup DirResolver(MemberBinder binder, Type type)
	{
		if (type.IsDefined(typeof(DontMapGetMemberNamesToDirAttribute), inherit: true))
		{
			return MemberGroup.EmptyGroup;
		}
		MemberGroup memberGroup = binder.GetMember(type, "GetMemberNames");
		if (memberGroup == MemberGroup.EmptyGroup && !typeof(IPythonObject).IsAssignableFrom(type) && typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
		{
			memberGroup = GetInstanceOpsMethod(type, "DynamicDir");
		}
		return memberGroup;
	}

	private static MemberGroup DocResolver(MemberBinder binder, Type type)
	{
		if (_docDescr == null)
		{
			_docDescr = new DocumentationDescriptor();
		}
		return new MemberGroup(new CustomAttributeTracker(type, "__doc__", _docDescr));
	}

	private static MemberGroup EnterResolver(MemberBinder binder, Type type)
	{
		if (!type.IsDefined(typeof(DontMapIDisposableToContextManagerAttribute), inherit: true) && typeof(IDisposable).IsAssignableFrom(type))
		{
			return GetInstanceOpsMethod(type, "EnterMethod");
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup ExitResolver(MemberBinder binder, Type type)
	{
		if (!type.IsDefined(typeof(DontMapIDisposableToContextManagerAttribute), inherit: true) && typeof(IDisposable).IsAssignableFrom(type))
		{
			return GetInstanceOpsMethod(type, "ExitMethod");
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup FormatResolver(MemberBinder binder, Type type)
	{
		if (typeof(IFormattable).IsAssignableFrom(type))
		{
			return GetInstanceOpsMethod(type, "Format");
		}
		return MemberGroup.EmptyGroup;
	}

	private static MemberGroup ContainsResolver(MemberBinder binder, Type type)
	{
		if (type.IsDefined(typeof(DontMapIEnumerableToContainsAttribute), inherit: true))
		{
			return MemberGroup.EmptyGroup;
		}
		List<MemberTracker> containsMembers = null;
		IList<Type> interfaces = binder.GetInterfaces(type);
		bool hasObjectContains = false;
		foreach (Type item in interfaces)
		{
			if (item.IsGenericType() && item.GetGenericTypeDefinition() == typeof(IDictionary<, >))
			{
				if (item.GetGenericArguments()[0] == typeof(object))
				{
					hasObjectContains = true;
				}
				if (containsMembers == null)
				{
					containsMembers = new List<MemberTracker>();
				}
				containsMembers.Add(MemberTracker.FromMemberInfo(item.GetMethod("ContainsKey")));
			}
		}
		if (containsMembers == null)
		{
			foreach (Type item2 in interfaces)
			{
				if (item2.IsGenericType() && item2.GetGenericTypeDefinition() == typeof(ICollection<>))
				{
					if (item2.GetGenericArguments()[0] == typeof(object))
					{
						hasObjectContains = true;
					}
					if (containsMembers == null)
					{
						containsMembers = new List<MemberTracker>();
					}
					containsMembers.Add(MemberTracker.FromMemberInfo(item2.GetMethod("Contains")));
				}
			}
		}
		if (!hasObjectContains)
		{
			if (interfaces.Contains(typeof(IList)))
			{
				if (containsMembers == null)
				{
					containsMembers = new List<MemberTracker>();
				}
				containsMembers.Add(MemberTracker.FromMemberInfo(typeof(IList).GetMethod("Contains")));
			}
			else if (interfaces.Contains(typeof(IDictionary)))
			{
				if (containsMembers == null)
				{
					containsMembers = new List<MemberTracker>();
				}
				containsMembers.Add(MemberTracker.FromMemberInfo(typeof(IDictionary).GetMethod("Contains")));
			}
			else if (containsMembers == null)
			{
				GetEnumeratorContains(type, interfaces, ref containsMembers, ref hasObjectContains, typeof(IEnumerable<>), typeof(IEnumerable), string.Empty);
				if (containsMembers == null)
				{
					GetEnumeratorContains(type, interfaces, ref containsMembers, ref hasObjectContains, typeof(IEnumerator<>), typeof(IEnumerator), "IEnumerator");
				}
			}
		}
		if (containsMembers != null)
		{
			return new MemberGroup(containsMembers.ToArray());
		}
		return MemberGroup.EmptyGroup;
	}

	private static void GetEnumeratorContains(Type type, IList<Type> intf, ref List<MemberTracker> containsMembers, ref bool hasObjectContains, Type ienumOfT, Type ienum, string name)
	{
		foreach (Type item in intf)
		{
			if (item.IsGenericType() && item.GetGenericTypeDefinition() == ienumOfT)
			{
				if (item.GetGenericArguments()[0] == typeof(object))
				{
					hasObjectContains = true;
				}
				if (containsMembers == null)
				{
					containsMembers = new List<MemberTracker>();
				}
				containsMembers.Add((MethodTracker)MemberTracker.FromMemberInfo(typeof(InstanceOps).GetMethod("ContainsGenericMethod" + name).MakeGenericMethod(item.GetGenericArguments()[0]), item));
			}
		}
		if (intf.Contains(type) && !hasObjectContains)
		{
			if (containsMembers == null)
			{
				containsMembers = new List<MemberTracker>();
			}
			containsMembers.Add(MemberTracker.FromMemberInfo(typeof(InstanceOps).GetMethod("ContainsMethod" + name), typeof(IEnumerable)));
		}
	}

	private static MethodTracker[] GetEqualityMethods(Type type, string name)
	{
		MethodInfo[] methodSet = GetMethodSet(name, 3);
		MethodTracker[] array = new MethodTracker[methodSet.Length];
		for (int i = 0; i < methodSet.Length; i++)
		{
			array[i] = (MethodTracker)MemberTracker.FromMemberInfo(methodSet[i].MakeGenericMethod(type), type);
		}
		return array;
	}

	private static MemberGroup GetMemberGroup(MemberBinder memberBinder, MemberRequestKind action, Type type, string name)
	{
		MemberResolver[] resolvers = _resolvers;
		foreach (MemberResolver memberResolver in resolvers)
		{
			MemberGroup memberGroup = memberResolver.ResolveMember(memberBinder, action, type, name);
			if (memberGroup.Count > 0)
			{
				return memberGroup;
			}
		}
		return MemberGroup.EmptyGroup;
	}

	private static IList<ResolvedMember> GetResolvedMembers(MemberBinder memberBinder, MemberRequestKind action, Type type)
	{
		List<ResolvedMember> list = new List<ResolvedMember>();
		MemberResolver[] resolvers = _resolvers;
		foreach (MemberResolver memberResolver in resolvers)
		{
			list.AddRange(memberResolver.ResolveMembers(memberBinder, action, type));
		}
		return list;
	}

	private static MemberGroup GetInstanceOpsMethod(Type extends, params string[] names)
	{
		MethodTracker[] array = new MethodTracker[names.Length];
		for (int i = 0; i < names.Length; i++)
		{
			array[i] = (MethodTracker)MemberTracker.FromMemberInfo(typeof(InstanceOps).GetMethod(names[i]), extends);
		}
		return new MemberGroup(array);
	}

	private static MethodInfo FindCastMethod(MemberBinder binder, Type fromType, List<Type> toTypes)
	{
		MethodInfo methodInfo = null;
		ParameterInfo[] array = null;
		foreach (Type contributingType in binder.GetContributingTypes(fromType))
		{
			string[] castNames = GetCastNames(fromType, toTypes[0]);
			foreach (string name in castNames)
			{
				MemberInfo[] member = contributingType.GetMember(name);
				foreach (MemberInfo memberInfo in member)
				{
					if (memberInfo.MemberType != MemberTypes.Method)
					{
						continue;
					}
					MethodInfo methodInfo2 = (MethodInfo)memberInfo;
					ParameterInfo[] parameters;
					if (!toTypes.Contains(methodInfo2.ReturnType) || (parameters = methodInfo2.GetParameters()).Length != 1)
					{
						continue;
					}
					if (methodInfo == null || methodInfo2.DeclaringType.IsSubclassOf(methodInfo.DeclaringType))
					{
						methodInfo = methodInfo2;
						array = parameters;
					}
					else
					{
						if (methodInfo2.DeclaringType != methodInfo.DeclaringType)
						{
							continue;
						}
						if (parameters[0].ParameterType.IsSubclassOf(array[0].ParameterType))
						{
							methodInfo = methodInfo2;
							array = parameters;
						}
						else
						{
							if (array[0].ParameterType != parameters[0].ParameterType)
							{
								continue;
							}
							if (methodInfo2.Name != methodInfo.Name)
							{
								if (methodInfo2.Name == "op_Implicit")
								{
									methodInfo = methodInfo2;
									array = parameters;
								}
								continue;
							}
							foreach (Type toType in toTypes)
							{
								if (methodInfo2.ReturnType == toType)
								{
									methodInfo = methodInfo2;
									array = parameters;
								}
								else if (methodInfo.ReturnType == toType)
								{
									break;
								}
							}
						}
					}
				}
			}
		}
		return methodInfo;
	}

	private static string[] GetCastNames(Type fromType, Type toType)
	{
		if (PythonBinder.IsPythonType(fromType))
		{
			return CastNames;
		}
		return new string[3]
		{
			"op_Implicit",
			"op_Explicit",
			"ConvertTo" + toType.Name
		};
	}

	private static Func<MemberBinder, Type, MemberGroup> MakeConversionResolver(List<Type> castPrec)
	{
		return delegate(MemberBinder binder, Type type)
		{
			MethodInfo methodInfo = FindCastMethod(binder, type, castPrec);
			if (methodInfo != null)
			{
				MethodTracker methodTracker = (MethodTracker)MemberTracker.FromMemberInfo(methodInfo, type);
				return new MemberGroup(methodTracker);
			}
			return MemberGroup.EmptyGroup;
		};
	}

	private static Func<MemberBinder, Type, MemberGroup> MakeIndexerResolver(bool set)
	{
		return delegate(MemberBinder binder, Type type)
		{
			List<MemberInfo> list = null;
			MemberInfo[] defaultMembers = type.GetDefaultMembers();
			foreach (MemberInfo memberInfo in defaultMembers)
			{
				PropertyInfo propertyInfo = memberInfo as PropertyInfo;
				if (propertyInfo != null)
				{
					MethodInfo methodInfo = (set ? propertyInfo.GetSetMethod() : propertyInfo.GetGetMethod());
					if (methodInfo != null)
					{
						list = list ?? new List<MemberInfo>();
						list.Add(methodInfo);
					}
				}
			}
			return (list == null) ? MemberGroup.EmptyGroup : new MemberGroup(list.ToArray());
		};
	}

	internal static bool IncludeOperatorMethod(Type t, PythonOperationKind op)
	{
		if (t == typeof(string) && op == PythonOperationKind.Compare)
		{
			return false;
		}
		if (t == typeof(bool) || (Converter.IsNumeric(t) && t != typeof(Complex) && t != typeof(double) && t != typeof(float)))
		{
			switch (op)
			{
			case PythonOperationKind.LessThan:
			case PythonOperationKind.GreaterThan:
			case PythonOperationKind.LessThanOrEqual:
			case PythonOperationKind.GreaterThanOrEqual:
			case PythonOperationKind.Equal:
			case PythonOperationKind.NotEqual:
				return false;
			}
		}
		return true;
	}

	private static MemberGroup FilterFieldAndEvent(MemberGroup members)
	{
		TrackerTypes trackerTypes = TrackerTypes.None;
		foreach (MemberTracker member in members)
		{
			trackerTypes |= member.MemberType;
		}
		if (trackerTypes == (TrackerTypes.Event | TrackerTypes.Field))
		{
			List<MemberTracker> list = new List<MemberTracker>();
			foreach (MemberTracker member2 in members)
			{
				if (member2.MemberType == TrackerTypes.Event)
				{
					list.Add(member2);
				}
			}
			return new MemberGroup(list.ToArray());
		}
		return members;
	}

	private static bool ProtectedOnly(MemberInfo input)
	{
		switch (input.MemberType)
		{
		case MemberTypes.Method:
			return ((MethodInfo)input).IsProtected();
		case MemberTypes.Property:
		{
			MethodInfo getMethod = ((PropertyInfo)input).GetGetMethod(nonPublic: true);
			if (getMethod != null)
			{
				return ProtectedOnly(getMethod);
			}
			return false;
		}
		case MemberTypes.Field:
			return ((FieldInfo)input).IsProtected();
		case MemberTypes.NestedType:
			return ((Type)input).IsProtected();
		default:
			return false;
		}
	}

	internal static bool IsReverseOperator(PythonOperationKind op)
	{
		return (op & PythonOperationKind.Reversed) != 0;
	}

	private static MemberGroup FilterForwardReverseMethods(string name, MemberGroup group, Type type, PythonOperationKind oper)
	{
		List<MethodTracker> list = new List<MethodTracker>(group.Count);
		PythonOperationKind pythonOperationKind = Symbols.OperatorToReverseOperator(oper);
		foreach (MemberTracker item in group)
		{
			if (item.MemberType != TrackerTypes.Method)
			{
				continue;
			}
			MethodTracker methodTracker = (MethodTracker)item;
			if (pythonOperationKind == PythonOperationKind.None)
			{
				list.Add(methodTracker);
				continue;
			}
			MethodInfo method = methodTracker.Method;
			if (!method.IsStatic)
			{
				if (!IsReverseOperator(oper))
				{
					list.Add(methodTracker);
				}
				continue;
			}
			ParameterInfo[] parameters = method.GetParameters();
			int num = ((parameters.Length > 0 && parameters[0].ParameterType == typeof(CodeContext)) ? 1 : 0);
			if (parameters.Length - num == 2)
			{
				Type parameterType = parameters[num].ParameterType;
				Type parameterType2 = parameters[1 + num].ParameterType;
				bool flag;
				bool flag2;
				if (parameterType == typeof(object) && parameterType2 == typeof(object))
				{
					flag = !IsReverseOperator(oper);
					flag2 = IsReverseOperator(oper);
				}
				else
				{
					flag = parameters.Length > 0 && AreTypesCompatible(parameterType, type);
					flag2 = (oper & PythonOperationKind.Comparison) == 0 && parameters.Length > 1 && AreTypesCompatible(parameterType2, type);
				}
				if (IsReverseOperator(oper))
				{
					if (flag2)
					{
						list.Add(methodTracker);
					}
				}
				else if (flag)
				{
					list.Add(methodTracker);
				}
			}
			else
			{
				list.Add(methodTracker);
			}
		}
		if (list.Count == 0)
		{
			return MemberGroup.EmptyGroup;
		}
		return new MemberGroup(new OperatorTracker(type, name, IsReverseOperator(oper), list.ToArray()));
	}

	private static bool AreTypesCompatible(Type paramType, Type declaringType)
	{
		if (paramType == typeof(object))
		{
			return declaringType == typeof(object);
		}
		if (paramType == declaringType || declaringType.IsSubclassOf(paramType))
		{
			return true;
		}
		if (declaringType.IsSubclassOf(typeof(Extensible<>).MakeGenericType(paramType)))
		{
			return true;
		}
		return DynamicHelpers.GetPythonTypeFromType(declaringType).IsSubclassOf(DynamicHelpers.GetPythonTypeFromType(paramType));
	}

	private static void EnsureOperatorTable()
	{
		if (_pythonOperatorTable == null)
		{
			_pythonOperatorTable = InitializeOperatorTable();
		}
	}

	private static MemberGroup FilterSpecialNames(MemberGroup group, string name, MemberRequestKind action)
	{
		bool flag = true;
		if (action == MemberRequestKind.Invoke || action == MemberRequestKind.Convert || action == MemberRequestKind.Operation)
		{
			flag = false;
		}
		if (!IsPythonRecognizedOperator(name))
		{
			flag = false;
		}
		List<MemberTracker> list = null;
		for (int i = 0; i < group.Count; i++)
		{
			MemberTracker memberTracker = group[i];
			bool flag2 = false;
			if (memberTracker.MemberType == TrackerTypes.Method)
			{
				MethodTracker methodTracker = (MethodTracker)memberTracker;
				if (methodTracker.Method.IsSpecialName && memberTracker.Name != "op_Implicit" && memberTracker.Name != "op_Explicit" && !IsPropertyWithParameters(methodTracker))
				{
					flag2 = true;
				}
				if (methodTracker.Method.IsDefined(typeof(ClassMethodAttribute), inherit: true))
				{
					return new MemberGroup(new ClassMethodTracker(group));
				}
			}
			else if (memberTracker.MemberType == TrackerTypes.Property)
			{
				PropertyTracker propertyTracker = (PropertyTracker)memberTracker;
				if (name == propertyTracker.Name && propertyTracker.GetIndexParameters().Length > 0 && IsPropertyDefaultMember(propertyTracker))
				{
					flag2 = true;
				}
			}
			else if (memberTracker.MemberType == TrackerTypes.Field)
			{
				FieldInfo field = ((FieldTracker)memberTracker).Field;
				if (field.IsDefined(typeof(SlotFieldAttribute), inherit: false) && list == null)
				{
					list = MakeListWithPreviousMembers(group, list, i);
					memberTracker = new CustomAttributeTracker(memberTracker.DeclaringType, memberTracker.Name, (PythonTypeSlot)field.GetValue(null));
				}
			}
			if (flag2 && flag)
			{
				if (list == null)
				{
					list = MakeListWithPreviousMembers(group, list, i);
				}
			}
			else
			{
				list?.Add(memberTracker);
			}
		}
		if (list != null)
		{
			if (list.Count == 0)
			{
				return MemberGroup.EmptyGroup;
			}
			return new MemberGroup(list.ToArray());
		}
		return group;
	}

	private static bool IsPropertyWithParameters(MethodTracker meth)
	{
		if (meth.Method.Name.StartsWith("get_"))
		{
			if (!IsMethodDefaultMember(meth))
			{
				ParameterInfo[] parameters = meth.Method.GetParameters();
				if (parameters.Length > 0)
				{
					return true;
				}
			}
		}
		else if (meth.Method.Name.StartsWith("set_") && !IsMethodDefaultMember(meth))
		{
			ParameterInfo[] parameters2 = meth.Method.GetParameters();
			if (parameters2.Length > 1)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsPythonRecognizedOperator(string name)
	{
		if (name.StartsWith("get_") || name.StartsWith("set_"))
		{
			return true;
		}
		switch (name)
		{
		case "Abs":
		case "TrueDivide":
		case "FloorDivide":
		case "Power":
		case "DivMod":
			return true;
		default:
		{
			bool result = false;
			OperatorMapping operatorMapping = OperatorMapping.GetOperatorMapping(name);
			if (operatorMapping != null)
			{
				EnsureOperatorTable();
				if (_pythonOperatorTable.ContainsValue(operatorMapping.Operator))
				{
					result = true;
				}
			}
			return result;
		}
		}
	}

	private static bool IsPropertyDefaultMember(PropertyTracker pt)
	{
		MemberInfo[] defaultMembers = pt.DeclaringType.GetDefaultMembers();
		foreach (MemberInfo memberInfo in defaultMembers)
		{
			if (memberInfo.Name == pt.Name)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsMethodDefaultMember(MethodTracker pt)
	{
		MemberInfo[] defaultMembers = pt.DeclaringType.GetDefaultMembers();
		foreach (MemberInfo memberInfo in defaultMembers)
		{
			if (memberInfo.MemberType == MemberTypes.Property)
			{
				PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
				if (propertyInfo.GetGetMethod() == pt.Method || propertyInfo.GetSetMethod() == pt.Method)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static List<MemberTracker> MakeListWithPreviousMembers(MemberGroup group, List<MemberTracker> mts, int i)
	{
		mts = new List<MemberTracker>(i);
		for (int j = 0; j < i; j++)
		{
			mts.Add(group[j]);
		}
		return mts;
	}

	private static MethodInfo[] GetMethodSet(string name, int expected)
	{
		MethodInfo[] methods = typeof(InstanceOps).GetMethods();
		MethodInfo[] array = new MethodInfo[expected];
		int num = 0;
		for (int i = 0; i < methods.Length; i++)
		{
			if (methods[i].Name == name)
			{
				array[num++] = methods[i];
				if (num == expected)
				{
					break;
				}
			}
		}
		return array;
	}

	public static Dictionary<string, PythonOperationKind> InitializeOperatorTable()
	{
		Dictionary<string, PythonOperationKind> dictionary = new Dictionary<string, PythonOperationKind>();
		dictionary["__add__"] = PythonOperationKind.Add;
		dictionary["__radd__"] = PythonOperationKind.ReverseAdd;
		dictionary["__iadd__"] = PythonOperationKind.InPlaceAdd;
		dictionary["__sub__"] = PythonOperationKind.Subtract;
		dictionary["__rsub__"] = PythonOperationKind.ReverseSubtract;
		dictionary["__isub__"] = PythonOperationKind.InPlaceSubtract;
		dictionary["__pow__"] = PythonOperationKind.Power;
		dictionary["__rpow__"] = PythonOperationKind.ReversePower;
		dictionary["__ipow__"] = PythonOperationKind.InPlacePower;
		dictionary["__mul__"] = PythonOperationKind.Multiply;
		dictionary["__rmul__"] = PythonOperationKind.ReverseMultiply;
		dictionary["__imul__"] = PythonOperationKind.InPlaceMultiply;
		dictionary["__floordiv__"] = PythonOperationKind.FloorDivide;
		dictionary["__rfloordiv__"] = PythonOperationKind.ReverseFloorDivide;
		dictionary["__ifloordiv__"] = PythonOperationKind.InPlaceFloorDivide;
		dictionary["__div__"] = PythonOperationKind.Divide;
		dictionary["__rdiv__"] = PythonOperationKind.ReverseDivide;
		dictionary["__idiv__"] = PythonOperationKind.InPlaceDivide;
		dictionary["__truediv__"] = PythonOperationKind.TrueDivide;
		dictionary["__rtruediv__"] = PythonOperationKind.ReverseTrueDivide;
		dictionary["__itruediv__"] = PythonOperationKind.InPlaceTrueDivide;
		dictionary["__mod__"] = PythonOperationKind.Mod;
		dictionary["__rmod__"] = PythonOperationKind.ReverseMod;
		dictionary["__imod__"] = PythonOperationKind.InPlaceMod;
		dictionary["__lshift__"] = PythonOperationKind.LeftShift;
		dictionary["__rlshift__"] = PythonOperationKind.ReverseLeftShift;
		dictionary["__ilshift__"] = PythonOperationKind.InPlaceLeftShift;
		dictionary["__rshift__"] = PythonOperationKind.RightShift;
		dictionary["__rrshift__"] = PythonOperationKind.ReverseRightShift;
		dictionary["__irshift__"] = PythonOperationKind.InPlaceRightShift;
		dictionary["__and__"] = PythonOperationKind.BitwiseAnd;
		dictionary["__rand__"] = PythonOperationKind.ReverseBitwiseAnd;
		dictionary["__iand__"] = PythonOperationKind.InPlaceBitwiseAnd;
		dictionary["__or__"] = PythonOperationKind.BitwiseOr;
		dictionary["__ror__"] = PythonOperationKind.ReverseBitwiseOr;
		dictionary["__ior__"] = PythonOperationKind.InPlaceBitwiseOr;
		dictionary["__xor__"] = PythonOperationKind.ExclusiveOr;
		dictionary["__rxor__"] = PythonOperationKind.ReverseExclusiveOr;
		dictionary["__ixor__"] = PythonOperationKind.InPlaceExclusiveOr;
		dictionary["__lt__"] = PythonOperationKind.LessThan;
		dictionary["__gt__"] = PythonOperationKind.GreaterThan;
		dictionary["__le__"] = PythonOperationKind.LessThanOrEqual;
		dictionary["__ge__"] = PythonOperationKind.GreaterThanOrEqual;
		dictionary["__eq__"] = PythonOperationKind.Equal;
		dictionary["__ne__"] = PythonOperationKind.NotEqual;
		dictionary["__lg__"] = PythonOperationKind.LessThanGreaterThan;
		dictionary["__getitem__"] = PythonOperationKind.GetItem;
		dictionary["__setitem__"] = PythonOperationKind.SetItem;
		dictionary["__delitem__"] = PythonOperationKind.DeleteItem;
		dictionary["__cmp__"] = PythonOperationKind.Compare;
		dictionary["__pos__"] = PythonOperationKind.Positive;
		dictionary["__neg__"] = PythonOperationKind.Negate;
		dictionary["__invert__"] = PythonOperationKind.OnesComplement;
		dictionary["__len__"] = PythonOperationKind.Length;
		dictionary["__divmod__"] = PythonOperationKind.DivMod;
		dictionary["__rdivmod__"] = PythonOperationKind.ReverseDivMod;
		dictionary["__pow__"] = PythonOperationKind.Power;
		dictionary["__contains__"] = PythonOperationKind.Contains;
		dictionary["__abs__"] = PythonOperationKind.AbsoluteValue;
		return dictionary;
	}
}
