using System;

namespace Firaxis.Asset;

public class AssetEditStyleAttribute : Attribute
{
	public AssetEditStyle EditStyle { get; private set; }

	public AssetEditStyleAttribute(AssetEditStyle editStyle)
	{
		EditStyle = editStyle;
	}
}
