using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public class SkinStyle
{
	public readonly Type TargetType;

	public readonly List<Setter> Setters = new List<Setter>();

	public readonly List<SkinStyle> Dependents = new List<SkinStyle>();

	public SkinStyle(Type targetType)
	{
		if (targetType == null)
		{
			throw new ArgumentNullException();
		}
		TargetType = targetType;
	}

	public override string ToString()
	{
		return "TargetType = " + TargetType;
	}
}
