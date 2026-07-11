using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using IronPython.Compiler;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

[Documentation("type(object) -> gets the type of the object\r\ntype(name, bases, dict) -> creates a new type instance with the given name, base classes, and members from the dictionary")]
[PythonType("type")]
[DebuggerDisplay("PythonType: {Name}")]
[DebuggerTypeProxy(typeof(DebugProxy))]
public class PythonType : IPythonMembersList, IMembersList, IDynamicMetaObjectProvider, IWeakReferenceable, ICodeFormattable, IFastGettable, IFastSettable, IFastInvokable
{
	private abstract class FastBindingBuilderBase
	{
		private readonly CodeContext _context;

		private readonly PythonInvokeBinder _binder;

		private readonly PythonType _type;

		private readonly Type _siteType;

		private readonly Type[] _genTypeArgs;

		public FastBindingBuilderBase(CodeContext context, PythonType type, PythonInvokeBinder binder, Type siteType, Type[] genTypeArgs)
		{
			_context = context;
			_type = type;
			_binder = binder;
			_siteType = siteType;
			_genTypeArgs = genTypeArgs;
		}

		public virtual Delegate MakeBindingResult()
		{
			int version = _type.Version;
			_type.TryResolveSlot(_context, "__new__", out var slot);
			_type.TryResolveSlot(_context, "__init__", out var slot2);
			Delegate obj;
			if (slot != InstanceOps.New)
			{
				obj = ((!(slot.GetType() == typeof(staticmethod)) || !(((staticmethod)slot)._func is PythonFunction)) ? null : GetNewSiteDelegate(_binder.Context.Invoke(_binder.Signature.InsertArgument(Argument.Simple)), ((staticmethod)slot)._func));
			}
			else
			{
				if (_genTypeArgs.Length > 0 && slot2 == InstanceOps.Init)
				{
					return null;
				}
				obj = GetOrCreateFastNew();
			}
			if ((object)obj == null)
			{
				return null;
			}
			return MakeDelegate(version, obj, _type.GetLateBoundInitBinder(_binder.Signature));
		}

		private Delegate GetOrCreateFastNew()
		{
			lock (_fastBindCtors)
			{
				Delegate value2;
				if (!_fastBindCtors.TryGetValue(_type.UnderlyingSystemType, out var value))
				{
					Dictionary<Type, Delegate> dictionary = (_fastBindCtors[_type.UnderlyingSystemType] = new Dictionary<Type, Delegate>());
					value = dictionary;
				}
				else if (value.TryGetValue(_siteType, out value2))
				{
					return value2;
				}
				ConstructorInfo[] constructors = _type.UnderlyingSystemType.GetConstructors();
				if (constructors.Length == 1 && constructors[0].GetParameters().Length == 1 && constructors[0].GetParameters()[0].ParameterType == typeof(PythonType))
				{
					ParameterExpression parameterExpression = Expression.Parameter(typeof(CodeContext));
					ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
					ParameterExpression[] array = new ParameterExpression[_genTypeArgs.Length + 2];
					array[0] = parameterExpression;
					array[1] = parameterExpression2;
					for (int i = 0; i < _genTypeArgs.Length; i++)
					{
						array[i + 2] = Expression.Parameter(_genTypeArgs[i]);
					}
					value2 = Expression.Lambda(Expression.Convert(Expression.New(constructors[0], Expression.Convert(parameterExpression2, typeof(PythonType))), typeof(object)), array).Compile();
					value[_siteType] = value2;
					return value2;
				}
				return null;
			}
		}

		protected abstract Delegate GetNewSiteDelegate(PythonInvokeBinder binder, object func);

		protected abstract Delegate MakeDelegate(int version, Delegate newDlg, LateBoundInitBinder initBinder);
	}

	private class FastBindingBuilder : FastBindingBuilderBase
	{
		public FastBindingBuilder(CodeContext context, PythonType type, PythonInvokeBinder binder, Type siteType, Type[] genTypeArgs)
			: base(context, type, binder, siteType, genTypeArgs)
		{
		}

		protected override Delegate GetNewSiteDelegate(PythonInvokeBinder binder, object func)
		{
			return new Func<CodeContext, object, object>(new NewSite(binder, func).Call);
		}

		protected override Delegate MakeDelegate(int version, Delegate newDlg, LateBoundInitBinder initBinder)
		{
			return new Func<CallSite, CodeContext, object, object>(new FastTypeSite(version, (Func<CodeContext, object, object>)newDlg, initBinder).CallTarget);
		}
	}

	private class FastTypeSite
	{
		private readonly int _version;

		private readonly Func<CodeContext, object, object> _new;

		private readonly CallSite<Func<CallSite, CodeContext, object, object>> _initSite;

		public FastTypeSite(int version, Func<CodeContext, object, object> @new, LateBoundInitBinder initBinder)
		{
			_version = version;
			_new = @new;
			_initSite = CallSite<Func<CallSite, CodeContext, object, object>>.Create(initBinder);
		}

		public object CallTarget(CallSite site, CodeContext context, object type)
		{
			if (type is PythonType pythonType && pythonType.Version == _version)
			{
				object obj = _new(context, type);
				_initSite.Target(_initSite, context, obj);
				return obj;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, type);
		}
	}

	private class NewSite
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, object>> _site;

		private readonly object _target;

