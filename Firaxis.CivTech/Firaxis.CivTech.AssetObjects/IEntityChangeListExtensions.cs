using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public static class IEntityChangeListExtensions
{
	public static void AddGenericEntityChangedEvents(this IEntityChangeList changeList, IEnumerable<EntityID> entities)
	{
		foreach (EntityID entity in entities)
		{
			changeList.Push<IEntityChangedEvent>(entity);
		}
	}

	public static IEntityChangedEvent CreateEntityChangedEvent(this IEntityChangeList changeList, InstanceType entityType, string entityName)
	{
		return changeList.Push<IEntityChangedEvent>(new EntityID(entityName, entityType));
	}

	public static IEntityCookParameterChanged CreateEntityCookParameterChangedEvent(this IEntityChangeList changeList, InstanceType entityType, string entityName, string parameterName, IValue newValue)
	{
		IEntityCookParameterChanged entityCookParameterChanged = changeList.Push<IEntityCookParameterChanged>(new EntityID(entityName, entityType));
		entityCookParameterChanged.ParameterName = parameterName;
		entityCookParameterChanged.ChangedValue = newValue;
		return entityCookParameterChanged;
	}

	public static IAssetTimelineChanged CreateAssetTimelineChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, ITimeline timeline)
	{
		IAssetTimelineChanged assetTimelineChanged = changeList.Push<IAssetTimelineChanged>(new EntityID(parentEntity));
		assetTimelineChanged.ChangedTimeline = timeline;
		return assetTimelineChanged;
	}

	public static IAssetTimelineRemoved CreateAssetTimelineRemovedChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string timelineName)
	{
		IAssetTimelineRemoved assetTimelineRemoved = changeList.Push<IAssetTimelineRemoved>(new EntityID(parentEntity));
		assetTimelineRemoved.TimelineName = timelineName;
		return assetTimelineRemoved;
	}

	public static IAssetTimelineSetChanged CreateAssetTimelineSetChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, IEnumerable<ITimelineBinding> changedTimelineBindings, IEnumerable<string> removedTimelines)
	{
		IAssetTimelineSetChanged assetTimelineSetChanged = changeList.Push<IAssetTimelineSetChanged>(new EntityID(parentEntity));
		if (changedTimelineBindings != null)
		{
			foreach (ITimelineBinding changedTimelineBinding in changedTimelineBindings)
			{
				assetTimelineSetChanged.SlotChanged(changedTimelineBinding.SlotName, changedTimelineBinding.TimelineName);
			}
		}
		if (removedTimelines != null)
		{
			foreach (string removedTimeline in removedTimelines)
			{
				assetTimelineSetChanged.SlotRemoved(removedTimeline);
			}
		}
		return assetTimelineSetChanged;
	}

	public static IAssetTimelineSetChanged CreateAssetTimelineBindEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, ITimelineBinding changedTimeline)
	{
		IAssetTimelineSetChanged assetTimelineSetChanged = changeList.Push<IAssetTimelineSetChanged>(new EntityID(parentEntity));
		assetTimelineSetChanged.SlotChanged(changedTimeline.SlotName, changedTimeline.TimelineName);
		return assetTimelineSetChanged;
	}

	public static IAssetTimelineSetChanged CreateAssetTimelineUnbindEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string slotName)
	{
		IAssetTimelineSetChanged assetTimelineSetChanged = changeList.Push<IAssetTimelineSetChanged>(new EntityID(parentEntity));
		assetTimelineSetChanged.SlotRemoved(slotName);
		return assetTimelineSetChanged;
	}

	public static void FillWithTimelineChangedEvents(this IEntityChangeList changeList, IInstanceEntity parentEntity, IEnumerable<ITimeline> timelines, IEnumerable<ITimelineBinding> changedTimelineBindings, IEnumerable<string> removedTimelines)
	{
		changeList.CreateAssetTimelineSetChangedEvent(parentEntity, changedTimelineBindings, removedTimelines);
		if (timelines != null)
		{
			foreach (ITimeline timeline in timelines)
			{
				changeList.CreateAssetTimelineChangedEvent(parentEntity, timeline);
			}
		}
		if (removedTimelines == null)
		{
			return;
		}
		foreach (string removedTimeline in removedTimelines)
		{
			changeList.CreateAssetTimelineRemovedChangedEvent(parentEntity, removedTimeline);
		}
	}

	public static IAssetDSGChanged CreateAssetDSGChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string dsgName)
	{
		IAssetDSGChanged assetDSGChanged = changeList.Push<IAssetDSGChanged>(new EntityID(parentEntity));
		assetDSGChanged.DSGName = dsgName;
		return assetDSGChanged;
	}

	public static IModelInstanceRemoved CreateModelInstanceRemovedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string modelName)
	{
		IModelInstanceRemoved modelInstanceRemoved = changeList.Push<IModelInstanceRemoved>(new EntityID(parentEntity));
		modelInstanceRemoved.ModelName = modelName;
		return modelInstanceRemoved;
	}

	public static IModelInstanceChanged CreateModelInstanceChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string modelName, string geoName, IModelInstance newModel)
	{
		IModelInstanceChanged modelInstanceChanged = changeList.Push<IModelInstanceChanged>(new EntityID(parentEntity));
		modelInstanceChanged.ModelName = modelName;
		modelInstanceChanged.GeoName = geoName;
		modelInstanceChanged.Model = newModel;
		return modelInstanceChanged;
	}

	public static IAttachmentRemoved CreateAttachmentRemovedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string attachmentName)
	{
		IAttachmentRemoved attachmentRemoved = changeList.Push<IAttachmentRemoved>(new EntityID(parentEntity));
		attachmentRemoved.AttachmentName = attachmentName;
		return attachmentRemoved;
	}

	public static IAttachmentCookParameterChanged CreateAttachmentCookParameterChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string attachmentName, string parameterName, IValue changedValue)
	{
		IAttachmentCookParameterChanged attachmentCookParameterChanged = changeList.Push<IAttachmentCookParameterChanged>(new EntityID(parentEntity));
		attachmentCookParameterChanged.AttachmentName = attachmentName;
		attachmentCookParameterChanged.ParameterName = parameterName;
		attachmentCookParameterChanged.ChangedValue = changedValue;
		return attachmentCookParameterChanged;
	}

	public static IAttachmentChanged CreateAttachmentChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string oldAttachmentName, string newAttachmentName, string modelInstanceName, string boneName, float[] pos, float[] rot, float scale)
	{
		IAttachmentChanged attachmentChanged = changeList.Push<IAttachmentChanged>(new EntityID(parentEntity));
		attachmentChanged.OldAttachmentName = oldAttachmentName;
		attachmentChanged.NewAttachmentName = newAttachmentName;
		attachmentChanged.ModelInstanceName = modelInstanceName;
		attachmentChanged.BoneName = boneName;
		attachmentChanged.Position = pos;
		attachmentChanged.Orientation = rot;
		attachmentChanged.Scale = scale;
		return attachmentChanged;
	}

	public static ISplineVertexChanged CreateSplineVertexChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string splineName, int vertexIndex, float[] pos)
	{
		ISplineVertexChanged splineVertexChanged = changeList.Push<ISplineVertexChanged>(new EntityID(parentEntity));
		splineVertexChanged.SplineName = splineName;
		splineVertexChanged.VertexIndex = vertexIndex;
		splineVertexChanged.Position = pos;
		return splineVertexChanged;
	}

	public static ITriGroupParameterChanged CreateTriGroupParameterChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string modelName, string meshName, string groupName, string stateName, string parameterName, IValue newValue)
	{
		ITriGroupParameterChanged triGroupParameterChanged = changeList.Push<ITriGroupParameterChanged>(new EntityID(parentEntity));
		triGroupParameterChanged.ModelName = modelName;
		triGroupParameterChanged.MeshName = meshName;
		triGroupParameterChanged.GroupName = groupName;
		triGroupParameterChanged.StateName = stateName;
		triGroupParameterChanged.ParameterName = parameterName;
		triGroupParameterChanged.ChangedValue = newValue;
		return triGroupParameterChanged;
	}

	public static IAssetAnimationSetChanged CreateAnimationSetChangedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string slotName, string animationName)
	{
		IAssetAnimationSetChanged assetAnimationSetChanged = changeList.Push<IAssetAnimationSetChanged>(new EntityID(parentEntity));
		assetAnimationSetChanged.SlotChanged(slotName, animationName);
		return assetAnimationSetChanged;
	}

	public static IParticleEffectAdded CreateParticleEffectAddedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string particleEffectName)
	{
		IParticleEffectAdded particleEffectAdded = changeList.Push<IParticleEffectAdded>(new EntityID(parentEntity));
		particleEffectAdded.ParticleEffectName = particleEffectName;
		return particleEffectAdded;
	}

	public static IParticleEffectRemoved CreateParticleEffectRemovedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string particleEffectName)
	{
		IParticleEffectRemoved particleEffectRemoved = changeList.Push<IParticleEffectRemoved>(new EntityID(parentEntity));
		particleEffectRemoved.ParticleEffectName = particleEffectName;
		return particleEffectRemoved;
	}

	public static ILightTagDirectionChanged CreateLightDirectionTagChangedEvent(this IEntityChangeList changeList, IEnvironmentLightInstance environmentLight, IEnvironmentLightDirectionTag lightTag)
	{
		ILightTagDirectionChanged lightTagDirectionChanged = changeList.Push<ILightTagDirectionChanged>(new EntityID(environmentLight));
		lightTagDirectionChanged.LightTag = lightTag;
		return lightTagDirectionChanged;
	}

	public static ILightTagDirectionRemoved CreateLightTagDirectionRemovedEvent(this IEntityChangeList changeList, IEnvironmentLightInstance environmentLight, string lightTagName)
	{
		ILightTagDirectionRemoved lightTagDirectionRemoved = changeList.Push<ILightTagDirectionRemoved>(new EntityID(environmentLight));
		lightTagDirectionRemoved.LightTagDirectionName = lightTagName;
		return lightTagDirectionRemoved;
	}

	public static IBehaviorAdded CreateBehaviorAddedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string behaviorName)
	{
		IBehaviorAdded behaviorAdded = changeList.Push<IBehaviorAdded>(new EntityID(parentEntity));
		behaviorAdded.BehaviorName = behaviorName;
		return behaviorAdded;
	}

	public static IBehaviorRemoved CreateBehaviorRemovedEvent(this IEntityChangeList changeList, IInstanceEntity parentEntity, string behaviorName)
	{
		IBehaviorRemoved behaviorRemoved = changeList.Push<IBehaviorRemoved>(new EntityID(parentEntity));
		behaviorRemoved.BehaviorName = behaviorName;
		return behaviorRemoved;
	}

	public static void PopulatePrimGroupStatesChangedEvents(this IEntityChangeList changeList, IModelInstance model, IInstanceEntity parentEntity)
	{
		foreach (IPrimGroupState primGroup in model.PrimGroups)
		{
			foreach (IValue item in primGroup.Values.Items)
			{
				changeList.CreateTriGroupParameterChangedEvent(parentEntity, model.Name, primGroup.MeshName, primGroup.GroupName, primGroup.StateName, item.ParameterName, item);
			}
		}
	}
}
