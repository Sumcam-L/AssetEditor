using System;

namespace Sce.Atf.Rendering.Dom;

public class PickEventArgs : EventArgs
{
	public readonly HitRecord[] HitArray;

	public readonly bool MultiSelect;

	public PickEventArgs(HitRecord[] hits, bool multiSelect)
	{
		HitArray = hits;
		MultiSelect = multiSelect;
	}
}
