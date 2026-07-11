using System;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels;

public class SlotChangedEventArgs : EventArgs
{
	public IInstanceEntity Entity { get; private set; }

	public int SlotID { get; private set; }

	public SlotChangedEventArgs(IInstanceEntity entity, int slotID)
	{
		Entity = entity;
		SlotID = slotID;
	}
}
