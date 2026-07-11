using System;

namespace Sce.Atf.Wpf;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ResourceDictionaryResourceAttribute : Attribute
{
	public string Path { get; private set; }

	public ResourceDictionaryResourceAttribute(string path)
	{
		Path = path;
	}
}
