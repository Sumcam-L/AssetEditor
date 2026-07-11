using System;

namespace Firaxis.Reflection;

[AttributeUsage(AttributeTargets.Class)]
public class SyntaxAttribute : Attribute
{
	public string Syntax { get; private set; }

	public SyntaxAttribute(string syntax)
	{
		Syntax = syntax;
	}
}
