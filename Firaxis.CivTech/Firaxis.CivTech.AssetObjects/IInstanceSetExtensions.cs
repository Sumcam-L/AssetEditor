using System;
using System.Collections.Generic;
using System.Linq;

namespace Firaxis.CivTech.AssetObjects;

public static class IInstanceSetExtensions
{
	private static int behaviorCount = 0;

	public static T LoadEntityIfUnique<T>(this IInstanceSet instanceSet, string entityName) where T : class, IInstanceEntity
	{
		BugSubmitter.SilentAssert(!string.IsNullOrEmpty(entityName), "Invalid value \"{0}\" for Name parameter passed to IInstanceSetExtensions.LoadEntityIfUnique<{1}> @summary Invalid Name parameter passed to IInstanceSetExtensions.LoadEntityIfUnique<T> @assign bwhitman", entityName, typeof(T));
		return instanceSet.LoadEntityIfUnique(entityName, StaticMethods.InstanceTypeFromEntityType<T>()) as T;
	}

	public static IInstanceEntity LoadEntityIfUnique(this IInstanceSet instanceSet, string entityName, InstanceType entityType)
	{
		BugSubmitter.SilentAssert(!string.IsNullOrEmpty(entityName), "Invalid value \"{0}\" for Name parameter passed to IInstanceSetExtensions.LoadEntityIfUnique @summary Invalid Name parameter passed to IInstanceSetExtensions.LoadEntityIfUnique @assign bwhitman", entityName);
		return instanceSet.LoadEntityByName(entityName, entityType);
	}

	public static T LoadEntity<T>(this IInstanceSet instanceSet, IVirtualPantry virtPan, string entityName) where T : class, IInstanceEntity
	{
		return instanceSet.LoadEntity(virtPan, entityName, StaticMethods.InstanceTypeFromEntityType<T>()) as T;
	}

	public static IInstanceEntity LoadEntity(this IInstanceSet instanceSet, IVirtualPantry virtPan, string entityName, InstanceType entityType)
	{
		if (string.IsNullOrWhiteSpace(entityName))
		{
			return null;
		}
		string pantryPath = virtPan.GetPantryPath(entityName, entityType);
		if (string.IsNullOrEmpty(pantryPath))
		{
			return null;
		}
		return instanceSet.LoadEntityByPath(pantryPath, entityType);
	}

	public static IEnumerable<IInstanceEntity> LoadEntities(this IInstanceSet instances, IVirtualPantry virtPan, IEnumerable<string> entityNames, InstanceType entityType)
	{
		ICollection<IInstanceEntity> collection = new List<IInstanceEntity>();
		foreach (string entityName in entityNames)
		{
			AddEntityToCollectionIfNew(instances, entityName, entityType, collection);
		}
		return collection;
	}

	public static IEnumerable<T> LoadEntities<T>(this IInstanceSet instances, IVirtualPantry virtPan, IEnumerable<string> entityNames) where T : class, IInstanceEntity
	{
		InstanceType entityType = StaticMethods.InstanceTypeFromEntityType<T>();
		return instances.LoadEntities(virtPan, entityNames, entityType).OfType<T>();
	}

	public static IEnumerable<IInstanceEntity> LoadEntities(this IInstanceSet instances, IVirtualPantry virtPan, IEnumerable<EntityID> entities)
	{
		ICollection<IInstanceEntity> collection = new List<IInstanceEntity>();
		foreach (EntityID entity in entities)
		{
			AddEntityToCollectionIfNew(instances, entity.Name, entity.Type, collection);
		}
		return collection;
	}

	public static IEnumerable<IInstanceEntity> LoadEntities(this IInstanceSet instances, IVirtualPantry virtPan, IEnumerable<Tuple<string, InstanceType>> entities)
	{
		ICollection<IInstanceEntity> collection = new List<IInstanceEntity>();
		foreach (Tuple<string, InstanceType> entity in entities)
		{
			AddEntityToCollectionIfNew(instances, entity.Item1, entity.Item2, collection);
		}
		return collection;
	}

	public static IEnumerable<IInstanceEntity> LoadEntities(this IInstanceSet instances, IEnumerable<KeyValuePair<string, InstanceType>> entities)
	{
		ICollection<IInstanceEntity> collection = new List<IInstanceEntity>();
		foreach (KeyValuePair<string, InstanceType> entity in entities)
		{
			AddEntityToCollectionIfNew(instances, entity.Key, entity.Value, collection);
		}
		return collection;
	}

	private static void AddEntityToCollectionIfNew(IInstanceSet instances, string entityName, InstanceType entityType, ICollection<IInstanceEntity> entitiesLoaded)
	{
		IInstanceEntity instanceEntity = instances.FindByNameAndType(entityName, entityType);
		if (instanceEntity == null)
		{
			instanceEntity = instances.LoadEntityByName(entityName, entityType);
			if (instanceEntity != null)
			{
				entitiesLoaded.Add(instanceEntity);
			}
		}
	}

