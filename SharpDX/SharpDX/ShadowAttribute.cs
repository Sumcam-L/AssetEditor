using System;

namespace SharpDX;

[AttributeUsage(AttributeTargets.Interface)]
internal class ShadowAttribute : Attribute
{
	private Type type;

	public Type Type => type;

	public ShadowAttribute(Type typeOfTheAssociatedShadow)
	{
		type = typeOfTheAssociatedShadow;
	}

	public static ShadowAttribute Get(Type type)
	{
		object[] customAttributes = type.GetCustomAttributes(typeof(ShadowAttribute), inherit: false);
		if (customAttributes.Length == 0)
		{
			return null;
		}
		return (ShadowAttribute)customAttributes[0];
	}
}
