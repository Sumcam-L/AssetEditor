using System;

namespace Firaxis.Reflection;

public class FileExtensionAttribute : Attribute
{
	public string Extension { get; private set; }

	public bool Primary { get; private set; }

	public FileExtensionAttribute(string ext)
		: this(ext, primary: true)
	{
	}

	public FileExtensionAttribute(string ext, bool primary)
	{
		Extension = ext;
		Primary = primary;
	}
}
