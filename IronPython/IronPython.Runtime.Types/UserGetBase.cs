using IronPython.Runtime.Binding;

namespace IronPython.Runtime.Types;

internal class UserGetBase : FastGetBase
{
	internal readonly int _version;

	public UserGetBase(PythonGetMemberBinder binder, int version)
	{
		_version = version;
	}

	public override bool IsValid(PythonType type)
	{
		return _version == type.Version;
	}
}