	public static IInstanceEntity CreateEntityByName(this IInstanceSet instanceSet, string name, InstanceType entityType)
	{
		return entityType switch
		{
			InstanceType.IT_TEXTURE => instanceSet.Push<ITextureInstance>(name), 
			InstanceType.IT_GEOMETRY => instanceSet.Push<IGeometryInstanceBuildable>(name), 
			InstanceType.IT_ANIMATION => instanceSet.Push<IAnimationInstance>(name), 
			InstanceType.IT_MATERIAL => instanceSet.Push<IMaterialInstance>(name), 
			InstanceType.IT_ASSET => instanceSet.Push<IAssetInstance>(name), 
			InstanceType.IT_DSG => instanceSet.Push<IDSGInstance>(name), 
			InstanceType.IT_ENVIRONMENT_LIGHT => instanceSet.Push<IEnvironmentLightInstance>(name), 
			InstanceType.IT_ANALYTIC_LIGHT => instanceSet.Push<IAnalyticLightInstance>(name), 
			InstanceType.IT_LIGHT_RIG => instanceSet.Push<ILightRigInstance>(name), 
			InstanceType.IT_PARTICLE_EFFECT => instanceSet.Push<IParticleEffectInstance>(name), 
			InstanceType.IT_BEHAVIOR => instanceSet.Push<IBehaviorInstance>(name), 
			InstanceType.IT_FIREFX => instanceSet.Push<IFireFXInstance>(name), 
			_ => null, 
		};
	}

	public static IInstanceEntity LoadEntityByName(this IInstanceSet instanceSet, string name, InstanceType entityType)
	{
		return entityType switch
		{
			InstanceType.IT_TEXTURE => instanceSet.LoadByName<ITextureInstance>(name), 
			InstanceType.IT_GEOMETRY => instanceSet.LoadByName<IGeometryInstanceBuildable>(name), 
			InstanceType.IT_ANIMATION => instanceSet.LoadByName<IAnimationInstance>(name), 
			InstanceType.IT_MATERIAL => instanceSet.LoadByName<IMaterialInstance>(name), 
			InstanceType.IT_ASSET => instanceSet.LoadByName<IAssetInstance>(name), 
			InstanceType.IT_DSG => instanceSet.LoadByName<IDSGInstance>(name), 
			InstanceType.IT_ENVIRONMENT_LIGHT => instanceSet.LoadByName<IEnvironmentLightInstance>(name), 
			InstanceType.IT_ANALYTIC_LIGHT => instanceSet.LoadByName<IAnalyticLightInstance>(name), 
			InstanceType.IT_LIGHT_RIG => instanceSet.LoadByName<ILightRigInstance>(name), 
			InstanceType.IT_PARTICLE_EFFECT => instanceSet.LoadByName<IParticleEffectInstance>(name), 
			InstanceType.IT_BEHAVIOR => instanceSet.LoadByName<IBehaviorInstance>(name), 
			InstanceType.IT_FIREFX => instanceSet.LoadByName<IFireFXInstance>(name), 
			_ => null, 
		};
	}

	public static IInstanceEntity LoadEntityByPath(this IInstanceSet instanceSet, string entityPath, InstanceType entityType)
	{
		IInstanceEntity result = null;
		switch (entityType)
		{
		case InstanceType.IT_TEXTURE:
			result = instanceSet.LoadByPath<ITextureInstance>(entityPath);
			break;
		case InstanceType.IT_GEOMETRY:
			result = instanceSet.LoadByPath<IGeometryInstanceBuildable>(entityPath);
			break;
		case InstanceType.IT_ANIMATION:
			result = instanceSet.LoadByPath<IAnimationInstance>(entityPath);
			break;
		case InstanceType.IT_MATERIAL:
			result = instanceSet.LoadByPath<IMaterialInstance>(entityPath);
			break;
		case InstanceType.IT_ASSET:
			result = instanceSet.LoadByPath<IAssetInstance>(entityPath);
			break;
		case InstanceType.IT_DSG:
			result = instanceSet.LoadByPath<IDSGInstance>(entityPath);
			break;
		case InstanceType.IT_ENVIRONMENT_LIGHT:
			result = instanceSet.LoadByPath<IEnvironmentLightInstance>(entityPath);
			break;
		case InstanceType.IT_ANALYTIC_LIGHT:
			result = instanceSet.LoadByPath<IAnalyticLightInstance>(entityPath);
			break;
		case InstanceType.IT_LIGHT_RIG:
			result = instanceSet.LoadByPath<ILightRigInstance>(entityPath);
			break;
		case InstanceType.IT_PARTICLE_EFFECT:
			result = instanceSet.LoadByPath<IParticleEffectInstance>(entityPath);
			break;
		case InstanceType.IT_BEHAVIOR:
			result = instanceSet.LoadByPath<IBehaviorInstance>(entityPath);
			break;
		case InstanceType.IT_FIREFX:
			result = instanceSet.LoadByPath<IFireFXInstance>(entityPath);
			break;
		}
		return result;
	}

