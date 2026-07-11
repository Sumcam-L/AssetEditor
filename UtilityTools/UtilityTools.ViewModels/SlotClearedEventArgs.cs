using System;

namespace UtilityTools.ViewModels;

public class SlotClearedEventArgs : EventArgs
{
	public int SlotID { get; private set; }

	public SlotClearedEventArgs(int slotID)
	{
		SlotID = slotID;
	}
}
