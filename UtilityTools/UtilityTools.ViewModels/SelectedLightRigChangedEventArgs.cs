using System;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels;

public class SelectedLightRigChangedEventArgs : EventArgs
{
	public ILightRigInstance NewLightRig { get; private set; }

	public SelectedLightRigChangedEventArgs(ILightRigInstance lightRig)
	{
		NewLightRig = lightRig;
	}
}
