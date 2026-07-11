using System;

namespace Firaxis.Reflection;

public class UniqueAttribute : Attribute
{
	public bool Unique { get; private set; }

	public UniqueAttribute()
		: this(unique: true)
	{
	}

	public UniqueAttribute(bool unique)
	{
		Unique = unique;
	}
}
