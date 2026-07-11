using System.Dynamic;
using Microsoft.Scripting.ComInterop;

namespace IronPython.Runtime.Binding;

internal class CompatibilityGetMember : GetMemberBinder, IPythonSite, IInvokeOnGetBinder
{
	private readonly PythonContext _context;

	private readonly bool _isNoThrow;

	public PythonContext Context => _context;

	public bool InvokeOnGet => false;

	public CompatibilityGetMember(PythonContext context, string name)
		: base(name, ignoreCase: false)
	{
		_context = context;
	}

	public CompatibilityGetMember(PythonContext context, string name, bool isNoThrow)
		: base(name, ignoreCase: false)
	{
		_context = context;
		_isNoThrow = isNoThrow;
	}

	public override DynamicMetaObject FallbackGetMember(DynamicMetaObject self, DynamicMetaObject errorSuggestion)
	{
		if (ComBinder.TryBindGetMember(this, self, out var result, delayInvocation: true))
		{
			return result;
		}
		return PythonGetMemberBinder.FallbackWorker(_context, self, PythonContext.GetCodeContextMOCls(this), base.Name, _isNoThrow ? GetMemberOptions.IsNoThrow : GetMemberOptions.None, this, errorSuggestion);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CompatibilityGetMember compatibilityGetMember))
		{
			return false;
		}
		if (compatibilityGetMember._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}
}
