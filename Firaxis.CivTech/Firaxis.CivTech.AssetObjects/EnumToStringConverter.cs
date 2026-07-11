using System;
using System.Collections.Generic;
using Firaxis.Reflection;

namespace Firaxis.CivTech.AssetObjects;

public static class EnumToStringConverter
{
	private static IDictionary<string, InstanceType> NameTypeLookupMap;

	private static IDictionary<InstanceType, string> TypeNameLookupMap;

	private static IDictionary<string, InstanceType> s_extTypes;

	private static IDictionary<InstanceType, string> s_typeExts;

	static EnumToStringConverter()
	{
		NameTypeLookupMap = new Dictionary<string, InstanceType>();
		TypeNameLookupMap = new Dictionary<InstanceType, string>();
		s_extTypes = new Dictionary<string, InstanceType>
		{
			{
				".ast",
				InstanceType.IT_ASSET
			},
			{
				".mtl",
				InstanceType.IT_MATERIAL
			},
			{
				".geo",
				InstanceType.IT_GEOMETRY
			},
			{
				".tex",
				InstanceType.IT_TEXTURE
			},
			{
				".anm",
				InstanceType.IT_ANIMATION
			},
			{
				".env",
				InstanceType.IT_ENVIRONMENT_LIGHT
			},
			{
				".lrg",
				InstanceType.IT_LIGHT_RIG
			},
			{
				".dsg",
				InstanceType.IT_DSG
			},
			{
				".ptl",
				InstanceType.IT_PARTICLE_EFFECT
			},
			{
				".lit",
				InstanceType.IT_ANALYTIC_LIGHT
			},
			{
				".bhv",
				InstanceType.IT_BEHAVIOR
			},
			{
				".ffx",
				InstanceType.IT_FIREFX
			}
		};
		s_typeExts = new Dictionary<InstanceType, string>
		{
			{
				InstanceType.IT_ASSET,
				".ast"
			},
			{
				InstanceType.IT_MATERIAL,
				".mtl"
			},
			{
				InstanceType.IT_GEOMETRY,
				".geo"
			},
			{
				InstanceType.IT_TEXTURE,
				".tex"
			},
			{
				InstanceType.IT_ANIMATION,
				".anm"
			},
			{
				InstanceType.IT_DSG,
				".dsg"
			},
			{
				InstanceType.IT_ENVIRONMENT_LIGHT,
				".env"
			},
			{
				InstanceType.IT_LIGHT_RIG,
				".lrg"
			},
			{
				InstanceType.IT_ANALYTIC_LIGHT,
				".lit"
			},
			{
				InstanceType.IT_PARTICLE_EFFECT,
				".ptl"
			},
			{
				InstanceType.IT_BEHAVIOR,
				".bhv"
			},
			{
				InstanceType.IT_FIREFX,
				".ffx"
			}
		};
		foreach (InstanceType value in Enum.GetValues(typeof(InstanceType)))
		{
			string displayName = ReflectionHelper.GetDisplayName(value);
			NameTypeLookupMap[displayName] = value;
			TypeNameLookupMap[value] = displayName;
		}
	}

	public static List<string> GetInstanceTypeName()
	{
		List<string> list = new List<string>();
		foreach (object value in Enum.GetValues(typeof(InstanceType)))
		{
			list.Add(ReflectionHelper.GetDisplayName((InstanceType)value));
		}
		return list;
	}

	public static string GetNameFromType(InstanceType type)
	{
		return TypeNameLookupMap[type];
	}

	public static InstanceType GetTypeFromName(string name)
	{
		InstanceType value = InstanceType.IT_COUNT;
		NameTypeLookupMap.TryGetValue(name, out value);
		return value;
	}

	public static InstanceType GetTypeFromExtension(string ext)
	{
		InstanceType result = InstanceType.IT_INVALID;
		if (s_extTypes.ContainsKey(ext))
		{
			result = s_extTypes[ext];
		}
		return result;
	}

	public static bool IsEntityExtension(string ext)
	{
		return s_extTypes.ContainsKey(ext);
	}

	public static string GetExtensionFromType(InstanceType extType)
	{
		string result = "";
		if (s_typeExts.ContainsKey(extType))
		{
			result = s_typeExts[extType];
		}
		return result;
	}
}
