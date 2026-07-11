using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

internal class MissingPred
{
	private HashSet<string> ick;

	public MissingPred(HashSet<string> ew)
	{
		ick = ew;
		base._002Ector();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public bool ShouldRemove(Tuple<string, IInstanceEntity> yuck)
	{
		return !ick.Contains(yuck.Item1);
	}
}
