using System;

namespace Firaxis.Asset;

public class AssetTypeAttribute : Attribute
{
	public string Extension { get; private set; }

	public AssetTypeAttribute(string extension)
	{
		Extension = extension;
	}
}
