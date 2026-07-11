using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public class WpfSkinStyle
{
	public readonly Type TargetType;

	public readonly ICollection<IWpfSkinStyleSetter> Setters = new List<IWpfSkinStyleSetter>();

	public WpfSkinStyle(Type targetType)
	{
		if (targetType == null)
		{
			throw new ArgumentNullException();
		}
		TargetType = targetType;
	}
}
