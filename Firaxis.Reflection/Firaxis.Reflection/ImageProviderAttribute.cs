using System;

namespace Firaxis.Reflection;

public class ImageProviderAttribute : Attribute
{
	public Type Location { get; private set; }

	public string Name { get; private set; }

	public ImageProviderAttribute(Type location, string name)
	{
		Location = location;
		Name = name;
	}
}
