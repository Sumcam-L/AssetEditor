using System;

namespace Firaxis.Reflection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public class StrideAttribute : Attribute
{
	private int size;

	public int Size => size;

	public StrideAttribute(int size)
	{
		this.size = size;
	}
}
