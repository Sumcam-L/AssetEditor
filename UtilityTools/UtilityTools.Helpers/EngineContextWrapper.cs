using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.Helpers;

public static class EngineContextWrapper
{
	public static IInstanceEntity GetInstanceByNameAndType(InstanceType eType, string name, IInstanceSet instanceSet)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		return instanceSet.LoadEntityByName(name, eType);
	}
}
