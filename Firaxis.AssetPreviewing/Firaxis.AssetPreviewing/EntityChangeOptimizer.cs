using System;
using System.Collections.Generic;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetPreviewing;

internal static class EntityChangeOptimizer
{
	private class ModelID : IEquatable<ModelID>
	{
		public EntityID Entity { get; set; }

		public string ModelName { get; set; }

		public bool Equals(ModelID other)
		{
			return Entity.Equals(other.Entity) && ModelName.Equals(other.ModelName);
		}

		public bool RefersToEvent(IEntityChangedEvent changeEvent)
		{
			if (changeEvent == null)
			{
				return false;
			}
			if (changeEvent.InstanceType != Entity.Type)
			{
				return false;
			}
			EntityChangeType type = changeEvent.Type;
			if (type == EntityChangeType.ECT_MODEL_INSTANCE_REMOVED || type == EntityChangeType.ECT_MODEL_INSTANCE_CHANGED || type == EntityChangeType.ECT_TRIGROUP_PARAMETER_CHANGED)
			{
				if (!changeEvent.EntityName.Equals(Entity.Name, StringComparison.CurrentCultureIgnoreCase))
				{
					return false;
				}
				switch (type)
				{
				case EntityChangeType.ECT_MODEL_INSTANCE_CHANGED:
					if (!(changeEvent is IModelInstanceChanged modelInstanceChanged))
					{
						return false;
					}
					return modelInstanceChanged.ModelName.Equals(ModelName);
				case EntityChangeType.ECT_MODEL_INSTANCE_REMOVED:
					if (!(changeEvent is IModelInstanceRemoved modelInstanceRemoved))
					{
						return false;
					}
					return modelInstanceRemoved.ModelName.Equals(ModelName);
				case EntityChangeType.ECT_TRIGROUP_PARAMETER_CHANGED:
					if (!(changeEvent is ITriGroupParameterChanged triGroupParameterChanged))
					{
						return false;
					}
					return triGroupParameterChanged.ModelName.Equals(ModelName);
				default:
					return false;
				}
			}
			return false;
		}
	}

	public static IEnumerable<IEntityChangedEvent> OptimizeChanges(IEnumerable<IEntityChangedEvent> changes)
	{
		List<ModelID> list = new List<ModelID>();
		HashSet<EntityID> hashSet = new HashSet<EntityID>();
		List<IEntityChangedEvent> list2 = new List<IEntityChangedEvent>();
		foreach (IEntityChangedEvent change in changes)
		{
			EntityID targetEntity = new EntityID(change.EntityName, change.InstanceType);
			if (hashSet.Contains(targetEntity))
			{
				continue;
			}
			if (change.Type == EntityChangeType.ECT_GENERIC)
			{
				list2.RemoveAll((IEntityChangedEvent change) => IsChangedEntity(change, targetEntity));
				list2.Add(change);
			}
			else if (change.Type == EntityChangeType.ECT_MODEL_INSTANCE_REMOVED)
			{
				IModelInstanceRemoved modelInstanceRemoved = change as IModelInstanceRemoved;
				if (modelInstanceRemoved == null)
				{
					BugSubmitter.SilentAssert(modelInstanceRemoved != null, "Type mismatch.  ChangeEvent with change type '{0}' has type '{1}' but should have type '{2}'.", change.Type.ToString(), change.GetType().ToString(), typeof(IModelInstanceRemoved).ToString());
					continue;
				}
				ModelID modelID = new ModelID();
				modelID.Entity = targetEntity;
				modelID.ModelName = modelInstanceRemoved.ModelName;
				list2.RemoveAll((IEntityChangedEvent change) => modelID.RefersToEvent(change));
				list2.Add(change);
				if (!list.Contains(modelID))
				{
					list.Add(modelID);
				}
			}
			else if (change.Type == EntityChangeType.ECT_MODEL_INSTANCE_CHANGED)
			{
				IModelInstanceChanged modelInstanceChanged = change as IModelInstanceChanged;
				if (modelInstanceChanged == null)
				{
					BugSubmitter.SilentAssert(modelInstanceChanged != null, "Type mismatch.  ChangeEvent with change type '{0}' has type '{1}' but should have type '{2}'.", change.Type.ToString(), change.GetType().ToString(), typeof(IModelInstanceChanged).ToString());
					continue;
				}
				ModelID modelID2 = new ModelID();
				modelID2.Entity = targetEntity;
				modelID2.ModelName = modelInstanceChanged.ModelName;
				list2.RemoveAll((IEntityChangedEvent change) => modelID2.RefersToEvent(change));
				list2.Add(change);
				if (list.Contains(modelID2))
				{
					list.Remove(modelID2);
				}
			}
			else if (change.Type == EntityChangeType.ECT_TRIGROUP_PARAMETER_CHANGED)
			{
				ITriGroupParameterChanged triGroupParameterChanged = change as ITriGroupParameterChanged;
				if (triGroupParameterChanged == null)
				{
					BugSubmitter.SilentAssert(triGroupParameterChanged != null, "Type mismatch.  ChangeEvent with change type '{0}' has type '{1}' but should have type '{2}'.", change.Type.ToString(), change.GetType().ToString(), typeof(ITriGroupParameterChanged).ToString());
					continue;
				}
				ModelID modelID3 = new ModelID();
				modelID3.Entity = targetEntity;
				modelID3.ModelName = triGroupParameterChanged.ModelName;
				if (!list.Contains(modelID3))
				{
					list2.Add(change);
				}
			}
			else
			{
				list2.Add(change);
			}
		}
		return list2;
	}

	private static bool IsChangedEntity(IEntityChangedEvent changeEvent, EntityID entityID)
	{
		return entityID.Type == changeEvent.InstanceType && entityID.Name.Equals(changeEvent.EntityName, StringComparison.CurrentCultureIgnoreCase);
	}
}