		public NewSite(PythonInvokeBinder binder, object target)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, object>>.Create(binder);
			_target = target;
		}

		public object Call(CodeContext context, object type)
		{
			return _site.Target(_site, context, _target, type);
		}
	}

	private class FastBindingBuilder<T0> : FastBindingBuilderBase
	{
		public FastBindingBuilder(CodeContext context, PythonType type, PythonInvokeBinder binder, Type siteType, Type[] genTypeArgs)
			: base(context, type, binder, siteType, genTypeArgs)
		{
		}

		protected override Delegate GetNewSiteDelegate(PythonInvokeBinder binder, object func)
		{
			return new Func<CodeContext, object, T0, object>(new NewSite<T0>(binder, func).Call);
		}

		protected override Delegate MakeDelegate(int version, Delegate newDlg, LateBoundInitBinder initBinder)
		{
			return new Func<CallSite, CodeContext, object, T0, object>(new FastTypeSite<T0>(version, (Func<CodeContext, object, T0, object>)newDlg, initBinder).CallTarget);
		}
	}

	private class FastTypeSite<T0>
	{
		private readonly int _version;

		private readonly Func<CodeContext, object, T0, object> _new;

		private readonly CallSite<Func<CallSite, CodeContext, object, T0, object>> _initSite;

		public FastTypeSite(int version, Func<CodeContext, object, T0, object> @new, LateBoundInitBinder initBinder)
		{
			_version = version;
			_new = @new;
			_initSite = CallSite<Func<CallSite, CodeContext, object, T0, object>>.Create(initBinder);
		}

		public object CallTarget(CallSite site, CodeContext context, object type, T0 arg0)
		{
			if (type is PythonType pythonType && pythonType.Version == _version)
			{
				object obj = _new(context, type, arg0);
				_initSite.Target(_initSite, context, obj, arg0);
				return obj;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, type, arg0);
		}
	}

	private class NewSite<T0>
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, T0, object>> _site;

		private readonly object _target;

		public NewSite(PythonInvokeBinder binder, object target)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, object>>.Create(binder);
			_target = target;
		}

		public object Call(CodeContext context, object typeOrInstance, T0 arg0)
		{
			return _site.Target(_site, context, _target, typeOrInstance, arg0);
		}
	}

	private class FastBindingBuilder<T0, T1> : FastBindingBuilderBase
	{
		public FastBindingBuilder(CodeContext context, PythonType type, PythonInvokeBinder binder, Type siteType, Type[] genTypeArgs)
			: base(context, type, binder, siteType, genTypeArgs)
		{
		}

		protected override Delegate GetNewSiteDelegate(PythonInvokeBinder binder, object func)
		{
			return new Func<CodeContext, object, T0, T1, object>(new NewSite<T0, T1>(binder, func).Call);
		}

		protected override Delegate MakeDelegate(int version, Delegate newDlg, LateBoundInitBinder initBinder)
		{
			return new Func<CallSite, CodeContext, object, T0, T1, object>(new FastTypeSite<T0, T1>(version, (Func<CodeContext, object, T0, T1, object>)newDlg, initBinder).CallTarget);
		}
	}

	private class FastTypeSite<T0, T1>
	{
		private readonly int _version;

		private readonly Func<CodeContext, object, T0, T1, object> _new;

		private readonly CallSite<Func<CallSite, CodeContext, object, T0, T1, object>> _initSite;

		public FastTypeSite(int version, Func<CodeContext, object, T0, T1, object> @new, LateBoundInitBinder initBinder)
		{
			_version = version;
			_new = @new;
			_initSite = CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>.Create(initBinder);
		}

		public object CallTarget(CallSite site, CodeContext context, object type, T0 arg0, T1 arg1)
		{
			if (type is PythonType pythonType && pythonType.Version == _version)
			{
				object obj = _new(context, type, arg0, arg1);
				_initSite.Target(_initSite, context, obj, arg0, arg1);
				return obj;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, type, arg0, arg1);
		}
	}

	private class NewSite<T0, T1>
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, T0, T1, object>> _site;

		private readonly object _target;

		public NewSite(PythonInvokeBinder binder, object target)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, object>>.Create(binder);
			_target = target;
		}

		public object Call(CodeContext context, object typeOrInstance, T0 arg0, T1 arg1)
		{
			return _site.Target(_site, context, _target, typeOrInstance, arg0, arg1);
		}
	}

	private class FastBindingBuilder<T0, T1, T2> : FastBindingBuilderBase
	{
		public FastBindingBuilder(CodeContext context, PythonType type, PythonInvokeBinder binder, Type siteType, Type[] genTypeArgs)
			: base(context, type, binder, siteType, genTypeArgs)
		{
		}

		protected override Delegate GetNewSiteDelegate(PythonInvokeBinder binder, object func)
		{
			return new Func<CodeContext, object, T0, T1, T2, object>(new NewSite<T0, T1, T2>(binder, func).Call);
		}

		protected override Delegate MakeDelegate(int version, Delegate newDlg, LateBoundInitBinder initBinder)
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, object>(new FastTypeSite<T0, T1, T2>(version, (Func<CodeContext, object, T0, T1, T2, object>)newDlg, initBinder).CallTarget);
		}
	}

	private class FastTypeSite<T0, T1, T2>
	{
		private readonly int _version;

		private readonly Func<CodeContext, object, T0, T1, T2, object> _new;

		private readonly CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>> _initSite;

		public FastTypeSite(int version, Func<CodeContext, object, T0, T1, T2, object> @new, LateBoundInitBinder initBinder)
		{
			_version = version;
			_new = @new;
			_initSite = CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>.Create(initBinder);
		}

		public object CallTarget(CallSite site, CodeContext context, object type, T0 arg0, T1 arg1, T2 arg2)
		{
			if (type is PythonType pythonType && pythonType.Version == _version)
			{
				object obj = _new(context, type, arg0, arg1, arg2);
				_initSite.Target(_initSite, context, obj, arg0, arg1, arg2);
				return obj;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, type, arg0, arg1, arg2);
		}
	}

	private class NewSite<T0, T1, T2>
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, object>> _site;

		private readonly object _target;

		public NewSite(PythonInvokeBinder binder, object target)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, object>>.Create(binder);
			_target = target;
		}

		public object Call(CodeContext context, object typeOrInstance, T0 arg0, T1 arg1, T2 arg2)
		{
			return _site.Target(_site, context, _target, typeOrInstance, arg0, arg1, arg2);
		}
	}

	private class FastBindingBuilder<T0, T1, T2, T3> : FastBindingBuilderBase
	{
		public FastBindingBuilder(CodeContext context, PythonType type, PythonInvokeBinder binder, Type siteType, Type[] genTypeArgs)
			: base(context, type, binder, siteType, genTypeArgs)
		{
		}

		protected override Delegate GetNewSiteDelegate(PythonInvokeBinder binder, object func)
		{
			return new Func<CodeContext, object, T0, T1, T2, T3, object>(new NewSite<T0, T1, T2, T3>(binder, func).Call);
		}

		protected override Delegate MakeDelegate(int version, Delegate newDlg, LateBoundInitBinder initBinder)
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>(new FastTypeSite<T0, T1, T2, T3>(version, (Func<CodeContext, object, T0, T1, T2, T3, object>)newDlg, initBinder).CallTarget);
		}
	}

	private class FastTypeSite<T0, T1, T2, T3>
	{
		private readonly int _version;

		private readonly Func<CodeContext, object, T0, T1, T2, T3, object> _new;

		private readonly CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>> _initSite;

		public FastTypeSite(int version, Func<CodeContext, object, T0, T1, T2, T3, object> @new, LateBoundInitBinder initBinder)
		{
			_version = version;
			_new = @new;
			_initSite = CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>.Create(initBinder);
		}

		public object CallTarget(CallSite site, CodeContext context, object type, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			if (type is PythonType pythonType && pythonType.Version == _version)
			{
				object obj = _new(context, type, arg0, arg1, arg2, arg3);
				_initSite.Target(_initSite, context, obj, arg0, arg1, arg2, arg3);
				return obj;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, type, arg0, arg1, arg2, arg3);
		}
	}

	private class NewSite<T0, T1, T2, T3>
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, object>> _site;

		private readonly object _target;

		public NewSite(PythonInvokeBinder binder, object target)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, object>>.Create(binder);
			_target = target;
		}

		public object Call(CodeContext context, object typeOrInstance, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			return _site.Target(_site, context, _target, typeOrInstance, arg0, arg1, arg2, arg3);
		}
	}

	private class FastBindingBuilder<T0, T1, T2, T3, T4> : FastBindingBuilderBase
	{
		public FastBindingBuilder(CodeContext context, PythonType type, PythonInvokeBinder binder, Type siteType, Type[] genTypeArgs)
			: base(context, type, binder, siteType, genTypeArgs)
		{
		}

		protected override Delegate GetNewSiteDelegate(PythonInvokeBinder binder, object func)
		{
			return new Func<CodeContext, object, T0, T1, T2, T3, T4, object>(new NewSite<T0, T1, T2, T3, T4>(binder, func).Call);
		}

		protected override Delegate MakeDelegate(int version, Delegate newDlg, LateBoundInitBinder initBinder)
		{
			return new Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>(new FastTypeSite<T0, T1, T2, T3, T4>(version, (Func<CodeContext, object, T0, T1, T2, T3, T4, object>)newDlg, initBinder).CallTarget);
		}
	}

	private class FastTypeSite<T0, T1, T2, T3, T4>
	{
		private readonly int _version;

		private readonly Func<CodeContext, object, T0, T1, T2, T3, T4, object> _new;

		private readonly CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>> _initSite;

		public FastTypeSite(int version, Func<CodeContext, object, T0, T1, T2, T3, T4, object> @new, LateBoundInitBinder initBinder)
		{
			_version = version;
			_new = @new;
			_initSite = CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>.Create(initBinder);
		}

		public object CallTarget(CallSite site, CodeContext context, object type, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (type is PythonType pythonType && pythonType.Version == _version)
			{
				object obj = _new(context, type, arg0, arg1, arg2, arg3, arg4);
				_initSite.Target(_initSite, context, obj, arg0, arg1, arg2, arg3, arg4);
				return obj;
			}
			return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, type, arg0, arg1, arg2, arg3, arg4);
		}
	}

	private class NewSite<T0, T1, T2, T3, T4>
	{
		private readonly CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, object>> _site;

		private readonly object _target;

		public NewSite(PythonInvokeBinder binder, object target)
		{
			_site = CallSite<Func<CallSite, CodeContext, object, object, T0, T1, T2, T3, T4, object>>.Create(binder);
			_target = target;
		}

		public object Call(CodeContext context, object typeOrInstance, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			return _site.Target(_site, context, _target, typeOrInstance, arg0, arg1, arg2, arg3, arg4);
		}
	}

	[Flags]
	private enum PythonTypeAttributes
	{
		None = 0,
		Immutable = 1,
		SystemType = 2,
		IsPythonType = 4,
		WeakReferencable = 8,
		HasDictionary = 0x10,
		SystemCtor = 0x20
	}

	private class Setter<T> : FastSetBase<T>
	{
		private readonly CodeContext _context;

		private readonly string _name;

		public Setter(CodeContext context, string name)
			: base(-1)
		{
			_context = context;
			_name = name;
		}

		public object Target(CallSite site, object self, T value)
		{
			if (self is PythonType { IsSystemType: false } pythonType)
			{
				pythonType.SetCustomMember(_context, _name, value);
				return value;
			}
			return FastSetBase<T>.Update(site, self, value);
		}
	}

	internal class DebugProxy
	{
		private readonly PythonType _type;

		public PythonType[] __bases__ => ArrayUtils.ToArray(_type.BaseTypes);

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public List<ObjectDebugView> Members
		{
			get
			{
				List<ObjectDebugView> list = new List<ObjectDebugView>();
				if (_type._dict != null)
				{
					foreach (KeyValuePair<string, PythonTypeSlot> item in _type._dict)
					{
						if (item.Value is PythonTypeUserDescriptorSlot)
						{
							list.Add(new ObjectDebugView(item.Key, ((PythonTypeUserDescriptorSlot)item.Value).Value));
						}
						else
						{
							list.Add(new ObjectDebugView(item.Key, item.Value));
						}
					}
				}
				return list;
			}
		}

		public DebugProxy(PythonType type)
		{
			_type = type;
		}
	}

	private const int TypeFlagHeapType = 512;

	private const int TypeFlagAbstractMethodsDefined = 524288;

	private const int TypeFlagAbstractMethodsNonEmpty = 1048576;

	private Type _underlyingSystemType;

	private string _name;

	private Dictionary<string, PythonTypeSlot> _dict;

	private PythonTypeAttributes _attrs;

	private int _flags;

	private int _version = GetNextVersion();

	private List<WeakReference> _subtypes;

	private PythonContext _pythonContext;

	private bool? _objectNew;

	private bool? _objectInit;

	internal Dictionary<CachedGetKey, FastGetBase> _cachedGets;

	internal Dictionary<CachedGetKey, FastGetBase> _cachedTryGets;

	internal Dictionary<SetMemberKey, FastSetBase> _cachedSets;

	internal Dictionary<string, TypeGetBase> _cachedTypeGets;

	internal Dictionary<string, TypeGetBase> _cachedTypeTryGets;

	private List<PythonType> _resolutionOrder;

	private PythonType[] _bases;

	private BuiltinFunction _ctor;

	private Type _finalSystemType;

	private WeakRefTracker _weakrefTracker;

	private WeakReference _weakRef;

	private string[] _slots;

	private OldClass _oldClass;

	private int _originalSlotCount;

	private InstanceCreator _instanceCtor;

	private CallSite<Func<CallSite, object, int>> _hashSite;

	private CallSite<Func<CallSite, object, object, bool>> _eqSite;

	private CallSite<Func<CallSite, object, object, int>> _compareSite;

	private Dictionary<CallSignature, LateBoundInitBinder> _lateBoundInitBinders;

	private string[] _optimizedInstanceNames;

	private int _optimizedInstanceVersion;

	private Dictionary<string, List<MethodInfo>> _extensionMethods;

	private PythonSiteCache _siteCache = new PythonSiteCache();

	private PythonTypeSlot _lenSlot;

	internal Func<string, Exception> _makeException = DefaultMakeException;

	private static int MasterVersion = 1;

	private static readonly CommonDictionaryStorage _pythonTypes = new CommonDictionaryStorage();

	internal static PythonType _pythonTypeType = DynamicHelpers.GetPythonTypeFromType(typeof(PythonType));

	private static readonly WeakReference[] _emptyWeakRef = new WeakReference[0];

	private static object _subtypesLock = new object();

	internal static Func<string, Exception> DefaultMakeException = (string message) => new Exception(message);

	private static Dictionary<Type, Dictionary<Type, Delegate>> _fastBindCtors = new Dictionary<Type, Dictionary<Type, Delegate>>();

	private static Dictionary<Type, BuiltinFunction> _userTypeCtors = new Dictionary<Type, BuiltinFunction>();

	[SlotField]
	public static PythonTypeSlot __dict__ = new PythonTypeDictSlot(_pythonTypeType);

	internal BuiltinFunction Ctor
	{
		get
		{
			EnsureConstructor();
			return _ctor;
		}
	}

	public PythonType this[params Type[] args]
	{
		get
		{
			if (UnderlyingSystemType == typeof(Array))
			{
				if (args.Length == 1)
				{
					return DynamicHelpers.GetPythonTypeFromType(args[0].MakeArrayType());
				}
				throw PythonOps.TypeError("expected one argument to make array type, got {0}", args.Length);
			}
			if (!UnderlyingSystemType.IsGenericTypeDefinition)
			{
				throw new InvalidOperationException("MakeGenericType on non-generic type");
			}
			return DynamicHelpers.GetPythonTypeFromType(UnderlyingSystemType.MakeGenericType(args));
		}
	}

	internal int SlotCount => _originalSlotCount;

	internal string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal int Version => _version;

	internal bool IsNull => UnderlyingSystemType == typeof(DynamicNull);

	internal IList<PythonType> ResolutionOrder
	{
		get
		{
			return _resolutionOrder;
		}
		set
		{
			lock (SyncRoot)
			{
				_resolutionOrder = new List<PythonType>(value);
			}
		}
	}

	internal CallSite<Func<CallSite, object, int>> HashSite
	{
		get
		{
			EnsureHashSite();
			return _hashSite;
		}
	}

	internal Type UnderlyingSystemType => _underlyingSystemType;

	internal Type FinalSystemType => _finalSystemType ?? (_finalSystemType = PythonTypeOps.GetFinalSystemType(_underlyingSystemType));

	internal Type ExtensionType
	{
		get
		{
			if (!_underlyingSystemType.IsEnum)
			{
				switch (Type.GetTypeCode(_underlyingSystemType))
				{
				case TypeCode.String:
					return typeof(ExtensibleString);
				case TypeCode.Int32:
					return typeof(Extensible<int>);
				case TypeCode.Double:
					return typeof(Extensible<double>);
				case TypeCode.Object:
					if (_underlyingSystemType == typeof(BigInteger))
					{
						return typeof(Extensible<BigInteger>);
					}
					if (_underlyingSystemType == typeof(Complex))
					{
						return typeof(ExtensibleComplex);
					}
					break;
				}
			}
			return _underlyingSystemType;
		}
	}

	internal IList<PythonType> BaseTypes
	{
		get
		{
			return _bases;
		}
		set
		{
			foreach (PythonType item in value)
			{
				if (item == null)
				{
					throw new ArgumentNullException("value", "a PythonType was null while assigning base classes");
				}
			}
			lock (_bases)
			{
				PythonType[] bases = _bases;
				foreach (PythonType pythonType in bases)
				{
					pythonType.RemoveSubType(this);
				}
				List<PythonType> list = new List<PythonType>(value);
				foreach (PythonType item2 in list)
				{
					item2.AddSubType(this);
				}
				UpdateVersion();
				_bases = list.ToArray();
			}
		}
	}

	internal bool IsSystemType
	{
		get
		{
			return (_attrs & PythonTypeAttributes.SystemType) != 0;
		}
		set
		{
			if (value)
			{
				_attrs |= PythonTypeAttributes.SystemType;
			}
			else
			{
				_attrs &= ~PythonTypeAttributes.SystemType;
			}
		}
	}

	internal bool IsWeakReferencable
	{
		get
		{
			return (_attrs & PythonTypeAttributes.WeakReferencable) != 0;
		}
		set
		{
			if (value)
			{
				_attrs |= PythonTypeAttributes.WeakReferencable;
			}
			else
			{
				_attrs &= ~PythonTypeAttributes.WeakReferencable;
			}
		}
	}

	internal bool HasDictionary
	{
		get
		{
			return (_attrs & PythonTypeAttributes.HasDictionary) != 0;
		}
		set
		{
			if (value)
			{
				_attrs |= PythonTypeAttributes.HasDictionary;
			}
			else
			{
				_attrs &= ~PythonTypeAttributes.HasDictionary;
			}
		}
	}

	internal bool HasSystemCtor => (_attrs & PythonTypeAttributes.SystemCtor) != 0;

	internal bool IsPythonType
	{
		get
		{
			return (_attrs & PythonTypeAttributes.IsPythonType) != 0;
		}
		set
		{
			if (value)
			{
				_attrs |= PythonTypeAttributes.IsPythonType;
			}
			else
			{
				_attrs &= ~PythonTypeAttributes.IsPythonType;
			}
		}
	}

	internal OldClass OldClass
	{
		get
		{
			return _oldClass;
		}
		set
		{
			_oldClass = value;
		}
	}

	internal bool IsOldClass => _oldClass != null;

	internal PythonContext PythonContext => _pythonContext;

	internal PythonContext Context => _pythonContext ?? DefaultContext.DefaultPythonContext;

	internal object SyncRoot => this;

	internal Dictionary<string, List<MethodInfo>> ExtensionMethods
	{
		get
		{
			if (_extensionMethods == null)
			{
				Dictionary<string, List<MethodInfo>> dictionary = new Dictionary<string, List<MethodInfo>>();
				MethodInfo[] methods = UnderlyingSystemType.GetMethods(BindingFlags.Static | BindingFlags.Public);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.IsExtension())
					{
						if (!dictionary.TryGetValue(methodInfo.Name, out var value))
						{
							value = (dictionary[methodInfo.Name] = new List<MethodInfo>());
						}
						value.Add(methodInfo);
					}
				}
				_extensionMethods = dictionary;
			}
			return _extensionMethods;
		}
	}

	private IList<WeakReference> SubTypes
	{
		get
		{
			if (_subtypes == null)
			{
				return _emptyWeakRef;
			}
			lock (_subtypesLock)
			{
				return _subtypes.ToArray();
			}
		}
	}

	FastBindResult<T> IFastInvokable.MakeInvokeBinding<T>(CallSite<T> site, PythonInvokeBinder binder, CodeContext context, object[] args)
	{
		ParameterInfo[] parameters = typeof(T).GetMethod("Invoke").GetParameters();
		if (parameters[2].ParameterType != typeof(object))
		{
			return default(FastBindResult<T>);
		}
		if (binder.Signature.IsSimple)
		{
			if (this == TypeCache.PythonType && args.Length == 1 && parameters[3].ParameterType == typeof(object))
			{
				return new FastBindResult<T>((T)(object)new Func<CallSite, CodeContext, object, object, object>(GetPythonType), shouldCache: true);
			}
			if (this == TypeCache.Set && args.Length == 0)
			{
				return new FastBindResult<T>((T)(object)new Func<CallSite, CodeContext, object, object>(EmptySet), shouldCache: true);
			}
			if (this == TypeCache.Object && args.Length == 0)
			{
				return new FastBindResult<T>((T)(object)new Func<CallSite, CodeContext, object, object>(NewObject), shouldCache: true);
			}
		}
		if (IsSystemType || IsMixedNewStyleOldStyle() || args.Length > 5 || HasSystemCtor || GetType() != typeof(PythonType) || HasAbstractMethods(context))
		{
			return default(FastBindResult<T>);
		}
		Type[] array = new Type[parameters.Length - 3];
		for (int i = 0; i < parameters.Length - 3; i++)
		{
			array[i] = parameters[i + 3].ParameterType;
		}
		FastBindingBuilderBase fastBindingBuilderBase = ((array.Length != 0) ? ((FastBindingBuilderBase)Activator.CreateInstance((array.Length switch
		{
			1 => typeof(FastBindingBuilder<>), 
			2 => typeof(FastBindingBuilder<, >), 
			3 => typeof(FastBindingBuilder<, , >), 
			4 => typeof(FastBindingBuilder<, , , >), 
			5 => typeof(FastBindingBuilder<, , , , >), 
			_ => throw new NotImplementedException(), 
		}).MakeGenericType(array), context, this, binder, typeof(T), array)) : new FastBindingBuilder(context, this, binder, typeof(T), array));
		return new FastBindResult<T>((T)(object)fastBindingBuilderBase.MakeBindingResult(), shouldCache: true);
	}

	private object GetPythonType(CallSite site, CodeContext context, object type, object instance)
	{
		if (type == TypeCache.PythonType)
		{
			return DynamicHelpers.GetPythonType(instance);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object, object>>)site).Update(site, context, type, instance);
	}

	private object EmptySet(CallSite site, CodeContext context, object type)
	{
		if (type == TypeCache.Set)
		{
			return new SetCollection();
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, type);
	}

	private object NewObject(CallSite site, CodeContext context, object type)
	{
		if (type == TypeCache.Object)
		{
			return new object();
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, type);
	}

	public PythonType(CodeContext context, string name, PythonTuple bases, PythonDictionary dict)
		: this(context, name, bases, dict, string.Empty)
	{
	}

	internal PythonType(CodeContext context, string name, PythonTuple bases, PythonDictionary dict, string selfNames)
	{
		InitializeUserType(context, name, bases, dict, selfNames);
	}

	internal PythonType()
	{
	}

	internal PythonType(Type underlyingSystemType)
	{
		_underlyingSystemType = underlyingSystemType;
		InitializeSystemType();
	}

	internal PythonType(PythonType baseType, string name, Func<string, Exception> exceptionMaker)
	{
		_underlyingSystemType = baseType.UnderlyingSystemType;
		IsSystemType = baseType.IsSystemType;
		IsPythonType = baseType.IsPythonType;
		Name = name;
		_bases = new PythonType[1] { baseType };
		ResolutionOrder = Mro.Calculate(this, _bases);
		_attrs |= PythonTypeAttributes.HasDictionary;
		_makeException = exceptionMaker;
	}

	internal PythonType(PythonType[] baseTypes, string name)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (PythonType pythonType in baseTypes)
		{
			flag |= pythonType.IsSystemType;
			flag2 |= pythonType.IsPythonType;
		}
		IsSystemType = flag;
		IsPythonType = flag2;
		Name = name;
		_bases = baseTypes;
		ResolutionOrder = Mro.Calculate(this, _bases);
		_attrs |= PythonTypeAttributes.HasDictionary;
	}

	internal PythonType(PythonType[] baseTypes, Type underlyingType, string name, Func<string, Exception> exceptionMaker)
		: this(baseTypes, name)
	{
		_underlyingSystemType = underlyingType;
		_makeException = exceptionMaker;
	}

	internal PythonType(PythonContext context, PythonType baseType, string name, string module, string doc, Func<string, Exception> exceptionMaker)
		: this(baseType, name, exceptionMaker)
	{
		EnsureDict();
		_dict["__doc__"] = new PythonTypeUserDescriptorSlot(doc, isntDescriptor: true);
		_dict["__module__"] = new PythonTypeUserDescriptorSlot(module, isntDescriptor: true);
		IsSystemType = false;
		IsPythonType = false;
		_pythonContext = context;
		_attrs |= PythonTypeAttributes.HasDictionary;
	}

	internal PythonType(PythonContext context, PythonType[] baseTypes, string name, string module, string doc)
		: this(baseTypes, name)
	{
		EnsureDict();
		_dict["__doc__"] = new PythonTypeUserDescriptorSlot(doc, isntDescriptor: true);
		_dict["__module__"] = new PythonTypeUserDescriptorSlot(module, isntDescriptor: true);
		_pythonContext = context;
		_attrs |= PythonTypeAttributes.HasDictionary;
	}

	internal PythonType(PythonContext context, PythonType[] baseTypes, Type underlyingType, string name, string module, string doc, Func<string, Exception> exceptionMaker)
		: this(baseTypes, underlyingType, name, exceptionMaker)
	{
		EnsureDict();
		_dict["__doc__"] = new PythonTypeUserDescriptorSlot(doc, isntDescriptor: true);
		_dict["__module__"] = new PythonTypeUserDescriptorSlot(module, isntDescriptor: true);
		IsSystemType = false;
		IsPythonType = false;
		_pythonContext = context;
		_attrs |= PythonTypeAttributes.HasDictionary;
	}

	internal PythonType(OldClass oc)
	{
		EnsureDict();
		_underlyingSystemType = typeof(OldInstance);
		Name = oc.Name;
		OldClass = oc;
		List<PythonType> list = new List<PythonType>(oc.BaseClasses.Count);
		foreach (OldClass baseClass in oc.BaseClasses)
		{
			list.Add(baseClass.TypeObject);
		}
		List<PythonType> resolutionOrder = new List<PythonType> { this };
		_bases = list.ToArray();
		_resolutionOrder = resolutionOrder;
		AddSlot("__class__", new PythonTypeUserDescriptorSlot(this, isntDescriptor: true));
	}

	public static object __new__(CodeContext context, PythonType cls, string name, PythonTuple bases, PythonDictionary dict)
	{
		return __new__(context, cls, name, bases, dict, string.Empty);
	}

	internal static object __new__(CodeContext context, PythonType cls, string name, PythonTuple bases, PythonDictionary dict, string selfNames)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("type() argument 1 must be string, not None");
		}
		if (bases == null)
		{
			throw PythonOps.TypeError("type() argument 2 must be tuple, not None");
		}
		if (dict == null)
		{
			throw PythonOps.TypeError("TypeError: type() argument 3 must be dict, not None");
		}
		EnsureModule(context, dict);
		PythonType pythonType = FindMetaClass(cls, bases);
		if (pythonType != TypeCache.OldInstance && pythonType != TypeCache.PythonType)
		{
			if (pythonType != cls)
			{
				return PythonCalls.Call(context, pythonType, name, bases, dict);
			}
			return pythonType.CreateInstance(context, name, bases, dict);
		}
		return new PythonType(context, name, bases, dict, selfNames);
	}

	public void __init__(string name, PythonTuple bases, PythonDictionary dict)
	{
	}

	internal static PythonType FindMetaClass(PythonType cls, PythonTuple bases)
	{
		PythonType pythonType = cls;
		foreach (object basis in bases)
		{
			PythonType pythonType2 = DynamicHelpers.GetPythonType(basis);
			if (pythonType2 != TypeCache.OldClass && !pythonType.IsSubclassOf(pythonType2))
			{
				if (!pythonType2.IsSubclassOf(pythonType))
				{
					throw PythonOps.TypeError("metaclass conflict {0} and {1}", pythonType2.Name, pythonType.Name);
				}
				pythonType = pythonType2;
			}
		}
		return pythonType;
	}

	public static object __new__(CodeContext context, object cls, object o)
	{
		return DynamicHelpers.GetPythonType(o);
	}

	public void __init__(object o)
	{
	}

	[SpecialName]
	[WrapperDescriptor]
	[PropertyMethod]
	public static PythonTuple Get__bases__(CodeContext context, PythonType type)
	{
		return type.GetBasesTuple();
	}

	private PythonTuple GetBasesTuple()
	{
		object[] array = new object[BaseTypes.Count];
		IList<PythonType> baseTypes = BaseTypes;
		for (int i = 0; i < baseTypes.Count; i++)
		{
			PythonType pythonType = baseTypes[i];
			if (pythonType.IsOldClass)
			{
				array[i] = pythonType.OldClass;
			}
			else
			{
				array[i] = pythonType;
			}
		}
		return PythonTuple.MakeTuple(array);
	}

	[SpecialName]
	[PropertyMethod]
	[WrapperDescriptor]
	public static PythonType Get__base__(CodeContext context, PythonType type)
	{
		foreach (object item in Get__bases__(context, type))
		{
			if (item is PythonType result)
			{
				return result;
			}
		}
		return null;
	}

	private bool SetAbstractMethodFlags(string name, object value)
	{
		if (name != "__abstractmethods__")
		{
			return false;
		}
		int num = _flags | 0x80000;
		num = ((value != null && PythonOps.TryGetEnumerator(DefaultContext.Default, value, out var enumerator) && enumerator.MoveNext()) ? (num | 0x100000) : (num & -1048577));
		_flags = num;
		return true;
	}

	private void ClearAbstractMethodFlags(string name)
	{
		if (name == "__abstractmethods__")
		{
			_flags &= -1572865;
		}
	}

	internal bool HasAbstractMethods(CodeContext context)
	{
		if ((_flags & 0x100000) != 0 && TryGetBoundCustomMember(context, "__abstractmethods__", out var value) && PythonOps.TryGetEnumerator(context, value, out var enumerator))
		{
			return enumerator.MoveNext();
		}
		return false;
	}

	internal string GetAbstractErrorMessage(CodeContext context)
	{
		if ((_flags & 0x100000) == 0)
		{
			return null;
		}
		if (!TryGetBoundCustomMember(context, "__abstractmethods__", out var value) || !PythonOps.TryGetEnumerator(context, value, out var enumerator) || !enumerator.MoveNext())
		{
			return null;
		}
		string value2 = "";
		StringBuilder stringBuilder = new StringBuilder("Can't instantiate abstract class ");
		stringBuilder.Append(Name);
		stringBuilder.Append(" with abstract methods ");
		int num = 0;
		do
		{
			string text = enumerator.Current as string;
			if (text == null && enumerator.Current is Extensible<string> extensible)
			{
				text = extensible.Value;
			}
			if (text == null)
			{
				return $"sequence item {num}: expected string, {PythonTypeOps.GetName(enumerator.Current)} found";
			}
			stringBuilder.Append(value2);
			stringBuilder.Append(enumerator.Current);
			value2 = ", ";
			num++;
		}
		while (enumerator.MoveNext());
		return stringBuilder.ToString();
	}

	[SpecialName]
	[PropertyMethod]
	[WrapperDescriptor]
	public static int Get__flags__(CodeContext context, PythonType type)
	{
		int num = type._flags;
		if (type.IsSystemType)
		{
			num |= 0x200;
		}
		return num;
	}

	[SpecialName]
	[WrapperDescriptor]
	[PropertyMethod]
	public static void Set__bases__(CodeContext context, PythonType type, object value)
	{
		if (!(value is PythonTuple pythonTuple))
		{
			throw PythonOps.TypeError("expected tuple of types or old-classes, got '{0}'", PythonTypeOps.GetName(value));
		}
		List<PythonType> list = new List<PythonType>();
		foreach (object item in pythonTuple)
		{
			PythonType pythonType = item as PythonType;
			if (pythonType == null)
			{
				if (!(item is OldClass oldClass))
				{
					throw PythonOps.TypeError("expected tuple of types, got '{0}'", PythonTypeOps.GetName(item));
				}
				pythonType = oldClass.TypeObject;
			}
			list.Add(pythonType);
		}
		Type newType = NewTypeMaker.GetNewType(type.Name, pythonTuple);
		if (type.UnderlyingSystemType != newType)
		{
			throw PythonOps.TypeErrorForIncompatibleObjectLayout("__bases__ assignment", type, newType);
		}
		List<PythonType> resolutionOrder = CalculateMro(type, list);
		type.BaseTypes = list;
		type._resolutionOrder = resolutionOrder;
	}

	private static List<PythonType> CalculateMro(PythonType type, IList<PythonType> ldt)
	{
		return Mro.Calculate(type, ldt);
	}

	private static bool TryReplaceExtensibleWithBase(Type curType, out Type newType)
	{
		if (curType.IsGenericType && curType.GetGenericTypeDefinition() == typeof(Extensible<>))
		{
			newType = curType.GetGenericArguments()[0];
			return true;
		}
		newType = null;
		return false;
	}

	public object __call__(CodeContext context, params object[] args)
	{
		return PythonTypeOps.CallParams(context, this, args);
	}

	public object __call__(CodeContext context, [ParamDictionary] IDictionary<string, object> kwArgs, params object[] args)
	{
		return PythonTypeOps.CallWorker(context, this, kwArgs, args);
	}

	public int __cmp__([NotNull] PythonType other)
	{
		if (other != this)
		{
			int num = Name.CompareTo(other.Name);
			if (num == 0)
			{
				long id = IdDispenser.GetId(this);
				long id2 = IdDispenser.GetId(other);
				if (id > id2)
				{
					return 1;
				}
				return -1;
			}
			return num;
		}
		return 0;
	}

	[Python3Warning("type inequality comparisons not supported in 3.x")]
	public static bool operator >(PythonType self, PythonType other)
	{
		return self.__cmp__(other) > 0;
	}

	[Python3Warning("type inequality comparisons not supported in 3.x")]
	public static bool operator <(PythonType self, PythonType other)
	{
		return self.__cmp__(other) < 0;
	}

	[Python3Warning("type inequality comparisons not supported in 3.x")]
	public static bool operator >=(PythonType self, PythonType other)
	{
		return self.__cmp__(other) >= 0;
	}

	[Python3Warning("type inequality comparisons not supported in 3.x")]
	public static bool operator <=(PythonType self, PythonType other)
	{
		return self.__cmp__(other) <= 0;
	}

	public void __delattr__(CodeContext context, string name)
	{
		DeleteCustomMember(context, name);
	}

	[SpecialName]
	[PropertyMethod]
	[WrapperDescriptor]
	public static object Get__doc__(CodeContext context, PythonType self)
	{
		if (self.TryLookupSlot(context, "__doc__", out var slot) && slot.TryGetValue(context, null, self, out var value))
		{
			return value;
		}
		if (self.IsSystemType)
		{
			return PythonTypeOps.GetDocumentation(self.UnderlyingSystemType);
		}
		return null;
	}

	public object __getattribute__(CodeContext context, string name)
	{
		if (TryGetBoundCustomMember(context, name, out var value))
		{
			return value;
		}
		throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'", Name, name);
	}

	[SpecialName]
	[WrapperDescriptor]
	[PropertyMethod]
	public static object Get__module__(CodeContext context, PythonType self)
	{
		if (self._dict != null && self._dict.TryGetValue("__module__", out var value) && value.TryGetValue(context, self, DynamicHelpers.GetPythonType(self), out var value2))
		{
			return value2;
		}
		return PythonTypeOps.GetModuleName(context, self.UnderlyingSystemType);
	}

	[SpecialName]
	[PropertyMethod]
	[PythonHidden]
	[WrapperDescriptor]
	public static string Get__clr_assembly__(PythonType self)
	{
		return self.UnderlyingSystemType.Namespace + " in " + self.UnderlyingSystemType.Assembly.FullName;
	}

	[SpecialName]
	[PropertyMethod]
	[WrapperDescriptor]
	public static void Set__module__(CodeContext context, PythonType self, object value)
	{
		if (self.IsSystemType)
		{
			throw PythonOps.TypeError("can't set {0}.__module__", self.Name);
		}
		self._dict["__module__"] = new PythonTypeUserDescriptorSlot(value);
		self.UpdateVersion();
	}

	[SpecialName]
	[PropertyMethod]
	[WrapperDescriptor]
	public static void Delete__module__(CodeContext context, PythonType self)
	{
		throw PythonOps.TypeError("can't delete {0}.__module__", self.Name);
	}

	[SpecialName]
	[WrapperDescriptor]
	[PropertyMethod]
	public static PythonTuple Get__mro__(PythonType type)
	{
		return PythonTypeOps.MroToPython(type.ResolutionOrder);
	}

	[SpecialName]
	[PropertyMethod]
	[WrapperDescriptor]
	public static string Get__name__(PythonType type)
	{
		return type.Name;
	}

	[SpecialName]
	[PropertyMethod]
	[WrapperDescriptor]
	public static void Set__name__(PythonType type, string name)
	{
		if (type.IsSystemType)
		{
			throw PythonOps.TypeError("can't set attributes of built-in/extension type '{0}'", type.Name);
		}
		type.Name = name;
	}

	public string __repr__(CodeContext context)
	{
		string name = Name;
		if (IsSystemType)
		{
			if (PythonTypeOps.IsRuntimeAssembly(UnderlyingSystemType.Assembly) || IsPythonType)
			{
				object obj = Get__module__(context, this);
				if (!obj.Equals("__builtin__"))
				{
					return $"<type '{obj}.{Name}'>";
				}
			}
			return $"<type '{Name}'>";
		}
		string arg = "unknown";
		if (TryLookupSlot(context, "__module__", out var slot) && slot.TryGetValue(context, this, this, out var value))
		{
			arg = value as string;
		}
		return $"<class '{arg}.{name}'>";
	}

	internal string GetTypeDebuggerDisplay()
	{
		string arg = "unknown";
		if (TryLookupSlot(Context.SharedContext, "__module__", out var slot) && slot.TryGetValue(Context.SharedContext, this, this, out var value))
		{
			arg = value as string;
		}
		return $"{arg}.{Name} instance";
	}

	public void __setattr__(CodeContext context, string name, object value)
	{
		SetCustomMember(context, name, value);
	}

	public List __subclasses__(CodeContext context)
	{
		List list = new List();
		IList<WeakReference> subTypes = SubTypes;
		if (subTypes != null)
		{
			PythonContext context2 = PythonContext.GetContext(context);
			foreach (WeakReference item in subTypes)
			{
				if (item.IsAlive)
				{
					PythonType pythonType = (PythonType)item.Target;
					if (pythonType.PythonContext == null || pythonType.PythonContext == context2)
					{
						list.AddNoLock(item.Target);
					}
				}
			}
		}
		return list;
	}

	public virtual List mro()
	{
		return new List(Get__mro__(this));
	}

	public virtual bool __instancecheck__(object instance)
	{
		return SubclassImpl(DynamicHelpers.GetPythonType(instance));
	}

	public virtual bool __subclasscheck__(PythonType sub)
	{
		return SubclassImpl(sub);
	}

	private bool SubclassImpl(PythonType sub)
	{
		if (UnderlyingSystemType.IsInterface && UnderlyingSystemType.IsAssignableFrom(sub.UnderlyingSystemType))
		{
			return true;
		}
		return sub.IsSubclassOf(this);
	}

	public virtual bool __subclasscheck__(OldClass sub)
	{
		return IsSubclassOf(sub.TypeObject);
	}

	public static implicit operator Type(PythonType self)
	{
		return self.UnderlyingSystemType;
	}

	public static implicit operator TypeTracker(PythonType self)
	{
		return ReflectionCache.GetTypeTracker(self.UnderlyingSystemType);
	}

	internal bool IsMixedNewStyleOldStyle()
	{
		if (!IsOldClass)
		{
			foreach (PythonType item in ResolutionOrder)
			{
				if (item.IsOldClass)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static PythonType GetPythonType(Type type)
	{
		if (!_pythonTypes.TryGetValue(type, out var value))
		{
			lock (_pythonTypes)
			{
				if (!_pythonTypes.TryGetValue(type, out value))
				{
					value = new PythonType(type);
					_pythonTypes.Add(type, value);
				}
			}
		}
		return (PythonType)value;
	}

	internal static PythonType SetPythonType(Type type, PythonType pyType)
	{
		lock (_pythonTypes)
		{
			_pythonTypes.Add(type, pyType);
			return pyType;
		}
	}

	internal object CreateInstance(CodeContext context)
	{
		EnsureInstanceCtor();
		return _instanceCtor.CreateInstance(context);
	}

	internal object CreateInstance(CodeContext context, object arg0)
	{
		EnsureInstanceCtor();
		return _instanceCtor.CreateInstance(context, arg0);
	}

	internal object CreateInstance(CodeContext context, object arg0, object arg1)
	{
		EnsureInstanceCtor();
		return _instanceCtor.CreateInstance(context, arg0, arg1);
	}

	internal object CreateInstance(CodeContext context, object arg0, object arg1, object arg2)
	{
		EnsureInstanceCtor();
		return _instanceCtor.CreateInstance(context, arg0, arg1, arg2);
	}

	internal object CreateInstance(CodeContext context, params object[] args)
	{
		EnsureInstanceCtor();
		return args.Length switch
		{
			0 => _instanceCtor.CreateInstance(context), 
			1 => _instanceCtor.CreateInstance(context, args[0]), 
			2 => _instanceCtor.CreateInstance(context, args[0], args[1]), 
			3 => _instanceCtor.CreateInstance(context, args[0], args[1], args[2]), 
			_ => _instanceCtor.CreateInstance(context, args), 
		};
	}

	internal object CreateInstance(CodeContext context, object[] args, string[] names)
	{
		EnsureInstanceCtor();
		return _instanceCtor.CreateInstance(context, args, names);
	}

	internal int Hash(object o)
	{
		EnsureHashSite();
		return _hashSite.Target(_hashSite, o);
	}

	internal bool TryGetLength(CodeContext context, object o, out int length)
	{
		CallSite<Func<CallSite, CodeContext, object, object>> callSite = ((!IsSystemType) ? _siteCache.GetLenSite(context) : PythonContext.GetContext(context).GetSiteCacheForSystemType(UnderlyingSystemType).GetLenSite(context));
		PythonTypeSlot slot = _lenSlot;
		if (slot == null && !PythonOps.TryResolveTypeSlot(context, this, "__len__", out slot))
		{
			length = 0;
			return false;
		}
		if (!slot.TryGetValue(context, o, this, out var value))
		{
			length = 0;
			return false;
		}
		object obj = callSite.Target(callSite, context, value);
		if (!(obj is int))
		{
			throw PythonOps.ValueError("__len__ must return int");
		}
		length = (int)obj;
		return true;
	}

	internal bool EqualRetBool(object self, object other)
	{
		if (_eqSite == null)
		{
			Interlocked.CompareExchange(ref _eqSite, Context.CreateComparisonSite(PythonOperationKind.Equal), null);
		}
		return _eqSite.Target(_eqSite, self, other);
	}

	internal int Compare(object self, object other)
	{
		if (_compareSite == null)
		{
			Interlocked.CompareExchange(ref _compareSite, Context.MakeSortCompareSite(), null);
		}
		return _compareSite.Target(_compareSite, self, other);
	}

	internal bool TryGetBoundAttr(CodeContext context, object o, string name, out object ret)
	{
		CallSite<Func<CallSite, object, CodeContext, object>> callSite = ((!IsSystemType) ? _siteCache.GetTryGetMemberSite(context, name) : PythonContext.GetContext(context).GetSiteCacheForSystemType(UnderlyingSystemType).GetTryGetMemberSite(context, name));
		try
		{
			ret = callSite.Target(callSite, o, context);
			return ret != OperationFailed.Value;
		}
		catch (MissingMemberException)
		{
			ret = null;
			return false;
		}
	}

	private void EnsureHashSite()
	{
		if (_hashSite == null)
		{
			Interlocked.CompareExchange(ref _hashSite, CallSite<Func<CallSite, object, int>>.Create(Context.Operation(PythonOperationKind.Hash)), null);
		}
	}

	internal bool IsSubclassOf(PythonType other)
	{
		if (other == this)
		{
			return true;
		}
		if (other.UnderlyingSystemType == typeof(ValueType) && UnderlyingSystemType.IsValueType())
		{
			return true;
		}
		return IsSubclassWorker(other);
	}

	private bool IsSubclassWorker(PythonType other)
	{
		for (int i = 0; i < _bases.Length; i++)
		{
			PythonType pythonType = _bases[i];
			if (pythonType == other || pythonType.IsSubclassWorker(other))
			{
				return true;
			}
		}
		return false;
	}

	internal void SetConstructor(BuiltinFunction ctor)
	{
		_ctor = ctor;
	}

	internal bool IsHiddenMember(string name)
	{
		if (!TryResolveSlot(DefaultContext.Default, name, out var slot))
		{
			return TryResolveSlot(DefaultContext.DefaultCLS, name, out slot);
		}
		return false;
	}

	internal LateBoundInitBinder GetLateBoundInitBinder(CallSignature signature)
	{
		if (_lateBoundInitBinders == null)
		{
			Interlocked.CompareExchange(ref _lateBoundInitBinders, new Dictionary<CallSignature, LateBoundInitBinder>(), null);
		}
		lock (_lateBoundInitBinders)
		{
			if (!_lateBoundInitBinders.TryGetValue(signature, out var value))
			{
				value = (_lateBoundInitBinders[signature] = new LateBoundInitBinder(this, signature));
			}
			return value;
		}
	}

	internal bool TryLookupSlot(CodeContext context, string name, out PythonTypeSlot slot)
	{
		if (IsSystemType)
		{
			return PythonBinder.GetBinder(context).TryLookupSlot(context, this, name, out slot);
		}
		return _dict.TryGetValue(name, out slot);
	}

	internal bool TryResolveSlot(CodeContext context, string name, out PythonTypeSlot slot)
	{
		for (int i = 0; i < _resolutionOrder.Count; i++)
		{
			PythonType pythonType = _resolutionOrder[i];
			if (pythonType.IsSystemType && !pythonType.UnderlyingSystemType.IsInterface())
			{
				return PythonBinder.GetBinder(context).TryResolveSlot(context, pythonType, this, name, out slot);
			}
			if (pythonType.TryLookupSlot(context, name, out slot))
			{
				return true;
			}
		}
		if (UnderlyingSystemType.IsInterface())
		{
			return TypeCache.Object.TryResolveSlot(context, name, out slot);
		}
		slot = null;
		return false;
	}

	internal bool TryResolveMixedSlot(CodeContext context, string name, out PythonTypeSlot slot)
	{
		for (int i = 0; i < _resolutionOrder.Count; i++)
		{
			PythonType pythonType = _resolutionOrder[i];
			if (pythonType.TryLookupSlot(context, name, out slot))
			{
				return true;
			}
			if (pythonType.OldClass != null && pythonType.OldClass.TryLookupSlot(name, out var ret))
			{
				slot = ToTypeSlot(ret);
				return true;
			}
		}
		slot = null;
		return false;
	}

	internal void AddSlot(string name, PythonTypeSlot slot)
	{
		_dict[name] = slot;
		if (name == "__new__")
		{
			_objectNew = null;
			ClearObjectNewInSubclasses(this);
		}
		else if (name == "__init__")
		{
			_objectInit = null;
			ClearObjectInitInSubclasses(this);
		}
	}

	private void ClearObjectNewInSubclasses(PythonType pt)
	{
		lock (_subtypesLock)
		{
			if (pt._subtypes == null)
			{
				return;
			}
			foreach (WeakReference subtype in pt._subtypes)
			{
				if (subtype.Target is PythonType pythonType)
				{
					pythonType._objectNew = null;
					ClearObjectNewInSubclasses(pythonType);
				}
			}
		}
	}

	private void ClearObjectInitInSubclasses(PythonType pt)
	{
		lock (_subtypesLock)
		{
			if (pt._subtypes == null)
			{
				return;
			}
			foreach (WeakReference subtype in pt._subtypes)
			{
				if (subtype.Target is PythonType pythonType)
				{
					pythonType._objectInit = null;
					ClearObjectInitInSubclasses(pythonType);
				}
			}
		}
	}

	internal bool TryGetCustomSetAttr(CodeContext context, out PythonTypeSlot pts)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (context2.Binder.TryResolveSlot(context, DynamicHelpers.GetPythonType(this), this, "__setattr__", out pts) && pts is BuiltinMethodDescriptor)
		{
			return ((BuiltinMethodDescriptor)pts).DeclaringType != typeof(PythonType);
		}
		return false;
	}

	internal void SetCustomMember(CodeContext context, string name, object value)
	{
		if ((TryResolveSlot(context, name, out var slot) && slot.TrySetValue(context, null, this, value)) || (_pythonTypeType.TryResolveSlot(context, name, out slot) && slot.TrySetValue(context, this, _pythonTypeType, value)))
		{
			return;
		}
		if (IsSystemType)
		{
			throw new MissingMemberException($"'{Name}' object has no attribute '{name}'");
		}
		if (!(value is PythonTypeSlot) && _dict.TryGetValue(name, out var value2) && value2 is PythonTypeUserDescriptorSlot)
		{
			if (SetAbstractMethodFlags(name, value))
			{
				UpdateVersion();
			}
			((PythonTypeUserDescriptorSlot)value2).Value = value;
		}
		else
		{
			SetAbstractMethodFlags(name, value);
			AddSlot(name, ToTypeSlot(value));
			UpdateVersion();
		}
	}

	internal static PythonTypeSlot ToTypeSlot(object value)
	{
		if (value is PythonTypeSlot result)
		{
			return result;
		}
		if (value != null)
		{
			return new PythonTypeUserDescriptorSlot(value);
		}
		return new PythonTypeUserDescriptorSlot(value, isntDescriptor: true);
	}

	internal bool DeleteCustomMember(CodeContext context, string name)
	{
		if (TryResolveSlot(context, name, out var slot) && slot.TryDeleteValue(context, null, this))
		{
			return true;
		}
		if (IsSystemType)
		{
			throw new MissingMemberException($"can't delete attributes of built-in/extension type '{Name}'");
		}
		if (!_dict.Remove(name))
		{
			throw new MissingMemberException(string.Format(CultureInfo.CurrentCulture, Resources.MemberDoesNotExist, new object[1] { name.ToString() }));
		}
		ClearAbstractMethodFlags(name);
		UpdateVersion();
		return true;
	}

	internal bool TryGetBoundCustomMember(CodeContext context, string name, out object value)
	{
		if (TryResolveSlot(context, name, out var slot) && slot.TryGetValue(context, null, this, out value))
		{
			return true;
		}
		PythonType pythonType = DynamicHelpers.GetPythonType(this);
		if (pythonType.TryResolveSlot(context, name, out slot) && slot.TryGetValue(context, this, pythonType, out value))
		{
			return true;
		}
		value = null;
		return false;
	}

	T IFastGettable.MakeGetBinding<T>(CallSite<T> site, PythonGetMemberBinder binder, CodeContext context, string name)
	{
		return (T)(object)new MetaPythonType.FastGetBinderHelper(this, context, binder).GetBinding();
	}

	internal object GetMember(CodeContext context, object instance, string name)
	{
		if (TryGetMember(context, instance, name, out var value))
		{
			return value;
		}
		throw new MissingMemberException(string.Format(CultureInfo.CurrentCulture, Resources.CantFindMember, new object[1] { name }));
	}

	internal void SetMember(CodeContext context, object instance, string name, object value)
	{
		if (TrySetMember(context, instance, name, value))
		{
			return;
		}
		throw new MissingMemberException(string.Format(CultureInfo.CurrentCulture, Resources.Slot_CantSet, new object[1] { name }));
	}

	internal void DeleteMember(CodeContext context, object instance, string name)
	{
		if (TryDeleteMember(context, instance, name))
		{
			return;
		}
		throw new MissingMemberException(string.Format(CultureInfo.CurrentCulture, "couldn't delete member {0}", new object[1] { name }));
	}

	internal bool TryGetMember(CodeContext context, object instance, string name, out object value)
	{
		if (TryGetNonCustomMember(context, instance, name, out value))
		{
			return true;
		}
		try
		{
			if (PythonTypeOps.TryInvokeBinaryOperator(context, instance, name, "__getattr__", out value))
			{
				return true;
			}
		}
		catch (MissingMemberException)
		{
		}
		return false;
	}

	internal bool TryGetNonCustomMember(CodeContext context, object instance, string name, out object value)
	{
		bool flag = false;
		value = null;
		if (instance is PythonType pythonType)
		{
			if (pythonType.TryLookupSlot(context, name, out var slot))
			{
				flag = slot.TryGetValue(context, null, this, out value);
			}
		}
		else if (instance is IPythonObject { Dict: var dict })
		{
			flag = dict?.TryGetValue(name, out value) ?? false;
		}
		for (int i = 0; i < _resolutionOrder.Count; i++)
		{
			PythonType pythonType2 = _resolutionOrder[i];
			if (pythonType2.TryLookupSlot(context, name, out var slot2) && (!flag || slot2.IsSetDescriptor(context, this)))
			{
				if (slot2.TryGetValue(context, instance, this, out var value2))
				{
					value = value2;
				}
				return true;
			}
		}
		return flag;
	}

	internal bool TryGetBoundMember(CodeContext context, object instance, string name, out object value)
	{
		if (TryResolveNonObjectSlot(context, instance, "__getattribute__", out var value2))
		{
			value = InvokeGetAttributeMethod(context, name, value2);
			return true;
		}
		return TryGetNonCustomBoundMember(context, instance, name, out value);
	}

	private object InvokeGetAttributeMethod(CodeContext context, string name, object getattr)
	{
		CallSite<Func<CallSite, CodeContext, object, string, object>> callSite = ((!IsSystemType) ? _siteCache.GetGetAttributeSite(context) : PythonContext.GetContext(context).GetSiteCacheForSystemType(UnderlyingSystemType).GetGetAttributeSite(context));
		return callSite.Target(callSite, context, getattr, name);
	}

	internal bool TryGetNonCustomBoundMember(CodeContext context, object instance, string name, out object value)
	{
		if (instance is IPythonObject { Dict: { } dict } && dict.TryGetValue(name, out value))
		{
			return true;
		}
		if (TryResolveSlot(context, instance, name, out value))
		{
			return true;
		}
		try
		{
			if (TryResolveNonObjectSlot(context, instance, "__getattr__", out var value2))
			{
				value = InvokeGetAttributeMethod(context, name, value2);
				return true;
			}
		}
		catch (MissingMemberException)
		{
		}
		value = null;
		return false;
	}

	private bool TryResolveSlot(CodeContext context, object instance, string name, out object value)
	{
		for (int i = 0; i < _resolutionOrder.Count; i++)
		{
			PythonType pythonType = _resolutionOrder[i];
			if (pythonType.TryLookupSlot(context, name, out var slot) && slot.TryGetValue(context, instance, this, out value))
			{
				return true;
			}
		}
		value = null;
		return false;
	}

	private bool TryResolveNonObjectSlot(CodeContext context, object instance, string name, out object value)
	{
		for (int i = 0; i < _resolutionOrder.Count; i++)
		{
			PythonType pythonType = _resolutionOrder[i];
			if (pythonType == TypeCache.Object)
			{
				break;
			}
			if (pythonType.TryLookupSlot(context, name, out var slot) && slot.TryGetValue(context, instance, this, out value))
			{
				return true;
			}
		}
		value = null;
		return false;
	}

	internal bool TrySetMember(CodeContext context, object instance, string name, object value)
	{
		if (TryResolveNonObjectSlot(context, instance, "__setattr__", out var value2))
		{
			CallSite<Func<CallSite, CodeContext, object, object, string, object, object>> callSite = ((!IsSystemType) ? _siteCache.GetSetAttrSite(context) : PythonContext.GetContext(context).GetSiteCacheForSystemType(UnderlyingSystemType).GetSetAttrSite(context));
			callSite.Target(callSite, context, value2, instance, name, value);
			return true;
		}
		return TrySetNonCustomMember(context, instance, name, value);
	}

	internal bool TrySetNonCustomMember(CodeContext context, object instance, string name, object value)
	{
		if (TryResolveSlot(context, name, out var slot) && slot.TrySetValue(context, instance, this, value))
		{
			return true;
		}
		if (instance is IPythonObject { Dict: var pythonDictionary } pythonObject)
		{
			if (pythonDictionary == null && pythonObject.PythonType.HasDictionary)
			{
				PythonDictionary dict = MakeDictionary();
				PythonDictionary pythonDictionary2 = pythonObject.SetDict(dict);
				pythonDictionary = pythonDictionary2;
				if (pythonDictionary2 == null)
				{
					return false;
				}
			}
			pythonDictionary[name] = value;
			return true;
		}
		return false;
	}

	internal bool TryDeleteMember(CodeContext context, object instance, string name)
	{
		try
		{
			if (TryResolveNonObjectSlot(context, instance, "__delattr__", out var value))
			{
				InvokeGetAttributeMethod(context, name, value);
				return true;
			}
		}
		catch (MissingMemberException)
		{
		}
		return TryDeleteNonCustomMember(context, instance, name);
	}

	internal bool TryDeleteNonCustomMember(CodeContext context, object instance, string name)
	{
		if (TryResolveSlot(context, name, out var slot) && slot.TryDeleteValue(context, instance, this))
		{
			return true;
		}
		if (instance is IPythonObject { Dict: var pythonDictionary } pythonObject)
		{
			if (pythonDictionary == null && pythonObject.PythonType.HasDictionary)
			{
				PythonDictionary dict = MakeDictionary();
				PythonDictionary pythonDictionary2 = pythonObject.SetDict(dict);
				pythonDictionary = pythonDictionary2;
				if (pythonDictionary2 == null)
				{
					return false;
				}
			}
			return pythonDictionary.Remove(name);
		}
		return false;
	}

	internal List GetMemberNames(CodeContext context)
	{
		return GetMemberNames(context, null);
	}

	internal List GetMemberNames(CodeContext context, object self)
	{
		List list = TryGetCustomDir(context, self);
		if (list != null)
		{
			return list;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		list = new List();
		for (int i = 0; i < _resolutionOrder.Count; i++)
		{
			PythonType pythonType = _resolutionOrder[i];
			if (pythonType.IsSystemType)
			{
				PythonBinder.GetBinder(context).ResolveMemberNames(context, pythonType, this, dictionary);
			}
			else
			{
				AddUserTypeMembers(context, dictionary, pythonType, list);
			}
		}
		ExtensionMethodSet extensionMethods = context.ModuleContext.ExtensionMethods;
		if (extensionMethods != ExtensionMethodSet.Empty)
		{
			foreach (MethodInfo extensionMethod in extensionMethods.GetExtensionMethods(this))
			{
				dictionary[extensionMethod.Name] = extensionMethod.Name;
			}
		}
		return AddInstanceMembers(self, dictionary, list);
	}

	private List TryGetCustomDir(CodeContext context, object self)
	{
		if (self != null && TryResolveNonObjectSlot(context, self, "__dir__", out var value))
		{
			CallSite<Func<CallSite, CodeContext, object, object>> callSite = ((!IsSystemType) ? _siteCache.GetDirSite(context) : PythonContext.GetContext(context).GetSiteCacheForSystemType(UnderlyingSystemType).GetDirSite(context));
			return new List(callSite.Target(callSite, context, value));
		}
		return null;
	}

	private static void AddUserTypeMembers(CodeContext context, Dictionary<string, string> keys, PythonType dt, List res)
	{
		if (dt.OldClass != null)
		{
			foreach (KeyValuePair<object, object> item in dt.OldClass._dict)
			{
				AddOneMember(keys, res, item.Key);
			}
			return;
		}
		foreach (KeyValuePair<string, PythonTypeSlot> item2 in dt._dict)
		{
			if (!keys.ContainsKey(item2.Key))
			{
				keys[item2.Key] = item2.Key;
			}
		}
	}

	private static void AddOneMember(Dictionary<string, string> keys, List res, object name)
	{
		if (name is string text)
		{
			keys[text] = text;
		}
		else
		{
			res.Add(name);
		}
	}

	private static List AddInstanceMembers(object self, Dictionary<string, string> keys, List res)
	{
		if (self is IPythonObject { Dict: { } dict })
		{
			lock (dict)
			{
				foreach (object key in dict.Keys)
				{
					AddOneMember(keys, res, key);
				}
			}
		}
		List<string> list = new List<string>(keys.Keys);
		list.Sort();
		res.extend(list);
		return res;
	}

	internal PythonDictionary GetMemberDictionary(CodeContext context)
	{
		return GetMemberDictionary(context, excludeDict: true);
	}

	internal PythonDictionary GetMemberDictionary(CodeContext context, bool excludeDict)
	{
		PythonDictionary pythonDictionary = PythonDictionary.MakeSymbolDictionary();
		if (IsSystemType)
		{
			PythonBinder.GetBinder(context).LookupMembers(context, this, pythonDictionary);
		}
		else
		{
			foreach (string key in _dict.Keys)
			{
				if ((!excludeDict || !(key.ToString() == "__dict__")) && TryLookupSlot(context, key, out var slot) && slot.TryGetValue(context, null, this, out var value))
				{
					if (slot is PythonTypeUserDescriptorSlot)
					{
						pythonDictionary[key] = value;
					}
					else
					{
						pythonDictionary[key] = slot;
					}
				}
			}
		}
		return pythonDictionary;
	}

	private void InitializeUserType(CodeContext context, string name, PythonTuple bases, PythonDictionary vars, string selfNames)
	{
		if (vars.ContainsKey("__mro__"))
		{
			throw new NotImplementedException("Overriding __mro__ of built-in types is not implemented");
		}
		if (vars.ContainsKey("mro"))
		{
			foreach (object basis in bases)
			{
				if (basis is PythonType pythonType && pythonType.IsSubclassOf(TypeCache.PythonType))
				{
					throw new NotImplementedException("Overriding type.mro is not implemented");
				}
			}
		}
		bases = ValidateBases(bases);
		_name = name;
		_bases = GetBasesAsList(bases).ToArray();
		_pythonContext = PythonContext.GetContext(context);
		_resolutionOrder = CalculateMro(this, _bases);
		bool flag = false;
		PythonType[] bases2 = _bases;
		foreach (PythonType pythonType2 in bases2)
		{
			if (pythonType2.GetUsedSlotCount() != 0)
			{
				if (flag)
				{
					throw PythonOps.TypeError("multiple bases have instance lay-out conflict");
				}
				flag = true;
			}
			pythonType2.AddSubType(this);
		}
		HashSet<string> hashSet = null;
		foreach (PythonType item in _resolutionOrder)
		{
			_originalSlotCount += item.GetUsedSlotCount();
			if (item._optimizedInstanceNames != null)
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<string>();
				}
				hashSet.UnionWith(item._optimizedInstanceNames);
			}
		}
		if (!string.IsNullOrEmpty(selfNames))
		{
			if (hashSet == null)
			{
				hashSet = new HashSet<string>();
			}
			hashSet.UnionWith(selfNames.Split(','));
		}
		if (hashSet != null)
		{
			_optimizedInstanceVersion = CustomInstanceDictionaryStorage.AllocateVersion();
			_optimizedInstanceNames = new List<string>(hashSet).ToArray();
		}
		EnsureDict();
		PopulateDictionary(context, name, bases, vars);
		_underlyingSystemType = NewTypeMaker.GetNewType(name, bases);
		_underlyingSystemType = __clrtype__();
		if (_underlyingSystemType == null)
		{
			throw PythonOps.ValueError("__clrtype__ must return a type, not None");
		}
		lock (_userTypeCtors)
		{
			if (!_userTypeCtors.TryGetValue(_underlyingSystemType, out _ctor))
			{
				ConstructorInfo[] constructors = _underlyingSystemType.GetConstructors();
				bool flag2 = false;
				ConstructorInfo[] array = constructors;
				foreach (ConstructorInfo constructorInfo in array)
				{
					ParameterInfo[] parameters = constructorInfo.GetParameters();
					if ((parameters.Length > 1 && parameters[0].ParameterType == typeof(CodeContext) && parameters[1].ParameterType == typeof(PythonType)) || (parameters.Length > 0 && parameters[0].ParameterType == typeof(PythonType)))
					{
						flag2 = true;
						break;
					}
				}
				_ctor = BuiltinFunction.MakeFunction(Name, constructors, _underlyingSystemType);
				if (flag2)
				{
					_userTypeCtors[_underlyingSystemType] = _ctor;
				}
				else
				{
					_instanceCtor = new SystemInstanceCreator(this);
					_attrs |= PythonTypeAttributes.SystemCtor;
				}
			}
		}
		UpdateObjectNewAndInit(context);
	}

	internal PythonDictionary MakeDictionary()
	{
		if (_optimizedInstanceNames != null)
		{
			return new PythonDictionary(new CustomInstanceDictionaryStorage(_optimizedInstanceNames, _optimizedInstanceVersion));
		}
		return PythonDictionary.MakeSymbolDictionary();
	}

	internal IList<string> GetOptimizedInstanceNames()
	{
		return _optimizedInstanceNames;
	}

	internal int GetOptimizedInstanceVersion()
	{
		return _optimizedInstanceVersion;
	}

	internal IList<string> GetTypeSlots()
	{
		if (_dict != null && _dict.TryGetValue("__slots__", out var value) && value is PythonTypeUserDescriptorSlot)
		{
			return SlotsToList(((PythonTypeUserDescriptorSlot)value).Value);
		}
		return ArrayUtils.EmptyStrings;
	}

	internal static List<string> GetSlots(PythonDictionary dict)
	{
		List<string> result = null;
		if (dict != null && dict.TryGetValue("__slots__", out var value))
		{
			result = SlotsToList(value);
		}
		return result;
	}

	internal static List<string> SlotsToList(object slots)
	{
		List<string> list = new List<string>();
		if (slots is IList<object> list2)
		{
			list = new List<string>(list2.Count);
			for (int i = 0; i < list2.Count; i++)
			{
				list.Add(GetSlotName(list2[i]));
			}
			list.Sort();
		}
		else
		{
			list = new List<string>(1);
			list.Add(GetSlotName(slots));
		}
		return list;
	}

	internal bool HasObjectNew(CodeContext context)
	{
		if (!_objectNew.HasValue)
		{
			UpdateObjectNewAndInit(context);
		}
		return _objectNew.Value;
	}

	internal bool HasObjectInit(CodeContext context)
	{
		if (!_objectInit.HasValue)
		{
			UpdateObjectNewAndInit(context);
		}
		return _objectInit.Value;
	}

	private void UpdateObjectNewAndInit(CodeContext context)
	{
		PythonType[] bases = _bases;
		foreach (PythonType pythonType in bases)
		{
			if (pythonType != TypeCache.Object)
			{
				if (!pythonType._objectNew.HasValue || !pythonType._objectInit.HasValue)
				{
					pythonType.UpdateObjectNewAndInit(context);
				}
				if (!pythonType._objectNew.Value)
				{
					_objectNew = false;
				}
				if (!pythonType._objectInit.Value)
				{
					_objectInit = false;
				}
			}
		}
		PythonTypeSlot slot;
		object value;
		if (!_objectInit.HasValue)
		{
			_objectInit = TryResolveSlot(context, "__init__", out slot) && slot.TryGetValue(context, null, this, out value) && value == InstanceOps.Init;
		}
		if (!_objectNew.HasValue)
		{
			_objectNew = TryResolveSlot(context, "__new__", out slot) && slot.TryGetValue(context, null, this, out value) && value == InstanceOps.New;
		}
	}

	private static string GetSlotName(object o)
	{
		if (!Converter.TryConvertToString(o, out var result) || string.IsNullOrEmpty(result))
		{
			throw PythonOps.TypeError("slots must be one string or a list of strings");
		}
		for (int i = 0; i < result.Length; i++)
		{
			if ((result[i] < 'a' || result[i] > 'z') && (result[i] < 'A' || result[i] > 'Z') && (i == 0 || result[i] < '0' || result[i] > '9') && result[i] != '_')
			{
				throw PythonOps.TypeError("__slots__ must be valid identifiers");
			}
		}
		return result;
	}

	private int GetUsedSlotCount()
	{
		int num = 0;
		if (_slots != null)
		{
			num = _slots.Length;
			if (Array.IndexOf(_slots, "__weakref__") != -1)
			{
				num--;
			}
			if (Array.IndexOf(_slots, "__dict__") != -1)
			{
				num--;
			}
		}
		return num;
	}

	private void PopulateDictionary(CodeContext context, string name, PythonTuple bases, PythonDictionary vars)
	{
		PopulateSlot("__doc__", null);
		List<string> slots = GetSlots(vars);
		if (slots != null)
		{
			_slots = slots.ToArray();
			int originalSlotCount = _originalSlotCount;
			string privatePrefix = Parser.GetPrivatePrefix(name);
			for (int i = 0; i < slots.Count; i++)
			{
				string text = slots[i];
				if (text.StartsWith("__") && !text.EndsWith("__"))
				{
					text = "_" + privatePrefix + text;
				}
				AddSlot(text, new ReflectedSlotProperty(text, name, i + originalSlotCount));
			}
			_originalSlotCount += slots.Count;
		}
		if (CheckForSlotWithDefault(context, bases, slots, "__weakref__"))
		{
			_attrs |= PythonTypeAttributes.WeakReferencable;
			AddSlot("__weakref__", new PythonTypeWeakRefSlot(this));
		}
		if (CheckForSlotWithDefault(context, bases, slots, "__dict__"))
		{
			_attrs |= PythonTypeAttributes.HasDictionary;
			bool flag = false;
			for (int j = 1; j < _resolutionOrder.Count; j++)
			{
				PythonType pythonType = _resolutionOrder[j];
				if (pythonType.TryResolveSlot(context, "__dict__", out var _))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				AddSlot("__dict__", new PythonTypeDictSlot(this));
			}
		}
		if (context.TryGetVariable("__name__", out var value))
		{
			PopulateSlot("__module__", value);
		}
		foreach (KeyValuePair<object, object> var in vars)
		{
			if (var.Key is string)
			{
				PopulateSlot((string)var.Key, var.Value);
			}
		}
		if (_dict.TryGetValue("__new__", out var value2) && value2 is PythonFunction)
		{
			AddSlot("__new__", new staticmethod(value2));
		}
	}

	private static bool CheckForSlotWithDefault(CodeContext context, PythonTuple bases, List<string> slots, string name)
	{
		bool result = true;
		PythonTypeSlot slot2;
		if (slots != null && !slots.Contains(name))
		{
			result = false;
			foreach (object basis in bases)
			{
				if (basis is PythonType pythonType && pythonType.TryLookupSlot(context, name, out var _))
				{
					result = true;
				}
			}
		}
		else if (slots != null && bases.Count > 0 && bases[0] is PythonType pythonType2 && pythonType2.TryLookupSlot(context, name, out slot2))
		{
			throw PythonOps.TypeError(name + " slot disallowed: we already got one");
		}
		return result;
	}

	[PythonHidden]
	public virtual Type __clrtype__()
	{
		return _underlyingSystemType;
	}

	private void PopulateSlot(string key, object value)
	{
		AddSlot(key, ToTypeSlot(value));
	}

	private static List<PythonType> GetBasesAsList(PythonTuple bases)
	{
		List<PythonType> list = new List<PythonType>();
		foreach (object basis in bases)
		{
			PythonType pythonType = basis as PythonType;
			if (pythonType == null)
			{
				pythonType = ((OldClass)basis).TypeObject;
			}
			list.Add(pythonType);
		}
		return list;
	}

	private static PythonTuple ValidateBases(PythonTuple bases)
	{
		PythonTuple pythonTuple = PythonTypeOps.EnsureBaseType(bases);
		for (int i = 0; i < pythonTuple.__len__(); i++)
		{
			for (int j = 0; j < pythonTuple.__len__(); j++)
			{
				if (i != j && pythonTuple[i] == pythonTuple[j])
				{
					if (pythonTuple[i] is OldClass oldClass)
					{
						throw PythonOps.TypeError("duplicate base class {0}", oldClass.Name);
					}
					throw PythonOps.TypeError("duplicate base class {0}", ((PythonType)pythonTuple[i]).Name);
				}
			}
		}
		return pythonTuple;
	}

	private static void EnsureModule(CodeContext context, PythonDictionary dict)
	{
		if (!dict.ContainsKey("__module__") && context.TryGetVariable("__name__", out var value))
		{
			dict["__module__"] = value;
		}
	}

	private void InitializeSystemType()
	{
		IsSystemType = true;
		IsPythonType = PythonBinder.IsPythonType(_underlyingSystemType);
		_name = NameConverter.GetTypeName(_underlyingSystemType);
		AddSystemBases();
	}

	private void AddSystemBases()
	{
		List<PythonType> list = new List<PythonType>();
		list.Add(this);
		if (_underlyingSystemType.GetBaseType() != null)
		{
			Type type = ((_underlyingSystemType == typeof(bool)) ? typeof(int) : ((!(_underlyingSystemType.GetBaseType() == typeof(ValueType))) ? _underlyingSystemType.GetBaseType() : typeof(object)));
			while (type.IsDefined(typeof(PythonHiddenBaseClassAttribute), inherit: false))
			{
				type = type.GetBaseType();
			}
			_bases = new PythonType[1] { GetPythonType(type) };
			Type type2 = type;
			while (type2 != null)
			{
				if (TryReplaceExtensibleWithBase(type2, out var newType))
				{
					list.Add(DynamicHelpers.GetPythonTypeFromType(newType));
				}
				else if (!type2.IsDefined(typeof(PythonHiddenBaseClassAttribute), inherit: false))
				{
					list.Add(DynamicHelpers.GetPythonTypeFromType(type2));
				}
				type2 = type2.GetBaseType();
			}
			if (!IsPythonType)
			{
				AddSystemInterfaces(list);
			}
		}
		else if (_underlyingSystemType.IsInterface())
		{
			Type[] interfaces = _underlyingSystemType.GetInterfaces();
			PythonType[] array = new PythonType[interfaces.Length];
			for (int i = 0; i < interfaces.Length; i++)
			{
				Type type3 = interfaces[i];
				PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(type3);
				list.Add(pythonTypeFromType);
				array[i] = pythonTypeFromType;
			}
			_bases = array;
		}
		else
		{
			_bases = new PythonType[0];
		}
		_resolutionOrder = list;
	}

	private void AddSystemInterfaces(List<PythonType> mro)
	{
		if (_underlyingSystemType.IsArray)
		{
			mro.Add(DynamicHelpers.GetPythonTypeFromType(typeof(IList)));
			mro.Add(DynamicHelpers.GetPythonTypeFromType(typeof(ICollection)));
			mro.Add(DynamicHelpers.GetPythonTypeFromType(typeof(IEnumerable)));
			return;
		}
		Type[] interfaces = _underlyingSystemType.GetInterfaces();
		Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
		bool flag = false;
		List<Type> list = new List<Type>(interfaces);
		Type[] array = interfaces;
		foreach (Type type in array)
		{
			InterfaceMapping interfaceMap = _underlyingSystemType.GetInterfaceMap(type);
			for (int j = 0; j < interfaceMap.TargetMethods.Length; j++)
			{
				MethodInfo methodInfo = interfaceMap.TargetMethods[j];
				if (!(methodInfo == null))
				{
					if (!methodInfo.IsPrivate)
					{
						dictionary[methodInfo.Name] = null;
					}
					else
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			for (int k = 0; k < interfaceMap.TargetMethods.Length; k++)
			{
				MethodInfo methodInfo2 = interfaceMap.TargetMethods[k];
				MethodInfo methodInfo3 = interfaceMap.InterfaceMethods[k];
				if (!(methodInfo2 != null) || !methodInfo2.IsPrivate)
				{
					continue;
				}
				flag = true;
				if (dictionary.TryGetValue(methodInfo3.Name, out var value))
				{
					if (value != null)
					{
						list.Remove(type);
						list.Remove(dictionary[methodInfo3.Name]);
						break;
					}
				}
				else
				{
					dictionary[methodInfo3.Name] = type;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (Type item in list)
		{
			mro.Add(DynamicHelpers.GetPythonTypeFromType(item));
		}
	}

	private void AddSystemConstructors()
	{
		if (typeof(Delegate).IsAssignableFrom(_underlyingSystemType))
		{
			SetConstructor(BuiltinFunction.MakeFunction(_underlyingSystemType.Name, new MethodInfo[1] { typeof(DelegateOps).GetMethod("__new__") }, _underlyingSystemType));
		}
		else if (!_underlyingSystemType.IsAbstract())
		{
			BuiltinFunction constructors = GetConstructors();
			if (constructors != null)
			{
				SetConstructor(constructors);
			}
		}
	}

	private BuiltinFunction GetConstructors()
	{
		Type underlyingSystemType = _underlyingSystemType;
		string name = Name;
		return PythonTypeOps.GetConstructorFunction(underlyingSystemType, name);
	}

	private void EnsureConstructor()
	{
		if (_ctor == null)
		{
			AddSystemConstructors();
			if (_ctor == null)
			{
				throw PythonOps.TypeError(_underlyingSystemType.FullName + " does not define any public constructors.");
			}
		}
	}

	private void EnsureInstanceCtor()
	{
		if (_instanceCtor == null)
		{
			_instanceCtor = InstanceCreator.Make(this);
		}
	}

	private void UpdateVersion()
	{
		foreach (WeakReference subType in SubTypes)
		{
			if (subType.IsAlive)
			{
				((PythonType)subType.Target).UpdateVersion();
			}
		}
		_lenSlot = null;
		_version = GetNextVersion();
	}

	private static int GetNextVersion()
	{
		if (MasterVersion < 0)
		{
			throw new InvalidOperationException(Resources.TooManyVersions);
		}
		return Interlocked.Increment(ref MasterVersion);
	}

	private void EnsureDict()
	{
		if (_dict == null)
		{
			Interlocked.CompareExchange(ref _dict, new Dictionary<string, PythonTypeSlot>(StringComparer.Ordinal), null);
		}
	}

	private void AddSubType(PythonType subtype)
	{
		if (_subtypes == null)
		{
			Interlocked.CompareExchange(ref _subtypes, new List<WeakReference>(), null);
		}
		lock (_subtypesLock)
		{
			_subtypes.Add(new WeakReference(subtype));
		}
	}

	private void RemoveSubType(PythonType subtype)
	{
		int num = 0;
		if (_subtypes == null)
		{
			return;
		}
		lock (_subtypesLock)
		{
			while (num < _subtypes.Count)
			{
				if (!_subtypes[num].IsAlive || _subtypes[num].Target == subtype)
				{
					_subtypes.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	IList<string> IMembersList.GetMemberNames()
	{
		return PythonOps.GetStringMemberList(this);
	}

	IList<object> IPythonMembersList.GetMemberNames(CodeContext context)
	{
		IList<object> memberNames = GetMemberNames(context);
		object[] array = new object[memberNames.Count];
		memberNames.CopyTo(array, 0);
		Array.Sort(array);
		return array;
	}

	WeakRefTracker IWeakReferenceable.GetWeakRef()
	{
		return _weakrefTracker;
	}

	bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
	{
		return Interlocked.CompareExchange(ref _weakrefTracker, value, null) == null;
	}

	void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
	{
		_weakrefTracker = value;
	}

	[PythonHidden]
	public DynamicMetaObject GetMetaObject(Expression parameter)
	{
		return new MetaPythonType(parameter, BindingRestrictions.Empty, this);
	}

	internal WeakReference GetSharedWeakReference()
	{
		if (_weakRef == null)
		{
			_weakRef = new WeakReference(this);
		}
		return _weakRef;
	}

	T IFastSettable.MakeSetBinding<T>(CallSite<T> site, PythonSetMemberBinder binder)
	{
		if (!IsSystemType && !TryGetCustomSetAttr(Context.SharedContext, out var _))
		{
			CodeContext sharedContext = PythonContext.GetPythonContext(binder).SharedContext;
			string name = binder.Name;
			Type typeFromHandle = typeof(T);
			if (typeFromHandle == typeof(Func<CallSite, object, object, object>))
			{
				return (T)(object)MakeFastSet<object>(sharedContext, name);
			}
			if (typeFromHandle == typeof(Func<CallSite, object, string, object>))
			{
				return (T)(object)MakeFastSet<string>(sharedContext, name);
			}
			if (typeFromHandle == typeof(Func<CallSite, object, int, object>))
			{
				return (T)(object)MakeFastSet<int>(sharedContext, name);
			}
			if (typeFromHandle == typeof(Func<CallSite, object, double, object>))
			{
				return (T)(object)MakeFastSet<double>(sharedContext, name);
			}
			if (typeFromHandle == typeof(Func<CallSite, object, List, object>))
			{
				return (T)(object)MakeFastSet<List>(sharedContext, name);
			}
			if (typeFromHandle == typeof(Func<CallSite, object, PythonTuple, object>))
			{
				return (T)(object)MakeFastSet<PythonTuple>(sharedContext, name);
			}
			if (typeFromHandle == typeof(Func<CallSite, object, PythonDictionary, object>))
			{
				return (T)(object)MakeFastSet<PythonDictionary>(sharedContext, name);
			}
			if (typeFromHandle == typeof(Func<CallSite, object, SetCollection, object>))
			{
				return (T)(object)MakeFastSet<SetCollection>(sharedContext, name);
			}
			if (typeFromHandle == typeof(Func<CallSite, object, FrozenSetCollection, object>))
			{
				return (T)(object)MakeFastSet<FrozenSetCollection>(sharedContext, name);
			}
		}
		return null;
	}

	private static Func<CallSite, object, T, object> MakeFastSet<T>(CodeContext context, string name)
	{
		return new Setter<T>(context, name).Target;
	}
}
