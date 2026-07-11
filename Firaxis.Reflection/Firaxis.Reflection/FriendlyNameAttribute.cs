using System;
using System.ComponentModel;

namespace Firaxis.Reflection;

[AttributeUsage(AttributeTargets.All)]
public class FriendlyNameAttribute : DisplayNameAttribute
{
	public FriendlyNameAttribute(string name)
		: base(name)
	{
	}
}
