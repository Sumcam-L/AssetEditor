namespace IronPython.Runtime.Types;

internal enum OptimizedSetKind
{
	None,
	SetAttr,
	UserSlot,
	SetDict,
	Error
}
