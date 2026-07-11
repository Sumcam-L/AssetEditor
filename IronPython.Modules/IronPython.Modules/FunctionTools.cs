using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class FunctionTools
{
	[PythonType]
	public class partial : IWeakReferenceable
	{
		private object _function;

		private object[] _args;

		private IDictionary<object, object> _keywordArgs;

		private CodeContext _context;

		private CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>> _dictSite;

		private CallSite<Func<CallSite, CodeContext, object, object[], object>> _splatSite;

		private PythonDictionary _dict;

		private WeakRefTracker _tracker;

		public object func => _function;

		public object args => PythonTuple.MakeTuple(_args);

		public object keywords => _keywordArgs;

		public PythonDictionary __dict__
		{
			get
			{
				return EnsureDict();
			}
			set
			{
				_dict = value;
			}
		}

		public partial(CodeContext context, object func, [NotNull] params object[] args)
			: this(context, func, null, args)
		{
		}

		public partial(CodeContext context, object func, [ParamDictionary] IDictionary<object, object> keywords, [NotNull] params object[] args)
		{
			if (!PythonOps.IsCallable(context, func))
			{
				throw PythonOps.TypeError("the first argument must be callable");
			}
			_function = func;
			_keywordArgs = keywords;
			_args = args;
			_context = context;
		}

		[SpecialName]
		[PropertyMethod]
		public void Delete__dict__()
		{
			throw PythonOps.TypeError("partial's dictionary may not be deleted");
		}

		public void __delattr__(string name)
		{
			if (name == "__dict__")
			{
				Delete__dict__();
			}
			if (_dict != null)
			{
				_dict.Remove(name);
			}
		}

		[SpecialName]
		public object Call(CodeContext context, params object[] args)
		{
			if (_keywordArgs == null)
			{
				EnsureSplatSite();
				return _splatSite.Target(_splatSite, context, _function, ArrayUtils.AppendRange(_args, args));
			}
			EnsureDictSplatSite();
			return _dictSite.Target(_dictSite, context, _function, ArrayUtils.AppendRange(_args, args), _keywordArgs);
		}

		[SpecialName]
		public object Call(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
		{
			IDictionary<object, object> arg;
			if (_keywordArgs != null)
			{
				PythonDictionary pythonDictionary = new PythonDictionary();
				pythonDictionary.update(context, _keywordArgs);
				pythonDictionary.update(context, dict);
				arg = pythonDictionary;
			}
			else
			{
				arg = dict;
			}
			EnsureDictSplatSite();
			return _dictSite.Target(_dictSite, context, _function, ArrayUtils.AppendRange(_args, args), arg);
		}

		[SpecialName]
		public void SetMemberAfter(CodeContext context, string name, object value)
		{
			EnsureDict();
			_dict[name] = value;
		}

		[SpecialName]
		public object GetBoundMember(CodeContext context, string name)
		{
			if (_dict != null && _dict.TryGetValue(name, out var value))
			{
				return value;
			}
			return OperationFailed.Value;
		}

		[SpecialName]
		public bool DeleteMember(CodeContext context, string name)
		{
			string text;
			if ((text = name) != null && text == "__dict__")
			{
				Delete__dict__();
			}
			if (_dict == null)
			{
				return false;
			}
			return _dict.Remove(name);
		}

		private void EnsureSplatSite()
		{
			if (_splatSite == null)
			{
				Interlocked.CompareExchange(ref _splatSite, CallSite<Func<CallSite, CodeContext, object, object[], object>>.Create(Binders.InvokeSplat(PythonContext.GetContext(_context))), null);
			}
		}

		private void EnsureDictSplatSite()
		{
			if (_dictSite == null)
			{
				Interlocked.CompareExchange(ref _dictSite, CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>>.Create(Binders.InvokeKeywords(PythonContext.GetContext(_context))), null);
			}
		}

		private PythonDictionary EnsureDict()
		{
			if (_dict == null)
			{
				_dict = PythonDictionary.MakeSymbolDictionary();
			}
			return _dict;
		}

		WeakRefTracker IWeakReferenceable.GetWeakRef()
		{
			return _tracker;
		}

		bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
		{
			return Interlocked.CompareExchange(ref _tracker, value, null) == null;
		}

		void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
		{
			_tracker = value;
		}
	}

	public const string __doc__ = "provides functionality for manipulating callable objects";

	public static object reduce(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object, object, object>>> siteData, object func, object seq)
	{
		return Builtin.reduce(context, siteData, func, seq);
	}

	public static object reduce(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object, object, object>>> siteData, object func, object seq, object initializer)
	{
		return Builtin.reduce(context, siteData, func, seq, initializer);
	}
}
