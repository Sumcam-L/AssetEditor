using System;
using System.Collections.Generic;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.AssetEditing;

public struct ObjectValueInfo
{
	public string name;

	public string type;

	public readonly string displayName;

	public readonly string className;

	public readonly IEnumerable<InstanceType> validTypes;

	public readonly IEnumerable<string> validClassNames;

	public ObjectValueInfo(ICivTechService civTechSvc, string name, string type, string className, IEnumerable<InstanceType> types, IEnumerable<string> classes)
	{
		this.name = name;
		this.type = type;
		this.className = className;
		validTypes = types;
		validClassNames = classes;
		displayName = name;
		if (string.IsNullOrEmpty(name) || !Enum.TryParse<InstanceType>(type, ignoreCase: true, out var result) || result != InstanceType.IT_TEXTURE)
		{
			return;
		}
		using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { civTechSvc.GetActivePantryPaths() });
		ITextureInstance textureInstance = instanceSet.LoadByName<ITextureInstance>(name);
		if (textureInstance != null)
		{
			displayName = $"({textureInstance.Width}x{textureInstance.Height})   {name}";
		}
	}

	public override string ToString()
	{
		return displayName;
	}
}
