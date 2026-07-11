using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.Packages;

public static class XLPExtensions
{
	public static IXLPClass GetClassByName(this IXLPClassSet xlpClasses, string xlpClassName)
	{
		return xlpClasses.Items.FirstOrDefault((IXLPClass cls) => cls.Name == xlpClassName);
	}

	public static IEnumerable<EntityID> GetReferencedEntities(this IXLP xlp, IXLPClassSet xlpClasses)
	{
		IXLPClass classByName = xlpClasses.GetClassByName(xlp.ClassName);
		if (classByName == null)
		{
			return Enumerable.Empty<EntityID>();
		}
		ICollection<EntityID> collection = new HashSet<EntityID>();
		foreach (IXLPEntry xLPEntry in xlp.XLPEntries)
		{
			EntityID item = new EntityID(xLPEntry.ObjectName, classByName.InstanceType);
			if (!collection.Contains(item))
			{
				collection.Add(item);
			}
		}
		return collection;
	}

	public static IEnumerable<IXLPClass> GetValidXLPClasses(this IXLPClassSet xlpClasses, IInstanceEntity entity)
	{
		if (xlpClasses == null || entity == null)
		{
			yield break;
		}
		foreach (IXLPClass xlpClass in xlpClasses.Items)
		{
			if (xlpClass.InstanceType == entity.Type && xlpClass.AllowedEntityClasses.Contains(entity.ClassName))
			{
				yield return xlpClass;
			}
		}
	}

	public static void DisposeXLPs(this IEnumerable<IXLP> xlpsToDispose)
	{
		foreach (IXLP item in xlpsToDispose)
		{
			item.Dispose();
		}
	}
}
