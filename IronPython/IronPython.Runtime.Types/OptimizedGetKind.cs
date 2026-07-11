namespace IronPython.Runtime.Types;

internal enum OptimizedGetKind
{
	None,
	SlotDict,
	SlotOnly,
	PropertySlot,
	UserSlotDict,
	UserSlotOnly
}
