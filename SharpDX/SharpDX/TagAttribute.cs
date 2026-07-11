using System;

namespace SharpDX;

[AttributeUsage(AttributeTargets.All)]
public class TagAttribute : Attribute
{
	public string Value { get; private set; }

	public TagAttribute(string value)
	{
		Value = value;
	}
}