	public static void LoadDependentBehaviors(this IInstanceSet instanceSet, IInstanceEntity entity)
	{
		Action<InstanceType, string> crawlAction = null;
		crawlAction = delegate(InstanceType type, string name)
		{
			if (type == InstanceType.IT_BEHAVIOR)
			{
				instanceSet.LoadEntityByName(name, type)?.CrawlCooktimeDependencies(crawlAction);
			}
		};
		entity.CrawlCooktimeDependencies(crawlAction);
	}

	public static void LoadDependentEntities(this IInstanceEntity entity, IInstanceSet instanceSet)
	{
		Action<InstanceType, string> crawlAction = null;
		crawlAction = delegate(InstanceType type, string name)
		{
			instanceSet.LoadEntityByName(name, type)?.CrawlCooktimeDependencies(crawlAction);
		};
		entity.CrawlCooktimeDependencies(crawlAction);
	}

	public static IEnumerable<IImportedEntity> GetImportableEntities(this IInstanceEntity entity, IInstanceSet instanceSet, bool recursive)
	{
		ICollection<IImportedEntity> importedEntities = new HashSet<IImportedEntity>();
		AddEntityToCollectionIfImportable(entity, importedEntities);
		Action<InstanceType, string> crawlAction = null;
		crawlAction = delegate(InstanceType type, string name)
		{
			IInstanceEntity instanceEntity = instanceSet.LoadEntityByName(name, type);
			AddEntityToCollectionIfImportable(instanceEntity, importedEntities);
			if (instanceEntity != null && recursive)
			{
				instanceEntity.CrawlCooktimeDependencies(crawlAction);
			}
		};
		entity.CrawlCooktimeDependencies(crawlAction);
		return importedEntities;
	}

	public static IEnumerable<IAssetInstance> GetReferencedAssets(this IAssetInstance asset, IClassSet classSet, IInstanceSet instanceSet, bool recursive)
	{
		instanceSet.LoadDependentBehaviors(asset);
		IBehaviorInstance behaviorInstance = instanceSet.Push<IBehaviorInstance>($"temp_behavior_{behaviorCount++}");
		asset.Flatten(instanceSet, classSet, behaviorInstance);
		List<IAssetInstance> list = new List<IAssetInstance>();
		GetReferencedAssets(behaviorInstance, classSet, instanceSet, recursive, list);
		return list;
	}

	public static IEnumerable<IAssetInstance> GetReferencedAssets(this IBehaviorInstance behavior, IClassSet classSet, IInstanceSet instanceSet, bool recursive)
	{
		instanceSet.LoadDependentBehaviors(behavior);
		IBehaviorInstance behaviorInstance = instanceSet.Push<IBehaviorInstance>($"temp_behavior_{behaviorCount++}");
		behavior.Flatten(instanceSet, classSet, behaviorInstance);
		List<IAssetInstance> list = new List<IAssetInstance>();
		GetReferencedAssets(behaviorInstance, classSet, instanceSet, recursive, list);
		return list;
	}

	private static void GetReferencedAssets(IBehaviorDataProvider provider, IClassSet classSet, IInstanceSet instanceSet, bool recursive, List<IAssetInstance> referencedAssets)
	{
		ITimelineSet timelines = provider.Timelines;
		foreach (ITimeline timeline in timelines.Timelines)
		{
			foreach (ITrigger trigger in timeline.Triggers)
			{
				if (!trigger.IsAssetTrigger())
				{
					continue;
				}
				string fXName = trigger.FXName;
				IAssetInstance assetInstance = instanceSet.LoadByName<IAssetInstance>(fXName);
				if (assetInstance != null)
				{
					AddAssetToCollectionIfUnique(assetInstance, referencedAssets);
					if (recursive)
					{
						GetReferencedAssets(assetInstance, classSet, instanceSet, recursive, referencedAssets);
					}
				}
			}
		}
	}

	private static void AddAssetToCollectionIfUnique(IAssetInstance asset, ICollection<IAssetInstance> collection)
	{
		if (asset != null && !collection.Any((IAssetInstance ast) => ast.Name == asset.Name))
		{
			collection.Add(asset);
		}
	}

	private static void AddEntityToCollectionIfImportable(IInstanceEntity entity, ICollection<IImportedEntity> entityCollection)
	{
		if (entity != null && entity is IImportedEntity item)
		{
			entityCollection.Add(item);
		}
	}
}
