using System;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

internal class GetAttributeDelegates : UserGetBase
{
	private readonly string _name;

	private readonly PythonTypeSlot _getAttributeSlot;

	private readonly PythonTypeSlot _getAttrSlot;

	private readonly SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>> _storage;

	private readonly bool _isNoThrow;

	public GetAttributeDelegates(PythonGetMemberBinder binder, string name, int version, PythonTypeSlot getAttributeSlot, PythonTypeSlot getAttrSlot)
		: base(binder, version)
	{
		_storage = new SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>>();
		_getAttributeSlot = getAttributeSlot;
		_getAttrSlot = getAttrSlot;
		_name = name;
		_func = GetAttribute;
		_isNoThrow = binder.IsNoThrow;
	}

	public object GetAttribute(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version)
		{
			if (_isNoThrow)
			{
				return UserTypeOps.GetAttributeNoThrow(context, self, _name, _getAttributeSlot, _getAttrSlot, _storage);
			}
			return UserTypeOps.GetAttribute(context, self, _name, _getAttributeSlot, _getAttrSlot, _storage);
		}
		return FastGetBase.Update(site, self, context);
	}
}
