using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using IronPython.Modules;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Debugging;
using Microsoft.Scripting.Debugging.CompilerServices;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public sealed class PythonContext : LanguageContext
{
	internal sealed class PythonEqualityComparer : IEqualityComparer, IEqualityComparer<object>
	{
		public readonly PythonContext Context;

		public PythonEqualityComparer(PythonContext context)
		{
			Context = context;
		}

		bool IEqualityComparer.Equals(object x, object y)
		{
			return PythonOps.EqualRetBool(Context._defaultContext, x, y);
		}

		bool IEqualityComparer<object>.Equals(object x, object y)
		{
			return PythonOps.EqualRetBool(Context._defaultContext, x, y);
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return Hash(obj);
		}

		int IEqualityComparer<object>.GetHashCode(object obj)
		{
			return Hash(obj);
		}
	}

	private sealed class OptimizedUserHasher
	{
		private readonly PythonContext _context;

		private readonly PythonType _pt;

		public OptimizedUserHasher(PythonContext context, PythonType pt)
		{
			_context = context;
			_pt = pt;
		}

		public int Hasher(object o, ref HashDelegate dlg)
		{
			if (o is IPythonObject pythonObject && pythonObject.PythonType == _pt)
			{
				return _pt.Hash(o);
			}
			dlg = _context.FallbackHasher;
			return _context.FallbackHasher(o, ref dlg);
		}
	}

	private sealed class OptimizedBuiltinHasher
	{
		private readonly PythonContext _context;

		private readonly Type _type;

		private readonly PythonType _pt;

		public OptimizedBuiltinHasher(PythonContext context, Type type)
		{
			_context = context;
			_type = type;
			_pt = DynamicHelpers.GetPythonTypeFromType(type);
		}

		public int Hasher(object o, ref HashDelegate dlg)
		{
			if (o != null && o.GetType() == _type)
			{
				return _pt.Hash(o);
			}
			dlg = _context.FallbackHasher;
			return _context.FallbackHasher(o, ref dlg);
		}
	}

	private class AssemblyResolveHolder
	{
		private readonly WeakReference _context;

		public AssemblyResolveHolder(PythonContext context)
		{
			_context = new WeakReference(context);
		}

		internal Assembly AssemblyResolveEvent(object sender, ResolveEventArgs args)
		{
			PythonContext pythonContext = (PythonContext)_context.Target;
			if (pythonContext != null)
			{
				return pythonContext.CurrentDomain_AssemblyResolve(sender, args);
			}
			AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolveEvent;
			return null;
		}
	}

	private class AttrKey : IEquatable<AttrKey>
	{
		private Type _type;

		private string _name;

		public AttrKey(Type type, string name)
		{
			_type = type;
			_name = name;
		}

		public bool Equals(AttrKey other)
		{
			if (other == null)
			{
				return false;
			}
			if (_type == other._type)
			{
				return _name == other._name;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AttrKey);
		}

		public override int GetHashCode()
		{
			return _type.GetHashCode() ^ _name.GetHashCode();
		}
	}

	private class DefaultPythonComparer : IComparer
	{
		private CallSite<Func<CallSite, object, object, int>> _site;

		public DefaultPythonComparer(PythonContext context)
		{
			_site = CallSite<Func<CallSite, object, object, int>>.Create(context.Operation(PythonOperationKind.Compare));
		}

		public int Compare(object x, object y)
		{
			return _site.Target(_site, x, y);
		}
	}

	private class FunctionComparer<T> : IComparer
	{
		private T _cmpfunc;

		private CallSite<Func<CallSite, CodeContext, T, object, object, int>> _funcSite;

		private CodeContext _context;

		public FunctionComparer(PythonContext context, T cmpfunc)
			: this(context, cmpfunc, MakeCompareSite<T>(context))
		{
		}

		public FunctionComparer(PythonContext context, T cmpfunc, CallSite<Func<CallSite, CodeContext, T, object, object, int>> site)
		{
			_cmpfunc = cmpfunc;
			_context = context.SharedContext;
			_funcSite = site;
		}

		public int Compare(object o1, object o2)
		{
			return _funcSite.Target(_funcSite, _context, _cmpfunc, o1, o2);
		}
	}

	private class OperationRetTypeKey<T> : IEquatable<OperationRetTypeKey<T>>
	{
		public readonly Type ReturnType;

		public readonly T Operation;

		public OperationRetTypeKey(Type retType, T operation)
		{
			ReturnType = retType;
			Operation = operation;
		}

		public bool Equals(OperationRetTypeKey<T> other)
		{
			if (other.ReturnType == ReturnType)
			{
				return other.Operation.Equals(Operation);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ReturnType.GetHashCode() ^ Operation.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is OperationRetTypeKey<T> other)
			{
				return Equals(other);
			}
			return false;
		}
	}

	internal const string IronPythonDisplayName = "IronPython 2.7.3";

	internal const string IronPythonNames = "IronPython;Python;py";

	internal const string IronPythonFileExtensions = ".py";

	private static readonly Guid PythonLanguageGuid = new Guid("03ed4b80-d10b-442f-ad9a-47dae85b2051");

	private static readonly Guid LanguageVendor_Microsoft = new Guid(-1723120188, -6423, 4562, 144, 63, 0, 192, 79, 163, 2, 161);

	private readonly IDictionary<object, object> _modulesDict = new PythonDictionary();

	private readonly Dictionary<string, ModuleGlobalCache> _builtinCache = new Dictionary<string, ModuleGlobalCache>(StringComparer.Ordinal);

	private readonly Dictionary<Type, string> _builtinModuleNames = new Dictionary<Type, string>();

	private readonly PythonOptions _options;

	private readonly PythonModule _systemState;

	private readonly Dictionary<string, Type> _builtinModulesDict;

	private readonly PythonOverloadResolverFactory _sharedOverloadResolverFactory;

	private readonly PythonBinder _binder;

	private readonly SysModuleDictionaryStorage _sysDict = new SysModuleDictionaryStorage();

	private readonly AssemblyResolveHolder _resolveHolder;

	private readonly HashSet<Assembly> _loadedAssemblies = new HashSet<Assembly>();

	private Encoding _defaultEncoding = PythonAsciiEncoding.Instance;

	private PythonService _pythonService;

	private string _initialExecutable;

	private string _initialPrefix = GetInitialPrefix();

	private string _initialVersionString;

	private PythonModule _clrModule;

	private PythonDictionary _builtinDict;

	private PythonModule _builtins;

	private PythonFileManager _fileManager;

	private Dictionary<string, object> _errorHandlers;

	private List<object> _searchFunctions;

	private Dictionary<object, object> _moduleState;

	internal BuiltinFunction NewObject;

	internal BuiltinFunction PythonReconstructor;

	private Dictionary<Type, object> _genericSiteStorage;

	private CallSite<Func<CallSite, CodeContext, object, object>>[] _newUnarySites;

	private CallSite<Func<CallSite, CodeContext, object, object, object, object>>[] _newTernarySites;

	private CallSite<Func<CallSite, object, object, int>> _compareSite;

	private Dictionary<AttrKey, CallSite<Func<CallSite, object, object, object>>> _setAttrSites;

	private Dictionary<AttrKey, CallSite<Action<CallSite, object>>> _deleteAttrSites;

	private CallSite<Func<CallSite, CodeContext, object, string, PythonTuple, PythonDictionary, object>> _metaClassSite;

	private CallSite<Func<CallSite, CodeContext, object, string, object>> _writeSite;

	private CallSite<Func<CallSite, object, object, object>> _getIndexSite;

	private CallSite<Func<CallSite, object, object, object>> _equalSite;

	private CallSite<Action<CallSite, object, object>> _delIndexSite;

	private CallSite<Func<CallSite, CodeContext, object, object>> _finalizerSite;

	private CallSite<Func<CallSite, CodeContext, PythonFunction, object>> _functionCallSite;

	private CallSite<Func<CallSite, object, object, bool>> _greaterThanSite;

	private CallSite<Func<CallSite, object, object, bool>> _lessThanSite;

	private CallSite<Func<CallSite, object, object, bool>> _greaterThanEqualSite;

	private CallSite<Func<CallSite, object, object, bool>> _lessThanEqualSite;

	private CallSite<Func<CallSite, object, object, bool>> _containsSite;

	private CallSite<Func<CallSite, CodeContext, object, object[], object>> _callSplatSite;

	private CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>> _callDictSite;

	private CallSite<Func<CallSite, CodeContext, object, object, object, object>> _callDictSiteLooselyTyped;

	private CallSite<Func<CallSite, CodeContext, object, string, PythonDictionary, PythonDictionary, PythonTuple, int, object>> _importSite;

	private CallSite<Func<CallSite, CodeContext, object, string, PythonDictionary, PythonDictionary, PythonTuple, object>> _oldImportSite;

	private CallSite<Func<CallSite, object, bool>> _isCallableSite;

	private CallSite<Func<CallSite, object, IList<string>>> _getSignaturesSite;

	private CallSite<Func<CallSite, object, object, object>> _addSite;

	private CallSite<Func<CallSite, object, object, object>> _divModSite;

	private CallSite<Func<CallSite, object, object, object>> _rdivModSite;

	private CallSite<Func<CallSite, object, object, object, object>> _setIndexSite;

	private CallSite<Func<CallSite, object, object, object, object>> _delSliceSite;

	private CallSite<Func<CallSite, object, object, object, object, object>> _setSliceSite;

	private CallSite<Func<CallSite, object, string>> _docSite;

	private CallSite<Func<CallSite, object, int>> _intSite;

	private CallSite<Func<CallSite, object, string>> _tryStringSite;

	private CallSite<Func<CallSite, object, object>> _tryIntSite;

	private CallSite<Func<CallSite, object, IEnumerable>> _tryIEnumerableSite;

	private Dictionary<Type, CallSite<Func<CallSite, object, object>>> _implicitConvertSites;

	private Dictionary<PythonOperationKind, CallSite<Func<CallSite, object, object, object>>> _binarySites;

	private Dictionary<Type, DefaultPythonComparer> _defaultComparer;

	private CallSite<Func<CallSite, CodeContext, object, object, object, int>> _sharedFunctionCompareSite;

	private CallSite<Func<CallSite, CodeContext, PythonFunction, object, object, int>> _sharedPythonFunctionCompareSite;

	private CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object, int>> _sharedBuiltinFunctionCompareSite;

	private CallSite<Func<CallSite, CodeContext, object, int, object>> _getItemCallSite;

	private CallSite<Func<CallSite, CodeContext, object, object, object>> _propGetSite;

	private CallSite<Func<CallSite, CodeContext, object, object, object>> _propDelSite;

	private CallSite<Func<CallSite, CodeContext, object, object, object, object>> _propSetSite;

	private CompiledLoader _compiledLoader;

	internal bool _importWarningThrows;

	private bool _importedEncodings;

	private Action<Action> _commandDispatcher;

	private ClrModule.ReferencesList _referencesList;

	private FloatFormat _floatFormat;

	private FloatFormat _doubleFormat;

	private CultureInfo _collateCulture;

	private CultureInfo _ctypeCulture;

	private CultureInfo _timeCulture;

	private CultureInfo _monetaryCulture;

	private CultureInfo _numericCulture;

	private CodeContext _defaultContext;

	private CodeContext _defaultClsContext;

	private readonly TopNamespaceTracker _topNamespace;

	private readonly IEqualityComparer<object> _equalityComparer;

	private readonly IEqualityComparer _equalityComparerNonGeneric;

	private Dictionary<Type, CallSite<Func<CallSite, object, object, bool>>> _equalSites;

	private Dictionary<Type, PythonSiteCache> _systemSiteCache;

	internal static object _syntaxErrorNoCaret = new object();

	private PythonInvokeBinder _invokeNoArgs;

	private PythonInvokeBinder _invokeOneArg;

	private Dictionary<CallSignature, PythonInvokeBinder> _invokeBinders;

	private Dictionary<string, PythonGetMemberBinder> _getMemberBinders;

	private Dictionary<string, PythonGetMemberBinder> _tryGetMemberBinders;

	private Dictionary<string, PythonSetMemberBinder> _setMemberBinders;

	private Dictionary<string, PythonDeleteMemberBinder> _deleteMemberBinders;

	private Dictionary<string, CompatibilityGetMember> _compatGetMember;

	private Dictionary<string, CompatibilityGetMember> _compatGetMemberNoThrow;

	private Dictionary<PythonOperationKind, PythonOperationBinder> _operationBinders;

	private Dictionary<ExpressionType, PythonUnaryOperationBinder> _unaryBinders;

	private PythonBinaryOperationBinder[] _binaryBinders;

	private Dictionary<OperationRetTypeKey<ExpressionType>, BinaryRetTypeBinder> _binaryRetTypeBinders;

	private Dictionary<OperationRetTypeKey<PythonOperationKind>, BinaryRetTypeBinder> _operationRetTypeBinders;

	private Dictionary<Type, PythonConversionBinder>[] _conversionBinders;

	private Dictionary<Type, DynamicMetaObjectBinder>[] _convertRetObjectBinders;

	private Dictionary<CallSignature, CreateFallback> _createBinders;

	private Dictionary<CallSignature, CompatibilityInvokeBinder> _compatInvokeBinders;

	private PythonGetSliceBinder _getSlice;

	private PythonSetSliceBinder _setSlice;

	private PythonDeleteSliceBinder _deleteSlice;

	private PythonGetIndexBinder[] _getIndexBinders;

	private PythonSetIndexBinder[] _setIndexBinders;

	private PythonDeleteIndexBinder[] _deleteIndexBinders;

	private DynamicMetaObjectBinder _invokeTwoConvertToInt;

	private static CultureInfo _CCulture;

	private DynamicDelegateCreator _delegateCreator;

	private DebugContext _debugContext;

	private ITracePipeline _tracePipeline;

	[ThreadStatic]
	private static Stack<PythonTracebackListener> _tracebackListeners;

	private static int _tracingThreads;

	internal FunctionCode.CodeList _allCodes;

	internal readonly object _codeCleanupLock = new object();

	internal readonly object _codeUpdateLock = new object();

	internal int _codeCount;

	internal int _nextCodeCleanup = 200;

	private int _recursionLimit;

	[ThreadStatic]
	private static bool _enableTracing;

	internal readonly List<FunctionStack> _mainThreadFunctionStack;

	private CallSite<Func<CallSite, CodeContext, object, object>> _callSite0LightEh;

	private List<WeakReference> _weakExtensionMethodSets;

	private CallSite<Func<CallSite, CodeContext, object, object>> _callSite0;

	private CallSite<Func<CallSite, CodeContext, object, object, object>> _callSite1;

	private CallSite<Func<CallSite, CodeContext, object, object, object, object>> _callSite2;

	private CallSite<Func<CallSite, CodeContext, object, object, object, object, object>> _callSite3;

	private CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object>> _callSite4;

	private CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object>> _callSite5;

	private CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object, object>> _callSite6;

	private Thread _mainThread;

	internal readonly HashDelegate InitialHasher;

	internal readonly HashDelegate IntHasher;

	internal readonly HashDelegate DoubleHasher;

	internal readonly HashDelegate StringHasher;

	internal readonly HashDelegate FallbackHasher;

	internal CallSite<Func<CallSite, CodeContext, object, object>> CallSite0
	{
		get
		{
			EnsureCall0Site();
			return _callSite0;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object>> CallSite1
	{
		get
		{
			EnsureCall1Site();
			return _callSite1;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object, object>> CallSite2
	{
		get
		{
			EnsureCall2Site();
			return _callSite2;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object, object, object>> CallSite3
	{
		get
		{
			EnsureCall3Site();
			return _callSite3;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object>> CallSite4
	{
		get
		{
			EnsureCall4Site();
			return _callSite4;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object>> CallSite5
	{
		get
		{
			EnsureCall5Site();
			return _callSite5;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object, object>> CallSite6
	{
		get
		{
			EnsureCall6Site();
			return _callSite6;
		}
	}

	public int RecursionLimit
	{
		get
		{
			return _recursionLimit;
		}
		set
		{
			if (value < 0)
			{
				throw PythonOps.ValueError("recursion limit must be positive");
			}
			lock (_codeUpdateLock)
			{
				_recursionLimit = value;
				if (_recursionLimit == int.MaxValue != (value == int.MaxValue))
				{
					FunctionCode.UpdateAllCode(this);
				}
			}
		}
	}

	internal bool EnableTracing
	{
		get
		{
			if (_tracingThreads <= 0)
			{
				return PythonOptions.Tracing;
			}
			return true;
		}
		set
		{
			lock (_codeUpdateLock)
			{
				bool enableTracing = _enableTracing;
				_enableTracing = value;
				bool flag = false;
				if (value && !enableTracing)
				{
					flag = _tracingThreads == 0;
					_tracingThreads++;
				}
				else if (!value && enableTracing)
				{
					_tracingThreads--;
					flag = _tracingThreads == 0;
					if (flag)
					{
						_tracePipeline.TraceCallback = null;
					}
				}
				if (flag)
				{
					FunctionCode.UpdateAllCode(this);
				}
			}
		}
	}

	internal TopNamespaceTracker TopNamespace => _topNamespace;

	public Thread MainThread
	{
		get
		{
			return _mainThread;
		}
		set
		{
			_mainThread = value;
		}
	}

	public IEqualityComparer<object> EqualityComparer => _equalityComparer;

	public IEqualityComparer EqualityComparerNonGeneric => _equalityComparerNonGeneric;

	public override LanguageOptions Options => PythonOptions;

	internal PythonOptions PythonOptions => _options;

	public override Guid VendorGuid => LanguageVendor_Microsoft;

	public override Guid LanguageGuid => PythonLanguageGuid;

	public PythonModule SystemState => _systemState;

	public PythonModule ClrModule
	{
		get
		{
			if (_clrModule == null)
			{
				Interlocked.CompareExchange(ref _clrModule, CreateBuiltinModule("clr"), null);
			}
			return _clrModule;
		}
	}

	internal object SystemStandardOut => GetSystemStateValue("stdout");

	internal object SystemStandardIn => GetSystemStateValue("stdin");

	internal object SystemStandardError => GetSystemStateValue("stderr");

	internal IDictionary<object, object> SystemStateModules => _modulesDict;

	public override Version LanguageVersion => GetPythonVersion();

	internal FloatFormat FloatFormat
	{
		get
		{
			return _floatFormat;
		}
		set
		{
			_floatFormat = value;
		}
	}

	internal FloatFormat DoubleFormat
	{
		get
		{
			return _doubleFormat;
		}
		set
		{
			_doubleFormat = value;
		}
	}

	public Encoding DefaultEncoding
	{
		get
		{
			return _defaultEncoding;
		}
		set
		{
			_defaultEncoding = value;
		}
	}

	internal Dictionary<string, Type> BuiltinModules => _builtinModulesDict;

	internal Dictionary<Type, string> BuiltinModuleNames => _builtinModuleNames;

	public PythonModule BuiltinModuleInstance => _builtins;

	public PythonDictionary BuiltinModuleDict => _builtinDict;

	internal string InitialPrefix => _initialPrefix;

	internal PythonFileManager RawFileManager => _fileManager;

	internal PythonFileManager FileManager
	{
		get
		{
			if (_fileManager == null)
			{
				Interlocked.CompareExchange(ref _fileManager, new PythonFileManager(), null);
			}
			return _fileManager;
		}
	}

	internal Dictionary<string, object> ErrorHandlers
	{
		get
		{
			if (_errorHandlers == null)
			{
				Interlocked.CompareExchange(ref _errorHandlers, new Dictionary<string, object>(), null);
			}
			return _errorHandlers;
		}
	}

	internal List<object> SearchFunctions
	{
		get
		{
			if (_searchFunctions == null)
			{
				Interlocked.CompareExchange(ref _searchFunctions, new List<object>(), null);
			}
			return _searchFunctions;
		}
	}

	internal CallSite<Func<CallSite, object, object, int>> CompareSite
	{
		get
		{
			if (_compareSite == null)
			{
				Interlocked.CompareExchange(ref _compareSite, MakeSortCompareSite(), null);
			}
			return _compareSite;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, string, PythonTuple, PythonDictionary, object>> MetaClassCallSite
	{
		get
		{
			if (_metaClassSite == null)
			{
				Interlocked.CompareExchange(ref _metaClassSite, CallSite<Func<CallSite, CodeContext, object, string, PythonTuple, PythonDictionary, object>>.Create(Invoke(new CallSignature(3))), null);
			}
			return _metaClassSite;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, string, object>> WriteCallSite
	{
		get
		{
			if (_writeSite == null)
			{
				Interlocked.CompareExchange(ref _writeSite, CallSite<Func<CallSite, CodeContext, object, string, object>>.Create(InvokeOne), null);
			}
			return _writeSite;
		}
	}

	internal CallSite<Func<CallSite, object, object, object>> GetIndexSite
	{
		get
		{
			if (_getIndexSite == null)
			{
				Interlocked.CompareExchange(ref _getIndexSite, CallSite<Func<CallSite, object, object, object>>.Create(GetIndex(1)), null);
			}
			return _getIndexSite;
		}
	}

	internal CallSite<Func<CallSite, object, object, object>> EqualSite
	{
		get
		{
			if (_equalSite == null)
			{
				Interlocked.CompareExchange(ref _equalSite, CallSite<Func<CallSite, object, object, object>>.Create(BinaryOperation(ExpressionType.Equal)), null);
			}
			return _equalSite;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object>> FinalizerSite
	{
		get
		{
			if (_finalizerSite == null)
			{
				Interlocked.CompareExchange(ref _finalizerSite, CallSite<Func<CallSite, CodeContext, object, object>>.Create(InvokeNone), null);
			}
			return _finalizerSite;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, PythonFunction, object>> FunctionCallSite
	{
		get
		{
			if (_functionCallSite == null)
			{
				Interlocked.CompareExchange(ref _functionCallSite, CallSite<Func<CallSite, CodeContext, PythonFunction, object>>.Create(InvokeNone), null);
			}
			return _functionCallSite;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, string, PythonDictionary, PythonDictionary, PythonTuple, int, object>> ImportSite
	{
		get
		{
			if (_importSite == null)
			{
				Interlocked.CompareExchange(ref _importSite, CallSite<Func<CallSite, CodeContext, object, string, PythonDictionary, PythonDictionary, PythonTuple, int, object>>.Create(Invoke(new CallSignature(5)).GetLightExceptionBinder()), null);
			}
			return _importSite;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, string, PythonDictionary, PythonDictionary, PythonTuple, object>> OldImportSite
	{
		get
		{
			if (_oldImportSite == null)
			{
				Interlocked.CompareExchange(ref _oldImportSite, CallSite<Func<CallSite, CodeContext, object, string, PythonDictionary, PythonDictionary, PythonTuple, object>>.Create(Invoke(new CallSignature(4)).GetLightExceptionBinder()), null);
			}
			return _oldImportSite;
		}
	}

	internal CodeContext SharedContext => _defaultContext;

	internal PythonOverloadResolverFactory SharedOverloadResolverFactory => _sharedOverloadResolverFactory;

	internal CodeContext SharedClsContext => _defaultClsContext;

	internal ClrModule.ReferencesList ReferencedAssemblies
	{
		get
		{
			if (_referencesList == null)
			{
				Interlocked.CompareExchange(ref _referencesList, new ClrModule.ReferencesList(), null);
			}
			return _referencesList;
		}
	}

	internal static CultureInfo CCulture
	{
		get
		{
			if (_CCulture == null)
			{
				Interlocked.CompareExchange(ref _CCulture, MakeCCulture(), null);
			}
			return _CCulture;
		}
	}

	internal CultureInfo CollateCulture
	{
		get
		{
			if (_collateCulture == null)
			{
				_collateCulture = CCulture;
			}
			return _collateCulture;
		}
		set
		{
			_collateCulture = value;
		}
	}

	internal CultureInfo CTypeCulture
	{
		get
		{
			if (_ctypeCulture == null)
			{
				_ctypeCulture = CCulture;
			}
			return _ctypeCulture;
		}
		set
		{
			_ctypeCulture = value;
		}
	}

	internal CultureInfo TimeCulture
	{
		get
		{
			if (_timeCulture == null)
			{
				_timeCulture = CCulture;
			}
			return _timeCulture;
		}
		set
		{
			_timeCulture = value;
		}
	}

	internal CultureInfo MonetaryCulture
	{
		get
		{
			if (_monetaryCulture == null)
			{
				_monetaryCulture = CCulture;
			}
			return _monetaryCulture;
		}
		set
		{
			_monetaryCulture = value;
		}
	}

	internal CultureInfo NumericCulture
	{
		get
		{
			if (_numericCulture == null)
			{
				_numericCulture = CCulture;
			}
			return _numericCulture;
		}
		set
		{
			_numericCulture = value;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object>> PropertyGetSite
	{
		get
		{
			if (_propGetSite == null)
			{
				Interlocked.CompareExchange(ref _propGetSite, CallSite<Func<CallSite, CodeContext, object, object, object>>.Create(InvokeOne), null);
			}
			return _propGetSite;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object>> PropertyDeleteSite
	{
		get
		{
			if (_propDelSite == null)
			{
				Interlocked.CompareExchange(ref _propDelSite, CallSite<Func<CallSite, CodeContext, object, object, object>>.Create(InvokeOne), null);
			}
			return _propDelSite;
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object, object>> PropertySetSite
	{
		get
		{
			if (_propSetSite == null)
			{
				Interlocked.CompareExchange(ref _propSetSite, CallSite<Func<CallSite, CodeContext, object, object, object, object>>.Create(Invoke(new CallSignature(2))), null);
			}
			return _propSetSite;
		}
	}

	internal PythonBinder Binder => _binder;

	internal CallSite<Func<CallSite, CodeContext, object, int, object>> GetItemCallSite
	{
		get
		{
			if (_getItemCallSite == null)
			{
				Interlocked.CompareExchange(ref _getItemCallSite, CallSite<Func<CallSite, CodeContext, object, int, object>>.Create(new PythonInvokeBinder(this, new CallSignature(1))), null);
			}
			return _getItemCallSite;
		}
	}

	public DynamicDelegateCreator DelegateCreator
	{
		get
		{
			if (_delegateCreator == null)
			{
				Interlocked.CompareExchange(ref _delegateCreator, new DynamicDelegateCreator(this), null);
			}
			return _delegateCreator;
		}
	}

	internal PythonInvokeBinder InvokeNone
	{
		get
		{
			if (_invokeNoArgs == null)
			{
				_invokeNoArgs = Invoke(new CallSignature(0));
			}
			return _invokeNoArgs;
		}
	}

	internal PythonInvokeBinder InvokeOne
	{
		get
		{
			if (_invokeOneArg == null)
			{
				_invokeOneArg = Invoke(new CallSignature(1));
			}
			return _invokeOneArg;
		}
	}

	internal DynamicMetaObjectBinder InvokeTwoConvertToInt
	{
		get
		{
			if (_invokeTwoConvertToInt == null)
			{
				ParameterMappingInfo[] array = new ParameterMappingInfo[4];
				for (int i = 0; i < 4; i++)
				{
					array[i] = ParameterMappingInfo.Parameter(i);
				}
				_invokeTwoConvertToInt = new ComboBinder(new BinderMappingInfo(Invoke(new CallSignature(2)), array), new BinderMappingInfo(Convert(typeof(int), ConversionResultKind.ExplicitCast), ParameterMappingInfo.Action(0)));
			}
			return _invokeTwoConvertToInt;
		}
	}

	internal PythonGetSliceBinder GetSlice
	{
		get
		{
			if (_getSlice == null)
			{
				Interlocked.CompareExchange(ref _getSlice, new PythonGetSliceBinder(this), null);
			}
			return _getSlice;
		}
	}

	internal PythonSetSliceBinder SetSliceBinder
	{
		get
		{
			if (_setSlice == null)
			{
				Interlocked.CompareExchange(ref _setSlice, new PythonSetSliceBinder(this), null);
			}
			return _setSlice;
		}
	}

	internal PythonDeleteSliceBinder DeleteSlice
	{
		get
		{
			if (_deleteSlice == null)
			{
				Interlocked.CompareExchange(ref _deleteSlice, new PythonDeleteSliceBinder(this), null);
			}
			return _deleteSlice;
		}
	}

	internal static PythonTracebackListener TracebackListener
	{
		get
		{
			if (_tracebackListeners == null)
			{
				return null;
			}
			return _tracebackListeners.Peek();
		}
	}

	internal DebugContext DebugContext
	{
		get
		{
			EnsureDebugContext();
			return _debugContext;
		}
	}

	internal ITracePipeline TracePipeline => _tracePipeline;

	private void EnsureCall0Site()
	{
		if (_callSite0 == null)
		{
			Interlocked.CompareExchange(ref _callSite0, CallSite<Func<CallSite, CodeContext, object, object>>.Create(Invoke(new CallSignature(0))), null);
		}
	}

	private void EnsureCall1Site()
	{
		if (_callSite1 == null)
		{
			Interlocked.CompareExchange(ref _callSite1, CallSite<Func<CallSite, CodeContext, object, object, object>>.Create(Invoke(new CallSignature(1))), null);
		}
	}

	private void EnsureCall2Site()
	{
		if (_callSite2 == null)
		{
			Interlocked.CompareExchange(ref _callSite2, CallSite<Func<CallSite, CodeContext, object, object, object, object>>.Create(Invoke(new CallSignature(2))), null);
		}
	}

	private void EnsureCall3Site()
	{
		if (_callSite3 == null)
		{
			Interlocked.CompareExchange(ref _callSite3, CallSite<Func<CallSite, CodeContext, object, object, object, object, object>>.Create(Invoke(new CallSignature(3))), null);
		}
	}

	private void EnsureCall4Site()
	{
		if (_callSite4 == null)
		{
			Interlocked.CompareExchange(ref _callSite4, CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object>>.Create(Invoke(new CallSignature(4))), null);
		}
	}

	private void EnsureCall5Site()
	{
		if (_callSite5 == null)
		{
			Interlocked.CompareExchange(ref _callSite5, CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object>>.Create(Invoke(new CallSignature(5))), null);
		}
	}

	private void EnsureCall6Site()
	{
		if (_callSite6 == null)
		{
			Interlocked.CompareExchange(ref _callSite6, CallSite<Func<CallSite, CodeContext, object, object, object, object, object, object, object, object>>.Create(Invoke(new CallSignature(6))), null);
		}
	}

	public PythonContext(ScriptDomainManager manager, IDictionary<string, object> options)
		: base(manager)
	{
		_options = new PythonOptions(options);
		_builtinModulesDict = CreateBuiltinTable();
		PythonDictionary globals = new PythonDictionary();
		ModuleContext moduleContext = new ModuleContext(globals, this);
		_defaultContext = moduleContext.GlobalContext;
		PythonDictionary dict = new PythonDictionary(_sysDict);
		_systemState = new PythonModule(dict);
		_systemState.__dict__["__name__"] = "sys";
		_systemState.__dict__["__package__"] = null;
		PythonBinder binder = new PythonBinder(this, _defaultContext);
		_sharedOverloadResolverFactory = new PythonOverloadResolverFactory(binder, System.Linq.Expressions.Expression.Constant(_defaultContext));
		_binder = binder;
		CodeContext defaultClsCodeContext = (_defaultClsContext = DefaultContext.CreateDefaultCLSContext(this));
		if (DefaultContext._default == null)
		{
			DefaultContext.InitializeDefaults(_defaultContext, defaultClsCodeContext);
		}
		InitializeBuiltins();
		InitializeSystemState();
		SetSystemStateValue("argv", (_options.Arguments.Count == 0) ? new List(new object[1] { string.Empty }) : new List(_options.Arguments));
		if (_options.WarningFilters.Count > 0)
		{
			_systemState.__dict__["warnoptions"] = new List(_options.WarningFilters);
		}
		if (_options.Frames)
		{
			BuiltinFunction value = BuiltinFunction.MakeFunction("_getframe", ArrayUtils.ConvertAll(typeof(SysModule).GetMember("_getframeImpl"), (MemberInfo x) => (MethodBase)x), typeof(SysModule));
			_systemState.__dict__["_getframe"] = value;
		}
		if (_options.Tracing)
		{
			EnsureDebugContext();
			RegisterTracebackHandler();
		}
		List list = new List(_options.SearchPaths);
		_resolveHolder = new AssemblyResolveHolder(this);
		try
		{
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			if (entryAssembly != null)
			{
				string directoryName = Path.GetDirectoryName(entryAssembly.Location);
				string item = Path.Combine(directoryName, "Lib");
				list.append(item);
				list.append(Path.Combine(directoryName, "DLLs"));
			}
		}
		catch (SecurityException)
		{
		}
		_systemState.__dict__["path"] = list;
		RecursionLimit = _options.RecursionLimit;
		if (options == null || !options.TryGetValue("NoAssemblyResolveHook", out var value2) || !System.Convert.ToBoolean(value2))
		{
			try
			{
				HookAssemblyResolve();
			}
			catch (SecurityException)
			{
			}
		}
		_equalityComparer = new PythonEqualityComparer(this);
		_equalityComparerNonGeneric = (IEqualityComparer)_equalityComparer;
		InitialHasher = InitialHasherImpl;
		IntHasher = IntHasherImpl;
		DoubleHasher = DoubleHasherImpl;
		StringHasher = StringHasherImpl;
		FallbackHasher = FallbackHasherImpl;
		_topNamespace = new TopNamespaceTracker(manager);
		foreach (Assembly loadedAssembly in manager.GetLoadedAssemblyList())
		{
			_topNamespace.LoadAssembly(loadedAssembly);
		}
		manager.AssemblyLoaded += ManagerAssemblyLoaded;
		_mainThreadFunctionStack = PythonOps.GetFunctionStack();
	}

	private void ManagerAssemblyLoaded(object sender, AssemblyLoadedEventArgs e)
	{
		_topNamespace.LoadAssembly(e.Assembly);
	}

	private int InitialHasherImpl(object o, ref HashDelegate dlg)
	{
		if (o == null)
		{
			return 505032256;
		}
		switch (o.GetType().GetTypeCode())
		{
		case TypeCode.String:
			dlg = StringHasher;
			return StringHasher(o, ref dlg);
		case TypeCode.Int32:
			dlg = IntHasher;
			return IntHasher(o, ref dlg);
		case TypeCode.Double:
			dlg = DoubleHasher;
			return DoubleHasher(o, ref dlg);
		default:
			if (o is IPythonObject)
			{
				dlg = new OptimizedUserHasher(this, ((IPythonObject)o).PythonType).Hasher;
			}
			else
			{
				dlg = new OptimizedBuiltinHasher(this, o.GetType()).Hasher;
			}
			return dlg(o, ref dlg);
		}
	}

	private int IntHasherImpl(object o, ref HashDelegate dlg)
	{
		if (o != null && o.GetType() == typeof(int))
		{
			return o.GetHashCode();
		}
		dlg = FallbackHasher;
		return FallbackHasher(o, ref dlg);
	}

	private int DoubleHasherImpl(object o, ref HashDelegate dlg)
	{
		if (o != null && o.GetType() == typeof(double))
		{
			return DoubleOps.__hash__((double)o);
		}
		dlg = FallbackHasher;
		return FallbackHasher(o, ref dlg);
	}

	private int StringHasherImpl(object o, ref HashDelegate dlg)
	{
		if (o != null && o.GetType() == typeof(string))
		{
			return o.GetHashCode();
		}
		dlg = FallbackHasher;
		return FallbackHasher(o, ref dlg);
	}

	private int FallbackHasherImpl(object o, ref HashDelegate dlg)
	{
		return PythonOps.Hash(SharedContext, o);
	}

	public bool HasModuleState(object key)
	{
		EnsureModuleState();
		lock (_moduleState)
		{
			return _moduleState.ContainsKey(key);
		}
	}

	private void EnsureModuleState()
	{
		if (_moduleState == null)
		{
			Interlocked.CompareExchange(ref _moduleState, new Dictionary<object, object>(), null);
		}
	}

	public object GetModuleState(object key)
	{
		EnsureModuleState();
		lock (_moduleState)
		{
			return _moduleState[key];
		}
	}

	public void SetModuleState(object key, object value)
	{
		EnsureModuleState();
		lock (_moduleState)
		{
			_moduleState[key] = value;
		}
	}

	public object GetSetModuleState(object key, object value)
	{
		EnsureModuleState();
		lock (_moduleState)
		{
			_moduleState.TryGetValue(key, out var value2);
			_moduleState[key] = value;
			return value2;
		}
	}

	public T GetOrCreateModuleState<T>(object key, Func<T> value) where T : class
	{
		EnsureModuleState();
		lock (_moduleState)
		{
			if (!_moduleState.TryGetValue(key, out var value2))
			{
				value2 = (_moduleState[key] = value());
			}
			return value2 as T;
		}
	}

	public PythonType EnsureModuleException(object key, PythonDictionary dict, string name, string module)
	{
		object obj = (dict[name] = GetOrCreateModuleState(key, () => PythonExceptions.CreateSubType(this, PythonExceptions.Exception, name, module, "", PythonType.DefaultMakeException)));
		return (PythonType)obj;
	}

	public PythonType EnsureModuleException(object key, PythonType baseType, PythonDictionary dict, string name, string module)
	{
		object obj = (dict[name] = GetOrCreateModuleState(key, () => PythonExceptions.CreateSubType(this, baseType, name, module, "", PythonType.DefaultMakeException)));
		return (PythonType)obj;
	}

	public PythonType EnsureModuleException(object key, PythonType baseType, Type underlyingType, PythonDictionary dict, string name, string module, Func<string, Exception> exceptionMaker)
	{
		object obj = (dict[name] = GetOrCreateModuleState(key, () => PythonExceptions.CreateSubType(this, baseType, underlyingType, name, module, "", exceptionMaker)));
		return (PythonType)obj;
	}

	public PythonType EnsureModuleException(object key, PythonType[] baseTypes, Type underlyingType, PythonDictionary dict, string name, string module)
	{
		object obj = (dict[name] = GetOrCreateModuleState(key, () => PythonExceptions.CreateSubType(this, baseTypes, underlyingType, name, module, "", PythonType.DefaultMakeException)));
		return (PythonType)obj;
	}

	internal bool TryGetSystemPath(out List path)
	{
		if (SystemState.__dict__.TryGetValue("path", out var value))
		{
			path = value as List;
		}
		else
		{
			path = null;
		}
		return path != null;
	}

	internal void UpdateExceptionInfo(object type, object value, object traceback)
	{
		_sysDict.UpdateExceptionInfo(type, value, traceback);
	}

	internal void UpdateExceptionInfo(Exception clrException, object type, object value, List<DynamicStackFrame> traceback)
	{
		_sysDict.UpdateExceptionInfo(clrException, type, value, traceback);
	}

	internal void ExceptionHandled()
	{
		_sysDict.ExceptionHandled();
	}

	internal PythonModule GetModuleByName(string name)
	{
		if (SystemStateModules.TryGetValue(name, out var value) && value is PythonModule result)
		{
			return result;
		}
		return null;
	}

	internal PythonModule GetModuleByPath(string path)
	{
		foreach (object value in SystemStateModules.Values)
		{
			if (value is PythonModule pythonModule && base.DomainManager.Platform.PathComparer.Compare(pythonModule.GetFile(), path) == 0)
			{
				return pythonModule;
			}
		}
		return null;
	}

	internal static Version GetPythonVersion()
	{
		return new AssemblyName(typeof(PythonContext).Assembly.FullName).Version;
	}

	private void InitializeSystemState()
	{
		SetSystemStateValue("argv", List.FromArrayNoCopy(string.Empty));
		SetSystemStateValue("modules", _modulesDict);
		InitializeSysFlags();
		_modulesDict["sys"] = _systemState;
		SetSystemStateValue("path", new List(3));
		SetStandardIO();
		SysModule.PerformModuleReload(this, _systemState.__dict__);
	}

	internal bool EmitDebugSymbols(SourceUnit sourceUnit)
	{
		if (sourceUnit.EmitDebugSymbols)
		{
			if (PythonOptions.NoDebug != null)
			{
				return !PythonOptions.NoDebug.IsMatch(sourceUnit.Path);
			}
			return true;
		}
		return false;
	}

	private void InitializeSysFlags()
	{
		SysModule.SysFlags sysFlags = new SysModule.SysFlags();
		SetSystemStateValue("flags", sysFlags);
		sysFlags.debug = (_options.Debug ? 1 : 0);
		sysFlags.py3k_warning = (_options.WarnPython30 ? 1 : 0);
		SetSystemStateValue("py3kwarning", _options.WarnPython30);
		switch (_options.DivisionOptions)
		{
		case PythonDivisionOptions.New:
			sysFlags.division_new = 1;
			break;
		case PythonDivisionOptions.Warn:
			sysFlags.division_warning = 1;
			break;
		case PythonDivisionOptions.WarnAll:
			sysFlags.division_warning = 2;
			break;
		}
		int inspect = (sysFlags.interactive = (_options.Inspect ? 1 : 0));
		sysFlags.inspect = inspect;
		if (_options.StripDocStrings)
		{
			sysFlags.optimize = 2;
		}
		else if (_options.Optimize)
		{
			sysFlags.optimize = 1;
		}
		sysFlags.dont_write_bytecode = 1;
		SetSystemStateValue("dont_write_bytecode", true);
		sysFlags.no_user_site = (_options.NoUserSite ? 1 : 0);
		sysFlags.no_site = (_options.NoSite ? 1 : 0);
		sysFlags.ignore_environment = (_options.IgnoreEnvironment ? 1 : 0);
		switch (_options.IndentationInconsistencySeverity)
		{
		case Severity.Warning:
			sysFlags.tabcheck = 1;
			break;
		case Severity.Error:
			sysFlags.tabcheck = 2;
			break;
		}
		sysFlags.verbose = (_options.Verbose ? 1 : 0);
		sysFlags.unicode = 1;
		sysFlags.bytes_warning = (_options.BytesWarning ? 1 : 0);
	}

	internal bool ShouldInterpret(PythonCompilerOptions options, SourceUnit source)
	{
		bool result = !_options.NoAdaptiveCompilation && !EmitDebugSymbols(source);
		if (!options.Interpreted)
		{
			return result;
		}
		return true;
	}

	private static PythonAst ParseAndBindAst(CompilerContext context)
	{
		ScriptCodeParseResult properties = ScriptCodeParseResult.Complete;
		bool flag = false;
		int num = 0;
		PythonAst pythonAst;
		using (Parser parser = Parser.CreateParser(context, GetPythonOptions(null)))
		{
			switch (context.SourceUnit.Kind)
			{
			case SourceCodeKind.InteractiveCode:
				pythonAst = parser.ParseInteractiveCode(out properties);
				flag = true;
				break;
			case SourceCodeKind.Expression:
				pythonAst = parser.ParseTopExpression();
				break;
			case SourceCodeKind.SingleStatement:
				pythonAst = parser.ParseSingleStatement();
				break;
			case SourceCodeKind.File:
				pythonAst = parser.ParseFile(makeModule: true, returnValue: false);
				break;
			case SourceCodeKind.Statements:
				pythonAst = parser.ParseFile(makeModule: false, returnValue: false);
				break;
			default:
				pythonAst = parser.ParseFile(makeModule: true, returnValue: true);
				break;
			}
			num = parser.ErrorCode;
		}
		if (!flag && num != 0)
		{
			properties = ScriptCodeParseResult.Invalid;
		}
		context.SourceUnit.CodeProperties = properties;
		if (num != 0 || properties == ScriptCodeParseResult.Empty)
		{
			return null;
		}
		pythonAst.Bind();
		return pythonAst;
	}

	internal static ScriptCode CompilePythonCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
	{
		PythonCompilerOptions pythonCompilerOptions = (PythonCompilerOptions)options;
		if (sourceUnit.Kind == SourceCodeKind.File)
		{
			pythonCompilerOptions.Module |= ModuleOptions.Initialize;
		}
		CompilerContext context = new CompilerContext(sourceUnit, options, errorSink);
		return ParseAndBindAst(context)?.ToScriptCode();
	}

	public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
	{
		ScriptCode scriptCode = CompilePythonCode(sourceUnit, options, errorSink);
		if (scriptCode != null)
		{
			Scope scope = scriptCode.CreateScope();
			PythonScopeExtension pythonScopeExtension = (PythonScopeExtension)scope.GetExtension(base.ContextId);
			if (pythonScopeExtension != null)
			{
				InitializeModule(sourceUnit.Path, pythonScopeExtension.ModuleContext, scriptCode, ModuleOptions.None);
			}
		}
		return scriptCode;
	}

	public override ScriptCode LoadCompiledCode(Delegate method, string path, string customData)
	{
		SourceUnit sourceUnit = new SourceUnit(this, NullTextContentProvider.Null, path, SourceCodeKind.File);
		return new OnDiskScriptCode((LookupCompilationDelegate)method, sourceUnit, customData);
	}

	public override SourceCodeReader GetSourceReader(Stream stream, Encoding defaultEncoding, string path)
	{
		ContractUtils.RequiresNotNull(stream, "stream");
		ContractUtils.RequiresNotNull(defaultEncoding, "defaultEncoding");
		ContractUtils.Requires(stream.CanSeek && stream.CanRead, "stream", "The stream must support seeking and reading");
		Encoding enc = PythonAsciiEncoding.SourceEncoding;
		long position = stream.Position;
		StreamReader reader = new StreamReader(stream, PythonAsciiEncoding.SourceEncoding);
		byte[] array = new byte[3];
		int num = stream.Read(array, 0, 3);
		int totalRead = 0;
		bool flag = false;
		if (num == 3 && array[0] == 239 && array[1] == 187 && array[2] == 191)
		{
			flag = true;
			totalRead = 3;
		}
		else
		{
			stream.Seek(0L, SeekOrigin.Begin);
		}
		string text;
		try
		{
			text = ReadOneLine(reader, ref totalRead);
		}
		catch (BadSourceException)
		{
			throw ReportEncodingError(stream, path);
		}
		bool flag2 = false;
		string encName = null;
		if (text != null && !(flag2 = Tokenizer.TryGetEncoding(defaultEncoding, text, ref enc, out encName)))
		{
			try
			{
				text = ReadOneLine(reader, ref totalRead);
			}
			catch (BadSourceException)
			{
				throw ReportEncodingError(stream, path);
			}
			if (text != null)
			{
				flag2 = Tokenizer.TryGetEncoding(defaultEncoding, text, ref enc, out encName);
			}
		}
		if (flag2 && flag && encName != "utf-8")
		{
			throw new IOException("file has both Unicode marker and PEP-263 file encoding.  You can only use \"utf-8\" as the encoding name when a BOM is present.");
		}
		if (enc == null)
		{
			throw new IOException("unknown encoding type");
		}
		if (!flag2 || stream.Position != stream.Length)
		{
			stream.Seek(position, SeekOrigin.Begin);
		}
		return new SourceCodeReader(new StreamReader(stream, enc), enc);
	}

	internal static Exception ReportEncodingError(Stream stream, string path)
	{
		stream.Seek(0L, SeekOrigin.Begin);
		byte[] array = new byte[1024];
		int num = 0;
		int num2 = 1;
		int num3 = 1;
		int num4 = 0;
		while ((num = stream.Read(array, 0, array.Length)) != -1)
		{
			for (int i = 0; i < num; i++)
			{
				if (array[i] > 127)
				{
					return PythonOps.BadSourceError(array[i], new SourceSpan(new SourceLocation(num4, num2, num3), new SourceLocation(num4, num2, num3)), path);
				}
				if (array[i] == 10)
				{
					num2++;
					num3 = 1;
				}
				else
				{
					num3++;
				}
				num4++;
			}
		}
		return new InvalidOperationException();
	}

	private static string ReadOneLine(StreamReader reader, ref int totalRead)
	{
		Stream baseStream = reader.BaseStream;
		byte[] array = new byte[256];
		StringBuilder stringBuilder = null;
		for (int num = baseStream.Read(array, 0, array.Length); num > 0; num = baseStream.Read(array, 0, array.Length))
		{
			totalRead += num;
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				if (array[i] == 13)
				{
					if (i + 1 < num)
					{
						if (array[i + 1] == 10)
						{
							totalRead -= num - (i + 2);
							baseStream.Seek(i + 2, SeekOrigin.Begin);
							reader.DiscardBufferedData();
							flag = true;
						}
					}
					else
					{
						totalRead -= num - (i + 1);
						baseStream.Seek(i + 1, SeekOrigin.Begin);
						reader.DiscardBufferedData();
						flag = true;
					}
				}
				else if (array[i] == 10)
				{
					totalRead -= num - (i + 1);
					baseStream.Seek(i + 1, SeekOrigin.Begin);
					reader.DiscardBufferedData();
					flag = true;
				}
				if (flag)
				{
					if (stringBuilder != null)
					{
						stringBuilder.Append(array.MakeString(), 0, i);
						return stringBuilder.ToString();
					}
					return array.MakeString().Substring(0, i);
				}
			}
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Append(array.MakeString(), 0, num);
		}
		return stringBuilder?.ToString();
	}

	public override SourceUnit GenerateSourceCode(CodeObject codeDom, string path, SourceCodeKind kind)
	{
		return new PythonCodeDomCodeGen().GenerateCode((CodeMemberMethod)codeDom, this, path, kind);
	}

	public override Scope GetScope(string path)
	{
		return GetModuleByPath(path)?.Scope;
	}

	public PythonModule InitializeModule(string fileName, ModuleContext moduleContext, ScriptCode scriptCode, ModuleOptions options)
	{
		if ((options & ModuleOptions.NoBuiltins) == 0)
		{
			moduleContext.InitializeBuiltins((options & ModuleOptions.ModuleBuiltins) != 0);
		}
		if (fileName != null && Path.GetFileName(fileName) == "__init__.py")
		{
			string directoryName = Path.GetDirectoryName(fileName);
			string fullPath = base.DomainManager.Platform.GetFullPath(directoryName);
			moduleContext.Globals["__path__"] = PythonOps.MakeList(fullPath);
		}
		moduleContext.ShowCls = (options & ModuleOptions.ShowClsMethods) != 0;
		moduleContext.Features = options;
		if ((options & ModuleOptions.Initialize) != ModuleOptions.None)
		{
			scriptCode.Run(moduleContext.GlobalScope);
			if (!moduleContext.Globals.ContainsKey("__package__"))
			{
				moduleContext.Globals["__package__"] = null;
			}
		}
		return moduleContext.Module;
	}

	public override ScopeExtension CreateScopeExtension(Scope scope)
	{
		PythonScopeExtension pythonScopeExtension = new PythonScopeExtension(this, scope);
		pythonScopeExtension.ModuleContext.InitializeBuiltins(moduleBuiltins: false);
		return pythonScopeExtension;
	}

	internal PythonModule CompileModule(string fileName, string moduleName, SourceUnit sourceCode, ModuleOptions options)
	{
		ScriptCode scriptCode;
		return CompileModule(fileName, moduleName, sourceCode, options, out scriptCode);
	}

	internal PythonModule CompileModule(string fileName, string moduleName, SourceUnit sourceCode, ModuleOptions options, out ScriptCode scriptCode)
	{
		ContractUtils.RequiresNotNull(fileName, "fileName");
		ContractUtils.RequiresNotNull(moduleName, "moduleName");
		ContractUtils.RequiresNotNull(sourceCode, "sourceCode");
		scriptCode = GetScriptCode(sourceCode, moduleName, options);
		Scope scope = scriptCode.CreateScope();
		return InitializeModule(fileName, ((PythonScopeExtension)scope.GetExtension(base.ContextId)).ModuleContext, scriptCode, options);
	}

	internal ScriptCode GetScriptCode(SourceUnit sourceCode, string moduleName, ModuleOptions options)
	{
		return GetScriptCode(sourceCode, moduleName, options, null);
	}

	internal ScriptCode GetScriptCode(SourceUnit sourceCode, string moduleName, ModuleOptions options, CompilationMode mode)
	{
		PythonCompilerOptions pythonCompilerOptions = GetPythonCompilerOptions();
		pythonCompilerOptions.SkipFirstLine = (options & ModuleOptions.SkipFirstLine) != 0;
		pythonCompilerOptions.ModuleName = moduleName;
		pythonCompilerOptions.Module = options;
		pythonCompilerOptions.CompilationMode = mode;
		return CompilePythonCode(sourceCode, pythonCompilerOptions, ThrowingErrorSink.Default);
	}

	internal PythonModule GetBuiltinModule(string name)
	{
		lock (this)
		{
			PythonModule pythonModule = CreateBuiltinModule(name);
			if (pythonModule != null)
			{
				PublishModule(name, pythonModule);
				return pythonModule;
			}
			return null;
		}
	}

	internal PythonModule CreateBuiltinModule(string name)
	{
		if (BuiltinModules.TryGetValue(name, out var value))
		{
			RuntimeHelpers.RunClassConstructor(value.TypeHandle);
			return CreateBuiltinModule(name, value);
		}
		return null;
	}

	internal PythonModule CreateBuiltinModule(string moduleName, Type type)
	{
		PythonDictionary pythonDictionary;
		if (type.IsSubclassOf(typeof(BuiltinPythonModule)))
		{
			BuiltinPythonModule builtinPythonModule = (BuiltinPythonModule)Activator.CreateInstance(type, this);
			Dictionary<string, PythonGlobal> dictionary = new Dictionary<string, PythonGlobal>();
			InstancedModuleDictionaryStorage storage = new InstancedModuleDictionaryStorage(builtinPythonModule, dictionary);
			pythonDictionary = new PythonDictionary(storage);
			IEnumerable<string> globalVariableNames = builtinPythonModule.GetGlobalVariableNames();
			CodeContext globalContext = new ModuleContext(pythonDictionary, this).GlobalContext;
			foreach (string item in globalVariableNames)
			{
				dictionary[item] = new PythonGlobal(globalContext, item);
			}
			builtinPythonModule.Initialize(globalContext, dictionary);
		}
		else
		{
			pythonDictionary = new PythonDictionary(new ModuleDictionaryStorage(type));
			if (type == typeof(Builtin))
			{
				Builtin.PerformModuleReload(this, pythonDictionary);
			}
			else if (type != typeof(SysModule))
			{
				MethodInfo method = type.GetMethod("PerformModuleReload");
				if (method != null)
				{
					method.Invoke(null, new object[2] { this, pythonDictionary });
				}
			}
		}
		PythonModule pythonModule = new PythonModule(pythonDictionary);
		pythonModule.__dict__["__name__"] = moduleName;
		pythonModule.__dict__["__package__"] = null;
		return pythonModule;
	}

	public void PublishModule(string name, PythonModule module)
	{
		ContractUtils.RequiresNotNull(name, "name");
		ContractUtils.RequiresNotNull(module, "module");
		SystemStateModules[name] = module;
	}

	internal PythonModule GetReloadableModule(PythonModule module)
	{
		if (!module.__dict__._storage.TryGetName(out var value) || !(value is string))
		{
			throw PythonOps.SystemError("nameless module");
		}
		if (!SystemStateModules.ContainsKey(value))
		{
			throw PythonOps.ImportError("module {0} not in sys.modules", value);
		}
		return module;
	}

	public object GetWarningsModule()
	{
		object result = null;
		try
		{
			if (!_importWarningThrows)
			{
				result = Importer.ImportModule(SharedContext, new PythonDictionary(), "warnings", bottom: false, -1);
			}
		}
		catch
		{
			_importWarningThrows = true;
		}
		return result;
	}

	public void EnsureEncodings()
	{
		if (!_importedEncodings)
		{
			try
			{
				Importer.ImportModule(SharedContext, new PythonDictionary(), "encodings", bottom: false, -1);
			}
			catch (ImportException)
			{
			}
			_importedEncodings = true;
		}
	}

	internal ModuleGlobalCache GetModuleGlobalCache(string name)
	{
		if (!TryGetModuleGlobalCache(name, out var cache))
		{
			return ModuleGlobalCache.NoCache;
		}
		return cache;
	}

	internal Assembly LoadAssemblyFromFile(string file)
	{
		if (TryGetSystemPath(out var path))
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(path);
			while (enumerator.MoveNext())
			{
				if (TryConvertToString(enumerator.Current, out var res))
				{
					string text = Path.Combine(res, file);
					if (TryLoadAssemblyFromFileWithPath(text, out var res2))
					{
						return res2;
					}
					if (TryLoadAssemblyFromFileWithPath(text + ".EXE", out res2))
					{
						return res2;
					}
					if (TryLoadAssemblyFromFileWithPath(text + ".DLL", out res2))
					{
						return res2;
					}
				}
			}
		}
		return null;
	}

	internal bool TryLoadAssemblyFromFileWithPath(string path, out Assembly res)
	{
		if (File.Exists(path) && Path.IsPathRooted(path))
		{
			res = Assembly.LoadFile(path);
			if (res != null)
			{
				_loadedAssemblies.Add(res);
				return true;
			}
		}
		res = null;
		return false;
	}

	internal Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		if (args.RequestingAssembly != null && !_loadedAssemblies.Contains(args.RequestingAssembly))
		{
			return null;
		}
		AssemblyName assemblyName = new AssemblyName(args.Name);
		try
		{
			return LoadAssemblyFromFile(assemblyName.Name);
		}
		catch
		{
			return null;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void HookAssemblyResolve()
	{
		AppDomain.CurrentDomain.AssemblyResolve += _resolveHolder.AssemblyResolveEvent;
	}

	private void UnhookAssemblyResolve()
	{
		try
		{
			AppDomain.CurrentDomain.AssemblyResolve -= _resolveHolder.AssemblyResolveEvent;
		}
		catch (SecurityException)
		{
		}
	}

	public override ICollection<string> GetSearchPaths()
	{
		List<string> list = new List<string>();
		if (TryGetSystemPath(out var path))
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(path);
			while (enumerator.MoveNext())
			{
				if (TryConvertToString(enumerator.Current, out var res))
				{
					list.Add(res);
				}
			}
		}
		return list;
	}

	public override void SetSearchPaths(ICollection<string> paths)
	{
		SetSystemStateValue("path", new List(paths));
	}

	public override void Shutdown()
	{
		UnhookAssemblyResolve();
		if (SystemStateModules.TryGetValue("threading", out var value) && value is PythonModule && ((PythonModule)value).__dict__.TryGetValue("_shutdown", out var value2))
		{
			try
			{
				PythonCalls.Call(SharedContext, value2);
			}
			catch (Exception exception)
			{
				PythonOps.PrintWithDest(SharedContext, SystemStandardError, $"Exception {FormatException(exception)} ignored");
			}
		}
		try
		{
			if (_systemState.__dict__.TryGetValue("exitfunc", out value2))
			{
				PythonCalls.Call(SharedContext, value2);
			}
		}
		finally
		{
			if (PythonOptions.PerfStats)
			{
				PerfTrack.DumpStats();
			}
		}
	}

	public override string FormatException(Exception exception)
	{
		ContractUtils.RequiresNotNull(exception, "exception");
		if (exception is SyntaxErrorException e)
		{
			return FormatPythonSyntaxError(e);
		}
		object pythonException = PythonExceptions.ToPython(exception);
		string text = FormatStackTraces(exception) + FormatPythonException(pythonException);
		if (Options.ShowClrExceptions)
		{
			text += Environment.NewLine;
			text += FormatCLSException(exception);
		}
		return text;
	}

	internal static string FormatPythonSyntaxError(SyntaxErrorException e)
	{
		string sourceLine = GetSourceLine(e);
		if (e.GetData(_syntaxErrorNoCaret) == null)
		{
			return string.Format("  File \"{1}\", line {2}{0}    {3}{0}    {4}^{0}{5}: {6}{0}", Environment.NewLine, e.GetSymbolDocumentName(), (e.Line > 0) ? e.Line.ToString() : "?", sourceLine?.Replace('\t', ' '), new string(' ', (e.Column != 0) ? (e.Column - 1) : 0), GetPythonExceptionClassName(PythonExceptions.ToPython(e)), e.Message);
		}
		return string.Format("  File \"{1}\", line {2}{0}{3}: {4}{0}", Environment.NewLine, e.GetSymbolDocumentName(), new string(' ', (e.Column != 0) ? (e.Column - 1) : 0), GetPythonExceptionClassName(PythonExceptions.ToPython(e)), e.Message);
	}

	internal static string GetSourceLine(SyntaxErrorException e)
	{
		if (e.SourceCode == null)
		{
			return null;
		}
		try
		{
			using StringReader stringReader = new StringReader(e.SourceCode);
			char[] array = new char[80];
			int num = 1;
			StringBuilder stringBuilder = new StringBuilder();
			int num2;
			while ((num2 = stringReader.Read(array, 0, array.Length)) > 0 && num <= e.Line)
			{
				for (int i = 0; i < num2; i++)
				{
					if (num == e.Line)
					{
						stringBuilder.Append(array[i]);
					}
					if (array[i] == '\n')
					{
						num++;
					}
					if (num > e.Line)
					{
						break;
					}
				}
			}
			return stringBuilder.ToString();
		}
		catch (IOException)
		{
			return null;
		}
	}

	private static string FormatCLSException(Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CLR Exception: ");
		while (e != null)
		{
			stringBuilder.Append("    ");
			stringBuilder.AppendLine(e.GetType().Name);
			if (!string.IsNullOrEmpty(e.Message))
			{
				stringBuilder.AppendLine(": ");
				stringBuilder.AppendLine(e.Message);
			}
			else
			{
				stringBuilder.AppendLine();
			}
			e = e.InnerException;
		}
		return stringBuilder.ToString();
	}

	internal static string FormatPythonException(object pythonException)
	{
		string text = "";
		if (pythonException != null)
		{
			if (pythonException is string text2)
			{
				text += text2;
			}
			else
			{
				text += GetPythonExceptionClassName(pythonException);
				string text3 = PythonOps.ToString(pythonException);
				if (!string.IsNullOrEmpty(text3))
				{
					text = text + ": " + text3;
				}
			}
		}
		return text;
	}

	private static string GetPythonExceptionClassName(object pythonException)
	{
		string text = "";
		if (PythonOps.TryGetBoundAttr(pythonException, "__class__", out var ret) && PythonOps.TryGetBoundAttr(ret, "__name__", out ret))
		{
			text = ret.ToString();
			if (PythonOps.TryGetBoundAttr(pythonException, "__module__", out ret))
			{
				string text2 = ret.ToString();
				if (text2 != "exceptions")
				{
					text = text2 + "." + text;
				}
			}
		}
		return text;
	}

	public override IList<DynamicStackFrame> GetStackFrames(Exception exception)
	{
		return PythonOps.GetDynamicStackFrames(exception);
	}

	private string FormatStackTraces(Exception e)
	{
		bool printedHeader = false;
		return FormatStackTraces(e, ref printedHeader);
	}

	private string FormatStackTraces(Exception e, ref bool printedHeader)
	{
		string text = "";
		if (Options.ExceptionDetail)
		{
			if (!printedHeader)
			{
				text = e.Message + Environment.NewLine;
				printedHeader = true;
			}
			IList<StackTrace> exceptionStackTraces = ExceptionHelpers.GetExceptionStackTraces(e);
			if (exceptionStackTraces != null)
			{
				for (int i = 0; i < exceptionStackTraces.Count; i++)
				{
					for (int j = 0; j < exceptionStackTraces[i].FrameCount; j++)
					{
						StackFrame frame = exceptionStackTraces[i].GetFrame(j);
						text = text + frame.ToString() + Environment.NewLine;
					}
				}
			}
			if (e.StackTrace != null)
			{
				text = text + e.StackTrace.ToString() + Environment.NewLine;
			}
			if (e.InnerException != null)
			{
				text += FormatStackTraces(e.InnerException, ref printedHeader);
			}
		}
		else
		{
			text = FormatStackTraceNoDetail(e, ref printedHeader);
		}
		return text;
	}

	internal string FormatStackTraceNoDetail(Exception e, ref bool printedHeader)
	{
		string text = string.Empty;
		if (e.InnerException != null)
		{
			text += FormatStackTraceNoDetail(e.InnerException, ref printedHeader);
		}
		if (!printedHeader)
		{
			text = text + "Traceback (most recent call last):" + Environment.NewLine;
			printedHeader = true;
		}
		DynamicStackFrame[] dynamicStackFrames = PythonExceptions.GetDynamicStackFrames(e);
		for (int num = dynamicStackFrames.Length - 1; num >= 0; num--)
		{
			DynamicStackFrame dynamicStackFrame = dynamicStackFrames[num];
			MethodBase method = dynamicStackFrame.GetMethod();
			if (!CallSiteHelpers.IsInternalFrame(method) && (!(method.DeclaringType != null) || !method.DeclaringType.FullName.StartsWith("IronPython.")))
			{
				text = text + FrameToString(dynamicStackFrame) + Environment.NewLine;
			}
		}
		return text;
	}

	private static string FrameToString(DynamicStackFrame frame)
	{
		string methodName = frame.GetMethodName();
		int fileLineNumber = frame.GetFileLineNumber();
		return string.Format("  File \"{0}\", line {1}, in {2}", frame.GetFileName(), (fileLineNumber == 0) ? "unknown" : fileLineNumber.ToString(), methodName);
	}

	internal static PythonContext GetContext(CodeContext context)
	{
		return context.LanguageContext;
	}

	public override TService GetService<TService>(params object[] args)
	{
		if (typeof(TService) == typeof(TokenizerService))
		{
			return (TService)(object)new Tokenizer(ErrorSink.Null, GetPythonCompilerOptions(), verbatim: true);
		}
		if (typeof(TService) == typeof(PythonService))
		{
			return (TService)(object)GetPythonService((ScriptEngine)args[0]);
		}
		if (typeof(TService) == typeof(DocumentationProvider))
		{
			return (TService)(object)new PythonDocumentationProvider(this);
		}
		return base.GetService<TService>(args);
	}

	internal PythonService GetPythonService(ScriptEngine engine)
	{
		if (_pythonService == null)
		{
			Interlocked.CompareExchange(ref _pythonService, new PythonService(this, engine), null);
		}
		return _pythonService;
	}

	internal static PythonOptions GetPythonOptions(CodeContext context)
	{
		return DefaultContext.DefaultPythonContext._options;
	}

	internal void InsertIntoPath(int index, string directory)
	{
		if (TryGetSystemPath(out var path))
		{
			path.insert(index, directory);
		}
	}

	internal void AddToPath(string directory)
	{
		if (TryGetSystemPath(out var path))
		{
			path.append(directory);
		}
	}

	internal PythonCompilerOptions GetPythonCompilerOptions()
	{
		ModuleOptions moduleOptions = ModuleOptions.None;
		if (PythonOptions.DivisionOptions == PythonDivisionOptions.New)
		{
			moduleOptions |= ModuleOptions.TrueDivision;
		}
		return new PythonCompilerOptions(moduleOptions);
	}

	public override CompilerOptions GetCompilerOptions()
	{
		return GetPythonCompilerOptions();
	}

	public override CompilerOptions GetCompilerOptions(Scope scope)
	{
		PythonCompilerOptions pythonCompilerOptions = GetPythonCompilerOptions();
		PythonScopeExtension pythonScopeExtension = (PythonScopeExtension)scope.GetExtension(base.ContextId);
		if (pythonScopeExtension != null)
		{
			pythonCompilerOptions.Module |= pythonScopeExtension.ModuleContext.Features;
		}
		return pythonCompilerOptions;
	}

	public override void GetExceptionMessage(Exception exception, out string message, out string typeName)
	{
		object pythonException = PythonExceptions.ToPython(exception);
		message = FormatPythonException(PythonExceptions.ToPython(exception));
		typeName = GetPythonExceptionClassName(pythonException);
	}

	public string GetDefaultEncodingName()
	{
		return DefaultEncoding.WebName.ToLower().Replace('-', '_');
	}

	private void InitializeBuiltins()
	{
		BuiltinsDictionaryStorage storage = new BuiltinsDictionaryStorage(BuiltinsChanged);
		PythonDictionary pythonDictionary = new PythonDictionary(storage);
		Builtin.PerformModuleReload(this, pythonDictionary);
		_builtinDict = pythonDictionary;
		_builtins = new PythonModule(pythonDictionary);
		_modulesDict["__builtin__"] = _builtins;
	}

	private Dictionary<string, Type> CreateBuiltinTable()
	{
		Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
		LoadBuiltins(dictionary, typeof(PythonContext).Assembly, updateSys: false);
		Assembly assembly = null;
		try
		{
			assembly = base.DomainManager.Platform.LoadAssembly(GetIronPythonAssembly("IronPython.Modules"));
		}
		catch (FileNotFoundException)
		{
		}
		if (assembly != null)
		{
			LoadBuiltins(dictionary, assembly, updateSys: false);
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				dictionary["posix"] = dictionary["nt"];
				dictionary.Remove("nt");
			}
		}
		return dictionary;
	}

	internal void LoadBuiltins(Dictionary<string, Type> builtinTable, Assembly assem, bool updateSys)
	{
		object[] customAttributes = assem.GetCustomAttributes(typeof(PythonModuleAttribute), inherit: false);
		if (customAttributes.Length > 0)
		{
			object[] array = customAttributes;
			for (int i = 0; i < array.Length; i++)
			{
				PythonModuleAttribute pythonModuleAttribute = (PythonModuleAttribute)array[i];
				builtinTable[pythonModuleAttribute.Name] = pythonModuleAttribute.Type;
				BuiltinModuleNames[pythonModuleAttribute.Type] = pythonModuleAttribute.Name;
			}
			if (updateSys)
			{
				SysModule.PublishBuiltinModuleNames(this, _systemState.__dict__);
			}
		}
	}

	public static string GetIronPythonAssembly(string baseName)
	{
		ContractUtils.RequiresNotNull(baseName, "baseName");
		string fullName = typeof(PythonContext).Assembly.FullName;
		int num = fullName.IndexOf(',');
		if (num <= 0)
		{
			return baseName;
		}
		return baseName + fullName.Substring(num);
	}

	private void BuiltinsChanged(object sender, ModuleChangeEventArgs e)
	{
		lock (_builtinCache)
		{
			if (_builtinCache.TryGetValue(e.Name, out var value))
			{
				switch (e.ChangeType)
				{
				case ModuleChangeType.Delete:
					value.Value = Uninitialized.Instance;
					break;
				case ModuleChangeType.Set:
					value.Value = e.Value;
					break;
				}
			}
			else
			{
				object value2 = ((e.ChangeType == ModuleChangeType.Set) ? e.Value : Uninitialized.Instance);
				_builtinCache[e.Name] = new ModuleGlobalCache(value2);
			}
		}
	}

	internal bool TryGetModuleGlobalCache(string name, out ModuleGlobalCache cache)
	{
		lock (_builtinCache)
		{
			if (!_builtinCache.TryGetValue(name, out cache) && BuiltinModuleInstance.__dict__.TryGetValue(name, out var value))
			{
				_builtinCache[name] = (cache = new ModuleGlobalCache(value));
			}
		}
		return cache != null;
	}

	internal void SetHostVariables(string prefix, string executable, string versionString)
	{
		_initialVersionString = ((!string.IsNullOrEmpty(versionString)) ? versionString : GetVersionString());
		_initialExecutable = executable ?? "";
		_initialPrefix = prefix;
		AddToPath(prefix);
		SetHostVariables(SystemState.__dict__);
	}

	internal void SetHostVariables(PythonDictionary dict)
	{
		dict["executable"] = _initialExecutable;
		dict["prefix"] = _initialPrefix;
		dict["exec_prefix"] = _initialPrefix;
		SetVersionVariables(dict);
	}

	private void SetVersionVariables(PythonDictionary dict)
	{
		Implementation implementation = (Implementation)(dict["implementation"] = new Implementation());
		dict["version_info"] = implementation.version;
		dict["hexversion"] = implementation.hexversion;
		dict["version"] = implementation.version.GetVersionString(_initialVersionString);
	}

	internal static string GetVersionString()
	{
		string text = "";
		string text2 = ((Type.GetType("Mono.Runtime") == null) ? ".NET" : "Mono");
		string text3 = (IntPtr.Size * 8).ToString();
		return string.Format("{0}{3} ({1}) on {4} {2} ({5}-bit)", "IronPython 2.7.3", GetPythonVersion().ToString(), Environment.Version, text, text2, text3);
	}

	private static string GetInitialPrefix()
	{
		try
		{
			return typeof(PythonContext).Assembly.CodeBase;
		}
		catch (SecurityException)
		{
			return string.Empty;
		}
		catch (MethodAccessException)
		{
			return string.Empty;
		}
	}

	public override IList<string> GetMemberNames(object obj)
	{
		List<string> list = new List<string>();
		foreach (object attrName in PythonOps.GetAttrNames(SharedContext, obj))
		{
			if (attrName is string)
			{
				list.Add((string)attrName);
			}
		}
		return list;
	}

	public override string FormatObject(DynamicOperations operations, object obj)
	{
		return PythonOps.Repr(_defaultContext, obj) ?? "None";
	}

	internal object GetSystemStateValue(string name)
	{
		if (SystemState.__dict__.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	internal void SetSystemStateValue(string name, object value)
	{
		SystemState.__dict__[name] = value;
	}

	internal void DelSystemStateValue(string name)
	{
		SystemState.__dict__.Remove(name);
	}

	private void SetStandardIO()
	{
		SharedIO sharedIO = base.DomainManager.SharedIO;
		PythonFile value = PythonFile.CreateConsole(this, sharedIO, ConsoleStreamType.Input, "<stdin>");
		PythonFile value2 = PythonFile.CreateConsole(this, sharedIO, ConsoleStreamType.Output, "<stdout>");
		PythonFile value3 = PythonFile.CreateConsole(this, sharedIO, ConsoleStreamType.ErrorOutput, "<stderr>");
		SetSystemStateValue("__stdin__", value);
		SetSystemStateValue("stdin", value);
		SetSystemStateValue("__stdout__", value2);
		SetSystemStateValue("stdout", value2);
		SetSystemStateValue("__stderr__", value3);
		SetSystemStateValue("stderr", value3);
	}

	public override int ExecuteProgram(SourceUnit program)
	{
		try
		{
			PythonCompilerOptions pythonCompilerOptions = (PythonCompilerOptions)GetCompilerOptions();
			pythonCompilerOptions.ModuleName = "__main__";
			pythonCompilerOptions.Module |= ModuleOptions.Initialize;
			program.Execute(pythonCompilerOptions, ErrorSink.Default);
		}
		catch (SystemExitException ex)
		{
			object otherCode;
			return ex.GetExitCode(out otherCode);
		}
		return 0;
	}

	internal SiteLocalStorage<T> GetGenericSiteStorage<T>()
	{
		if (_genericSiteStorage == null)
		{
			Interlocked.CompareExchange(ref _genericSiteStorage, new Dictionary<Type, object>(), null);
		}
		lock (_genericSiteStorage)
		{
			if (!_genericSiteStorage.TryGetValue(typeof(T), out var value))
			{
				value = (_genericSiteStorage[typeof(T)] = new SiteLocalStorage<T>());
			}
			return (SiteLocalStorage<T>)value;
		}
	}

	internal SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> GetGenericCallSiteStorage()
	{
		return GetGenericSiteStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>>();
	}

	internal SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object>>> GetGenericCallSiteStorage0()
	{
		return GetGenericSiteStorage<CallSite<Func<CallSite, CodeContext, object, object>>>();
	}

	internal SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>>> GetGenericKeywordCallSiteStorage()
	{
		return GetGenericSiteStorage<CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>>>();
	}

	public override ConvertBinder CreateConvertBinder(Type toType, bool? explicitCast)
	{
		if (explicitCast.HasValue)
		{
			return Convert(toType, explicitCast.Value ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast).CompatBinder;
		}
		return Convert(toType, ConversionResultKind.ImplicitCast).CompatBinder;
	}

	public override DeleteMemberBinder CreateDeleteMemberBinder(string name, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return new PythonDeleteMemberBinder(this, name, ignoreCase);
		}
		return DeleteMember(name);
	}

	public override GetMemberBinder CreateGetMemberBinder(string name, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return new CompatibilityGetMember(this, name, isNoThrow: false);
		}
		return CompatGetMember(name, isNoThrow: false);
	}

	public override InvokeBinder CreateInvokeBinder(CallInfo callInfo)
	{
		return CompatInvoke(callInfo);
	}

	public override BinaryOperationBinder CreateBinaryOperationBinder(ExpressionType operation)
	{
		return BinaryOperation(operation);
	}

	public override UnaryOperationBinder CreateUnaryOperationBinder(ExpressionType operation)
	{
		return UnaryOperation(operation);
	}

	public override SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return new PythonSetMemberBinder(this, name, ignoreCase);
		}
		return SetMember(name);
	}

	public override CreateInstanceBinder CreateCreateBinder(CallInfo callInfo)
	{
		return Create(CompatInvoke(callInfo), callInfo);
	}

	private bool InvokeOperatorWorker(CodeContext context, UnaryOperators oper, object target, out object result)
	{
		if (_newUnarySites == null)
		{
			Interlocked.CompareExchange(ref _newUnarySites, new CallSite<Func<CallSite, CodeContext, object, object>>[4], null);
		}
		if (_newUnarySites[(int)oper] == null)
		{
			Interlocked.CompareExchange(ref _newUnarySites[(int)oper], CallSite<Func<CallSite, CodeContext, object, object>>.Create(InvokeNone), null);
		}
		CallSite<Func<CallSite, CodeContext, object, object>> callSite = _newUnarySites[(int)oper];
		string unarySymbol = GetUnarySymbol(oper);
		PythonType pythonType = DynamicHelpers.GetPythonType(target);
		if (pythonType.TryResolveMixedSlot(context, unarySymbol, out var slot) && slot.TryGetValue(context, target, pythonType, out var value))
		{
			result = callSite.Target(callSite, context, value);
			return true;
		}
		result = null;
		return false;
	}

	private static string GetUnarySymbol(UnaryOperators oper)
	{
		return oper switch
		{
			UnaryOperators.Repr => "__repr__", 
			UnaryOperators.Length => "__len__", 
			UnaryOperators.Hash => "__hash__", 
			UnaryOperators.String => "__str__", 
			_ => throw new ValueErrorException("unknown unary symbol"), 
		};
	}

	private bool InvokeOperatorWorker(CodeContext context, TernaryOperators oper, object target, object value1, object value2, out object result)
	{
		if (_newTernarySites == null)
		{
			Interlocked.CompareExchange(ref _newTernarySites, new CallSite<Func<CallSite, CodeContext, object, object, object, object>>[2], null);
		}
		if (_newTernarySites[(int)oper] == null)
		{
			Interlocked.CompareExchange(ref _newTernarySites[(int)oper], CallSite<Func<CallSite, CodeContext, object, object, object, object>>.Create(Invoke(new CallSignature(2))), null);
		}
		CallSite<Func<CallSite, CodeContext, object, object, object, object>> callSite = _newTernarySites[(int)oper];
		string ternarySymbol = GetTernarySymbol(oper);
		PythonType pythonType = DynamicHelpers.GetPythonType(target);
		if (pythonType.TryResolveMixedSlot(context, ternarySymbol, out var slot) && slot.TryGetValue(context, target, pythonType, out var value3))
		{
			result = callSite.Target(callSite, context, value3, value1, value2);
			return true;
		}
		result = null;
		return false;
	}

	private static string GetTernarySymbol(TernaryOperators oper)
	{
		return oper switch
		{
			TernaryOperators.SetDescriptor => "__set__", 
			TernaryOperators.GetDescriptor => "__get__", 
			_ => throw new ValueErrorException("unknown ternary operator"), 
		};
	}

	internal static object InvokeUnaryOperator(CodeContext context, UnaryOperators oper, object target, string errorMsg)
	{
		if (GetContext(context).InvokeOperatorWorker(context, oper, target, out var result))
		{
			return result;
		}
		throw PythonOps.TypeError(errorMsg);
	}

	internal static object InvokeUnaryOperator(CodeContext context, UnaryOperators oper, object target)
	{
		if (GetContext(context).InvokeOperatorWorker(context, oper, target, out var result))
		{
			return result;
		}
		throw PythonOps.TypeError(string.Empty);
	}

	internal static bool TryInvokeTernaryOperator(CodeContext context, TernaryOperators oper, object target, object value1, object value2, out object res)
	{
		return GetContext(context).InvokeOperatorWorker(context, oper, target, value1, value2, out res);
	}

	internal CallSite<Func<CallSite, object, object, int>> MakeSortCompareSite()
	{
		return CallSite<Func<CallSite, object, object, int>>.Create(Operation(PythonOperationKind.Compare));
	}

	internal void SetAttr(CodeContext context, object o, string name, object value)
	{
		if (_setAttrSites == null)
		{
			Interlocked.CompareExchange(ref _setAttrSites, new Dictionary<AttrKey, CallSite<Func<CallSite, object, object, object>>>(), null);
		}
		CallSite<Func<CallSite, object, object, object>> value2;
		lock (_setAttrSites)
		{
			AttrKey key = new AttrKey(CompilerHelpers.GetType(o), name);
			if (!_setAttrSites.TryGetValue(key, out value2))
			{
				value2 = (_setAttrSites[key] = CallSite<Func<CallSite, object, object, object>>.Create(SetMember(name)));
			}
		}
		value2.Target(value2, o, value);
	}

	internal void DeleteAttr(CodeContext context, object o, string name)
	{
		AttrKey key = new AttrKey(CompilerHelpers.GetType(o), name);
		if (_deleteAttrSites == null)
		{
			Interlocked.CompareExchange(ref _deleteAttrSites, new Dictionary<AttrKey, CallSite<Action<CallSite, object>>>(), null);
		}
		CallSite<Action<CallSite, object>> value;
		lock (_deleteAttrSites)
		{
			if (!_deleteAttrSites.TryGetValue(key, out value))
			{
				value = (_deleteAttrSites[key] = CallSite<Action<CallSite, object>>.Create(DeleteMember(name)));
			}
		}
		value.Target(value, o);
	}

	internal void DelIndex(object target, object index)
	{
		if (_delIndexSite == null)
		{
			Interlocked.CompareExchange(ref _delIndexSite, CallSite<Action<CallSite, object, object>>.Create(DeleteIndex(1)), null);
		}
		_delIndexSite.Target(_delIndexSite, target, index);
	}

	internal void DelSlice(object target, object start, object end)
	{
		if (_delSliceSite == null)
		{
			Interlocked.CompareExchange(ref _delSliceSite, CallSite<Func<CallSite, object, object, object, object>>.Create(DeleteSlice), null);
		}
		_delSliceSite.Target(_delSliceSite, target, start, end);
	}

	internal void SetIndex(object a, object b, object c)
	{
		if (_setIndexSite == null)
		{
			Interlocked.CompareExchange(ref _setIndexSite, CallSite<Func<CallSite, object, object, object, object>>.Create(SetIndex(1)), null);
		}
		_setIndexSite.Target(_setIndexSite, a, b, c);
	}

	internal void SetSlice(object a, object start, object end, object value)
	{
		if (_setSliceSite == null)
		{
			Interlocked.CompareExchange(ref _setSliceSite, CallSite<Func<CallSite, object, object, object, object, object>>.Create(SetSliceBinder), null);
		}
		_setSliceSite.Target(_setSliceSite, a, start, end, value);
	}

	public override string GetDocumentation(object obj)
	{
		if (_docSite == null)
		{
			_docSite = CallSite<Func<CallSite, object, string>>.Create(Operation(PythonOperationKind.Documentation));
		}
		return _docSite.Target(_docSite, obj);
	}

	internal PythonSiteCache GetSiteCacheForSystemType(Type type)
	{
		if (_systemSiteCache == null)
		{
			Interlocked.CompareExchange(ref _systemSiteCache, new Dictionary<Type, PythonSiteCache>(), null);
		}
		lock (_systemSiteCache)
		{
			if (!_systemSiteCache.TryGetValue(type, out var value))
			{
				value = (_systemSiteCache[type] = new PythonSiteCache());
			}
			return value;
		}
	}

	internal int ConvertToInt32(object value)
	{
		if (_intSite == null)
		{
			Interlocked.CompareExchange(ref _intSite, MakeExplicitConvertSite<int>(), null);
		}
		return _intSite.Target(_intSite, value);
	}

	internal bool TryConvertToString(object str, out string res)
	{
		if (_tryStringSite == null)
		{
			Interlocked.CompareExchange(ref _tryStringSite, MakeExplicitTrySite<string>(), null);
		}
		res = _tryStringSite.Target(_tryStringSite, str);
		return res != null;
	}

	internal bool TryConvertToInt32(object val, out int res)
	{
		if (_tryIntSite == null)
		{
			Interlocked.CompareExchange(ref _tryIntSite, MakeExplicitStructTrySite<int>(), null);
		}
		object obj = _tryIntSite.Target(_tryIntSite, val);
		if (obj != null)
		{
			res = (int)obj;
			return true;
		}
		res = 0;
		return false;
	}

	internal bool TryConvertToIEnumerable(object enumerable, out IEnumerable res)
	{
		if (_tryIEnumerableSite == null)
		{
			Interlocked.CompareExchange(ref _tryIEnumerableSite, MakeExplicitTrySite<IEnumerable>(), null);
		}
		res = _tryIEnumerableSite.Target(_tryIEnumerableSite, enumerable);
		return res != null;
	}

	private CallSite<Func<CallSite, object, T>> MakeExplicitTrySite<T>() where T : class
	{
		return MakeTrySite<T, T>(ConversionResultKind.ExplicitTry);
	}

	private CallSite<Func<CallSite, object, object>> MakeExplicitStructTrySite<T>() where T : struct
	{
		return MakeTrySite<T, object>(ConversionResultKind.ExplicitTry);
	}

	private CallSite<Func<CallSite, object, TRet>> MakeTrySite<T, TRet>(ConversionResultKind kind)
	{
		return CallSite<Func<CallSite, object, TRet>>.Create(Convert(typeof(T), kind));
	}

	internal object ImplicitConvertTo<T>(object value)
	{
		if (_implicitConvertSites == null)
		{
			Interlocked.CompareExchange(ref _implicitConvertSites, new Dictionary<Type, CallSite<Func<CallSite, object, object>>>(), null);
		}
		CallSite<Func<CallSite, object, object>> value2;
		lock (_implicitConvertSites)
		{
			if (!_implicitConvertSites.TryGetValue(typeof(T), out value2))
			{
				value2 = (_implicitConvertSites[typeof(T)] = MakeImplicitConvertSite<T>());
			}
		}
		return value2.Target(value2, value);
	}

	private CallSite<Func<CallSite, object, T>> MakeExplicitConvertSite<T>()
	{
		return MakeConvertSite<T>(ConversionResultKind.ExplicitCast);
	}

	private CallSite<Func<CallSite, object, object>> MakeImplicitConvertSite<T>()
	{
		return CallSite<Func<CallSite, object, object>>.Create(ConvertRetObject(typeof(T), ConversionResultKind.ImplicitCast));
	}

	private CallSite<Func<CallSite, object, T>> MakeConvertSite<T>(ConversionResultKind kind)
	{
		return CallSite<Func<CallSite, object, T>>.Create(Convert(typeof(T), kind));
	}

	internal object Operation(PythonOperationKind operation, object self, object other)
	{
		if (_binarySites == null)
		{
			Interlocked.CompareExchange(ref _binarySites, new Dictionary<PythonOperationKind, CallSite<Func<CallSite, object, object, object>>>(), null);
		}
		CallSite<Func<CallSite, object, object, object>> value;
		lock (_binarySites)
		{
			if (!_binarySites.TryGetValue(operation, out value))
			{
				value = (_binarySites[operation] = CallSite<Func<CallSite, object, object, object>>.Create(Binders.BinaryOperationBinder(this, operation)));
			}
		}
		return value.Target(value, self, other);
	}

	internal bool GreaterThan(object self, object other)
	{
		return Comparison(self, other, ExpressionType.GreaterThan, ref _greaterThanSite);
	}

	internal bool LessThan(object self, object other)
	{
		return Comparison(self, other, ExpressionType.LessThan, ref _lessThanSite);
	}

	internal bool GreaterThanOrEqual(object self, object other)
	{
		return Comparison(self, other, ExpressionType.GreaterThanOrEqual, ref _greaterThanEqualSite);
	}

	internal bool LessThanOrEqual(object self, object other)
	{
		return Comparison(self, other, ExpressionType.LessThanOrEqual, ref _lessThanEqualSite);
	}

	internal bool Contains(object self, object other)
	{
		return Comparison(self, other, PythonOperationKind.Contains, ref _containsSite);
	}

	internal static bool Equal(object self, object other)
	{
		return DynamicHelpers.GetPythonType(self).EqualRetBool(self, other);
	}

	internal static bool NotEqual(object self, object other)
	{
		return !Equal(self, other);
	}

	private bool Comparison(object self, object other, ExpressionType operation, ref CallSite<Func<CallSite, object, object, bool>> comparisonSite)
	{
		if (comparisonSite == null)
		{
			Interlocked.CompareExchange(ref comparisonSite, CreateComparisonSite(operation), null);
		}
		return comparisonSite.Target(comparisonSite, self, other);
	}

	internal CallSite<Func<CallSite, object, object, bool>> CreateComparisonSite(ExpressionType op)
	{
		return CallSite<Func<CallSite, object, object, bool>>.Create(BinaryOperationRetType(BinaryOperation(op), Convert(typeof(bool), ConversionResultKind.ExplicitCast)));
	}

	private bool Comparison(object self, object other, PythonOperationKind operation, ref CallSite<Func<CallSite, object, object, bool>> comparisonSite)
	{
		if (comparisonSite == null)
		{
			Interlocked.CompareExchange(ref comparisonSite, CreateComparisonSite(operation), null);
		}
		return comparisonSite.Target(comparisonSite, self, other);
	}

	internal CallSite<Func<CallSite, object, object, bool>> CreateComparisonSite(PythonOperationKind op)
	{
		return CallSite<Func<CallSite, object, object, bool>>.Create(OperationRetType(Operation(op), Convert(typeof(bool), ConversionResultKind.ExplicitCast)));
	}

	internal object CallSplat(object func, params object[] args)
	{
		EnsureCallSplatSite();
		return _callSplatSite.Target(_callSplatSite, SharedContext, func, args);
	}

	internal object CallWithContext(CodeContext context, object func, params object[] args)
	{
		EnsureCallSplatSite();
		return _callSplatSite.Target(_callSplatSite, context, func, args);
	}

	internal object Call(CodeContext context, object func)
	{
		EnsureCall0Site();
		return _callSite0.Target(_callSite0, context, func);
	}

	private void EnsureCall0SiteLightEh()
	{
		if (_callSite0LightEh == null)
		{
			Interlocked.CompareExchange(ref _callSite0LightEh, CallSite<Func<CallSite, CodeContext, object, object>>.Create(Invoke(new CallSignature(0)).GetLightExceptionBinder()), null);
		}
	}

	internal object CallLightEh(CodeContext context, object func)
	{
		EnsureCall0SiteLightEh();
		return _callSite0LightEh.Target(_callSite0LightEh, context, func);
	}

	internal object Call(CodeContext context, object func, object arg0)
	{
		EnsureCall1Site();
		return _callSite1.Target(_callSite1, context, func, arg0);
	}

	internal object Call(CodeContext context, object func, object arg0, object arg1)
	{
		EnsureCall2Site();
		return _callSite2.Target(_callSite2, context, func, arg0, arg1);
	}

	private void EnsureCallSplatSite()
	{
		if (_callSplatSite == null)
		{
			Interlocked.CompareExchange(ref _callSplatSite, MakeSplatSite(), null);
		}
	}

	internal CallSite<Func<CallSite, CodeContext, object, object[], object>> MakeSplatSite()
	{
		return CallSite<Func<CallSite, CodeContext, object, object[], object>>.Create(Binders.InvokeSplat(this));
	}

	internal object CallWithKeywords(object func, object[] args, IDictionary<object, object> dict)
	{
		if (_callDictSite == null)
		{
			Interlocked.CompareExchange(ref _callDictSite, MakeKeywordSplatSite(), null);
		}
		return _callDictSite.Target(_callDictSite, SharedContext, func, args, dict);
	}

	internal CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>> MakeKeywordSplatSite()
	{
		return CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>>.Create(Binders.InvokeKeywords(this));
	}

	internal object CallWithKeywords(object func, object args, object dict)
	{
		if (_callDictSiteLooselyTyped == null)
		{
			Interlocked.CompareExchange(ref _callDictSiteLooselyTyped, MakeKeywordSplatSiteLooselyTyped(), null);
		}
		return _callDictSiteLooselyTyped.Target(_callDictSiteLooselyTyped, SharedContext, func, args, dict);
	}

	internal CallSite<Func<CallSite, CodeContext, object, object, object, object>> MakeKeywordSplatSiteLooselyTyped()
	{
		return CallSite<Func<CallSite, CodeContext, object, object, object, object>>.Create(Binders.InvokeKeywords(this));
	}

	public override bool IsCallable(object obj)
	{
		if (_isCallableSite == null)
		{
			Interlocked.CompareExchange(ref _isCallableSite, CallSite<Func<CallSite, object, bool>>.Create(Operation(PythonOperationKind.IsCallable)), null);
		}
		return _isCallableSite.Target(_isCallableSite, obj);
	}

	internal static int Hash(object o)
	{
		if (o != null)
		{
			switch (o.GetType().GetTypeCode())
			{
			case TypeCode.Int32:
				return Int32Ops.__hash__((int)o);
			case TypeCode.String:
				return ((string)o).GetHashCode();
			case TypeCode.Double:
				return DoubleOps.__hash__((double)o);
			case TypeCode.Int16:
				return Int16Ops.__hash__((short)o);
			case TypeCode.Int64:
				return Int64Ops.__hash__((long)o);
			case TypeCode.SByte:
				return SByteOps.__hash__((sbyte)o);
			case TypeCode.Single:
				return SingleOps.__hash__((float)o);
			case TypeCode.UInt16:
				return UInt16Ops.__hash__((ushort)o);
			case TypeCode.UInt32:
				return UInt32Ops.__hash__((uint)o);
			case TypeCode.UInt64:
				return UInt64Ops.__hash__((ulong)o);
			case TypeCode.Decimal:
				return DecimalOps.__hash__((decimal)o);
			case TypeCode.DateTime:
				return ((DateTime)o/*cast due to .constrained prefix*/).GetHashCode();
			case TypeCode.Boolean:
				return ((bool)o).GetHashCode();
			case TypeCode.Byte:
				return ByteOps.__hash__((byte)o);
			}
		}
		return DynamicHelpers.GetPythonType(o).Hash(o);
	}

	internal object Add(object x, object y)
	{
		CallSite<Func<CallSite, object, object, object>> callSite = EnsureAddSite();
		return callSite.Target(callSite, x, y);
	}

	internal CallSite<Func<CallSite, object, object, object>> EnsureAddSite()
	{
		if (_addSite == null)
		{
			Interlocked.CompareExchange(ref _addSite, CallSite<Func<CallSite, object, object, object>>.Create(BinaryOperation(ExpressionType.Add)), null);
		}
		return _addSite;
	}

	internal object DivMod(object x, object y)
	{
		if (_divModSite == null)
		{
			Interlocked.CompareExchange(ref _divModSite, CallSite<Func<CallSite, object, object, object>>.Create(Operation(PythonOperationKind.DivMod)), null);
		}
		object obj = _divModSite.Target(_divModSite, x, y);
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (_rdivModSite == null)
		{
			Interlocked.CompareExchange(ref _rdivModSite, CallSite<Func<CallSite, object, object, object>>.Create(Operation(PythonOperationKind.ReverseDivMod)), null);
		}
		obj = _rdivModSite.Target(_rdivModSite, x, y);
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		throw PythonOps.TypeErrorForBinaryOp("divmod", x, y);
	}

	internal CompiledLoader GetCompiledLoader()
	{
		if (_compiledLoader == null && Interlocked.CompareExchange(ref _compiledLoader, new CompiledLoader(), null) == null)
		{
			List list;
			if (!SystemState.__dict__.TryGetValue("meta_path", out var value) || (list = value as List) == null)
			{
				list = (List)(SystemState.__dict__["meta_path"] = new List());
			}
			list.append(_compiledLoader);
		}
		return _compiledLoader;
	}

	private static CultureInfo MakeCCulture()
	{
		CultureInfo cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
		NumberFormatInfo numberFormat = cultureInfo.NumberFormat;
		int[] numberGroupSizes = new int[1];
		numberFormat.NumberGroupSizes = numberGroupSizes;
		NumberFormatInfo numberFormat2 = cultureInfo.NumberFormat;
		int[] currencyGroupSizes = new int[1];
		numberFormat2.CurrencyGroupSizes = currencyGroupSizes;
		return cultureInfo;
	}

	public Action<Action> GetSetCommandDispatcher(Action<Action> newDispatcher)
	{
		return Interlocked.Exchange(ref _commandDispatcher, newDispatcher);
	}

	public Action<Action> GetCommandDispatcher()
	{
		return _commandDispatcher;
	}

	public void DispatchCommand(Action command)
	{
		Action<Action> commandDispatcher = _commandDispatcher;
		if (commandDispatcher != null)
		{
			commandDispatcher(command);
		}
		else
		{
			command?.Invoke();
		}
	}

	private static CallSite<Func<CallSite, CodeContext, T, object, object, int>> MakeCompareSite<T>(PythonContext context)
	{
		return CallSite<Func<CallSite, CodeContext, T, object, object, int>>.Create(context.InvokeTwoConvertToInt);
	}

	internal IComparer GetComparer(object cmp, Type type)
	{
		if (type == null)
		{
			if (cmp == null)
			{
				return new DefaultPythonComparer(this);
			}
			if (cmp is PythonFunction)
			{
				return new FunctionComparer<PythonFunction>(this, (PythonFunction)cmp);
			}
			if (cmp is BuiltinFunction)
			{
				return new FunctionComparer<BuiltinFunction>(this, (BuiltinFunction)cmp);
			}
			return new FunctionComparer<object>(this, cmp);
		}
		if (cmp == null)
		{
			if (_defaultComparer == null)
			{
				Interlocked.CompareExchange(ref _defaultComparer, new Dictionary<Type, DefaultPythonComparer>(), null);
			}
			lock (_defaultComparer)
			{
				if (!_defaultComparer.TryGetValue(type, out var value))
				{
					value = (_defaultComparer[type] = new DefaultPythonComparer(this));
				}
				return value;
			}
		}
		if (cmp is PythonFunction)
		{
			if (_sharedPythonFunctionCompareSite == null)
			{
				_sharedPythonFunctionCompareSite = MakeCompareSite<PythonFunction>(this);
			}
			return new FunctionComparer<PythonFunction>(this, (PythonFunction)cmp, _sharedPythonFunctionCompareSite);
		}
		if (cmp is BuiltinFunction)
		{
			if (_sharedBuiltinFunctionCompareSite == null)
			{
				_sharedBuiltinFunctionCompareSite = MakeCompareSite<BuiltinFunction>(this);
			}
			return new FunctionComparer<BuiltinFunction>(this, (BuiltinFunction)cmp, _sharedBuiltinFunctionCompareSite);
		}
		if (_sharedFunctionCompareSite == null)
		{
			_sharedFunctionCompareSite = MakeCompareSite<object>(this);
		}
		return new FunctionComparer<object>(this, cmp, _sharedFunctionCompareSite);
	}

	internal CallSite<Func<CallSite, object, object, bool>> GetEqualSite(Type type)
	{
		if (_equalSites == null)
		{
			Interlocked.CompareExchange(ref _equalSites, new Dictionary<Type, CallSite<Func<CallSite, object, object, bool>>>(), null);
		}
		CallSite<Func<CallSite, object, object, bool>> value;
		lock (_equalSites)
		{
			if (!_equalSites.TryGetValue(type, out value))
			{
				value = (_equalSites[type] = MakeEqualSite());
			}
		}
		return value;
	}

	internal CallSite<Func<CallSite, object, object, bool>> MakeEqualSite()
	{
		return CreateComparisonSite(ExpressionType.Equal);
	}

	internal static CallSite<Func<CallSite, object, int>> GetHashSite(PythonType type)
	{
		return type.HashSite;
	}

	internal CallSite<Func<CallSite, object, int>> MakeHashSite()
	{
		return CallSite<Func<CallSite, object, int>>.Create(Operation(PythonOperationKind.Hash));
	}

	public override IList<string> GetCallSignatures(object obj)
	{
		if (_getSignaturesSite == null)
		{
			Interlocked.CompareExchange(ref _getSignaturesSite, CallSite<Func<CallSite, object, IList<string>>>.Create(Operation(PythonOperationKind.CallSignatures)), null);
		}
		return _getSignaturesSite.Target(_getSignaturesSite, obj);
	}

	internal int Collect(int generation)
	{
		if (generation > GC.MaxGeneration || generation < 0)
		{
			throw PythonOps.ValueError("invalid generation {0}", generation);
		}
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		for (int i = 0; i < 2; i++)
		{
			GC.Collect(generation);
			GC.WaitForPendingFinalizers();
			if (generation == GC.MaxGeneration)
			{
				FunctionCode.CleanFunctionCodes(this, synchronous: true);
			}
		}
		return (int)Math.Max(totalMemory - GC.GetTotalMemory(forceFullCollection: false), 0L);
	}

	internal CompatibilityInvokeBinder CompatInvoke(CallInfo callInfo)
	{
		if (_compatInvokeBinders == null)
		{
			Interlocked.CompareExchange(ref _compatInvokeBinders, new Dictionary<CallSignature, CompatibilityInvokeBinder>(), null);
		}
		lock (_compatInvokeBinders)
		{
			CallSignature key = BindingHelpers.CallInfoToSignature(callInfo);
			if (!_compatInvokeBinders.TryGetValue(key, out var value))
			{
				value = (_compatInvokeBinders[key] = new CompatibilityInvokeBinder(this, callInfo));
			}
			return value;
		}
	}

	internal PythonConversionBinder Convert(Type type, ConversionResultKind resultKind)
	{
		if (_conversionBinders == null)
		{
			Interlocked.CompareExchange(ref _conversionBinders, new Dictionary<Type, PythonConversionBinder>[4], null);
		}
		if (_conversionBinders[(int)resultKind] == null)
		{
			Interlocked.CompareExchange(ref _conversionBinders[(int)resultKind], new Dictionary<Type, PythonConversionBinder>(), null);
		}
		Dictionary<Type, PythonConversionBinder> dictionary = _conversionBinders[(int)resultKind];
		lock (dictionary)
		{
			if (!dictionary.TryGetValue(type, out var value))
			{
				value = (dictionary[type] = new PythonConversionBinder(this, type, resultKind));
			}
			return value;
		}
	}

	internal DynamicMetaObjectBinder ConvertRetObject(Type type, ConversionResultKind resultKind)
	{
		if (_convertRetObjectBinders == null)
		{
			Interlocked.CompareExchange(ref _convertRetObjectBinders, new Dictionary<Type, DynamicMetaObjectBinder>[4], null);
		}
		if (_convertRetObjectBinders[(int)resultKind] == null)
		{
			Interlocked.CompareExchange(ref _convertRetObjectBinders[(int)resultKind], new Dictionary<Type, DynamicMetaObjectBinder>(), null);
		}
		Dictionary<Type, DynamicMetaObjectBinder> dictionary = _convertRetObjectBinders[(int)resultKind];
		lock (dictionary)
		{
			if (!dictionary.TryGetValue(type, out var value))
			{
				value = (dictionary[type] = new PythonConversionBinder(this, type, resultKind, retObject: true));
			}
			return value;
		}
	}

	internal CreateFallback Create(CompatibilityInvokeBinder realFallback, CallInfo callInfo)
	{
		if (_createBinders == null)
		{
			Interlocked.CompareExchange(ref _createBinders, new Dictionary<CallSignature, CreateFallback>(), null);
		}
		lock (_createBinders)
		{
			CallSignature key = BindingHelpers.CallInfoToSignature(callInfo);
			if (!_createBinders.TryGetValue(key, out var value))
			{
				value = (_createBinders[key] = new CreateFallback(realFallback, callInfo));
			}
			return value;
		}
	}

	internal PythonGetMemberBinder GetMember(string name)
	{
		return GetMember(name, isNoThrow: false);
	}

	internal PythonGetMemberBinder GetMember(string name, bool isNoThrow)
	{
		Dictionary<string, PythonGetMemberBinder> dictionary;
		if (isNoThrow)
		{
			if (_tryGetMemberBinders == null)
			{
				Interlocked.CompareExchange(ref _tryGetMemberBinders, new Dictionary<string, PythonGetMemberBinder>(), null);
			}
			dictionary = _tryGetMemberBinders;
		}
		else
		{
			if (_getMemberBinders == null)
			{
				Interlocked.CompareExchange(ref _getMemberBinders, new Dictionary<string, PythonGetMemberBinder>(), null);
			}
			dictionary = _getMemberBinders;
		}
		lock (dictionary)
		{
			if (!dictionary.TryGetValue(name, out var value))
			{
				value = (dictionary[name] = new PythonGetMemberBinder(this, name, isNoThrow));
			}
			return value;
		}
	}

	internal CompatibilityGetMember CompatGetMember(string name, bool isNoThrow)
	{
		Dictionary<string, CompatibilityGetMember> dictionary;
		if (isNoThrow)
		{
			if (_compatGetMemberNoThrow == null)
			{
				Interlocked.CompareExchange(ref _compatGetMemberNoThrow, new Dictionary<string, CompatibilityGetMember>(), null);
			}
			dictionary = _compatGetMemberNoThrow;
		}
		else
		{
			if (_compatGetMember == null)
			{
				Interlocked.CompareExchange(ref _compatGetMember, new Dictionary<string, CompatibilityGetMember>(), null);
			}
			dictionary = _compatGetMember;
		}
		lock (dictionary)
		{
			if (!dictionary.TryGetValue(name, out var value))
			{
				value = (dictionary[name] = new CompatibilityGetMember(this, name, isNoThrow));
			}
			return value;
		}
	}

	internal PythonSetMemberBinder SetMember(string name)
	{
		if (_setMemberBinders == null)
		{
			Interlocked.CompareExchange(ref _setMemberBinders, new Dictionary<string, PythonSetMemberBinder>(), null);
		}
		lock (_setMemberBinders)
		{
			if (!_setMemberBinders.TryGetValue(name, out var value))
			{
				value = (_setMemberBinders[name] = new PythonSetMemberBinder(this, name));
			}
			return value;
		}
	}

	internal PythonDeleteMemberBinder DeleteMember(string name)
	{
		if (_deleteMemberBinders == null)
		{
			Interlocked.CompareExchange(ref _deleteMemberBinders, new Dictionary<string, PythonDeleteMemberBinder>(), null);
		}
		lock (_deleteMemberBinders)
		{
			if (!_deleteMemberBinders.TryGetValue(name, out var value))
			{
				value = (_deleteMemberBinders[name] = new PythonDeleteMemberBinder(this, name));
			}
			return value;
		}
	}

	internal PythonInvokeBinder Invoke(CallSignature signature)
	{
		if (_invokeBinders == null)
		{
			Interlocked.CompareExchange(ref _invokeBinders, new Dictionary<CallSignature, PythonInvokeBinder>(), null);
		}
		lock (_invokeBinders)
		{
			if (!_invokeBinders.TryGetValue(signature, out var value))
			{
				value = (_invokeBinders[signature] = new PythonInvokeBinder(this, signature));
			}
			return value;
		}
	}

	internal PythonOperationBinder Operation(PythonOperationKind operation)
	{
		if (_operationBinders == null)
		{
			Interlocked.CompareExchange(ref _operationBinders, new Dictionary<PythonOperationKind, PythonOperationBinder>(), null);
		}
		lock (_operationBinders)
		{
			if (!_operationBinders.TryGetValue(operation, out var value))
			{
				value = (_operationBinders[operation] = new PythonOperationBinder(this, operation));
			}
			return value;
		}
	}

	internal PythonUnaryOperationBinder UnaryOperation(ExpressionType operation)
	{
		if (_unaryBinders == null)
		{
			Interlocked.CompareExchange(ref _unaryBinders, new Dictionary<ExpressionType, PythonUnaryOperationBinder>(), null);
		}
		lock (_unaryBinders)
		{
			if (!_unaryBinders.TryGetValue(operation, out var value))
			{
				value = (_unaryBinders[operation] = new PythonUnaryOperationBinder(this, operation));
			}
			return value;
		}
	}

	internal PythonBinaryOperationBinder BinaryOperation(ExpressionType operation)
	{
		if (_binaryBinders == null)
		{
			Interlocked.CompareExchange(ref _binaryBinders, new PythonBinaryOperationBinder[85], null);
		}
		PythonBinaryOperationBinder pythonBinaryOperationBinder = _binaryBinders[(int)operation];
		if (pythonBinaryOperationBinder != null)
		{
			return pythonBinaryOperationBinder;
		}
		PythonBinaryOperationBinder pythonBinaryOperationBinder2 = Interlocked.CompareExchange(ref _binaryBinders[(int)operation], pythonBinaryOperationBinder = new PythonBinaryOperationBinder(this, operation), null);
		return pythonBinaryOperationBinder2 ?? pythonBinaryOperationBinder;
	}

	internal BinaryRetTypeBinder BinaryOperationRetType(PythonBinaryOperationBinder opBinder, PythonConversionBinder convBinder)
	{
		if (_binaryRetTypeBinders == null)
		{
			Interlocked.CompareExchange(ref _binaryRetTypeBinders, new Dictionary<OperationRetTypeKey<ExpressionType>, BinaryRetTypeBinder>(), null);
		}
		lock (_binaryRetTypeBinders)
		{
			OperationRetTypeKey<ExpressionType> key = new OperationRetTypeKey<ExpressionType>(convBinder.Type, opBinder.Operation);
			if (!_binaryRetTypeBinders.TryGetValue(key, out var value))
			{
				value = (_binaryRetTypeBinders[key] = new BinaryRetTypeBinder(opBinder, convBinder));
			}
			return value;
		}
	}

	internal BinaryRetTypeBinder OperationRetType(PythonOperationBinder opBinder, PythonConversionBinder convBinder)
	{
		if (_operationRetTypeBinders == null)
		{
			Interlocked.CompareExchange(ref _operationRetTypeBinders, new Dictionary<OperationRetTypeKey<PythonOperationKind>, BinaryRetTypeBinder>(), null);
		}
		lock (_operationRetTypeBinders)
		{
			OperationRetTypeKey<PythonOperationKind> key = new OperationRetTypeKey<PythonOperationKind>(convBinder.Type, opBinder.Operation);
			if (!_operationRetTypeBinders.TryGetValue(key, out var value))
			{
				value = (_operationRetTypeBinders[key] = new BinaryRetTypeBinder(opBinder, convBinder));
			}
			return value;
		}
	}

	internal PythonGetIndexBinder GetIndex(int argCount)
	{
		if (_getIndexBinders == null)
		{
			Interlocked.CompareExchange(ref _getIndexBinders, new PythonGetIndexBinder[argCount + 1], null);
		}
		lock (this)
		{
			if (_getIndexBinders.Length <= argCount)
			{
				Array.Resize(ref _getIndexBinders, argCount + 1);
			}
			if (_getIndexBinders[argCount] == null)
			{
				_getIndexBinders[argCount] = new PythonGetIndexBinder(this, argCount);
			}
			return _getIndexBinders[argCount];
		}
	}

	internal PythonSetIndexBinder SetIndex(int argCount)
	{
		if (_setIndexBinders == null)
		{
			Interlocked.CompareExchange(ref _setIndexBinders, new PythonSetIndexBinder[argCount + 1], null);
		}
		lock (this)
		{
			if (_setIndexBinders.Length <= argCount)
			{
				Array.Resize(ref _setIndexBinders, argCount + 1);
			}
			if (_setIndexBinders[argCount] == null)
			{
				_setIndexBinders[argCount] = new PythonSetIndexBinder(this, argCount);
			}
			return _setIndexBinders[argCount];
		}
	}

	internal PythonDeleteIndexBinder DeleteIndex(int argCount)
	{
		if (_deleteIndexBinders == null)
		{
			Interlocked.CompareExchange(ref _deleteIndexBinders, new PythonDeleteIndexBinder[argCount + 1], null);
		}
		lock (this)
		{
			if (_deleteIndexBinders.Length <= argCount)
			{
				Array.Resize(ref _deleteIndexBinders, argCount + 1);
			}
			if (_deleteIndexBinders[argCount] == null)
			{
				_deleteIndexBinders[argCount] = new PythonDeleteIndexBinder(this, argCount);
			}
			return _deleteIndexBinders[argCount];
		}
	}

	public static PythonContext GetPythonContext(DynamicMetaObjectBinder action)
	{
		if (action is IPythonSite pythonSite)
		{
			return pythonSite.Context;
		}
		return DefaultContext.DefaultPythonContext;
	}

	public static System.Linq.Expressions.Expression GetCodeContext(DynamicMetaObjectBinder action)
	{
		return Utils.Constant(GetPythonContext(action)._defaultContext);
	}

	public static DynamicMetaObject GetCodeContextMO(DynamicMetaObjectBinder action)
	{
		return new DynamicMetaObject(Utils.Constant(GetPythonContext(action)._defaultContext), BindingRestrictions.Empty, GetPythonContext(action)._defaultContext);
	}

	public static DynamicMetaObject GetCodeContextMOCls(DynamicMetaObjectBinder action)
	{
		return new DynamicMetaObject(Utils.Constant(GetPythonContext(action).SharedClsContext), BindingRestrictions.Empty, GetPythonContext(action).SharedClsContext);
	}

	public override T ScopeGetVariable<T>(Scope scope, string name)
	{
		if (scope.Storage is ScopeStorage scopeStorage && scopeStorage.TryGetValue(name, ignoreCase: false, out object value))
		{
			return base.Operations.ConvertTo<T>(value);
		}
		if (scope.Storage is StringDictionaryExpando stringDictionaryExpando && stringDictionaryExpando.Dictionary.TryGetValue(name, out value))
		{
			return base.Operations.ConvertTo<T>(value);
		}
		return base.ScopeGetVariable<T>(scope, name);
	}

	public override dynamic ScopeGetVariable(Scope scope, string name)
	{
		if (scope.Storage is ScopeStorage scopeStorage && scopeStorage.TryGetValue(name, ignoreCase: false, out object value))
		{
			return value;
		}
		if (scope.Storage is StringDictionaryExpando stringDictionaryExpando && stringDictionaryExpando.Dictionary.TryGetValue(name, out value))
		{
			return value;
		}
		return base.ScopeGetVariable(scope, name);
	}

	public override void ScopeSetVariable(Scope scope, string name, object value)
	{
		if (scope.Storage is ScopeStorage scopeStorage)
		{
			scopeStorage.SetValue(name, ignoreCase: false, value);
		}
		else if (scope.Storage is StringDictionaryExpando stringDictionaryExpando)
		{
			stringDictionaryExpando.Dictionary[name] = value;
		}
		else
		{
			base.ScopeSetVariable(scope, name, value);
		}
	}

	public override bool ScopeTryGetVariable(Scope scope, string name, out dynamic value)
	{
		if (scope.Storage is ScopeStorage scopeStorage && scopeStorage.TryGetValue(name, ignoreCase: false, out value))
		{
			return true;
		}
		if (scope.Storage is StringDictionaryExpando stringDictionaryExpando && stringDictionaryExpando.Dictionary.TryGetValue(name, out value))
		{
			return true;
		}
		return base.ScopeTryGetVariable(scope, name, out value);
	}

	internal void EnsureDebugContext()
	{
		if (_debugContext == null || _tracePipeline == null)
		{
			lock (this)
			{
				if (_debugContext == null)
				{
					_debugContext = DebugContext.CreateInstance();
					_tracePipeline = Microsoft.Scripting.Debugging.TracePipeline.CreateInstance(_debugContext);
				}
			}
		}
		if (_tracebackListeners == null)
		{
			_tracebackListeners = new Stack<PythonTracebackListener>();
			_tracebackListeners.Push(new PythonTracebackListener(this));
		}
	}

	internal void RegisterTracebackHandler()
	{
		_tracePipeline.TraceCallback = _tracebackListeners.Peek();
		EnableTracing = true;
	}

	internal void UnregisterTracebackHandler()
	{
		EnableTracing = false;
	}

	internal void PushTracebackHandler(PythonTracebackListener listener)
	{
		if (_debugContext != null)
		{
			while (_tracebackListeners.Count > 0 && _tracebackListeners.Peek().ExceptionThrown)
			{
				_tracebackListeners.Pop();
			}
			_tracebackListeners.Push(listener);
		}
	}

	internal void PopTracebackHandler()
	{
		if (_debugContext != null && _tracebackListeners.Count > 1)
		{
			_tracebackListeners.Pop();
		}
	}

	internal ExtensionMethodSet UniqifyExtensions(ExtensionMethodSet newSet)
	{
		int num = -1;
		if (_weakExtensionMethodSets == null)
		{
			Interlocked.CompareExchange(ref _weakExtensionMethodSets, new List<WeakReference>(), null);
		}
		lock (_weakExtensionMethodSets)
		{
			for (int i = 0; i < _weakExtensionMethodSets.Count; i++)
			{
				WeakReference weakReference = _weakExtensionMethodSets[i];
				ExtensionMethodSet extensionMethodSet = (ExtensionMethodSet)weakReference.Target;
				if (extensionMethodSet != null)
				{
					if (extensionMethodSet == newSet)
					{
						return extensionMethodSet;
					}
				}
				else
				{
					num = i;
				}
			}
			if (num == -1)
			{
				_weakExtensionMethodSets.Add(new WeakReference(newSet));
			}
			else
			{
				_weakExtensionMethodSets[num].Target = newSet;
			}
			return newSet;
		}
	}
}
